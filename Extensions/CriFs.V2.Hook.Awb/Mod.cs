using System.Runtime.CompilerServices;
using AWB.Stream.Emulator.Interfaces;
using CriFs.V2.Hook.Awb.Template;
using CriFs.V2.Hook.Interfaces;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Mod.Interfaces;

[module: SkipLocalsInit]
namespace CriFs.V2.Hook.Awb;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public partial class Mod : ModBase // <= Do not Remove.
{
    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly Logger _logger;

    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;
    
    private readonly IAwbEmulator _awbEmulator = null!;
    private ICriFsRedirectorApi _criFsApi = null!;
    private string _bindingDirectory = null!;

    public Mod(ModContext context)
    {
        var modLoader = context.ModLoader;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;
        _logger = new Logger(context.Logger, _configuration.LogLevel);

        // For more information about this template, please see
        // https://reloaded-project.github.io/Reloaded-II/ModTemplate/

        // If you want to implement e.g. unload support in your mod,
        // and some other neat features, override the methods in ModBase.
        modLoader.GetController<IAwbEmulator>().TryGetTarget(out _awbEmulator!);
        modLoader.GetController<ICriFsRedirectorApi>().TryGetTarget(out _criFsApi!);
        _criFsApi.AddUnbindCallback(OnCpkUnbind);
        _criFsApi.AddBindCallback(OnCpkBind);
        
        var modConfigDirectory = modLoader.GetModConfigDirectory(_modConfig.ModId);
        _bindingDirectory = _criFsApi.GenerateBindingDirectory(modConfigDirectory);
    }
    
    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;
        _logger.LogLevel = _configuration.LogLevel;
        _logger.Info($"[{_modConfig.ModId}] Config Updated: Applying");
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}