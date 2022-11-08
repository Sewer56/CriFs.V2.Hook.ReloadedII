using CriFs.V2.Hook.Interfaces;
using p5rpc.modloader;

namespace CriFs.V2.Hook;

/// <inheritdoc/>
public class Api : ICriFsRedirectorApi
{
    private readonly ReloadedBindBuilderCreator _reloadedBuilder;

    public Api(ReloadedBindBuilderCreator reloadedBuilder) => _reloadedBuilder = reloadedBuilder;
    
    /// <inheritdoc/>
    public void AddProbingPath(string relativePath) => _reloadedBuilder.AddProbingPath(relativePath);

    /// <inheritdoc/>
    public void AddUnbindCallback(Action<ICriFsRedirectorApi.UnbindContext> callback) => _reloadedBuilder.AddUnbindCallback(callback);

    /// <inheritdoc/>
    public void AddBindCallback(Action<ICriFsRedirectorApi.BindContext> callback) => _reloadedBuilder.AddBindCallback(callback);
}