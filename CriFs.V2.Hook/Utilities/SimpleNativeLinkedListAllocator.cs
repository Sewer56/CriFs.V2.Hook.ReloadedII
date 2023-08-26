using System.Runtime.CompilerServices;
using CriFs.V2.Hook.Hooks;

namespace CriFs.V2.Hook.Utilities;

/// <summary>
///     Implementation of a simple linked-list based allocator.
/// </summary>
/// <remarks>
///     This allocator implementation is very, very limited and trivial; it's made
///     for specific use case of reusing memory that was rendered unused after
///     replacement in <see cref="CpkBinder._initializeLibraryHook" />; and only has
///     usually only 1 allocation. (In some rare cases, a few if multiple bind prefixes exist
///     or game has multiple binders)
/// </remarks>
public class SimpleNativeLinkedListAllocator
{
    /// <summary>
    /// Returns true if the allocator points to null.
    /// </summary>
    public unsafe bool IsNull => _startOfBuffer == null;
    
    /// <summary>
    ///     Start of the buffer in memory.
    /// </summary>
    private readonly unsafe byte* _startOfBuffer;

    /// <summary>
    ///     First item in this linked list.
    /// </summary>
    private unsafe LinkedListItem* _first;

    /// <summary>
    ///     Size of the buffer.
    /// </summary>
    private readonly int _bufferSize;

    /// <summary>
    ///     Address of the end of the buffer.
    /// </summary>
    private unsafe LinkedListItem* EndOfBuffer => (LinkedListItem*)(_startOfBuffer + _bufferSize);
    
    /// <summary />
    /// <param name="memoryPtr">Address of free memory region.</param>
    /// <param name="memorySize">Size of memory region.</param>
    public unsafe SimpleNativeLinkedListAllocator(void* memoryPtr, int memorySize)
    {
        _startOfBuffer = (byte*)memoryPtr;
        _first = (LinkedListItem*)memoryPtr;
        _bufferSize = memorySize;
    }

    /// <summary>
    ///     Allocates some memory within the linked list.
    /// </summary>
    /// <param name="size">Amount of bytes to allocate.</param>
    /// <returns>Address of allocated item, null if allocation was not possible.</returns>
    public unsafe void* Allocate(int size)
    {
        var current = _first;

        // Special case for start of the buffer.
        // Check if available space and insert new item.
        var startOfBufferItem = (LinkedListItem*)_startOfBuffer;
        if (!startOfBufferItem->IsAllocated) // no item at start
        {
            var spaceAvailable = (byte*)_first - _startOfBuffer - sizeof(LinkedListItem);
            if (size <= spaceAvailable)
            {
                var newItem = startOfBufferItem;
                newItem->Size = size;
                newItem->Next = _first;
                _first = newItem;
                return LinkedListItem.GetDataAddress(newItem);
            }
        }

        // Allocates an item within the linked list. Either at the end of the list or
        // in a gap between existing items.
        while (current != null)
        {
            if (current->IsAllocated)
            {
                // If there's a next block, compare against the end of current block
                var potentialStart = (byte*)current + sizeof(LinkedListItem) + current->Size;
                var potentialEnd = potentialStart + sizeof(LinkedListItem) + size;

                if (potentialEnd <= (byte*)current->Next)
                {
                    var newItem = (LinkedListItem*)potentialStart;
                    newItem->Size = size;
                    newItem->Next = current->Next;
                    current->Next = newItem;
                    return LinkedListItem.GetDataAddress(newItem);
                }
            }
            else
            {
                // If this is the last block (not allocated), compare against the buffer's end
                var potentialStart = (byte*)current;
                var potentialEnd = potentialStart + sizeof(LinkedListItem) + size;

                if (potentialEnd <= (byte*)EndOfBuffer)
                {
                    var newItem = (LinkedListItem*)potentialStart;
                    newItem->Size = size;
                    newItem->Next = (LinkedListItem*)potentialEnd; // Mark as the last block
                    return LinkedListItem.GetDataAddress(newItem);
                }
            }

            current = current->Next;
        }

        return null; // No suitable memory block found
    }

    /// <summary>
    ///     Deallocates memory from the linked list
    /// </summary>
    /// <param name="dataPtr">Pointer of the data to deallocate.</param>
    public unsafe void Deallocate(void* dataPtr)
    {
        // Update the linked list to exclude the item, leaving a gap
        // The space in that gap can be re-used in future allocations

        // DataPtr should point to the data section of a LinkedListItem, so we need to get its header
        var itemToDeallocate = (LinkedListItem*)((byte*)dataPtr - sizeof(LinkedListItem));

        LinkedListItem* previous = null;
        var current = _first;

        while (current != null)
        {
            if (current == itemToDeallocate)
            {
                // Remove the item from the linked list
                if (previous == null)
                {
                    // It's the first item
                    _first = current->Next;
                }
                else
                {
                    previous->Next = current->Next;
                }

                // Reset the state of the deallocated item.
                current->Size = 0;
                current->Next = null;
                break;
            }

            previous = current;
            current = current->Next;
        }
    }

    /// <summary>
    ///     Clears the entire buffer through zero-ing it.
    /// </summary>
    public unsafe void Clear()
    {
        Unsafe.InitBlockUnaligned(_startOfBuffer, 0, (uint)_bufferSize);
    }
}

/// <summary>
///     Individual native item used by the allocator.
/// </summary>
public struct LinkedListItem
{
    /// <summary>
    ///     Pointer of the next item in the chain.
    /// </summary>
    public unsafe LinkedListItem* Next;

    /// <summary>
    ///     Size of this item.
    /// </summary>
    public int Size;

    /// <summary>
    ///     True is an item is allocated here, else false.
    /// </summary>
    public unsafe bool IsAllocated => Next != (LinkedListItem*)0;

    /// <summary>
    ///     Returns the address of raw data in this linked list item.
    /// </summary>
    /// <returns>Address of the raw data.</returns>
    public static unsafe void* GetDataAddress(LinkedListItem* @this)
    {
        return (byte*)@this + sizeof(LinkedListItem);
    }
}