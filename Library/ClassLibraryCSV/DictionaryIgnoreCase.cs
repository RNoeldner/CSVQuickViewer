using System;
using System.Collections.Generic;

namespace CsvTools;

/// <summary>
/// A dictionary wrapper with case-insensitive keys (OrdinalIgnoreCase) 
/// and optional value comparer support.
/// Ensures consistent equality and hashing for both keys and values.
/// </summary>
/// <typeparam name="TValue">
/// The type of values stored in the dictionary.
/// If TValue is string and no comparer is supplied, StringComparer.OrdinalIgnoreCase is used.
/// </typeparam>
public class DictionaryIgnoreCase<TValue> : Dictionary<string, TValue>, IEquatable<DictionaryIgnoreCase<TValue>>
{
  /// <summary>
  /// Comparer used for value equality and hash code generation.
  /// Automatically OrdinalIgnoreCase for string values if no comparer is supplied.
  /// </summary>
  private readonly IEqualityComparer<TValue> m_ValueComparer;


  /// <summary>
  /// Initializes an empty dictionary with case-insensitive keys.
  /// Optionally accepts a custom comparer for TValue.
  /// </summary>
  public DictionaryIgnoreCase() : base(StringComparer.OrdinalIgnoreCase)
  {
    m_ValueComparer = GetValueComparer(null);
  }

  /// <summary>
  /// Initializes an empty dictionary with case-insensitive keys.
  /// Optionally accepts a custom comparer for TValue.
  /// </summary>
  public DictionaryIgnoreCase(int capacity) : base(capacity, StringComparer.OrdinalIgnoreCase)
  {
    m_ValueComparer = GetValueComparer(null);
  }

  /// <summary>
  /// Initializes a dictionary from an existing collection.
  /// Copies all key-value pairs into a case-insensitive dictionary.
  /// </summary>
  /// <param name="collection">Source dictionary (can be null).</param>
  /// <param name="valueComparer">
  /// Optional equality comparer for TValue. 
  /// If null and TValue is string, uses OrdinalIgnoreCase.
  /// </param>
  public DictionaryIgnoreCase(IDictionary<string, TValue>? collection, IEqualityComparer<TValue>? valueComparer = null)
    : base(StringComparer.OrdinalIgnoreCase)
  {
    m_ValueComparer = GetValueComparer(valueComparer);
    if (collection != null)
    {
      foreach (var kvp in collection)
      {
        this[kvp.Key] = kvp.Value;
      }
    }
  }

  /// <summary>
  /// Determines the value comparer to use.
  /// Returns the supplied comparer if provided, 
  /// otherwise OrdinalIgnoreCase for string, or default for other types.
  /// </summary>
  private static IEqualityComparer<TValue> GetValueComparer(IEqualityComparer<TValue>? valueComparer = null)
  {
    if (valueComparer != null)
      return valueComparer;
    if (typeof(TValue) == typeof(string))
    {
      // Cast required to satisfy generic type
      return (IEqualityComparer<TValue>) StringComparer.OrdinalIgnoreCase;
    }
    return EqualityComparer<TValue>.Default;
  }

  /// <summary>
  /// Object-based equality override.
  /// Returns true if the object is a DictionaryIgnoreCase with matching key-value pairs.
  /// </summary>
  public bool Equals(DictionaryIgnoreCase<TValue>? other)
  {
    if (other is null || Count != other.Count)
      return false;

    foreach (var kvp in this)
    {
      if (!other.TryGetValue(kvp.Key, out var val) || !m_ValueComparer.Equals(kvp.Value, val))
        return false;
    }

    return true;
  }

  /// <summary>
  /// Object-based equality override.
  /// </summary>
  public override bool Equals(object? obj) => Equals(obj as DictionaryIgnoreCase<TValue>);

  /// <summary>
  /// Generates a hash code based on case-insensitive keys and values.
  /// Ensures consistency with Equals.
  /// </summary>
  public override int GetHashCode()
  {
    int hash = 0;
    foreach (var kvp in this)
    {
      int keyHash = StringComparer.OrdinalIgnoreCase.GetHashCode(kvp.Key);
      // Ensure we never pass null to GetHashCode (for reference types)
      int valueHash = kvp.Value is null ? 0 : m_ValueComparer.GetHashCode(kvp.Value);
      hash ^= keyHash ^ valueHash;
    }
    return hash;
  }

  /// <summary>
  /// Equality operator overload.
  /// </summary>
  public static bool operator ==(DictionaryIgnoreCase<TValue>? left, DictionaryIgnoreCase<TValue>? right) => Equals(left, right);

  /// <summary>
  /// Inequality operator overload.
  /// </summary>
  public static bool operator !=(DictionaryIgnoreCase<TValue>? left, DictionaryIgnoreCase<TValue>? right) => !Equals(left, right);
}