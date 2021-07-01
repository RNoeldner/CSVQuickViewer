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
using System.Collections.ObjectModel;

namespace CsvTools
{
  using System.Collections.Generic;
  using System.Linq;

  public sealed class ColumnCollection : ObservableCollection<Column>, ICloneable<ColumnCollection>,
    IEquatable<ColumnCollection>
  {
    public ColumnCollection()
    {
    }

    public ColumnCollection(IEnumerable<IColumn>? items)
    {
      if (items == null) return;
      foreach (var col in items)
        AddIfNew(col);
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

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
    /// </returns>
    public bool Equals(ColumnCollection other) => Items.CollectionEqual(other);

    public void CopyFrom(IEnumerable<IColumn>? items)
    {
      CheckReentrancy();
      ClearItems();
      if (items == null) return;
      foreach (var col in items)
        AddIfNew(col);
    }

    /// <summary>
    ///   Adds the <see cref="Column" /> format to the column list if it does not exist yet
    /// </summary>
    /// <remarks>
    ///   If the column name already exist it does nothing but return the already defined column
    /// </remarks>
    /// <param name="columnFormat">The column format.</param>
    public void AddIfNew(IColumn columnFormat)
    {
      if (columnFormat is null)
        throw new ArgumentNullException(nameof(columnFormat));
      var found = Get(columnFormat.Name);
      if (found != null) return;
      Column? toAdd = null;
      switch (columnFormat)
      {
        case ImmutableColumn cro:
          toAdd = cro.ToMutable();
          break;

        case Column col:
          toAdd = col;
          break;
      }

      if (toAdd == null)
        throw new InvalidOperationException("Implementation must be of type ImmutableColumn or Column");
      Add(toAdd);
    }

    /// <summary>
    ///   Copies all properties to the other instance
    /// </summary>
    /// <param name="other">The other instance</param>
    public void CopyTo(ColumnCollection other) => Items.CollectionCopy(other);

    /// <summary>
    ///   Gets the <see cref="CsvTools.Column" /> with the specified field name.
    /// </summary>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    /// <value>The column format found by the given name, <c>NULL</c> otherwise</value>
    public Column? Get(string? fieldName) =>
      Items.FirstOrDefault(column => column.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyCollection<IColumn> ReadonlyCopy() =>
      Items.Select(col => new ImmutableColumn(col)).Cast<IColumn>().ToList();
  }
}