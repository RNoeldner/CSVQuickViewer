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
    [DebuggerStepThrough]
    public static void AddComma(this StringBuilder sb)
    {
      if (sb.Length > 0)
        sb.Append(", ");
    }

    /// <summary>
    ///   Check if the application should assume its gZIP
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public static bool AssumeGZip(this string fileName) => fileName.EndsWith(".gz", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".gzip", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///   Check if the application should assume its PGP.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public static bool AssumePgp(this string fileName) => fileName.EndsWith(".pgp", StringComparison.OrdinalIgnoreCase) ||
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
      if (other == null)
        return;

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
    ///   Copies the row to table.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="dataTable">The data table.</param>
    /// <param name="columnWarningsReader">The column warnings reader.</param>
    /// <param name="dataTableInfo">The data table information.</param>
    /// <param name="handleColumnIssues">The handle column issues.</param>
    public static void CopyRowToTable(this IFileReader reader, DataTable dataTable,
      ColumnErrorDictionary columnWarningsReader,
      CopyToDataTableInfo dataTableInfo, Action<ColumnErrorDictionary, DataRow> handleColumnIssues)
    {
      Contract.Requires(dataTable != null);

      var dataRow = dataTable.NewRow();
      if (dataTableInfo.RecordNumber != null)
        dataRow[dataTableInfo.RecordNumber] = reader.RecordNumber;

      if (dataTableInfo.EndLine != null)
        dataRow[dataTableInfo.EndLine] = reader.EndLineNumber;

      if (dataTableInfo.StartLine != null)
        dataRow[dataTableInfo.StartLine] = reader.StartLineNumber;
      dataTable.Rows.Add(dataRow);

      // TODO: check if this is really necessary, usually GetValue  should not error out
      try
      {
        foreach (var keyValuePair in dataTableInfo.Mapping)
          dataRow[keyValuePair.Value] = reader.GetValue(keyValuePair.Key);
      }
      catch (Exception exc)
      {
        columnWarningsReader.Add(-1, exc.ExceptionMessages());
      }

      if (columnWarningsReader.Count > 0)
      {
        handleColumnIssues?.Invoke(columnWarningsReader, dataRow);
        columnWarningsReader.Clear();
      }
    }

    /// <summary>
    ///   Counts the items in the enumeration
    /// </summary>
    /// <param name="items">The items.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static int Count(this IEnumerable items)
    {
      if (items == null)
        return 0;

      if (items is ICollection col)
        return col.Count;

      return Enumerable.Count(items.Cast<object>());
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

        default:
          return "Text";
      }
    }

    /// <summary>
    ///   Gets the message of the current exception
    /// </summary>
    /// <param name="exception">The exception of type <see cref="Exception" /></param>
    /// <param name="maxDepth">The maximum depth.</param>
    /// <returns>
    ///   A string with all messages in the error stack
    /// </returns>
    [DebuggerStepThrough]
    public static string ExceptionMessages(this Exception exception, int maxDepth = 3)
    {
      Contract.Requires(exception != null);
      Contract.Ensures(Contract.Result<string>() != null);

      var sb = new StringBuilder();

      // Special handling of AggregateException
      // There can be many InnerExceptions
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

        case TypeCode.Object:
          if (type.ToString().Equals("System.TimeSpan", StringComparison.Ordinal))
            return DataType.DateTime;
          if (type.ToString().Equals("System.Guid", StringComparison.Ordinal))
            return DataType.Guid;
          return DataType.String;

        default:
          return DataType.String;
      }
    }

    public static char GetFirstChar(this string text)
    {
      if (string.IsNullOrEmpty(text))
        return '\0';

      return text[0];
    }

    /// <summary>
    ///   Gets a suitable ID for a filename
    /// </summary>
    /// <param name="path">The complete path to a file</param>
    /// <returns>The filename without special characters</returns>
    public static string GetIdFromFileName(this string path)
    {
      Contract.Requires(path != null);
      Contract.Ensures(Contract.Result<string>() != null);

      var fileName = StringUtils.ProcessByCategory(FileSystemUtils.SplitPath(path).FileName, x =>
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

      // Replace Dates YYYYMMDDHHMMSS
      // fileName = Regex.Replace(fileName, S + YYYY + S + MM + S + DD + T + "?" + HH + T + MS + T + "?" + MS + "?" + TT, string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

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

        case DataType.Integer:
          if (IntPtr.Size == 4)
            return typeof(int);
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


    public static DataTable Read2DataTable(this IDataReader dataReader, IProcessDisplay processDisplay, long recordLimit)
    {
      var dataTable = new DataTable();
      var display = (processDisplay?.Maximum ?? 0) > 1 ? "Reading rows\nRecord {0:N0}/" + $"{processDisplay?.Maximum:N0}" : "Reading rows\nRecords {0:N0}";
      try
      {
        if (processDisplay != null && recordLimit > 0)
          processDisplay.Maximum = recordLimit;

        // create columns
        var schemaTable = dataReader.GetSchemaTable();
        if (schemaTable == null)
          return null;
        var columns = schemaTable.Rows.Count;

        // We could have duplicate column names
        // in this case we have need to adjust the conflicting name
        var previousColumns = new List<string>();
        foreach (DataRow dataRow in schemaTable.Rows)
        {
          var colName = StringUtils.MakeUniqueInCollection(previousColumns, (string)dataRow["ColumnName"]);
          dataTable.Columns.Add(new DataColumn(colName, (Type)dataRow["DataType"]) { AllowDBNull = true });
          previousColumns.Add(colName);
        }

        dataTable.BeginLoadData();
        if (recordLimit < 1)
          recordLimit = int.MaxValue;
        // load the Data into the dataTable        
        var action = new IntervalAction(.3);
        while (dataReader.Read() && dataTable.Rows.Count < recordLimit && !processDisplay.CancellationToken.IsCancellationRequested)
        {
          var readerValues = new object[columns];
          if (dataReader.GetValues(readerValues) > 0)
            dataTable.Rows.Add(readerValues);
          action.Invoke(() => processDisplay?.SetProcess(string.Format(display, dataTable.Rows.Count), dataTable.Rows.Count, false));
        }
      }
      finally
      {
        processDisplay?.SetProcess(string.Format(display, dataTable.Rows.Count), dataTable.Rows.Count, false);
        dataTable.EndLoadData();
      }
      return dataTable;
    }

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
        if (!BaseFileReader.ArtificalFields.Contains(col.ColumnName))
          yield return col;
    }

    /// <summary>
    ///   Combines all inner exceptions to one formatted string for logging.
    /// </summary>
    /// <param name="exception">The exception of type <see cref="Exception" /></param>
    /// <param name="maxDepth">The maximum depth.</param>
    /// <returns>
    ///   A string with all inner messages of the error stack
    /// </returns>
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

    public static string GetPlaceholderType(this string input, string placeholder)
    {
      var type = "{" + placeholder + "}";
      if (input.IndexOf(type, StringComparison.OrdinalIgnoreCase) != -1)
        return type;
      type = "#" + placeholder + "#";
      if (input.IndexOf(type, StringComparison.OrdinalIgnoreCase) != -1)
        return type;

      type = "#" + placeholder;
      if (input.EndsWith(type, StringComparison.OrdinalIgnoreCase) || input.IndexOf(type + " ", StringComparison.OrdinalIgnoreCase) != -1)
        return type;
      return null;
    }

    public static string PlaceHolderTimes(this string text, string format, IFileSetting fileSetting, DateTime LastExecution, DateTime LastExecutionStart)
    {
      if (!string.IsNullOrEmpty(text))
      {
        if (fileSetting.ProcessTimeUtc != BaseSettings.ZeroTime)
        {
          var value = fileSetting.ProcessTimeUtc.ToString(format);
          text = text.PlaceholderReplace("LastRunUTC", value);
        }
        if (LastExecutionStart != BaseSettings.ZeroTime)
        {
          var value = LastExecutionStart.ToString(format);
          text = text.PlaceholderReplace("ScriptStartUTC", value);
        }
        if (LastExecution != BaseSettings.ZeroTime)
        {
          var value = LastExecution.ToString(format);
          text = text.PlaceholderReplace("LastScriptEndUTC", value);
        }
      }
      return text;
    }

    /// <summary>
    ///   Replaces a placeholders with a text. The placeholder are identified surrounding { or a leading #
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="placeholder">The placeholder.</param>
    /// <param name="replacement">The replacement.</param>
    /// <returns>The new text based on input</returns>
    ///[DebuggerStepThrough]
    public static string PlaceholderReplace(this string input, string placeholder, string replacement)
    {
      if (!string.IsNullOrEmpty(replacement))
      {
        var type = input.GetPlaceholderType(placeholder);
        if (type != null)
        {
          if (input.IndexOf(" - " + type, StringComparison.OrdinalIgnoreCase) != -1)
            type = " - " + type;
          else if (input.IndexOf(" " + type, StringComparison.OrdinalIgnoreCase) != -1)
          {
            type = " " + type;
            replacement = " " + replacement;
          }
          else if (input.IndexOf(type + " ", StringComparison.OrdinalIgnoreCase) != -1)
            replacement += " ";

          return input.ReplaceCaseInsensitive(type, replacement);
        }
      }
      return input;
    }

    /// <summary>
    ///   Remove all mapping that do not have a source
    /// </summary>
    /// <param name="columns">List of columns</param>
    /// <param name="fileSetting">The setting.</param>
    public static IEnumerable<string> RemoveMappingWithoutSource(this IFileSetting fileSetting,
      ICollection<string> columns)
    {
      Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
      var notFoundColumnNames = new List<string>();

      if (fileSetting == null || columns == null || columns.Count == 0)
        return notFoundColumnNames;
      foreach (var map in fileSetting.MappingCollection)
      {
        var foundColumn = false;
        foreach (var col in columns)
        {
          if (!col.Equals(map.FileColumn, StringComparison.OrdinalIgnoreCase))
            continue;
          foundColumn = true;
          break;
        }

        if (!foundColumn)
          notFoundColumnNames.Add(map.FileColumn);
      }

      if (notFoundColumnNames.Count <= 0)
        return notFoundColumnNames;

      foreach (var notFound in notFoundColumnNames)
      {
        Logger.Warning(
          "Column {columnname} was expected but was not found in {filesetting}. The invalid column mapping will be removed.",
          fileSetting, notFound);
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

      var exhange1 = !string.IsNullOrEmpty(old1) && string.Compare(old1, new1, StringComparison.Ordinal) != 0;
      var exhange2 = !string.IsNullOrEmpty(old2) && string.Compare(old2, new2, StringComparison.Ordinal) != 0;
      if (exhange1 && exhange2 && new1 == old2)
      {
        inputValue = inputValue.Replace(old1, "{\0}");
        inputValue = inputValue.Replace(old2, new2);
        inputValue = inputValue.Replace("{\0}", new1);
      }
      else
      {
        if (exhange1)
          inputValue = inputValue.Replace(old1, new1);
        if (exhange2)
          inputValue = inputValue.Replace(old2, new2);
      }

      return inputValue;
    }

    /// <summary>
    ///   Replace placeholder in a template with value of property
    /// </summary>
    /// <param name="template">The template with placeholder in {}, e.G. ID:{ID} </param>
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
      var props = new List<PropertyInfo>();
      foreach (var prop in obj.GetType().GetProperties())
        if (prop.GetMethod != null)
          props.Add(prop);

      foreach (Match match in rgx.Matches(template))
        if (!placeholder.ContainsKey(match.Value))
        {
          PropertyInfo prop = null;
          foreach (var x in props)
            if (x.Name.Equals(match.Value.Substring(1, match.Value.Length - 2), StringComparison.OrdinalIgnoreCase))
            {
              prop = x;
              break;
            }

          if (prop != null)
            placeholder.Add(match.Value, prop.GetValue(obj).ToString());
        }

      // replace them  with the property value from setting
      foreach (var pro in placeholder)
        template = template.ReplaceCaseInsensitive(pro.Key, pro.Value);

      return template.Replace("  ", " ");
    }

    /// <summary>
    ///   Replace placeholder in a template with the text provide in the parameters the order of the placeholders is important
    ///   not their contend
    /// </summary>
    /// <param name="template">The template with placeholder in {}, e.G. ID:{ID} </param>
    /// <param name="values">a variable number of text that will replace the placeholder in order of appearance</param>
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

      // replace them  with the property value from setting
      foreach (var pro in placeholder)
        template = template.ReplaceCaseInsensitive(pro.Key, pro.Value);

      return template.Replace("  ", " ");
    }

    public static string CsvToolsStackTrace(this Exception exception)
    {
      var start = 0;
      if (string.IsNullOrEmpty(exception.StackTrace))
        return null;
      var indexof = exception.StackTrace.IndexOf("at ", StringComparison.Ordinal);
      while (indexof != -1)
      {
        if (start >= 0 && exception.StackTrace.Length > start)
        {
          var part = exception.StackTrace.Substring(start, indexof - start).Trim();
          if (!string.IsNullOrEmpty(part) && part.StartsWith("CsvTools."))
          {
            var msg = part.Substring("CsvTools.".Length);
            // In case it was an extension method, use stack trace
            if (msg.StartsWith("ClassLibraryCsvExtensionMethods"))
            {
              var st = new StackTrace();
              var lastWaitToCompleteTask = -1;
              for (var index = 0; index < st.FrameCount; index++)
              {
                var trace = st.GetFrame(index);
                if (trace.GetMethod().ToString().Contains("WaitToCompleteTask"))
                  lastWaitToCompleteTask = index;
                else
                {
                  if (lastWaitToCompleteTask > 0)
                  {
                    msg = trace.ToString();
                    var tracePosIn = msg.IndexOf(" in ", StringComparison.Ordinal);
                    if (tracePosIn == -1)
                      return msg;
                    else
                      return msg.Substring(0, tracePosIn);
                  }
                }
              }
            }
            var posIn = msg.IndexOf(" in ", StringComparison.Ordinal);
            var posLine = msg.LastIndexOf(":", StringComparison.Ordinal);
            if (posIn != -1 && posLine != -1)
              return msg.Substring(0, posIn) + " " + msg.Substring(posLine + 1);
            else
              return msg;
          }
        }

        start = indexof + 3;
        indexof = exception.StackTrace.IndexOf("at ", start, StringComparison.Ordinal);
      }
      return null;
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
      if (value < int.MinValue)
        return int.MinValue;
      return Convert.ToInt32(value);
    }

    /// <summary>
    ///   Run a task to completion with timeout
    ///   You should you expose synchronous wrappers for asynchronous methods, still here it is be very careful
    /// </summary>
    /// <param name="executeTask">The started <see cref="System.Threading.Tasks.Task" /></param>
    /// <param name="timeoutSeconds">
    ///   Timeout for the completion of the task, if more time is spent running / waiting the wait
    ///   is finished
    /// </param>
    /// <param name="every250Ms">Action to be invoked every 1/4 second while waiting to finish, usually used for UI updates</param>
    /// <param name="cancellationToken">
    ///   Best is to start tasks with the cancellation token but some async methods do not do, so
    ///   it can be provided
    /// </param>
    /// <remarks>Will only return the first exception in case of aggregate exceptions.</remarks>
    public static void WaitToCompleteTask(this Task executeTask, double timeoutSeconds, Action every250Ms = null,
      CancellationToken cancellationToken = default(CancellationToken))
    {
      if (executeTask == null)
        throw new ArgumentNullException(nameof(executeTask));

      if (executeTask.IsCompleted)
        return;
      cancellationToken.ThrowIfCancellationRequested();

      var stopwatch = (timeoutSeconds > 0.01) ? new Stopwatch() : null;
      stopwatch?.Start();

      try
      {
        // IsCompleted := RanToCompletion || Canceled || Faulted
        while (!executeTask.IsCompleted)
        {
          cancellationToken.ThrowIfCancellationRequested();
          if (stopwatch != null)
          {
            // Raise an exception when not activated
            //if (executeTask.Status == TaskStatus.WaitingForActivation && stopwatch.Elapsed.TotalSeconds > timeoutSeconds)
            //  throw new TimeoutException($"Waited longer than {stopwatch.Elapsed.TotalSeconds:N1} seconds for task activation");

            // Raise an exception when waiting too long
            if (timeoutSeconds > 0 && stopwatch.Elapsed.TotalSeconds > timeoutSeconds)
              throw new TimeoutException($"Timeout after {stopwatch.Elapsed.TotalSeconds:N1} seconds");
          }

          // Invoke action every 1/4 second
          every250Ms?.Invoke();

          // wait will raise an AggregateException if the task throws an exception
          executeTask.Wait(250, cancellationToken);
        }

        if (executeTask.IsFaulted && executeTask.Exception != null)
          throw executeTask.Exception;
      }
      catch (Exception ex)
      {
        var st = ex.CsvToolsStackTrace();
        if (st == null)
          Logger.Warning(ex, "{exception}", ex.ExceptionMessages());
        else
          Logger.Warning(ex, "{stacktrace}: {exception}", ex.CsvToolsStackTrace(), ex.ExceptionMessages());
        // return only the first exception if there are many
        if (ex is AggregateException ae)
          throw ae.Flatten().InnerExceptions[0];
        throw;
      }
    }

    /// <summary>
    ///   Run a task to completion with timeout
    ///   You should you expose synchronous wrappers for asynchronous methods, still here it is be very careful
    /// </summary>
    /// <param name="executeTask">The started <see cref="System.Threading.Tasks.Task" /> with a return value</param>
    /// <param name="timeoutSeconds">
    ///   Timeout for the completion of the task, if more time is spent running / waiting the wait
    ///   is finished
    /// </param>
    /// <param name="every250Ms">Action to be invoked every 1/4 second while waiting to finish, usually used for UI updates</param>
    /// <param name="cancellationToken">
    ///   Best is to start tasks with the cancellation token but some async methods do not do, so
    ///   it can be provided
    /// </param>
    /// <returns>Task Result if finished successfully, otherwise raises an error</returns>
    public static T WaitToCompleteTaskResult<T>(this Task<T> executeTask, double timeoutSeconds, Action every250Ms = null, CancellationToken cancellationToken = default(CancellationToken))
    {
      WaitToCompleteTask(executeTask, timeoutSeconds, every250Ms, cancellationToken);
      return executeTask.Result;
    }

    private static bool HasColumnName(this IFileReader reader, string columnName)
    {
      Contract.Requires(reader != null, "reader");
      Contract.Requires(!string.IsNullOrEmpty(columnName));

      for (var col = 0; col < reader.FieldCount; col++)
      {
        var column = reader.GetColumn(col);
        if (column.Ignore)
          continue;
        if (columnName.Equals(column.Name, StringComparison.OrdinalIgnoreCase))
          return true;
      }

      return false;
    }

    public static CopyToDataTableInfo RemapCopyTo(this IFileReader fileReader, IFileSetting fileSetting,
      DataTable dataTable, bool includeErrorField)
    {
      if (fileReader == null)
        throw new ArgumentNullException(nameof(fileReader));
      if (fileSetting == null)
        throw new ArgumentNullException(nameof(fileSetting));
      if (dataTable == null)
        throw new ArgumentNullException(nameof(dataTable));

      var result = new CopyToDataTableInfo
      {
        Mapping = new BiDirectionalDictionary<int, int>(),
        ReaderColumns = new List<string>()
      };

      // Initialize a based on file reader
      for (var col = 0; col < fileReader.FieldCount; col++)
      {
        result.ReaderColumns.Add(fileReader.GetName(col));
        if (fileReader.IgnoreRead(col))
          continue;
        var cf = fileReader.GetColumn(col);
        if (cf.Name.Equals(BaseFileReader.cStartLineNumberFieldName, StringComparison.OrdinalIgnoreCase))
          continue;
        result.Mapping.Add(col, dataTable.Columns[cf.Name].Ordinal);
      }

      result.StartLine = dataTable.Columns[BaseFileReader.cStartLineNumberFieldName];
      if (fileSetting.DisplayRecordNo && !fileReader.HasColumnName(BaseFileReader.cRecordNumberFieldName))
        result.RecordNumber = dataTable.Columns[BaseFileReader.cRecordNumberFieldName];

      if (fileSetting.DisplayEndLineNo && !fileReader.HasColumnName(BaseFileReader.cEndLineNumberFieldName))
        result.EndLine = dataTable.Columns[BaseFileReader.cEndLineNumberFieldName];

      if (includeErrorField && !fileReader.HasColumnName(BaseFileReader.cErrorField))
        result.Error = dataTable.Columns[BaseFileReader.cErrorField];

      return result;
    }
    public static CopyToDataTableInfo GetCopyToDataTableInfo(this IFileReader fileReader, IFileSetting fileSetting,
      DataTable dataTable, bool includeErrorField)
    {
      if (fileReader == null)
        throw new ArgumentNullException(nameof(fileReader));
      if (fileSetting == null)
        throw new ArgumentNullException(nameof(fileSetting));
      if (dataTable == null)
        throw new ArgumentNullException(nameof(dataTable));

      dataTable.TableName = fileSetting.ID;
      dataTable.Locale = CultureInfo.InvariantCulture;

      var result = new CopyToDataTableInfo
      {
        Mapping = new BiDirectionalDictionary<int, int>(),
        ReaderColumns = new List<string>()
      };

      // Initialize a based on file reader
      for (var col = 0; col < fileReader.FieldCount; col++)
      {
        result.ReaderColumns.Add(fileReader.GetName(col));
        if (fileReader.IgnoreRead(col))
          continue;
        var cf = fileReader.GetColumn(col);
        // a reader column BaseFileReader.cStartLineNumberFieldName will be ignored
        if (cf.Name.Equals(BaseFileReader.cStartLineNumberFieldName, StringComparison.OrdinalIgnoreCase))
          continue;
        result.Mapping.Add(col, dataTable.Columns.Count);
        // Special handling for #Line, this has to be a int64, not based on system
        dataTable.Columns.Add(new DataColumn(cf.Name, cf.DataType.GetNetType()));
      }

      // Append Artificial columns
      // This needs to happen in the same order as we have in CreateTableFromReader otherwise BulkCopy does not work
      // see  SqlServerConnector.CreateTable
      result.StartLine = new DataColumn(BaseFileReader.cStartLineNumberFieldName, typeof(long));
      dataTable.Columns.Add(result.StartLine);

      dataTable.PrimaryKey = new[] { result.StartLine };

      if (fileSetting.DisplayRecordNo && !fileReader.HasColumnName(BaseFileReader.cRecordNumberFieldName))
      {
        result.RecordNumber = new DataColumn(BaseFileReader.cRecordNumberFieldName, typeof(long));
        dataTable.Columns.Add(result.RecordNumber);
      }

      if (fileSetting.DisplayEndLineNo && !fileReader.HasColumnName(BaseFileReader.cEndLineNumberFieldName))
      {
        result.EndLine = new DataColumn(BaseFileReader.cEndLineNumberFieldName, typeof(long));
        dataTable.Columns.Add(result.EndLine);
      }

      if (includeErrorField && !fileReader.HasColumnName(BaseFileReader.cErrorField))
      {
        result.Error = new DataColumn(BaseFileReader.cErrorField, typeof(string));
        dataTable.Columns.Add(result.Error);
      }

      return result;
    }

    /// <summary>
    ///   Writes the data to a data table.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="records">The number of records.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///   A <see cref="DataTable" /> with the records
    /// </returns>
    public static DataTable WriteToDataTable(this IFileReader reader, IFileSetting fileSetting, long records,
      CancellationToken cancellationToken)
    {
      var requestedRecords = records < 1 ? long.MaxValue : records;


      var dataTable = new DataTable(fileSetting.ID)
      {
        MinimumCapacity = (int)Math.Min(requestedRecords, 5000)
      };

      try
      {
        var columnErrorDictionary = new ColumnErrorDictionary(reader);
        var copyToDataTableInfo = reader.GetCopyToDataTableInfo(fileSetting, dataTable, false);

        while (!cancellationToken.IsCancellationRequested && requestedRecords > 0 && reader.Read())
        {
          reader.CopyRowToTable(dataTable, columnErrorDictionary, copyToDataTableInfo,
            (columnError, row) =>
            {
              foreach (var keyValuePair in columnError)
                if (keyValuePair.Key == -1)
                  row.RowError = keyValuePair.Value;
                else if (copyToDataTableInfo.Mapping.TryGetValue(keyValuePair.Key, out var dbCol))
                  row.SetColumnError(dbCol, keyValuePair.Value);
            });
          requestedRecords--;
        }
      }
      catch
      {
        dataTable.Dispose();
        throw;
      }

      return dataTable;
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
          inputString.Equals("Carriage return", StringComparison.OrdinalIgnoreCase))
        return '\r';

      if (inputString.Equals("Check mark", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Check", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("✓", StringComparison.OrdinalIgnoreCase))
        return '✓';

      if (inputString.Equals("Feed", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("LineFeed", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("LF", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Line feed", StringComparison.OrdinalIgnoreCase))
        return '\n';

      if (inputString.StartsWith("Unit separator", StringComparison.OrdinalIgnoreCase) ||
          inputString.Contains("31") ||
          inputString.Equals("US", StringComparison.OrdinalIgnoreCase))
        return '\u001F';

      if (inputString.StartsWith("Record separator", StringComparison.OrdinalIgnoreCase) ||
          inputString.Contains("30") ||
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
    ///   Check if a collection is equal, the items can be in any order as long as all exist in the th other
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
      var equal = true;
      // Check the item, all should be the same, order does not matter though
      foreach (var ot in other)
      {
        var found = false;
        if (ot == null)
          foreach (var x in self)
          {
            if (x != null)
              continue;
            found = true;
            break;
          }
        else
          foreach (var th in self)
          {
            if (!ot.Equals(th))
              continue;
            found = true;
            break;
          }

        if (found)
          continue;
        equal = false;
        break;
      }

      // No need to check the other way around, as all items are the same and the number matches
      return equal;
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
        var hashCode = 731;
        var order = 0;
        foreach (var item in collection)
          hashCode = (hashCode * 397) ^ (item.GetHashCode() + order++);
        return hashCode;
      }
    }

#endif
  }
}