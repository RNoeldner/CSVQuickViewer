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

namespace CsvTools
{
  using System;

  /// <summary>
  ///   A representation for a group / cluster of records
  /// </summary>
  public class ValueCluster
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="ValueCluster" /> class.
    /// </summary>
    /// <param name="display">The text displayed for the value.</param>
    /// <param name="condition">teh sql condition to be applied</param>
    /// <param name="sort">A text used for the  order</param>
    /// <param name="count">Number of records that do have this value</param>
    /// <param name="active">Flag indicating if teh filter for teh value is active</param>
    public ValueCluster(string display, string condition, string sort, int count = 0, bool active = false)
    {
      Display = display;
      SQLCondition = condition;
      Sort = sort ?? string.Empty;

      // These values might change later
      Count = count;
      Active = active;
    }

    /// <summary>
    ///   Gets or sets a value indicating whether this <see cref="ValueCluster" /> is active.
    /// </summary>
    /// <value>
    ///   <c>true</c> if active; otherwise, <c>false</c>.
    /// </value>
    public bool Active { get; set; }

    /// <summary>
    ///   Gets or sets the number of records that this cluster contains.
    /// </summary>
    /// <value>
    ///   The count.
    /// </value>
    public int Count { get; set; }

    /// <summary>
    ///   Gets or sets the displayed text
    /// </summary>
    /// <value>
    ///   The display.
    /// </value>
    public string Display
    {
      get;
    }

    /// <summary>
    ///   Gets or sets the displayed text
    /// </summary>
    /// <value>
    ///   The display.
    /// </value>
    public string Sort
    {
      get;
    }

    /// <summary>
    ///   Gets or sets the SQL condition to get a list of the records
    /// </summary>
    /// <value>
    ///   The SQL condition.
    /// </value>
    public string SQLCondition
    {
      get;
    }

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise,
    ///   <see langword="false" />.
    /// </returns>
    public bool Equals(ValueCluster other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return string.Equals(Display, other.Display, StringComparison.OrdinalIgnoreCase)
             && string.Equals(Sort, other.Sort, StringComparison.Ordinal)
             && string.Equals(SQLCondition, other.SQLCondition, StringComparison.OrdinalIgnoreCase)
             && Active == other.Active && Count == other.Count;
    }

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object. </param>
    /// <returns>
    ///   <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object obj)
    {
      if (obj is null)
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return obj is ValueCluster typed && GetHashCode() == typed.GetHashCode();
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = (Display != null ? Display.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (Sort != null ? Sort.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (SQLCondition != null ? SQLCondition.GetHashCode() : 0);
        return hashCode;
      }
    }

    /// <summary>
    ///   Return a string representation of this cluster
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{Display ?? "[empty]"} {Count} {(Count == 1 ? "item" : "items")}";
  }
}