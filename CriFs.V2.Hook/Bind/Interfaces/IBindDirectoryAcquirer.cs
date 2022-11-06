namespace CriFs.V2.Hook.Bind.Interfaces;

/// <summary>
/// Interface that can be used for acquiring the binding directory.
/// </summary>
public interface IBindDirectoryAcquirer
{
    /// <summary>
    /// Acquires the bind directory.
    /// </summary>
    public string BindDirectory { get; }
}