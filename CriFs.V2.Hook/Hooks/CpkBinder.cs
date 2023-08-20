using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using CriFs.V2.Hook.Utilities;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Sources;
using static CriFs.V2.Hook.CRI.CpkBinderPointers;
using static CriFs.V2.Hook.CRI.CRI;
using static CriFs.V2.Hook.CRI.CRI.CriFsBinderStatus;
using Native = CriFs.V2.Hook.Utilities.Native;

namespace CriFs.V2.Hook.Hooks;

/// <summary>
/// Class to bind our custom CPKs via hooking.
/// </summary>
public static unsafe class CpkBinder
{
    private static string _outputDirectory = null!;
    private static Logger _logger = null!;

    private static IHook<criFs_InitializeLibrary>? _initializeLibraryHook;
    private static IHook<criFs_FinalizeLibrary>? _finalizeLibraryHook;
    private static IHook<criFsBinder_BindCpk>? _bindCpkHook;
    
    private static criFs_CalculateWorkSizeForLibrary? _getWorkSizeForLibraryFn;
    private static criFsBinder_BindFiles? _bindFilesFn;
    private static criFsBinder_GetWorkSizeForBindFiles? _getSizeForBindFilesFn;
    private static IHook<criFsLoader_LoadRegisteredFile_Internal>? _loadRegisteredFileFn;
    private static criFsBinder_GetStatus? _getStatusFn;
    private static criFsBinder_SetPriority? _setPriorityFn;
    private static criFsBinder_Unbind? _unbindFn;
    private static List<string> _content = new();
    private static int _contentLength = 0;
    
    private static readonly HashSet<IntPtr> BinderHandles = new(16); // 16 is default for max handle count.
    private static readonly List<CpkBinding> Bindings = new();
    private static int _additionalFiles = 0;
    private static void* _libraryMemory;
    private static MemoryAllocatorWithLinkedListBackup _allocator;

    public static void Init(string outputDirectory, Logger logger, IReloadedHooks hooks)
    {
        _logger = logger;
        _outputDirectory = outputDirectory;
        if (!AssertWillFunction())
            return;
        
        _initializeLibraryHook = hooks.CreateHook<criFs_InitializeLibrary>(InitializeLibraryImpl, CriFs_InitializeLibrary).Activate();
        _bindCpkHook = hooks.CreateHook<criFsBinder_BindCpk>(BindCpkImpl, CriFsBinder_BindCpk).Activate();
        _bindFilesFn = hooks.CreateWrapper<criFsBinder_BindFiles>(CriFsBinder_BindFiles, out _);
        _getSizeForBindFilesFn = hooks.CreateWrapper<criFsBinder_GetWorkSizeForBindFiles>(CriFsBinder_GetSizeForBindFiles, out _);
        _getStatusFn = hooks.CreateWrapper<criFsBinder_GetStatus>(CriFsBinder_GetStatus, out _);
        _unbindFn = hooks.CreateWrapper<criFsBinder_Unbind>(CriFsBinder_Unbind, out _);
        _getWorkSizeForLibraryFn = hooks.CreateWrapper<criFs_CalculateWorkSizeForLibrary>(CriFs_CalculateWorkSizeForLibrary, out _);
        
        if (CriFs_FinalizeLibrary != 0)
            _finalizeLibraryHook = hooks.CreateHook<criFs_FinalizeLibrary>(FinalizeLibraryImpl, CriFs_FinalizeLibrary).Activate();
        
        if (CriFsBinder_SetPriority != 0)
            _setPriorityFn = hooks.CreateWrapper<criFsBinder_SetPriority>(CriFsBinder_SetPriority, out _);

        if (CriFsLoader_LoadRegisteredFile != 0)
            _loadRegisteredFileFn = hooks.CreateHook<criFsLoader_LoadRegisteredFile_Internal>(LoadRegisteredFileInternal, CriFsLoader_LoadRegisteredFile).Activate();
    }

    private static CriError FinalizeLibraryImpl()
    {
        // No need to clear _allocator, because it's only used for CRI related stuff
        // so any data in it will become invalid after it's done here.
        Memory.Instance.Free((nuint)_libraryMemory);
        return _finalizeLibraryHook!.OriginalFunction();
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
        _libraryMemory = (void*)Memory.Instance.Allocate(size);
        return _initializeLibraryHook!.OriginalFunction(config, _libraryMemory, size);
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
        config->MaxFiles += _additionalFiles;
    }

    private static bool AssertWillFunction()
    {
        if (CriFsBinder_BindCpk == 0 || CriFsBinder_BindFiles == 0 || CriFsBinder_GetSizeForBindFiles == 0 || CriFsBinder_GetStatus == 0 || CriFsBinder_Unbind == 0 || CriFs_InitializeLibrary == 0 || CriFs_CalculateWorkSizeForLibrary == 0)
        {
            _logger.Fatal("One of the required functions is missing. CRI FS version in this game is incompatible.");
            return false;
        }
        
        if (CriFs_FinalizeLibrary == 0)
            _logger.Warning("FinalizeLibrary function is missing. Ignore this unless game can shutdown and restart CRI APIs for some reason (not previously seen in wild; but maybe possible in e.g. game collections).");

        if (CriFsBinder_SetPriority == 0)
            _logger.Warning("SetPriority function is missing. There's no guarantee custom mod files will have priority over originals.");

        if (CriFsLoader_LoadRegisteredFile == 0)
            _logger.Warning("LoadRegisteredFile function is missing. File Access Logging is Disabled.");
        
        return true;
    }

    private static IntPtr LoadRegisteredFileInternal(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5)
    {
        var namePtr = (IntPtr*)IntPtr.Add(a1, 16);
        _logger.Info(Marshal.PtrToStringAnsi(*namePtr)!);
        return _loadRegisteredFileFn!.OriginalFunction(a1, a2, a3, a4, a5);
    }
    
    private static CriError BindCpkImpl(IntPtr bndrhn, IntPtr srcbndrhn, [MarshalAs(UnmanagedType.LPStr)] string path, IntPtr work, int worksize, uint* bndrid)
    {
        if (BinderHandles.Add(bndrhn))
            BindAll(bndrhn);
        
        return _bindCpkHook!.OriginalFunction(bndrhn, srcbndrhn, path, work, worksize, bndrid);
    }

    private static void BindAll(IntPtr bndrhn)
    {
        _logger.Info("Setting Up Binds for Handle {0}", bndrhn);
        
        // Get all prefixes :wink:
        WindowsDirectorySearcher.TryGetDirectoryContents(_outputDirectory, out _, out var directories);
        foreach (var directory in directories)
            BindFolder(bndrhn, directory, int.MaxValue);
    }

    private static void BindFolder(nint bndrhn, DirectoryInformation dirInfo, int priority)
    {
        uint bndrid = 0;
        CriFsBinderStatus status = 0;
        CriError err = 0;
        
        _logger.Debug("Binding Directory {0} with priority {1}", dirInfo.FullPath, priority);
        int size = 0;
        
        if (_content.Count > _additionalFiles)
        {
            Native.MessageBox(0, "Unable to load custom files because file limit will be exceeded (game would freeze).\n" +
                                 "This should only ever occur if you've tried to load extra mods while game is running.\n" +
                                 "If it occurred in any other context, please report this as a bug!", "Oh Noes!", 0);
            
            _logger.Error("Unable to Bind {0} due to open file limit reached!");
            return;
        }
        
        var fileList = new StringBuilder(_contentLength);
        foreach (var file in _content)
        {
            fileList.Append(file);
            fileList.Append("\n");
        }

        fileList.TrimFinalNewline();
        var fileListStr = fileList.ToString();
        err = _getSizeForBindFilesFn!(bndrhn, fileListStr, &size);
        if (err < 0)
        {
            _logger.Error("Binding Files Failed: Failed to get size of Bind Files {0}", err);
            return;
        }

        var workMem = _allocator.Allocate(size);
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
                    _logger.Info("Bind Complete! {0}, Id: {1}", fileListStr, bndrid);
                    Bindings.Add(new CpkBinding(workMem, bndrid));
                    return;
                case CRIFSBINDER_STATUS_ERROR:
                    _logger.Info("Bind Failed! {0}", dirInfo.FullPath);
                    _unbindFn!(bndrid);
                    workMem.Dispose();
                    return;
                default:
                    Thread.Sleep(10);
                    break;
            }
        }
    }

    /// <summary>
    /// Unbinds all binded directories.
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
    /// Binds all directories.
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
    /// Enables/disables printing of file access.
    /// </summary>
    public static void SetPrintFileAccess(bool printFileAccess)
    {
        if (printFileAccess)
            _loadRegisteredFileFn?.Enable();
        else
            _loadRegisteredFileFn?.Disable();
    }

    /// <summary>
    /// Updates the content that will be bound at runtime.
    /// </summary>
    /// <param name="content">
    ///     The content to be bound.
    ///     This is a list of all paths relative to game directory to be bound.
    /// </param>
    public static void Update(List<string> content)
    {
        _content = content;
        
        // Calculate content length.
        int bindLength = 0;
        foreach (var item in CollectionsMarshal.AsSpan(content))
            bindLength += (item.Length + 1);

        _contentLength = bindLength;
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

    public void Dispose() => _alloc.Dispose();
}