using CriFs.V2.Hook.Interfaces.Structs;
using CriFsV2Lib.Definitions;

namespace CriFs.V2.Hook.Interfaces;

/// <summary>
/// API for controlling the behaviour of the redirector.
/// </summary>
public interface ICriFsRedirectorApi
{
    /// <summary>
    /// Adds a path (relative to other mods' main folder) that is used for probing for mod files.
    /// </summary>
    public void AddProbingPath(string relativePath);

    /// <summary>
    /// Adds a method that is fired when the binder performs an unbind (e.g. Hot Reload).
    /// </summary>
    /// <param name="callback">The callback method to fire.</param>
    public void AddUnbindCallback(Action<UnbindContext> callback);

    /// <summary>
    /// Adds a method that is fired when the binder bind is about to be created.
    /// </summary>
    /// <param name="callback">Method which contains the binding information.</param>
    public void AddBindCallback(Action<BindContext> callback);

    /// <summary>
    /// Generates a directory that can be used for binding custom in <see cref="AddBindCallback"/> callback.  
    /// Guarantees directory will be unique for this process (thus allowing multiple instance of application).  
    /// Guarantees directories related to dead processes/previous game runs will be cleaned up.  
    /// </summary>
    /// <param name="baseDirectory">The directory where your mod stores its binds, preferably `Bind` in mod config dir.</param>
    public string GenerateBindingDirectory(string baseDirectory);
    
    /// <summary>
    /// Returns list of all CPK files in game directory.
    /// This list is cached, used to speed up rebuilds.
    /// </summary>
    public string[] GetCpkFilesInGameDir();
    
    /// <summary>
    /// Gets the file contents of a given CPK file.
    /// Data can be read from this CPK file by using <see cref="ICriFsLib.CreateCpkReader"/> with FileStream.
    /// 
    /// This is an optimisation/speedup to prevent repeat parsing when multiple mods need to extract files.
    /// This cache is cleared after each rebuild.
    /// </summary>
    public CpkCacheEntry GetCpkFilesCached(string filePath);

    /// <summary>
    /// Gets an instance of the library used to read CPK files.
    /// </summary>
    public ICriFsLib GetCriFsLib();

    /// <summary>
    /// The context used for binding operations.
    /// </summary>
    public struct UnbindContext
    {
        /// <summary>
        /// The directory that is getting unbound.
        /// </summary>
        public string BindDirectory { get; set; }
    }
    
    /// <summary>
    /// The context used for binding operations.
    /// </summary>
    public struct BindContext
    {
        /// <summary>
        /// The directory that is getting bound.
        /// </summary>
        public string BindDirectory { get; set; }

        /// <summary>
        /// Contains list of all files that will be bound.
        /// </summary>
        public Dictionary<string, List<BindFileInfo>> RelativePathToFileMap { get; set; }
    }

    /// <summary>
    /// Information about an individual file used in binding context.
    /// </summary>
    public struct BindFileInfo
    {
        /// <summary>
        /// Full path to the file.
        /// </summary>
        public string FullPath;

        /// <summary>
        /// Last time file was written to.
        /// </summary>
        public DateTime LastWriteTime;

        /// <summary>
        /// ID of the mod where the file originates from.
        /// </summary>
        public string ModId;
    }
}

