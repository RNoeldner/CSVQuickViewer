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

namespace CsvTools
{
  /// <summary>
  /// Collection of Columns, this class is not serializable
  /// </summary>
  public sealed class ColumnCollection : ObservableCollectionWithItemChange<IColumn>
  {
    /// <summary>
    ///   Adds the <see cref="IColumn" /> as <see cref="ImmutableColumn"/>
    /// </summary>
    /// <remarks>If the column name already exist it does nothing</remarks>
    /// <param name="column">The column format.</param>
    public new void Add(IColumn column)
    {
      // Store ImmutableColumns only since Immutable column is not ICloneable
      // Add will not create a copy.
      if (string.IsNullOrEmpty(column.Name))
        throw new ArgumentException("The name of a column can not be empty in the collection", nameof(column));

      base.Add(column as ImmutableColumn ?? new ImmutableColumn(column));
    }

    /// <summary>
    ///   Gets the <see cref="CsvTools.IColumn" /> with the specified field name.
    /// </summary>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    /// <value>The column format found by the given name, <c>NULL</c> otherwise</value>
    public IColumn? Get(string? fieldName)
    {
      if (fieldName is null) return null;
      var index = IndexOf(new ImmutableColumn(fieldName));
      return index == -1 ? null : Items[index];
    }
    
    /// <summary>
    ///   Replaces an existing column of the same name, if it does not exist it adds the column
    /// </summary>
    /// <param name="column"></param>
    public void Replace(IColumn column)
    {
      if (column is null)
        throw new ArgumentNullException(nameof(column));

      var index = IndexOf(column);
      if (index != -1)
      {
        Items.RemoveAt(index);
        Items.Insert(index, column is ImmutableColumn immutableColumn ? immutableColumn : new ImmutableColumn(column));
        OnCollectionChanged(
          new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized
            .NotifyCollectionChangedAction.Reset));
      }
      else
      {
        Add(column);
      }
    }
  }
}