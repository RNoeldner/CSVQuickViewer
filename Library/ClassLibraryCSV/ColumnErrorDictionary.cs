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

namespace CsvTools
{
  /// <inheritdoc />
  /// <summary>
  ///   Column errors for one row
  /// </summary>
  public sealed class ColumnErrorDictionary : Dictionary<int, string>
  {
    private readonly ICollection<int> m_IgnoredColumns = new HashSet<int>();

    public ColumnErrorDictionary()
    {
    }

    public ColumnErrorDictionary(in IFileReader reader)
    {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      for (var col = 0; col < reader.FieldCount; col++)
      {
        var column = reader.GetColumn(col);
        if (!column.Ignore) continue;
        m_IgnoredColumns.Add(col);
      }

      reader.Warning += (s, args) => { Add(args.ColumnNumber, args.Message); };
    }

    /// <summary>
    ///   Combines all messages in order to display them
    /// </summary>
    /// <value>One string with all messages</value>
    public string Display => Values.Join(ErrorInformation.cSeparator);

    /// <summary>
    ///   Adds the column error.
    /// </summary>
    /// <param name="columnNumber">The column number.</param>
    /// <param name="message">The message.</param>
    public void Add(in int columnNumber, in string message)
    {
      if (m_IgnoredColumns.Contains(columnNumber) || string.IsNullOrEmpty(message))
        return;

      if (TryGetValue(columnNumber, out var old))
        base[columnNumber] = old.AddMessage(message);
      else
        base.Add(columnNumber, message);
    }
  }
}