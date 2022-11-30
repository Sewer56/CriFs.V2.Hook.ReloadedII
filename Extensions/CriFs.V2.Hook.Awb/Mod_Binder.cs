using System.Diagnostics;
using System.Runtime.InteropServices;
using CriFs.V2.Hook.Interfaces;
using CriFs.V2.Hook.Interfaces.Structs;
using FileEmulationFramework.Lib;

namespace CriFs.V2.Hook.Awb;

/// <summary>
/// Contains the binding logic.
/// Not separated from mod class as this mod's small.
/// </summary>
public partial class Mod
{
    // Mod implementation.
    private const string R2 = "R2"; // DO NOT CHANGE
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
        // Note: After profiling, no caching needed here.
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
                _logger.Error("[CriFsV2.Awb] AWB file for {0} not found in any CPK!!", route.FullPath);
                continue;
            }
            
            // Get matched file.
            var awbFile = cachedFile.Files[awbFileIndex];
            _logger.Info("[CriFsV2.Awb] Found AWB file {0} in CPK {1}", route.FullPath, cpkPath);

            var relativeAcbPath = Path.ChangeExtension(awbFile.FullPath, AcbExtension);
            if (!cachedFile.FilesByPath.TryGetValue(relativeAcbPath, out var acbFileIndex))
            {
                _logger.Warning("[CriFsV2.Awb] We didn't find an ACB for {0}!!", route.FullPath);
                continue;
            }
            
            // Register AWB
            var awbBindPath = Path.Combine(R2, awbFile.FullPath);
            if (bind.RelativePathToFileMap.ContainsKey(awbBindPath))
            {
                _logger.Info("[CriFsV2.Awb] Binder input already contains AWB {0}, we'll use existing one.", awbFile.FullPath);
            }
            else
            {
                var emulatedFilePath = Path.Combine(bind.BindDirectory, awbBindPath);
                Directory.CreateDirectory(Path.GetDirectoryName(emulatedFilePath)!);
                _logger.Info("[CriFsV2.Awb] Creating Emulated File {0}", emulatedFilePath);
                _awbEmulator.TryCreateFromFileSlice(cpkPath, awbFile.File.FileOffset, route.FullPath, emulatedFilePath);
                _boundFiles.Add(emulatedFilePath);
            }
            
            // Extract ACB.
            var acbBindPath = Path.Combine(R2, relativeAcbPath);
            if (bind.RelativePathToFileMap.ContainsKey(acbBindPath))
            {
                _logger.Info("[CriFsV2.Awb] Binder input already contains ACB {0}, we'll use existing one.", relativeAcbPath);
            }
            else
            {
                using var cpkStream = new FileStream(cpkPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                using var reader = criFsLib.CreateCpkReader(cpkStream, false);
                var extractedAcb = reader.ExtractFile(cachedFile.Files[acbFileIndex].File);
            
                // Write ACB
                var acbPath = Path.Combine(_bindingDirectory, relativeAcbPath);
                _logger.Info("[CriFsV2.Awb] Writing {0}", acbPath);
                Directory.CreateDirectory(Path.GetDirectoryName(acbPath)!);            
                var outputFileStream = new FileStream(acbPath, new FileStreamOptions()
                {
                    Access = FileAccess.ReadWrite,
                    Mode = FileMode.Create,
                    BufferSize = 0,
                    Share = FileShare.ReadWrite,
                    PreallocationSize = extractedAcb.Span.Length
                });
            
                tasks.Add(outputFileStream.WriteAsync(extractedAcb.RawArray, 0, extractedAcb.Count).ContinueWith(_ =>
                {
                    extractedAcb.Dispose();
                    outputFileStream.Dispose();
                }));
                
                bind.RelativePathToFileMap[acbBindPath] = new List<ICriFsRedirectorApi.BindFileInfo>()
                {
                    new()
                    {
                       FullPath = acbPath,
                       LastWriteTime = DateTime.UtcNow,
                       ModId = "CriFs.V2.Hook.Awb"
                    }
                };
            }
        }

        Task.WhenAll(tasks).Wait();
        _logger.Debug($"[CriFsV2.Awb] Setup AWB Redirector Support for CRIFsHook in {watch.ElapsedMilliseconds}ms");
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