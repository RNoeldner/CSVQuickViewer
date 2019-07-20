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

using System.Collections.Generic;
using System.Text;

namespace CsvTools
{
  /// <summary>
  ///   Column errors for one row
  /// </summary>
  public class ColumnErrorDictionary
  {
    /// <summary>
    ///   Gets the internal dictionary
    /// </summary>
    /// <value>
    ///   The dictionary with messages by column number
    /// </value>
    public IDictionary<int, string> Dictionary { get; } = new Dictionary<int, string>();

    /// <summary>
    ///   Combines all messages in order to display them
    /// </summary>
    /// <value>One string with all messages</value>
    public virtual string Display
    {
      get
      {
        var sb = new StringBuilder();
        foreach (var message in Dictionary.Values)
        {
          if (sb.Length > 0)
            sb.Append(ErrorInformation.cSeparator);
          sb.Append(message);
        }

        return sb.ToString();
      }
    }

    /// <summary>
    ///   Gets the <see cref="string" /> with the specified column number.
    /// </summary>
    /// <value>
    ///   The <see cref="string" />.
    /// </value>
    /// <param name="columnNumber">The column number.</param>
    /// <returns><c>null</c> if there is no error for that column</returns>
    public string this[int columnNumber]
    {
      get
      {
        Dictionary.TryGetValue(columnNumber, out var ret);
        return ret;
      }
    }

    /// <summary>
    ///   Adds the column error.
    /// </summary>
    /// <param name="columnNumber">The column number.</param>
    /// <param name="message">The message.</param>
    public void Add(int columnNumber, string message)
    {
      if (Dictionary.TryGetValue(columnNumber, out var old))
        Dictionary[columnNumber] = old.AddMessage(message);
      else
        Dictionary.Add(columnNumber, message);
    }
  }
}