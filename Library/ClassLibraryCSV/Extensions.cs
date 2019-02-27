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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace CsvTools
{
  /// <summary>
  ///   Class with extensions used in the class Library
  /// </summary>
  public static class ClassLibraryCsvExtensionMethods
  {
    private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
    public static bool AssumeGZip(this string fileName)
    {
      return fileName.EndsWith(".gz", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///   Check if the application should assume its PGP.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public static bool AssumePgp(this string fileName)
    {
      return fileName.EndsWith(".pgp", StringComparison.OrdinalIgnoreCase) ||
             fileName.EndsWith(".gpg", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///   Copies all elements from one collection to the other
    /// </summary>
    /// <typeparam name="T">the type</typeparam>
    /// <param name="self">The collection.</param>
    /// <param name="other">The other collection.</param>
    public static void CollectionCopy<T>(this ICollection<T> self, ICollection<T> other) where T : ICloneable<T>
    {
      Contract.Requires(self != null);
      Contract.Requires(other != null);

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
    public static void CollectionCopyStruct<T>(this ICollection<T> self, ICollection<T> other) where T : struct
    {
      Contract.Requires(self != null);
      Contract.Requires(other != null);

      other.Clear();
      foreach (var item in self)
        other.Add(item);
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
      Contract.Requires(self != null);
      if (ReferenceEquals(other, self))
        return true;
      if (other?.Count != self.Count)
        return false;
      var equal = true;
      // Check the item, all should be the same, order does not matter though
      foreach (var ot in other)
      {
        var found = false;
        if (ot == null)
          found = self.Any(x => x == null);
        else
          foreach (var th in self)
          {
            if (!ot.Equals(th)) continue;
            found = true;
            break;
          }

        if (found) continue;
        equal = false;
        break;
      }

      // No need to check the other way around, as all items are the same and the number matches
      return equal;
    }

    /// <summary>
    ///   Check if two lists are equal, the items need to be in the right order
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self">The collection.</param>
    /// <param name="other">The other collection.</param>
    /// <returns></returns>
    public static bool CollectionEqualWithOrder<T>(this IList<T> self, IList<T> other)
    {
      Contract.Requires(self != null);
      if (ReferenceEquals(other, self))
        return true;
      if (other == null || other.Count != self.Count)
        return false;

      // Check the item, all should be the same, order does not matter though
      for (var pos = 0; pos < self.Count; pos++)
        if (!self[pos].Equals(other[pos]))
          return false;

      return true;
    }

    /// <summary>
    /// Get the hash code of a collection, the order of items should not matter.
    /// </summary>
    /// <param name="collection">The collection itself.</param>
    /// <returns></returns>
    public static int CollectionHashCode(this ICollection collection)
    {
      unchecked
      {
        int hashCode = 387;
        foreach (var item in collection)
          hashCode += item.GetHashCode();
        return hashCode;
      }
    }

#endif

    /// <summary>
    ///   Writes the current record into a data table.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="dataTable">The data table.</param>
    /// <param name="warningsList">The warnings list.</param>
    /// <param name="columnMapping">The column mapping.</param>
    /// <param name="recordNumberColumn">The record number column.</param>
    /// <param name="endLineNumberColumn">The end line number column.</param>
    /// <param name="startLineNumberColumn">The start line number column.</param>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    public static void CopyRowToTable(this IFileReader reader, DataTable dataTable, RowErrorCollection warningsList,
      int[] columnMapping,
      DataColumn recordNumberColumn, DataColumn endLineNumberColumn, DataColumn startLineNumberColumn)
    {
      Contract.Requires(dataTable != null);
      Contract.Requires(columnMapping != null);
      var columns = columnMapping.Length;
      var dataRow = dataTable.NewRow();
      for (var col = 0; col < columns; col++)
        try
        {
          dataRow[col] = reader.GetValue(columnMapping[col]);
        }
        catch (Exception exc)
        {
          // Any error here is caused by storing a value that does not fit into the field
          warningsList?.Add(reader,
            new WarningEventArgs(reader.RecordNumber, col, exc.ExceptionMessages(), 0, 0, null));
        }

      if (recordNumberColumn != null)
        dataRow[recordNumberColumn] = reader.RecordNumber;

      if (endLineNumberColumn != null)
        dataRow[endLineNumberColumn] = reader.EndLineNumber;

      if (startLineNumberColumn != null)
        dataRow[startLineNumberColumn] = reader.StartLineNumber;

      if (warningsList != null && warningsList.CountRows > 0)
        if (warningsList.TryGetValue(reader.RecordNumber, out var warningsforRow))
          for (var col = -1; col < columns; col++)
          {
            var error = warningsforRow[col];
            if (string.IsNullOrEmpty(error)) continue;
            if (col == -1)
              dataRow.RowError = error;
            else
              dataRow.SetColumnError(col, error);
          }

      dataTable.Rows.Add(dataRow);
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
    ///   Counts the items in the enumeration
    /// </summary>
    /// <param name="items">The items.</param>
    /// <returns></returns>
    public static int Count(this IEnumerable items)
    {
      if (items == null)
        return 0;

      if (items is ICollection col)
        return col.Count;

      var counter = 0;
      foreach (var unused in items)
        counter++;
      return counter;
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
    public static string ExceptionMessages(this Exception exception, int maxDepth = 3)
    {
      Contract.Requires(exception != null);
      Contract.Ensures(Contract.Result<string>() != null);
      var mainMessage = exception.Message;
      var innerMessage = exception.InnerExceptionMessages(maxDepth - 1);
      if (string.IsNullOrEmpty(innerMessage))
        return mainMessage;
      return $"{mainMessage}\n{innerMessage}";
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
           x == UnicodeCategory.UppercaseLetter || x == UnicodeCategory.LowercaseLetter || x == UnicodeCategory.OtherLetter ||
           x == UnicodeCategory.ConnectorPunctuation || x == UnicodeCategory.DashPunctuation || x == UnicodeCategory.OtherPunctuation ||
           x == UnicodeCategory.DecimalDigitNumber);

      const string T = @"(:|-|_)";
      const string HH = @"(2[0-3]|((0|1)?\d))";    // 0-9 10-19 20-23
      const string MS = @"([0-5][0-9])";           // 00-59
      const string TT = @"((_| )?(AM|PM))?";
      const string S = @"(\/|.|-|_)?";
      const string YYYY = @"((19\d{2})|(2\d{3}))"; // 1900 - 2999
      const string MM = @"([0,1]?\d{1})";
      const string DD = @"(([0-2]?\d{1})|([3][0,1]{1}))";
      // Replace Times 3_53_34_AM
      fileName = Regex.Replace(fileName, T + "?" + HH + T + MS + T + "?" + MS + "?" + TT, string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

      // Replace Dates YYYYMMDDHHMMSS
      fileName = Regex.Replace(fileName, S + YYYY + S + MM + S + DD + T + "?" + HH + T + MS + T + "?" + MS + "?" + TT, string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

      // Replace Dates DDMMYYYY
      fileName = Regex.Replace(fileName, S + YYYY + S + MM + S + DD, string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
      // Replace Dates MMDDYYYY
      fileName = Regex.Replace(fileName, S + MM + S + DD + S + YYYY, string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
      // Replace Dates DDMMYYYY
      fileName = Regex.Replace(fileName, S + DD + S + MM + S + YYYY, string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

      return fileName.Trim('_', '-', ' ', '\t').Replace("__", "_").Replace("__", "_").Replace("--", "-").Replace("--", "-");
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

    /// <summary>
    ///   Get a list of column names that are not artificial
    /// </summary>
    /// <param name="dataTable">The <see cref="DataTable" /> containing the columns</param>
    /// <returns>A enumeration of ColumnNames</returns>
    public static IEnumerable<string> GetRealColumns(this DataTable dataTable)
    {
      Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
      return GetRealDataColumns(dataTable).Select(x => x.ColumnName);
    }

    /// <summary>
    ///   Get a list of columns that are not artificial
    /// </summary>
    /// <param name="dataTable">The <see cref="DataTable" /> containing the columns</param>
    /// <returns>A enumeration of <see cref="DataColumn" /></returns>
    public static IEnumerable<DataColumn> GetRealDataColumns(this DataTable dataTable)
    {
      Contract.Ensures(Contract.Result<IEnumerable<DataColumn>>() != null);

      if (dataTable == null) yield break;
      foreach (DataColumn col in dataTable.Columns)
        if (!BaseFileReader.ArtificalFields.Contains(col.ColumnName))
          yield return col;
    }

    /// <summary>
    ///   A function to check and address that used tables might not be current and need to be read again
    /// </summary>
    /// <param name="parentFileSetting">The file setting.</param>
    /// <param name="check">The check.</param>
    /// <param name="cancellationToken">The cancellation token source.</param>
    /// <param name="level">The level.</param>
    /// <returns>
    ///   A set of IFileSetting that should be checked, this is a depth first recursion
    /// </returns>
    public static ICollection<IFileSetting> GetSourceFileSettings(this IFileSetting parentFileSetting,
      Func<IFileSetting, bool> check, CancellationToken cancellationToken, int level = 0)
    {
      Contract.Requires(parentFileSetting != null);
      Contract.Ensures(Contract.Result<ICollection<IFileSetting>>() != null);
      if ((parentFileSetting.SourceFileSettings != null) && (level == 0))
        return parentFileSetting.SourceFileSettings;

      if ((parentFileSetting.SourceFileSettings == null))
      {
        parentFileSetting.SourceFileSettings = new List<IFileSetting>();
      }

      // Prevent infinite recursion in case we have a cycle
      if (level >= 5) return parentFileSetting.SourceFileSettings;
      if (string.IsNullOrWhiteSpace(parentFileSetting.SqlStatement)) return parentFileSetting.SourceFileSettings;
      var tables = parentFileSetting.SqlStatement.GetSQLTableNames();
      foreach (var tbl in tables)
      {
        if (cancellationToken.IsCancellationRequested)
          break;
        // get the Setting to match the table
        foreach (var setting in ApplicationSetting.ToolSetting.Input.Where(x =>
          tbl.Equals(x.ID, StringComparison.OrdinalIgnoreCase) && !Equals(x, parentFileSetting)))
        {
          foreach (var src in GetSourceFileSettings(setting, check, cancellationToken, level + 1))
            if (parentFileSetting.SourceFileSettings.Contains(src))
              parentFileSetting.SourceFileSettings.Add(src);

          if (check(setting))
            parentFileSetting.SourceFileSettings.Add(setting);
        }
      }

      return parentFileSetting.SourceFileSettings;
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
    ///   Determines whether this enumeration is empty.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <returns></returns>
    public static bool IsEmpty(this IEnumerable items)
    {
      if (items == null) return true;
      foreach (var unused in items)
        return false;
      return true;
    }

    /// <summary>
    /// Replaces a placeholders with a text. The placeholder are identified surrounding { or a leading #
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="placeholder">The placeholder.</param>
    /// <param name="replacement">The replacement.</param>
    /// <returns>The new text based on input</returns>
    public static string PlaceholderReplace(this string input, string placeholder, string replacement)
    {
      var type = "{" + placeholder + "}";
      if (input.IndexOf(type, StringComparison.OrdinalIgnoreCase) != -1)
      {
        // remove leading delimiters " - " along with the empty text
        if (string.IsNullOrEmpty(replacement))
        {
          if (input.IndexOf(" - " + type, StringComparison.OrdinalIgnoreCase) != -1)
            type = " - " + type;
          else if (input.IndexOf(" " + type, StringComparison.OrdinalIgnoreCase) != -1)
            type = " " + type;
        }
        input = input.ReplaceCaseInsensitive(type, replacement);
      }

      if (input.EndsWith("#" + placeholder, StringComparison.OrdinalIgnoreCase))
        input = input.Substring(0, input.Length - placeholder.Length - 1) + replacement;

      return input.ReplaceCaseInsensitive("#" + placeholder + " ", replacement + " ");
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

      if (fileSetting == null || columns.IsEmpty()) return notFoundColumnNames;
      foreach (var map in fileSetting.Mapping)
      {
        var foundColumn = false;
        foreach (var col in columns)
        {
          if (!col.Equals(map.FileColumn, StringComparison.OrdinalIgnoreCase)) continue;
          foundColumn = true;
          break;
        }

        if (!foundColumn)
          notFoundColumnNames.Add(map.FileColumn);
      }

      if (notFoundColumnNames.Count <= 0) return notFoundColumnNames;
      foreach (var notFound in notFoundColumnNames) fileSetting.RemoveMapping(notFound);
      return notFoundColumnNames;
    }

    /// <summary>
    ///   String replace function that is Case Insensitive
    /// </summary>
    /// <param name="original">The source</param>
    /// <param name="pattern">The text to replace</param>
    /// <param name="replacement">the character to which it should be changed</param>
    /// <returns>The source text with the replacement</returns>
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
        for (var i = position0; i < position1; ++i) chars[count++] = original[i];
        chars[count++] = replacement;
        position0 = position1 + pattern.Length;
      }

      if (position0 == 0) return original;
      for (var i = position0; i < original.Length; ++i) chars[count++] = original[i];
      return new string(chars, 0, count);
    }

    /// <summary>
    ///   String replace function that is Case Insensitive
    /// </summary>
    /// <param name="original">The source</param>
    /// <param name="pattern">The text to replace</param>
    /// <param name="replacement">the text to which it should be changed</param>
    /// <returns>The source text with the replacement</returns>
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
        for (var i = 0; i < replacement.Length; ++i)
          chars[count++] = replacement[i];
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
    ///   Check if the Source settings are all read before the write was done
    /// </summary>
    /// <param name="fileSettingWrite"></param>
    /// <param name="token"></param>
    /// <returns><c>true</c> if the file written is older than all sources</returns>
    public static bool SettingLaterThanSources(this IFileSetting fileSettingWrite, CancellationToken token)
    {
      if (fileSettingWrite.FileLastWriteTimeUtc.Ticks < 10)
        return false;
      return fileSettingWrite.GetSourceFileSettings(setting => setting.FileLastWriteTimeUtc < fileSettingWrite.FileLastWriteTimeUtc,
        token).Count > 0;
    }

    /// <summary>
    ///   Sets the validation result.
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="numberRecords">The number records.</param>
    /// <param name="errorCount">The error count.</param>
    /// <param name="warningCount">The warning count.</param>
    public static void SetValidationResult(this IFileSetting fileSetting, long numberRecords, long errorCount,
      long warningCount)
    {      
      var ret = new ValidationResult
      {
        TableName = fileSetting.ID,
        NumberRecords = numberRecords,
        ErrorCount = errorCount,
        WarningCount = warningCount
      };

      // Do not remove validation result information if one is present and the new one is empty
      if (fileSetting.ValidationResult != null)
      {
        if (numberRecords < 1 && fileSetting.ValidationResult.NumberRecords > 0)
        {
          ret.NumberRecords = fileSetting.ValidationResult.NumberRecords;
        }

        if (errorCount < 0 && fileSetting.ValidationResult.ErrorCount >= 0)
        {
          ret.ErrorCount = fileSetting.ValidationResult.ErrorCount;
        }

        if (warningCount < 0 && fileSetting.ValidationResult.WarningCount >= 0)
        {
          ret.WarningCount = fileSetting.ValidationResult.WarningCount;
        }
      }

      // TODO: Improve this, to check if we have a physical file, IFileSettingNoFile is defined in other assembly.
      if (fileSetting is IFileSettingRemoteDownload && FileSystemUtils.FileExists(fileSetting.FullPath))
        ret.FileSize = FileSystemUtils.FileInfo(fileSetting.FullPath).Length;

      fileSetting.ValidationResult = ret;

      // ToolSetting is guaranteed to be set but we might not have a Cache
      if (ApplicationSetting.ToolSetting.ValidationResultCache != null)
        ApplicationSetting.ToolSetting.ValidationResultCache.Set(fileSetting.ID, ret);
    }

    /// <summary>
    ///   Combines all inner exceptions to one formatted string for logging.
    /// </summary>
    /// <param name="exception">The exception of type <see cref="Exception" /></param>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    public static string SourceExceptionMessage(this Exception exception)
    {
      Contract.Ensures(Contract.Result<string>() != null);
      if (exception.InnerException == null)
        return exception.Message;

      return exception.InnerException.SourceExceptionMessage();
    }

    /// <summary>
    ///   Get a valid unsigned integer from the integer
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static uint ToUint(this int value)
    {
      if (value < 0)
        return 0;

      return (uint)value;
    }

    /// <summary>
    ///   Writes the data to a data table.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="records">The number of records.</param>
    /// <param name="warningsList">A Warning list to be used here</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///   A <see cref="DataTable" /> with the records
    /// </returns>
    public static DataTable WriteToDataTable(this IFileReader reader, IFileSetting fileSetting, uint records,
      RowErrorCollection warningsList, CancellationToken cancellationToken)
    {
      var requestedRecords = records < 1 ? uint.MaxValue : records;
      Log.Info("Reading data…");
      var dataTable = new DataTable
      {
        TableName = "DataTable",
        Locale = CultureInfo.InvariantCulture,
        MinimumCapacity = (int)Math.Min(requestedRecords, 5000)
      };

      try
      {
        var hasRecordNo = false;
        var hasLineNoEnd = false;
        var hasLineNoStart = false;

        var columnList = new List<int>();
        // Create the columns from the FieldHeaders
        for (var column = 0; column < reader.FieldCount; column++)
        {
          if (reader.IgnoreRead(column)) continue;
          var readerCol = reader.GetColumn(column);
          Contract.Assume(readerCol != null);
          Contract.Assume(!string.IsNullOrEmpty(readerCol.Name));
          var dataCol = dataTable.Columns.Add(readerCol.Name, readerCol.DataType.GetNetType());
          columnList.Add(column);
          dataCol.AllowDBNull = true;
          hasRecordNo |=
            readerCol.Name.Equals(BaseFileReader.cRecordNumberFieldName, StringComparison.OrdinalIgnoreCase);
          hasLineNoEnd |=
            readerCol.Name.Equals(BaseFileReader.cEndLineNumberFieldName, StringComparison.OrdinalIgnoreCase);
          hasLineNoStart |= readerCol.Name.Equals(BaseFileReader.cStartLineNumberFieldName,
            StringComparison.OrdinalIgnoreCase);
        }

        var columnMapping = columnList.ToArray();

        if (fileSetting.DisplayRecordNo && !hasRecordNo)
        {
          var col = dataTable.Columns.Add(BaseFileReader.cRecordNumberFieldName, typeof(int));
          col.AllowDBNull = true;
        }

        if (fileSetting.DisplayStartLineNo && !hasLineNoStart)
        {
          var col = dataTable.Columns.Add(BaseFileReader.cStartLineNumberFieldName, typeof(int));
          col.AllowDBNull = true;
        }

        if (fileSetting.DisplayEndLineNo && !hasLineNoEnd)
        {
          var col = dataTable.Columns.Add(BaseFileReader.cEndLineNumberFieldName, typeof(int));
          col.AllowDBNull = true;
        }

        DataColumn recordNumberColumn = null;
        if (fileSetting.DisplayRecordNo)
          recordNumberColumn = dataTable.Columns[BaseFileReader.cRecordNumberFieldName];

        DataColumn lineNumberColumnEnd = null;
        if (fileSetting.DisplayEndLineNo)
          lineNumberColumnEnd = dataTable.Columns[BaseFileReader.cEndLineNumberFieldName];

        DataColumn lineNumberColumnStart = null;
        if (fileSetting.DisplayStartLineNo)
          lineNumberColumnStart = dataTable.Columns[BaseFileReader.cStartLineNumberFieldName];

        while (!cancellationToken.IsCancellationRequested && requestedRecords > 0 && reader.Read())
        {
          reader.CopyRowToTable(dataTable, warningsList, columnMapping, recordNumberColumn, lineNumberColumnEnd,
            lineNumberColumnStart);
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
  }
}