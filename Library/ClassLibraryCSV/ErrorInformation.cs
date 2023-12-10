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
using System.Data;
using System.Linq;
using System.Text;

namespace CsvTools
{
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
    private const char cAlternateColumnMessageSeparator = ':';
    /// <summary>
    ///   Char to separate two column names
    /// </summary>
    private const char cFieldSeparator = ',';

    /// <summary>
    ///   Identifier for a warning message
    /// </summary>
    private const string cWarningId = "Warning: ";

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
    public static string AddMessage(this string errorList, in string newError, bool isWarning)
    {
      if (string.IsNullOrEmpty(newError))
        throw new ArgumentException("Error can not be empty", nameof(newError));

      // no need to check for null
      if (errorList.Length == 0)
        return newError;

      // if the message is already in the text do not do anything
      if (errorList.Contains(newError))
        return errorList;

      var sb = new StringBuilder(errorList.Length + newError.Length +1);

      // If the new message is considered an error put it in front, this way its easier to check if
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
    public static string AddMessage(this string errorList, in string newError)
    {
      if (string.IsNullOrEmpty(newError))
        throw new ArgumentException("Error can not be empty", nameof(newError));

      // no need to check for null
      if (errorList.Length == 0)
        return newError;

      // if the message is already in the text do not do anything
      if (errorList.Contains(newError))
        return errorList;

      var sb = new StringBuilder(errorList.Length + newError.Length +1);

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
    ///   Combines column and error information
    /// </summary>
    /// <param name="column">The column.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>An error message to be stored</returns>
    public static string CombineColumnAndError(in string column, in string errorMessage)
    {
      if (errorMessage is null)
        throw new ArgumentNullException(nameof(errorMessage));
      // pass back messages that already have column information
      if (column.Length==0 && errorMessage[0] == cOpenField)
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
      list.AddRange(row.GetColumnsInError().Select(col => new ColumnAndMessage(col.ColumnName, row.GetColumnError(col))));
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
    ///   <c>true</c> if the text should be regarded as a error message, <c>false</c> if it's a
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
      if (errorList.StartsWith(cWarningId, StringComparison.Ordinal))
        return true;

      // In case we have a column name in front we have to look into the middle of the string We
      // only look at the first entry, assuming error would be sorted into the front
      var splitter = errorList.IndexOf(cClosingField);
      return splitter != -1 && splitter < errorList.Length - 2
                            && errorList.Substring(splitter + 2).StartsWith(cWarningId, StringComparison.Ordinal);
    }

    /// <summary>
    ///   Stores the error information in a single string
    /// </summary>
    /// <param name="columnErrors">The column errors by column number</param>
    /// <param name="columnNames">The column names, for replacing the index to the name</param>
    /// <returns>The error message with a resulting of the column number to the column name</returns>
    public static string ReadErrorInformation(IDictionary<int, string>? columnErrors, in IReadOnlyList<string> columnNames)
    {
      if (columnErrors is null || columnErrors.Count == 0)
        return string.Empty;
      var list = new List<ColumnAndMessage>();

      // Tried Parallel.Foreach but it was not reliable, with a few million executions some values
      // where wrong
      foreach (var entry in columnErrors)
      {
        if (entry.Key < -1) continue;
        var colName = entry.Key >= 0 && columnNames.Count > entry.Key ? columnNames[entry.Key] : string.Empty;
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
            if (row.Table.Columns.Contains(colName) && !string.IsNullOrEmpty(parts.Message) )
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
    public static string WithoutWarningId(this string errorList) =>
      errorList.Length <= cWarningId.Length ? errorList : errorList.Substring(cWarningId.Length);

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
    ///   Parses the list
    /// </summary>
    /// <param name="errorList">The error list.</param>
    /// <returns></returns>
    private static IEnumerable<ColumnAndMessage> ParseList(string errorList)
    {
      if (errorList.Length == 0)
        yield break;
      var start = 0;
      while (start < errorList.Length)
      {
        var end = errorList.IndexOf(cSeparator, start + 1);
        if (end == -1)
          end = errorList.Length;
        yield return SplitColumnAndMessage(errorList.Substring(start, end - start));
        start = end + 1;
      }
    }

    public readonly struct ColumnAndMessage
    {
      public readonly string Column;
      public readonly string Message;

      public ColumnAndMessage(in string column, in string message)
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
    private static ColumnAndMessage SplitColumnAndMessage(in string text)
    {
      if (text[0] == cOpenField)
      {
        var close = text.IndexOf(cClosingField);
        return close == -1 ?
          new ColumnAndMessage(string.Empty, text) :
          new ColumnAndMessage(text.Substring(1, close - 1), text.Substring(close + 2));
      }

      var splitterAlt = text.IndexOf(cAlternateColumnMessageSeparator);
      return splitterAlt == -1 ?
        new ColumnAndMessage(string.Empty, text) :
        new ColumnAndMessage(text.Substring(0, splitterAlt), text.Substring(splitterAlt + 1).TrimStart());
    }
  }
}