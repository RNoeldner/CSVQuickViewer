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
  ///   Appends or prepends a message to an existing error list text, maintaining specific sorting logic.
  /// </summary>
  /// <param name="errorList">The existing list of error and warning messages.</param>
  /// <param name="newError">The new message to be added to the list.</param>
  /// <param name="isWarning">
  ///   <see langword="true"/> if the new message is a warning; <see langword="false"/> if it is an error.
  /// </param>
  /// <returns>
  ///   A new string containing the combined messages. If <paramref name="newError"/> is already 
  ///   contained within <paramref name="errorList"/>, the original <paramref name="errorList"/> is returned.
  /// </returns>
  /// <remarks>
  ///   Errors are prioritized at the start of the list. If <paramref name="isWarning"/> is <see langword="true"/>, 
  ///   the message is appended with the <see cref="cWarningId"/> prefix. If it is <see langword="false"/>, 
  ///   the error is prepended to the list to ensure errors appear first.
  /// </remarks>
  /// <exception cref="ArgumentException">Thrown when <paramref name="newError"/> is empty.</exception>
  public static string AddMessage(this ReadOnlySpan<char> errorList, ReadOnlySpan<char> newError, bool isWarning)
  {
    if (newError.IsEmpty)
      throw new ArgumentException("Error can not be empty", nameof(newError));

    if (errorList.IsEmpty)
      return isWarning ? string.Concat(cWarningId, newError.ToString()) : newError.ToString();

    // Check if the message is already contained
    if (errorList.Contains(newError, StringComparison.Ordinal))
      return errorList.ToString();
    

    return isWarning 
      ? string.Concat(errorList.ToString(), cSeparator.ToString(), cWarningId, newError.ToString())
      : string.Concat(newError.ToString(), cSeparator.ToString(), errorList.ToString());
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
  public static string AddMessage(this ReadOnlySpan<char> errorList, ReadOnlySpan<char> newError)
    =>  errorList.AddMessage(newError, newError.IsWarningMessage());

  /// <summary>
  ///   String method to add the warning identifier to an error message
  /// </summary>
  /// <param name="message">The message that should get the ID</param>
  /// <returns>The text with the leading WarningID</returns>
  public static string AddWarningId(this ReadOnlySpan<char> message)
  {
    // Ensure cWarningId is treated as a Span
    ReadOnlySpan<char> warningIdSpan = cWarningId.AsSpan();

    if (message.Length == 0 || message.StartsWith(warningIdSpan, StringComparison.Ordinal))
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

    return string.Concat(cOpenField.ToString(), column.ToString(), cClosingField.ToString(), errorMessage.ToString());
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
  ///   Extracts and separates the errors and warnings from a combined error message list.
  /// </summary>
  /// <remarks>
  ///   This method parses the provided <paramref name="errorList"/>, identifies individual 
  ///   messages, and categorizes them into either error or warning status based on their content.
  ///   The resulting <see cref="ColumnAndMessage"/> contains both categorized lists, 
  ///   reconstructed with their respective column information if available.
  /// </remarks>
  /// <param name="errorList">
  ///   A <see cref="ReadOnlySpan{Char}"/> containing concatenated error and warning messages.
  /// </param>
  /// <returns>
  ///   A <see cref="ColumnAndMessage"/> instance where the <c>Column</c> property contains 
  ///   all identified error messages and the <c>Message</c> property contains all identified 
  ///   warning messages, separated by the defined <see cref="cSeparator"/>.
  /// </returns>
  public static ColumnAndMessage GetErrorsAndWarnings(this ReadOnlySpan<char> errorList)
  {
    var sbErrors = new StringBuilder();
    var sbWarning = new StringBuilder();

    foreach (var parts in ParseList(errorList))
    {
      ReadOnlySpan<char> messageSpan = parts.Message.AsSpan();
      ReadOnlySpan<char> columnSpan = parts.Column.AsSpan();

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

    return new ColumnAndMessage(sbErrors.ToString(), sbWarning.ToString());
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

    // 1. Direct match at start
    if (errorList.StartsWith(warnId, StringComparison.Ordinal))
      return true;

    // 2. Match after first field (e.g., "[ColumnName]: [WARNING]")
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
  public static string WithoutWarningId(this ReadOnlySpan<char> errorList)
  {
    return errorList.Length <= cWarningId.Length
      ? errorList.ToString()
      : errorList.Slice(cWarningId.Length).ToString();
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
  private static List<ColumnAndMessage> ParseList(ReadOnlySpan<char> errorList)
  {
    var result = new List<ColumnAndMessage>();
    if (errorList.IsEmpty)
      return result;

    int start = 0;
    while (start < errorList.Length)
    {
      int end = errorList.Slice(start + 1).IndexOf(cSeparator) + 1;
      if (end == 0)
        end = errorList.Length - start;
      result.Add(SplitColumnAndMessage(errorList.Slice(start, end)));
      start += end + 1;
    }

    return result;
  }


  /// <summary>
  ///   Splits column and error information
  /// </summary>
  /// <param name="span">Text span with the message</param>  
  /// <returns></returns>
  private static ColumnAndMessage SplitColumnAndMessage(ReadOnlySpan<char> span)
  {
    if (!span.IsEmpty && span[0] == cOpenField)
    {
      var close = span.Slice(1).IndexOf(cClosingField);
      if (close == -1)
        return new ColumnAndMessage(string.Empty, span.ToString());
      var column = span.Slice(1, close).ToString();
      var message = span.Slice(close + 3).ToString();
      return new ColumnAndMessage(column, message);
    }

    return new ColumnAndMessage(string.Empty, span.ToString());
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
}