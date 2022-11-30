using Reloaded.Memory.Sources;

namespace CriFs.V2.Hook.Hooks;

/// <summary>
/// Patch that disables logging of errors from accessing files via directory bind.
/// </summary>
internal class DontLogCriDirectoryBinds
{
    public static void Activate(in HookContext context)
    {
        var baseAddr = context.BaseAddress;
        if (!context.Config.DisableCriBindLogging) 
            return;
        
        context.ScanHelper.FindPatternOffset("48 8B FA 75 7E", (offset) =>
        {
            var nopJmpOne = (nuint)(baseAddr + offset + 3);
            var nopJmpTwo = nopJmpOne + 10;
            var nopJmp = new byte[] { 0x90, 0x90 };
            Memory.Instance.SafeWriteRaw(nopJmpOne, nopJmp);
            Memory.Instance.SafeWriteRaw(nopJmpTwo, nopJmp);
        }, "Disable CRI Bind Logging");
    }
}