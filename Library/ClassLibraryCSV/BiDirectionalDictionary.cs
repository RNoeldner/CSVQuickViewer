/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
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
using System.Reflection;
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace CsvTools
{

  /// <summary>
  /// A bidirectional dictionary that maps keys to values and values to keys for fast lookups in both directions.
  /// Inherits from <see cref="Dictionary{TKey, TValue}" /> and adds reverse lookups.
  /// The keys and values cannot be null. Keys and Values are unique.
  /// </summary>
  /// <typeparam name="TKey"></typeparam>
  /// <typeparam name="TValue"></typeparam>
  /// <remarks>Adding or Removing is not thread safe</remarks>
  [DebuggerDisplay("Count = {Count}")]
  [DefaultMember("Item")]
  public sealed class BiDirectionalDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    where TKey : notnull where TValue : notnull
  {


    /// <summary>
    /// Dictionary mapping values to keys for fast lookups of keys from values.
    /// </summary>
    private readonly Dictionary<TValue, TKey> m_SecondToFirst;

    /// <inheritdoc />
    /// <summary>
    ///   Initializes a new instance of the <see cref="T:CsvTools.BiDirectionalDictionary`2" /> class.
    /// </summary>
    public BiDirectionalDictionary() => m_SecondToFirst = new Dictionary<TValue, TKey>();

    /// <inheritdoc />
    /// <summary>
    ///   Initializes a new instance of the <see cref="T:CsvTools.BiDirectionalDictionary`2" /> class.
    /// </summary>
    /// <param name="capacity">Initial capacity.</param>
    public BiDirectionalDictionary(int capacity)
      : base(capacity) =>
      m_SecondToFirst = new Dictionary<TValue, TKey>(capacity);

    /// <inheritdoc />
    /// <summary>
    ///   Initializes a new instance of the <see cref="T:CsvTools.BiDirectionalDictionary`2" /> class.
    /// </summary>
    /// <param name="dictionary">
    ///   A <see cref="T:System.Collections.Generic.IDictionary`2" />, that will copy to the new class
    /// </param>
    /// <exception cref="T:System.ArgumentException">Duplicate key - key or Duplicate value - value</exception>
    public BiDirectionalDictionary(in IDictionary<TKey, TValue>? dictionary)
    {
      m_SecondToFirst = new Dictionary<TValue, TKey>(dictionary?.Count ?? 0);
      if (dictionary is null) return;
      foreach (var keyValuePair in dictionary)
        Add(keyValuePair.Key, keyValuePair.Value);
    }

    /// <inheritdoc cref="Dictionary{TKey,TValue}" />
    /// <remarks>This is not thread safe</remarks>
    public void Add(in TKey key, in TValue value)
    {
      if (ContainsKey(key))
        throw new ArgumentException("Duplicate key", nameof(key));
      if (m_SecondToFirst.ContainsKey(value))
        throw new ArgumentException("Duplicate value", nameof(value));

      base.Add(key, value);
      m_SecondToFirst.Add(value, key);
    }

    /// <inheritdoc cref="Dictionary{TKey,TValue}" />
    public new void Remove(TKey key)
    {
      m_SecondToFirst.Remove(base[key]);
      base.Remove(key);
    }

    /// <inheritdoc cref="Dictionary{TKey,TValue}" />
    public new void Clear()
    {
      m_SecondToFirst.Clear();
      base.Clear();
    }

    /// <summary>
    ///   Reverse lookup. Throws an exception if value is not found
    /// </summary>
    /// <param name="value">the value to search for</param>
    /// <returns>The key corresponding to value</returns>
    public TKey GetByValue(in TValue value)
    {
      if (!m_SecondToFirst.TryGetValue(value, out var key))
        throw new ArgumentException(nameof(value));

      return key;
    }

    /// <summary>
    ///   Tries to add the pair to the dictionary. Returns false if element is already in the dictionary
    /// </summary>
    /// <param name="key">The key value</param>
    /// <param name="value">The value</param>
    /// <returns>true if successfully added, false if either element are already in the dictionary</returns>
    /// <remarks>This is not thread safe</remarks>
    public bool TryAdd(in TKey key, in TValue value)
    {
      if (ContainsKey(key) || m_SecondToFirst.ContainsKey(value))
        return false;
      base.Add(key, value);
      m_SecondToFirst.Add(value, key);
      return true;
    }

    /// <summary>
    ///   Reverse lookup. Returns false if value is not found.
    /// </summary>
    /// <param name="value">the key to search for</param>
    /// <param name="key">the corresponding value</param>
    /// <returns>true if value is in the dictionary, false otherwise</returns>
    public bool TryGetByValue(in TValue value,
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      [MaybeNullWhen(false)]
#endif
      out TKey key) => m_SecondToFirst.TryGetValue(value, out key);
  }
}