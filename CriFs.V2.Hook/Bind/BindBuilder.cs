using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CriFs.V2.Hook.Interfaces;
using FileEmulationFramework.Lib;
using FileEmulationFramework.Lib.IO;

namespace CriFs.V2.Hook.Bind;

/// <summary>
/// Utility that builds folders with files that we will inject into games through any of the following methods
/// - File Emulation
/// - Binding (e.g. CRI CPK Binding)
/// </summary>
public class BindBuilder
{
    /// <summary>
    /// The folder where all the data to be bound will be stored.
    /// </summary>
    public string OutputFolder { get; private set; }

    /// <summary>
    /// Current list of items that will constitute the final output.
    /// </summary>
    public List<BuilderItem> Items { get; private set; } = new();

    /// <summary>
    /// If set all data will be bound under this name, else not.
    /// </summary>
    public string? BindFolderName { get; private set; }

    /// <summary/>
    /// <param name="outputFolder">The folder where the generated data to be bound will be stored.</param>
    /// <param name="bindFolderName">If set all data will be bound under this name, else not.</param>
    public BindBuilder(string outputFolder, string? bindFolderName = null)
    {
        OutputFolder = outputFolder;
        BindFolderName = bindFolderName;
    }

    /// <summary>
    /// Adds an item to be used in the output.
    /// </summary>
    /// <param name="item">The item to be included in the output.</param>
    public void AddItem(BuilderItem item) => Items.Add(item);

    /// <summary>
    /// Builds the bind folders :P.
    /// </summary>
    /// <param name="bindCallbacks">Callbacks used for binding the data.</param>
    /// <param name="files">List of all the files in the bind folder.</param>
    /// <returns>The folder inside which bound data is contained.</returns>
    public string Build(List<Action<ICriFsRedirectorApi.BindContext>> bindCallbacks, out Dictionary<string, List<ICriFsRedirectorApi.BindFileInfo>> files)
    {
        // This code finds duplicate files should we ever need to do merging in the future.
        files = GetFiles();
        
        // Normalize keys so all mods go in same base directory
        var context = new ICriFsRedirectorApi.BindContext()
        {
            BindDirectory = OutputFolder,
            RelativePathToFileMap = files
        };
        
        foreach (var bindCallback in bindCallbacks)
            bindCallback(context);
        
        // Add data in `Bind` folder to the output.
        WindowsDirectorySearcher.GetDirectoryContentsRecursive(OutputFolder, out var bindFolderFiles, out _);
        foreach (var file in bindFolderFiles)
        {
            var relativePath = file.DirectoryPath.Substring(OutputFolder.Length + 1);
            relativePath = Path.Combine(relativePath, file.FileName); 
            var bindInfo = new ICriFsRedirectorApi.BindFileInfo()
            {
                ModId = "CriFs.V2.Hook",
                FullPath = Path.Combine(file.DirectoryPath, file.FileName),
                LastWriteTime = DateTime.Now
            };
            
            if (files.TryGetValue(relativePath, out var existing))
                existing.Add(bindInfo);
            else
                files[relativePath] = new List<ICriFsRedirectorApi.BindFileInfo> { bindInfo };
        }
        
        return OutputFolder;
    }

    /// <summary>
    /// Finds all files within the given builder items.
    /// </summary>
    /// <returns>A dictionary of relative path [in custom bind folder] to full paths of duplicate files that would potentially need merging.</returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public Dictionary<string, List<ICriFsRedirectorApi.BindFileInfo>> GetFiles()
    {
        var relativeToFullPaths = new Dictionary<string, List<ICriFsRedirectorApi.BindFileInfo>>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in CollectionsMarshal.AsSpan(Items))
        {
            foreach (var file in item.Files)
            {
                var fullPath = Path.Combine(file.DirectoryPath, file.FileName);
                var relativePath = Route.GetRoute(Path.GetDirectoryName(item.FolderPath)!, fullPath);
                
                // Inject custom bind folder name.
                relativePath = string.IsNullOrEmpty(BindFolderName) ? relativePath : ReplaceFirstFolderInPath(relativePath, BindFolderName);
                if (!relativeToFullPaths.TryGetValue(relativePath, out var existingPaths))
                {
                    existingPaths = new List<ICriFsRedirectorApi.BindFileInfo>();
                    relativeToFullPaths[relativePath] = existingPaths;
                }
                    
                existingPaths.Add(new ICriFsRedirectorApi.BindFileInfo()
                {
                    FullPath = fullPath,
                    LastWriteTime = file.LastWriteTime,
                    ModId = item.ModId
                });
            }
        }
        
        return relativeToFullPaths;
    }
    
    private string ReplaceFirstFolderInPath(string originalRelativePath, string newFolderName)
    {
        var separatorIndex = originalRelativePath.IndexOf(Path.DirectorySeparatorChar);
        if (separatorIndex == -1)
            separatorIndex = originalRelativePath.IndexOf(Path.AltDirectorySeparatorChar);
        
        return newFolderName + Path.DirectorySeparatorChar + originalRelativePath.Substring(separatorIndex + 1);
    }
}

/// <summary>
/// Represents an individual item that can be submitted to the builder.
/// </summary>
/// <param name="ModId">ID of the mod where the file comes from.</param>
/// <param name="FolderPath">Path to the base folder containing the contents.</param>
/// <param name="Files">The contents of said base folder.</param>
public record struct BuilderItem(string ModId, string FolderPath, List<FileInformation> Files);