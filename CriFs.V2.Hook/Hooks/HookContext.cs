using CriFs.V2.Hook.Utilities;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Hooks.Definitions;

namespace CriFs.V2.Hook.Hooks;

/// <summary>
/// Data passed down to all patches in this mod.
/// </summary>
internal ref struct HookContext
{
    public IntPtr BaseAddress { get; init; }
    public Config Config { get; init; }
    public SigScanHelper ScanHelper { get; init; }
    public IReloadedHooks Hooks { get; init; }
    public Logger Logger { get; init; }
}