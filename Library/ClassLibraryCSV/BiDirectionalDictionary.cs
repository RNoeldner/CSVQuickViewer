/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */
#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace CsvTools;

/// <summary>
/// A bidirectional dictionary that maps keys to values and values to keys for fast lookups in both directions.
/// The keys and values cannot be null. Keys and Values are unique.
/// </summary>
/// <typeparam name="TKey">The type of the keys, must be non-null and unique.</typeparam>
/// <typeparam name="TValue">The type of the values, must be non-null and unique.</typeparam>
/// <remarks>This collection is not thread-safe.</remarks>
[DebuggerDisplay("Count = {Count}")]
public sealed class BiDirectionalDictionary<TKey, TValue> : Dictionary<TKey, TValue>
  where TKey : notnull
  where TValue : notnull
{
  private readonly Dictionary<TValue, TKey> m_SecondToFirst;

  /// <inheritdoc />
  /// <summary>
  ///   Initializes a new instance of the <see cref="T:CsvTools.BiDirectionalDictionary`2" /> class.
  /// </summary>
  public BiDirectionalDictionary()
  {
    m_SecondToFirst = new Dictionary<TValue, TKey>();
  }

  /// <inheritdoc />
  /// <summary>
  ///   Initializes a new instance of the <see cref="T:CsvTools.BiDirectionalDictionary`2" /> class.
  /// </summary>
  /// <param name="capacity">Initial capacity.</param>
  public BiDirectionalDictionary(int capacity) : base(capacity)
  {
    m_SecondToFirst = new Dictionary<TValue, TKey>(capacity);
  }

  /// <summary>
  /// Dictionary mapping values to keys for fast lookups of keys from values.
  /// </summary>
  public BiDirectionalDictionary(IDictionary<TKey, TValue>? dictionary)
  {
    m_SecondToFirst = new Dictionary<TValue, TKey>(dictionary?.Count ?? 0);
    if (dictionary is null) return;
    foreach (var kv in dictionary)
      Add(kv.Key, kv.Value);
  }



  /// <summary>
  /// Adds a new key-value pair, ensuring uniqueness in both directions.
  /// </summary>
  /// <exception cref="ArgumentException">If either key or value already exists.</exception>
  public new void Add(TKey key, TValue value)
  {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      if (!base.TryAdd(key, value))
        throw new ArgumentException("Duplicate key", nameof(key));
      if (m_SecondToFirst.TryAdd(value, key)) return;
      base.Remove(key);
      throw new ArgumentException("Duplicate value", nameof(value));
#else
    if (base.ContainsKey(key))
      throw new ArgumentException("Duplicate key", nameof(key));
    if (m_SecondToFirst.ContainsKey(value))
      throw new ArgumentException("Duplicate value", nameof(value));

    base.Add(key, value);
    m_SecondToFirst.Add(value, key);
#endif
  }

  /// <summary>
  /// Clears the dictionary.
  /// </summary>
  public new void Clear()
  {
    m_SecondToFirst.Clear();
    base.Clear();
  }

  /// <summary>
  /// Reverse lookup by value. Throws if not found.
  /// </summary>
  /// <exception cref="KeyNotFoundException">If value is not in the dictionary.</exception>
  public TKey GetByValue(TValue value) => !m_SecondToFirst.TryGetValue(value, out var key) ? throw new KeyNotFoundException($"The value '{value}' was not found.") : key;

  /// <summary>
  /// Removes a key and its value.
  /// </summary>
  public new bool Remove(TKey key)
  {
    if (!TryGetValue(key, out var value))
      return false;
    base.Remove(key);
    m_SecondToFirst.Remove(value);
    return true;
  }

  /// <summary>
  /// Attempts to add a pair, returns false if key or value already exists.
  /// </summary>
  public
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    new 
#endif
    bool TryAdd(TKey key, TValue value)
  {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      if (!base.TryAdd(key, value))
        return false;
      if (m_SecondToFirst.TryAdd(value, key)) return true;
      base.Remove(key);
      return false;
#else
    if (base.ContainsKey(key) || m_SecondToFirst.ContainsKey(value))
      return false;
    base.Add(key, value);
    m_SecondToFirst.Add(value, key);
    return true;
#endif
  }
  /// <summary>
  /// Reverse lookup by value. Returns false if not found.
  /// </summary>
  public bool TryGetByValue(
    TValue value,
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      [MaybeNullWhen(false)]
#endif
    out TKey key) => m_SecondToFirst.TryGetValue(value, out key);
}