namespace CriFs.V2.Hook.Utilities;

/// <summary>
/// Factory class which provides convenience methods to create instances of <see cref="FileSystemWatcher"/>.
/// </summary>
public static class FileSystemWatcherFactory
{
    /// <summary>
    /// A factory method that creates a <see cref="FileSystemWatcher"/> which calls a specified method
    /// <see cref="Action{T}"/> when files at a given path change.
    /// </summary>
    /// <param name="watchDirectory">The path of the directory containing the stuff to watch.</param>
    /// <param name="action">The function to run when something is altered or changed.</param>
    /// <param name="enableSubdirectories">Decides whether subdirectories in a given path should be monitored.</param>
    /// <param name="filter">The filter used to determine which files are being watched for.</param>
    public static FileSystemWatcher Create(string watchDirectory, FileSystemEventHandler action, bool enableSubdirectories = true, string filter = "*.*")
    {
        var watcher = new FileSystemWatcher(watchDirectory);
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = enableSubdirectories;
        watcher.Filter = filter;
        watcher.NotifyFilter = 0;

        watcher.Deleted += action;
        watcher.Changed += action;
        watcher.Created += action;
        watcher.Renamed += (sender, args) => action(sender, args);
        watcher.NotifyFilter |= NotifyFilters.DirectoryName | NotifyFilters.FileName;
        watcher.NotifyFilter |= NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;
        watcher.NotifyFilter |= NotifyFilters.DirectoryName | NotifyFilters.FileName;
        watcher.NotifyFilter |= NotifyFilters.DirectoryName | NotifyFilters.FileName;
        
        return watcher;
    }
}