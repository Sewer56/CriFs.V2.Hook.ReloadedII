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
}