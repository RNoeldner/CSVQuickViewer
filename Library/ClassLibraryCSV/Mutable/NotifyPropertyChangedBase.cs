using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CsvTools
{
  public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
  {
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///   Occurs when a string value property changed providing information on old and new value
    /// </summary>
    public event EventHandler<PropertyChangedStringEventArgs>? PropertyChangedString;

    /// <summary>
    ///   Notifies the completed property changed through <see cref="PropertyChanged" />
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
      if (PropertyChanged is null)
        return;
      try
      {
        // ReSharper disable once PolymorphicFieldLikeEventInvocation
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
      catch (TargetInvocationException)
      {
        // Ignore
      }
    }

    /// <summary>
    ///   Notifies on changed property strings
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    protected void NotifyPropertyChangedString(in string propertyName, in string oldValue, in string newValue)
    {
      try
      {
        PropertyChangedString?.Invoke(this, new PropertyChangedStringEventArgs(propertyName, oldValue, newValue));
      }
      catch (TargetInvocationException)
      {
        // Ignore
      }
    }

    /// <summary>
    ///   Sets the collection of a backing store and raises <see cref="PropertyChanged" /> after the
    ///   value is changed
    /// </summary>
    /// <param name="field">The backing store.</param>
    /// <param name="value">The new collection.</param>
    /// <param name="withOrder">
    ///   if <c>true</c> the order of the collections is important and will indicate a change
    /// </param>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns><c>true&gt;</c> if the value was changed</returns>
    protected bool SetCollection<T>(ICollection<T> field, in IEnumerable<T>? value, bool withOrder = true, [CallerMemberName] string propertyName = "") where T : IEquatable<T>
    {
      if (withOrder && field.CollectionEqualWithOrder(value))
        return false;
      if (!withOrder && field.CollectionEqual(value))
        return false;

      field.Clear();
      if (value != null)
      {
        foreach (var item in value)
        {
          if (item is ICloneable cloneable)
            field.Add((T) cloneable.Clone());
          else
            field.Add(item);
        }
      }
      NotifyPropertyChanged(propertyName);
      return true;
    }

    /// <summary>
    ///   Sets the property of a backing store and raises <see cref="PropertyChanged" /> after the
    ///   value is changed
    /// </summary>
    /// <param name="field">The backing store.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns><c>true&gt;</c> if teh value was changed</returns>
    protected bool SetField<T>(ref T field, in T value, [CallerMemberName] string propertyName = "") // where T : struct
    {
      if (EqualityComparer<T>.Default.Equals(field, value))
        return false;
      field = value;
      NotifyPropertyChanged(propertyName);
      return true;
    }

    /// <summary>
    ///   Sets the string of a backing store and raises <see cref="PropertyChanged" /> after the
    ///   value is changed
    /// </summary>
    /// <param name="field">The backing store.</param>
    /// <param name="value">The new value.</param>
    /// <param name="comparison">
    ///   Specifies the culture, case, and sort rules to be used for comparison, use <see
    ///   cref="StringComparison.OrdinalIgnoreCase" /> if not sure
    /// </param>
    /// <param name="notifyChange">
    ///   If <c>true</c> the <see cref="PropertyChangedString" /> event is raised before the data is updated
    /// </param>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns><c>true</c> if the value was changed</returns>
    protected bool SetField(ref string field, in string? value, StringComparison comparison, bool notifyChange = false, [CallerMemberName] string propertyName = "")
    {
      var newValue = value ?? string.Empty;
      if (field.Equals(newValue, comparison))
        return false;
      if (notifyChange && PropertyChangedString != null)
      {
        var oldValue = field;
        field = newValue;
        NotifyPropertyChangedString(propertyName, oldValue, newValue);
      }
      else
      {
        field = newValue;
      }

      NotifyPropertyChanged(propertyName);
      return true;
    }
  }
}