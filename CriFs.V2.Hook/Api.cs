using System.Runtime.InteropServices;
using CriFs.V2.Hook.Bind;
using CriFs.V2.Hook.Bind.Interfaces;
using CriFs.V2.Hook.Interfaces;
using CriFs.V2.Hook.Interfaces.Structs;
using CriFs.V2.Hook.Utilities;
using CriFsV2Lib;
using CriFsV2Lib.Definitions;
using FileEmulationFramework.Lib.IO;

namespace CriFs.V2.Hook;

/// <inheritdoc/>
public class Api : ICriFsRedirectorApi
{
    private readonly ReloadedBindBuilderCreator _reloadedBuilder;
    private readonly CpkContentCache _cpkContentCache;
    private readonly ICurrentProcessProvider _currentProcessProvider;
    private readonly IProcessListProvider _processListProvider;
    private string _mainModulePath;
    private string[]? _cpkFiles;

    public Api(ReloadedBindBuilderCreator reloadedBuilder, CpkContentCache cpkContentCache, string mainModulePath, ICurrentProcessProvider currentProcessProvider, IProcessListProvider processListProvider)
    {
        _reloadedBuilder = reloadedBuilder;
        _cpkContentCache = cpkContentCache;
        _mainModulePath = mainModulePath;
        _currentProcessProvider = currentProcessProvider;
        _processListProvider = processListProvider;
    }

    /// <inheritdoc/>
    public void AddProbingPath(string relativePath) => _reloadedBuilder.AddProbingPath(relativePath);

    /// <inheritdoc/>
    public void AddUnbindCallback(Action<ICriFsRedirectorApi.UnbindContext> callback) => _reloadedBuilder.AddUnbindCallback(callback);

    /// <inheritdoc/>
    public void AddBindCallback(Action<ICriFsRedirectorApi.BindContext> callback) => _reloadedBuilder.AddBindCallback(callback);

    /// <inheritdoc/>
    public string GenerateBindingDirectory(string baseDirectory)
    {
        return new BindDirectoryAcquirer(baseDirectory, _currentProcessProvider, _processListProvider).BindDirectory;
    }

    /// <inheritdoc/>
    public CpkCacheEntry GetCpkFilesCached(string filePath) => _cpkContentCache.Get(filePath);

    /// <inheritdoc />
    public ICriFsLib GetCriFsLib() => CriFsLib.Instance;
    
    /// <inheritdoc/>
    public string[] GetCpkFilesInGameDir()
    {
        if (_cpkFiles != null)
            return _cpkFiles;
        
        // Note: In some cases, applications might store binaries in subfolders.
        // We will go down folders until we find a CPK file.
        var currentFolder = _mainModulePath;
        var results = new List<string>();
        var fileInfo = new List<FileInformation>();
        var directoryInfo = new List<DirectoryInformation>();
        
        do
        {
            currentFolder = Path.GetDirectoryName(currentFolder);
            if (currentFolder == null)
                return Array.Empty<string>();
            
            fileInfo.Clear();
            directoryInfo.Clear();
            WindowsDirectorySearcher.GetDirectoryContentsRecursive(currentFolder, fileInfo, directoryInfo);

            foreach (var file in CollectionsMarshal.AsSpan(fileInfo))
            {
                if (file.FileName.EndsWith(".cpk", StringComparison.OrdinalIgnoreCase))
                    results.Add(Path.GetFullPath(Path.Combine(file.DirectoryPath, file.FileName)));
            }
        } 
        while (results.Count <= 0);

        _cpkFiles = results.ToArray();
        return _cpkFiles;
    }
}