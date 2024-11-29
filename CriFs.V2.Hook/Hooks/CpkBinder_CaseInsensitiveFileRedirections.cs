using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using CriFs.V2.Hook.Utilities;
using CriFs.V2.Hook.Utilities.Extensions;
using static CriFs.V2.Hook.CRI.CpkBinderPointers;

namespace CriFs.V2.Hook.Hooks;

/// <summary>
///     This file is responsible for redirecting items outside of game folder, as well as case insensitivity.
/// </summary>
public static unsafe partial class CpkBinder
{
    private static SpanOfCharDict<nint> NewToOriginalCasing = null!;
    private static readonly char[] CriSeparators = { ',', '\n', '\t' };

    // Note: Separators can be overwritten by game(s). I haven't seen a game which does it yet though, so not implemented.
    private static CRI.CRI.CriError BindFileImpl(nint bndrhn, nint srcbndrhn, byte* path, nint work, int worksize,
        uint* bndrid)
    {
        // Some games might bind file(s) using one of the Binded CPKs as the source handle
        // i.e. Bind a File Inside a CPK.
        if (srcbndrhn == 0)
            return _bindFileHook!.OriginalFunction(bndrhn, srcbndrhn, path, work, worksize, bndrid);

        if (!BinderHandles.Contains(srcbndrhn))
        {
            _logger.Warning("BindFile with Unrecognized Source Handle {0} called.", srcbndrhn);
            return _bindFileHook!.OriginalFunction(bndrhn, srcbndrhn, path, work, worksize, bndrid);
        }

        var pathStr = Marshal.PtrToStringAnsi((nint)path);
        if (!GetNewBindFilePaths(pathStr!, out var newPath))
            return _bindFileHook!.OriginalFunction(bndrhn, srcbndrhn, path, work, worksize, bndrid);

        _logger.Debug("BindFile Replace {0} -> {1}", pathStr, newPath);
        var tempStr = Marshal.StringToHGlobalAnsi(newPath);
        var result = _bindFileHook!.OriginalFunction(bndrhn, srcbndrhn, (byte*)tempStr, work, worksize, bndrid);
        Marshal.FreeHGlobal(tempStr);
        return result;
    }

    private static CRI.CRI.CriError BindFilesImpl(nint bndrhn, nint srcbndrhn, byte* path, nint work, int worksize,
        uint* bndrid)
    {
        // Some games might bind file(s) using one of the Binded CPKs as the source handle
        // i.e. Bind a File Inside a CPK.
        if (srcbndrhn == 0)
            return _bindFilesHook!.OriginalFunction(bndrhn, srcbndrhn, path, work, worksize, bndrid);

        if (!BinderHandles.Contains(srcbndrhn))
        {
            _logger.Warning("BindFiles with Unrecognized Source Handle {0} called.", srcbndrhn);
            return _bindFilesHook!.OriginalFunction(bndrhn, srcbndrhn, path, work, worksize, bndrid);
        }

        var pathStr = Marshal.PtrToStringAnsi((nint)path);
        if (!GetNewBindFilePaths(pathStr!, out var newPath))
            return _bindFilesHook!.OriginalFunction(bndrhn, srcbndrhn, path, work, worksize, bndrid);

        _logger.Debug("BindFiles Replace {0} -> {1}", pathStr, newPath);
        var tempStr = Marshal.StringToHGlobalAnsi(newPath);
        var result = _bindFilesHook!.OriginalFunction(bndrhn, srcbndrhn, (byte*)tempStr, work, worksize, bndrid);
        Marshal.FreeHGlobal(tempStr);
        return result;
    }

    private static bool GetNewBindFilePaths(string path, out string newCase)
    {
        // In this case, we must alter the case as needed.
        var separatorIdx = path!.IndexOfAny(CriSeparators);

        // Single File
        if (separatorIdx == -1)
            return _relativeToFullPathDict.TryGetValue(SanitizeCriPath(path!), out _, out newCase!);

        // Multiple files
        var separator = path[separatorIdx];
        var files = path.Split(separator);

        // Fixup the paths
        var replacedAny = false;
        for (var x = 0; x < files.Length; x++)
        {
            if (_relativeToFullPathDict.TryGetValue(SanitizeCriPath(files[x]), out _, out var newCasedPath))
            {
                files[x] = newCasedPath;
                replacedAny = true;
            }
        }

        if (!replacedAny)
        {
            newCase = "";
            return false;
        }

        var builder = new StringBuilder(path.Length + 1);
        foreach (var file in files)
        {
            builder.Append(file);
            builder.Append(separator);
        }

        newCase = builder.ToString();
        return replacedAny;
    }

    private static nint ExistsImpl(byte* stringPtr, int* result)
    {
        if (stringPtr == null)
            return _ioExistsHook!.OriginalFunction(stringPtr, result);

        if (Pointers.CriFsIo_IsUtf8 != (int*)0 && *Pointers.CriFsIo_IsUtf8 == 1)
        {
            var str = Marshal.PtrToStringUTF8((nint)stringPtr);
            if (!_relativeToFullPathDict.TryGetValue(SanitizeCriPath(str!), out var value, out _))
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
            if (!_relativeToFullPathDict.TryGetValue(SanitizeCriPath(str!), out var value, out _))
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
            if (!_relativeToFullPathDict.TryGetValue(SanitizeCriPath(str!), out var value, out _))
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
            if (!_relativeToFullPathDict.TryGetValue(SanitizeCriPath(str!), out var value, out _))
                return _ioOpenHook!.OriginalFunction(stringPtr, fileCreationType, desiredAccess, result);

            var tempStr = Marshal.StringToHGlobalAnsi(value);
            if (_printFileRedirects)
                _logger.Info("Open_Ansi_Redirect: {0}", value);

            var err = _ioOpenHook!.OriginalFunction((byte*)tempStr, fileCreationType, desiredAccess, result);
            Marshal.FreeHGlobal(tempStr);
            return err;
        }
    }

    private static nint CriFsLoaderRegisterFileImpl(nint loader,
        nint binder,
        nint path,
        int fileId,
        nint zero)
    {
        // This hook converts case sensitive file paths to our code's casing.
        // Such that CRI can load our paths.
        if (fileId != -1)
            return _registerFileHook!.OriginalFunction(loader, binder, path, fileId, zero);

        var str = Marshal.PtrToStringAnsi(path);
        if (_printFileRegister)
            _logger.Info("Register_File: {0}", str);

        if (!_relativeToFullPathDict.TryGetValue(SanitizeCriPath(str!), out _, out var originalKey))
            return _registerFileHook!.OriginalFunction(loader, binder, path, fileId, zero);

        // Call Find with path that matches ours
        var tempStr = Marshal.StringToHGlobalAnsi(originalKey);
        _logger.Debug("Register_File_Original: {0}, BndrHn: {1}", str, binder);
        _logger.Debug("Register_File_Redirect: {0}, BndrHn: {1}", originalKey, binder);
        var err = _registerFileHook!.OriginalFunction(loader, binder, tempStr, fileId, zero);

        Marshal.FreeHGlobal(tempStr);

        // Return result.
        return err;
    }
    
    private static nint CriFsBinderFindImpl(nint bndrhn, nint path, CRI.CRI.CriFsBinderFileInfo* finfo, int* exist) 
    { 
        // This hook converts case sensitive file paths to our code's casing. 
        // Such that CRI can find them. 
        if (path == 0) 
            return _findFileHook!.OriginalFunction(bndrhn, path, finfo, exist); 
 
        var str = Marshal.PtrToStringAnsi(path); 
        if (_printBinderAccess) 
            _logger.Info("Binder_Find: {0}", str); 
 
        if (!_relativeToFullPathDict.TryGetValue(SanitizeCriPath(str!), out _, out var originalKey)) 
            return _findFileHook!.OriginalFunction(bndrhn, path, finfo, exist); 
 
        // Call Find with path that matches ours 
        var tempStr = Marshal.StringToHGlobalAnsi(originalKey); 
        _logger.Debug("Binder_Find_Original: {0}", str); 
        _logger.Debug("Binder_Find_Redirect: {0}", originalKey); 
         
        var newExist = 0; 
        var err = _findFileHook!.OriginalFunction(bndrhn, tempStr, finfo, &newExist); 
        _logger.Debug("Binder_Find_Redirect Exist: {0}", newExist); 
         
        // Copy back exist value if pointer is non-null.         
        if (exist != null) 
            *exist = newExist; 
 
        Marshal.FreeHGlobal(tempStr); 
 
        if (finfo == null) 
            return err; 
         
        // Return original path in struct, in case user checks path in returned handles. 
        if (!NewToOriginalCasing.TryGetValue(str, out var ptr, out _)) 
        { 
            ptr = Marshal.StringToHGlobalAnsi(str); 
            NewToOriginalCasing.AddOrReplace(str!, ptr); 
        }
         
        finfo->Path = (byte*)ptr; 
 
        // Return result. 
        return err; 
    } 

    private static ReadOnlySpan<char> SanitizeCriPath(string str)
    {
        str!.ReplaceBackWithForwardSlashInPlace();
        return str.StartsWith('/') ? str.AsSpan(1) : str.AsSpan();
    }

    private static void FreeNewToOriginalCasing()
    {
        if (NewToOriginalCasing == null!)
            return;

        foreach (var val in NewToOriginalCasing.GetValues())
            Marshal.FreeHGlobal(val.Value);
    }
}