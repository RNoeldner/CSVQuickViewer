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

namespace CsvTools;

/// <summary>
///   A representation for a group / cluster of records
///   TODO: Check if we should make it generic over T for Start/End
/// </summary>
public sealed class ValueCluster : IEquatable<ValueCluster>
{
  /// <summary>
  /// Initializes a new instance of the <see cref="ValueCluster"/> class.
  /// </summary>
  /// <param name="display">The text displayed for the value in the UI or reports.</param>
  /// <param name="condition">The SQL condition to be applied for filtering this value.</param>
  /// <param name="count">The number of records that have this value.</param>
  /// <param name="start">The lower bound of the cluster range. Should be set if HasEnclosingCluster is used non string types</param>
  /// <param name="end">The upper bound of the cluster range. Can be <c>null</c> if unbounded.</param>
  public ValueCluster(string display, string condition, int count, object? start, object? end)
  {
    Display = display;
    SQLCondition = condition;
    Count = count;
    Start = start;
    End = end;
  }

  public ValueCluster(string display, string condition, int count)
     : this(display, condition, count, string.Empty, string.Empty)
  {
  }

  /// <summary>
  ///   Gets or sets the number of records that this cluster contains.
  /// </summary>
  /// <value>The count.</value>
  public int Count
  {
    get;
  }

  /// <summary>
  ///   Gets or sets the displayed text
  /// </summary>
  /// <value>The display.</value>
  public string Display
  {
    get;
  }

  public object? End { get; }

  /// <summary>
  ///   Gets or sets the SQL condition to get a list of the records
  /// </summary>
  /// <value>The SQL condition.</value>
  public string SQLCondition
  {
    get;
  }

  public object? Start { get; }

  /// <summary>
  ///   Indicates whether the current object is equal to another object of the same type.
  /// </summary>
  /// <param name="other">An object to compare with this object.</param>
  /// <returns>
  ///   <see langword="true" /> if the current object is equal to the <paramref name="other" />
  ///   parameter; otherwise, <see langword="false" />.
  /// </returns>
  public bool Equals(ValueCluster? other)
  {
    if (other is null)
      return false;

    return string.Equals(Display, other.Display, StringComparison.OrdinalIgnoreCase)
           && string.Equals(SQLCondition, other.SQLCondition, StringComparison.OrdinalIgnoreCase);
  }

  /// <summary>
  ///   Determines whether the specified object is equal to the current object.
  /// </summary>
  /// <param name="obj">The object to compare with the current object.</param>
  /// <returns>
  ///   <see langword="true" /> if the specified object is equal to the current object; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  public override bool Equals(object? obj) => Equals(obj as ValueCluster);

  public override int GetHashCode()
  {
    unchecked
    {
      return (StringComparer.OrdinalIgnoreCase.GetHashCode(Display) * 397) ^
             StringComparer.OrdinalIgnoreCase.GetHashCode(SQLCondition);
    }
  }

  /// <summary>
  ///   Return a string representation of this cluster
  /// </summary>
  /// <returns></returns>
  public override string ToString() => $"{Display} ({Count:N0} {(Count == 1 ? "item" : "items")})";
}