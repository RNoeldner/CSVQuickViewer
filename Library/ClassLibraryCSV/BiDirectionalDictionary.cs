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
using System.Xml.Serialization;

namespace CsvTools
{
  [XmlRoot("dictionary")]
  public class BiDirectionalDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    where TKey : notnull where TValue : notnull
  {
    #region IXmlSerializable Members

    public System.Xml.Schema.XmlSchema? GetSchema() => null;

    public void ReadXml(System.Xml.XmlReader reader)
    {
      var keySerializer = new XmlSerializer(typeof(TKey));
      var valueSerializer = new XmlSerializer(typeof(TValue));

      var wasEmpty = reader.IsEmptyElement;
      reader.Read();

      if (wasEmpty)
        return;

      while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
      {
        reader.ReadStartElement("item");
        var key = (TKey) keySerializer.Deserialize(reader);
        var value = (TValue) valueSerializer.Deserialize(reader);
        Add(key, value);
        reader.ReadEndElement();
        reader.MoveToContent();
      }
      reader.ReadEndElement();
    }

    public void WriteXml(System.Xml.XmlWriter writer)
    {
      var keySerializer = new XmlSerializer(typeof(TKey));
      var valueSerializer = new XmlSerializer(typeof(TValue));

      foreach (var key in Keys)
      {
        writer.WriteStartElement("item");
        keySerializer.Serialize(writer, key);
        valueSerializer.Serialize(writer, this[key]);
        writer.WriteEndElement();
      }
    }

    #endregion


    private readonly IDictionary<TValue, TKey> m_SecondToFirst;

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
    ///   A <see cref="T:System.Collections.Generic.IDictionary`2" />, that will copied to the new class
    /// </param>
    /// <exception cref="T:System.ArgumentException">Duplicate key - key or Duplicate value - value</exception>
    public BiDirectionalDictionary(in IDictionary<TKey, TValue>? dictionary)
    {
      m_SecondToFirst = new Dictionary<TValue, TKey>(dictionary?.Count ?? 0);
      if (dictionary is null) return;
      foreach (var keyValuePair in dictionary)
        Add(keyValuePair.Key, keyValuePair.Value);
    }

    /// <summary>
    ///   Adds a key / value to the dictionary
    /// </summary>
    /// <param name="key">The key of the dictionary.</param>
    /// <param name="value">the value for the key, there can not be two keys with the same value</param>
    /// <exception cref="ArgumentException">Duplicate key - key or Duplicate value - value</exception>
    public void Add(in TKey key, in TValue value)
    {
      if (ContainsKey(key))
        throw new ArgumentException("Duplicate key", nameof(key));
      if (m_SecondToFirst.ContainsKey(value))
        throw new ArgumentException("Duplicate value", nameof(value));

      base.Add(key, value);
      m_SecondToFirst.Add(value, key);
    }

    public new void Remove(TKey key)
    {
      m_SecondToFirst.Remove(base[key]);
      base.Remove(key);
    }

    /// <summary>
    ///   Removes all items from the dictionary.
    /// </summary>
    public new void Clear()
    {
      m_SecondToFirst.Clear();
      base.Clear();
    }

    /// <summary>
    ///   Find the TFirst corresponding to the Second value. Throws an exception if value is not in
    ///   the dictionary.
    /// </summary>
    /// <param name="value">the key to search for</param>
    /// <returns>the value corresponding to value</returns>
    public TKey GetByValue(in TValue value)
    {
      if (!m_SecondToFirst.TryGetValue(value, out var key))
        throw new ArgumentException(nameof(value));

      return key;
    }

    /// <summary>
    ///   Tries to add the pair to the dictionary. Returns false if either element is already in the dictionary
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns>true if successfully added, false if either element are already in the dictionary</returns>
    public bool TryAdd(in TKey key, in TValue value)
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
    public bool TryGetByValue(in TValue value, out TKey key) => m_SecondToFirst.TryGetValue(value, out key);
  }
}