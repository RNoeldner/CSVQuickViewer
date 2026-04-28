using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CsvTools;

/// <summary>
/// A high-performance buffer that stores all columns of a single row in a contiguous memory block.
/// Supports out-of-order writes (Upsert), character-by-character building (Append), and direct Span access.
/// </summary>
public sealed class RowColumnsBuffer : ICollection<string>, IDisposable
{
  public static readonly RowColumnsBuffer Empty = new RowColumnsBuffer(0);
  private readonly int[] m_EndOffsets;
  private char[] m_ArrayPool;
  private int m_ColumnCount;
  private bool m_IsDisposed;
  private int m_TotalLength;
  /// <summary>
  /// Initializes a new instance of the <see cref="RowColumnsBuffer"/> class.
  /// </summary>
  /// <param name="initialColumnCount">Initial capacity for the number of columns.</param>
  public RowColumnsBuffer(int initialColumnCount)
  {
    if (initialColumnCount < 0) 
      throw new ArgumentOutOfRangeException(nameof(initialColumnCount));
    m_ArrayPool = ArrayPool<char>.Shared.Rent(256);
    m_IsDisposed = false;
    m_EndOffsets = new int[initialColumnCount];
  }

  /// <inheritdoc/>
  public int Count => m_ColumnCount;

  /// <inheritdoc/>
  public bool IsReadOnly => false;

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
    ThrowMaxCapacity();
    CheckDisposed();
    EnsureBufferCapacity(value.Length);
    value.CopyTo(m_ArrayPool.AsSpan(m_TotalLength));
    m_TotalLength += value.Length;
    AddEmpty();
  }

  /// <inheritdoc/>
  public void Add(string item) => Add(item.AsSpan());


  /// <inheritdoc/>
  public void Clear()
  {
    m_ColumnCount = 0;
    m_TotalLength = 0;
  }

  /// <inheritdoc/>
  public bool Contains(string item) => throw new NotSupportedException();
  // => Enumerable.Range(0, m_ColumnCount).Any(i => GetSpan(i).SequenceEqual(item.AsSpan()));

  /// <inheritdoc/>
  public void CopyTo(string[] array, int arrayIndex)
  {
    for (int i = 0; i < m_ColumnCount; i++) array[arrayIndex + i] = this[i];
  }

  /// <inheritdoc/>
  public void Dispose()
  {
    if (m_IsDisposed) return;
    ArrayPool<char>.Shared.Return(m_ArrayPool);
    m_IsDisposed = true;
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
    if (index < 0 || index >= m_ColumnCount) return ReadOnlySpan<char>.Empty;

    int start = (index == 0) ? 0 : m_EndOffsets[index - 1];
    int length = m_EndOffsets[index] - start;
    return m_ArrayPool.AsSpan(start, length);
  }

  /// <inheritdoc/>
  public bool Remove(string item) => throw new NotSupportedException();

  /// <summary>
  /// Updates an existing column or inserts new columns at the specified index, filling gaps with empty fields.
  /// Useful for out-of-order data like JSON fields or column merging.
  /// </summary>
  /// <param name="index">The zero-based index of the column to set.</param>
  /// <param name="value">The text value to place in that column.</param>
  public void Upsert(int index, ReadOnlySpan<char> value)
  {
    CheckDisposed();
    if (index < 0 || index >= m_EndOffsets.Length) throw new ArgumentOutOfRangeException(nameof(index));

    if (index >= m_ColumnCount)
    {
      while (m_ColumnCount < index)
        AddEmpty();
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
      EnsureBufferCapacity(lengthDelta > 0 ? lengthDelta : 0);

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
  public void Upsert(int index, string value) => Upsert(index, value.AsSpan());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void CheckDisposed()
  {
    if (m_IsDisposed) throw new ObjectDisposedException(nameof(RowColumnsBuffer));
  }

  private void EnsureBufferCapacity(int additionalLength)
  {
    int minCapacity = m_TotalLength + additionalLength;
    if (minCapacity > m_ArrayPool.Length)
    {
      int newSize = Math.Max(m_ArrayPool.Length * 2, minCapacity);
      char[] newBuffer = ArrayPool<char>.Shared.Rent(newSize);
      Array.Copy(m_ArrayPool, newBuffer, m_TotalLength);
      ArrayPool<char>.Shared.Return(m_ArrayPool);
      m_ArrayPool = newBuffer;
    }
  }

  /// <summary>
  /// Finalizes the characters appended as new column.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AddEmpty()
      => m_EndOffsets[m_ColumnCount++] = m_TotalLength;

  private void ThrowMaxCapacity()
  {
    if (m_ColumnCount >= m_EndOffsets.Length)
      throw new InvalidOperationException($"Maximum column capacity ({m_EndOffsets.Length}) reached.");
  }
}