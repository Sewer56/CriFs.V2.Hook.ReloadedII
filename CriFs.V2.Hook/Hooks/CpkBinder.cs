using System.Runtime.InteropServices;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Sources;
using static CriFs.V2.Hook.CRI.CpkBinderPointers;
using static CriFs.V2.Hook.CRI.CRI;
using static CriFs.V2.Hook.CRI.CRI.CriFsBinderStatus;

namespace CriFs.V2.Hook.Hooks;

/// <summary>
/// Class to bind our custom CPKs via hooking.
/// </summary>
public static unsafe class CpkBinder
{
    private static string _outputDirectory = null!;
    private static Logger _logger = null!;

    private static IHook<criFsBinder_BindCpk>? _bindCpkHook;
    
    private static criFsBinder_BindCpk? _bindDirFn;
    private static criFsBinder_GetWorkSizeForBindDirectory? _getSizeForBindDirFn;
    private static IHook<criFsLoader_LoadRegisteredFile_Internal>? _loadRegisteredFileFn;
    private static criFsBinder_GetStatus? _getStatusFn;
    private static criFsBinder_SetPriority? _setPriorityFn;
    private static criFsBinder_Unbind? _unbindFn;
    private static HashSet<IntPtr> _binderHandles;
    private static List<CpkBinding> _bindings = null!;

    public static void Init(string outputDirectory, Logger logger, IReloadedHooks hooks)
    {
        _logger = logger;
        _outputDirectory = outputDirectory;
        _bindings = new List<CpkBinding>();
        _binderHandles = new HashSet<nint>(16);
        if (!AssertWillFunction())
            return;
            
        _bindCpkHook = hooks.CreateHook<criFsBinder_BindCpk>(BindCpkImpl, BindCpk).Activate();
        _bindDirFn = hooks.CreateWrapper<criFsBinder_BindCpk>(BindDir, out _);
        _getSizeForBindDirFn = hooks.CreateWrapper<criFsBinder_GetWorkSizeForBindDirectory>(GetSizeForBindDir, out _);
        _getStatusFn = hooks.CreateWrapper<criFsBinder_GetStatus>(GetStatus, out _);
        _unbindFn = hooks.CreateWrapper<criFsBinder_Unbind>(Unbind, out _);
        
        if (SetPriority != 0)
            _setPriorityFn = hooks.CreateWrapper<criFsBinder_SetPriority>(SetPriority, out _);

        if (LoadRegisteredFile != 0)
            _loadRegisteredFileFn = hooks.CreateHook<criFsLoader_LoadRegisteredFile_Internal>(LoadRegisteredFileInternal, LoadRegisteredFile).Activate();
    }

    private static bool AssertWillFunction()
    {
        if (BindCpk == 0 || BindDir == 0 || GetSizeForBindDir == 0 || GetStatus == 0 || Unbind == 0)
        {
            _logger.Fatal("One of the required functions is missing. CRI FS version in this game is incompatible.");
            return false;
        }

        if (SetPriority == 0)
            _logger.Warning("SetPriority function is missing. There's no guarantee custom mod files will have priority over originals.");

        if (LoadRegisteredFile == 0)
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
        if (_binderHandles.Add(bndrhn))
            BindAll(bndrhn);
        
        return _bindCpkHook!.OriginalFunction(bndrhn, srcbndrhn, path, work, worksize, bndrid);
    }

    private static void BindAll(IntPtr bndrhn)
    {
        _logger.Info("Setting Up Binds for Handle {0}", bndrhn);
        WindowsDirectorySearcher.TryGetDirectoryContents(_outputDirectory, out _, out var directories);
        foreach (var directory in directories)
            BindFolder(bndrhn, directory.FullPath, int.MaxValue);
    }

    private static void BindFolder(nint bndrhn, string path, int priority)
    {
        uint bndrid = 0;
        CriFsBinderStatus status = 0;
        CriError err = 0;
        
        _logger.Debug("Binding Directory {0} with priority {1}", path, priority);
        int size = 0;
        err = _getSizeForBindDirFn!(bndrhn, path, &size);
        if (err < 0)
        {
            _logger.Error("Binding Directory Failed: Failed to get size of Bind Directory {0}", err);
            return;
        }

        var workMem = Memory.Instance.Allocate(size);
        err = _bindDirFn!(bndrhn, IntPtr.Zero, path, (nint)workMem, size, &bndrid);
        
        if (err < 0)
        {
            // either find a way to handle bindCpk errors properly or ignore
            _logger.Error("Binding Directory Failed with Error {0}", err);
            Memory.Instance.Free(workMem);
            return;
        }

        while (true)
        {
            _getStatusFn!(bndrid, &status);
            switch (status)
            {
                case CRIFSBINDER_STATUS_COMPLETE:
                    _setPriorityFn?.Invoke(bndrid, priority);
                    _logger.Info("Bind Complete! {0}, Id: {1}", path, bndrid);
                    _bindings.Add(new CpkBinding(workMem, bndrid));
                    return;
                case CRIFSBINDER_STATUS_ERROR:
                    _logger.Info("Bind Failed! {0}", path);
                    _unbindFn!(bndrid);
                    Memory.Instance.Free(workMem);
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
        foreach (var binding in _bindings)
        {
            // If this fails, we can't do much, but log it anyway.
            var result = _unbindFn!(binding.BindId);
            if (result < 0)
                _logger.Error("Unbind Failed! :(");

            binding.Dispose();
        }

        _bindings.Clear();
    }

    /// <summary>
    /// Binds all directories.
    /// </summary>
    public static void BindAll()
    {
        if (_binderHandles.Count <= 0)
        {
            _logger.Warning("Bind all is no-op because no CPK/binder has been created yet.");
            return;
        }

        foreach (var binderHandle in _binderHandles)
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
}

internal struct CpkBinding : IDisposable
{
    private nuint _workAreaPtr;
    internal readonly uint BindId;

    public CpkBinding(nuint workAreaPtr, uint bindId) : this()
    {
        _workAreaPtr = workAreaPtr;
        BindId = bindId;
    }

    public void Dispose() => Memory.Instance.Free(_workAreaPtr);
}