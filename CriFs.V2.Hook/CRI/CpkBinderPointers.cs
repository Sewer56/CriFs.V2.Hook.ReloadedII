using CriFs.V2.Hook.Utilities;

namespace CriFs.V2.Hook.CRI;

internal static class CpkBinderPointers
{
    internal static long BindCpk;
    internal static long BindDir;
    internal static long GetSizeForBindDir;
    internal static long LoadRegisteredFile;
    internal static long SetPriority;
    internal static long GetStatus;
    internal static long Unbind;
    
    public static void Init(SigScanHelper helper, nint baseAddr)
    {
        helper.FindPatternOffset("48 83 EC 48 48 8B 44 24 78 C7 44 24 30 01 00 00 00 48 89 44 24 28 8B", 
            (offset) => BindCpk = baseAddr + offset, "CRI Bind CPK");

        helper.FindPatternOffset("48 8B C4 48 89 58 08 48 89 68 10 48 89 70 18 48 89 78 20 41 54 41 56 41 57 48 83 EC 40 48", 
            (offset) => BindDir = baseAddr + offset, "CRI Bind Directory");

        helper.FindPatternOffset("48 89 5C 24 08 57 48 83 EC 20 8B FA E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 75 18", 
            (offset) => SetPriority = baseAddr + offset, "CRI Set Priority");

        helper.FindPatternOffset("48 89 5C 24 08 57 48 83 EC 20 48 8B DA 8B F9 85", 
            (offset) => GetStatus = baseAddr + offset, "CRI Get Status");

        helper.FindPatternOffset("48 89 5C 24 08 57 48 83 EC 20 8B F9 E8 ?? ?? ?? ?? 48 8B", 
            (offset) => Unbind = baseAddr + offset, "CRI Unbind");
        
        helper.FindPatternOffset("48 83 EC 28 4D 85 C0 75 1B", 
            (offset) => GetSizeForBindDir = baseAddr + offset, "CRI Get Size for Bind Dir");
        
        helper.FindPatternOffset("48 89 5C 24 10 4C 89 4C 24 20 55 56 57 41 54 41 55 41 56 41 57 48 81", 
            (offset) => LoadRegisteredFile = baseAddr + offset, "CRI Load File");
    }
}