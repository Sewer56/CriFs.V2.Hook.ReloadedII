using Reloaded.Memory.Sigscan.Definitions;
using Reloaded.Memory.Sigscan.Definitions.Structs;

namespace CriFs.V2.Hook.Utilities;

/// <summary>
/// Extensions for the <see cref="IScanner"/> class.
/// </summary>
internal static class ScannerExtensions
{
    internal static bool TryFindPattern(this IScanner scan, string pattern, int offset, out PatternScanResult res)
    {
        res = scan.FindPattern(pattern, offset);
        return res.Found;
    }
    
    internal static bool TryFindEitherPattern(this IScanner scan, string patternA, string patternB, int offset, out PatternScanResult res)
    {
        res = scan.FindPattern(patternA, offset);
        if (res.Found)
            return true;
        
        res = scan.FindPattern(patternB, offset);
        return res.Found;
    }
}