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
    private static bool _previousCallCode1Set;
    private static readonly byte[] _previousCallCode1 = new byte[5];
    private static readonly byte[] _newCallCode1 = { 0x90, 0x90, 0x90, 0x90, 0x90 };

    private static bool _previousCallCode2Set;
    private static readonly byte[] _previousCallCode2 = new byte[5];
    private static readonly byte[] _newCallCode2 = { 0x90, 0x90, 0x90, 0x90, 0x90 };

    public static void SetDisableLogging(bool disableLogging)
    {
        // Not supported on non-x64
        // TODO: Separate patch for x86
        if (RuntimeInformation.ProcessArchitecture != Architecture.X64)
            return;

        PatchContentNotInCpk1(disableLogging);
        PatchContentNotInCpk2(disableLogging);
    }

    private static void PatchContentNotInCpk1(bool disableLogging)
    {
        // Patch disable logging permanently
        if (Pointers.DisableFileBindWarning == 0)
            return;

        var disableWarn = (byte*)Pointers.DisableFileBindWarning;
        if (disableLogging)
        {
            _previousCallCode1Set = true;
            Memory.Instance.SafeRead((nuint)disableWarn, _previousCallCode1.AsSpan());
            Memory.Instance.SafeWrite((nuint)disableWarn, _newCallCode1.AsSpan());
        }
        else if(_previousCallCode1Set)
        {
            Memory.Instance.SafeWrite((nuint)disableWarn, _previousCallCode1.AsSpan());
        }
    }
    
    private static void PatchContentNotInCpk2(bool disableLogging)
    {
        // Patch disable logging permanently
        if (Pointers.DisableGetContentsInfoDetailsWarning == 0)
            return;

        var disableWarn = (byte*)Pointers.DisableGetContentsInfoDetailsWarning;
        if (disableLogging)
        {
            _previousCallCode2Set = true;
            Memory.Instance.SafeRead((nuint)disableWarn, _previousCallCode2.AsSpan());
            Memory.Instance.SafeWrite((nuint)disableWarn, _newCallCode2.AsSpan());
        }
        else if (_previousCallCode2Set)
        {
            Memory.Instance.SafeWrite((nuint)disableWarn, _previousCallCode2.AsSpan());
        }
    }
}