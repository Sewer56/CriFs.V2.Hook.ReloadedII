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
    
    [DisplayName("Print File Registration")]
    [Description("Prints when a file is registered onto the internal CRI File Loader\nThis can be useful to find out when a file is being accessed for the first time.")]
    [DefaultValue(false)]
    public bool PrintFileRegister { get; set; } = false;
    
    [DisplayName("Print File Redirects")]
    [Description("Prints redirected files to console using the Info log level.")]
    [DefaultValue(false)]
    public bool PrintFileRedirects { get; set; } = false;
    
    [DisplayName("Print Binder Access")]
    [Description("Prints all instances of the Binder's Find function using the Info log level.\nThis can be used to detect file access initiated from internal CRI logic.")]
    [DefaultValue(false)]
    public bool PrintBinderAccess { get; set; } = false;
    
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