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
using System.Diagnostics.Contracts;
using System.Text;

namespace CsvTools
{
  /// <summary>
  ///   Stores all Messages for a reader
  /// </summary>
  public class RowErrorCollection
  {
    private readonly int m_MaxRows;

    /// <summary>
    ///   A List containing warnings by row/column
    /// </summary>
    private readonly IDictionary<long, ColumnErrorDictionary> m_RowErrorCollection =
      new Dictionary<long, ColumnErrorDictionary>();

    public RowErrorCollection() : this(int.MaxValue)
    {
    }

    public RowErrorCollection(int maxRows)
    {
      m_MaxRows = maxRows;
    }

    /// <summary>
    ///   Number of Rows in the warning lits
    /// </summary>
    /// <value>The number of warnings</value>
    public virtual int CountRows => m_RowErrorCollection.Count;

    /// <summary>
    ///   Combines all messages in order to display them
    /// </summary>
    /// <value>One string with all messages</value>
    public virtual string Display
    {
      get
      {
        var sb = new StringBuilder();
        // Go though all rows
        foreach (var errorsInColumn in m_RowErrorCollection.Values)
          // And all columns
          foreach (var message in errorsInColumn.Dictionary.Values)
          {
            if (sb.Length > 0)
              sb.Append(ErrorInformation.cSeparator);
            sb.Append(message);
          }

        return sb.ToString();
      }
    }

    public virtual string DisplayByRecordNumber
    {
      get
      {
        var sb = new StringBuilder();
        // Go though all rows
        foreach (var pair in m_RowErrorCollection)
        {
          if (sb.Length > 0)
            sb.Append(ErrorInformation.cSeparator);
          var start = $"Row {pair.Key:N0}";
          sb.Append(start);
          sb.Append('\t');
          var errorsInColumn = pair.Value;
          var first = true;
          // And all columns
          foreach (var message in errorsInColumn.Dictionary.Values)
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
    public virtual void Add(object sender, WarningEventArgs args)
    {
      if (CountRows >= m_MaxRows)
        return;

      if (!m_RowErrorCollection.TryGetValue(args.RecordNumber, out var columnErrorCollection))
      {
        columnErrorCollection = new ColumnErrorDictionary();
        m_RowErrorCollection.Add(args.RecordNumber, columnErrorCollection);
      }

      columnErrorCollection.Add(args.ColumnNumber, args.Message);
    }

    /// <summary>
    ///   Empties out the warning list
    /// </summary>
    public void Clear()
    {
      m_RowErrorCollection.Clear();
    }

    /// <summary>
    ///   Tries the retrieve the value for a given record
    /// </summary>
    /// <param name="recordNumber">The record number.</param>
    /// <param name="returnValue">The return value.</param>
    /// <returns></returns>
    public bool TryGetValue(long recordNumber, out ColumnErrorDictionary returnValue)
    {
      // if we return true, th dictionary is not null
      Contract.Ensures(Contract.Result<bool>() == false ||
                       Contract.ValueAtReturn(out returnValue) != null);
      return m_RowErrorCollection.TryGetValue(recordNumber, out returnValue);
    }
  }
}