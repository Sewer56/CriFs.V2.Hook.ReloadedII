using CriFs.V2.Hook.Interfaces.Structs;
using CriFsV2Lib;

namespace CriFs.V2.Hook.Utilities;

/// <summary>
/// Caches the contents of CPK files.
/// </summary>
public class CpkContentCache
{
    private Dictionary<string, CpkCacheEntry> _pathToEntry = new();

    /// <summary>
    /// Gets contents of CPK file from the cache.
    /// </summary>
    /// <param name="filePath">Path to the CPK file.</param>
    public CpkCacheEntry Get(string filePath)
    {
        var normalizedPath = Path.GetFullPath(filePath);
        if (_pathToEntry.TryGetValue(normalizedPath, out var result))
            return result;

        return CacheCpk(normalizedPath);
    }

    private CpkCacheEntry CacheCpk(string normalizedPath)
    {
        using var stream = new FileStream(normalizedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        var reader  = CriFsLib.Instance.CreateCpkReader(stream, false);
        var files   = reader.GetFiles();
        
        var array = GC.AllocateUninitializedArray<CachedCpkFile>(files.Length);
        var relativePathDictionary = new Dictionary<string, int>(files.Length, StringComparer.OrdinalIgnoreCase);
        var fileNameDictionary = new Dictionary<string, int>(files.Length, StringComparer.OrdinalIgnoreCase);
        
        for (int x = 0; x < files.Length; x++)
        {
            ref var file = ref files[x];
            var cachedFile = new CachedCpkFile()
            {
                File = file
            };

            cachedFile.FullPath = file.Directory != null
                ? $"{file.Directory.Replace('/', '\\')}\\{file.FileName}" 
                : file.FileName;

            array[x] = cachedFile;
            relativePathDictionary[cachedFile.FullPath] = x;
            fileNameDictionary[file.FileName] = x;
        }

        var result = new CpkCacheEntry()
        {
            Files = array,
            FilesByPath = relativePathDictionary,
            FilesByFileName = fileNameDictionary,
            LastModified = File.GetLastWriteTime(stream.SafeFileHandle)
        };
        
        _pathToEntry[normalizedPath] = result;
        return result;
    }

    /// <summary>
    /// Clears the cache.
    /// </summary>
    public void Clear() => _pathToEntry.Clear();
}