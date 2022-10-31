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

namespace CsvTools
{
  /// <summary>
  ///   Collection of Columns, this class is not serializable
  /// </summary>
  public sealed class ColumnCollection : UniqueObservableCollection<IColumn>
  {
    /// <summary>
    ///   Adds the <see cref="IColumn" /> as <see cref="ImmutableColumn" />
    /// </summary>
    /// <remarks>If the column name already exist it does nothing</remarks>
    /// <param name="column">The column format.</param>
    public new void Add(IColumn column)
    {
      // Store ImmutableColumns only since Immutable column is not ICloneable Add will not create a copy.
      if (column is null ||string.IsNullOrEmpty(column.Name))
        throw new ArgumentException("The name of a column can not be empty in the collection", nameof(column));

      base.Add(column.ToImmutableColumn());
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
        Items.Insert(index, column.ToImmutableColumn());
        OnCollectionChanged(
          new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized
            .NotifyCollectionChangedAction.Reset));
      }
      else
      {
        Add(column);
      }
    }

    /// <summary>
    /// Gets the specified column by its name.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <returns><c>null</c> if the column is not found, otherwise the column with that name</returns>
    public IColumn? GetByName(string? columnName)
    {
      if (columnName is null || columnName.Length == 0)
        return null;
      var index = GetIndexByIdentifier(columnName.IdentifierHash());
      return index == -1 ? null : Items[index];
    }

  }
}