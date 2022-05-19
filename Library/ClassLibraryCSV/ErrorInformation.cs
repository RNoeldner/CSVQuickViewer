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

    private const char cClosingField = ']';

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

      var sb = new StringBuilder();

      // If the new message is considered an error put it in front, this way its easier to check if
      // there is an error
      if (newError.IsErrorMessage() && errorList.IsWarningMessage())
      {
        // Put in front of previous messages, to have errors first
        sb.Append(newError);
        sb.Append(cSeparator);
        sb.Append(errorList);
      }
      else
      {
        // Append to previous messages
        sb.Append(errorList);
        sb.Append(cSeparator);
        sb.Append(newError);
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
    public static string CombineColumnAndError(string? column, in string errorMessage)
    {
      if (errorMessage is null)
        throw new ArgumentNullException(nameof(errorMessage));
      if (string.IsNullOrEmpty(column) && errorMessage.StartsWith("[", StringComparison.Ordinal))
        return errorMessage;
      return $"[{column}] {errorMessage}";
    }

    /// <summary>
    ///   Retrieved the error information from a <see cref="DataRow" /> in a text
    /// </summary>
    /// <param name="row">The DataRow with the error information</param>
    /// <returns>A string with all error information of the DataRow</returns>
    public static string GetErrorInformation(this DataRow row)
    {
      var list = new List<Tuple<string, string>>();
      if (!string.IsNullOrEmpty(row.RowError))
        list.Add(new Tuple<string, string>(string.Empty, row.RowError));
      list.AddRange(
        row.GetColumnsInError().Select(col => new Tuple<string, string>(col.ColumnName, row.GetColumnError(col))));
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
    public static Tuple<string, string> GetErrorsAndWarnings(this string errorList)
    {
      var sbErrors = new StringBuilder();
      var sbWaring = new StringBuilder();

      foreach (var parts in ParseList(errorList))
      {
        // errors are in front so if overall message is not an error there is no error
        var start = 0;
        while (start < parts.Item2.Length)
        {
          var end = parts.Item2.IndexOf(cSeparator, start + 1);
          if (end == -1)
            end = parts.Item2.Length;
          if (start < end)
          {
            var part = parts.Item2.Substring(start, end - start);
            if (part.IsWarningMessage())
            {
              if (sbWaring.Length > 0)
                sbWaring.Append(cSeparator);
              sbWaring.Append(
                parts.Item1.Length == 0
                  ? part.WithoutWarningId()
                  : CombineColumnAndError(parts.Item1, part.WithoutWarningId()));
            }
            else
            {
              if (sbErrors.Length > 0)
                sbErrors.Append(cSeparator);
              sbErrors.Append(parts.Item1.Length == 0 ? part : CombineColumnAndError(parts.Item1, part));
            }
          }

          start = end + 1;
        }
      }

      return new Tuple<string, string>(sbErrors.ToString(), sbWaring.ToString());
    }

    /// <summary>
    ///   String method to check if the text should be regarded as an error in an error list text
    /// </summary>
    /// <param name="errorList">A text containing different types of messages that are concatenated</param>
    /// <returns>
    ///   <c>true</c> if the text should be regarded as a error message, <c>false</c> if its a
    ///   warning message or empty
    /// </returns>
    public static bool IsErrorMessage(this string errorList)
    {
      if (errorList.Length == 0)
        return false;
      return !errorList.IsWarningMessage();
    }

    /// <summary>
    ///   String method to check if the text should is regarded as an warning in an error list text
    /// </summary>
    /// <param name="errorList">A text containing different types of messages that are concatenated</param>
    /// <returns>
    ///   <c>true</c> if the text should be regarded as a warning message, <c>false</c> if its an
    ///   error message or empty
    /// </returns>
    public static bool IsWarningMessage(this string errorList)
    {
      if (errorList.Length <= cWarningId.Length)
        return false;

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
    /// <param name="columnErrors">The column errors.</param>
    /// <param name="columns">The column names, for replacing the index to the name</param>
    /// <returns></returns>
    public static string ReadErrorInformation(IDictionary<int, string>? columnErrors, in IReadOnlyList<string> columns)
    {
      if (columnErrors is null || columnErrors.Count == 0)
        return string.Empty;
      var list = new List<Tuple<string, string>>();

      // Tried Parallel.Foreach but it was not reliable, with a few million executions some values
      // where wrong
      foreach (var entry in columnErrors)
      {
        if (entry.Key < -1) continue;
        var colName = entry.Key >= 0 && columns.Count > entry.Key ? columns[entry.Key] : string.Empty;
        var start = 0;
        while (start < entry.Value.Length)
        {
          var end = entry.Value.IndexOf(cSeparator, start + 1);
          if (end == -1)
            end = entry.Value.Length;
          // Add one Line for each error in the column
          list.Add(new Tuple<string, string>(colName, entry.Value.Substring(start, end - start)));
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
    public static void SetErrorInformation(this DataRow row, string? errorList)
    {
      row.ClearErrors();

      if (string.IsNullOrEmpty(errorList))
        return;

      foreach (var parts in ParseList(errorList))
        if (parts.Item1.Length == 0)
          row.RowError = parts.Item2;
        else
          SetColumnErrorInformation(row, parts);
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
    private static string BuildList(in ICollection<Tuple<string, string>> errorList)
    {
      var errors = new StringBuilder();

      // Errors first
      foreach (var (column, message) in errorList)
        if (!message.IsWarningMessage())
        {
          if (errors.Length > 0)
            errors.Append(cSeparator);
          errors.Append(CombineColumnAndError(column, message));
        }

      foreach (var part in from part in errorList
               where part.Item2.IsWarningMessage()
               select part)
      {
        if (errors.Length > 0)
          errors.Append(cSeparator);
        errors.Append(CombineColumnAndError(part.Item1, part.Item2));
      }

      return errors.ToString();
    }

    /// <summary>
    ///   Parses the list
    /// </summary>
    /// <param name="errorList">The error list.</param>
    /// <returns></returns>
    private static IEnumerable<Tuple<string, string>> ParseList(string? errorList)
    {
      if (errorList is null || errorList.Length == 0)
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

    /// <summary>
    ///   Sets the column error information.
    /// </summary>
    /// <param name="row">The row.</param>
    /// <param name="columnErrorInformation">The column error information.</param>
    private static void SetColumnErrorInformation(in DataRow row, in Tuple<string, string> columnErrorInformation)
    {
      var start = 0;
      // If we have combinations of columns, e,G. Combined Keys or Less Than errors store the error
      // in each column
      while (start < columnErrorInformation.Item1.Length)
      {
        var end = columnErrorInformation.Item1.IndexOf(cFieldSeparator, start + 1);
        if (end == -1)
          end = columnErrorInformation.Item1.Length;
        var colName = columnErrorInformation.Item1.Substring(start, end - start);
        if (row.Table.Columns.Contains(colName))
          row.SetColumnError(colName, row.GetColumnError(colName).AddMessage(columnErrorInformation.Item2));
        else
          row.RowError = row.RowError.AddMessage(
            CombineColumnAndError(columnErrorInformation.Item1, columnErrorInformation.Item2));
        start = end + 1;
      }
    }

    /// <summary>
    ///   Splits column and error information
    /// </summary>
    /// <param name="columnWithError">The column with error.</param>
    /// <returns></returns>
    private static Tuple<string, string> SplitColumnAndMessage(in string columnWithError)
    {
      var splitter = -2;
      if (columnWithError[0] != '[')
        return new Tuple<string, string>(string.Empty, columnWithError.Substring(splitter + 2));
      splitter = columnWithError.IndexOf(cClosingField);
      if (splitter == -1)
        splitter = -2;

      return new Tuple<string, string>(
        splitter <= 1 ? string.Empty : columnWithError.Substring(1, splitter - 1),
        columnWithError.Substring(splitter + 2));
    }
  }
}