using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CriFs.V2.Hook.Utilities.Extensions;

/// <summary>
/// Extensions for array types.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ArrayExtensions
{
    /// <summary>
    /// Returns a reference to an element at a specified index without performing a bounds check.
    /// </summary>
    /// <typeparam name="T">The type of elements in the input <typeparamref name="T"/> array instance.</typeparam>
    /// <param name="array">The input <typeparamref name="T"/> array instance.</param>
    /// <param name="i">The index of the element to retrieve within <paramref name="array"/>.</param>
    /// <returns>A reference to the element within <paramref name="array"/> at the index specified by <paramref name="i"/>.</returns>
    /// <remarks>This method doesn't do any bounds checks, therefore it is responsibility of the caller to ensure the <paramref name="i"/> parameter is valid.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T DangerousGetReferenceAt<T>(this T[] array, int i)
    {
        ref T r0 = ref MemoryMarshal.GetArrayDataReference(array);
        return ref Unsafe.Add(ref r0, (nint)(uint)i);
    }
    
    /// <summary>
    /// Returns a reference to an element at a specified index within a given <see cref="ReadOnlySpan{T}"/>, with no bounds checks.
    /// </summary>
    /// <typeparam name="T">The type of elements in the input <see cref="ReadOnlySpan{T}"/> instance.</typeparam>
    /// <param name="span">The input <see cref="ReadOnlySpan{T}"/> instance.</param>
    /// <param name="i">The index of the element to retrieve within <paramref name="span"/>.</param>
    /// <returns>A reference to the element within <paramref name="span"/> at the index specified by <paramref name="i"/>.</returns>
    /// <remarks>This method doesn't do any bounds checks, therefore it is responsibility of the caller to ensure the <paramref name="i"/> parameter is valid.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T DangerousGetReferenceAt<T>(this ReadOnlySpan<T> span, Index i)
    {
        ref T r0 = ref MemoryMarshal.GetReference(span);
        ref T ri = ref Unsafe.Add(ref r0, (nint)(uint)i.GetOffset(span.Length));

        return ref ri;
    }
}