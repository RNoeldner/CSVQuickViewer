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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace CsvTools;

/// <summary>
/// A high-performance, composition-based bidirectional dictionary mapping keys to values and values to keys.
/// Optimized for zero-allocation lookups and JIT method inlining.
/// </summary>
/// <typeparam name="TKey">The type of the keys, must be non-null and unique.</typeparam>
/// <typeparam name="TValue">The type of the values, must be non-null and unique.</typeparam>
/// <remarks>This collection is not thread-safe.</remarks>
[DebuggerDisplay("Count = {Count}")]
public sealed class BiDirectionalDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
  where TKey : notnull
  where TValue : notnull
{
  private readonly Dictionary<TKey, TValue> m_Forward;
  private readonly Dictionary<TValue, TKey> m_Reverse;

  /// <summary>
  /// Initializes a new instance of the <see cref="BiDirectionalDictionary{TKey, TValue}"/> class that is empty.
  /// </summary>
  public BiDirectionalDictionary()
  {
    m_Forward = new Dictionary<TKey, TValue>();
    m_Reverse = new Dictionary<TValue, TKey>();
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="BiDirectionalDictionary{TKey, TValue}"/> class that is empty and has the specified initial capacity.
  /// </summary>
  /// <param name="capacity">The initial number of elements that the dictionary can contain.</param>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 0.</exception>
  public BiDirectionalDictionary(int capacity)
  {
    m_Forward = new Dictionary<TKey, TValue>(capacity);
    m_Reverse = new Dictionary<TValue, TKey>(capacity);
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="BiDirectionalDictionary{TKey, TValue}"/> class that contains elements copied from the specified dictionary.
  /// </summary>
  /// <param name="dictionary">The dictionary whose elements are copied to the new bidirectional dictionary.</param>
  /// <exception cref="ArgumentException"><paramref name="dictionary"/> contains one or more duplicate keys or duplicate values.</exception>
  public BiDirectionalDictionary(IDictionary<TKey, TValue>? dictionary)
  {
    var capacity = dictionary?.Count ?? 0;
    m_Forward = new Dictionary<TKey, TValue>(capacity);
    m_Reverse = new Dictionary<TValue, TKey>(capacity);

    if (dictionary is null) return;
    foreach (var kv in dictionary)
      Add(kv.Key, kv.Value);
  }

  /// <summary>
  /// Gets the number of key-value pairs contained in the <see cref="BiDirectionalDictionary{TKey, TValue}"/>.
  /// </summary>
  /// <value>The number of key-value pairs contained in the dictionary.</value>
  public int Count => m_Forward.Count;

  /// <summary>
  /// Gets a collection containing the keys in the <see cref="BiDirectionalDictionary{TKey, TValue}"/>.
  /// </summary>
  public Dictionary<TKey, TValue>.KeyCollection Keys => m_Forward.Keys;

  /// <summary>
  /// Gets a collection containing the values in the <see cref="BiDirectionalDictionary{TKey, TValue}"/>.
  /// </summary>
  public Dictionary<TKey, TValue>.ValueCollection Values => m_Forward.Values;
  /// <summary>
  /// Gets the value associated with the specified key.
  /// </summary>
  /// <param name="key">The key of the value to get.</param>
  /// <returns>The value associated with the specified key.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
  /// <exception cref="KeyNotFoundException">The property is retrieved and <paramref name="key"/> does not exist in the collection.</exception>
  public TValue this[TKey key]
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => m_Forward[key];
  }

  /// <summary>
  /// Adds the specified key and value to the dictionary, ensuring uniqueness in both directions.
  /// </summary>
  /// <param name="key">The key of the element to add.</param>
  /// <param name="value">The value of the element to add.</param>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="value"/> is null.</exception>
  /// <exception cref="ArgumentException">An element with the same <paramref name="key"/> or the same <paramref name="value"/> already exists in the dictionary.</exception>
  public void Add(TKey key, TValue value)
  {
    if (m_Forward.ContainsKey(key))
      throw new ArgumentException("Duplicate key encountered.", nameof(key));
    if (m_Reverse.ContainsKey(value))
      throw new ArgumentException("Duplicate value encountered.", nameof(value));

    m_Forward.Add(key, value);
    m_Reverse.Add(value, key);
  }

  /// <summary>
  /// Removes all keys and values from the <see cref="BiDirectionalDictionary{TKey, TValue}"/>.
  /// </summary>
  public void Clear()
  {
    m_Forward.Clear();
    m_Reverse.Clear();
  }

  /// <summary>
  /// Determines whether the <see cref="BiDirectionalDictionary{TKey, TValue}"/> contains the specified key.
  /// </summary>
  /// <param name="key">The key to locate in the dictionary.</param>
  /// <returns><c>true</c> if the dictionary contains an element with the specified key; otherwise, <c>false</c>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool ContainsKey(TKey key) => m_Forward.ContainsKey(key);

  /// <summary>
  /// Determines whether the <see cref="BiDirectionalDictionary{TKey, TValue}"/> contains the specified value.
  /// </summary>
  /// <param name="value">The value to locate in the dictionary.</param>
  /// <returns><c>true</c> if the dictionary contains an element with the specified value; otherwise, <c>false</c>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool ContainsValue(TValue value) => m_Reverse.ContainsKey(value);
  /// <summary>
  /// Performs a reverse lookup to find the key associated with the specified value. Throws an exception if not found.
  /// </summary>
  /// <param name="value">The value to look up.</param>
  /// <returns>The key associated with the specified value.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
  /// <exception cref="KeyNotFoundException">The specified <paramref name="value"/> was not found in the dictionary.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
  [return: NotNull]
#endif
  public TKey GetByValue(TValue value)
  {
    if (!m_Reverse.TryGetValue(value, out var key))
    {
      ThrowKeyNotFound(value);
    }
    return key;
  }

  /// <summary>
  /// Returns an enumerator that iterates through the forward mappings of the <see cref="BiDirectionalDictionary{TKey, TValue}"/>.
  /// </summary>
  /// <returns>An enumerator structural representation that can be used to iterate through the collection.</returns>
  [IteratorStateMachine(typeof(Dictionary<,>.Enumerator))]
  public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => m_Forward.GetEnumerator();

  /// <summary>
  /// Returns an enumerator that iterates through a collection.
  /// </summary>
  /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
  IEnumerator IEnumerable.GetEnumerator() => m_Forward.GetEnumerator();

  /// <summary>
  /// Removes the element with the specified key from both directions of the dictionary.
  /// </summary>
  /// <param name="key">The key of the element to remove.</param>
  /// <returns><c>true</c> if the element is successfully found and removed; otherwise, <c>false</c>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
  public bool Remove(TKey key)
  {
    if (!m_Forward.TryGetValue(key, out var value))
      return false;

    m_Forward.Remove(key);
    m_Reverse.Remove(value);
    return true;
  }

  /// <summary>
  /// Attempts to add the specified key and value to the dictionary.
  /// </summary>
  /// <param name="key">The key of the element to add.</param>
  /// <param name="value">The value of the element to add.</param>
  /// <returns><c>true</c> if the key-value pair was added successfully; <c>false</c> if the <paramref name="key"/> or <paramref name="value"/> already exists.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="value"/> is null.</exception>
  public bool TryAdd(TKey key, TValue value)
  {
    if (m_Forward.ContainsKey(key) || m_Reverse.ContainsKey(value))
      return false;

    m_Forward.Add(key, value);
    m_Reverse.Add(value, key);
    return true;
  }
  /// <summary>
  /// Performs a reverse lookup to attempt to find the key associated with the specified value.
  /// </summary>
  /// <param name="value">The value to look up.</param>
  /// <param name="key">When this method returns, contains the key associated with the specified value, if the value is found; otherwise, the default value for the type of the key parameter. This parameter is passed uninitialized.</param>
  /// <returns><c>true</c> if the dictionary contains an element with the specified value; otherwise, <c>false</c>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool TryGetByValue(TValue value,
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    [MaybeNullWhen(false)]
#endif
    out TKey key) => m_Reverse.TryGetValue(value, out key);

  /// <summary>
  /// Gets the value associated with the specified key.
  /// </summary>
  /// <param name="key">The key of the value to get.</param>
  /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
  /// <returns><c>true</c> if the dictionary contains an element with the specified key; otherwise, <c>false</c>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool TryGetValue(TKey key,
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    [MaybeNullWhen(false)]
#endif
    out TValue value) => m_Forward.TryGetValue(key, out value);
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
  [DoesNotReturn]
#endif
  private static void ThrowKeyNotFound(TValue value)
  {
    throw new KeyNotFoundException($"The value '{value}' could not be reverse-mapped back to a valid key.");
  }
}