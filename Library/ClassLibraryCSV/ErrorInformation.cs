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
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CsvTools;

/// <summary>
///   Static class to extract and restore dataRow error information
/// </summary>
public static class ErrorInformation
{
  /// <summary>
  ///   Char to separate two or more errors and warnings
  /// </summary>
  public const char cSeparator = '\n';

  private const char cOpenField = '[';
  private const char cClosingField = ']';

  /// <summary>
  ///   Char to separate two column names
  /// </summary>
  private const char cFieldSeparator = ',';

  /// <summary>
  ///   Identifier for a warning message
  /// </summary>
  public const string cWarningId = "Warning: ";

  /// <summary>
  ///   String method to append a message an error list text
  /// </summary>
  /// <param name="errorList">A text containing different types of messages that are concatenated</param>
  /// <param name="newError">A new message that should be added to the list</param>
  /// <param name="isWarning"><c>true</c> if this message is a warning</param>
  /// <returns>
  ///   A new error list text, if the message was already contained, is not added a second time,
  ///   usually messages are appended, unless they are errors and the list contains only warnings
  ///   so far
  /// </returns>
  public static string AddMessage(this ReadOnlySpan<char> errorList, ReadOnlySpan<char> newError, bool isWarning)
  {
    if (newError.IsEmpty)
      throw new ArgumentException("Error can not be empty", nameof(newError));

    // no need to check for null
    if (errorList.Length == 0)
      return newError.ToString();

    // if the message is already in the text, do not do anything
    if (errorList.Contains(newError, StringComparison.Ordinal))
      return errorList.ToString();
    return isWarning
        ? string.Concat(errorList.ToString(), cSeparator.ToString(), newError.AddWarningId())
        : string.Concat(newError.ToString(), cSeparator.ToString(), errorList.ToString());
  }

  /// <summary>
  ///   String method to append a message an error list text
  /// </summary>
  /// <param name="errorList">A text containing different types of messages that are concatenated</param>
  /// <param name="newError">A new message that should be added to the list</param>
  /// <returns>
  ///   A new error list text, if the message was already contained, is not added a second time,
  ///   usually messages are appended, unless they are errors and the list contains only warnings
  ///   so far
  /// </returns>
  public static string AddMessage(this ReadOnlySpan<char> errorList, ReadOnlySpan<char> newError)
    => AddMessage(errorList, newError, newError.IsWarningMessage());
 
  /// <summary>
  ///   String method to add the warning identifier to an error message
  /// </summary>
  /// <param name="message">The message that should get the ID</param>
  /// <returns>The text with the leading WarningID</returns>
  public static string AddWarningId(this ReadOnlySpan<char> message)
  {
    if (message.Length == 0 || message.StartsWith(cWarningId.AsSpan(), StringComparison.Ordinal))
      return message.ToString();
    return string.Concat(cWarningId, message.ToString());
  }

  /// <summary>
  ///   Combines column and error information
  /// </summary>
  /// <param name="column">The column.</param>
  /// <param name="errorMessage">The error message.</param>
  /// <returns>An error message to be stored</returns>
  public static string CombineColumnAndError(ReadOnlySpan<char> column, ReadOnlySpan<char> errorMessage)
  {
    if (errorMessage.IsEmpty)
      throw new ArgumentNullException(nameof(errorMessage));
    // pass back messages that already have column information
    if (column.Length == 0 && errorMessage[0] == cOpenField)
      return errorMessage.ToString();
    return $"{cOpenField}{column.ToString()}{cClosingField} {errorMessage.ToString()}";
  }

  /// <summary>
  ///   Retrieved the error information from a <see cref="DataRow" /> in a text
  /// </summary>
  /// <param name="row">The DataRow with the error information</param>
  /// <returns>A string with all error information of the DataRow</returns>
  public static string GetErrorInformation(this DataRow row)
  {
    var list = new List<ColumnAndMessage>();
    if (!string.IsNullOrEmpty(row.RowError))
      list.Add(new ColumnAndMessage(ReadOnlyMemory<char>.Empty, row.RowError.AsMemory()));
    list.AddRange(
      row.GetColumnsInError().Select(col => new ColumnAndMessage(col.ColumnName.AsMemory(), row.GetColumnError(col).AsMemory())));
    return BuildList(list);
  }

  /// <summary>
  ///   Extracts the errors from a combined error warning text
  /// </summary>
  /// <remarks>
  ///   This is currently only used to get the errors from HTML for a cell or a row, could
  ///   possibly be optimized to that purpose
  /// </remarks>
  /// <param name="errorList">A text containing different types of messages that are concatenated.</param>
  /// <returns>two strings first error second warnings</returns>
  public static ColumnAndMessage GetErrorsAndWarnings(this string errorList)
  {
    var sbErrors = new StringBuilder();
    var sbWarning = new StringBuilder();

    foreach (var parts in ParseList(errorList))
    {
      ReadOnlySpan<char> messageSpan = parts.Message.Span;
      ReadOnlySpan<char> columnSpan = parts.Column.Span;

      var start = 0;
      while (start < messageSpan.Length)
      {
        var end = messageSpan.Slice(start).IndexOf(cSeparator);
        int length = (end == -1) ? messageSpan.Length - start : end;

        if (length > 0)
        {
          ReadOnlySpan<char> part = messageSpan.Slice(start, length);

          if (part.IsWarningMessage())
          {
            if (sbWarning.Length > 0) sbWarning.Append(cSeparator);

            // Slice the warning ID off directly without temporary strings
            ReadOnlySpan<char> cleanPart = part.Slice(cWarningId.Length);

            sbWarning.Append(columnSpan.IsEmpty
                ? cleanPart.ToString()
                : CombineColumnAndError(columnSpan, cleanPart));
          }
          else
          {
            if (sbErrors.Length > 0) sbErrors.Append(cSeparator);

            sbErrors.Append(columnSpan.IsEmpty
                ? part.ToString()
                : CombineColumnAndError(columnSpan, part));
          }
        }

        if (end == -1) break;
        start += length + 1;
      }
    }

    return new ColumnAndMessage(sbErrors.ToString().AsMemory(), sbWarning.ToString().AsMemory());
  }

  /// <summary>
  ///   String method to check if the text should be regarded as an error in an error list text
  /// </summary>
  /// <param name="errorList">A text containing different types of messages that are concatenated</param>
  /// <returns>
  ///   <c>true</c> if the text should be regarded as an error message, <c>false</c> if it's a
  ///   warning message or empty
  /// </returns>
  public static bool IsErrorMessage(this ReadOnlySpan<char> errorList)
  {
    if (errorList.Length == 0)
      return false;
    return !errorList.IsWarningMessage();
  }

  /// <summary>
  ///   String method to check if the text is regarded as a warning in an error list text.
  /// </summary>
  /// <param name="errorList">A text containing different types of messages that are concatenated.</param>
  /// <returns>
  ///   <c>true</c> if the text should be regarded as a warning message, <c>false</c> if it's an
  ///   error message or empty.
  /// </returns>
  public static bool IsWarningMessage(this ReadOnlySpan<char> errorList)
  {
    // Compiler flow analysis understands that errorList is non-null after this.
    if (errorList.IsEmpty)
      return false;

    ReadOnlySpan<char> warnId = cWarningId.AsSpan();

    // 1. Direct match at the start
    if (errorList.StartsWith(warnId, StringComparison.Ordinal))
      return true;

    // 2. Match after the first field (e.g., "[ColumnName]: [WARNING]")
    var splitter = errorList.IndexOf(cClosingField);
    if (splitter != -1)
    {
      var startIdx = splitter + 2;
      // Safety check to ensure we don't slice out of bounds
      if (startIdx <= errorList.Length - warnId.Length)
        return errorList.Slice(startIdx).StartsWith(warnId, StringComparison.Ordinal);
    }
    return false;
  }

  /// <summary>
  ///   Stores the error information in a single string (usually stored in #error)
  /// </summary>
  /// <param name="columnErrors">The column errors by column number</param>
  /// <param name="getColumnName">A Function to get the column name</param>
  /// <returns>The error message with a resulting of the column number to the column name</returns>
  public static string ReadErrorInformation(IDictionary<int, ReadOnlyMemory<char>>? columnErrors, Func<int, string> getColumnName)
  {
    if (columnErrors is null || columnErrors.Count == 0)
      return string.Empty;

    var list = new List<ColumnAndMessage>();
    foreach (var entry in columnErrors)
    {
      if (entry.Key < -1 || entry.Value.IsEmpty) continue;

      var colNameMemory = getColumnName(entry.Key).AsMemory();
      var span = entry.Value.Span;

      int start = 0;
      while (start < span.Length)
      {
        int end = span.Slice(start).IndexOf(cSeparator);
        int length = (end == -1) ? span.Length - start : end;

        // Slicing the existing memory instead of Substring
        list.Add(new ColumnAndMessage(colNameMemory, entry.Value.Slice(start, length)));

        if (end == -1) break;
        start += length + 1;
      }
    }

    return BuildList(list);
  }

  /// <summary>
  ///   Set the Row and Column errors of the DataRow, based on a parameter
  /// </summary>
  /// <param name="row">The DataRow that will get the error information</param>
  /// <param name="errorList">A text containing different types of messages that are concatenated</param>
  /// <param name="onlyColumnErrors">If set true, row errors will not be set</param>
  public static void SetErrorInformation(this DataRow row, string errorList, bool onlyColumnErrors = false)
  {
    row.ClearErrors();

    if (string.IsNullOrEmpty(errorList))
      return;

    foreach (var parts in ParseList(errorList))
    {
      if (parts.Column.IsEmpty)
      {
        if (!onlyColumnErrors)
          row.RowError = parts.Message.ToString();
      }
      else
      {
        var columnSpan = parts.Column.Span;
        var start = 0;
        // If we have combinations of columns, e,G. Combined Keys or Less Than errors store the error
        // in each column
        while (start < columnSpan.Length)
        {
          var end = columnSpan.Slice(start).IndexOf(cFieldSeparator);
          if (end == -1)
            end = parts.Column.Length;
          var colName = columnSpan.Slice(start, end).ToString();

          if (row.Table.Columns.Contains(colName) && !parts.Message.IsEmpty)
          {
            var currentError = row.GetColumnError(colName);
            // Assuming AddMessage can handle string and span/memory
            row.SetColumnError(colName, currentError.AsSpan().AddMessage(parts.Message.Span));
          }
          else if (!onlyColumnErrors)
          {
            row.RowError = row.RowError.AsSpan().AddMessage(
              CombineColumnAndError(columnSpan.Slice(start, end), parts.Message.Span));
          }

          start += end + 1;
        }
      }
    }
  }

  /// <summary>
  ///   String method to remove the warning identifier from an error list text
  /// </summary>
  /// <param name="errorList">A text containing different types of messages that are concatenated</param>
  /// <returns>The text without the leading WarningID</returns>
  public static string WithoutWarningId(this ReadOnlySpan<char> errorList)
  {    
    return errorList.Length <= cWarningId.Length
      ? errorList.ToString()
      : errorList.Slice(cWarningId.Length).ToString();
  }

  /// <summary>
  ///   Builds a list from the provided error list, sorting error in front; this will show
  ///   duplicates though
  /// </summary>
  /// <param name="errorList">The error list.</param>
  private static string BuildList(in IEnumerable<ColumnAndMessage> errorList)
  {
    var errors = new StringBuilder();
    // False before True in Linq order by
    foreach (var entry in errorList.OrderBy(part => part.Message.Span.IsWarningMessage()))
    {
      if (errors.Length > 0)
        errors.Append(cSeparator);
      errors.Append(CombineColumnAndError(entry.Column.Span, entry.Message.Span));
    }

    return errors.ToString();
  }

  /// <summary>
  /// Parses the error list text into individual messages with their columns.
  /// </summary>
  /// <param name="errorList">The combined error text.</param>
  /// <returns>A list of messages with column information.</returns>
  private static IEnumerable<ColumnAndMessage> ParseList(string errorList)
  {
    if (string.IsNullOrEmpty(errorList))
      yield break;

    var memory = errorList.AsMemory();
    int start = 0;

    while (start < memory.Length)
    {
      // Find the next separator
      int index = memory.Span.Slice(start).IndexOf(cSeparator);
      int length = (index == -1) ? memory.Length - start : index;

      // Yield the split result using memory slices
      yield return SplitColumnAndMessage(memory.Slice(start, length));

      if (index == -1) break;
      start += length + 1;
    }
  }


  /// <summary>
  /// Struct for Column and Message information
  /// </summary>
  public readonly struct ColumnAndMessage
  {
    /// <summary>
    /// The name of the column
    /// </summary>
    public readonly ReadOnlyMemory<char> Column;
    
    /// <summary>
    /// The Message for the column
    /// </summary>
    public readonly ReadOnlyMemory<char> Message;

    /// <summary>
    /// Create an instance for ColumnAndMessage
    /// </summary>
    /// <param name="column">The name of the column</param>
    /// <param name="message">The Message for the column</param>
    public ColumnAndMessage(ReadOnlyMemory<char> column, ReadOnlyMemory<char> message)
    {
      Column = column;
      Message = message;
    }
  }

  /// <summary>
  ///   Splits column and error information
  /// </summary>
  /// <param name="memory">Text span with the message</param>  
  private static ColumnAndMessage SplitColumnAndMessage(ReadOnlyMemory<char> memory)
  {
    var span = memory.Span;
    if (!span.IsEmpty && span[0] == cOpenField)
    {
      var close = span.Slice(1).IndexOf(cClosingField);
      if (close == -1)
        return new ColumnAndMessage(ReadOnlyMemory<char>.Empty, memory);

      // Slice the memory directly
      var column = memory.Slice(1, close);
      // Symmetrical to CombineColumnAndError need 3 as we have a space
      var message = memory.Slice(close + 3);
      return new ColumnAndMessage(column, message);
    }
    return new ColumnAndMessage(ReadOnlyMemory<char>.Empty, memory);
  }

}