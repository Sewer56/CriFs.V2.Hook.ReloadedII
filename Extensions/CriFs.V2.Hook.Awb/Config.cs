using System.ComponentModel;
using CriFs.V2.Hook.Awb.Template.Configuration;
using FileEmulationFramework.Lib.Utilities;

namespace CriFs.V2.Hook.Awb;

public class Config : Configurable<Config>
{
    /*
        User Properties:
            - Please put all of your configurable properties here.

        By default, configuration saves as "Config.json" in mod user config folder.    
        Need more config files/classes? See Configuration.cs

        Available Attributes:
        - Category
        - DisplayName
        - Description
        - DefaultValue

        // Technically Supported but not Useful
        - Browsable
        - Localizable

        The `DefaultValue` attribute is used as part of the `Reset` button in Reloaded-Launcher.
    */

    [DisplayName("Log Level")]
    [Description("Declares which elements should be logged to the console.\nMessages less important than this level will not be logged.")]
    [DefaultValue(LogSeverity.Information)]
    public LogSeverity LogLevel { get; set; } = LogSeverity.Information;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}