namespace CriFs.V2.Hook;

/// <summary>
/// Locations of items inside 3rd party mod packages.
/// </summary>
public static class Routes
{
    /// <summary>
    /// Relative file path used by the CPK redirector.
    /// </summary>
    public const string DefaultProbingPath = "CRIFsV2Hook/CPK";
    
    /// <summary>
    /// Gets the base directory used for binding of CPKs.
    /// </summary>
    /// <param name="modConfigDirectory">Config directory for the Reloaded mod.</param>
    public static string GetBindBaseDirectory(string modConfigDirectory) => Path.Combine(modConfigDirectory, "Bind");
}