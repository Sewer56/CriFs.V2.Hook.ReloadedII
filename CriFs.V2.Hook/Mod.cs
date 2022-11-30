using System.Diagnostics;
using System.Runtime.CompilerServices;
using CriFs.V2.Hook.Bind;
using CriFs.V2.Hook.Bind.Utilities;
using CriFs.V2.Hook.CRI;
using CriFs.V2.Hook.Hooks;
using CriFs.V2.Hook.Interfaces;
using CriFs.V2.Hook.Template;
using CriFs.V2.Hook.Utilities;
using CriFsV2Lib.Definitions;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Native = CriFs.V2.Hook.Utilities.Native;

[module: SkipLocalsInit]
namespace CriFs.V2.Hook;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public class Mod : ModBase, IExports // <= Do not Remove.
{
    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    /// Provides access to the Reloaded.Hooks API.
    /// </summary>
    /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
    private readonly IReloadedHooks? _hooks;

    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly Logger _logger;

    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;
    
    private ReloadedBindBuilderCreator? _cpkBuilder;
    private BindDirectoryAcquirer _directoryAcquirer;

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        var owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;
        _logger = new Logger(context.Logger, _configuration.LogLevel);

        // For more information about this template, please see
        // https://reloaded-project.github.io/Reloaded-II/ModTemplate/

        // If you want to implement e.g. unload support in your mod,
        // and some other neat features, override the methods in ModBase.

        _modLoader.GetController<IStartupScanner>().TryGetTarget(out var startupScanner);
        var scanHelper = new SigScanHelper(_logger, startupScanner);
        var currentProcess = Process.GetCurrentProcess();
        var mainModule = currentProcess.MainModule;
        var baseAddr = mainModule!.BaseAddress;

        var hookContext = new HookContext()
        {
            BaseAddress = baseAddr,
            Config = _configuration,
            Logger = _logger,
            Hooks = _hooks!,
            ScanHelper = scanHelper
        };
        
        // Patches
        CpkBinderPointers.Init(scanHelper, baseAddr);
        DontLogCriDirectoryBinds.Activate(hookContext);
        
        // CPK Builder & Redirector
        var modConfigDirectory = _modLoader.GetDirectoryForModId(_modConfig.ModId);
        var currentProcessProvider = new CurrentProcessProvider(currentProcess.Id);
        var processListProvider = new ProcessListProvider();
        
        var cpkContentCache = new CpkContentCache();
        _directoryAcquirer = new BindDirectoryAcquirer(modConfigDirectory, currentProcessProvider, processListProvider);
        _cpkBuilder = new ReloadedBindBuilderCreator(_modLoader, _logger, _directoryAcquirer, cpkContentCache);
        _cpkBuilder.SetHotReload(_configuration.HotReload);
        _modLoader.OnModLoaderInitialized += OnLoaderInitialized;
        _modLoader.ModLoaded += OnModLoaded;
        _modLoader.ModUnloading += OnModUnloaded;
        
        // Add API
        _modLoader.AddOrReplaceController<ICriFsRedirectorApi>(owner, 
            new Api(_cpkBuilder, cpkContentCache, mainModule.FileName, currentProcessProvider, processListProvider));
    }

    // In case user loads mod in real time.
    private void OnModUnloaded(IModV1 arg1, IModConfigV1 arg2)
    {
        if (_cpkBuilder!.TryRemoveMod((IModConfig)arg2))
            _cpkBuilder.Rebuild();
    }

    private void OnModLoaded(IModV1 arg1, IModConfigV1 arg2)
    {
        if (_cpkBuilder!.TryAddMod((IModConfig)arg2))
            _cpkBuilder.Rebuild();
    }

    private void OnLoaderInitialized()
    {
        _modLoader.OnModLoaderInitialized -= OnLoaderInitialized;
        AssertAwbIncompatibility();
        CpkBinder.Init(_directoryAcquirer.BindDirectory, _logger, _hooks!);
        CpkBinder.SetPrintFileAccess(_configuration.PrintFileAccess);
        _cpkBuilder?.Build(); 
    }

    private void AssertAwbIncompatibility()
    {
        // We messed up and didn't properly set up updates in extension mod,
        // so we assert its sufficiently recent.
        var mods = _modLoader.GetActiveMods();
        var minVersion = new Version(1, 0, 5);
        foreach (var mod in mods)
        {
            var modConfig = mod.Generic;
            if (modConfig.ModId != "CriFs.V2.Hook.Awb")
                continue;

            if (new Version(modConfig.ModVersion) < minVersion)
                Native.MessageBox(0, "Version of AWB Emulator Extension is out of Date.\n" +
                                     "If you're seeing this message you most likely have an older version that has misconfigured update support and thus cannot receive updates.\n\n" +
                                     "Select 'AWB Emulator Support for CRI FileSystem V2 Hook' in Launcher, click Open Folder and delete all files.\n" +
                                     "Newer version will redownload on next game launch, thanks!", "We did an oopsie!", 0);
                
            return;
        }
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;        
        _logger.LogLevel = _configuration.LogLevel;
        _logger.Info($"[{_modConfig.ModId}] Config Updated: Applying");
        CpkBinder.SetPrintFileAccess(_configuration.PrintFileAccess);
        _cpkBuilder?.SetHotReload(_configuration.HotReload);
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion

    public Type[] GetTypes() => new[] { typeof(ICriFsRedirectorApi), typeof(ICriFsLib) };
}