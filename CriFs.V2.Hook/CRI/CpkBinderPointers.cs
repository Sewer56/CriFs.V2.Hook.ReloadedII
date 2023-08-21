using CriFs.V2.Hook.Utilities;

// ReSharper disable InconsistentNaming

namespace CriFs.V2.Hook.CRI;

internal static class CpkBinderPointers
{
/*
    Below are Function Order in original `.obj` file(s)
    for each relevant group of pointers.

    [They were found in binary without dead code removal]
    If you are dealing with an unknown version of CRI,
    sort functions by address (ascending).

    They should match these functions.
    Note: Some functions may be skipped/not present,
          this will be because unused code will be eliminated.

    Note: Year is provided in listings; in some earlier/later versions,
          these groupings might change
*/

/*
    criFs_ .obj file [2019]

    criFs_AddressToPath
    criFs_BeginGroup
    criFs_CalculateWorkSizeForLibrary
    criFs_ControlFileIoMode
    criFs_DecrementReferenceCounter
    criFs_EndGroup
    criFs_ExecuteDataDecompression
    criFs_ExecuteFileAccess
    criFs_ExecuteFileAccessInternal
    criFs_ExecuteMain
    criFs_ExecuteMainInternal
    criFs_FinalizeLibrary
    criFs_Free
    criFs_GetDataDecompressionThreadPriority
    criFs_GetDefaultIoInterface
    criFs_GetDeviceInfo
    criFs_GetFileAccessThreadPriority
    criFs_GetFileIoMode
    criFs_GetInstallerThreadPriority_0
    criFs_GetMaxPathLength

    criFs_GetMemoryFileSystemThreadPriority
    criFs_GetNumBinds
    criFs_GetNumOpenedFiles
    criFs_GetNumUsedBinders
    criFs_GetNumUsedGroupLoaders
    criFs_GetNumUsedInstallers
    criFs_GetNumUsedLoaders
    criFs_GetNumUsedStdioHandles
    criFs_GetRuntimeLibraryVersionNumber
    criFs_GetServerThreadPriority
    criFs_IncrementReferenceCounter
    criFs_InitializeLibrary
    criFs_IsInitialized
    criFs_IsMemoryFileSystemPath
    criFs_Malloc
    criFs_PathToAddress
    criFs_SetBeginGroupCallback
    criFs_SetDataDecompressionThreadAffinityMask
    criFs_SetDataDecompressionThreadPriority
    criFs_SetDefaultPathSeparator
    criFs_SetDeviceInfo
    criFs_SetEndGroupCallback
    criFs_SetFileAccessThreadAffinityMask
    criFs_SetFileAccessThreadPriority
    criFs_SetInstallerThreadPriority
    criFs_SetLoadLimiterSize
    criFs_SetLoadLimiterUnit
    criFs_SetMemoryFileSystemSyncCopyLimit
    criFs_SetMemoryFileSystemThreadAffinityMask
    criFs_SetMemoryFileSystemThreadPriority
    criFs_SetOpenRetryMode
    criFs_SetReadRetryMode
    criFs_SetSelectIoCallback
    criFs_SetServerThreadAffinityMask

    criFs_SetServerThreadPriority
    criFs_SetUserFreeFunction
    criFs_SetUserMallocFunction
    criFs_UpdateHandleStatus
*/

    // Used for Init
    internal static long CriFs_CalculateWorkSizeForLibrary;
    internal static long CriFs_FinalizeLibrary;
    internal static long CriFs_InitializeLibrary;

/*
    criFsBinder .obj file [2019]
     
    There's also some other binder functions?? But they are
    in different .obj files. Maybe they were part of different .c/.h file

    criFsBinder_AllocFromUsrHeap
    criFsBinder_AnalyzeBinderHn
    criFsBinder_AnalyzeWorkSizeForBindCpk
    criFsBinder_AnalyzeWorkSizeForBindCpkById
    criFsBinder_BindCpk
    criFsBinder_BindCpkById
    criFsBinder_BindDirectory
    criFsBinder_BindFile
    criFsBinder_BindFiles
    criFsBinder_BindMemoryCpk
    criFsBinder_CalcWorkSize
    criFsBinder_CalculateWorkSizeForBindCpk
    criFsBinder_CleanImplicitUnbindList
    criFsBinder_Create
    criFsBinder_Destroy
    criFsBinder_DestroyAll
    criFsBinder_ExecuteServer
    criFsBinder_Finalize
    criFsBinder_Find
    criFsBinder_FindById
    criFsBinder_FindExById
    criFsBinder_FindGroupFromMultiBind
    criFsBinder_FindWithFullpath
    criFsBinder_FindWithFullpathEx
    criFsBinder_FreeToUsrHeap
    criFsBinder_GetBinderFromBinderId
    criFsBinder_GetBinderIdInfo
    criFsBinder_GetBinderTypeForPath
    criFsBinder_GetContentsFileCrc32ByFullPath
    criFsBinder_GetContentsFileCrc32ById
    criFsBinder_GetContentsFileCrc32ByIndex
    criFsBinder_GetContentsFileInfo
    criFsBinder_GetContentsFileInfoById
    criFsBinder_GetContentsFileInfoByIndex
    criFsBinder_GetContentsFileInfoByIndexForInternal
    criFsBinder_GetContentsNum
    criFsBinder_GetCpkCodecType
    criFsBinder_GetCpkCore

    criFsBinder_GetCpkDivideSize
    criFsBinder_GetDirectoryBinderFileHandlePointer
    criFsBinder_GetDpkBufferSize
    criFsBinder_GetDpkRequiredMemorySize
    criFsBinder_GetFileSize
    criFsBinder_GetFileSizeById
    criFsBinder_GetGroup
    criFsBinder_GetHandle
    criFsBinder_GetIoError
    criFsBinder_GetNumBinds
    criFsBinder_GetNumRemainBinders
    criFsBinder_GetNumUsedHandles
    criFsBinder_GetNumberOfGroupFiles
    criFsBinder_GetPriority
    criFsBinder_GetSimpleWorkSizeForBindCpk
    criFsBinder_GetStatus
    criFsBinder_GetStepsFromWorkSizeForCpkIdAccessTable
    criFsBinder_GetTopBinderId
    criFsBinder_GetTotalGroupDataSize
    criFsBinder_GetWorkSizeForBindCpk
    criFsBinder_GetWorkSizeForBindCpkById
    criFsBinder_GetWorkSizeForBindDirectory
    criFsBinder_GetWorkSizeForBindFile
    criFsBinder_GetWorkSizeForBindFiles
    criFsBinder_GetWorkSizeForCpkIdAccessTable
    criFsBinder_Initialize
    criFsBinder_IsBindedFile
    criFsBinder_IsPrimaryCpkActive
    criFsBinder_IsPrimaryCpkContents
    criFsBinder_SetCrcCheckMasterSwitch
    criFsBinder_SetCurrentDirectory
    criFsBinder_SetDefaultDirectory
    criFsBinder_SetGroup
    criFsBinder_SetNumRootBinders
    criFsBinder_SetPathSeparatorForBindFiles
    criFsBinder_SetPriority
    criFsBinder_SetUserHeapFunc
    criFsBinder_SetupCpkIdAccessTable
    criFsBinder_Unbind
    criFsBinder_UnbindAsync
    crifsbinder_LockMdl
    crifsbinder_UnlockMdl

*/

    // Used for binding
    internal static long CriFsBinder_BindCpk;
    internal static long CriFsBinder_BindFiles;
    internal static long CriFsBinder_Find;
    internal static long CriFsBinder_GetSizeForBindFiles;
    internal static long CriFsBinder_SetPriority;
    internal static long CriFsBinder_GetStatus;
    internal static long CriFsBinder_Unbind;
    
    // (Optional)
    internal static long CriFsLoader_LoadRegisteredFile;
    
/*
    Platform I/O functions:
    
    Replace <XXX> with platform
    
        Win: Windows     
        

    criFsIoXXX_Exists
    criFsIoXXX_Delete
    criFsIoXXX_Rename
    criFsIoXXX_Open
    criFsIoXXX_Close=
    <Unknown>
    criFsIoXXX_GetFileSize
    criFsIoXXX_Read
    criFsIoXXX_IsReadComplete
    <Unknown>
    criFsIoXXX_GetReadSize
    criFsIoXXX_Write
    criFsIoXXX_IsWriteComplete
    <Unknown>
    criFsIoXXX_GetWriteSize
    criFsIoXXX_Flush
    criFsIoXXX_Resize
    criFsIoXXX_GetNativeFileHandle
*/

    // Used for redirecting files out of game folder and fixing case sensitivity.
    internal static long CriFsIo_Open;
    internal static long CriFsIo_Exists;
    internal static unsafe int* CriFsIo_IsUtf8;
    
    

    public static unsafe void Init(SigScanHelper helper, nint baseAddr)
    {
        // TODO: Make a set of signatures for every known version.
        // Then scan them all and pick set of signatures with most matches.
        // Officially, this is postponed till Rust port, which will also bring Switch support, but PRs welcome.
        
        if (nint.Size == 8)
        {
            // For ~2019 version
            helper.FindPatternOffset("48 89 5C 24 18 48 89 74 24 20 55 57 41 54 41 56 41 57 48 8D 6C 24 C9 48 81 EC A0",
                offset => CriFs_CalculateWorkSizeForLibrary = baseAddr + offset,
                "CRI Binder Calculate Work Size for Library");
            
            helper.FindPatternOffset(
                "4C 8B DC 49 89 5B 08 49 89 6B 10 49 89 7B 18 41 56 48 83 EC 60 48 8D 05 2C 89 46 01 41 8B E8 48 89 05 2A F8 49 02",
                offset => CriFs_InitializeLibrary = baseAddr + offset, "CRI Initialize FS Library");

            helper.FindPatternOffset("48 83 EC 28 83 3D ?? ?? ?? ?? ?? 75 16",
                offset => CriFs_FinalizeLibrary = baseAddr + offset, "CRI Initialize FS Library");
            
            helper.FindPatternOffset("48 83 EC 48 48 8B 44 24 78 C7 44 24 30 01 00 00 00 48 89 44 24 28 8B",
                offset => CriFsBinder_BindCpk = baseAddr + offset, "CRI Binder Bind CPK");

            helper.FindPatternOffset(
                "48 83 EC 48 48 8B 44 24 78 48 89 44 24 30 8B 44 24 70 89 44 24 28 4C 89 4C 24 20 41 83",
                offset => CriFsBinder_BindFiles = baseAddr + offset, "CRI Binder Bind Files");

            helper.FindPatternOffset("48 89 5C 24 08 48 89 74 24 20 57 48 81 EC 50",
                offset => CriFsBinder_GetSizeForBindFiles = baseAddr + offset, "CRI Binder Get Size for Bind Files");
            
            helper.FindPatternOffset("48 89 5C 24 08 57 48 83 EC 20 48 8B DA 8B F9 85",
                offset => CriFsBinder_GetStatus = baseAddr + offset, "CRI Binder Get Status");
            
            helper.FindPatternOffset("48 89 5C 24 08 57 48 83 EC 20 8B FA E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 75 18",
                offset => CriFsBinder_SetPriority = baseAddr + offset, "CRI Binder Set Priority");
            
            helper.FindPatternOffset("48 89 5C 24 08 57 48 83 EC 20 8B F9 E8 ?? ?? ?? ?? 48 8B",
                offset => CriFsBinder_Unbind = baseAddr + offset, "CRI Binder Unbind");
            
            helper.FindPatternOffset("48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 48 83 EC 40 49 8B F9 49 8B D8 48",
                offset => CriFsBinder_Find = baseAddr + offset, "CRI FS Binder: Find");
            
            // Capitalization Fixing
            helper.FindPatternOffset("48 89 5C 24 18 57 48 81 EC 70 08",
                offset => CriFsIo_Exists = baseAddr + offset, "CRI FS IO Exists");
            
            helper.FindPatternOffset("48 8B C4 48 89 58 10 48 89 68 18 48 89 70 20 57 41 54 41 55 41 56 41 57 48 83 EC 50",
                offset => CriFsIo_Open = baseAddr + offset, "CRI FS IO Open");
            
            helper.FindPatternOffset("83 3D ?? ?? ?? ?? ?? 74 38 E8 ?? ?? ?? ?? 48 8D 4C 24 30 C7 44 24 28 11 04 00 00 48 89 4C 24 20 4C 8B C7",
                offset =>
                {
                    // Extract from cmp instruction, i.e. cmp [14028DC48], 0
                    var rip = baseAddr + offset;
                    var cmpOffset = *(int*)(rip + 2);
                    CriFsIo_IsUtf8 = (int*)((nint)rip + 7 + cmpOffset); // 7 = instruction length
                }, "CRI FS IO Is UTF8");
            
            // Optional, used for printing loaded files.
            helper.FindPatternOffset("48 89 5C 24 10 4C 89 4C 24 20 55 56 57 41 54 41 55 41 56 41 57 48 81",
                offset => CriFsLoader_LoadRegisteredFile = baseAddr + offset, "CRI FS Loader: Load Registered File");
        }
        else if (nint.Size == 4)
        {
            // For ~2014 version
            helper.FindPatternOffset("55 8B EC 83 EC 68 A1 ?? ?? ?? ?? 33 C5 89 45 FC 8B 45 0C 53 8B",
                offset => CriFs_CalculateWorkSizeForLibrary = baseAddr + offset,
                "CRI Binder Calculate Work Size for Library x86");

            helper.FindPatternOffset("55 8B EC 83 EC 38 A1 ?? ?? ?? ?? 33 C5 89 45 FC 53 8B 5D 0C",
                offset => CriFs_InitializeLibrary = baseAddr + offset, "CRI Initialize FS Library x86");

            helper.FindPatternOffset("56 33 F6 39 35 ?? ?? ?? ?? 75 12",
                offset => CriFs_FinalizeLibrary = baseAddr + offset, "CRI Finalize FS Library x86");

            helper.FindPatternOffset("55 8B EC 6A 01 FF 75 1C",
                offset => CriFsBinder_BindCpk = baseAddr + offset, "CRI Binder Bind CPK x86");

            helper.FindPatternOffset("55 8B EC FF 75 1C 8B 55",
                offset => CriFsBinder_BindFiles = baseAddr + offset, "CRI Binder Bind Files x86");

            helper.FindPatternOffset("55 8B EC 81 EC 14 02 00 00 A1 ?? ?? ?? ?? 33 C5 89 45 FC 53",
                offset => CriFsBinder_GetSizeForBindFiles = baseAddr + offset,
                "CRI Binder Get Size for Bind Files x86");
            
            helper.FindPatternOffset("55 8B EC 56 8B 75 08 57 85 F6 74 35",
                offset => CriFsBinder_GetStatus = baseAddr + offset, "CRI Binder Get Status x86");
            
            helper.FindPatternOffset("55 8B EC 56 FF 75 08 E8 ?? ?? ?? ?? 8B F0 33",
                offset => CriFsBinder_SetPriority = baseAddr + offset, "CRI Binder Set Priority x86");
            
            helper.FindPatternOffset(
                "55 8B EC 56 FF 75 08 E8 ?? ?? ?? ?? 8B F0 59 85 F6 75 13 68 ?? ?? ?? ?? 6A 01 E8 ?? ?? ?? ?? 59 59 6A FE 58 EB 36",
                offset => CriFsBinder_Unbind = baseAddr + offset, "CRI Binder Unbind x86");
            
            // Optional
            
            
        }
    }
}