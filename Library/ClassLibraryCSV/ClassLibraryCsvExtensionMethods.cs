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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   Class with extensions used in the class Library
  /// </summary>
  public static class ClassLibraryCsvExtensionMethods
  {
    public static string Description(this RecordDelimiterType item)
    {
      var descConv = new EnumDescriptionConverter(typeof(RecordDelimiterType));
      return descConv.ConvertToString(item);
    }

    public static string NewLineString(this RecordDelimiterType type)
    {
      if (type == RecordDelimiterType.LF) return "\n";
      else if (type == RecordDelimiterType.CR) return "\r";
      else if (type == RecordDelimiterType.CRLF) return "\r\n";
      else if (type == RecordDelimiterType.LFCR) return "\n\r";
      else if (type == RecordDelimiterType.RS) return "▲";
      else if (type == RecordDelimiterType.US) return "▼";
      return string.Empty;
    }

    /// <summary>
    ///   Check if the application should assume its gZIP
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public static bool AssumeGZip(this string fileName) =>
      fileName.EndsWith(".gz", StringComparison.OrdinalIgnoreCase) ||
      fileName.EndsWith(".gzip", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///   Check if the application should assume its PGP.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public static bool AssumePgp(this string fileName) =>
      fileName.EndsWith(".pgp", StringComparison.OrdinalIgnoreCase) ||
      fileName.EndsWith(".gpg", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///   Check if the application should assume its ZIP
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public static bool AssumeZip(this string fileName) => fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///   Copies all elements from one collection to the other
    /// </summary>
    /// <typeparam name="T">the type</typeparam>
    /// <param name="self">The collection.</param>
    /// <param name="other">The other collection.</param>
    [DebuggerStepThrough]
    public static void CollectionCopy<T>(this IEnumerable<T> self, ICollection<T> other) where T : ICloneable<T>
    {
      Contract.Requires(self != null);
      if (other == null) return;
      other.Clear();
      foreach (var item in self)
        other.Add(item.Clone());
    }

    /// <summary>
    ///   Copies all elements from one collection to the other
    /// </summary>
    /// <typeparam name="T">the type</typeparam>
    /// <param name="self">The collection.</param>
    /// <param name="other">The other collection.</param>
    [DebuggerStepThrough]
    public static void CollectionCopyStruct<T>(this IEnumerable<T> self, ICollection<T> other) where T : struct
    {
      Contract.Requires(self != null);
      if (other == null)
        return;

      other.Clear();
      foreach (var item in self)
        other.Add(item);
    }

    /// <summary>
    ///   Counts the items in the enumeration
    /// </summary>
    /// <param name="items">The items.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static int Count(this IEnumerable items)
    {
      switch (items)
      {
        case null:
          return 0;

        case ICollection col:
          return col.Count;

        default:
          return Enumerable.Count(items.Cast<object>());
      }
    }

    /// <summary>
    ///   User Display for a data type
    /// </summary>
    /// <param name="dt">The <see cref="DataType" />.</param>
    /// <returns>A text representing the dataType</returns>
    public static string DataTypeDisplay(this DataType dt)
    {
      Contract.Ensures(Contract.Result<string>() != null);

      switch (dt)
      {
        case DataType.DateTime:
          return "Date Time";

        case DataType.Integer:
          return "Integer";

        case DataType.Double:
          return "Floating  Point (High Range)";

        case DataType.Numeric:
          return "Money (High Precision)";

        case DataType.Boolean:
          return "Boolean";

        case DataType.Guid:
          return "Guid";

        case DataType.TextPart:
          return "Text Part";

        case DataType.TextToHtml:
          return "Encode HTML (Linefeed and CData Tags)";

        case DataType.TextToHtmlFull:
          return "Encode HTML ('<' -> '&lt;')";

        case DataType.String:
          return "Text";

        default:
          throw new ArgumentOutOfRangeException(nameof(dt), dt, "Data Type not known");
      }
    }

    /// <summary>
    ///   Gets the message of the current exception
    /// </summary>
    /// <param name="exception">The exception of type <see cref="Exception" /></param>
    /// <param name="maxDepth">The maximum depth.</param>
    /// <returns>A string with all messages in the error stack</returns>
    [DebuggerStepThrough]
    public static string ExceptionMessages(this Exception exception, int maxDepth = 3)
    {
      Contract.Requires(exception != null);
      Contract.Ensures(Contract.Result<string>() != null);

      var sb = new StringBuilder();

      // Special handling of AggregateException There can be many InnerExceptions
      if (exception is AggregateException ae)
      {
        foreach (var ie in ae.Flatten().InnerExceptions)
        {
          if (sb.Length > 0)
            sb.Append("\n");
          sb.Append(ie.Message);
        }

        return sb.ToString();
      }

      sb.Append(exception.Message);
      if (exception.InnerException != null)
      {
        sb.Append("\n");
        sb.Append(exception.InnerExceptionMessages(maxDepth - 1));
      }

      return sb.ToString();
    }

    /// <summary>
    ///   Gets the CsvTools type for a .NET type
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The matching <see cref="DataType" />.</returns>
    public static DataType GetDataType(this Type type)
    {
      Contract.Requires(type != null);
      switch (Type.GetTypeCode(type))
      {
        case TypeCode.Boolean:
          return DataType.Boolean;

        case TypeCode.DateTime:
          return DataType.DateTime;

        case TypeCode.Single:
        case TypeCode.Double:
          return DataType.Double;

        case TypeCode.Decimal:
          return DataType.Numeric;

        case TypeCode.Byte:
        case TypeCode.Int16:
        case TypeCode.Int32:
        case TypeCode.Int64:
        case TypeCode.SByte:
        case TypeCode.UInt16:
        case TypeCode.UInt32:
        case TypeCode.UInt64:
          return DataType.Integer;

        case TypeCode.Object when type.ToString().Equals("System.TimeSpan", StringComparison.Ordinal):
          return DataType.DateTime;

        case TypeCode.Object when type.ToString().Equals("System.Guid", StringComparison.Ordinal):
          return DataType.Guid;

        case TypeCode.Object:
          return DataType.String;

        default:
          return DataType.String;
      }
    }

    public static string NoRecordSQL(this string source)
    {
      if (string.IsNullOrEmpty(source))
        return string.Empty;

      // if its not SQL or has a Where condition do nothing
      if (!source.Contains("SELECT", StringComparison.OrdinalIgnoreCase))
        return source;

      if (source.Contains(" WHERE ", StringComparison.OrdinalIgnoreCase))
        return source.Replace(" WHERE ", " WHERE 1=0 AND ");

      // Remove Order By and Add a WHERE
      var indexOf = source.IndexOf("ORDER BY", StringComparison.OrdinalIgnoreCase);
      if (indexOf == -1)
        source += " WHERE 1=0";
      else
        source = source.Substring(0, indexOf) + "WHERE 1=0";

      return source;
    }

    public static char GetFirstChar(this string text) => string.IsNullOrEmpty(text) ? '\0' : text[0];

    /// <summary>
    ///   Gets a suitable ID for a filename
    /// </summary>
    /// <param name="path">The complete path to a file</param>
    /// <returns>The filename without special characters</returns>
    public static string GetIdFromFileName(this string path)
    {
      Contract.Requires(path != null);
      Contract.Ensures(Contract.Result<string>() != null);

      var fileName = StringUtils.ProcessByCategory(FileSystemUtils.GetFileName(path), x =>
        x == UnicodeCategory.UppercaseLetter || x == UnicodeCategory.LowercaseLetter ||
        x == UnicodeCategory.OtherLetter ||
        x == UnicodeCategory.ConnectorPunctuation || x == UnicodeCategory.DashPunctuation ||
        x == UnicodeCategory.OtherPunctuation ||
        x == UnicodeCategory.DecimalDigitNumber);

      const string c_TimeSep = @"(:|-|_)?";
      const string c_DateSep = @"(\/|\.|-|_)?";

      const string c_Hour = @"(2[0-3]|((0|1)\d))"; // 00-09 10-19 20-23
      const string c_MinSec = @"([0-5][0-9])"; // 00-59
      const string c_AmPm = @"((_| )?(AM|PM))?";

      const string c_Year = @"((19\d{2})|(2\d{3}))"; // 1900 - 2999
      const string c_Month = @"(0[1-9]|1[012])"; // 01-12
      const string c_Day = @"(0[1-9]|[12]\d|3[01])"; // 01 - 31

      // Replace Dates YYYYMMDDHHMMSS fileName = Regex.Replace(fileName, S + YYYY + S + MM + S + DD
      // + T + "?" + HH + T + MS + T + "?" + MS + "?" + TT, string.Empty, RegexOptions.IgnoreCase |
      // RegexOptions.Singleline | RegexOptions.CultureInvariant);

      // Replace Dates YYYYMMDD / MMDDYYYY / DDMMYYYY
      fileName = Regex.Replace(fileName,
        "(" + c_DateSep + c_Year + c_DateSep + c_Month + c_DateSep + c_Day + ")|(" + c_DateSep + c_Month + c_DateSep +
        c_Day + c_DateSep + c_Year + ")|(" + c_DateSep + c_Day + c_DateSep + c_Month + c_DateSep + c_Year + ")",
        string.Empty, RegexOptions.Singleline);

      // Replace Times 3_53_34_AM
      fileName = Regex.Replace(fileName,
        c_DateSep + c_Hour + c_TimeSep + c_MinSec + c_TimeSep + c_MinSec + "?" + c_AmPm, string.Empty,
        RegexOptions.IgnoreCase | RegexOptions.Singleline);
      /*
      // Replace Dates YYMMDD
      fileName = Regex.Replace(fileName, "(" + DateSep + YY + DateSep + MM + DateSep + DD + DateSep + ")", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
      // Replace Dates MMDDYY
      fileName = Regex.Replace(fileName, "(" + DateSep + MM + DateSep + DD + DateSep + YY + DateSep + ")", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
      // Replace Dates DDMMYY
      fileName = Regex.Replace(fileName, "(" + DateSep + DD + DateSep + MM + DateSep + YY + DateSep + ")", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
      */
      return fileName.Trim('_', '-', ' ', '\t').Replace("__", "_").Replace("__", "_").Replace("--", "-")
        .Replace("--", "-");
    }

    /// <summary>
    ///   Gets the .NET type for a given CsvTools type
    /// </summary>
    /// <param name="dt">The <see cref="DataType" />.</param>
    /// <returns>The .NET Type</returns>
    public static Type GetNetType(this DataType dt)
    {
      Contract.Ensures(Contract.Result<Type>() != null);

      switch (dt)
      {
        case DataType.DateTime:
          return typeof(DateTime);

        case DataType.Integer when IntPtr.Size == 4:
          return typeof(int);

        case DataType.Integer:
          return typeof(long);

        case DataType.Double:
          return typeof(double);

        case DataType.Numeric:
          return typeof(decimal);

        case DataType.Boolean:
          return typeof(bool);

        case DataType.Guid:
          return typeof(Guid);

        default:
          return typeof(string);
      }
    }

    /// <summary>
    ///   Common implementation of Read2DataTableAsync used for DbDataReader and IDataReaderAsync
    /// </summary>
    /// <param name="getSchemaTable"></param>
    /// <param name="readAsync"></param>
    /// <param name="getValues"></param>
    /// <param name="processDisplay"></param>
    /// <param name="recordLimit"></param>
    /// <returns></returns>
    //public static async Task<DataTable> Read2DataTableAsync(Func<DataTable> getSchemaTable, Func<Task<bool>> readAsync,
    //  Func<object[], int> getValues,
    //  IProcessDisplay processDisplay,
    //  long recordLimit)
    //{
    //  var dataTable = new DataTable();
    //  long oldMax = -1;
    //  var display = (processDisplay?.Maximum ?? 0) > 1
    //    ? "Reading rows\nRecord {0:N0}/" + $"{processDisplay?.Maximum:N0}"
    //    : "Reading rows\nRecords {0:N0}";

    // try { if (processDisplay != null && recordLimit > 0) { oldMax = processDisplay.Maximum;
    // processDisplay.Maximum = recordLimit; }

    // // create columns var schemaTable = getSchemaTable.Invoke(); if (schemaTable == null) return
    // null; var columns = schemaTable.Rows.Count;

    // // We could have duplicate column names in this case we have need to adjust the conflicting
    // name var previousColumns = new List<string>(); foreach (DataRow dataRow in schemaTable.Rows)
    // { var colName = StringUtils.MakeUniqueInCollection(previousColumns,
    // (string)dataRow["ColumnName"]); dataTable.Columns.Add(new DataColumn(colName,
    // (Type)dataRow["DataType"]) { AllowDBNull = true }); previousColumns.Add(colName); }

    // if (!(processDisplay?.CancellationToken.IsCancellationRequested ?? false)) {
    // dataTable.BeginLoadData(); if (recordLimit < 1) recordLimit = long.MaxValue; // load the Data
    // into the dataTable var action = processDisplay == null ? null : new IntervalAction(.3); while
    // (await readAsync() && dataTable.Rows.Count < recordLimit &&
    // !(processDisplay?.CancellationToken.IsCancellationRequested ?? false)) { var readerValues =
    // new object[columns]; if (getValues(readerValues) > 0) dataTable.Rows.Add(readerValues);
    // action?.Invoke(() => processDisplay.SetProcess(string.Format(display, dataTable.Rows.Count),
    // dataTable.Rows.Count, false)); } } } finally { if (processDisplay != null) {
    // processDisplay.SetProcess(string.Format(display, dataTable.Rows.Count), dataTable.Rows.Count,
    // false); if (oldMax > 0) processDisplay.Maximum = oldMax; }

    // dataTable.EndLoadData(); }

    //  return dataTable;
    //}

    /// <summary>
    ///   Get a list of column names that are not artificial
    /// </summary>
    /// <param name="dataTable">The <see cref="DataTable" /> containing the columns</param>
    /// <returns>A enumeration of ColumnNames</returns>
    public static IEnumerable<string> GetRealColumns(this DataTable dataTable)
    {
      Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
      foreach (var x in GetRealDataColumns(dataTable))
        yield return x.ColumnName;
    }

    /// <summary>
    ///   Get a list of columns that are not artificial
    /// </summary>
    /// <param name="dataTable">The <see cref="DataTable" /> containing the columns</param>
    /// <returns>A enumeration of <see cref="DataColumn" /></returns>
    public static IEnumerable<DataColumn> GetRealDataColumns(this DataTable dataTable)
    {
      Contract.Ensures(Contract.Result<IEnumerable<DataColumn>>() != null);

      if (dataTable == null)
        yield break;
      foreach (DataColumn col in dataTable.Columns)
        if (!BaseFileReader.ArtificialFields.Contains(col.ColumnName))
          yield return col;
    }

    /// <summary>
    ///   Combines all inner exceptions to one formatted string for logging.
    /// </summary>
    /// <param name="exception">The exception of type <see cref="Exception" /></param>
    /// <param name="maxDepth">The maximum depth.</param>
    /// <returns>A string with all inner messages of the error stack</returns>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    [DebuggerStepThrough]
    public static string InnerExceptionMessages(this Exception exception, int maxDepth = 2)
    {
      Contract.Ensures(Contract.Result<string>() != null);
      if (exception == null)
        return string.Empty;
      try
      {
        var sb = new StringBuilder();
        while (exception.InnerException != null && maxDepth > 0)
        {
          if (sb.Length > 0)
            sb.Append('\n');
          sb.Append(exception.InnerException.Message);
          exception = exception.InnerException;
          maxDepth--;
        }

        // if there is no inner Exception fall back to the exception
        if (sb.Length == 0)
          sb.Append(exception.Message);
        return sb.ToString();
      }
      catch (Exception)
      {
        // Ignore problems within this method - there's nothing more stupid than an error in the
        // error handler
        return string.Empty;
      }
    }

    /// <summary>
    ///   Gets the type of the placeholder.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="placeholder">The placeholder.</param>
    /// <returns></returns>
    private static string GetPlaceholderType(this string input, string placeholder)
    {
      var type = "{" + placeholder + "}";
      if (input.IndexOf(type, StringComparison.OrdinalIgnoreCase) != -1)
        return type;
      type = "#" + placeholder + "#";
      if (input.IndexOf(type, StringComparison.OrdinalIgnoreCase) != -1)
        return type;

      type = "#" + placeholder;
      if (input.EndsWith(type, StringComparison.OrdinalIgnoreCase) ||
          input.IndexOf(type + " ", StringComparison.OrdinalIgnoreCase) != -1)
        return type;
      return null;
    }

    /// <summary>
    ///   Places the holder times.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="format">The format.</param>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="lastExecution">The last execution.</param>
    /// <param name="lastExecutionStart">The last execution start.</param>
    /// <returns></returns>
    public static string PlaceHolderTimes(this string text, string format, IFileSetting fileSetting,
      DateTime lastExecution, DateTime lastExecutionStart)
    {
      if (string.IsNullOrEmpty(text)) return text;
      if (fileSetting.ProcessTimeUtc != BaseSettings.ZeroTime)
      {
        var value = fileSetting.ProcessTimeUtc.ToString(format);
        text = text.PlaceholderReplace("LastRunUTC", value);
      }

      if (lastExecutionStart != BaseSettings.ZeroTime)
      {
        var value = lastExecutionStart.ToString(format);
        text = text.PlaceholderReplace("ScriptStartUTC", value);
      }

      if (lastExecution == BaseSettings.ZeroTime) return text;
      {
        var value = lastExecution.ToString(format);
        text = text.PlaceholderReplace("LastScriptEndUTC", value);
      }

      return text;
    }

    /// <summary>
    ///   Replaces a placeholders with a text. The placeholder are identified surrounding { or a
    ///   leading #
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="placeholder">The placeholder.</param>
    /// <param name="replacement">The replacement.</param>
    /// <returns>The new text based on input</returns>
    /// [DebuggerStepThrough]
    public static string PlaceholderReplace(this string input, string placeholder, string replacement)
    {
      if (string.IsNullOrEmpty(replacement)) return input;
      var type = input.GetPlaceholderType(placeholder);
      if (type != null)
      {
        if (input.IndexOf(" - " + type, StringComparison.OrdinalIgnoreCase) != -1)
        {
          type = " - " + type;
        }
        else if (input.IndexOf(" " + type, StringComparison.OrdinalIgnoreCase) != -1)
        {
          type = " " + type;
          replacement = " " + replacement;
        }
        else if (input.IndexOf(type + " ", StringComparison.OrdinalIgnoreCase) != -1)
        {
          replacement += " ";
        }

        return input.ReplaceCaseInsensitive(type, replacement);
      }

      return input;
    }

    /// <summary>
    ///   Remove all mapping that do not have a source
    /// </summary>
    /// <param name="columns">List of columns</param>
    /// <param name="fileSetting">The setting.</param>
    public static IEnumerable<string> RemoveMappingWithoutSource(this IFileSetting fileSetting,
      IEnumerable<string> columns)
    {
      Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
      var notFoundColumnNames = new List<string>();

      if (fileSetting == null || columns == null)
        return notFoundColumnNames;
      notFoundColumnNames.AddRange(
        from map in fileSetting.MappingCollection
        where !columns.Any(col => col.Equals(map.FileColumn, StringComparison.OrdinalIgnoreCase))
        select map.FileColumn);

      if (notFoundColumnNames.Count <= 0)
        return notFoundColumnNames;

      foreach (var notFound in notFoundColumnNames)
      {
        Logger.Warning(
          "Column {columnname} was expected but was not found in {filesetting}. The invalid column mapping will be removed.",
          notFound, fileSetting.ToString());
        fileSetting.MappingCollection.RemoveColumn(notFound);
      }

      return notFoundColumnNames;
    }

    /// <summary>
    ///   String replace function that is Case Insensitive
    /// </summary>
    /// <param name="original">The source</param>
    /// <param name="pattern">The text to replace</param>
    /// <param name="replacement">the character to which it should be changed</param>
    /// <returns>The source text with the replacement</returns>
    [DebuggerStepThrough]
    public static string ReplaceCaseInsensitive(this string original, string pattern, char replacement)
    {
      Contract.Requires(original != null);
      Contract.Ensures(Contract.Result<string>() != null);

      var count = 0;
      var position0 = 0;
      int position1;

      if (string.IsNullOrEmpty(pattern))
        return original;

      var upperString = original.ToUpperInvariant();
      var upperPattern = pattern.ToUpperInvariant();

      var inc = original.Length / pattern.Length * (1 - pattern.Length);
      var chars = new char[original.Length + Math.Max(0, inc)];

      while ((position1 = upperString.IndexOf(upperPattern, position0, StringComparison.Ordinal)) != -1)
      {
        for (var i = position0; i < position1; ++i)
          chars[count++] = original[i];
        chars[count++] = replacement;
        position0 = position1 + pattern.Length;
      }

      if (position0 == 0)
        return original;
      for (var i = position0; i < original.Length; ++i)
        chars[count++] = original[i];
      return new string(chars, 0, count);
    }

    /// <summary>
    ///   String replace function that is Case Insensitive
    /// </summary>
    /// <param name="original">The source</param>
    /// <param name="pattern">The text to replace</param>
    /// <param name="replacement">the text to which it should be changed</param>
    /// <returns>The source text with the replacement</returns>
    [DebuggerStepThrough]
    public static string ReplaceCaseInsensitive(this string original, string pattern, string replacement)
    {
      Contract.Requires(original != null);
      Contract.Ensures(Contract.Result<string>() != null);

      if (string.IsNullOrEmpty(pattern))
        return original;

      if (replacement == null)
        replacement = string.Empty;

      // if pattern matches replacement exit
      if (replacement.Equals(pattern, StringComparison.Ordinal))
        return original;

      var inc = original.Length / pattern.Length * (replacement.Length - pattern.Length);
      var chars = new char[original.Length + Math.Max(0, inc)];

      var count = 0;
      var positionLast = 0;
      int positionNew;

      while ((positionNew = original.IndexOf(pattern, positionLast, StringComparison.OrdinalIgnoreCase)) != -1)
      {
        for (var i = positionLast; i < positionNew; ++i)
          chars[count++] = original[i];
        foreach (var t in replacement)
          chars[count++] = t;

        positionLast = positionNew + pattern.Length;
      }

      if (positionLast == 0)
        return original;

      for (var i = positionLast; i < original.Length; ++i)
        chars[count++] = original[i];

      return new string(chars, 0, count);
    }

    /// <summary>
    ///   Replaces the two string
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <param name="old1">The old1.</param>
    /// <param name="new1">The new1.</param>
    /// <param name="old2">The old2.</param>
    /// <param name="new2">The new2.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static string ReplaceDefaults(this string inputValue, string old1, string new1, string old2, string new2)
    {
      Contract.Requires(!string.IsNullOrEmpty(old1));
      Contract.Ensures(Contract.Result<string>() != null);

      if (string.IsNullOrEmpty(inputValue))
        return string.Empty;

      var exchange1 = !string.IsNullOrEmpty(old1) && string.Compare(old1, new1, StringComparison.Ordinal) != 0;
      var exchange2 = !string.IsNullOrEmpty(old2) && string.Compare(old2, new2, StringComparison.Ordinal) != 0;
      if (exchange1 && exchange2 && new1 == old2)
      {
        inputValue = inputValue.Replace(old1, "{\0}");
        inputValue = inputValue.Replace(old2, new2);
        inputValue = inputValue.Replace("{\0}", new1);
      }
      else
      {
        if (exchange1)
          inputValue = inputValue.Replace(old1, new1);
        if (exchange2)
          inputValue = inputValue.Replace(old2, new2);
      }

      return inputValue;
    }

    /// <summary>
    ///   Replace placeholder in a template with value of property
    /// </summary>
    /// <param name="template">The template with placeholder in {}, e.G. ID:{ID}</param>
    /// <param name="obj">The object that is used to look at the properties</param>
    /// <returns>Any found property placeholder is replaced by the property value</returns>
    [DebuggerStepThrough]
    public static string ReplacePlaceholderWithPropertyValues(this string template, object obj)
    {
      if (template.IndexOf('{') == -1)
        return template;

      // get all placeholders in {}
      var rgx = new Regex(@"\{[^\}]+\}");

      var placeholder = new Dictionary<string, string>();
      var props = obj.GetType().GetProperties().Where(prop => prop.GetMethod != null).ToList();

      foreach (Match match in rgx.Matches(template))
        if (!placeholder.ContainsKey(match.Value))
        {
          PropertyInfo prop = props.FirstOrDefault(
            x => x.Name.Equals(match.Value.Substring(1, match.Value.Length - 2), StringComparison.OrdinalIgnoreCase));

          if (prop != null)
            placeholder.Add(match.Value, prop.GetValue(obj).ToString());
        }

      // replace them with the property value from setting
      template = placeholder.Aggregate(template, (current, pro) => current.ReplaceCaseInsensitive(pro.Key, pro.Value));

      return template.Replace("  ", " ");
    }

    /// <summary>
    ///   Replace placeholder in a template with the text provide in the parameters the order of the
    ///   placeholders is important not their contend
    /// </summary>
    /// <param name="template">The template with placeholder in {}, e.G. ID:{ID}</param>
    /// <param name="values">
    ///   a variable number of text that will replace the placeholder in order of appearance
    /// </param>
    /// <returns>Any found property placeholder is replaced by the provide text</returns>
    [DebuggerStepThrough]
    public static string ReplacePlaceholderWithText(this string template, params string[] values)
    {
      if (template.IndexOf('{') == -1)
        return template;

      // get all placeholders in {}
      var rgx = new Regex(@"\{[^\}]+\}");

      var placeholder = new Dictionary<string, string>();
      var index = 0;
      foreach (Match match in rgx.Matches(template))
      {
        if (index >= values.Length)
          break;
        if (!placeholder.ContainsKey(match.Value))
          placeholder.Add(match.Value, values[index++]);
      }

      // replace them with the property value from setting
      template = placeholder.Aggregate(template, (current, pro) => current.ReplaceCaseInsensitive(pro.Key, pro.Value));

      return template.Replace("  ", " ");
    }

    public static string CsvToolsStackTrace(this Exception exception)
    {
      if (string.IsNullOrEmpty(exception.StackTrace))
        return null;

      var start = exception.StackTrace.LastIndexOf("   ", StringComparison.Ordinal);
      if (start == -1) return null;
      start = exception.StackTrace.IndexOf(" ", start + 3, StringComparison.Ordinal);
      return $"at {exception.StackTrace.Substring(start)}";
    }

    /// <summary>
    ///   Get the inner most exception message
    /// </summary>
    /// <param name="exception">Any exception <see cref="Exception" /></param>
    [DebuggerStepThrough]
    public static string SourceExceptionMessage(this Exception exception)
    {
      var loop = exception;
      while (loop.InnerException != null)
        loop = loop.InnerException;

      return loop.Message;
    }

    public static int ToInt(this long value)
    {
      if (value > int.MaxValue)
        return int.MaxValue;
      return value < int.MinValue ? int.MinValue : Convert.ToInt32(value);
    }

    /// <summary>
    ///   Run a task to completion with timeout You should you expose synchronous wrappers for
    ///   asynchronous methods, still here it is be very careful
    /// </summary>
    /// <param name="executeTask">The started <see cref="System.Threading.Tasks.Task" /></param>
    /// <param name="timeoutSeconds">
    ///   Timeout for the completion of the task, if more time is spent running / waiting the wait
    ///   is finished
    /// </param>
    /// <param name="cancellationToken">
    ///   Best is to start tasks with the cancellation token but some async methods do not do, so it
    ///   can be provided
    /// </param>
    /// <remarks>Will only return the first exception in case of aggregate exceptions.</remarks>
    public static void WaitToCompleteTask(this Task executeTask, double timeoutSeconds,
      CancellationToken cancellationToken = default)
    {
      if (executeTask == null)
        throw new ArgumentNullException(nameof(executeTask));

      if (executeTask.IsCompleted)
      {
        if (executeTask.Exception != null)
          throw executeTask.Exception.Flatten().InnerExceptions[0];
        return;
      }

      cancellationToken.ThrowIfCancellationRequested();

      var stopwatch = timeoutSeconds > 0.01 ? new Stopwatch() : null;
      stopwatch?.Start();

      try
      {
        // IsCompleted := RanToCompletion || Canceled || Faulted
        while (!executeTask.IsCompleted)
        {
          cancellationToken.ThrowIfCancellationRequested();
          if (stopwatch != null)
            // Raise an exception when not activated
            //if (executeTask.Status == TaskStatus.WaitingForActivation && stopwatch.Elapsed.TotalSeconds > timeoutSeconds)
            //  throw new TimeoutException($"Waited longer than {stopwatch.Elapsed.TotalSeconds:N1} seconds for task activation");

            // Raise an exception when waiting too long
            if (timeoutSeconds > 0 && stopwatch.Elapsed.TotalSeconds > timeoutSeconds)
              throw new TimeoutException($"Timeout after {stopwatch.Elapsed.TotalSeconds:N1} seconds");

          // Invoke action every 1/4 second FunctionalDI.SignalBackground?.Invoke();
          FunctionalDI.SignalBackground?.Invoke();

          // wait will raise an AggregateException if the task throws an exception
          executeTask.Wait(250, cancellationToken);
        }

        if (executeTask.IsFaulted && executeTask.Exception != null)
          throw executeTask.Exception;
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "{exception}", ex.ExceptionMessages());

        // return only the first exception if there are many
        if (ex is AggregateException ae)
          throw ae.Flatten().InnerExceptions[0];
        throw;
      }
    }

    /// <summary>
    ///   Run a task to completion with timeout You should you expose synchronous wrappers for
    ///   asynchronous methods, still here it is be very careful
    /// </summary>
    /// <param name="executeTask">
    ///   The started <see cref="System.Threading.Tasks.Task" /> with a return value
    /// </param>
    /// <param name="timeoutSeconds">
    ///   Timeout for the completion of the task, if more time is spent running / waiting the wait
    ///   is finished
    /// </param>
    /// <param name="cancellationToken">
    ///   Best is to start tasks with the cancellation token but some async methods do not do, so it
    ///   can be provided
    /// </param>
    /// <returns>Task Result if finished successfully, otherwise raises an error</returns>
    public static T WaitToCompleteTask<T>(this Task<T> executeTask, double timeoutSeconds,
      CancellationToken cancellationToken = default)
    {
      WaitToCompleteTask((Task)executeTask, timeoutSeconds, cancellationToken);
      return executeTask.Result;
    }

    /// <summary>
    ///   Replaces a written English punctuation to the punctuation character
    /// </summary>
    /// <param name="inputString">The source</param>
    /// <returns>return '\0' if the text was not interpreted as punctuation</returns>
    public static char WrittenPunctuationToChar(this string inputString)
    {
      Contract.Requires(inputString != null);

      if (inputString.Equals("Tab", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Tabulator", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Horizontal Tab", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("HorizontalTab", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("\t", StringComparison.Ordinal))
        return '\t';

      if (inputString.Equals("Space", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals(" ", StringComparison.Ordinal))
        return ' ';

      if (inputString.Equals("hash", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("sharp", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("#", StringComparison.Ordinal))
        return '#';

      if (inputString.Equals("whirl", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("at", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("monkey", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("@", StringComparison.Ordinal))
        return '@';

      if (inputString.Equals("underbar", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("underscore", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("understrike", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("_", StringComparison.Ordinal))
        return '_';

      if (inputString.Equals("Comma", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals(",", StringComparison.Ordinal))
        return ',';

      if (inputString.Equals("Dot", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Point", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Full Stop", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals(".", StringComparison.Ordinal))
        return '.';

      if (inputString.Equals("amper", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("ampersand", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("&", StringComparison.Ordinal))
        return '&';

      if (inputString.Equals("Pipe", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Vertical bar", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("VerticalBar", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("|", StringComparison.Ordinal))
        return '|';

      if (inputString.Equals("broken bar", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("BrokenBar", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("¦", StringComparison.Ordinal))
        return '¦';

      if (inputString.Equals("fullwidth broken bar", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("FullwidthBrokenBar", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("￤", StringComparison.Ordinal))
        return '￤';

      if (inputString.Equals("Semicolon", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals(";", StringComparison.Ordinal))
        return ';';

      if (inputString.Equals("Colon", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals(":", StringComparison.Ordinal))
        return ':';

      if (inputString.Equals("Doublequote", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Doublequotes", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Quote", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Quotation marks", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("\"", StringComparison.Ordinal))
        return '"';

      if (inputString.Equals("Apostrophe", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Singlequote", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("tick", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("'", StringComparison.Ordinal))
        return '\'';

      if (inputString.Equals("Slash", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Stroke", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("forward slash", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("/", StringComparison.Ordinal))
        return '/';

      if (inputString.Equals("backslash", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("backslant", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("\\", StringComparison.Ordinal))
        return '\\';

      if (inputString.Equals("Tick", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Tick Mark", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("`", StringComparison.Ordinal))
        return '`';

      if (inputString.Equals("Star", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Asterisk", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("*", StringComparison.Ordinal))
        return '*';

      if (inputString.Equals("NBSP", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Non-breaking space", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Non breaking space", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("NonBreakingSpace", StringComparison.OrdinalIgnoreCase))
        return '\u00A0';

      if (inputString.Equals("Return", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("CarriageReturn", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("CR", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("␍", StringComparison.Ordinal) ||
          inputString.Equals("Carriage return", StringComparison.OrdinalIgnoreCase))
        return '\r';

      if (inputString.Equals("Check mark", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Check", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("✓", StringComparison.OrdinalIgnoreCase))
        return '✓';

      if (inputString.Equals("Feed", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("LineFeed", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("LF", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("␊", StringComparison.Ordinal) ||
          inputString.Equals("Line feed", StringComparison.OrdinalIgnoreCase))
        return '\n';

      if (inputString.StartsWith("Unit separator", StringComparison.OrdinalIgnoreCase) ||
          inputString.Contains("31") ||
          inputString.Equals("␟", StringComparison.Ordinal) ||
          inputString.Equals("US", StringComparison.OrdinalIgnoreCase))
        return '\u001F';

      if (inputString.StartsWith("Record separator", StringComparison.OrdinalIgnoreCase) ||
          inputString.Contains("30") ||
          inputString.Equals("␞", StringComparison.Ordinal) ||
          inputString.Equals("RS", StringComparison.OrdinalIgnoreCase))
        return '\u001E';

      if (inputString.StartsWith("Group separator", StringComparison.OrdinalIgnoreCase) ||
          inputString.Contains("29") ||
          inputString.Equals("GS", StringComparison.OrdinalIgnoreCase))
        return '\u001D';

      if (inputString.StartsWith("File separator", StringComparison.OrdinalIgnoreCase) ||
          inputString.Contains("28") ||
          inputString.Equals("FS", StringComparison.OrdinalIgnoreCase))
        return '\u001C';

      return '\0';
    }

#if !GetHashByGUID

    /// <summary>
    ///   Check if a collection is equal, the items can be in any order as long as all exist in the
    ///   th other
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self">The collection.</param>
    /// <param name="other">The other collection.</param>
    /// <returns></returns>
    public static bool CollectionEqual<T>(this ICollection<T> self, ICollection<T> other) where T : IEquatable<T>
    {
      if (self == null)
        throw new ArgumentNullException(nameof(self));
      if (other == null)
        return false;
      if (ReferenceEquals(other, self))
        return true;
      if (other.Count != self.Count)
        return false;
      // Check the item, all should be the same, order does not matter though
      return other.All(ot => ot == null ? self.Any(x => x == null) : self.Any(th => ot.Equals(th)));
    }

    /// <summary>
    ///   Check if two enumerations are equal, the items need to be in the right order
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self">The collection.</param>
    /// <param name="other">The other collection.</param>
    /// <returns></returns>
    public static bool CollectionEqualWithOrder<T>(this IEnumerable<T> self, IEnumerable<T> other)
      where T : IEquatable<T>
    {
      if (self == null)
        throw new ArgumentNullException(nameof(self));
      if (other == null)
        return false;

      if (ReferenceEquals(other, self))
        return true;

      // Shortcut if we have collections but different number of Items
      if (self is ICollection<T> selfCol && other is ICollection<T> otherCol)
        if (selfCol.Count != otherCol.Count)
          return false;

      // use Enumerators to compare the two collections
      var comparer = EqualityComparer<T>.Default;
      using (var selfEnum = self.GetEnumerator())
      using (var otherEnum = other.GetEnumerator())
      {
        while (true)
        {
          // move to the next item
          var s = selfEnum.MoveNext();
          var o = otherEnum.MoveNext();
          if (!s && !o)
            return true;
          if (!s || !o)
            return false;
          if (!comparer.Equals(selfEnum.Current, otherEnum.Current))
            return false;
        }
      }
    }

    /// <summary>
    ///   Get the hash code of a collection, the order of items should not matter.
    /// </summary>
    /// <param name="collection">The collection itself.</param>
    /// <returns></returns>
    public static int CollectionHashCode(this ICollection collection)
    {
      unchecked
      {
        var order = 0;
        return collection.Cast<object>().Aggregate(731, (current, item) => (current * 397) ^ (item.GetHashCode() + order++));
      }
    }

#endif
  }
}