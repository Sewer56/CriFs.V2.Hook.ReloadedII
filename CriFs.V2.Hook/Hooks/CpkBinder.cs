using System.Runtime.InteropServices;
using CriFs.V2.Hook.CRI;
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
    private static criFsBinder_GetStatus? _getStatusFn;
    private static criFsBinder_SetPriority? _setPriorityFn;
    private static criFsBinder_Unbind? _unbindFn;
    private static bool _firstCpkLoaded;
    private static IntPtr _binderHandle;
    private static List<CpkBinding> _bindings = null!;

    public static void Init(string outputDirectory, Logger logger, IReloadedHooks hooks)
    {
        _logger = logger;
        _outputDirectory = outputDirectory;
        _bindings = new List<CpkBinding>();
        if (!AssertWillFunction())
            return;
            
        _bindCpkHook = hooks.CreateHook<criFsBinder_BindCpk>(BindCpkImpl, _bindCpk).Activate();
        _bindDirFn = hooks.CreateWrapper<criFsBinder_BindCpk>(_bindDir, out _);
        _getSizeForBindDirFn = hooks.CreateWrapper<criFsBinder_GetWorkSizeForBindDirectory>(_getSizeForBindDir, out _);
        _getStatusFn = hooks.CreateWrapper<criFsBinder_GetStatus>(_getStatus, out _);
        _unbindFn = hooks.CreateWrapper<criFsBinder_Unbind>(_unbind, out _);
        
        if (_setPriority != 0)
            _setPriorityFn = hooks.CreateWrapper<criFsBinder_SetPriority>(_setPriority, out _);
    }

    private static bool AssertWillFunction()
    {
        if (_bindCpk == 0 || _bindDir == 0 || _getSizeForBindDir == 0 || _getStatus == 0 || _unbind == 0)
        {
            _logger.Fatal("One of the required functions is missing. CRI FS version in this game is incompatible.");
            return false;
        }

        if (_setPriority == 0)
            _logger.Warning("SetPriority function is missing. There's no guarantee custom mod files will have priority over originals.");

        return true;
    }

    private static CriError BindCpkImpl(IntPtr bndrhn, IntPtr srcbndrhn, [MarshalAs(UnmanagedType.LPStr)] string path, IntPtr work, int worksize, uint* bndrid)
    {
        if (!_firstCpkLoaded)
        {
            _binderHandle = bndrhn;
            BindAll(bndrhn);
        }

        _firstCpkLoaded = true;
        return _bindCpkHook!.OriginalFunction(bndrhn, srcbndrhn, path, work, worksize, bndrid);
    }

    private static void BindAll(IntPtr bndrhn)
    {
        _logger.Info("Setting Up Binds!!");
        WindowsDirectorySearcher.TryGetDirectoryContents(_outputDirectory, out _, out var directories);
        foreach (var directory in directories)
            BindFolder(bndrhn, directory.FullPath, 0x10000000);
    }

    private static uint BindFolder(IntPtr bndrhn, string path, int priority)
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
            return 0;
        }

        var workMem = Memory.Instance.Allocate(size);
        err = _bindDirFn!(bndrhn, IntPtr.Zero, path, (nint)workMem, size, &bndrid);
        
        if (err < 0)
        {
            // either find a way to handle bindCpk errors properly or ignore
            _logger.Error("Binding Directory Failed with Error {0}", err);
            Memory.Instance.Free(workMem);
            return 0;
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
                    return bndrid;
                case CRIFSBINDER_STATUS_ERROR:
                    _logger.Info("Bind Failed! {0}", path);
                    _unbindFn!(bndrid);
                    Memory.Instance.Free(workMem);
                    return 0;
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
            var result = _unbindFn!(binding._bindId);
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
        if (!_firstCpkLoaded)
        {
            _logger.Warning("Bind all not possible because first CPK is not loaded yet.");
            return;
        }
        
        BindAll(_binderHandle);
    }
}

internal struct CpkBinding : IDisposable
{
    private nuint _workAreaPtr;
    internal uint _bindId;

    public CpkBinding(nuint workAreaPtr, uint bindId) : this()
    {
        _workAreaPtr = workAreaPtr;
        _bindId = bindId;
    }

    public void Dispose() => Memory.Instance.Free(_workAreaPtr);
}