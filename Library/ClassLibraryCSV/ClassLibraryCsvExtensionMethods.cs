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
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
#if !QUICK
    public static async Task<long> WriteAsync(this IFileWriter writer, string sqlStatement,
                                              int timeout, Action<string>? reportProgress, CancellationToken cancellationToken)
    {
      if (string.IsNullOrEmpty(sqlStatement))
        return 0;

      using var sqlReader = await FunctionalDI
        .SQLDataReader(sqlStatement, (sender, s) => reportProgress?.Invoke(s.Text), timeout, cancellationToken)
        .ConfigureAwait(false);
      await sqlReader.OpenAsync(cancellationToken).ConfigureAwait(false);
      return await writer.WriteAsync(sqlReader, cancellationToken).ConfigureAwait(false);
    }
#endif
    public static string Description(this RecordDelimiterType item)
    {
      var descConv = new EnumDescriptionConverter(typeof(RecordDelimiterType));
      return descConv.ConvertToString(item) ?? string.Empty;
    }

    public static string NewLineString(this RecordDelimiterType type)
    {
      return type switch
      {
        RecordDelimiterType.LF => "\n",
        RecordDelimiterType.CR => "\r",
        RecordDelimiterType.CRLF => "\r\n",
        RecordDelimiterType.LFCR => "\n\r",
        RecordDelimiterType.RS => "▲",
        RecordDelimiterType.US => "▼",
        RecordDelimiterType.None => string.Empty,
        _ => string.Empty
      };
    }

    /// <summary>
    ///   Check if the application should assume its gZIP
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public static bool AssumeGZip(this string fileName) =>
      fileName.EndsWith(".gz", StringComparison.OrdinalIgnoreCase) ||
      fileName.EndsWith(".gzip", StringComparison.OrdinalIgnoreCase);

    public static bool AssumeDeflate(this string fileName) =>
      fileName.EndsWith(".cmp", StringComparison.OrdinalIgnoreCase) ||
      fileName.EndsWith(".dfl", StringComparison.OrdinalIgnoreCase);

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
    public static bool AssumeZip(this string fileName) =>
      fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///   Copies all elements from one collection to the other
    /// </summary>
    /// <typeparam name="T">the type</typeparam>
    /// <param name="self">The collection.</param>
    /// <param name="other">The other collection.</param>
    [DebuggerStepThrough]
    public static void CollectionCopy<T>(this IEnumerable<T> self, ICollection<T>? other)
      where T : ICloneable<T>
    {
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
    public static void CollectionCopyStruct<T>(this IEnumerable<T> self, ICollection<T>? other)
      where T : struct
    {
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
    public static int Count(this IEnumerable? items)
    {
      return items switch
      {
        null => 0,
        ICollection col => col.Count,
        _ => Enumerable.Count(items.Cast<object>())
      };
    }

    /// <summary>
    ///   User Display for a data type
    /// </summary>
    /// <param name="dt">The <see cref="DataType" />.</param>
    /// <returns>A text representing the dataType</returns>
    public static string DataTypeDisplay(this DataType dt)
    {
      return dt switch
      {
        DataType.DateTime => "Date Time",
        DataType.Integer => "Integer",
        DataType.Double => "Floating  Point (High Range)",
        DataType.Numeric => "Money (High Precision)",
        DataType.Boolean => "Boolean",
        DataType.Guid => "Guid",
        DataType.TextPart => "Text Part",
        DataType.TextToHtml => "Encode HTML (Linefeed and CData Tags)",
        DataType.TextToHtmlFull => "Encode HTML ('<' -> '&lt;')",
        DataType.String => "Text",
        _ => throw new ArgumentOutOfRangeException(nameof(dt), dt, "Data Type not known")
      };
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
      if (exception.InnerException == null) return sb.ToString();
      sb.Append("\n");
      sb.Append(exception.InnerExceptionMessages(maxDepth - 1));

      return sb.ToString();
    }

    /// <summary>
    ///   Gets the CsvTools type for a .NET type
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The matching <see cref="DataType" />.</returns>
    public static DataType GetDataType(this Type type)
    {
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

        case TypeCode.Char:
        case TypeCode.String:
        case TypeCode.Object:
        case TypeCode.Empty:
        case TypeCode.DBNull:
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
      var whereRegEx = new Regex("\\sWHERE\\s", RegexOptions.Multiline | RegexOptions.IgnoreCase);
      var matchWhere = whereRegEx.Match(source);
      if (matchWhere.Length>0)
        return whereRegEx.Replace(source, " WHERE 1=0 AND ");
      var orderRegEx = new Regex("\\sORDER\\sBY\\s", RegexOptions.Multiline | RegexOptions.IgnoreCase);
      var matchOrder = orderRegEx.Match(source);
      if (matchOrder.Index<1)
        // Remove Order By and Add a WHERE
        source += " WHERE 1=0";
      else
        source = source.Substring(0, matchOrder.Index) + " WHERE 1=0";

      return source;
    }

    /// <summary>
    ///   Gets a suitable ID for a filename
    /// </summary>
    /// <param name="path">The complete path to a file</param>
    /// <returns>The filename without special characters</returns>
    public static string GetIdFromFileName(this string path)
    {
      var fileName = FileSystemUtils.GetFileName(path).ProcessByCategory(x =>
        x == UnicodeCategory.UppercaseLetter || x == UnicodeCategory.LowercaseLetter ||
        x == UnicodeCategory.OtherLetter ||
        x == UnicodeCategory.ConnectorPunctuation || x == UnicodeCategory.DashPunctuation ||
        x == UnicodeCategory.OtherPunctuation ||
        x == UnicodeCategory.DecimalDigitNumber);

      const string timeSep = @"(:|-|_)?";
      const string dateSep = @"(\/|\.|-|_)?";

      const string hour = @"(2[0-3]|((0|1)\d))"; // 00-09 10-19 20-23
      const string minSec = @"([0-5][0-9])";     // 00-59
      const string amPm = @"((_| )?(AM|PM))?";

      const string year = @"((19\d{2})|(2\d{3}))"; // 1900 - 2999
      const string month = @"(0[1-9]|1[012])";     // 01-12
      const string day = @"(0[1-9]|[12]\d|3[01])"; // 01 - 31

      // Replace Dates YYYYMMDD / MMDDYYYY / DDMMYYYY
      fileName = Regex.Replace(fileName,
        "(" + dateSep + year + dateSep + month + dateSep + day + ")|(" + dateSep + month + dateSep +
        day + dateSep + year + ")|(" + dateSep + day + dateSep + month + dateSep + year + ")",
        string.Empty, RegexOptions.Singleline);

      // Replace Times 3_53_34_AM
      fileName = Regex.Replace(fileName,
        dateSep + hour + timeSep + minSec + timeSep + minSec + "?" + amPm, string.Empty,
        RegexOptions.IgnoreCase | RegexOptions.Singleline);

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

        case DataType.String:
        case DataType.TextToHtml:
        case DataType.TextToHtmlFull:
        case DataType.TextPart:
          return typeof(string);

        default:
          return typeof(string);
      }
    }

    /// <summary>
    ///   Gets the SQl Info messages and logs them
    /// </summary>
    /// <param name="process">The process display.</param>
    /// <returns></returns>
    public static EventHandler<string> GetLogInfoMessage(this IProcessDisplay process) =>
      delegate (object? sender, string message)
      {
        Logger.Information("SQL Information: {message}", message);
        process.SetProcess(message, -1, true);
      };

    /// <summary>
    ///   Get a list of column names that are not artificial
    /// </summary>
    /// <param name="dataTable">The <see cref="DataTable" /> containing the columns</param>
    /// <returns>A enumeration of ColumnNames</returns>
    public static IEnumerable<string> GetRealColumns(this DataTable dataTable) =>
      GetRealDataColumns(dataTable).Select(x => x.ColumnName);

    /// <summary>
    ///   Get a list of columns that are not artificial
    /// </summary>
    /// <param name="dataTable">The <see cref="DataTable" /> containing the columns</param>
    /// <returns>A enumeration of <see cref="DataColumn" /></returns>
    public static IEnumerable<DataColumn> GetRealDataColumns(this DataTable dataTable)
    {
      foreach (DataColumn col in dataTable.Columns)
        if (!ReaderConstants.ArtificialFields.Contains(col.ColumnName))
          yield return col;
    }

    /// <summary>
    ///   Combines all inner exceptions to one formatted string for logging.
    /// </summary>
    /// <param name="exception">The exception of type <see cref="Exception" /></param>
    /// <param name="maxDepth">The maximum depth.</param>
    /// <returns>A string with all inner messages of the error stack</returns>
    [DebuggerStepThrough]
    public static string InnerExceptionMessages(this Exception? exception, int maxDepth = 2)
    {
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

    public static string PlaceholderReplaceFormat(this string input, string placeholder, object? replacement)
    {
      if (replacement == null) return input;
      // in case we have a placeholder with a formatting part e.G. {date:yyyy-MM-dd} we us
      // string.Format to process {0:...

      // General regex without matching the placeholder name is: (?:[{#])([^:\s}#]*)(:[^}]*)?(?:[}#\s])

      // Needs to start with # or { Ends with #, space or } May contain a Format description starting
      // with : ending with }
      var regEx = new Regex(@"(?:[{#])(" + Regex.Escape(placeholder) +  @")(:[^}]*)?(?:[}#\s])", RegexOptions.IgnoreCase | RegexOptions.Singleline);
      if (regEx.IsMatch(input))
      {
        var withFormat = regEx.Replace(input, "{0$2}");
        return string.Format(withFormat, replacement);
      }
      // using string format but replace the placeholder text with 0 as only argument
      else
        return PlaceholderReplace(input, placeholder, Convert.ToString(replacement));
    }

    /// <summary>
    ///   Replaces a placeholders with a text. The placeholder are identified surrounding { or a
    ///   leading #
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="placeholder">The placeholder.</param>
    /// <param name="replacement">The replacement.</param>
    /// <returns>The new text based on input</returns>
    [DebuggerStepThrough]
    public static string PlaceholderReplace(this string input, string placeholder,
                                            string? replacement)
    {
      if (string.IsNullOrEmpty(replacement)) return input;

      var type = "{" + placeholder + "}";
      if (input.IndexOf(type, StringComparison.OrdinalIgnoreCase) == -1)
      {
        type = "#" + placeholder + "#";
        if (input.IndexOf(type, StringComparison.OrdinalIgnoreCase) == -1)
        {
          type = "#" + placeholder;
          if (!input.EndsWith(type, StringComparison.OrdinalIgnoreCase) &&
              input.IndexOf(type + " ", StringComparison.OrdinalIgnoreCase) == -1)
            return input;
        }
      }
      // TODO: Not sure why this is in the code, where was it needed?
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

    /// <summary>
    ///   String replace function that is Case Insensitive
    /// </summary>
    /// <param name="original">The source</param>
    /// <param name="pattern">The text to replace</param>
    /// <param name="replacement">the character to which it should be changed</param>
    /// <returns>The source text with the replacement</returns>
    [DebuggerStepThrough]
    public static string ReplaceCaseInsensitive(this string original, string? pattern,
                                                char replacement)
    {
      var count = 0;
      var position0 = 0;
      int position1;

      if (string.IsNullOrEmpty(pattern))
        return original;

      var upperString = original.ToUpperInvariant();
      var upperPattern = pattern!.ToUpperInvariant();

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
    public static string ReplaceCaseInsensitive(this string original, string? pattern,
                                                string? replacement)
    {
      if (string.IsNullOrEmpty(pattern))
        return original;

      replacement ??= string.Empty;
      pattern ??= string.Empty;

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
    public static string ReplaceDefaults(this string inputValue, string? old1,
                                         string? new1, string? old2, string? new2)
    {
      if (string.IsNullOrEmpty(inputValue))
        return string.Empty;

      var exchange1 = !string.IsNullOrEmpty(old1) && string.Compare(old1, new1, StringComparison.Ordinal) != 0;
      var exchange2 = !string.IsNullOrEmpty(old2) && string.Compare(old2, new2, StringComparison.Ordinal) != 0;
      if (exchange1 && exchange2 && string.Equals(new1, old2))
      {
        inputValue = inputValue.Replace(old1!, "{\0}");
        inputValue = inputValue.Replace(old2!, new2);
        return inputValue.Replace("{\0}", new1);
      }

      if (exchange1)
        inputValue = inputValue.Replace(old1!, new1);
      if (exchange2)
        inputValue = inputValue.Replace(old2!, new2);

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
      {
        if (string.IsNullOrEmpty(match.Value) || placeholder.ContainsKey(match.Value) || match.Value.Length<2)
          continue;

        var prop = props.FirstOrDefault(
          x => x.Name.Equals(match.Value.Substring(1, match.Value.Length - 2), StringComparison.OrdinalIgnoreCase));

        if (prop == null) continue;
        var val = Convert.ToString(prop.GetValue(obj));
        if (!string.IsNullOrEmpty(val))
          placeholder.Add(match.Value, val);
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

    public static string? CsvToolsStackTrace(this Exception exception)
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

    public static void SetMaximum(this IProcessDisplay? processDisplay, long maximum)
    {
      if (processDisplay is IProcessDisplayTime processDisplayTime)
        processDisplayTime.Maximum = maximum;
    }

    public static int ToInt(this ulong value) => value > int.MaxValue ? int.MaxValue : Convert.ToInt32(value);

    public static int ToInt(this long value)
    {
      if (value > int.MaxValue)
        return int.MaxValue;
      return value < int.MinValue ? int.MinValue : Convert.ToInt32(value);
    }

    public static int ToInt(this decimal value)
    {
      if (value > int.MaxValue)
        return int.MaxValue;
      return value < int.MinValue ? int.MinValue : Convert.ToInt32(value);
    }

    public static int ToInt(this double value)
    {
      if (value > int.MaxValue)
        return int.MaxValue;
      return value < int.MinValue ? int.MinValue : Convert.ToInt32(value);
    }

    public static long ToInt64(this double value)
    {
      if (value > long.MaxValue)
        return long.MaxValue;
      return value < long.MinValue ? long.MinValue : Convert.ToInt64(value);
    }

    public static long ToInt64(this decimal value)
    {
      if (value > long.MaxValue)
        return long.MaxValue;
      return value < long.MinValue ? long.MinValue : Convert.ToInt64(value);
    }

    public static string ToStringHandle0(this char input) => input=='\0' ? string.Empty : input.ToString();

    public static string GetDescription(this char input) => input.ToStringHandle0().GetDescription();

    /// <summary>
    ///   Gets a char from a text
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns></returns>
    public static string GetDescription(this string input)
    {
      if (string.IsNullOrEmpty(input))
        return string.Empty;

      return input.WrittenPunctuationToChar() switch
      {
        '\t' => "Horizontal Tab",
        ' ' => "Space",
        (char) 0xA0 => "Non-breaking space",
        '\\' => "Backslash: \\",
        '/' => "Slash: /",
        ',' => "Comma: ,",
        ';' => "Semicolon: ;",
        ':' => "Colon: :",
        '|' => "Pipe: |",
        '\"' => "Quotation marks: \"",
        '\'' => "Apostrophe: \'",
        '&' => "Ampersand: &",
        '*' => "Asterisk: *",
        '`' => "Tick Mark: `",
        '✓' => "Check mark: ✓",
        '\u001F' => "Unit Separator: Char 31",
        '\u001E' => "Record Separator: Char 30",
        '\u001D' => "Group Separator: Char 29",
        '\u001C' => "File Separator: Char 28",
        _ => input
      };
    }

    /// <summary>
    ///   Return a string resolving written punctuation
    /// </summary>
    /// <param name="inputString"></param>
    /// <returns>A string of length 1 or empty</returns>
    public static string WrittenPunctuation(this string inputString)
    {
      if (string.IsNullOrEmpty(inputString))
        return string.Empty;

      if (inputString.Length==1)
      {
        if (inputString.Equals("␍", StringComparison.Ordinal))
          return "\r";
        if (inputString.Equals("␊", StringComparison.Ordinal))
          return "\n";
        return inputString;
      }

      if (inputString.Equals("Tab", StringComparison.OrdinalIgnoreCase) ||
            inputString.Equals("Tabulator", StringComparison.OrdinalIgnoreCase) ||
            inputString.Equals("Horizontal Tab", StringComparison.OrdinalIgnoreCase) ||
            inputString.Equals("HorizontalTab", StringComparison.OrdinalIgnoreCase))
        return "\t";

      if (inputString.Equals("Space", StringComparison.OrdinalIgnoreCase))
        return " ";

      if (inputString.Equals("hash", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("sharp", StringComparison.OrdinalIgnoreCase))
        return "#";

      if (inputString.Equals("whirl", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("at", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("monkey", StringComparison.OrdinalIgnoreCase))
        return "@";

      if (inputString.Equals("underbar", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("underscore", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("understrike", StringComparison.OrdinalIgnoreCase))
        return "_";

      if (inputString.Equals("Comma", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Comma: ,", StringComparison.OrdinalIgnoreCase))
        return ",";

      if (inputString.Equals("Dot", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Point", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Full Stop", StringComparison.OrdinalIgnoreCase))
        return ".";

      if (inputString.Equals("amper", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("ampersand", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Ampersand: &", StringComparison.OrdinalIgnoreCase))
        return "&";

      if (inputString.Equals("Pipe", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Vertical bar", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("VerticalBar", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Pipe: |", StringComparison.OrdinalIgnoreCase))
        return "|";

      if (inputString.Equals("broken bar", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("BrokenBar", StringComparison.OrdinalIgnoreCase))
        return "¦";

      if (inputString.Equals("fullwidth broken bar", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("FullwidthBrokenBar", StringComparison.OrdinalIgnoreCase))
        return "￤";

      if (inputString.Equals("Semicolon", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Semicolon: ;", StringComparison.OrdinalIgnoreCase))
        return ";";

      if (inputString.Equals("Colon", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Colon: :", StringComparison.OrdinalIgnoreCase))
        return ":";

      if (inputString.Equals("Doublequote", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Doublequotes", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Quote", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Quotation marks", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Quotation marks: \"", StringComparison.OrdinalIgnoreCase))
        return "\"";

      if (inputString.Equals("Apostrophe", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Singlequote", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("tick", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Apostrophe: \'", StringComparison.OrdinalIgnoreCase))
        return "'";

      if (inputString.Equals("Slash", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Stroke", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("forward slash", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Slash: /", StringComparison.OrdinalIgnoreCase))
        return "/";

      if (inputString.Equals("backslash", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("backslant", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Backslash: \\", StringComparison.OrdinalIgnoreCase))
        return "\\";

      if (inputString.Equals("Tick", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Tick Mark", StringComparison.OrdinalIgnoreCase))
        return "`";

      if (inputString.Equals("Star", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Asterisk", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Asterisk: *", StringComparison.OrdinalIgnoreCase))
        return "*";

      if (inputString.Equals("NBSP", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Non-breaking space", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Non breaking space", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("NonBreakingSpace", StringComparison.OrdinalIgnoreCase))
        return "\u00A0";

      if (inputString.Equals("Return", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("CarriageReturn", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("CR", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("␍", StringComparison.Ordinal) ||
          inputString.Equals("Carriage return", StringComparison.OrdinalIgnoreCase))
        return "\r";

      if (inputString.Equals("Check mark", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("Check", StringComparison.OrdinalIgnoreCase))
        return "✓";

      if (inputString.Equals("Feed", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("LineFeed", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("LF", StringComparison.OrdinalIgnoreCase) ||
          inputString.Equals("␊", StringComparison.Ordinal) ||
          inputString.Equals("Line feed", StringComparison.OrdinalIgnoreCase))
        return "\n";

      if (inputString.StartsWith("Unit separator", StringComparison.OrdinalIgnoreCase) ||
          inputString.Contains("31") ||
          inputString.Equals("␟", StringComparison.Ordinal) ||
          inputString.Equals("US", StringComparison.OrdinalIgnoreCase))
        return "\u001F";

      if (inputString.StartsWith("Record separator", StringComparison.OrdinalIgnoreCase) ||
          inputString.Contains("30") ||
          inputString.Equals("␞", StringComparison.Ordinal) ||
          inputString.Equals("RS", StringComparison.OrdinalIgnoreCase))
        return "\u001E";

      if (inputString.StartsWith("Group separator", StringComparison.OrdinalIgnoreCase) ||
          inputString.Contains("29") ||
          inputString.Equals("GS", StringComparison.OrdinalIgnoreCase))
        return "\u001D";

      if (inputString.StartsWith("File separator", StringComparison.OrdinalIgnoreCase) ||
          inputString.Contains("28") ||
          inputString.Equals("FS", StringComparison.OrdinalIgnoreCase))
        return "\u001C";

      return inputString.Substring(0, 1);
    }

    /// <summary>
    ///   Replaces a written English punctuation to the punctuation character
    /// </summary>
    /// <param name="inputString">The source</param>
    /// <returns>return '\0' if the text was not interpreted as punctuation</returns>
    public static char WrittenPunctuationToChar(this string inputString) => string.IsNullOrEmpty(inputString) ? '\0' : WrittenPunctuation(inputString)[0];

    public static char StringToChar(this string inputString) => string.IsNullOrEmpty(inputString) ? '\0' : inputString[0];

#if !GetHashByGUID

    /// <summary>
    ///   Check if a collection is equal, the items can be in any order as long as all exist in the
    ///   other collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self">The collection.</param>
    /// <param name="other">The other collection.</param>
    /// <returns></returns>
    /// <remarks>
    ///   Parameter is IEnumerable to make it work with ICollections, IReadOnlyCollection, Arrays
    ///   and ObservableCollection
    /// </remarks>
    public static bool CollectionEqual<T>(this IEnumerable<T> self, IEnumerable<T>? other) where T : IEquatable<T>
    {
      if (other == null)
        return false;
      if (ReferenceEquals(other, self))
        return true;
      // ReSharper disable PossibleMultipleEnumeration Making sure the passed in IEnumerable is
      // indeed a collection, otherwise make it one
      if (!(self is Collection<T> || self is ICollection<T> || self is IReadOnlyCollection<T>))
        self = self.ToList();
      if (!(other is Collection<T> || other is ICollection<T> || other is IReadOnlyCollection<T>))
        other = other.ToList();
      if (other.Count() != self.Count())
        return false;
      if (!other.Any()) // both are empty
        return true;
      var ret = other.All(ot => ot == null ? self.Any(x => x == null) : self.Any(ot.Equals));
      // Check all items, all should be the same, order does not matter though
      return ret;
    }

    /// <summary>
    ///   Check if two enumerations are equal, the items need to be in the right order
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self">The collection.</param>
    /// <param name="other">The other collection.</param>
    /// <returns></returns>
    public static bool CollectionEqualWithOrder<T>(this IEnumerable<T> self, IEnumerable<T>? other)
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
      using var selfEnum = self.GetEnumerator();
      using var otherEnum = other.GetEnumerator();
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
        return collection.Cast<object>()
                         .Aggregate(731, (current, item) => (current * 397) ^ (item.GetHashCode() + order++));
      }
    }

#endif
  }
}