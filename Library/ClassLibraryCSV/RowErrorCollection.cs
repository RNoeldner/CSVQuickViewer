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

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsvTools
{
  /// <summary>
  ///   Stores all Messages for a m_Reader
  /// </summary>
  public sealed class RowErrorCollection
  {
    /// <summary>
    ///   A List containing warnings by row/column
    /// </summary>
    private readonly Dictionary<long, Dictionary<int, string>> m_RowErrorCollection = new Dictionary<long, Dictionary<int, string>>();

    /// <summary>
    ///   Number of Rows in the warning list
    /// </summary>
    /// <value>The number of warnings</value>
    public int CountRows => m_RowErrorCollection.Count;

    /// <summary>
    ///   Combines all messages in order to display them
    /// </summary>
    /// <value>One string with all messages</value>
    public string Display
    {
      get
      {
        var sb = new StringBuilder();
        // Go through all rows
        foreach (var message in m_RowErrorCollection.Values.SelectMany(errorsInColumn => errorsInColumn.Values))
        {
          if (sb.Length > 0)
            sb.Append(ErrorInformation.cSeparator);
          sb.Append(message);
        }

        return sb.ToString();
      }
    }

    /// <summary>
    /// Gets the text representation for errors in a data table row error collection
    /// </summary>
    /// <value>
    /// All errors for all rows
    /// </value>
    public string DisplayByRecordNumber
    {
      get
      {
        var sb = new StringBuilder();
        // Go through all rows
        foreach (var keyValuePair in m_RowErrorCollection)
        {
          if (sb.Length > 0)
            sb.Append(ErrorInformation.cSeparator);
          var start = $"Row {keyValuePair.Key:N0}";
          sb.Append(start);
          sb.Append('\t');
          var first = true;
          // And all columns
          foreach (var message in keyValuePair.Value.Values)
          {
            // indent next message
            if (!first)
            {
              sb.Append(ErrorInformation.cSeparator);
              sb.Append(new string(' ', start.Length));
              sb.Append('\t');
            }

            sb.Append(message);
            first = false;
          }
        }

        return sb.ToString();
      }
    }

    /// <summary>
    ///   Add a warning to the list of warnings
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void Add(object? sender, WarningEventArgs args)
    {

      // Ensure we have a dictionary for the given row. If it doesn't exist, create one.
      if (!m_RowErrorCollection.TryGetValue(args.RecordNumber, out var columnErrorCollection))
      {
        columnErrorCollection = new Dictionary<int, string>();
        m_RowErrorCollection[args.RecordNumber] = columnErrorCollection;
      }

      // If there is already an error message for the column, append the new message.
      // Otherwise, add the new column error.
      columnErrorCollection[args.ColumnNumber] =
          columnErrorCollection.TryGetValue(args.ColumnNumber, out var old)
              ? old.AddMessage(args.Message)  // Combine old message with new
              : args.Message;                // Add new message if none exists
    }

    /// <summary>
    ///   Empties out the warning list
    /// </summary>
    public void Clear() => m_RowErrorCollection.Clear();


    /// <summary>
    ///   Tries the retrieve the value for a given record
    /// </summary>
    /// <param name="recordNumber">The record number.</param>
    /// <param name="returnValue">The return value.</param>
    /// <returns></returns>
    public bool TryGetValue(long recordNumber, out Dictionary<int, string>? returnValue) =>
      // if we return true, th dictionary is not null
      m_RowErrorCollection.TryGetValue(recordNumber, out returnValue);
  }
}
