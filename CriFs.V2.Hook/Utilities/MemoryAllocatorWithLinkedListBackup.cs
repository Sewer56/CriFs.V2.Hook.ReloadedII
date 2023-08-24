using Reloaded.Memory;
using Reloaded.Memory.Structs;

namespace CriFs.V2.Hook.Utilities;

/// <summary>
///     A simple memory allocator with backup of native linked list memory.
/// </summary>
internal readonly struct MemoryAllocatorWithLinkedListBackup
{
    private readonly SimpleNativeLinkedListAllocator _allocator;
    public MemoryAllocatorWithLinkedListBackup(SimpleNativeLinkedListAllocator allocator) => _allocator = allocator;

    public unsafe IMemoryAllocation Allocate(int size)
    {
        var data = _allocator.Allocate(size);
        if (data != null)
            return new LinkedListAllocation(data, _allocator);

        return new NativeMemoryAllocation(Memory.Instance.Allocate((nuint)size));
    }
}

internal unsafe struct NativeMemoryAllocation : IMemoryAllocation
{
    public byte* Address => (byte*)Allocation.Address; 
    public unsafe MemoryAllocation Allocation { get; }
    public unsafe NativeMemoryAllocation(MemoryAllocation alloc)
    {
        Allocation = alloc;
    }

    public unsafe void Dispose() => Memory.Instance.Free(Allocation);
}

internal struct LinkedListAllocation : IMemoryAllocation
{
    public unsafe byte* Address { get; }
    private SimpleNativeLinkedListAllocator _allocator;

    public unsafe LinkedListAllocation(void* data, SimpleNativeLinkedListAllocator alloc)
    {
        Address = (byte*)data;
        _allocator = alloc;
    }

    public unsafe void Dispose() => _allocator.Deallocate(Address);
}

internal unsafe interface IMemoryAllocation : IDisposable
{
    public byte* Address { get; }
}