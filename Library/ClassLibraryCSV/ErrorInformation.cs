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
  // private const char cAlternateColumnMessageSeparator = ':';

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
  ///   A new error list text, if the message was already contained is not added a second time,
  ///   usually messages are appended, unless they are errors and the list contains only warnings
  ///   so far
  /// </returns>
  public static string AddMessage(this string errorList, string newError, bool isWarning)
  {
    if (string.IsNullOrEmpty(newError))
      throw new ArgumentException("Error can not be empty", nameof(newError));

    // no need to check for null
    if (errorList.Length == 0)
      return newError;

    // if the message is already in the text do not do anything
    if (errorList.Contains(newError))
      return errorList;

    var sb = new StringBuilder(errorList.Length + newError.Length + 1);

    // If the new message is considered an error put it in front, this way it's easier to check if
    // there is an error
    if (isWarning)
    {
      // Append to previous messages
      sb.Append(errorList);
      sb.Append(cSeparator);
      sb.Append(cWarningId);
      sb.Append(newError);
    }
    else
    {
      // Put in front of previous messages, to have errors first
      sb.Append(newError);
      sb.Append(cSeparator);
      sb.Append(errorList);
    }

    return sb.ToString();
  }

  /// <summary>
  ///   String method to append a message an error list text
  /// </summary>
  /// <param name="errorList">A text containing different types of messages that are concatenated</param>
  /// <param name="newError">A new message that should be added to the list</param>
  /// <returns>
  ///   A new error list text, if the message was already contained is not added a second time,
  ///   usually messages are appended, unless they are errors and the list contains only warnings
  ///   so far
  /// </returns>
  public static string AddMessage(this string errorList, string newError)
  {
    if (string.IsNullOrEmpty(newError))
      throw new ArgumentException("Error can not be empty", nameof(newError));

    // no need to check for null
    if (errorList.Length == 0)
      return newError;

    // if the message is already in the text do not do anything
    if (errorList.Contains(newError))
      return errorList;

    var sb = new StringBuilder(errorList.Length + newError.Length + 1);

    // If the new message is considered an error put it in front, this way it's easier to check if
    // there is an error
    if (newError.IsWarningMessage())
    {
      // Append to previous messages
      sb.Append(errorList);
      sb.Append(cSeparator);
      sb.Append(newError);
    }
    else
    {
      // Put in front of previous messages, to have errors first
      sb.Append(newError);
      sb.Append(cSeparator);
      sb.Append(errorList);
    }

    return sb.ToString();
  }

  /// <summary>
  ///   String method to add the warning identifier to an error message
  /// </summary>
  /// <param name="message">The message that should get the ID</param>
  /// <returns>The text with the leading WarningID</returns>
  public static string AddWarningId(this string message)
  {
    if (message.Length == 0 || message.StartsWith(cWarningId, StringComparison.Ordinal))
      return message;

    var sb = new StringBuilder();
    sb.Append(cWarningId);
    sb.Append(message);
    return sb.ToString();
  }

  /// <summary>
  ///   Method to add the warning identifier to an error message
  /// </summary>
  /// <param name="message">The message that should get the ID</param>
  /// <param name="buffer">The span that will be modifed</param>
  /// <returns>The text with the leading WarningID</returns>
  public static int AddWarningId(ReadOnlySpan<char> message, Span<char> buffer)
  {
    if (message.StartsWith(cWarningId.AsSpan(), StringComparison.Ordinal))
    {
      message.CopyTo(buffer);
      return message.Length;
    }

    cWarningId.AsSpan().CopyTo(buffer);
    message.CopyTo(buffer.Slice(cWarningId.Length));
    return cWarningId.Length + message.Length;
  }

  /// <summary>
  ///   Combines column and error information
  /// </summary>
  /// <param name="column">The column.</param>
  /// <param name="errorMessage">The error message.</param>
  /// <returns>An error message to be stored</returns>
  public static string CombineColumnAndError(string column, string errorMessage)
  {
    if (string.IsNullOrEmpty(errorMessage))
      throw new ArgumentNullException(nameof(errorMessage));
    // pass back messages that already have column information
    if (column.Length == 0 && errorMessage[0] == cOpenField)
      return errorMessage;
    return $"{cOpenField}{column}{cClosingField} {errorMessage}";
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
      list.Add(new ColumnAndMessage(string.Empty, row.RowError));
    list.AddRange(
      row.GetColumnsInError().Select(col => new ColumnAndMessage(col.ColumnName, row.GetColumnError(col))));
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
    var sbWaring = new StringBuilder();

    foreach (var parts in ParseList(errorList))
    {
      // errors are in front so if overall message is not an error there is no error
      var start = 0;
      while (start < parts.Message.Length)
      {
        var end = parts.Message.IndexOf(cSeparator, start + 1);
        if (end == -1)
          end = parts.Message.Length;
        if (start < end)
        {
          var part = parts.Message.Substring(start, end - start);
          if (part.IsWarningMessage())
          {
            if (sbWaring.Length > 0)
              sbWaring.Append(cSeparator);
            sbWaring.Append(
              parts.Column.Length == 0
                ? part.WithoutWarningId()
                : CombineColumnAndError(parts.Column, part.WithoutWarningId()));
          }
          else
          {
            if (sbErrors.Length > 0)
              sbErrors.Append(cSeparator);
            sbErrors.Append(parts.Column.Length == 0 ? part : CombineColumnAndError(parts.Column, part));
          }
        }

        start = end + 1;
      }
    }

    return new ColumnAndMessage(sbErrors.ToString(), sbWaring.ToString());
  }

  /// <summary>
  ///   String method to check if the text should be regarded as an error in an error list text
  /// </summary>
  /// <param name="errorList">A text containing different types of messages that are concatenated</param>
  /// <returns>
  ///   <c>true</c> if the text should be regarded as an error message, <c>false</c> if it's a
  ///   warning message or empty
  /// </returns>
  public static bool IsErrorMessage(this string errorList)
  {
    if (errorList.Length == 0)
      return false;
    return !errorList.IsWarningMessage();
  }

  /// <summary>
  ///   String method to check if the text should is regarded as a warning in an error list text
  /// </summary>
  /// <param name="errorList">A text containing different types of messages that are concatenated</param>
  /// <returns>
  ///   <c>true</c> if the text should be regarded as a warning message, <c>false</c> if it's an
  ///   error message or empty
  /// </returns>
  public static bool IsWarningMessage(this string errorList)
  {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
  ReadOnlySpan<char> span = errorList.AsSpan();

  if (span.StartsWith(cWarningId.AsSpan(), StringComparison.Ordinal))
    return true;

  var splitter = span.IndexOf(cClosingField);
  return splitter != -1 && splitter < span.Length - 2
                        && span.Slice(splitter + 2).StartsWith(cWarningId.AsSpan(), StringComparison.Ordinal);
#else
    if (errorList.StartsWith(cWarningId, StringComparison.Ordinal))
      return true;

    var splitter = errorList.IndexOf(cClosingField);
    return splitter != -1 && splitter < errorList.Length - 2
                          && errorList.Substring(splitter + 2).StartsWith(cWarningId, StringComparison.Ordinal);
#endif
  }

  /// <summary>
  ///   Stores the error information in a single string (usually stored in #error )
  /// </summary>
  /// <param name="columnErrors">The column errors by column number</param>
  /// <param name="getColumnName">A Function to get the column name</param>
  /// <returns>The error message with a resulting of the column number to the column name</returns>
  public static string ReadErrorInformation(IDictionary<int, string>? columnErrors, Func<int, string> getColumnName)
  {
    if (columnErrors is null || columnErrors.Count == 0)
      return string.Empty;

    var list = new List<ColumnAndMessage>();
    // Parallel.Foreach but it not reliable
    foreach (var entry in columnErrors)
    {
      if (entry.Key < -1) continue;
      var colName = getColumnName(entry.Key);
      var start = 0;
      while (start < entry.Value.Length)
      {
        var end = entry.Value.IndexOf(cSeparator, start + 1);
        if (end == -1)
          end = entry.Value.Length;
        // Add one Line for each error in the column
        list.Add(new ColumnAndMessage(colName, entry.Value.Substring(start, end - start)));
        start = end + 1;
      }
    }

    return BuildList(list);
  }

  /// <summary>
  ///   Set the Row and Column errors of the DataRow, based on parameter
  /// </summary>
  /// <param name="row">The DataRow that will get the error information</param>
  /// <param name="errorList"></param>
  /// <param name="onlyColumnErrors">If set true, row errors will not be set</param>
  public static void SetErrorInformation(this DataRow row, string errorList, bool onlyColumnErrors = false)
  {
    row.ClearErrors();

    if (string.IsNullOrEmpty(errorList))
      return;

    foreach (var parts in ParseList(errorList))
      if (parts.Column.Length == 0)
      {
        if (!onlyColumnErrors)
          row.RowError = parts.Message;
      }
      else
      {
        var start = 0;
        // If we have combinations of columns, e,G. Combined Keys or Less Than errors store the error
        // in each column
        while (start < parts.Column.Length)
        {
          var end = parts.Column.IndexOf(cFieldSeparator, start + 1);
          if (end == -1)
            end = parts.Column.Length;
          var colName = parts.Column.Substring(start, end - start);
          if (row.Table.Columns.Contains(colName) && !string.IsNullOrEmpty(parts.Message))
            row.SetColumnError(colName, row.GetColumnError(colName).AddMessage(parts.Message));
          else
          {
            if (!onlyColumnErrors)
              row.RowError = row.RowError.AddMessage(
                CombineColumnAndError(parts.Column, parts.Message));
          }

          start = end + 1;
        }
      }
  }

  /// <summary>
  ///   String method to remove the warning identifier from an error list text
  /// </summary>
  /// <param name="errorList">A text containing different types of messages that are concatenated</param>
  /// <returns>The text without the leading WarningID</returns>
  public static string WithoutWarningId(this string errorList)
  {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
  ReadOnlySpan<char> span = errorList.AsSpan();
  return span.Length <= cWarningId.Length 
    ? errorList 
    : span.Slice(cWarningId.Length).ToString();
#else
    return errorList.Length <= cWarningId.Length
      ? errorList
      : errorList.Substring(cWarningId.Length);
#endif
  }

  /// <summary>
  ///   Builds a list from the provided error list, sorting error in front, this will show
  ///   duplicates though
  /// </summary>
  /// <param name="errorList">The error list.</param>
  /// <returns></returns>
  private static string BuildList(in IEnumerable<ColumnAndMessage> errorList)
  {
    var errors = new StringBuilder();
    // False before True in Linq order by
    foreach (var entry in errorList.OrderBy(part => part.Message.IsWarningMessage()))
    {
      if (errors.Length > 0)
        errors.Append(cSeparator);
      errors.Append(CombineColumnAndError(entry.Column, entry.Message));
    }

    return errors.ToString();
  }

  /// <summary>
  /// Parses the error list text into individual messages with their columns.
  /// </summary>
  /// <param name="errorList">The combined error text.</param>
  /// <returns>A list of messages with column information.</returns>
  private static List<ColumnAndMessage> ParseList(string errorList)
  {
    var result = new List<ColumnAndMessage>();
    if (string.IsNullOrEmpty(errorList))
      return result;

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    ReadOnlySpan<char> span = errorList.AsSpan();
    int start = 0;
    while (start < span.Length)
    {
        int end = span.Slice(start + 1).IndexOf(cSeparator);
        if (end == -1)
            end = span.Length - start;
        result.Add(SplitColumnAndMessage(span.Slice(start, end).ToString()));
        start += end + 1;
    }
#else
    int start = 0;
    while (start < errorList.Length)
    {
      int end = errorList.IndexOf(cSeparator, start + 1);
      if (end == -1)
        end = errorList.Length;
      result.Add(SplitColumnAndMessage(errorList.Substring(start, end - start)));
      start = end + 1;
    }
#endif

    return result;
  }


  /// <summary>
  /// Struct for Column and Message information
  /// </summary>
  public readonly struct ColumnAndMessage
  {
    /// <summary>
    /// Name of the Column
    /// </summary>
    public readonly string Column;

    /// <summary>
    /// Message for this column
    /// </summary>
    public readonly string Message;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="column"></param>
    /// <param name="message"></param>
    public ColumnAndMessage(string column, string message)
    {
      Column = column;
      Message = message;
    }
  }

  /// <summary>
  ///   Splits column and error information
  /// </summary>
  /// <param name="text">The column with error.</param>
  /// <returns></returns>
  private static ColumnAndMessage SplitColumnAndMessage(string text)
  {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
  ReadOnlySpan<char> span = text.AsSpan();
  if (!span.IsEmpty && span[0] == cOpenField)
  {
    var close = span.Slice(1).IndexOf(cClosingField);
    if (close == -1)
      return new ColumnAndMessage(string.Empty, text);

    var column = span.Slice(1, close).ToString();
    var message = span.Slice(close + 2).ToString();
    return new ColumnAndMessage(column, message);
  }
  return new ColumnAndMessage(string.Empty, text);
#else
    if (text.Length <= 0 || text[0] != cOpenField) return new ColumnAndMessage(string.Empty, text);
    var close = text.IndexOf(cClosingField);
    return close == -1
      ? new ColumnAndMessage(string.Empty, text)
      : new ColumnAndMessage(text.Substring(1, close - 1), text.Substring(close + 2));
#endif
  }
}