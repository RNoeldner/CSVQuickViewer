using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CsvTools;

/// <summary>
/// A high-performance, reusable buffer that stores all columns of a single row in a contiguous memory block.
/// Supports out-of-order writes (Upsert), dynamic column growth, and zero-allocation span access.
/// </summary>
/// <remarks>
/// This class utilizes <see cref="ArrayPool{T}"/> to minimize GC pressure. 
/// Always ensure <see cref="Dispose"/> is called to return buffers to the pool.
/// </remarks>
public sealed class RowColumnsBuffer : ICollection<string>, IReadOnlyList<string>, IDisposable
{
  private char[] m_ArrayPool;
  private int m_ColumnCount;
  private bool m_Disposed;
  private int[] m_EndOffsets;
  private int m_TotalLength;

  /// <summary>
  /// Initializes a new instance of the <see cref="RowColumnsBuffer"/> class.
  /// </summary>
  /// <param name="initialColumnCount">The initial capacity for the number of columns.</param>
  /// <param name="initialBufferCapacity">The initial capacity for the total character length of the row.</param>
  public RowColumnsBuffer(int initialColumnCount = 32, int initialBufferCapacity = 2048)
  {
    m_ArrayPool = ArrayPool<char>.Shared.Rent(initialBufferCapacity);
    m_EndOffsets = ArrayPool<int>.Shared.Rent(initialColumnCount);
  }

  /// <summary>
  /// Number of Columns
  /// </summary>
  public int Count => m_ColumnCount;

  /// <inheritdoc/>
  public bool IsReadOnly => false;

  /// <summary>
  /// Gets the total number of characters currently stored across all columns.
  /// </summary>
  public int Position => m_TotalLength;

  /// <summary>
  /// Gets the string representation of the column at the specified index.
  /// </summary>
  /// <param name="index">The zero-based index of the column.</param>
  /// <returns>The column content as a string.</returns>
  public string this[int index] => GetSpan(index).ToString();

  /// <summary>
  /// Adds a complete column value to the end of the current row.
  /// </summary>
  /// <param name="value">The text to add as a new column.</param>
  public void Add(ReadOnlySpan<char> value)
  {
    CheckDisposed();
    if (value.Length > 0)
    {
      EnsureBufferCapacity(m_TotalLength + value.Length);
      value.CopyTo(m_ArrayPool.AsSpan(m_TotalLength));
      m_TotalLength += value.Length;
    }

    NextColumn();
  }

  /// <inheritdoc/>
  public void Add(string? item) => Add(item.AsSpan());

  /// <summary>
  /// Adds a sequence of column values to the end of the current row.
  /// </summary>
  /// <param name="collection">The sequence of strings to add.</param>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="collection"/> is null.</exception>
  public void AddRange(IEnumerable<string> collection)
  {
    CheckDisposed();
    if (collection == null) throw new ArgumentNullException(nameof(collection));

    // Optimization: If we know the count, ensure we have enough offset space once.
    if (collection is IReadOnlyCollection<string> readOnly)
    {
      EnsureOffsetsCapacity(m_ColumnCount + readOnly.Count);
    }
    else if (collection is ICollection<string> col)
    {
      EnsureOffsetsCapacity(m_ColumnCount + col.Count);
    }

    foreach (string item in collection)
      Add(item);
  }

  /// <summary>
  /// Appends a single character to the latest current column being parsed.
  /// </summary>
  /// <param name="c">The character to append.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Append(char c)
  {
    EnsureBufferCapacity(m_TotalLength + 1);
    m_ArrayPool[m_TotalLength++] = c;
  }

  /// <inheritdoc/>
  public void Clear()
  {
    m_ColumnCount = 0;
    m_TotalLength = 0;
  }

  /// <inheritdoc/>
  public bool Contains(string item) =>
    Enumerable.Range(0, m_ColumnCount).Any(i => GetSpan(i).SequenceEqual(item.AsSpan()));

  /// <inheritdoc/>
  public void CopyTo(string[] array, int arrayIndex)
  {
    for (int i = 0; i < m_ColumnCount; i++) array[arrayIndex + i] = this[i];
  }

  /// <inheritdoc/>
  public void Dispose()
  {
    if (m_Disposed) return;
    ArrayPool<char>.Shared.Return(m_ArrayPool);
    ArrayPool<int>.Shared.Return(m_EndOffsets);
    m_Disposed = true;
  }

  /// <inheritdoc/>
  public IEnumerator<string> GetEnumerator()
  {
    for (int i = 0; i < m_ColumnCount; i++) yield return this[i];
  }

  /// <inheritdoc/>
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

  /// <summary>
  /// Returns the column text as a <see cref="ReadOnlySpan{Char}"/>.
  /// </summary>
  /// <param name="index">The zero-based index of the column.</param>
  /// <returns>A span representing the column text.</returns>
  public ReadOnlySpan<char> GetSpan(int index)
  {
    // CheckDisposed();
    if (index < 0 || index >= m_ColumnCount) return ReadOnlySpan<char>.Empty;
    int start = (index == 0) ? 0 : m_EndOffsets[index - 1];
    int length = m_EndOffsets[index] - start;
    return m_ArrayPool.AsSpan(start, length);
  }

  /// <summary>
  /// Finalizes the current character sequence as a discrete column and prepares the buffer for the next entry.
  /// </summary>
  /// <remarks>
  /// This method should be called after manually writing characters to the internal buffer 
  /// or when you need to represent an empty column. It records the current <see cref="Position"/> 
  /// into the offset map and increments the <see cref="Count"/>.
  /// </remarks>
  /// <exception cref="ObjectDisposedException">Thrown if the buffer has been disposed.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void NextColumn()
  {
    CheckDisposed();
    EnsureOffsetsCapacity(m_ColumnCount + 1);
    m_EndOffsets[m_ColumnCount++] = m_TotalLength;
  }

  /// <inheritdoc/>
  public bool Remove(string item) => throw new NotSupportedException();


  /// <summary>
  /// Returns the character span of the currently active (not yet finalized) column.
  /// </summary>
  /// <remarks>
  /// This span represents the data written since the end of the last finalized column
  /// (or from the start of the buffer if no columns have been finalized yet).
  /// The column is considered transient until <see cref="NextColumn"/> is called,
  /// and therefore is not included in <see cref="Count"/>.
  /// </remarks>
  /// <returns>
  /// A <see cref="ReadOnlySpan{Char}"/> over the characters of the active column.
  /// </returns>
  /// <exception cref="ObjectDisposedException">
  /// Thrown if the buffer has been disposed.
  /// </exception>
  public ReadOnlySpan<char> GetCurrentSpan()
  {
    CheckDisposed();
    int start = (m_ColumnCount == 0) ? 0 : m_EndOffsets[m_ColumnCount - 1];
    return m_ArrayPool.AsSpan(start, m_TotalLength - start);
  }

  /// <summary>
  /// Updates an existing column or inserts new columns at the specified index, filling gaps with empty fields.
  /// Useful for out-of-order data like JSON fields or column merging.
  /// </summary>
  /// <param name="index">The zero-based index of the column to set.</param>
  /// <param name="value">The text value to place in that column.</param>
  public void Upsert(int index, ReadOnlySpan<char> value)
  {
    CheckDisposed();
    if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));

    if (index >= m_ColumnCount)
    {
      while (m_ColumnCount < index)
        NextColumn();
      Add(value);
      return;
    }

    int start = (index == 0) ? 0 : m_EndOffsets[index - 1];
    int oldEnd = m_EndOffsets[index];
    int oldLength = oldEnd - start;
    int newLength = value.Length;
    int lengthDelta = newLength - oldLength;

    if (lengthDelta != 0)
    {
      EnsureBufferCapacity(m_TotalLength + lengthDelta);

      if (index < m_ColumnCount - 1)
      {
        ReadOnlySpan<char> source = m_ArrayPool.AsSpan(oldEnd, m_TotalLength - oldEnd);
        Span<char> destination = m_ArrayPool.AsSpan(oldEnd + lengthDelta);
        source.CopyTo(destination);
      }

      for (int i = index; i < m_ColumnCount; i++)
        m_EndOffsets[i] += lengthDelta;

      m_TotalLength += lengthDelta;
    }

    value.CopyTo(m_ArrayPool.AsSpan(start, newLength));
  }

  /// <summary>
  /// Updates an existing column or inserts new columns at the specified index.
  /// </summary>
  /// <param name="index">The zero-based index of the column to set.</param>
  /// <param name="value">The string value to place in that column.</param>
  public void Upsert(int index, string? value) => Upsert(index, value.AsSpan());

  /// <summary>
  /// Ensures the internal buffer can hold at least the specified number of items.
  /// </summary>
  /// <param name="currentBuffer">The current buffer to ensure capacity for.</param>
  /// <param name="requiredCapacity">The absolute minimum capacity required.</param>
  /// <param name="validCount">The number of valid items in the current buffer.</param>
  private static void EnsureCapacity<T>(ref T[] currentBuffer, int requiredCapacity, int validCount)
  {
    // Use the actual length of the rented buffer to avoid unnecessary resizing
    if (requiredCapacity > currentBuffer.Length)
    {
      // Growth strategy: Double or jump to required
      int newSize = Math.Max(currentBuffer.Length == 0 ? 256 : currentBuffer.Length * 2, requiredCapacity);
      T[] newBuffer = ArrayPool<T>.Shared.Rent(newSize);

      // Optimization: Only copy the part of the buffer that contains actual data
      if (validCount > 0)
        Array.Copy(currentBuffer, 0, newBuffer, 0, validCount);

      // Return the specific buffer passed in to the correct pool
      ArrayPool<T>.Shared.Return(currentBuffer);

      // Update the reference to point to the new rented array
      currentBuffer = newBuffer;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void CheckDisposed()
  {
    if (m_Disposed) throw new ObjectDisposedException(nameof(RowColumnsBuffer));
  }

  /// <summary>
  /// Ensures the internal character buffer can hold at least the specified number of characters.
  /// </summary>
  /// <param name="requiredCapacity">The absolute minimum capacity required.</param>
  private void EnsureBufferCapacity(int requiredCapacity)
    => EnsureCapacity(ref m_ArrayPool, requiredCapacity, m_TotalLength);

  private void EnsureOffsetsCapacity(int requiredCapacity)
    => EnsureCapacity(ref m_EndOffsets, requiredCapacity, m_ColumnCount);
}