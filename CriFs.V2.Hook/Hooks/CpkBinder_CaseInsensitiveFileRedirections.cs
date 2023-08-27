using System.Runtime.InteropServices;
using CriFs.V2.Hook.Utilities.Extensions;
using static CriFs.V2.Hook.CRI.CpkBinderPointers;

namespace CriFs.V2.Hook.Hooks;

/// <summary>
///     This file is responsible for redirecting items outside of game folder, as well as case insensitivity.
/// </summary>
public static unsafe partial class CpkBinder
{
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

    private static nint CriFsBinderFindImpl(nint bndrhn, nint path, void* finfo, int* exist)
    {
        // This hook converts case sensitive file paths to our code's casing.
        // Such that CRI can find them.
        if (path == 0)
            return _findFileHook!.OriginalFunction(bndrhn, path, finfo, exist);

        var str = Marshal.PtrToStringAnsi(path);
        if (_printFileAccess)
            _logger.Info("Binder_Find: {0}", str);

        if (!_content.TryGetValue(SanitizeCriPath(str!), out _, out var originalKey))
            return _findFileHook!.OriginalFunction(bndrhn, path, finfo, exist);

        var tempStr = Marshal.StringToHGlobalAnsi(originalKey);
        _logger.Debug("Binder_Find_Original: {0}", str);
        _logger.Debug("Binder_Find_Redirect: {0}", originalKey);
        var newExist = 0;
        var err = _findFileHook!.OriginalFunction(bndrhn, tempStr, finfo, &newExist);
        _logger.Debug("Binder_Find_Redirect Exist: {0}", newExist);
        if (exist != null)
            *exist = newExist;

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
}