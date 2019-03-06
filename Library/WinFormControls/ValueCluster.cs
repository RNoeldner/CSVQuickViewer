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
using System.Diagnostics.Contracts;

namespace CsvTools
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

  /// <summary>
  ///   A representation for a group / cluster of records
  /// </summary>
  public class ValueCluster : IEquatable<ValueCluster>, ICloneable<ValueCluster>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  {
    private string m_Display = string.Empty;
    private string m_Sort;
    private string m_SQLCondition = string.Empty;

    /// <summary>
    ///   Initializes a new instance of the <see cref="ValueCluster" /> class.
    /// </summary>
    public ValueCluster()
    {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="ValueCluster" /> class.
    /// </summary>
    /// <param name="display">The display.</param>
    /// <param name="count">The count.</param>
    public ValueCluster(string display, int count)
    {
      Display = display;
      Count = count;
      Active = false;
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
    public string Sort
    {
      get => m_Sort ?? Display;
      set => m_Sort = value ?? string.Empty;
    }

    /// <summary>
    ///   Gets or sets the displayed text
    /// </summary>
    /// <value>
    ///   The display.
    /// </value>
    public string Display
    {
      get => m_Display;
      set => m_Display = value ?? string.Empty;
    }

    /// <summary>
    ///   Gets or sets the parent.
    /// </summary>
    /// <value>
    ///   The parent.
    /// </value>
    public ValueCluster Parent { get; set; }

    /// <summary>
    ///   Gets or sets the SQL condition to get a list of the records
    /// </summary>
    /// <value>
    ///   The SQL condition.
    /// </value>
    public string SQLCondition
    {
      get => m_SQLCondition;
      set => m_SQLCondition = value ?? string.Empty;
    }

    /// <summary>
    ///   Clones this instance into a new instance of the same type
    /// </summary>
    /// <returns></returns>
    public ValueCluster Clone()
    {
      Contract.Ensures(Contract.Result<ValueCluster>() != null);
      var other = new ValueCluster();
      CopyTo(other);
      return other;
    }

    /// <summary>
    ///   Copies all properties to the other instance
    /// </summary>
    /// <param name="other">The other instance</param>
    public void CopyTo(ValueCluster other)
    {
      if (other == null)
        return;
      other.SQLCondition = SQLCondition;
      other.Display = Display;
      other.Count = Count;
      other.Parent = Parent;
      other.Active = Active;
    }

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise,
    ///   <see langword="false" />.
    /// </returns>
    public bool Equals(ValueCluster other)
    {
      if (other is null) return false;
      if (ReferenceEquals(this, other)) return true;
      return string.Equals(Display, other.Display, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(Sort, other.Sort, StringComparison.Ordinal) && string.Equals(SQLCondition, other.SQLCondition, StringComparison.OrdinalIgnoreCase) &&
             Active == other.Active && Count == other.Count && Equals(Parent, other.Parent);
    }

    /// <summary>
    ///   Return a string representation of this cluster
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return $"{Display ?? "[empty]"} {Count} {(Count == 1 ? "item" : "items")}";
    }

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object. </param>
    /// <returns>
    ///   <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object obj)
    {
      if (obj is null) return false;
      if (ReferenceEquals(this, obj)) return true;
      return (obj is ValueCluster typed) && Equals(typed);
    }

    /*
    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = m_Display.GetHashCode();
        hashCode = (hashCode * 397) ^ (m_Sort != null ? m_Sort.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ m_SQLCondition.GetHashCode();
        hashCode = (hashCode * 397) ^ Active.GetHashCode();
        hashCode = (hashCode * 397) ^ Count;
        return hashCode;
      }
    }
    */
  }
}