using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using CriFs.V2.Hook.CRI;
using CriFs.V2.Hook.Utilities;
using CriFs.V2.Hook.Utilities.Extensions;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory;
using Reloaded.Memory.Interfaces;
using Reloaded.Memory.Sigscan.Definitions;
using Reloaded.Memory.Structs;
using static CriFs.V2.Hook.CRI.CpkBinderPointers;
using static CriFs.V2.Hook.CRI.CRI;
using static CriFs.V2.Hook.CRI.CRI.CriFsBinderStatus;
using Native = CriFs.V2.Hook.Utilities.Native;

namespace CriFs.V2.Hook.Hooks;

/// <summary>
///     Class to bind our custom CPKs via hooking.
/// </summary>
public static unsafe class CpkBinder
{
    private static Logger _logger = null!;

    private static IHook<criFsBinder_Find>? _findFileHook;
    private static IHook<criFs_InitializeLibrary>? _initializeLibraryHook;
    private static IHook<criFs_FinalizeLibrary>? _finalizeLibraryHook;
    private static IHook<criFsBinder_BindCpk>? _bindCpkHook;
    private static IHook<criFsIo_Exists>? _ioExistsHook;
    private static IHook<criFsIo_Open>? _ioOpenHook;

    private static criFs_CalculateWorkSizeForLibrary? _getWorkSizeForLibraryFn;
    private static criFsBinder_BindFiles? _bindFilesFn;
    private static criFsBinder_GetWorkSizeForBindFiles? _getSizeForBindFilesFn;
    private static criFsBinder_GetStatus? _getStatusFn;
    private static criFsBinder_SetPriority? _setPriorityFn;
    private static criFsBinder_Unbind? _unbindFn;
    private static SpanOfCharDict<string> _content = new(0);
    private static int _contentLength;

    private static readonly HashSet<IntPtr> BinderHandles = new(16); // 16 is default for max handle count.
    private static readonly List<CpkBinding> Bindings = new();
    private static int _additionalFiles;
    private static MemoryAllocation _libraryMemory;
    private static MemoryAllocatorWithLinkedListBackup _allocator;
    private static bool _printFileAccess;
    private static bool _printFileRedirects;

    /// <remarks>
    /// This should be called after <see cref="CpkBinderPointers"/> is initialized.
    /// </remarks>
    public static void Init(Logger logger, IReloadedHooks hooks, IScannerFactory scannerFactory)
    {
        _logger = logger;
        if (!AssertWillFunction())
            return;

        _findFileHook =
            hooks.CreateHook<criFsBinder_Find>(CriFsBinderFindImpl, Pointers.CriFsBinder_Find).Activate();
        _initializeLibraryHook =
            hooks.CreateHook<criFs_InitializeLibrary>(InitializeLibraryImpl, Pointers.CriFs_InitializeLibrary).Activate();
        _bindCpkHook = hooks.CreateHook<criFsBinder_BindCpk>(BindCpkImpl, Pointers.CriFsBinder_BindCpk).Activate();
        _ioExistsHook = hooks.CreateHook<criFsIo_Exists>(ExistsImpl, Pointers.CriFsIo_Exists).Activate();
        _ioOpenHook = hooks.CreateHook<criFsIo_Open>(CriFsOpenImpl, Pointers.CriFsIo_Open).Activate();

        _bindFilesFn = hooks.CreateWrapper<criFsBinder_BindFiles>(Pointers.CriFsBinder_BindFiles, out _);
        _getSizeForBindFilesFn =
            hooks.CreateWrapper<criFsBinder_GetWorkSizeForBindFiles>(Pointers.CriFsBinder_GetSizeForBindFiles, out _);
        _getStatusFn = hooks.CreateWrapper<criFsBinder_GetStatus>(Pointers.CriFsBinder_GetStatus, out _);
        _unbindFn = hooks.CreateWrapper<criFsBinder_Unbind>(Pointers.CriFsBinder_Unbind, out _);
        _getWorkSizeForLibraryFn =
            hooks.CreateWrapper<criFs_CalculateWorkSizeForLibrary>(Pointers.CriFs_CalculateWorkSizeForLibrary, out _);

        if (Pointers.CriFs_FinalizeLibrary != 0)
            _finalizeLibraryHook = hooks.CreateHook<criFs_FinalizeLibrary>(FinalizeLibraryImpl, Pointers.CriFs_FinalizeLibrary)
                .Activate();

        if (Pointers.CriFsBinder_SetPriority != 0)
            _setPriorityFn = hooks.CreateWrapper<criFsBinder_SetPriority>(Pointers.CriFsBinder_SetPriority, out _);
        
        PatchBindFileIntoBindFiles(scannerFactory);
    }

    private static void PatchBindFileIntoBindFiles(IScannerFactory scannerFactory)
    {
        var bindFiles = Pointers.CriFsBinder_BindFiles;
        if (IntPtr.Size == 4 && RuntimeInformation.ProcessArchitecture == Architecture.X86)
        {
            // Try patch `push 1` -> `push -1`
            var scanner = scannerFactory.CreateScanner((byte*)bindFiles, 34);
            var ofs = scanner.FindPattern("6A 01");
            if (ofs.Found)
            {
                _logger.Info("Patching BindFiles' 1 file limit on x86");
                Span<byte> newBytes = stackalloc byte[] { 0x6A, 0xFF };
                Memory.Instance.SafeWrite((nuint)(bindFiles + ofs.Offset), newBytes);
                return;
            }

            // Try patch `1` when it's optimized to be passed via register.

            // We will check for eax, ecx and edx because these are caller saved; 
            // we will assume an optimizing compiler would not emit a register pass for
            // a callee saved register as that would be less efficient than passing by stack,
            // which is covered by above case.

            // `40` = inc eax -> `48` = dec eax
            // `41` = inc ecx -> `49` = dec ecx
            // `42` = inc edx -> `4A` = dec edx

            // `31 C0` or `33 C0` = xor eax, eax
            // `31 C9` or `33 C9` = xor ecx, ecx
            // `31 D2` or `33 D2` = xor edx, edx

            TryPatchIncrement("31 C0", "33 C0", "40", 0x48, "Patched BindFile inc eax -> dec eax");
            TryPatchIncrement("31 C9", "33 C9", "41", 0x49, "Patched BindFile inc ecx -> dec ecx");
            TryPatchIncrement("31 D2", "33 D2", "42", 0x4A, "Patched BindFile inc edx -> dec edx");

            // We need to check for the 'xor' instruction first, then for presence of associated inc instruction
            // Changing from inc to dec, will underflow and result in a -1 value
            void TryPatchIncrement(string sig1, string sig2, string incSig, byte decValue, string message)
            {
                if (!scanner.TryFindEitherPattern(sig1, sig2, 0, out var localRes))
                    return;

                if (scanner.TryFindPattern(incSig, localRes.Offset, out var res))
                {
                    Memory.Instance.SafeWrite((nuint)((byte*)bindFiles + res.Offset), new Span<byte>(&decValue, 1));
                    _logger.Info(message);
                }
            }
        }
        else if (IntPtr.Size == 8 && RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            // Try patch `mov reg, 1` -> `mov reg, -1`
            var scanner = scannerFactory.CreateScanner((byte*)bindFiles, 40);

            // Target MSFT calling convention integer argument registers
            // mov ecx, 1 -> mov ecx,-1 || b9 01 00 00 00 -> b9 ff ff ff ff
            // mov edx, 1 -> mov edx,-1 || ba 01 00 00 00 -> ba ff ff ff ff
            // mov r8d, 1 -> mov r8d,-1 || 41 b8 01 00 00 00 -> 41 b8 ff ff ff ff
            // mov r9d, 1 -> mov r9d,-1 || 41 b9 01 00 00 00 -> 41 b9 ff ff ff ff
            TryPatchMovReg("b9 01 00 00 00", "Patched BindFile mov ecx, 1 -> mov ecx,-1");
            TryPatchMovReg("ba 01 00 00 00", "Patched BindFile mov edx, 1 -> mov edx,-1");
            TryPatchMovReg("41 b8 01 00 00 00", "Patched BindFile mov r8d, 1 -> mov r8d,-1");
            TryPatchMovReg("41 b9 01 00 00 00", "Patched BindFile mov r9d, 1 -> mov r9d,-1");

            void TryPatchMovReg(string movRegSig, string message)
            {
                int newValue = -1;
                if (scanner!.TryFindPattern(movRegSig, 0, out var res))
                {
                    var operandOffset = movRegSig.Length == 14 ? 1 : 2; // check if 32-bit register or not.
                    Memory.Instance.SafeWrite((nuint)((byte*)bindFiles + res.Offset + operandOffset),
                        new Span<byte>(&newValue, 1));
                    _logger.Info(message);
                }
            }
        }
    }

    #region Init

    private static CriError FinalizeLibraryImpl()
    {
        // No need to clear _allocator, because it's only used for CRI related stuff
        // so any data in it will become invalid after it's done here.
        var result = _finalizeLibraryHook!.OriginalFunction();
        Memory.Instance.Free(_libraryMemory);
        return result;
    }

    private static CriError InitializeLibraryImpl(CriFsConfig* config, void* buffer, int size)
    {
        // Note: This wastes a bit of memory, because the original memory passed by the game now doesn't have much
        //       use. 
        UpdateCriConfig(config);

        // The data from CRI buffers may be sourced from malloc and is not guaranteed to be zero'd
        // Therefore we clear.
        var alloc = new SimpleNativeLinkedListAllocator(buffer, size);
        alloc.Clear();
        _allocator = new MemoryAllocatorWithLinkedListBackup(alloc);

        // Replace the buffer with our own one.
        _getWorkSizeForLibraryFn!(config, &size);
        _libraryMemory = Memory.Instance.Allocate((nuint)size);
        return _initializeLibraryHook!.OriginalFunction(config, (void*)_libraryMemory.Address, size);
    }

    private static void UpdateCriConfig(CriFsConfig* config)
    {
        // We are making a wild guess here:
        // - Max 8 binder handles a game would use (very generous, usually this is just 1)
        // - Max 4 binder prefixes used by extensions of CriFs.V2.Hook (BindBuilder.BindFolderName)
        config->MaxBinds += 32; // 8 * 4

        // Here we make a rough estimate for max file count.
        // Reloaded can load mods at runtime, however we cannot predict what the user might load, and there is no
        // error handling mechanism in CRI for exceeding max files, which is a bit problematic.
        // In our case, we will insert a MessageBox to handle this edge case.
        _additionalFiles = _content.Count * 2;

        // An additional loader is used when we call BindFiles.
        config->MaxLoaders += 1;
        config->MaxFiles += _additionalFiles;
    }

    private static bool AssertWillFunction()
    {
        if (Pointers.CriFsBinder_BindCpk == 0 || 
            Pointers.CriFsBinder_BindFiles == 0 || 
            Pointers.CriFsBinder_GetSizeForBindFiles == 0 ||
            Pointers.CriFsBinder_GetStatus == 0 || 
            Pointers.CriFsBinder_Unbind == 0 || 
            Pointers.CriFs_InitializeLibrary == 0 ||
            Pointers.CriFs_CalculateWorkSizeForLibrary == 0 || 
            Pointers.CriFsIo_Open == 0 || 
            Pointers.CriFsBinder_Find == 0)
        {
            _logger.Fatal("One of the required functions is missing (see log). CRI FS version in this game is incompatible.");
            return false;
        }

        if (Pointers.CriFsIo_Exists == 0)
            _logger.Warning("IO Exists is missing. This should generally have no runtime impact.");

        if (Pointers.CriFsIo_IsUtf8 == (void*)0)
            _logger.Warning("IsUtf8 flag is missing. We will assume ANSI mode.");

        if (Pointers.CriFs_FinalizeLibrary == 0)
            _logger.Warning(
                "FinalizeLibrary function is missing. Ignore this unless game can shutdown and restart CRI APIs for some reason (not previously seen in wild; but maybe possible in e.g. game collections).");

        if (Pointers.CriFsBinder_SetPriority == 0)
            _logger.Warning(
                "SetPriority function is missing. There's no guarantee custom mod files will have priority over originals.");

        return true;
    }

    #endregion

    #region Binding

    private static CriError BindCpkImpl(IntPtr bndrhn, IntPtr srcbndrhn, [MarshalAs(UnmanagedType.LPStr)] string path,
        IntPtr work, int worksize, uint* bndrid)
    {
        if (BinderHandles.Add(bndrhn))
            BindAll(bndrhn);

        return _bindCpkHook!.OriginalFunction(bndrhn, srcbndrhn, path, work, worksize, bndrid);
    }

    private static void BindAll(IntPtr bndrhn)
    {
        _logger.Info("Setting Up Binds for Handle {0}", bndrhn);
        BindFolder(bndrhn, Int32.MaxValue);
    }

    private static void BindFolder(nint bndrhn, int priority)
    {
        uint bndrid = 0;
        CriFsBinderStatus status = 0;

        _logger.Debug("Binding Custom Files!! with priority {0}", priority);
        var size = 0;

        if (_content.Count > _additionalFiles)
        {
            Native.MessageBox(0,
                "Unable to load custom files because file limit will be exceeded (game would freeze).\n" +
                "This should only ever occur if you've tried to load extra mods while game is running.\n" +
                "If it occurred in any other context, please report this as a bug!", "Oh Noes!", 0);

            _logger.Error("Unable to Bind {0} due to open file limit reached!");
            return;
        }

        var fileList = new StringBuilder(_contentLength);
        foreach (var file in _content.GetValues())
        {
            fileList.Append(file.Key);
            fileList.Append("\n");
        }

        fileList.TrimFinalNewline();
        var fileListStr = fileList.ToString();
        var err = _getSizeForBindFilesFn!(bndrhn, fileListStr, &size);
        if (err < 0)
        {
            _logger.Error("Binding Files Failed: Failed to get size of Bind Files {0}", err);
            return;
        }

        var workMem = _allocator.Allocate(size);
        var watch = Stopwatch.StartNew();
        err = _bindFilesFn!(bndrhn, IntPtr.Zero, fileListStr, (nint)workMem.Address, size, &bndrid);

        if (err < 0)
        {
            // either find a way to handle bindCpk errors properly or ignore
            _logger.Error("Binding Files Failed with Error {0}", err);
            workMem.Dispose();
            return;
        }

        while (true)
        {
            _getStatusFn!(bndrid, &status);
            switch (status)
            {
                case CRIFSBINDER_STATUS_COMPLETE:
                    _setPriorityFn?.Invoke(bndrid, priority);
                    _logger.Info("Bind Complete! {0} files, Id: {1}, Time: {2}ms", _content.Count, bndrid,
                        watch.ElapsedMilliseconds);
                    Bindings.Add(new CpkBinding(workMem, bndrid));
                    return;
                case CRIFSBINDER_STATUS_ERROR:
                    _logger.Info("Bind Failed!");
                    _unbindFn!(bndrid);
                    workMem.Dispose();
                    return;
            }
        }
    }

    #endregion

    #region Public API

    /// <summary>
    ///     Unbinds all binded directories.
    /// </summary>
    public static void UnbindAll()
    {
        foreach (var binding in Bindings)
        {
            // If this fails, we can't do much, but log it anyway.
            var result = _unbindFn!(binding.BindId);
            if (result < 0)
                _logger.Error("Unbind Failed! :(");

            binding.Dispose();
        }

        Bindings.Clear();
    }

    /// <summary>
    ///     Binds all directories.
    /// </summary>
    public static void BindAll()
    {
        if (BinderHandles.Count <= 0)
        {
            _logger.Warning("Bind all is no-op because no CPK/binder has been created yet.");
            return;
        }

        foreach (var binderHandle in BinderHandles)
            BindAll(binderHandle);
    }


    /// <summary>
    ///     Enables/disables printing of file access.
    /// </summary>
    public static void SetPrintFileAccess(bool printFileAccess) => _printFileAccess = printFileAccess;
    
    /// <summary>
    ///     Enables/disables printing of file redirects.
    /// </summary>
    public static void SetPrintFileRedirect(bool printFileRedirects) => _printFileRedirects = printFileRedirects;

    /// <summary>
    ///     Updates the content that will be bound at runtime.
    /// </summary>
    /// <param name="content">
    ///     The content to be bound.
    ///     This is a dictionary of relative paths (using forward slashes) to their full file paths.
    ///     The key must ignore case.
    /// </param>
    public static void UpdateDataToBind(SpanOfCharDict<string> content)
    {
        _content = content;

        // Calculate content length.
        var bindLength = 0;
        foreach (var item in content.GetValues())
            bindLength += item.Value.Length + 1;

        _contentLength = bindLength;
    }

    #endregion

    #region Redirect Out of Game Folder & Fix Caps

    private static nint ExistsImpl(byte* stringPtr, int* result)
    {
        if (stringPtr == null)
            return _ioExistsHook!.OriginalFunction(stringPtr, result);

        if (Pointers.CriFsIo_IsUtf8 != (int*)0 && *Pointers.CriFsIo_IsUtf8 == 1)
        {
            var str = Marshal.PtrToStringUTF8((nint)stringPtr);
            if (!_content.TryGetValue(SanitizeCriPath(str!), out var value, out _))
                return _ioExistsHook!.OriginalFunction(stringPtr, result);

            var tempStr = Marshal.StringToCoTaskMemUTF8(value);
            if (_printFileRedirects)
                _logger.Info("Exist_Utf_Redirect: {0}", value);

            var err = _ioExistsHook!.OriginalFunction((byte*)tempStr, result);
            Marshal.FreeCoTaskMem(tempStr);
            return err;
        }
        else
        {
            var str = Marshal.PtrToStringAnsi((nint)stringPtr);
            if (!_content.TryGetValue(SanitizeCriPath(str!), out var value, out _))
                return _ioExistsHook!.OriginalFunction(stringPtr, result);

            var tempStr = Marshal.StringToHGlobalAnsi(value);
            if (_printFileRedirects)
                _logger.Info("Exist_Ansi_Redirect: {0}", value);

            var err = _ioExistsHook!.OriginalFunction((byte*)tempStr, result);
            Marshal.FreeHGlobal(tempStr);
            return err;
        }
    }

    private static nint CriFsOpenImpl(byte* stringPtr, int fileCreationType, int desiredAccess, nint** result)
    {
        if (stringPtr == null)
            return _ioOpenHook!.OriginalFunction(stringPtr, fileCreationType, desiredAccess, result);

        if (Pointers.CriFsIo_IsUtf8 != (int*)0 && *Pointers.CriFsIo_IsUtf8 == 1)
        {
            var str = Marshal.PtrToStringUTF8((nint)stringPtr);
            if (!_content.TryGetValue(SanitizeCriPath(str!), out var value, out _))
                return _ioOpenHook!.OriginalFunction(stringPtr, fileCreationType, desiredAccess, result);

            var tempStr = Marshal.StringToCoTaskMemUTF8(value);
            if (_printFileRedirects)
                _logger.Info("Open_Utf_Redirect: {0}", value);

            var err = _ioOpenHook!.OriginalFunction((byte*)tempStr, fileCreationType, desiredAccess, result);
            Marshal.FreeCoTaskMem(tempStr);
            return err;
        }
        else
        {
            var str = Marshal.PtrToStringAnsi((nint)stringPtr);
            if (!_content.TryGetValue(SanitizeCriPath(str!), out var value, out _))
                return _ioOpenHook!.OriginalFunction(stringPtr, fileCreationType, desiredAccess, result);

            var tempStr = Marshal.StringToHGlobalAnsi(value);
            if (_printFileRedirects)
                _logger.Info("Open_Ansi_Redirect: {0}", value);

            var err = _ioOpenHook!.OriginalFunction((byte*)tempStr, fileCreationType, desiredAccess, result);
            Marshal.FreeHGlobal(tempStr);
            return err;
        }
    }

    #endregion

    private static nint CriFsBinderFindImpl(nint bndrhn, nint path, void* finfo, bool* exist)
    {
        if (path == 0)
            return _findFileHook!.OriginalFunction(bndrhn, path, finfo, exist);

        var str = Marshal.PtrToStringAnsi(path);
        if (_printFileAccess)
            _logger.Info("Binder_Find: {0}", str);
        
        if (!_content.TryGetValue(SanitizeCriPath(str!), out _, out var originalKey))
            return _findFileHook!.OriginalFunction(bndrhn, path, finfo, exist);
        
        var tempStr = Marshal.StringToHGlobalAnsi(originalKey);
        _logger.Debug("Binder_Find_Redirect: {0}", originalKey);
        var err = _findFileHook!.OriginalFunction(bndrhn, tempStr, finfo, exist);
        Marshal.FreeHGlobal(tempStr);
        return err;
    }

    private static ReadOnlySpan<char> SanitizeCriPath(string str)
    {
        str!.ReplaceBackWithForwardSlashInPlace();
        if (str.StartsWith('/'))
            return str.AsSpan(1);

        return str.AsSpan();
    }

    // x86/x64 specific code
    private static byte[] _previousCallCode = new byte[5];
    private static byte[] _newCallCode = { 0x90, 0x90, 0x90, 0x90, 0x90 };
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

internal readonly struct CpkBinding : IDisposable
{
    private readonly IMemoryAllocation _alloc;
    internal readonly uint BindId;

    public CpkBinding(IMemoryAllocation alloc, uint bindId) : this()
    {
        _alloc = alloc;
        BindId = bindId;
    }

    public void Dispose()
    {
        _alloc.Dispose();
    }
}