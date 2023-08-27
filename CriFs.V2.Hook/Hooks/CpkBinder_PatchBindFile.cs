using System.Runtime.InteropServices;
using CriFs.V2.Hook.Utilities;
using Reloaded.Memory;
using Reloaded.Memory.Interfaces;
using Reloaded.Memory.Sigscan.Definitions;
using static CriFs.V2.Hook.CRI.CpkBinderPointers;

namespace CriFs.V2.Hook.Hooks;

/// <summary>
///     This file is responsible for patching BindFile API into BindFiles.
/// </summary>
public static unsafe partial class CpkBinder
{
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
                var newValue = -1;
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
}