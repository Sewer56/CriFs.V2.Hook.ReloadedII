using CriFs.V2.Hook.Utilities;

namespace CriFs.V2.Hook.CRI;

internal static class CpkBinderPointers
{
    internal static long InitializeLibrary;
    internal static long CalculateWorkSizeForLibrary;
    internal static long BindCpk;
    internal static long BindFiles;
    internal static long GetSizeForBindFiles;
    internal static long LoadRegisteredFile;
    internal static long SetPriority;
    internal static long GetStatus;
    internal static long Unbind;
    
    public static void Init(SigScanHelper helper, nint baseAddr)
    {
        if (nint.Size == 8)
        {
            helper.FindPatternOffset("4C 8B DC 49 89 5B 08 49 89 6B 10 49 89 7B 18 41 56 48 83 EC 60 48 8D 05 2C 89 46 01 41 8B E8 48 89 05 2A F8 49 02", 
                (offset) => InitializeLibrary = baseAddr + offset, "CRI Initialize FS Library");
            
            helper.FindPatternOffset("48 89 5C 24 18 48 89 74 24 20 55 57 41 54 41 56 41 57 48 8D 6C 24 C9 48 81 EC A0", 
                (offset) => CalculateWorkSizeForLibrary = baseAddr + offset, "CRI Calculate Work Size for Library");
            
            helper.FindPatternOffset("48 83 EC 48 48 8B 44 24 78 C7 44 24 30 01 00 00 00 48 89 44 24 28 8B", 
                (offset) => BindCpk = baseAddr + offset, "CRI Bind CPK");

            helper.FindPatternOffset("48 83 EC 48 48 8B 44 24 78 48 89 44 24 30 8B 44 24 70 89 44 24 28 4C 89 4C 24 20 41 83", 
                (offset) => BindFiles = baseAddr + offset, "CRI Bind Files");

            helper.FindPatternOffset("48 89 5C 24 08 57 48 83 EC 20 8B FA E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 75 18", 
                (offset) => SetPriority = baseAddr + offset, "CRI Set Priority");

            helper.FindPatternOffset("48 89 5C 24 08 57 48 83 EC 20 48 8B DA 8B F9 85", 
                (offset) => GetStatus = baseAddr + offset, "CRI Get Status");

            helper.FindPatternOffset("48 89 5C 24 08 57 48 83 EC 20 8B F9 E8 ?? ?? ?? ?? 48 8B", 
                (offset) => Unbind = baseAddr + offset, "CRI Unbind");
        
            helper.FindPatternOffset("48 89 5C 24 08 48 89 74 24 20 57 48 81 EC 50", 
                (offset) => GetSizeForBindFiles = baseAddr + offset, "CRI Get Size for Bind Files");
        
            helper.FindPatternOffset("48 89 5C 24 10 4C 89 4C 24 20 55 56 57 41 54 41 55 41 56 41 57 48 81", 
                (offset) => LoadRegisteredFile = baseAddr + offset, "CRI Load File");
        }
        else if (nint.Size == 4)
        {
            throw new Exception("32-bit titles are not currently supported.");
        }
    }
}