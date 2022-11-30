using System.ComponentModel;
using CriFs.V2.Hook.Template.Configuration;
using FileEmulationFramework.Lib.Utilities;

namespace CriFs.V2.Hook;

public class Config : Configurable<Config>
{
    [DisplayName("Disable Bind Warnings(s)")]
    [Description("Disables warnings printed to the console as a result of CRI loading files from disk.")]
    [DefaultValue(true)]
    public bool DisableCriBindLogging { get; set; } = true;
    
    [DisplayName("Print File Access")]
    [Description("Prints loaded file to console using the Info log level.")]
    [DefaultValue(false)]
    public bool PrintFileAccess { get; set; } = false;
    
    [DisplayName("Hot Reload")]
    [Description("Allows for loaded files to be updated/replaced at runtime.")]
    [DefaultValue(false)]
    public bool HotReload { get; set; } = false;
    
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