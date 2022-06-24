using System;
using System.ComponentModel;

namespace CsvTools
{
  /// <summary>
  ///   Provides data for the <see langword="PropertyChanged" /> event.
  /// </summary>
  public sealed class PropertyChangedStringEventArgs : PropertyChangedEventArgs
  {
    /// <summary>
    ///   Initializes a new instance of the class.
    /// </summary>
    public PropertyChangedStringEventArgs(string propertyName, string? oldValue, string? newValue) : base(propertyName)
    {
      OldValue = oldValue ?? string.Empty;
      NewValue = newValue ?? string.Empty;
    }

    /// <summary>
    ///   The old value of the property
    /// </summary>
    public string OldValue { get; }

    /// <summary>
    ///   The old value of the property
    /// </summary>
    public string NewValue { get; }
  }
}