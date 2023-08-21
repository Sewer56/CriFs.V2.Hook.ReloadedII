using CriFs.V2.Hook.Utilities.Extensions;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace CriFs.V2.Hook.Utilities;

/// <summary>
/// Provides a limited dictionary implementation with the following characteristics:
///     - String Key, Also Indexable with Span{char}.
///     - Non-threadsafe.
/// </summary>
/// <remarks>
///     This is a forked/modified version of code from Sewer's VFS.
///     This version compares non-case sensitive, and can return original key.
/// </remarks>
[ExcludeFromCodeCoverage]
public class SpanOfCharDict<T>
{
    // Note: Do not need a Remove function, for our purposes, we'll never end up using it,
    // because we will need a full rebuild on file removal at runtime.
    
    private DictionaryEntry[] _entries; // buffer of entries. Placed together for improved cache coherency.
    private int[] _buckets; // pointers to first entry in each bucket. Encoded as 1 based, so default 0 value is seen as invalid.
    
    /// <summary>
    /// Number of items stored in this dictionary.  
    /// </summary>
    public int Count { get; private set; }
    
    /// <summary>
    /// Number of items allocated in this dictionary.  
    /// This does not decrease with item removal.  
    /// </summary>
    public int Allocated { get; private set; } // also index of next entry
    
    /// <summary/>
    /// <param name="targetSize">Amount of expected items in this dictionary.</param>
    public SpanOfCharDict(int targetSize)
    {
        // Min size.
        if (targetSize > 0)
        {
            _buckets = new int[BitOperations.RoundUpToPowerOf2((uint)(targetSize))];
            _entries = new DictionaryEntry[targetSize];
            return;
        }

        _buckets = Array.Empty<int>();
        _entries = Array.Empty<DictionaryEntry>();
    }
    
    /// <summary>
    /// Clones this dictionary instance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SpanOfCharDict<T> Clone()
    {
        var result = new SpanOfCharDict<T>(Allocated);
        
        // Copy existing items.
        // Inlined: GetValues for perf. Notably because hot path; so memory saving here might be not so bad.
        int x = 0;
        int allocated = Allocated;
        while (x < allocated)
        {
            x = GetNextItemIndex(x);
            if (x == -1) 
                return result;

            ref var entry = ref _entries.DangerousGetReferenceAt(x++);
            result.AddOrReplaceWithKnownHashCode(entry.HashCode, entry.Key!, entry.Value);
        }

        return result;
    }
    
    /// <summary>
    /// Empties the contents of the dictionary.
    /// </summary>
    public void Clear()
    {
        Count = 0;
        Array.Fill(_buckets, 0);
    }

    /// <summary>
    /// Adds or replaces a specified value in the dictionary.
    /// </summary>
    /// <param name="key">The key for the dictionary.</param>
    /// <param name="value">The value to be inserted into the dictionary.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddOrReplace(string key, T value)
    {
        AddOrReplaceWithKnownHashCode(key.AsSpan().GetNonRandomizedHashCodeLower(), key, value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddOrReplaceWithKnownHashCode(nuint hashCode, string key, T value)
    {
        // Grow if needed.
        var allocated = Allocated;
        if (allocated >= _entries.Length)
            GrowDictionaryRare();
        
        ref var entryIndex = ref GetBucketEntry(hashCode);

        // No entry exists for this bucket.
        if (entryIndex <= 0)
        {
            entryIndex = allocated + 1; // Bucket entries encoded as 1 indexed.

            ref DictionaryEntry newEntry = ref _entries.DangerousGetReferenceAt(allocated);
            newEntry.Key = key;
            newEntry.Value = value;
            newEntry.HashCode = hashCode;
            // newEntry.NextItem = 0; <= not needed, since we don't support removal and will already be initialised as 0.
            Allocated = allocated + 1;
            Count++;
            return;
        }

        // Get entry.
        var keySpan = key.AsSpan();
        var index = entryIndex - 1;
        ref DictionaryEntry entry = ref Unsafe.NullRef<DictionaryEntry>();

        do
        {
            entry = ref _entries.DangerousGetReferenceAt(index);
            if (entry.HashCode == hashCode && keySpan.Equals(entry.Key.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                // Update existing entry.
                entry.Value = value;
                return;
            }

            // Reallocate previously freed spot.
            if (entry.IsFree())
            {
                entry.HashCode = hashCode;
                entry.Key = key;
                entry.Value = value;
                Count++;
                return;
            }

            index = (int)(entry.NextItem - 1);
        } while (index > 0);

        // Item is not in there, we add and exit.
        ref DictionaryEntry nextEntry = ref _entries.DangerousGetReferenceAt(allocated);
        nextEntry.Key = key;
        nextEntry.Value = value;
        nextEntry.HashCode = hashCode;
        // entry.NextItem = 0; <= not needed, since we don't support removal and will already be initialised as 0.

        // Update last in chain and total count.
        entry.NextItem = (uint)allocated + 1;
        Allocated = allocated + 1;
        Count++;
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // On a hot path but rarely call, do not inline.
    private void GrowDictionaryRare()
    {
        // Grow entries
        var newEntries = new DictionaryEntry[Math.Max(1, _entries.Length * 2)];
        _entries.AsSpan().CopyTo(newEntries);
        _entries = newEntries;
        
        // Grow and re-populate bucket.
        var bucketCount = BitOperations.RoundUpToPowerOf2((uint)(newEntries.Length));
        if (bucketCount <= _buckets.Length) 
            return;
        
        var bucket = new int[bucketCount];
        var enumerator = GetEntryEnumerator();
        int count = 0;
        while (enumerator.MoveNext())
        {
            var hash = enumerator.Current.HashCode;
            ref var entryIndex = ref GetBucketEntry(bucket, hash);
            if (entryIndex <= 0)
                entryIndex = count + 1;
            
            count++;
        }
        
        _buckets = bucket;
    }

    /// <summary>
    /// Checks if a given item is present in the dictionary.
    /// </summary>
    /// <param name="key">The key for the dictionary.</param>
    /// <returns>True if the item was found, else false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(ReadOnlySpan<char> key) => TryGetValue(key, out _, out _);

    /// <summary>
    /// Gets an item from the dictionary if present.
    /// </summary>
    /// <param name="key">The key for the dictionary.</param>
    /// <param name="value">The value to return.</param>
    /// <param name="originalKey">Original key used for the value.</param>
    /// <returns>True if the item was found, else false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(ReadOnlySpan<char> key, [MaybeNullWhen(false)] out T value, [MaybeNullWhen(false)] out string originalKey)
    {
        value = default;
        originalKey = default;
        if (_buckets.Length <= 0)
            return false;
        
        var hashCode   = key.GetNonRandomizedHashCodeLower();
        var entryIndex = GetBucketEntry(hashCode);

        // No entry exists for this bucket.
        // Note: Do not invert branch. We assume it is not taken in ASM.
        // It is written this way as entryindex <= 0 is the rare(r) case.
        if (entryIndex > 0)
        {
            var index = entryIndex - 1; // Move up here because 3 instructions below [DangerousGetReferenceAt] depends on this.
            ref DictionaryEntry entry = ref Unsafe.NullRef<DictionaryEntry>();
            var entries = _entries;

            do
            {
                entry = ref entries.DangerousGetReferenceAt(index);
                if (entry.HashCode == hashCode && key.Equals(entry.Key.AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    value = entry.Value;
                    originalKey = entry.Key!;
                    return true;
                }

                index = (int)(entry.NextItem - 1);
            } while (index > 0);

            return false;
        }

        return false;
    }

    /// <summary>
    /// Gets an item from the dictionary if present, by reference.
    /// </summary>
    /// <param name="key">The key for the dictionary.</param>
    /// <returns>
    ///    Null reference if not found, else a valid reference.
    ///    Use <see cref="Unsafe.IsNullRef{T}"/> to test.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] 
    public ref T GetValueRef(ReadOnlySpan<char> key)
    {
        // without aggressive inline, ~1% faster than runtime when string, ~1% slower on ROS<char>
        // with inline, ~15% faster.
        var hashCode   = key.GetNonRandomizedHashCodeLower();
        var entryIndex = GetBucketEntry(hashCode);

        // No entry exists for this bucket.
        // Note: Do not invert branch. We assume it is not taken in ASM.
        // It is written this way as entryindex <= 0 is the rare(r) case.
        if (entryIndex > 0)
        {
            var index = entryIndex - 1; // Move up here because 3 instructions below [DangerousGetReferenceAt] depends on this.
            ref DictionaryEntry entry = ref Unsafe.NullRef<DictionaryEntry>();
            var entries = _entries;

            do
            {
                entry = ref entries.DangerousGetReferenceAt(index);
                if (entry.HashCode == hashCode && key.Equals(entry.Key, StringComparison.OrdinalIgnoreCase))
                    return ref entry.Value;

                index = (int)(entry.NextItem - 1);
            } while (index > 0);

            return ref Unsafe.NullRef<T>();
        }

        return ref Unsafe.NullRef<T>();
    }

    /// <summary>
    /// Tries to remove an entry from the dictionary.
    /// </summary>
    /// <param name="key">The key corresponding to the entry.</param>
    /// <param name="entry">The entry that was just removed.</param>
    /// <returns>Whether the entry removal operation was successful or not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] 
    public bool TryRemoveEntry(ReadOnlySpan<char> key, out DictionaryEntry entry)
    {
        var hashCode   = key.GetNonRandomizedHashCodeLower();
        var entryIndex = GetBucketEntry(hashCode);

        // No entry exists for this bucket.
        // Note: Do not invert branch. We assume it is not taken in ASM.
        // It is written this way as entryindex <= 0 is the rare(r) case.
        if (entryIndex > 0)
        {
            var index = entryIndex - 1; // Move up here because 3 instructions below [DangerousGetReferenceAt] depends on this.
            ref DictionaryEntry curEntry  = ref Unsafe.NullRef<DictionaryEntry>();
            var entries = _entries;

            do
            {
                curEntry = ref entries.DangerousGetReferenceAt(index);
                if (curEntry.HashCode == hashCode && key.Equals(curEntry.Key, StringComparison.OrdinalIgnoreCase))
                {
                    // Return me.
                    entry = curEntry;
                    curEntry.Key = default;
                    curEntry.HashCode = default;
                    curEntry.Value = default!;
                    Count--;
                    return true;
                }
                
                index = (int)(curEntry.NextItem - 1);
            } while (index > 0);

            entry = default;
            return false;
        }

        entry = default;
        return false;
    }
    
    /// <summary>
    /// Tries to remove a value from the dictionary.
    /// </summary>
    /// <param name="key">The key to remove from the dictionary.</param>
    /// <param name="value">The value obtained from the dictionary.</param>
    /// <returns>Whether the value removal operation was successful or not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] 
    public bool TryRemoveValue(ReadOnlySpan<char> key, out T value)
    {
        var result = TryRemoveEntry(key, out var entry);
        value = entry.Value;
        return result;
    }

    /// <summary>
    /// An optimised search implementation that returns the first value in dictionary by reference.
    /// </summary>
    /// <param name="key">The key of this item.</param>
    /// <remarks>
    ///     This is intended to be used when <see cref="Allocated"/> == 1.
    ///     When this is not the case, element returned is undefined.
    /// </remarks>
    /// <returns>
    ///    Null reference if not found, else a valid reference.
    ///    Use <see cref="Unsafe.IsNullRef{T}"/> to test.
    /// </returns>
    public ref T GetFirstItem(out string? key)
    {
        int index = GetNextItemIndex(0);
        if (index != -1)
        {
            ref var entry = ref _entries.DangerousGetReferenceAt(index);
            key = entry.Key;
            return ref entry.Value;
        }

        key = default;
        return ref Unsafe.NullRef<T>();
    }

    /// <summary>
    /// Retrieves the values stored within this dictionary instance.
    /// </summary>
    public ItemEntry[] GetValues()
    {
        if (Count == 0)
            return Array.Empty<ItemEntry>();
        
        var entries = GC.AllocateUninitializedArray<ItemEntry>(Count);
        int entryIndex = 0;
        
        const int unrollFactor = 4; // for readability purposes
        int maxItem = Math.Max(Allocated - unrollFactor, 0);
        int x = 0;
        for (; x < maxItem; x += unrollFactor)
        {
            ref var x0 = ref _entries.DangerousGetReferenceAt(x);
            ref var x1 = ref _entries.DangerousGetReferenceAt(x + 1);
            ref var x2 = ref _entries.DangerousGetReferenceAt(x + 2);
            ref var x3 = ref _entries.DangerousGetReferenceAt(x + 3);

            // Remember, we are 1 indexed
            if (x0.Key != null)
                entries[entryIndex++] = new ItemEntry(x0);

            if (x1.Key != null)
                entries[entryIndex++] = new ItemEntry(x1);

            if (x2.Key != null)
                entries[entryIndex++] = new ItemEntry(x2);

            if (x3.Key != null)
                entries[entryIndex++] = new ItemEntry(x3);
        }

        // Not-unroll remainder
        int count = Allocated;
        for (; x < count; x++)
        {
            ref var x0 = ref _entries.DangerousGetReferenceAt(x);
            if (x0.Key != null)
                entries[entryIndex++] = new ItemEntry(x0);
        }

        return entries;
    }
    
    /// <summary>
    /// Gets an enumerator that exposes all values available in this dictionary instance.
    /// </summary>
    public EntryEnumerator GetEntryEnumerator()
    {
        // Note: Significant performance Ws from this.
        return new EntryEnumerator(this);
    }

    /// <inheritdoc />
    public struct EntryEnumerator : IEnumerator<DictionaryEntry>
    {
        private int CurrentIndex { get; set; }
        private SpanOfCharDict<T> Owner { get; }

        /// <inheritdoc />
        public DictionaryEntry Current { get; private set; }

        /// <summary/>
        /// <param name="owner">The dictionary that owns this enumerator.</param>
        public EntryEnumerator(SpanOfCharDict<T> owner)
        {
            Owner = owner;
            Current = default;
            CurrentIndex = default;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            while (CurrentIndex < Owner.Allocated)
            {
                CurrentIndex = Owner.GetNextItemIndex(CurrentIndex);
                
                // Hot path is no branch, hence written this way
                if (CurrentIndex != -1)
                {
                    Current = Owner._entries.DangerousGetReferenceAt(CurrentIndex++);
                    return true;
                }
                
                return false;
            }
            
            return false;
        }

        /// <inheritdoc />
        public void Reset() => CurrentIndex = 0;

        /// <inheritdoc />
        public void Dispose() { }
        object IEnumerator.Current => Current;
    }

    private int GetNextItemIndex(int x)
    {
        const int unrollFactor = 4; // for readability purposes
        int maxItem = Math.Max(Allocated - unrollFactor, 0);
        for (; x < maxItem; x += unrollFactor)
        {
            ref var x0 = ref _entries.DangerousGetReferenceAt(x);
            ref var x1 = ref _entries.DangerousGetReferenceAt(x + 1);
            ref var x2 = ref _entries.DangerousGetReferenceAt(x + 2);
            ref var x3 = ref _entries.DangerousGetReferenceAt(x + 3);

            // Remember, we are 1 indexed
            if (x0.Key != null)
                return x;

            if (x1.Key != null)
                return x + 1;

            if (x2.Key != null)
                return x + 2;

            if (x3.Key != null)
                return x + 3;
        }

        // Not-unroll remainder
        int count = Allocated;
        for (; x < count; x++)
        {
            ref var x0 = ref _entries.DangerousGetReferenceAt(x);
            if (x0.Key != null)
                return x;
        }

        return -1;
    }

    /// <summary>
    /// Gets index of first entry from bucket.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref int GetBucketEntry(int[] bucket, nuint hashCode)
    {
        return ref bucket.DangerousGetReferenceAt((int)hashCode & (bucket.Length - 1));
    }

    /// <summary>
    /// Gets index of first entry from bucket.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref int GetBucketEntry(nuint hashCode)
    {
        return ref GetBucketEntry(_buckets, hashCode);
    }
    
    /// <summary>
    /// Individual dictionary entry in the dictionary.
    /// </summary>
    public struct DictionaryEntry
    {
        /// <summary>
        /// Index of next item. 1 indexed.
        /// </summary>
        public uint NextItem;
        
        /// <summary>
        /// Full hashcode for this item key.
        /// </summary>
        public nuint HashCode;
        
        /// <summary>
        /// Key for this item.
        /// </summary>
        public string? Key;
        
        /// <summary>
        /// Value for this item.
        /// </summary>
        public T Value;

        /// <summary>
        /// Returns true if this entry is free to use for subsequent allocations.
        /// </summary>
        public bool IsFree()
        {
            return HashCode == default && Key == default;
        }
    }
    
    /// <summary>
    /// Individual item entry in this dictionary in a form suitable for returning.
    /// </summary>
    public struct ItemEntry
    {
        /// <summary>
        /// Full hashcode for this item key.
        /// </summary>
        public nuint HashCode;
        
        /// <summary>
        /// Key for this item.
        /// </summary>
        public string? Key;
        
        /// <summary>
        /// Value for this item.
        /// </summary>
        public T Value;

        /// <summary/>
        public ItemEntry(DictionaryEntry dict)
        {
            HashCode = dict.HashCode;
            Key = dict.Key;
            Value = dict.Value;
        }

        /// <summary>
        /// Returns true if this entry is free to use for subsequent allocations.
        /// </summary>
        public bool IsFree()
        {
            return HashCode == default && Key == default;
        }
    }
}