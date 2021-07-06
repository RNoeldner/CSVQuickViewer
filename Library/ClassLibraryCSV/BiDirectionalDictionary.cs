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

using System;
using System.Collections.Generic;

namespace CsvTools
{
  public class BiDirectionalDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull where TValue : notnull
  {
    private readonly IDictionary<TValue, TKey> m_SecondToFirst;

    /// <summary>
    ///   Initializes a new instance of the <see cref="BiDirectionalDictionary{TKey, TValue}" /> class.
    /// </summary>
    public BiDirectionalDictionary() => m_SecondToFirst = new Dictionary<TValue, TKey>();

    /// <summary>
    ///   Initializes a new instance of the <see cref="BiDirectionalDictionary{TKey, TValue}" /> class.
    /// </summary>
    /// <param name="capacity">Initial capacity.</param>
    public BiDirectionalDictionary(int capacity) : base(capacity) =>
      m_SecondToFirst = new Dictionary<TValue, TKey>(capacity);

    /// <summary>
    ///   Initializes a new instance of the <see cref="BiDirectionalDictionary{TKey, TValue}" /> class.
    /// </summary>
    /// <param name="dictionary">
    ///   A <see cref="T:System.Collections.Generic.IDictionary`2" />, that will copied to the new class
    /// </param>
    /// <exception cref="ArgumentException">Duplicate key - key or Duplicate value - value</exception>
    public BiDirectionalDictionary(IDictionary<TKey, TValue>? dictionary)
    {
      m_SecondToFirst = new Dictionary<TValue, TKey>(dictionary?.Count ?? 0);
      if (dictionary == null) return;
      foreach (var keyValuePair in dictionary)
        Add(keyValuePair.Key, keyValuePair.Value);
    }

    /// <summary>
    ///   Adds a key / value to the dictionary
    /// </summary>
    /// <param name="key">The key of the dictionary.</param>
    /// <param name="value">the value for the key, there can not be two keys with the same value</param>
    /// <exception cref="ArgumentException">Duplicate key - key or Duplicate value - value</exception>
    public new void Add(TKey key, TValue value)
    {
      if (ContainsKey(key))
        throw new ArgumentException("Duplicate key", nameof(key));
      if (m_SecondToFirst.ContainsKey(value))
        throw new ArgumentException("Duplicate value", nameof(value));

      base.Add(key, value);
      m_SecondToFirst.Add(value, key);
    }

    /// <summary>
    ///   Tries to add the pair to the dictionary. Returns false if either element is already in the dictionary
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns>true if successfully added, false if either element are already in the dictionary</returns>
    public bool TryAdd(TKey key, TValue value)
    {
      if (ContainsKey(key) || m_SecondToFirst.ContainsKey(value))
        return false;

      base.Add(key, value);
      m_SecondToFirst.Add(value, key);
      return true;
    }

    /// <summary>
    ///   Find the TFirst corresponding to the TSecond value. Returns false if value is not in the dictionary.
    /// </summary>
    /// <param name="value">the key to search for</param>
    /// <param name="key">the corresponding value</param>
    /// <returns>true if value is in the dictionary, false otherwise</returns>
    public bool TryGetByValue(TValue value, out TKey key) =>
      m_SecondToFirst.TryGetValue(value, out key);

    /// <summary>
    ///   Find the TFirst corresponding to the Second value. Throws an exception if value is not in
    ///   the dictionary.
    /// </summary>
    /// <param name="value">the key to search for</param>
    /// <returns>the value corresponding to value</returns>
    public TKey GetByValue(TValue value)
    {
      if (!m_SecondToFirst.TryGetValue(value, out var key))
        throw new ArgumentException(nameof(value));

      return key;
    }

    /// <summary>
    ///   Removes all items from the dictionary.
    /// </summary>
    public new void Clear()
    {
      m_SecondToFirst.Clear();
      base.Clear();
    }
  }
}