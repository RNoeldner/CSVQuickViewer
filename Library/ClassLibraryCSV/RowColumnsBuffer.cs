using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CsvTools;

/// <summary>
/// A high-performance buffer that stores all columns of a single row in a contiguous memory block.
/// Supports out-of-order writes (Upsert), character-by-character building (Append), and direct Span access.
/// </summary>
public sealed class RowColumnsBuffer : ICollection<string>, IDisposable
{
  private char[] m_ArrayPool;
  private int[] m_EndOffsets;
  private int m_ColumnCount;
  private int m_TotalLength;

  /// <summary>
  /// Initializes a new instance of the <see cref="RowColumnsBuffer"/> class.
  /// </summary>
  /// <param name="initialColumnCount">Initial capacity for the number of columns.</param>
  /// <param name="initialBufferCapacity">Initial capacity for the total character length of a row.</param>
  public RowColumnsBuffer(int initialColumnCount = 32, int initialBufferCapacity = 2048)
  {
    m_ArrayPool = ArrayPool<char>.Shared.Rent(initialBufferCapacity);
    m_EndOffsets = ArrayPool<int>.Shared.Rent(initialColumnCount);
  }

  /// <inheritdoc/>
  public int Count => m_ColumnCount;

  /// <inheritdoc/>
  public bool IsReadOnly => false;

  /// <summary>
  /// Gets the current total character length written to the internal buffer.
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
    EnsureBufferCapacity(value.Length);
    value.CopyTo(m_ArrayPool.AsSpan(m_TotalLength));
    m_TotalLength += value.Length;
    NextColumn();
  }

  /// <inheritdoc/>
  public void Add(string? item) => Add(item.AsSpan());

  /// <summary>
  /// Appends a single character to the current field being parsed.
  /// </summary>
  /// <param name="c">The character to append.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Append(char c)
  {
    if (m_TotalLength >= m_ArrayPool.Length)
      EnsureBufferCapacity(1);
    m_ArrayPool[m_TotalLength++] = c;
  }

  /// <inheritdoc/>
  public void Clear() => Reset();

  /// <inheritdoc/>
  public bool Contains(string item) => Enumerable.Range(0, m_ColumnCount).Any(i => GetSpan(i).SequenceEqual(item.AsSpan()));

  /// <inheritdoc/>
  public void CopyTo(string[] array, int arrayIndex)
  {
    for (int i = 0; i < m_ColumnCount; i++) array[arrayIndex + i] = this[i];
  }

  /// <inheritdoc/>
  public void Dispose()
  {
    if (m_ArrayPool != null)
    {
      ArrayPool<char>.Shared.Return(m_ArrayPool);
      m_ArrayPool = null!;
    }
    if (m_EndOffsets != null)
    {
      ArrayPool<int>.Shared.Return(m_EndOffsets);
      m_EndOffsets = null!;
    }
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

  /// <summary>
  /// Finalizes the characters appended via <see cref="Append"/> as a new column.
  /// </summary>
  public void NextColumn()
  {
    if (m_ColumnCount >= m_EndOffsets.Length)
      EnsureOffsetCapacity();
    m_EndOffsets[m_ColumnCount++] = m_TotalLength;
  }

  /// <summary>
  /// Removes the last specified number of characters from the current field.
  /// </summary>
  /// <param name="count">The number of characters to remove.</param>
  public void RawBackup(int count)
  {
    m_TotalLength = Math.Max(0, m_TotalLength - count);
  }

  /// <inheritdoc/>
  public bool Remove(string item) => throw new NotSupportedException();

  /// <summary>
  /// Resets the buffer for a new row. Does not release internal arrays to the pool.
  /// </summary>
  public void Reset()
  {
    m_ColumnCount = 0;
    m_TotalLength = 0;
  }

  /// <summary>
  /// Updates an existing column or inserts new columns at the specified index, filling gaps with empty fields.
  /// Useful for out-of-order data like JSON fields or column merging.
  /// </summary>
  /// <param name="index">The zero-based index of the column to set.</param>
  /// <param name="value">The text value to place in that column.</param>
  public void Upsert(int index, ReadOnlySpan<char> value)
  {
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
  public void Upsert(int index, string? value) => Upsert(index, value.AsSpan());

  private void EnsureBufferCapacity(int additionalLength)
  {
    if (m_TotalLength + additionalLength > m_ArrayPool.Length)
    {
      int newSize = Math.Max(m_ArrayPool.Length * 2, m_TotalLength + additionalLength);
      char[] newBuffer = ArrayPool<char>.Shared.Rent(newSize);
      Array.Copy(m_ArrayPool, newBuffer, m_TotalLength);
      ArrayPool<char>.Shared.Return(m_ArrayPool);
      m_ArrayPool = newBuffer;
    }
  }

  private void EnsureOffsetCapacity()
  {
    int[] newOffsets = ArrayPool<int>.Shared.Rent(m_EndOffsets.Length * 2);
    Array.Copy(m_EndOffsets, newOffsets, m_ColumnCount);
    ArrayPool<int>.Shared.Return(m_EndOffsets);
    m_EndOffsets = newOffsets;
  }
}