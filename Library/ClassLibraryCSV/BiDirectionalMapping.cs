using System;
using System.Collections.Generic;

namespace CsvTools
{
  public class BiDirectionalDictionary<TKey, TValue> : Dictionary<TKey, TValue>
  {
    private readonly IDictionary<TValue, TKey> m_SecondToFirst;

    public BiDirectionalDictionary() => m_SecondToFirst = new Dictionary<TValue, TKey>();

    public BiDirectionalDictionary(int capacity) : base(capacity) => m_SecondToFirst = new Dictionary<TValue, TKey>(capacity);

    public BiDirectionalDictionary(IDictionary<TKey, TValue> dictionary)
    {
      m_SecondToFirst = new Dictionary<TValue, TKey>(dictionary.Count);
      foreach (var keyValuePair in dictionary)
        Add(keyValuePair.Key, keyValuePair.Value);
    }

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
    /// Tries to add the pair to the dictionary.
    /// Returns false if either element is already in the dictionary
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
    /// Find the TFirst corresponding to the TSecond value.
    /// Returns false if value is not in the dictionary.
    /// </summary>
    /// <param name="value">the key to search for</param>
    /// <param name="key">the corresponding value</param>
    /// <returns>true if value is in the dictionary, false otherwise</returns>
    public bool TryGetByValue(TValue value, out TKey key) => m_SecondToFirst.TryGetValue(value, out key);

    /// <summary>
    /// Find the TFirst corresponding to the Second value.
    /// Throws an exception if value is not in the dictionary.
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
    /// Removes all items from the dictionary.
    /// </summary>
    public new void Clear()
    {
      base.Clear();
      m_SecondToFirst.Clear();
    }
  }
}