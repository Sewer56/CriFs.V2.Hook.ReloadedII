using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AWB.Stream.Emulator.Interfaces;
using CriFs.V2.Hook.Awb.Template;
using CriFs.V2.Hook.Interfaces;
using CriFs.V2.Hook.Interfaces.Structs;
using FileEmulationFramework.Lib;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Mod.Interfaces;

[module: SkipLocalsInit]
namespace CriFs.V2.Hook.Awb;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public class Mod : ModBase // <= Do not Remove.
{
    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly Logger _logger;

    /// <summary>
    /// Entry point into the mod, instance that created this class.
    /// </summary>
    private readonly IMod _owner;

    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;
    
    private readonly IAwbEmulator _awbEmulator = null!;
    private ICriFsRedirectorApi _criFsApi = null!;
    private string _bindingDirectory = null!;

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;
        _logger = new Logger(context.Logger, _configuration.LogLevel);

        // For more information about this template, please see
        // https://reloaded-project.github.io/Reloaded-II/ModTemplate/

        // If you want to implement e.g. unload support in your mod,
        // and some other neat features, override the methods in ModBase.
        _modLoader.GetController<IAwbEmulator>().TryGetTarget(out _awbEmulator!);
        _modLoader.GetController<ICriFsRedirectorApi>().TryGetTarget(out _criFsApi!);
        _criFsApi.AddUnbindCallback(OnCpkUnbind);
        _criFsApi.AddBindCallback(OnCpkBind);
        
        var modConfigDirectory = _modLoader.GetModConfigDirectory(_modConfig.ModId);
        _bindingDirectory = _criFsApi.GenerateBindingDirectory(modConfigDirectory);
    }
    
    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;
        _logger.LogLevel = _configuration.LogLevel;
        _logger.Info($"[{_modConfig.ModId}] Config Updated: Applying");
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
    
    // Mod implementation.
    private const string R2 = "R2";
    private const string AcbExtension = ".acb";
    private readonly List<string> _boundFiles = new();

    private void OnCpkUnbind(ICriFsRedirectorApi.UnbindContext bind)
    {
        foreach (var boundFile in CollectionsMarshal.AsSpan(_boundFiles))
            _awbEmulator.InvalidateFile(boundFile);

        _boundFiles.Clear();
    }

    private void OnCpkBind(ICriFsRedirectorApi.BindContext bind)
    {
        var input    = _awbEmulator.GetEmulatorInput();
        var cpks     = _criFsApi.GetCpkFilesInGameDir();
        var criFsLib = _criFsApi.GetCriFsLib();
        var tasks = new List<Task>();
        var watch = Stopwatch.StartNew();
        
        foreach (var inputItem in input)
        {
            var route = new Route(inputItem.Route);

            if (!TryFindAwbInAnyCpk(route, cpks, out var cpkPath, out var cachedFile, out int awbFileIndex))
            {
                _logger.Error("AWB file for {0} not found in any CPK!!", route.FullPath);
                continue;
            }
            
            // Get matched file.
            var awbFile = cachedFile.Files[awbFileIndex];
            _logger.Info("Found AWB file {0} in CPK {1}", route.FullPath, cpkPath);

            var relativeAcbPath = Path.ChangeExtension(awbFile.FullPath, AcbExtension);
            if (!cachedFile.FilesByPath.TryGetValue(relativeAcbPath, out var acbFileIndex))
            {
                _logger.Warning("We didn't find an ACB for {0}!!", route.FullPath);
                continue;
            }
            
            // Register AWB
            var emulatedFilePath = Path.Combine(bind.BindDirectory, R2, awbFile.FullPath);
            Directory.CreateDirectory(Path.GetDirectoryName(emulatedFilePath)!);
            _logger.Info("Creating Emulated File {0}", emulatedFilePath);
            _awbEmulator.TryCreateFromFileSlice(cpkPath, awbFile.File.FileOffset, route.FullPath, emulatedFilePath);
            _boundFiles.Add(emulatedFilePath);
            
            // Extract ACB.
            using var cpkStream = new FileStream(cpkPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            using var reader = criFsLib.CreateCpkReader(cpkStream, false);
            using var extractedAcb = reader.ExtractFile(cachedFile.Files[acbFileIndex].File);
            
            // Write ACB
            var acbPath = Path.Combine(_bindingDirectory, relativeAcbPath);
            _logger.Info("Writing {0}", acbPath);
            Directory.CreateDirectory(Path.GetDirectoryName(acbPath)!);            
            using var outputFileStream = new FileStream(acbPath, new FileStreamOptions()
            {
                Access = FileAccess.ReadWrite,
                Mode = FileMode.Create,
                BufferSize = 0,
                Share = FileShare.ReadWrite,
                PreallocationSize = extractedAcb.Span.Length
            });
            
            tasks.Add(outputFileStream.WriteAsync(extractedAcb.RawArray, 0, extractedAcb.Count));
            bind.RelativePathToFileMap[Path.Combine(R2, relativeAcbPath)] = new List<string>() { acbPath };
        }

        Task.WhenAll(tasks).Wait();
        _logger.Info($"Setup AWB Redirector Support for CRIFsHook in {watch.ElapsedMilliseconds}ms");
    }

    private bool TryFindAwbInAnyCpk(Route route, string[] cpkFiles, out string cpkPath, out CpkCacheEntry cachedFile, out int fileIndex)
    {
        foreach (var cpk in cpkFiles)
        {
            cpkPath = cpk;
            cachedFile = _criFsApi.GetCpkFilesCached(cpk);
            var fileNameSpan = Path.GetFileName(route.FullPath.AsSpan());
                
            // If we find, check for ACB.
            if (cachedFile.FilesByPath.TryGetValue(route.FullPath, out fileIndex))
                return true;

            if (!cachedFile.FilesByFileName.TryGetValue(fileNameSpan.ToString(), out fileIndex)) 
                continue;
            
            // If route only has file name, we can take this as answer.
            if (Path.GetDirectoryName(route.FullPath) == null)
                return true;
            
            // If matches by file name we have to search all routes because it's possible duplicate
            // file names can exist under different subfolders
            for (var x = 0; x < cachedFile.Files.Length; x++)
            {
                var file = cachedFile.Files[x];
                if (!new Route(file.FullPath).Matches(route.FullPath)) 
                    continue;
                
                fileIndex = x;
                return true;
            }
        }

        cpkPath = string.Empty;
        fileIndex = -1;
        cachedFile = default;
        return false;
    }
}