using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using CriFs.V2.Hook.CRI;
using CriFs.V2.Hook.Utilities;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory;
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
public static unsafe partial class CpkBinder
{
    private static Logger _logger = null!;

    private static IHook<criFsBinder_Find>? _findFileHook;
    private static IHook<criFsLoader_RegisterFile>? _registerFileHook;
    private static IHook<criFs_InitializeLibrary>? _initializeLibraryHook;
    private static IHook<criFs_FinalizeLibrary>? _finalizeLibraryHook;
    private static IHook<criFsBinder_BindCpk>? _bindCpkHook;
    private static IHook<criFsIo_Exists>? _ioExistsHook;
    private static IHook<criFsIo_Open>? _ioOpenHook;
    private static IHook<criFsBinder_BindFiles_WithoutMarshalling>? _bindFileHook;
    private static IHook<criFsBinder_BindFiles_WithoutMarshalling>? _bindFilesHook;

    private static criFs_CalculateWorkSizeForLibrary? _getWorkSizeForLibraryFn;
    private static criFsBinder_GetWorkSizeForBindFiles? _getSizeForBindFilesFn;
    private static criFsBinder_GetStatus? _getStatusFn;
    private static criFsBinder_BindFiles? _bindFileFn;
    private static criFsBinder_BindFiles? _bindFilesFn;
    private static criFsBinder_SetPriority? _setPriorityFn;
    private static criFsBinder_Unbind? _unbindFn;
    private static SpanOfCharDict<string> _content = new(0);
    private static int _contentLength;
    private static int _maxFilesMultiplier = 2;

    private static readonly HashSet<nint> BinderHandles = new(16); // 16 is default for max handle count.
    private static readonly List<CpkBinding> Bindings = new();
    private static int _additionalFiles;
    private static MemoryAllocation _libraryMemory;
    private static MemoryAllocatorWithLinkedListBackup _allocator;
    private static bool _printFileRegister;
    private static bool _printBinderAccess;
    private static bool _printFileRedirects;

    /// <remarks>
    ///     This should be called after <see cref="CpkBinderPointers" /> is initialized.
    /// </remarks>
    public static void Init(Logger logger, IReloadedHooks hooks, IScannerFactory scannerFactory)
    {
        _logger = logger;
        if (!AssertWillFunction())
            return;

        _findFileHook =
            hooks.CreateHook<criFsBinder_Find>(CriFsBinderFindImpl, Pointers.CriFsBinder_Find).Activate();
        _registerFileHook =
            hooks.CreateHook<criFsLoader_RegisterFile>(CriFsLoaderRegisterFileImpl, Pointers.CriFsLoader_RegisterFile)
                .Activate();
        _initializeLibraryHook =
            hooks.CreateHook<criFs_InitializeLibrary>(InitializeLibraryImpl, Pointers.CriFs_InitializeLibrary)
                .Activate();
        _bindCpkHook = hooks.CreateHook<criFsBinder_BindCpk>(BindCpkImpl, Pointers.CriFsBinder_BindCpk).Activate();
        _ioExistsHook = hooks.CreateHook<criFsIo_Exists>(ExistsImpl, Pointers.CriFsIo_Exists).Activate();
        _ioOpenHook = hooks.CreateHook<criFsIo_Open>(CriFsOpenImpl, Pointers.CriFsIo_Open).Activate();

        if (Pointers.CriFsBinder_BindFile != 0)
            _bindFileHook =
                hooks.CreateHook<criFsBinder_BindFiles_WithoutMarshalling>(BindFileImpl, Pointers.CriFsBinder_BindFile).Activate();
        
        if (Pointers.CriFsBinder_BindFiles != 0)
            _bindFilesHook =
                hooks.CreateHook<criFsBinder_BindFiles_WithoutMarshalling>(BindFilesImpl,
                    Pointers.CriFsBinder_BindFiles).Activate();

        _getSizeForBindFilesFn =
            hooks.CreateWrapper<criFsBinder_GetWorkSizeForBindFiles>(Pointers.CriFsBinder_GetSizeForBindFiles, out _);
        _getStatusFn = hooks.CreateWrapper<criFsBinder_GetStatus>(Pointers.CriFsBinder_GetStatus, out _);
        _unbindFn = hooks.CreateWrapper<criFsBinder_Unbind>(Pointers.CriFsBinder_Unbind, out _);
        _getWorkSizeForLibraryFn =
            hooks.CreateWrapper<criFs_CalculateWorkSizeForLibrary>(Pointers.CriFs_CalculateWorkSizeForLibrary, out _);

        if (Pointers.CriFsBinder_BindFile != 0)
            _bindFileFn = hooks.CreateWrapper<criFsBinder_BindFiles>(Pointers.CriFsBinder_BindFile, out _);

        if (Pointers.CriFsBinder_BindFiles != 0)
            _bindFilesFn = hooks.CreateWrapper<criFsBinder_BindFiles>(Pointers.CriFsBinder_BindFiles, out _);

        if (Pointers.CriFs_FinalizeLibrary != 0)
            _finalizeLibraryHook = hooks
                .CreateHook<criFs_FinalizeLibrary>(FinalizeLibraryImpl, Pointers.CriFs_FinalizeLibrary)
                .Activate();

        if (Pointers.CriFsBinder_SetPriority != 0)
            _setPriorityFn = hooks.CreateWrapper<criFsBinder_SetPriority>(Pointers.CriFsBinder_SetPriority, out _);

        PatchBindFileIntoBindFiles(scannerFactory);
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
        _additionalFiles = _content.Count * _maxFilesMultiplier;

        // An additional loader is used when we call BindFiles.
        config->MaxLoaders += 1;
        config->MaxFiles += _additionalFiles;
    }

    private static bool AssertWillFunction()
    {
        var missingBindFile = Pointers is { CriFsBinder_BindFiles: 0, CriFsBinder_BindFile: 0 };
        if (missingBindFile ||
            Pointers.CriFsBinder_BindCpk == 0 ||
            Pointers.CriFsBinder_GetSizeForBindFiles == 0 ||
            Pointers.CriFsBinder_GetStatus == 0 ||
            Pointers.CriFsBinder_Unbind == 0 ||
            Pointers.CriFs_InitializeLibrary == 0 ||
            Pointers.CriFs_CalculateWorkSizeForLibrary == 0 ||
            Pointers.CriFsIo_Open == 0)
        {
            _logger.Fatal(
                "One of the required functions is missing (see log). CRI FS version in this game is incompatible.");
            return false;
        }

        if (Pointers.CriFsLoader_RegisterFile == 0)
            _logger.Warning(
                "RegisterFile is missing. Injected files are case sensitive and some logging may be missing!");

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

    private static CriError BindCpkImpl(nint bndrhn, nint srcbndrhn, [MarshalAs(UnmanagedType.LPStr)] string path,
        nint work, int worksize, uint* bndrid)
    {
        if (BinderHandles.Add(bndrhn))
            BindAll(bndrhn);

        return _bindCpkHook!.OriginalFunction(bndrhn, srcbndrhn, path, work, worksize, bndrid);
    }

    private static void BindAll(nint bndrhn)
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

        err = _bindFilesFn != null
            ? _bindFilesFn!(bndrhn, IntPtr.Zero, fileListStr, (nint)workMem.Address, size, &bndrid)
            : _bindFileFn!(bndrhn, IntPtr.Zero, fileListStr, (nint)workMem.Address, size,
                &bndrid); // patched to allow multiple

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
                    _logger.Debug("Bound files: {0}", fileListStr);
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
    ///     Enables/disables printing of files being registered.
    /// </summary>
    public static void SetPrintFileRegister(bool printFileRegister)
    {
        _printFileRegister = printFileRegister;
    }

    /// <summary>
    ///     Enables/disables printing of file redirects.
    /// </summary>
    public static void SetPrintFileRedirect(bool printFileRedirects)
    {
        _printFileRedirects = printFileRedirects;
    }

    /// <summary>
    ///     Enables/disables printing of Binder Accesses.
    /// </summary>
    public static void SetPrintBinderAccess(bool PrintBinderAccess)
    {
        _printBinderAccess = PrintBinderAccess;
    }

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
        FreeNewToOriginalCasing();
        NewToOriginalCasing = new SpanOfCharDict<nint>(_content.Count);

        // Calculate content length.
        var bindLength = 0;
        foreach (var item in content.GetValues())
            bindLength += item.Value.Length + 1;

        _contentLength = bindLength;
    }

    public static void SetMaxFilesMultiplier(int multiplier)
    {
        _maxFilesMultiplier = multiplier;
    }


    #endregion
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