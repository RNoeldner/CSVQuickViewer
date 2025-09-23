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
namespace CsvTools
{
  /// <inheritdoc />
  /// <summary>
  ///   Property Changed Event Argument providing information of old and new value
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class PropertyChangedEventArgs<T> : System.ComponentModel.PropertyChangedEventArgs
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="T:CsvTools.PropertyChangedEventArgs`1" /> class.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    public PropertyChangedEventArgs(string propertyName, T oldValue, T newValue) : base(propertyName)
    {
      OldValue = oldValue;
      NewValue = newValue;
    }

    /// <summary>
    ///   Gets or sets the new value.
    /// </summary>
    /// <value>The new value.</value>
    public T NewValue { get; set; }

    /// <summary>
    ///   Gets or sets the old value.
    /// </summary>
    /// <value>The old value.</value>
    public T OldValue { get; set; }
  }
}
