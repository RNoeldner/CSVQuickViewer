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
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CsvTools
{
  public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged, INotifyPropertyChangedString
  {
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///   Occurs when a string value property changed providing information on old and new value
    /// </summary>
    public event EventHandler<PropertyChangedEventArgs<string>>? PropertyChangedString;

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
      catch (ArgumentOutOfRangeException)
      {
        // some UI elements raise this error if the value is not valid (e.G. not in range
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
        PropertyChangedString?.Invoke(this, new PropertyChangedEventArgs<string>(propertyName, oldValue, newValue));
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
    /// <returns><c>true</c> if the value was changed</returns>
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
    ///   Overwrite properties of a class with the properties of another class, allowing usage of
    ///   readonly fields being set
    /// </summary>
    /// <param name="field">The field to be overwritten</param>
    /// <param name="value">the class with the new values, supporting a copy to</param>
    /// <param name="propertyName">The name of the property</param>
    /// <returns><c>true</c> if the value was changed</returns>
    protected bool CopyTo<T>(T field, in IWithCopyTo<T> value, [CallerMemberName] string propertyName = "")
    {
      if (value.Equals(field))
        return false;
      value.CopyTo(field);
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
    /// <returns><c>true</c> if the value was changed</returns>
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
    /// <param name="notifyValues">
    ///   If <c>true</c> the <see cref="PropertyChangedString" /> event is raised with old and new data
    /// </param>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns><c>true</c> if the value was changed</returns>
    protected bool SetField(ref string field, in string? value, StringComparison comparison, bool notifyValues = false, [CallerMemberName] string propertyName = "")
    {
      var newValue = value ?? string.Empty;
      if (field.Equals(newValue, comparison))
        return false;
      if (notifyValues && PropertyChangedString != null)
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