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
using System.Linq;

namespace CsvTools
{
  /// <summary>
  /// Collection for Mappings
  /// </summary>
  public sealed class MappingCollection : UniqueObservableCollection<Mapping>
  {

    /// <summary>
    /// Get the mapping for a column
    /// </summary>
    /// <param name="columnName">Name of the column / target</param>
    /// <returns></returns>
    public IEnumerable<Mapping> GetByColumn(string columnName) =>
      Items.Where(mapping => mapping.FileColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    ///   Get the IFileSetting Mapping by template column
    /// </summary>
    /// <param name="templateFieldName">The template column.</param>
    /// <returns>Null if the template table field is not mapped</returns>
    public Mapping? GetByField(string? templateFieldName)
    {
      if (string.IsNullOrEmpty(templateFieldName)) return null;
      return Items.FirstOrDefault(
        map => map.TemplateField.Equals(templateFieldName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get the name of a column  for a field
    /// </summary>
    /// <param name="templateFieldName"></param>
    /// <returns></returns>
    public string? GetColumnName(string? templateFieldName) => GetByField(templateFieldName)?.FileColumn;

    /// <summary>
    ///   Remove a Fields mapping.
    /// </summary>
    /// <param name="columnName">The source name.</param>
    public void RemoveColumn(string columnName)
    {
      var toBeRemoved = new List<Mapping>(GetByColumn(columnName));

      if (toBeRemoved.Count == 0)
        return;

      foreach (var fieldMapping in toBeRemoved)
        Remove(fieldMapping);
    }
  }
}