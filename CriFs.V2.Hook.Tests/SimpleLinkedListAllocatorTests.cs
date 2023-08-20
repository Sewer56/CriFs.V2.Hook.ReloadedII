using CriFs.V2.Hook.Utilities;

namespace CriFs.V2.Hook.Tests;

public unsafe class SimpleLinkedListAllocatorTests
{
    [Fact]
    public void CanAllocate()
    {
        var buffer = new byte[1024];
        fixed (byte* bufferPtr = buffer)
        {
            var allocator = new SimpleNativeLinkedListAllocator(bufferPtr, buffer.Length);
            var allocatedPtr1 = allocator.Allocate(256);
            var allocatedPtr2 = allocator.Allocate(256);

            NotNull(allocatedPtr1);
            NotNull(allocatedPtr2);
            Assert.NotEqual((nint)allocatedPtr1, (nint)allocatedPtr2);
            Assert.True(allocatedPtr2 > allocatedPtr1);
        }
    }
    
    [Fact]
    public void CanDeallocate_WithFirstItemDeallocated()
    {
        var buffer = new byte[1024];
        fixed (byte* bufferPtr = buffer)
        {
            var allocator = new SimpleNativeLinkedListAllocator(bufferPtr, buffer.Length);
            var allocatedPtr1 = allocator.Allocate(256);
            _ = allocator.Allocate(256);

            // Deallocate the first item.
            allocator.Deallocate(allocatedPtr1);
            var allocatedPtr3 = allocator.Allocate(256);

            // After deallocating allocatedPtr1, we should be able to allocate again
            // in the same space.
            Assert.Equal((nint)allocatedPtr1, (nint)allocatedPtr3);
        }
    }
    
    [Fact]
    public void CanDeallocate_WithMiddleItemDeallocated()
    {
        var buffer = new byte[1024];
        fixed (byte* bufferPtr = buffer)
        {
            var allocator = new SimpleNativeLinkedListAllocator(bufferPtr, buffer.Length);
            _ = allocator.Allocate(256);
            var allocatedPtr2 = allocator.Allocate(256);
            _ = allocator.Allocate(256);

            // Deallocate the second item.
            allocator.Deallocate(allocatedPtr2);
            var allocatedPtr3 = allocator.Allocate(256);

            // After deallocating allocatedPtr2, we should be able to allocate again
            // in the same space.
            Assert.Equal((nint)allocatedPtr2, (nint)allocatedPtr3);
        }
    }

    [Fact]
    public void Allocate_ReturnsNullIfNoSpaceAvailable()
    {
        var buffer = new byte[256];
        fixed (byte* bufferPtr = buffer)
        {
            var allocator = new SimpleNativeLinkedListAllocator(bufferPtr, buffer.Length);

            var allocatedPtr1 = allocator.Allocate(100);
            var allocatedPtr2 = allocator.Allocate(100);
            var allocatedPtr3 = allocator.Allocate(100); 

            NotNull(allocatedPtr1);
            NotNull(allocatedPtr2);
            IsNull(allocatedPtr3);
        }
    }

    [Fact]
    public void Deallocate_IgnoresNonAllocatedMemory()
    {
        var buffer = new byte[256];
        fixed (byte* bufferPtr = buffer)
        {
            var allocator = new SimpleNativeLinkedListAllocator(bufferPtr, buffer.Length);

            var dummyPtr = (void*)12345; // Some random pointer not managed by allocator
            allocator.Deallocate(dummyPtr); 

            var allocatedPtr = allocator.Allocate(100);
            NotNull(allocatedPtr);
        }
    }

    private static void NotNull(void* ptr, string? message = null)
    {
        Assert.True(ptr != (void*)0, message ?? "Expected pointer to be not null, but it was null.");
    }

    private static void IsNull(void* ptr, string? message = null)
    {
        Assert.True(ptr == (void*)0, message ?? "Expected pointer to be null, but it was not null.");
    }
}