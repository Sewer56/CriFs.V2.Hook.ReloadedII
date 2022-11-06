using CriFs.V2.Hook.Bind.Interfaces;

namespace CriFs.V2.Hook.Bind;

/// <inheritdoc/>
public class BindDirectoryAcquirer : IBindDirectoryAcquirer
{
    public string BindDirectory { get; }
    
    public BindDirectoryAcquirer(string modConfigDirectory, ICurrentProcessProvider currentProcessProvider, IProcessListProvider processListProvider)
    {
        var folderGen = new BindingOutputDirectoryGenerator(Routes.GetBindBaseDirectory(modConfigDirectory));
        BindDirectory = folderGen.Generate(currentProcessProvider);
        folderGen.Cleanup(processListProvider);
    }
}