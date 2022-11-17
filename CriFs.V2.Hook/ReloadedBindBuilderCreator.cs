using CriFs.V2.Hook;
using CriFs.V2.Hook.Bind;
using CriFs.V2.Hook.Bind.Interfaces;
using CriFs.V2.Hook.Hooks;
using CriFs.V2.Hook.Interfaces;
using CriFs.V2.Hook.Utilities;
using CriFsV2Lib.Definitions.Utilities;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Mod.Interfaces;

namespace p5rpc.modloader;

/// <summary>
/// Class that bridges the code between Reloaded mods and the CPK bind builder & hooks.
/// </summary>
public class ReloadedBindBuilderCreator
{
    private readonly IModLoader _loader;
    private readonly Logger _logger;
    private readonly IBindDirectoryAcquirer _bindDirAcquirer;
    private readonly CpkContentCache _cpkContentCache;
    private bool _canRebuild = false;

    private List<string> _probingPaths = new(2) { Routes.DefaultProbingPath, "P5REssentials/CPK" }; // <= Legacy Support
    private List<FileSystemWatcher> _watchers = new();
    private List<Action<ICriFsRedirectorApi.UnbindContext>> _unbindCallbacks = new();
    private List<Action<ICriFsRedirectorApi.BindContext>> _bindCallbacks = new();

    public ReloadedBindBuilderCreator(IModLoader loader, Logger logger, IBindDirectoryAcquirer bindDirAcquirer,
        CpkContentCache cpkContentCache)
    {
        _loader = loader;
        _logger = logger;
        _bindDirAcquirer = bindDirAcquirer;
        _cpkContentCache = cpkContentCache;
    }

    /// <summary>
    /// Adds a path that is searched for mod content.
    /// </summary>
    /// <param name="relativePath">A path relative to folder of a mod, like 'P5REssentials/CPK'</param>
    public void AddProbingPath(string relativePath) => _probingPaths.Add(relativePath);

    /// <summary>
    /// Checks if mod has any qualifying folders and triggers a rebuild if necessary.
    /// </summary>
    /// <param name="modConfig">The mod configuration.</param>
    public void RebuildIfNeeded(IModConfig modConfig)
    {
        if (!_canRebuild)
            return;
        
        foreach (var probingPath in _probingPaths)
        {
            var path = _loader.GetDirectoryForModId(modConfig.ModId);
            if (!TryGetCpkFolder(path, probingPath, out _)) 
                continue;
            
            Rebuild();
            return;
        }
    }

    /// <summary>
    /// Builds the bind folders :P.
    /// </summary>
    public void Build()
    {
        _cpkContentCache.Clear();
        
        // Get binding directory & cleanup.
        var mods       = _loader.GetActiveMods();
        var builder    = new BindBuilder(_bindDirAcquirer.BindDirectory, "R2");
        
        // Get list of input mods.
        foreach (var mod in mods)
        foreach (var probingPath in _probingPaths)
            Add(builder, (IModConfig)mod.Generic, probingPath);

        builder.Build(_bindCallbacks);
        ArrayRental.Reset(); // cleanup mem
        _canRebuild = true;
    }
    
    /// <summary>
    /// (Conditionally) adds CPK folders for binding to the builder.
    /// </summary>
    /// <param name="modConfig">Mod configuration.</param>
    /// <param name="probingPath">The probing path to use</param>
    private void Add(BindBuilder builder, IModConfig modConfig, string probingPath)
    {
        var path = _loader.GetDirectoryForModId(modConfig.ModId);
        if (!TryGetCpkFolder(path, probingPath, out var cpkFolder)) 
            return;
        
        _logger.Info("Adding CPK Folder: {0}", cpkFolder);
        _watchers.Add(FileSystemWatcherFactory.Create(cpkFolder, TriggerHotReload));
        
        // Get all CPK folders
        WindowsDirectorySearcher.TryGetDirectoryContents(cpkFolder, out _, out var directories);
        foreach (var directory in directories)
        {
            WindowsDirectorySearcher.GetDirectoryContentsRecursive(directory.FullPath, out var files, out _);
            builder.AddItem(new BuilderItem(directory.FullPath, files));
        }
    }

    private void TriggerHotReload(object sender, FileSystemEventArgs e)
    {
        var watcher = (FileSystemWatcher)sender;
        watcher.EnableRaisingEvents = false;
        watcher.Dispose();
        _watchers.Clear();
        Rebuild();
    }

    /// <summary>
    /// Unbinds all, deletes data, rebinds all.
    /// </summary>
    private void Rebuild()
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