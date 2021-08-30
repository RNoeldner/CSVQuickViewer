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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CsvTools
{
  public sealed class ColumnCollection : ObservableCollection<IColumn>,
                                         ICloneable<ColumnCollection>,
                                         IEquatable<ColumnCollection>
  {
    public ColumnCollection()
    {
    }

    public ColumnCollection(IEnumerable<IColumn>? items)
    {
      if (items is null) return;
      foreach (var col in items)
        Add(col);
    }

    /// <summary>
    ///   Adds the <see cref="IColumn" /> to the column list if it does not exist yet
    /// </summary>
    /// <remarks>
    ///   If the column name already exist it does nothing but return the already defined column
    /// </remarks>
    /// <param name="column">The column format.</param>
    public new void Add(IColumn? column)
    {
      if (column is null)
        throw new ArgumentNullException(nameof(column));
      var index = GetIndex(column.Name);
      if (index != -1) return;
      base.Add(column is ImmutableColumn immutableColumn ? immutableColumn : new ImmutableColumn(column));
    }

    /// <summary>
    ///   Clones this instance into a new instance of the same type
    /// </summary>
    /// <returns></returns>
    public ColumnCollection Clone()
    {
      var newColumnCollection = new ColumnCollection();
      CopyTo(newColumnCollection);
      return newColumnCollection;
    }

    public void CopyFrom(IEnumerable<IColumn>? items)
    {
      CheckReentrancy();
      ClearItems();
      if (items is null) return;
      foreach (var col in items)
        Add(col);
    }

    /// <summary>
    ///   Copies all properties to the other instance
    /// </summary>
    /// <param name="other">The other instance</param>
    public void CopyTo(ColumnCollection other) => Items.CollectionCopy(other);

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
    /// </returns>
    public bool Equals(ColumnCollection? other) => Items.CollectionEqual(other);

    /// <summary>
    ///   Gets the <see cref="CsvTools.IColumn" /> with the specified field name.
    /// </summary>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    /// <value>The column format found by the given name, <c>NULL</c> otherwise</value>
    public IColumn? Get(string? fieldName)
    {
      if (string.IsNullOrEmpty(fieldName)) return null;
      var index = GetIndex(fieldName!);
      if (index == -1) return null;
      return Items[index];
    }

    public int GetIndex(string colName)
    {
      for (var index = 0; index < Items.Count; index++)
        if (string.Equals(Items[index].Name, colName, StringComparison.OrdinalIgnoreCase))
          return index;
      return -1;
    }

    public void Replace(IColumn column)
    {
      if (column is null)
        throw new ArgumentNullException(nameof(column));

      var index = GetIndex(column.Name);

      if (index != -1)
      {
        Items.RemoveAt(index);
        Items.Insert(index, column is ImmutableColumn immutableColumn ? immutableColumn : new ImmutableColumn(column));
      }
    }
  }
}