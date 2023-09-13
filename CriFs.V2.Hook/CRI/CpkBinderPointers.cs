using CriFs.V2.Hook.Utilities;
using FileEmulationFramework.Lib.Utilities;
using static CriFs.V2.Hook.CRI.CRI;

// ReSharper disable InconsistentNaming

namespace CriFs.V2.Hook.CRI;

internal static class CpkBinderPointers
{
    // Additional info
    internal static CriPointers Pointers;

    public static unsafe void Init(SigScanHelper helper, nint baseAddr, Logger logger)
    {
        // Note: The pattern scanner is cached, so searching the same pattern multiple times will result
        //       in the cached value being pulled. This is why duplicating sigs between different CRI versions is fine.

        CriPointerScanInfo[] possibilities = default!;

        if (IntPtr.Size == 8)
        {
            // 64-bit x64 games
            // For more details on individual pointers, see CriPointers struct itself
            // Note: Please sort this list by build date.
            possibilities = new CriPointerScanInfo[]
            {
                new()
                {
                    SourcedFrom = "Yakuza Kiwami",
                    CriVersion = "CRI File System/PCx64 Ver.2.71.02 Build:Oct  6 2015 19:45:41",
                    CriCompiler = "MSC17.00.61030.0,MT",
                    Patterns = new CriPointerPatterns
                    {
                        CriFs_CalculateWorkSizeForLibrary =
                            "48 89 5C 24 18 48 89 74 24 20 55 57 41 54 41 56 41 57 48 8D 6C 24 C9 48 81 EC A0",
                        CriFs_InitializeLibrary = "48 89 5C 24 18 48 89 7C 24 20 55 41",
                        CriFs_FinalizeLibrary = "48 83 EC 28 83 3D ?? ?? ?? ?? ?? 75 16",
                        CriFsBinder_BindCpk = "48 83 EC 48 48 8B 44 24 78 C7 44 24 30 01 00 00 00 48 89 44 24 28 8B",
                        CriFsBinder_BindFile =
                            "48 83 EC 48 48 8B 44 24 78 48 89 44 24 30 8B 44 24 70 89 44 24 28 4C 89 4C 24 20 41", // BindFile
                        CriFsBinder_BindFiles = "", // Not Present
                        CriFsBinder_Find =
                            "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 48 83 EC 40 49 8B F9 49 8B D8 48",
                        CriFsBinder_GetSizeForBindFiles = "48 89 5C 24 08 48 89 74 24 20 57 48 81 EC 50",
                        CriFsBinder_GetStatus = "48 89 5C 24 08 57 48 83 EC 20 48 8B DA 8B F9 85",
                        CriFsBinder_SetPriority = "",
                        CriFsBinder_Unbind = "48 89 5C 24 08 57 48 83 EC 20 8B F9 E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 75",
                        CriFsLoader_RegisterFile =
                            "48 8B C4 48 89 58 10 48 89 70 18 48 89 78 20 55 41 54 41 55 41 56 41 57 48 8D 68 A9",
                        CriFsIo_Exists = "48 89 5C 24 18 57 48 81 EC 60 04",
                        CriFsIo_Open =
                            "48 8B C4 48 89 58 10 48 89 68 18 48 89 70 20 57 41 54 41 55 41 56 41 57 48 83 EC 50",
                        CriFsIo_IsUtf8 = "83 3D ?? ?? ?? ?? ?? 74 38",
                        DisableFileBindWarning = "",
                        DisableGetContentsInfoDetailsWarning = "" // not supported
                    }
                },
                new()
                {
                    SourcedFrom = "One Piece Burning Blood",
                    CriVersion = "CRI File System/PCx64 Ver.2.70.00 Build:Oct  8 2015 13:15:23",
                    CriCompiler = "MSC16.00.40219.1,MT",
                    Patterns = new CriPointerPatterns
                    {
                        CriFs_CalculateWorkSizeForLibrary =
                            "48 89 5C 24 18 48 89 74 24 20 55 57 41 54 41 55 41 56 48 8D",
                        CriFs_InitializeLibrary = "48 89 5C 24 18 48 89 74 24 20 55 41",
                        CriFs_FinalizeLibrary = "48 83 EC 28 83 3D ?? ?? ?? ?? ?? 75 16",
                        CriFsBinder_BindCpk =
                            "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 41 54 41 55 41 56 41 57 48 83 EC 70",
                        CriFsBinder_BindFile = "48 83 EC 48 48 8B 44 24 78 48",
                        CriFsBinder_BindFiles = "", // Missing
                        CriFsBinder_Find =
                            "48 8B C4 48 89 58 08 48 89 68 10 48 89 70 18 48 89 78 20 41 54 48 83 EC 40 45",
                        CriFsBinder_GetSizeForBindFiles = "48 89 5C 24 08 48 89 74 24 20 57 48 81",
                        CriFsBinder_GetStatus = "48 89 5C 24 08 57 48 83 EC 20 48 8B DA 8B F9 85",
                        CriFsBinder_SetPriority = "",
                        CriFsBinder_Unbind = "48 89 5C 24 08 57 48 83 EC 20 8B F9 E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 75",
                        CriFsIo_Exists = "48 89 5C 24 18 57 48 81 EC 60",
                        CriFsIo_Open = "48 8B C4 48 89 58 10 48 89 68 18 48 89 70 20 57 41 54 41 55 41 56 41 57 48 83",
                        CriFsIo_IsUtf8 = "83 3D ?? ?? ?? ?? ?? 74 38",
                        DisableFileBindWarning = "",
                        CriFsLoader_RegisterFile =
                            "48 8B C4 48 89 58 10 48 89 70 18 48 89 78 20 55 41 54 41 55 41 56 41 57 48 8D 68 A9",
                        DisableGetContentsInfoDetailsWarning = "" // not supported
                    }
                },
                new()
                {
                    SourcedFrom = "NieR Automata (YoRHa Edition)",
                    CriVersion = "CRI File System/PCx64 Ver.2.73.00 Build:Jun 20 2016 18:17:30",
                    CriCompiler = "MSC17.00.61030.0,MT",
                    Patterns = new CriPointerPatterns
                    {
                        CriFs_CalculateWorkSizeForLibrary =
                            "40 55 53 56 57 41 54 41 56 41 57 48 8D 6C 24 D9 48 81 EC 90 00 00 00 48 8B",
                        CriFs_InitializeLibrary = "48 89 5C 24 08 48 89 74 24 10 55 57 41 56 48 8B EC 48 83 EC 50",
                        CriFs_FinalizeLibrary = "48 83 EC 28 83 3D ?? ?? ?? ?? ?? 75 16",
                        CriFsBinder_BindCpk = "48 83 EC 48 48 8B 44 24 78 C7 44 24 30 01 00 00 00 48 89 44 24 28 8B",
                        CriFsBinder_BindFile = "", // Missing
                        CriFsBinder_BindFiles = "", // Missing
                        CriFsBinder_Find =
                            "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 48 83 EC 40 49 8B F9 49 8B D8 48",
                        CriFsBinder_GetSizeForBindFiles = "", // Missing
                        CriFsBinder_GetStatus = "48 89 5C 24 08 57 48 83 EC 20 48 8B DA 8B F9 85",
                        CriFsBinder_SetPriority = "", // Missing
                        CriFsBinder_Unbind = "48 89 5C 24 08 57 48 83 EC 20 8B F9 E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 75",
                        CriFsIo_Exists = "48 89 5C 24 08 57 48 81 EC 50 04",
                        CriFsIo_Open =
                            "48 8B C4 48 89 58 10 48 89 68 18 48 89 70 20 57 41 54 41 55 41 56 41 57 48 83 EC 50",
                        CriFsIo_IsUtf8 =
                            "83 3D ?? ?? ?? ?? ?? 74 38 E8 ?? ?? ?? ?? 48 8D 4C 24 30 C7 44 24 28 09 02 00 00 48 89 4C 24 20 44 8D 48 01 4C 8B C7",
                        DisableFileBindWarning = "",
                        CriFsLoader_RegisterFile =
                            "48 8B C4 48 89 58 10 48 89 70 18 48 89 78 20 55 41 54 41 55 41 56 41 57 48 8D 68 A9",
                        DisableGetContentsInfoDetailsWarning = "" // not supported
                    }
                },
                new()
                {
                    SourcedFrom = "Tekken 7",
                    CriVersion = "CRI File System/PCx64 Ver.2.73.00 Build:Jul 27 2017 11:01:21",
                    CriCompiler = "MSC17.00.61030.0,MT",
                    Patterns = new CriPointerPatterns
                    {
                        CriFs_CalculateWorkSizeForLibrary =
                            "40 55 53 56 57 41 54 41 56 41 57 48 8D 6C 24 D9 48 81 EC 90 00 00 00 48 8B F2",
                        CriFs_InitializeLibrary = "48 89 5C 24 08 48 89 74 24 10 55 57 41 56 48 8B EC 48 83 EC 50",
                        CriFs_FinalizeLibrary = "48 83 EC 28 83 3D ?? ?? ?? ?? ?? 75 16",
                        CriFsBinder_BindCpk = "48 83 EC 48 48 8B 44 24 78 C7",
                        CriFsBinder_BindFile =
                            "48 83 EC 48 48 8B 44 24 78 48 89 44 24 30 8B 44 24 70 89 44 24 28 4C 89 4C 24 20 41 B9",
                        CriFsBinder_BindFiles =
                            "48 83 EC 48 48 8B 44 24 78 48 89 44 24 30 8B 44 24 70 89 44 24 28 4C 89 4C 24 20 41 83",
                        CriFsBinder_Find =
                            "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 48 83 EC 40 49 8B F9 49 8B D8 48",
                        CriFsBinder_GetSizeForBindFiles = "48 8B C4 48 89 58 08 48 89 70 18 57 48 81 EC 30",
                        CriFsBinder_GetStatus = "48 89 5C 24 08 57 48 83 EC 20 48 8B DA 8B F9 85",
                        CriFsBinder_SetPriority =
                            "48 89 5C 24 08 57 48 83 EC 20 8B FA E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 75 18 8D 58 FE 48 8D 15 ?? ?? ?? ?? 33 C9 44 8B C3 E8 ?? ?? ?? ?? 8B C3 EB 3E",
                        CriFsBinder_Unbind = "48 89 5C 24 08 57 48 83 EC 20 8B F9 E8 ?? ?? ?? ?? 48 8B D8",
                        CriFsLoader_RegisterFile =
                            "48 8B C4 48 89 58 10 48 89 70 18 48 89 78 20 55 41 54 41 55 41 56 41 57 48 8D 68 A9 48 81 EC 90",
                        CriFsIo_Exists = "48 89 5C 24 08 57 48 81 EC 50 04",
                        CriFsIo_Open =
                            "48 8B C4 48 89 58 10 48 89 68 18 48 89 70 20 57 41 54 41 55 41 56 41 57 48 83 EC 50",
                        CriFsIo_IsUtf8 =
                            "83 3D ?? ?? ?? ?? ?? 74 38 E8 ?? ?? ?? ?? 48 8D 4C 24 30 C7 44 24 28 09 02 00 00 48 89 4C 24 20 44 8D 48 01 4C 8B C7",
                        DisableFileBindWarning = "",
                        DisableGetContentsInfoDetailsWarning = "" // not supported
                    }
                },
                new()
                {
                    SourcedFrom = "Sonic Forces",
                    CriVersion = "CRI File System/PCx64 Ver.2.75.05 Build:Oct  6 2017 14:17:55",
                    CriCompiler = "MSC17.00.61030.0,MT",
                    Patterns = new CriPointerPatterns
                    {
                        CriFs_CalculateWorkSizeForLibrary =
                            "40 55 53 56 57 41 54 41 56 41 57 48 8D 6C 24 D9 48 81 EC 90",
                        CriFs_InitializeLibrary = "48 89 5C 24 08 48 89 74 24 10 55 57 41 56 48 8B EC 48 83 EC 50 48",
                        CriFs_FinalizeLibrary = "48 83 EC 28 83 3D ?? ?? ?? ?? ?? 75 16",
                        CriFsBinder_BindCpk = "48 83 EC 48 48 8B 44 24 78 C7 44 24 30 01 00 00 00 48 89 44 24 28 8B",
                        CriFsBinder_BindFile =
                            "48 83 EC 48 48 8B 44 24 78 48 89 44 24 30 8B 44 24 70 89 44 24 28 4C 89 4C 24 20 41 B9",
                        CriFsBinder_BindFiles =
                            "48 83 EC 48 48 8B 44 24 78 48 89 44 24 30 8B 44 24 70 89 44 24 28 4C 89 4C 24 20 41 83",
                        CriFsBinder_Find =
                            "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 48 83 EC 40 49 8B F9 49 8B D8 48",
                        CriFsBinder_GetSizeForBindFiles = "48 8B C4 48 89 58 08 48 89 70 18 57 48 81 EC 30",
                        CriFsBinder_GetStatus = "48 89 5C 24 08 57 48 83 EC 20 48 8B DA 8B F9 85",
                        CriFsBinder_SetPriority =
                            "48 89 5C 24 08 57 48 83 EC 20 8B FA E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 75 18",
                        CriFsBinder_Unbind = "48 89 5C 24 08 57 48 83 EC 20 8B F9 E8 ?? ?? ?? ?? 48 8B",
                        CriFsLoader_RegisterFile =
                            "48 8B C4 48 89 58 10 48 89 70 18 48 89 78 20 55 41 54 41 55 41 56 41 57 48 8D 68 A9",
                        CriFsIo_Exists = "48 89 5C 24 08 57 48 81 EC 50 04",
                        CriFsIo_Open =
                            "48 8B C4 48 89 58 10 48 89 68 18 48 89 70 20 57 41 54 41 55 41 56 41 57 48 83 EC 50",
                        CriFsIo_IsUtf8 = "83 3D ?? ?? ?? ?? ?? 74 38",
                        DisableFileBindWarning = "",
                        DisableGetContentsInfoDetailsWarning = "" // not supported
                    }
                },
                new()
                {
                    SourcedFrom = "Yakuza Kiwami 2",
                    CriVersion = "CRI File System/PCx64 Ver.2.75.05 Build:Oct  6 2017 14:17:55",
                    CriCompiler = "MSC17.00.61030.0,MT",
                    Patterns = new CriPointerPatterns
                    {
                        CriFs_CalculateWorkSizeForLibrary =
                            "40 55 53 56 57 41 54 41 56 41 57 48 8D 6C 24 D9 48 81 EC 90 00 00 00 48 8B",
                        CriFs_InitializeLibrary = "48 89 5C 24 08 48 89 74 24 10 55 57 41 56 48 8B EC 48 83 EC 50",
                        CriFs_FinalizeLibrary = "48 83 EC 28 83 3D ?? ?? ?? ?? ?? 75 16",
                        CriFsBinder_BindCpk = "48 83 EC 48 48 8B 44 24 78 C7 44 24 30 01 00 00 00 48 89 44 24 28 8B",
                        CriFsBinder_BindFile =
                            "48 83 EC 48 48 8B 44 24 78 48 89 44 24 30 8B 44 24 70 89 44 24 28 4C 89 4C 24 20 41 B9",
                        CriFsBinder_BindFiles =
                            "48 83 EC 48 48 8B 44 24 78 48 89 44 24 30 8B 44 24 70 89 44 24 28 4C 89 4C 24 20 41 83",
                        CriFsBinder_Find =
                            "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 48 83 EC 40 49 8B F9 49 8B D8 48",
                        CriFsBinder_GetSizeForBindFiles = "48 8B C4 48 89 58 08 48 89 70 18 57 48 81 EC 30",
                        CriFsBinder_GetStatus = "48 89 5C 24 08 57 48 83 EC 20 48 8B DA 8B F9 85",
                        CriFsBinder_SetPriority =
                            "48 89 5C 24 08 57 48 83 EC 20 8B FA E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 75 18 8D 58 FE 48 8D 15 ?? ?? ?? ?? 33 C9 44 8B C3 E8 ?? ?? ?? ?? 8B C3 EB 3E",
                        CriFsBinder_Unbind = "48 89 5C 24 08 57 48 83 EC 20 8B F9 E8 ?? ?? ?? ?? 48 8B D8",
                        CriFsLoader_RegisterFile =
                            "48 8B C4 48 89 58 10 48 89 70 18 48 89 78 20 55 41 54 41 55 41 56 41 57 48 8D 68 A9",
                        CriFsIo_Exists = "48 89 5C 24 08 57 48 81 EC 50 04",
                        CriFsIo_Open =
                            "48 8B C4 48 89 58 10 48 89 68 18 48 89 70 20 57 41 54 41 55 41 56 41 57 48 83 EC 50 4D",
                        CriFsIo_IsUtf8 =
                            "83 3D ?? ?? ?? ?? ?? 74 38 E8 ?? ?? ?? ?? 48 8D 4C 24 30 C7 44 24 28 09 02 00 00 48 89 4C 24 20 44 8D 48 01 4C 8B C7",
                        DisableFileBindWarning = "",
                        DisableGetContentsInfoDetailsWarning = "" // not supported
                    }
                },
                new()
                {
                    SourcedFrom = "Persona 5 Royal",
                    CriVersion = "CRI File System/PCx64 Ver.2.81.6 Build:Dec 28 2021 11:03:45",
                    CriCompiler = "MSC19.00.24210.0,MT",
                    Patterns = new CriPointerPatterns
                    {
                        CriFs_CalculateWorkSizeForLibrary =
                            "48 89 5C 24 18 48 89 74 24 20 55 57 41 54 41 56 41 57 48 8D 6C 24 C9 48 81 EC A0",
                        CriFs_InitializeLibrary =
                            "4C 8B DC 49 89 5B 08 49 89 6B 10 49 89 7B 18 41 56 48 83 EC 60 48 8D 05 ?? ?? ?? ?? 41 8B E8 48 89 05 ?? ?? ?? ??",
                        CriFs_FinalizeLibrary = "48 83 EC 28 83 3D ?? ?? ?? ?? ?? 75 16",
                        CriFsBinder_BindCpk = "48 83 EC 48 48 8B 44 24 78 C7 44 24 30 01 00 00 00 48 89 44 24 28 8B",
                        CriFsBinder_BindFile =
                            "48 83 EC 48 48 8B 44 24 78 48 89 44 24 30 8B 44 24 70 89 44 24 28 4C 89 4C 24 20 41 B9",
                        CriFsBinder_BindFiles =
                            "48 83 EC 48 48 8B 44 24 78 48 89 44 24 30 8B 44 24 70 89 44 24 28 4C 89 4C 24 20 41 83",
                        CriFsBinder_Find =
                            "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 48 83 EC 40 49 8B F9 49 8B D8 48",
                        CriFsBinder_GetSizeForBindFiles = "48 89 5C 24 08 48 89 74 24 20 57 48 81 EC 50",
                        CriFsBinder_GetStatus = "48 89 5C 24 08 57 48 83 EC 20 48 8B DA 8B F9 85",
                        CriFsBinder_SetPriority =
                            "48 89 5C 24 08 57 48 83 EC 20 8B FA E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 75 18",
                        CriFsBinder_Unbind = "48 89 5C 24 08 57 48 83 EC 20 8B F9 E8 ?? ?? ?? ?? 48 8B D8",

                        CriFsLoader_RegisterFile = "48 8B C4 48 89 58 08 48 89 70 10 4C",

                        CriFsIo_Exists = "48 89 5C 24 18 57 48 81 EC 70 08",
                        CriFsIo_Open =
                            "48 8B C4 48 89 58 10 48 89 68 18 48 89 70 20 57 41 54 41 55 41 56 41 57 48 83 EC 50",
                        CriFsIo_IsUtf8 =
                            "83 3D ?? ?? ?? ?? ?? 74 38 E8 ?? ?? ?? ?? 48 8D 4C 24 30 C7 44 24 28 11 04 00 00 48 89 4C 24 20 4C 8B C7",
                        DisableFileBindWarning = "E8 ?? ?? ?? ?? 83 C8 FF EB 45 48 8B 0D",
                        DisableGetContentsInfoDetailsWarning = "E8 ?? ?? ?? ?? 83 C8 FF 4C 8D 5C 24 60 49 8B 5B 10 49 8B 6B 18 49 8B 73 20 49 8B E3" // not supported
                    }
                },
                new()
                {
                    SourcedFrom = "Persona 3 Portable / Persona 4 The Golden",
                    CriVersion = "CRI File System/PCx64 Ver.2.82.15 Build:May 12 2022 19:34:26",
                    CriCompiler = "MSC19.16.27045.0,MT",
                    Patterns = new CriPointerPatterns
                    {
                        CriFs_CalculateWorkSizeForLibrary =
                            "48 89 5C 24 18 48 89 74 24 20 55 57 41 54 41 56 41 57 48 8D 6C 24 C9 48 81 EC A0",
                        CriFs_InitializeLibrary = "4C 8B DC 49 89 5B 08 49 89 6B 10 49 89 7B",
                        CriFs_FinalizeLibrary = "48 83 EC 28 83 3D ?? ?? ?? ?? ?? 75 16",
                        CriFsBinder_BindCpk = "48 83 EC 48 48 8B 44 24 78 C7 44 24 30 01 00 00 00 48 89 44 24 28 8B",
                        CriFsBinder_BindFile =
                            "48 83 EC 48 48 8B 44 24 78 48 89 44 24 30 8B 44 24 70 89 44 24 28 4C 89 4C 24 20 41 B9",
                        CriFsBinder_BindFiles =
                            "48 83 EC 48 48 8B 44 24 78 48 89 44 24 30 8B 44 24 70 89 44 24 28 4C 89 4C 24 20 41 83",
                        CriFsBinder_Find =
                            "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 48 83 EC 40 49 8B F9 49 8B D8 48",
                        CriFsBinder_GetSizeForBindFiles = "48 89 5C 24 08 48 89 74 24 20 57 48 81 EC 50",
                        CriFsBinder_GetStatus = "48 89 5C 24 08 57 48 83 EC 20 48 8B DA 8B F9 85",
                        CriFsBinder_SetPriority =
                            "48 89 5C 24 08 57 48 83 EC 20 8B FA E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 75 18",
                        CriFsBinder_Unbind = "48 89 5C 24 08 57 48 83 EC 20 8B F9 E8 ?? ?? ?? ?? 48 8B",

                        CriFsLoader_RegisterFile = "48 8B C4 48 89 58 08 48 89 70 10 4C",

                        CriFsIo_Exists = "48 89 5C 24 18 57 48 81 EC 70 08",
                        CriFsIo_Open =
                            "48 8B C4 48 89 58 10 48 89 68 18 48 89 70 20 57 41 54 41 55 41 56 41 57 48 83 EC 50",
                        CriFsIo_IsUtf8 =
                            "83 3D ?? ?? ?? ?? ?? 74 38 E8 ?? ?? ?? ?? 48 8D 4C 24 30 C7 44 24 28 11 04 00 00 48 89 4C 24 20 4C 8B C7",
                        DisableFileBindWarning = "E8 ?? ?? ?? ?? 83 C8 FF EB 45 48 8B 0D",
                        DisableGetContentsInfoDetailsWarning = "" // not supported
                    }
                }
            };
        }
        else if (IntPtr.Size == 4)
        {
            // 32-bit x86 games
            // For more details on individual pointers, see CriPointers struct itself
            // Note: Please sort this list by build date.
            possibilities = new CriPointerScanInfo[]
            {
                new()
                {
                    SourcedFrom = "Sonic 4 Episode 1 (Not Supported, Missing APIs)",
                    CriVersion = "CRI File System/PCx86 Ver.2.24.04 Build:Apr  1 2011 21:08:31",
                    CriCompiler = "MSC1400,MT",
                    Patterns = new CriPointerPatterns
                    {
                        CriFs_CalculateWorkSizeForLibrary =
                            "83 3D ?? ?? ?? ?? ?? 75 12 68 ?? ?? ?? ?? 6A 00 E8 ?? ?? ?? ?? 59 59 83 C8 FF C3 E8 ?? ?? ?? ?? E8 ?? ?? ?? ?? E8",
                        CriFs_InitializeLibrary = "55 8B EC 83 EC 2C 56",
                        CriFs_FinalizeLibrary =
                            "83 3D ?? ?? ?? ?? ?? 75 12 68 ?? ?? ?? ?? 6A 00 E8 ?? ?? ?? ?? 59 59 83 C8 FF C3 E8 ?? ?? ?? ?? E8 ?? ?? ?? ?? E8",
                        CriFsBinder_BindCpk =
                            "", // bindCpkSub exists but not BindCpk. Because an under the hood method is used.
                        CriFsBinder_BindFile = "",
                        CriFsBinder_BindFiles = "",
                        CriFsBinder_Find =
                            "55 8B EC 56 8B 75 14 85 F6 74 03 83 26 00 83 7D 10 00 74 09 FF 75 10 E8 ?? ?? ?? ?? 59 E8 ?? ?? ?? ?? 85 C0 74 05 83 C8 FF EB 16",
                        CriFsBinder_GetSizeForBindFiles = "", // missing
                        CriFsBinder_GetStatus = "", // can't find (not present?)
                        CriFsBinder_SetPriority = "", // not present
                        CriFsBinder_Unbind = "53 8B 5C 24 08 56 E8 ?? ?? ?? ?? 8B F0 85 F6 75 13",
                        CriFsLoader_RegisterFile = "55 8B EC 83 EC 4C 83",
                        CriFsIo_Exists = "83 7C 24 04 00 56",
                        CriFsIo_Open = "55 8B EC 51 53 56 57 8B 7D 08 33",
                        CriFsIo_IsUtf8 = "", // not supported
                        DisableFileBindWarning = "",
                        DisableGetContentsInfoDetailsWarning = "" // not supported
                    }
                },
                new()
                {
                    // Same as Sonic Generations
                    SourcedFrom =
                        "Sonic 4 Episode 2 (Not Supported, Missing APIs, Possible to copy/reimplement code from Generations)",
                    CriVersion = "CRI File System/PCx86 Ver.2.24.04 Build:Apr  1 2011 21:08:31",
                    CriCompiler = "MSC1500,MT",
                    Patterns = new CriPointerPatterns
                    {
                        CriFs_CalculateWorkSizeForLibrary = "55 8B EC 83 EC 4C 53",
                        CriFs_InitializeLibrary = "55 8B EC 83 EC 2C 56",
                        CriFs_FinalizeLibrary = "56 33 F6 39 35 ?? ?? ?? ?? 75 12",
                        CriFsBinder_BindCpk = "55 8B EC 6A 01 FF 75 1C",
                        CriFsBinder_BindFile = "", // needs copy from Generations
                        CriFsBinder_BindFiles = "", // needs copy from Generations
                        CriFsBinder_Find = "55 8B EC 56 8B 75 14 85 F6 74",
                        CriFsBinder_GetSizeForBindFiles = "", // needs copy from Generations
                        CriFsBinder_GetStatus = "53 8B 5C 24 08 E8 ?? ?? ?? ?? 85 C0 75",
                        CriFsBinder_SetPriority = "",
                        CriFsBinder_Unbind = "53 8B 5C 24 08 56 E8 ?? ?? ?? ?? 8B F0 85 F6 75 13",
                        CriFsLoader_RegisterFile = "55 8B EC 83 EC 4C 83",
                        CriFsIo_Exists = "83 7C 24 04 00 56 74",
                        CriFsIo_Open = "55 8B EC 83 EC 0C 53 56 57 8B 7D 08 33",
                        CriFsIo_IsUtf8 = "", // not supported
                        DisableFileBindWarning = "",
                        DisableGetContentsInfoDetailsWarning = "" // not supported
                    }
                },
                new()
                {
                    SourcedFrom = "Sonic Generations",
                    CriVersion = "CRI File System/PCx86 Ver.2.24.04 Build:Apr  1 2011 21:08:31",
                    CriCompiler = "MSC1500,MT",
                    Patterns = new CriPointerPatterns
                    {
                        CriFs_CalculateWorkSizeForLibrary = "55 8B EC 83 EC 4C 53",
                        CriFs_InitializeLibrary = "55 8B EC 83 EC 2C 56 33",
                        CriFs_FinalizeLibrary = "56 33 F6 39 35 ?? ?? ?? ?? 75 12",
                        CriFsBinder_BindCpk = "55 8B EC 6A 01 FF 75 1C",
                        CriFsBinder_BindFile = "55 8B EC FF 75 18 8B 55",
                        CriFsBinder_BindFiles = "",
                        CriFsBinder_Find = "55 8B EC 56 8B 75 14 85 F6 74",
                        CriFsBinder_GetSizeForBindFiles = "55 8B EC 81 EC 10 02",
                        CriFsBinder_GetStatus = "53 8B 5C 24 08 E8 ?? ?? ?? ?? 85 C0 75",
                        CriFsBinder_SetPriority = "53 8B 5C 24 08 56 E8 ?? ?? ?? ?? 8B F0 33",
                        CriFsBinder_Unbind = "53 8B 5C 24 08 56 E8 ?? ?? ?? ?? 8B F0 85 F6 75 13",
                        CriFsLoader_RegisterFile = "55 8B EC 83 EC 4C 83",
                        CriFsIo_Exists = "83 7C 24 04 00 56 74",
                        CriFsIo_Open = "55 8B EC 83 EC 0C 53 56 57 8B 7D 08 33",
                        CriFsIo_IsUtf8 = "", // not supported
                        DisableFileBindWarning = "",
                        DisableGetContentsInfoDetailsWarning = "" // not supported
                    }
                },
                new()
                {
                    SourcedFrom = "Sonic Lost World",
                    CriVersion = "CRI File System/PCx86 Ver.2.59.21 Build:Feb 19 2013 12:43:50",
                    CriCompiler = "MSC1600,MT",
                    Patterns = new CriPointerPatterns
                    {
                        CriFs_CalculateWorkSizeForLibrary =
                            "55 8B EC 83 EC 64 A1 ?? ?? ?? ?? 33 C5 89 45 FC 8B 45 0C 53",
                        CriFs_InitializeLibrary =
                            "55 8B EC 83 EC 34 A1 ?? ?? ?? ?? 33 C5 89 45 FC 53 8B 5D 08 56 57 8B",
                        CriFs_FinalizeLibrary = "56 33 F6 39 35 ?? ?? ?? ?? 75 12",
                        CriFsBinder_BindCpk = "55 8B EC 6A 01 FF 75 1C",
                        CriFsBinder_BindFile = "55 8B EC FF 75 1C 8B 55",
                        CriFsBinder_BindFiles = "",
                        CriFsBinder_Find = "55 8B EC 56 8B 75 14 85 F6 74",
                        CriFsBinder_GetSizeForBindFiles = "55 8B EC 81 EC 14 02 00 00 A1 ?? ?? ?? ?? 33 C5 89 45 FC 53",
                        CriFsBinder_GetStatus = "55 8B EC 56 8B 75 08 57 85 F6 74 35",
                        CriFsBinder_SetPriority = "55 8B EC 56 FF 75 08 E8 ?? ?? ?? ?? 8B F0 33",
                        CriFsBinder_Unbind = "55 8B EC 56 FF 75 08 E8 ?? ?? ?? ?? 8B F0 59 85 F6 75 13",
                        CriFsLoader_RegisterFile = "55 8B EC 83 EC 50 8D",
                        CriFsIo_Exists = "55 8B EC 83 7D 08 00 56 74 28",
                        CriFsIo_Open = "55 8B EC 83 EC 0C 53 56 57 8B 7D 08 33",
                        CriFsIo_IsUtf8 = "", // not supported
                        DisableFileBindWarning = "",
                        DisableGetContentsInfoDetailsWarning = "" // not supported
                    }
                },
                new()
                {
                    SourcedFrom = "One Piece: Unlimited World Red",
                    CriVersion = "CRI File System/PCx86 Ver.2.63.08 Build:Mar  3 2014 14:59:30",
                    CriCompiler = "MSC1600,MT",
                    Patterns = new CriPointerPatterns
                    {
                        CriFs_CalculateWorkSizeForLibrary =
                            "55 8B EC 83 EC 68 A1 ?? ?? ?? ?? 33 C5 89 45 FC 8B 45 0C 53 8B",
                        CriFs_InitializeLibrary = "55 8B EC 83 EC 38 A1 ?? ?? ?? ?? 33 C5 89 45 FC 53 8B 5D 0C",
                        CriFs_FinalizeLibrary = "56 33 F6 39 35 ?? ?? ?? ?? 75 12",
                        CriFsBinder_BindCpk = "55 8B EC 6A 01 FF 75 1C",
                        CriFsBinder_BindFile = "55 8B EC FF 75 1C 8B 55",
                        CriFsBinder_BindFiles = "",
                        CriFsBinder_Find = "55 8B EC 53 8B 5D 14 56 57",
                        CriFsBinder_GetSizeForBindFiles = "55 8B EC 81 EC 14 02 00 00 A1 ?? ?? ?? ?? 33 C5 89 45 FC 53",
                        CriFsBinder_GetStatus = "55 8B EC 56 8B 75 08 57 85 F6 74 35",
                        CriFsBinder_SetPriority = "",
                        CriFsBinder_Unbind =
                            "55 8B EC 56 FF 75 08 E8 ?? ?? ?? ?? 8B F0 59 85 F6 75 13 68 ?? ?? ?? ?? 6A 01 E8 ?? ?? ?? ?? 59 59 6A FE 58 EB 36",
                        CriFsLoader_RegisterFile = "55 8B EC 83 EC 50 8D",
                        CriFsIo_Exists = "55 8B EC 83 7D 08 00 56 74 28",
                        CriFsIo_Open = "55 8B EC 83 EC 0C 53 56 57 8B 7D 08 33",
                        CriFsIo_IsUtf8 = "", // not supported
                        DisableFileBindWarning = "",
                        DisableGetContentsInfoDetailsWarning = "" // not supported
                    }
                },
                new()
                {
                    SourcedFrom = "Bayonetta",
                    CriVersion = "CRI File System/PCx86 Ver.2.61.09 Build:Jan 27 2017 19:10:26",
                    CriCompiler = "MSC1600,MT",
                    Patterns = new CriPointerPatterns
                    {
                        CriFs_CalculateWorkSizeForLibrary = "55 8B EC 83 EC 68 A1 ?? ?? ?? ?? 33 C5 89 45 FC 8B 45 0C",
                        CriFs_InitializeLibrary = "55 8B EC 83 EC 38 A1 ?? ?? ?? ?? 33 C5 89 45 FC 53",
                        CriFs_FinalizeLibrary =
                            "56 33 F6 39 35 ?? ?? ?? ?? 75 12 68 ?? ?? ?? ?? 56 E8 ?? ?? ?? ?? 59 59 83 C8 FF 5E C3 39",
                        CriFsBinder_BindCpk = "55 8B EC 6A 01 FF 75 1C",
                        CriFsBinder_BindFile = "55 8B EC FF 75 1C 8B 55",
                        CriFsBinder_BindFiles = "",
                        CriFsBinder_Find = "55 8B EC 53 8B 5D 14 56",
                        CriFsBinder_GetSizeForBindFiles = "55 8B EC 81 EC 14 02 00 00 A1 ?? ?? ?? ?? 33 C5 89 45 FC 53",
                        CriFsBinder_GetStatus = "55 8B EC 56 8B 75 08 57 85 F6 74 35",
                        CriFsBinder_SetPriority = "",
                        CriFsBinder_Unbind =
                            "55 8B EC 56 FF 75 08 E8 ?? ?? ?? ?? 8B F0 59 85 F6 75 13 68 ?? ?? ?? ?? 6A 01 E8 ?? ?? ?? ?? 59 59 6A FE 58 EB 36",
                        CriFsIo_Exists = "55 8B EC 83 7D 08 00 56 74 28",
                        CriFsLoader_RegisterFile = "55 8B EC 83 EC 50 8D",
                        CriFsIo_Open = "55 8B EC 83 EC 0C 53 56 57 8B 7D 08 33",
                        CriFsIo_IsUtf8 = "", // not supported
                        DisableFileBindWarning = "",
                        DisableGetContentsInfoDetailsWarning = "" // not supported
                    }
                }
            };
        }

        foreach (var pos in possibilities)
        {
            if (!String.IsNullOrEmpty(pos.Patterns.CriFs_CalculateWorkSizeForLibrary))
            {
                helper.FindPatternOffsetSilent(pos.Patterns.CriFs_CalculateWorkSizeForLibrary,
                    offset => pos.Results.CriFs_CalculateWorkSizeForLibrary = baseAddr + offset);
            }

            if (!String.IsNullOrEmpty(pos.Patterns.CriFs_InitializeLibrary))
            {
                helper.FindPatternOffsetSilent(pos.Patterns.CriFs_InitializeLibrary,
                    offset => pos.Results.CriFs_InitializeLibrary = baseAddr + offset);
            }

            if (!String.IsNullOrEmpty(pos.Patterns.CriFs_FinalizeLibrary))
            {
                helper.FindPatternOffsetSilent(pos.Patterns.CriFs_FinalizeLibrary,
                    offset => pos.Results.CriFs_FinalizeLibrary = baseAddr + offset);
            }

            if (!String.IsNullOrEmpty(pos.Patterns.CriFsBinder_BindCpk))
            {
                helper.FindPatternOffsetSilent(pos.Patterns.CriFsBinder_BindCpk,
                    offset => pos.Results.CriFsBinder_BindCpk = baseAddr + offset);
            }

            if (!String.IsNullOrEmpty(pos.Patterns.CriFsBinder_BindFile))
            {
                helper.FindPatternOffsetSilent(pos.Patterns.CriFsBinder_BindFile,
                    offset => pos.Results.CriFsBinder_BindFile = baseAddr + offset);
            }

            if (!String.IsNullOrEmpty(pos.Patterns.CriFsBinder_BindFiles))
            {
                helper.FindPatternOffsetSilent(pos.Patterns.CriFsBinder_BindFiles,
                    offset => pos.Results.CriFsBinder_BindFiles = baseAddr + offset);
            }
            
            if (!String.IsNullOrEmpty(pos.Patterns.CriFsBinder_Find))
            {
                helper.FindPatternOffsetSilent(pos.Patterns.CriFsBinder_Find,
                    offset => pos.Results.CriFsBinder_Find = baseAddr + offset);
            }

            if (!String.IsNullOrEmpty(pos.Patterns.CriFsLoader_RegisterFile))
            {
                helper.FindPatternOffsetSilent(pos.Patterns.CriFsLoader_RegisterFile,
                    offset => pos.Results.CriFsLoader_RegisterFile = baseAddr + offset);
            }

            if (!String.IsNullOrEmpty(pos.Patterns.CriFsBinder_GetSizeForBindFiles))
            {
                helper.FindPatternOffsetSilent(pos.Patterns.CriFsBinder_GetSizeForBindFiles,
                    offset => pos.Results.CriFsBinder_GetSizeForBindFiles = baseAddr + offset);
            }

            if (!String.IsNullOrEmpty(pos.Patterns.CriFsBinder_GetStatus))
            {
                helper.FindPatternOffsetSilent(pos.Patterns.CriFsBinder_GetStatus,
                    offset => pos.Results.CriFsBinder_GetStatus = baseAddr + offset);
            }

            if (!String.IsNullOrEmpty(pos.Patterns.CriFsBinder_SetPriority))
            {
                helper.FindPatternOffsetSilent(pos.Patterns.CriFsBinder_SetPriority,
                    offset => pos.Results.CriFsBinder_SetPriority = baseAddr + offset);
            }

            if (!String.IsNullOrEmpty(pos.Patterns.CriFsBinder_Unbind))
            {
                helper.FindPatternOffsetSilent(pos.Patterns.CriFsBinder_Unbind,
                    offset => pos.Results.CriFsBinder_Unbind = baseAddr + offset);
            }

            if (!String.IsNullOrEmpty(pos.Patterns.CriFsIo_Exists))
            {
                helper.FindPatternOffsetSilent(pos.Patterns.CriFsIo_Exists,
                    offset => pos.Results.CriFsIo_Exists = baseAddr + offset);
            }

            if (!String.IsNullOrEmpty(pos.Patterns.CriFsIo_Open))
            {
                helper.FindPatternOffsetSilent(pos.Patterns.CriFsIo_Open,
                    offset => pos.Results.CriFsIo_Open = baseAddr + offset);
            }

            if (!String.IsNullOrEmpty(pos.Patterns.DisableFileBindWarning))
            {
                helper.FindPatternOffsetSilent(pos.Patterns.DisableFileBindWarning,
                    offset => pos.Results.DisableFileBindWarning = baseAddr + offset);
            }
            
            if (!String.IsNullOrEmpty(pos.Patterns.DisableGetContentsInfoDetailsWarning))
            {
                helper.FindPatternOffsetSilent(pos.Patterns.DisableGetContentsInfoDetailsWarning,
                    offset => pos.Results.DisableGetContentsInfoDetailsWarning = baseAddr + offset);
            }

            // Rarely used
            // Not supported in older library versions
            if (!String.IsNullOrEmpty(pos.Patterns.CriFsIo_IsUtf8))
            {
                helper.FindPatternOffsetSilent(
                    pos.Patterns.CriFsIo_IsUtf8,
                    offset =>
                    {
                        // Extract from cmp instruction, i.e. cmp [14028DC48], 0
                        if (IntPtr.Size == 8)
                        {
                            var rip = baseAddr + offset;
                            var cmpOffset = *(int*)(rip + 2);
                            pos.Results.CriFsIo_IsUtf8 = (int*)((nint)rip + 7 + cmpOffset); // 7 = instruction length
                        }
                    });
            }
        }

        // Sig Scanner returns results in order they were requested [by API contract].
        // We use this FindPattern to execute code once all other results have been gathered.
        helper.FindPatternOffsetSilent("00", _ =>
        {
            var best = possibilities.OrderByDescending(info => info.Results.GetNumFoundPatterns()).First();
            Pointers = best.Results;

            logger.Info("----- CRIFsV2Hook Analysis -----");
            logger.Info("Closest CRI Version: {0}", best.CriVersion);
            logger.Info("Compiler: {0}", best.CriCompiler);
            logger.Info("Sourced From: {0}", best.SourcedFrom);

            void PrintResult(long value, string functionName)
            {
                if (value != 0)
                    logger.Info("Signature Found: {0} at {1}", functionName, value.ToString("X"));
                else
                    logger.Warning("Signature Missing: {0}", functionName);
            }

            PrintResult(best.Results.CriFs_CalculateWorkSizeForLibrary, nameof(criFs_CalculateWorkSizeForLibrary));
            PrintResult(best.Results.CriFs_FinalizeLibrary, nameof(criFs_FinalizeLibrary));
            PrintResult(best.Results.CriFs_InitializeLibrary, nameof(criFs_InitializeLibrary));

            PrintResult(best.Results.CriFsBinder_BindCpk, nameof(criFsBinder_BindCpk));
            PrintResult(best.Results.CriFsBinder_BindFile, "criFsBinder_BindFile");
            PrintResult(best.Results.CriFsBinder_BindFiles, nameof(criFsBinder_BindFiles));
            PrintResult(best.Results.CriFsBinder_Find, nameof(criFsBinder_Find));
            PrintResult(best.Results.CriFsBinder_GetSizeForBindFiles, nameof(criFsBinder_GetWorkSizeForBindFiles));
            PrintResult(best.Results.CriFsBinder_SetPriority, nameof(criFsBinder_SetPriority));
            PrintResult(best.Results.CriFsBinder_GetStatus, nameof(criFsBinder_GetStatus));
            PrintResult(best.Results.CriFsBinder_Unbind, nameof(criFsBinder_Unbind));

            PrintResult(best.Results.CriFsLoader_RegisterFile, nameof(criFsLoader_RegisterFile));

            PrintResult(best.Results.CriFsIo_Open, nameof(criFsIo_Open));
            PrintResult(best.Results.CriFsIo_Exists, nameof(criFsIo_Exists));
            PrintResult((long)best.Results.CriFsIo_IsUtf8, "Is UTF8 Flag.");
            logger.Info("----- CRIFsV2Hook Analysis -----");
        });
    }

    /// <summary>
    ///     Contains the information for one set of inputs for pointer scanning.
    /// </summary>
    public class CriPointerScanInfo
    {
        /// <summary>
        ///     CRI version string, for example `CRI File System/PCx64 Ver.2.81.6 Build:Dec 28 2021 11:03:45`
        /// </summary>
        public required string CriVersion;

        /// <summary>
        ///     Compiler used to build CRI code. This is usually right after version string in binaries.
        ///     e.g. MSC19.00.24210.0,MT
        /// </summary>
        public required string CriCompiler;

        /// <summary>
        ///     The game where these symbols were sourced from.
        /// </summary>
        public required string SourcedFrom;

        /// <summary>
        ///     Pointer scan results.
        /// </summary>
        public CriPointers Results;

        /// <summary>
        ///     Pointer scan patterns.
        /// </summary>
        public required CriPointerPatterns Patterns;
    }

    /// <summary>
    ///     Contains pointer scan patterns, these correspond to the fields in <see cref="CriPointers" />.
    /// </summary>
    public struct CriPointerPatterns
    {
        internal required string CriFs_CalculateWorkSizeForLibrary;
        internal required string CriFs_FinalizeLibrary;
        internal required string CriFs_InitializeLibrary;

        internal required string CriFsBinder_BindCpk;
        internal required string CriFsBinder_BindFile;
        internal required string CriFsBinder_BindFiles;
        internal required string CriFsBinder_Find;
        internal required string CriFsBinder_GetSizeForBindFiles;
        internal required string CriFsBinder_SetPriority;
        internal required string CriFsBinder_GetStatus;
        internal required string CriFsBinder_Unbind;

        internal required string CriFsLoader_RegisterFile;

        internal required string CriFsIo_Open;
        internal required string CriFsIo_Exists;
        internal required string CriFsIo_IsUtf8;

        // Patch for 'The contents file not found in the binderhn' warning on newer games.
        internal required string DisableFileBindWarning;
        internal required string DisableGetContentsInfoDetailsWarning;
    }

    public struct CriPointers
    {
        // Used for Init
        internal long CriFs_CalculateWorkSizeForLibrary;
        internal long CriFs_FinalizeLibrary;
        internal long CriFs_InitializeLibrary;

        // Used for binding
        internal long CriFsBinder_BindCpk;
        internal long CriFsBinder_BindFile;
        internal long CriFsBinder_BindFiles;
        internal long CriFsBinder_Find;

        internal long CriFsBinder_GetSizeForBindFiles;
        internal long CriFsBinder_SetPriority;
        internal long CriFsBinder_GetStatus;
        internal long CriFsBinder_Unbind;

        // Used for redirecting files out of game folder and fixing case sensitivity.
        internal long CriFsLoader_RegisterFile;

        internal long CriFsIo_Open;
        internal long CriFsIo_Exists;
        internal unsafe int* CriFsIo_IsUtf8;
        internal long DisableFileBindWarning;
        internal long DisableGetContentsInfoDetailsWarning;

        public int GetNumFoundPatterns()
        {
            var found = 0;
            found += Convert.ToInt32(CriFs_CalculateWorkSizeForLibrary != 0);
            found += Convert.ToInt32(CriFs_FinalizeLibrary != 0);
            found += Convert.ToInt32(CriFs_InitializeLibrary != 0);

            found += Convert.ToInt32(CriFsBinder_BindCpk != 0);
            found += Convert.ToInt32(CriFsBinder_BindFile != 0);
            found += Convert.ToInt32(CriFsBinder_BindFiles != 0);
            found += Convert.ToInt32(CriFsBinder_Find != 0);
            found += Convert.ToInt32(CriFsBinder_GetSizeForBindFiles != 0);
            found += Convert.ToInt32(CriFsBinder_SetPriority != 0);
            found += Convert.ToInt32(CriFsBinder_GetStatus != 0);
            found += Convert.ToInt32(CriFsBinder_Unbind != 0);

            found += Convert.ToInt32(CriFsLoader_RegisterFile != 0);

            found += Convert.ToInt32(CriFsIo_Open != 0);
            found += Convert.ToInt32(CriFsIo_Exists != 0);

            // Optional
            // found += Convert.ToInt32(CriFsIo_IsUtf8 != (void*)0);

            return found;
        }
    }
}

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