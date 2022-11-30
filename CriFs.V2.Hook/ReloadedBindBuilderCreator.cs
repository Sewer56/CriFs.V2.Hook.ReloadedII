using CriFs.V2.Hook.Bind;
using CriFs.V2.Hook.Bind.Interfaces;
using CriFs.V2.Hook.Hooks;
using CriFs.V2.Hook.Interfaces;
using CriFs.V2.Hook.Utilities;
using CriFsV2Lib.Definitions.Utilities;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Mod.Interfaces;

namespace CriFs.V2.Hook;

/// <summary>
/// Class that bridges the code between Reloaded mods and the CPK bind builder & hooks.
/// </summary>
public class ReloadedBindBuilderCreator
{
    private readonly IModLoader _loader;
    private readonly Logger _logger;
    private readonly IBindDirectoryAcquirer _bindDirAcquirer;
    private readonly CpkContentCache _cpkContentCache;
    private bool _canRebuild;

    private readonly List<string> _probingPaths = new(2) { Routes.DefaultProbingPath, "P5REssentials/CPK" }; // <= Legacy Support
    private readonly List<FileSystemWatcher> _watchers = new();
    private readonly List<Action<ICriFsRedirectorApi.UnbindContext>> _unbindCallbacks = new();
    private readonly List<Action<ICriFsRedirectorApi.BindContext>> _bindCallbacks = new();
    private readonly List<string> _modIdsToBuild = new();
    private bool _hotReload;

    public ReloadedBindBuilderCreator(IModLoader loader, Logger logger, IBindDirectoryAcquirer bindDirAcquirer,
        CpkContentCache cpkContentCache)
    {
        _loader = loader;
        _logger = logger;
        _bindDirAcquirer = bindDirAcquirer;
        _cpkContentCache = cpkContentCache;
    }

    /// <summary>
    /// Sets whether Hot Reload functionality should be used.
    /// </summary>
    /// <param name="enable">True to enable hot reload.</param>
    public void SetHotReload(bool enable)
    {
        bool triggerReload = !_hotReload && enable;
        _hotReload = enable;
        if (triggerReload)   
            TriggerHotReload(null!, null!);

        if (!_hotReload)
            ClearHotReload();
    }

    /// <summary>
    /// Adds a path that is searched for mod content.
    /// </summary>
    /// <param name="relativePath">A path relative to folder of a mod, like 'P5REssentials/CPK'</param>
    public void AddProbingPath(string relativePath) => _probingPaths.Add(relativePath);

    /// <summary>
    /// Tries to remove a mod from the internal list of mods to build.
    /// </summary>
    /// <param name="modConfig">The mod config to remove.</param>
    /// <returns>True if a mod has been successfully removed and a rebuild needs to run.</returns>
    public bool TryRemoveMod(IModConfig modConfig) => _modIdsToBuild.Remove(modConfig.ModId);

    /// <summary>
    /// Adds a mod to the internal list of mods to build.
    /// </summary>
    /// <param name="modConfig">The mod config to add.</param>
    /// <returns>True if a mod has been successfully added and a rebuild needs to run.</returns>
    public bool TryAddMod(IModConfig modConfig)
    {
        var path = _loader.GetDirectoryForModId(modConfig.ModId);
        foreach (var probingPath in _probingPaths)
        {
            if (TryGetCpkFolder(path, probingPath, out _)) 
                continue;
            
            _modIdsToBuild.Add(modConfig.ModId);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Triggers a rebuild. Call this if <see cref="TryAddMod"/> or <see cref="TryRemoveMod"/> return true.
    /// </summary>
    public void Rebuild()
    {
        if (!_canRebuild)
            return;
        
        Rebuild_Internal();
    }

    /// <summary>
    /// Builds the bind folders :P.
    /// </summary>
    public void Build()
    {
        _cpkContentCache.Clear();
        
        // Get binding directory & cleanup.
        var builder    = new BindBuilder(_bindDirAcquirer.BindDirectory, "R2");
        
        // Get list of input mods.
        foreach (var modId in _modIdsToBuild)
        foreach (var probingPath in _probingPaths)
            Add(builder, modId, probingPath);

        builder.Build(_bindCallbacks);
        ArrayRental.Reset(); // cleanup mem
        _canRebuild = true;
    }

    /// <summary>
    /// (Conditionally) adds CPK folders for binding to the builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="modId">The id of the individual mod.</param>
    /// <param name="probingPath">The probing path to use.</param>
    private void Add(BindBuilder builder, string modId, string probingPath)
    {
        var path = _loader.GetDirectoryForModId(modId);
        if (!TryGetCpkFolder(path, probingPath, out var cpkFolder)) 
            return;
        
        _logger.Info("[BindBuilderCreator] Adding CPK Folder: {0}", cpkFolder);
        if (_hotReload)
            _watchers.Add(FileSystemWatcherFactory.Create(cpkFolder, TriggerHotReload));
        
        // Get all CPK folders
        WindowsDirectorySearcher.TryGetDirectoryContents(cpkFolder, out _, out var directories);
        foreach (var directory in directories)
        {
            WindowsDirectorySearcher.GetDirectoryContentsRecursive(directory.FullPath, out var files, out _);
            builder.AddItem(new BuilderItem(modId, directory.FullPath, files));
        }
    }

    private void TriggerHotReload(object sender, FileSystemEventArgs e)
    {
        if (!_canRebuild)
            return;

        ClearHotReload();
        Rebuild_Internal();
    }

    private void ClearHotReload()
    {
        foreach (var watcher in _watchers)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        _watchers.Clear();
    }

    /// <summary>
    /// Unbinds all, deletes data, rebinds all.
    /// </summary>
    private void Rebuild_Internal()
    {
        _logger.Info("Hot Reload Triggered, Rebuilding");
        
        // Unbind, build and rebind.
        var unbindContext = new ICriFsRedirectorApi.UnbindContext()
        {
            BindDirectory = _bindDirAcquirer.BindDirectory
        };

        CpkBinder.UnbindAll();
        
        foreach (var unbindCallback in _unbindCallbacks)
            unbindCallback(unbindContext);
        
        // Try delete directory.
        try
        {
            Directory.Delete(_bindDirAcquirer.BindDirectory, true);
            Directory.CreateDirectory(_bindDirAcquirer.BindDirectory);
        }
        catch (Exception e)
        {
            _logger.Warning($"Failed to delete original binding directory, uh oh. {e.Message}");
        }
        
        Build();
        CpkBinder.BindAll();
    }

    /// <summary>
    /// Checks if there is a folder for redirected CPK data.
    /// </summary>
    /// <param name="folderToTest">The folder to check.</param>
    /// <param name="probingPath">Relative path in folder to check.</param>
    /// <param name="cpkFolder">Folder containing the CPK data to redirect.</param>
    /// <returns>True if exists, else false.</returns>
    private bool TryGetCpkFolder(string folderToTest, string probingPath, out string cpkFolder)
    {
        cpkFolder = Path.Combine(folderToTest, probingPath);
        return Directory.Exists(cpkFolder);
    }

    public void AddUnbindCallback(Action<ICriFsRedirectorApi.UnbindContext> callback) => _unbindCallbacks.Add(callback);

    public void AddBindCallback(Action<ICriFsRedirectorApi.BindContext> callback) => _bindCallbacks.Add(callback);
}