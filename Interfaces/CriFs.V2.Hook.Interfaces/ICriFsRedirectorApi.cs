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
        public Dictionary<string, List<string>> RelativePathToFileMap { get; set; }
    }
}

