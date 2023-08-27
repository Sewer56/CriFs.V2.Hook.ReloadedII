using System.Runtime.InteropServices;
using Reloaded.Memory;
using Reloaded.Memory.Interfaces;
using static CriFs.V2.Hook.CRI.CpkBinderPointers;

namespace CriFs.V2.Hook.Hooks;

/// <summary>
///     This file contains patches for disabling CRI logging
/// </summary>
public static unsafe partial class CpkBinder
{
    private static readonly byte[] _previousCallCode = new byte[5];
    private static readonly byte[] _newCallCode = { 0x90, 0x90, 0x90, 0x90, 0x90 };

    public static void SetDisableLogging(bool disableLogging)
    {
        // Not supported on non-x64
        // TODO: Separate patch for x86
        if (RuntimeInformation.ProcessArchitecture != Architecture.X64)
            return;

        // Patch disable logging permanently
        if (Pointers.DisableFileBindWarning == 0)
            return;

        var disableWarn = (byte*)Pointers.DisableFileBindWarning;
        if (disableLogging)
        {
            Memory.Instance.SafeRead((nuint)disableWarn, _previousCallCode.AsSpan());
            Memory.Instance.SafeWrite((nuint)disableWarn, _newCallCode.AsSpan());
        }
        else
        {
            Memory.Instance.SafeWrite((nuint)disableWarn, _previousCallCode.AsSpan());
        }
    }
}