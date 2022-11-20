using CriFsV2Lib.Definitions.Structs;

namespace CriFs.V2.Hook.Interfaces.Structs;

/// <summary>
/// A <see cref="CpkFile"/> obtained from cache.
/// </summary>
public class CachedCpkFile
{
    /// <summary>
    /// Full path inside the CPK (directory & filename combined), normalized to use system path separators.
    /// </summary>
    public string FullPath = string.Empty;
    
    /// <summary>
    /// The CPK file from CPK library.
    /// </summary>
    public CpkFile File;
}