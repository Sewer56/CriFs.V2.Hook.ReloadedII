﻿using System.Diagnostics;
using System.Runtime.CompilerServices;
using CriFs.V2.Hook.Bind;
using CriFs.V2.Hook.Bind.Utilities;
using CriFs.V2.Hook.Configuration;
using CriFs.V2.Hook.CRI;
using CriFs.V2.Hook.Hooks;
using CriFs.V2.Hook.Interfaces;
using CriFs.V2.Hook.Template;
using CriFs.V2.Hook.Utilities;
using FileEmulationFramework.Lib.Utilities;
using p5rpc.modloader;
using p5rpc.modloader.Patches.Common;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

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
    
    private ReloadedBindBuilderCreator? _cpkBuilder;
    private BindDirectoryAcquirer _directoryAcquirer;
    private SigScanHelper _scanHelper;
    
    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;
        _logger = new Logger(context.Logger, _configuration.LogLevel);

        // For more information about this template, please see
        // https://reloaded-project.github.io/Reloaded-II/ModTemplate/

        // If you want to implement e.g. unload support in your mod,
        // and some other neat features, override the methods in ModBase.

        _modLoader.GetController<IStartupScanner>().TryGetTarget(out var startupScanner);
        _scanHelper = new SigScanHelper(_logger, startupScanner);
        var currentProcess = Process.GetCurrentProcess();
        var mainModule = currentProcess.MainModule;
        var baseAddr = mainModule!.BaseAddress;

        var hookContext = new HookContext()
        {
            BaseAddress = baseAddr,
            Config = _configuration,
            Logger = _logger,
            Hooks = _hooks!,
            ScanHelper = _scanHelper
        };
        
        // Patches
        CpkBinderPointers.Init(_scanHelper, baseAddr);
        DontLogCriDirectoryBinds.Activate(hookContext);
        
        // CPK Builder & Redirector
        var modConfigDirectory = _modLoader.GetModConfigDirectory(_modConfig.ModId);
        _directoryAcquirer = new BindDirectoryAcquirer(modConfigDirectory, new CurrentProcessProvider(currentProcess.Id), new ProcessListProvider());
        _cpkBuilder = new ReloadedBindBuilderCreator(_modLoader, _logger, _directoryAcquirer);
        _modLoader.OnModLoaderInitialized += OnLoaderInitialized;
        _modLoader.ModLoaded += OnModLoaded;
        _modLoader.ModUnloading += OnModUnloaded;
        
        // Add API
        _modLoader.AddOrReplaceController<ICriFsRedirectorApi>(_owner, new Api(_cpkBuilder));
    }

    // In case user loads mod in real time.
    private void OnModUnloaded(IModV1 arg1, IModConfigV1 arg2) => _cpkBuilder!.RebuildIfNeeded((IModConfig)arg2);
    private void OnModLoaded(IModV1 arg1, IModConfigV1 arg2) => _cpkBuilder!.RebuildIfNeeded((IModConfig)arg2);

    private void OnLoaderInitialized()
    {
        _modLoader.OnModLoaderInitialized -= OnLoaderInitialized;
        CpkBinder.Init(_directoryAcquirer.BindDirectory, _logger, _hooks!);
        CpkBinder.SetPrintFileAccess(_configuration.PrintFileAccess);
        _cpkBuilder?.Build(); 
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
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion

    public Type[] GetTypes() => new[] { typeof(ICriFsRedirectorApi) };
}