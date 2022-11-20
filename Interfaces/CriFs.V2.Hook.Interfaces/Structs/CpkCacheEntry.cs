namespace CriFs.V2.Hook.Interfaces.Structs;

/// <summary>
/// Represents an individual entry storing cached data for each CPK.
/// </summary>
public struct CpkCacheEntry
{
    /// <summary>
    /// The files stores for this CPK.
    /// </summary>
    public CachedCpkFile[] Files { get; init; }
    
    /// <summary>
    /// Last time CPK was modified, can be used to invalidate custom user cache.
    /// </summary>
    public DateTime LastModified { get; init; }
    
    /// <summary>
    /// Contains a map of all relative paths in CPK to corresponding index in <see cref="Files"/> array.
    /// Directory separators normalized to <see cref="Path.DirectorySeparatorChar"/>.
    /// </summary>
    public Dictionary<string, int> FilesByPath { get; init; }
    
    /// <summary>
    /// Contains a map of all file names corresponding to index in <see cref="Files"/> array.
    /// For fast lookup.
    /// </summary>
    public Dictionary<string, int> FilesByFileName { get; init; }
}