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
using System.Text;

namespace CsvTools
{
  /// <summary>
  ///   Stores all Messages for a reader
  /// </summary>
  public sealed class RowErrorCollection
  {
    private readonly int m_MaxRows = int.MaxValue;

    /// <summary>
    ///   A List containing warnings by row/column
    /// </summary>
    private readonly Dictionary<long, ColumnErrorDictionary> m_RowErrorCollection =
      new Dictionary<long, ColumnErrorDictionary>();

    private ICollection<int>? m_IgnoredColumns;

    /// <summary>
    ///   Attach the error collection to the reader
    /// </summary>
    /// <param name="reader"></param>
    public RowErrorCollection(in IFileReader reader) => reader.Warning += Add;

    /// <summary>
    /// Initializes a new instance of the <see cref="RowErrorCollection"/> class.
    /// </summary>
    /// <param name="maxRows">The maximum rows.</param>
    public RowErrorCollection(int maxRows) => m_MaxRows = maxRows;

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
    /// Occurs when warning needs to be passed on 
    /// </summary>
    public event EventHandler<WarningEventArgs>? PassWarning;

    /// <summary>
    ///   Add a warning to the list of warnings
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void Add(object? sender, WarningEventArgs args)
    {
      if ((m_IgnoredColumns != null && m_IgnoredColumns.Contains(args.ColumnNumber)) ||
          m_RowErrorCollection.Count >= m_MaxRows)
        return;

      if (!m_RowErrorCollection.TryGetValue(args.RecordNumber, out var columnErrorCollection))
      {
        columnErrorCollection = new ColumnErrorDictionary();
        m_RowErrorCollection.Add(args.RecordNumber, columnErrorCollection);
      }

      columnErrorCollection.Add(args.ColumnNumber, args.Message);
      PassWarning?.Invoke(sender, args);
    }

    /// <summary>
    ///   Empties out the warning list
    /// </summary>
    public void Clear() => m_RowErrorCollection.Clear();

    /// <summary>
    /// Handles the ignored columns.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <exception cref="System.InvalidOperationException">Reader has not been opened.</exception>
    public void HandleIgnoredColumns(in IFileReader reader)
    {
      if (reader.IsClosed)
        throw new InvalidOperationException("Reader has not been opened.");

      for (var col = 0; col < reader.FieldCount; col++)
      {
        if (!reader.GetColumn(col).Ignore) continue;
        m_IgnoredColumns ??= new HashSet<int>();
        m_IgnoredColumns.Add(col);
      }
    }

    /// <summary>
    ///   Tries the retrieve the value for a given record
    /// </summary>
    /// <param name="recordNumber">The record number.</param>
    /// <param name="returnValue">The return value.</param>
    /// <returns></returns>
    public bool TryGetValue(long recordNumber, out ColumnErrorDictionary? returnValue) =>
      // if we return true, th dictionary is not null
      m_RowErrorCollection.TryGetValue(recordNumber, out returnValue);
  }
}