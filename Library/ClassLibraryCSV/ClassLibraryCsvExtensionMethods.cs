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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
// ReSharper disable CommentTypo

namespace CsvTools
{
  public delegate DateTime TimeZoneChangeDelegate(in DateTime input, in string srcTimeZone, in string destTimeZone, in Action<string>? handleWarning);



  /// <summary>
  ///   Class with extensions used in the class Library
  /// </summary>
  public static class ClassLibraryCsvExtensionMethods
  {

    /// <summary>
    ///   Gets the passphrase, possibly using the UI when needed 
    /// </summary>
    /// <param name="fileName">Path to the file</param>
    /// <param name="defaultPassphrase">The possibly already known passphrase</param>
    /// <param name="getPassphraseInteractive">A routine used to get a passpjharse interactively</param>
    /// <returns>The Passphrase to use</returns>
    public static string GetPassphraseForFile(this string fileName,
      in string defaultPassphrase,
      Func<string>? getPassphraseInteractive)
    {
      if (!(fileName.AssumePgp() || fileName.AssumeZip()))
        return string.Empty;
      var passphrase = defaultPassphrase;

#if SupportPGP
      // Passphrase stored in Setting
      if (passphrase.Length == 0)
        passphrase = PgpHelper.LookupPassphrase(fileName);
#endif

      // try to get the passphrase stored for the key
      if (passphrase.Length == 0 && getPassphraseInteractive !=null)
        passphrase = getPassphraseInteractive.Invoke();

      if (passphrase.Length == 0)
        throw new FileReaderException("A passphrase is needed for decryption.");

      // when the passphrase is valid it will stored it with PGPKeyStorage
      return passphrase;
    }

#if SupportPGP
    public static (string passphrase, string keyFile, string key) GetKeyAndPassphraseForFile(this string fileName, string passphrase,
      string keyFile, Func<(string passphrase, string keyFile, string key)>? getKeyAndPassphraseInteractive)
    {
      if (!(fileName.AssumePgp()))
        return (string.Empty, string.Empty, string.Empty);

      // Passphrase stored in Setting
      if (passphrase.Length == 0)
        passphrase = PgpHelper.LookupPassphrase(fileName);

      if (keyFile.Length==0)
        keyFile = PgpHelper.LookupKeyFile(fileName);

      var key = PgpHelper.GetKeyAndValidate(fileName, keyFile);
      if ((key.Length == 0 || passphrase.Length == 0) && getKeyAndPassphraseInteractive!= null)
      {
        var res = getKeyAndPassphraseInteractive.Invoke();
        key = res.key;
        keyFile = res.keyFile;
        passphrase= res.passphrase;
      }
      if (passphrase.Length == 0)
        throw new FileReaderException("A passphrase is needed for decryption.");
      if (key.Length == 0)
        throw new EncryptionException("The key information is needed");

      return (passphrase, keyFile, key);
    }

    /// <summary>
    ///   Gets the key to use possibly using the UI when needed 
    /// </summary>
    /// <param name="fileName">Path to the file</param>
    /// <param name="keyFile">The file setting that may contain information on the passphrase</param>
    /// <param name="getKeyInteractive">A routine used to check possibly already known keys for that file</param>
    /// <returns>The Passphrase to use</returns>
    public static string GetKeyForFile(this string fileName,
      in string keyFile, Func<string>? getKeyInteractive)
    {
      if (!fileName.AssumePgp())
        return string.Empty;

      // Key stored in file 
      var key = PgpHelper.GetKeyAndValidate(fileName, keyFile);

      // Key file stored in in lookup
      if (key.Length == 0)
        key = PgpHelper.LookupKey(fileName);

      // try to get the passphrase stored for the key, there is no key file in this case
      if (key.Length == 0 && getKeyInteractive !=null)
        key = getKeyInteractive.Invoke();

      if (key.Length == 0)
        throw new EncryptionException("The key information is needed");

      return key;
    }
#endif
    /// <summary>
    /// Move the field from on position in the list to another
    /// </summary>
    /// <param name="list">The List with the items.</param>
    /// <param name="oldIndex">The position of the item to move from</param>
    /// <param name="newIndex">The position of the item to move to</param>
    [DebuggerStepThrough]
    public static void Move<T>(this IList<T> list, int oldIndex, int newIndex) where T : notnull
    {
      if (oldIndex== newIndex)
        return;
      T removedItem = list[oldIndex];
      list.RemoveAt(oldIndex);
      list.Insert(newIndex, removedItem);
    }

    public static int IdentifierHash(this string name)
      => name.ToUpperInvariant().GetHashCode();

    public static int IdentifierHash(this string name, in string name2)
      => name.ToUpperInvariant().GetHashCode() + name2.ToUpperInvariant().GetHashCode();


    public static bool AssumeDeflate(this string fileName) =>
      fileName.EndsWith(".cmp", StringComparison.OrdinalIgnoreCase)
      || fileName.EndsWith(".dfl", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///   Check if the application should assume its gZIP
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public static bool AssumeGZip(this string fileName) =>
      fileName.EndsWith(".gz", StringComparison.OrdinalIgnoreCase)
      || fileName.EndsWith(".gzip", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///   Check if the application should assume its PGP.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public static bool AssumePgp(this string fileName) =>
      fileName.EndsWith(".pgp", StringComparison.OrdinalIgnoreCase)
      || fileName.EndsWith(".gpg", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///   Check if the application should assume its ZIP
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public static bool AssumeZip(this string fileName) => fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);

    public static bool AssumeDelimited(this string fileName) =>
      fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ||
      fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)||
      fileName.EndsWith(".tab", StringComparison.OrdinalIgnoreCase)||
      fileName.EndsWith(".tsv", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///   Copies all elements from one collection to the other
    /// </summary>
    /// <typeparam name="T">the type</typeparam>
    /// <param name="self">The collection.</param>
    /// <param name="other">The other collection.</param>
    [DebuggerStepThrough]
    public static void CollectionCopy<T>(this IEnumerable<T> self, ICollection<T>? other)
      where T : ICloneable
    {
      if (other is null) return;
      other.Clear();
      foreach (var item in self)
        other.Add((T) item.Clone());
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
      if (other is null)
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
    public static int Count(this IEnumerable? items) =>
      items switch
      {
        null => 0,
        ICollection col => col.Count,
        _ => Enumerable.Count(items.Cast<object>())
      };

    /// <summary>
    /// Get the descriptive text for an enum value
    /// </summary>
    /// <param name="value">The enum value</param>
    /// <returns>The description attribute of the value or the name, if not set the name of the enum</returns>
    public static string Description(this Enum value)
    {
      var fieldInfo = value.GetType().GetField(value.ToString());
      DescriptionAttribute? attribute = null;
      if (fieldInfo != null)
        attribute = fieldInfo.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
      return attribute?.Description ?? value.ToString();
    }

    /// <summary>
    /// Get the short descriptive text for an enum value
    /// </summary>
    /// <param name="value">The enum value</param>
    /// <returns>The short description attribute of the value or the name, if that is not set empty string</returns>
    public static string ShortDescription(this Enum value)
    {
      var fieldInfo = value.GetType().GetField(value.ToString());
      ShortDescriptionAttribute? attribute = null;
      if (fieldInfo != null)
        attribute = fieldInfo.GetCustomAttribute(typeof(ShortDescriptionAttribute)) as ShortDescriptionAttribute;
      return attribute?.ShortDescription ?? string.Empty;
    }

    /// <summary>
    ///   Get the Display Text of an Enum, using <see cref="ShortDescription"/> and as fallback <see cref="Description"/>
    /// </summary>
    public static string Display(this Enum value)
    {
      var shortDescription = value.ShortDescription();
      return (shortDescription.Length>0) ? shortDescription : value.Description();
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
      if (exception.InnerException is null) return sb.ToString();
      sb.Append("\n");
      sb.Append(exception.InnerExceptionMessages(maxDepth - 1));

      return sb.ToString();
    }

    /// <summary>
    ///   Gets the CsvTools type for a .NET type
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The matching <see cref="DataTypeEnum" />.</returns>
    public static DataTypeEnum GetDataType(this Type type)
    {
      switch (Type.GetTypeCode(type))
      {
        case TypeCode.Boolean:
          return DataTypeEnum.Boolean;

        case TypeCode.DateTime:
          return DataTypeEnum.DateTime;

        case TypeCode.Single:
        case TypeCode.Double:
          return DataTypeEnum.Double;

        case TypeCode.Decimal:
          return DataTypeEnum.Numeric;

        case TypeCode.Byte:
        case TypeCode.Int16:
        case TypeCode.Int32:
        case TypeCode.Int64:
        case TypeCode.SByte:
        case TypeCode.UInt16:
        case TypeCode.UInt32:
        case TypeCode.UInt64:
          return DataTypeEnum.Integer;

        case TypeCode.Object when type.ToString().Equals("System.Image", StringComparison.Ordinal):
          return DataTypeEnum.Binary;

        case TypeCode.Object when type.ToString().Equals("System.TimeSpan", StringComparison.Ordinal):
          return DataTypeEnum.DateTime;

        case TypeCode.Object when type.ToString().Equals("System.Guid", StringComparison.Ordinal):
          return DataTypeEnum.Guid;

        case TypeCode.Char:
        case TypeCode.String:
        case TypeCode.Object:
        case TypeCode.Empty:
        case TypeCode.DBNull:
          return DataTypeEnum.String;

        default:
          return DataTypeEnum.String;
      }
    }


    /// <summary>
    ///   Gets a suitable ID for a filename
    /// </summary>
    /// <param name="path">The complete path to a file</param>
    /// <returns>The filename without special characters</returns>
    public static string GetIdFromFileName(this string path)
    {
      var fileName = FileSystemUtils.SplitPath(path).FileNameWithoutExtension.ProcessByCategory(
        x => x == UnicodeCategory.UppercaseLetter || x == UnicodeCategory.LowercaseLetter
                                                  || x == UnicodeCategory.OtherLetter
                                                  || x == UnicodeCategory.ConnectorPunctuation
                                                  || x == UnicodeCategory.DashPunctuation
                                                  || x == UnicodeCategory.OtherPunctuation
                                                  || x == UnicodeCategory.DecimalDigitNumber);

      const string timeSep = @"(:|-|_)?";
      const string dateSep = @"(\/|\.|-|_)?";

      const string hour = @"(2[0-3]|((0|1)\d))"; // 00-09 10-19 20-23
      const string minSec = @"([0-5][0-9])"; // 00-59
      const string amPm = @"((_| )?(AM|PM))?";

      const string year = @"((19\d{2})|(2\d{3}))"; // 1900 - 2999
      const string month = @"(0[1-9]|1[012])"; // 01-12
      const string day = @"(0[1-9]|[12]\d|3[01])"; // 01 - 31

      // Replace Dates YYYYMMDD / MMDDYYYY / DDMMYYYY
      fileName = Regex.Replace(
        fileName,
        "(" + dateSep + year + dateSep + month + dateSep + day + ")|(" + dateSep + month + dateSep + day + dateSep
        + year + ")|(" + dateSep + day + dateSep + month + dateSep + year + ")",
        string.Empty,
        RegexOptions.Singleline);

      // Replace Times 3_53_34_AM
      fileName = Regex.Replace(
        fileName,
        dateSep + hour + timeSep + minSec + timeSep + minSec + "?" + amPm,
        string.Empty,
        RegexOptions.IgnoreCase | RegexOptions.Singleline);

      return fileName.Trim('_', '-', ' ', '\t').Replace("__", "_").Replace("__", "_").Replace("--", "-")
        .Replace("--", "-");
    }

    /// <summary>
    ///   Gets the .NET type for a given CsvTools type Always using long for integer values no
    ///   matter if 32 or 64 bit system
    /// </summary>
    /// <param name="dt">The <see cref="DataTypeEnum" />.</param>
    /// <returns>The .NET Type</returns>
    public static Type GetNetType(this DataTypeEnum dt) =>
      dt switch
      {
        DataTypeEnum.DateTime => typeof(DateTime),
        DataTypeEnum.Integer => typeof(long),
        DataTypeEnum.Double => typeof(double),
        DataTypeEnum.Numeric => typeof(decimal),
        DataTypeEnum.Boolean => typeof(bool),
        DataTypeEnum.Guid => typeof(Guid),
        _ => typeof(string)
      };

    /// <summary>
    ///   Get a list of column names that are not artificial
    /// </summary>
    /// <param name="dataTable">The <see cref="DataTable" /> containing the columns</param>
    /// <returns>A enumeration of ColumnNames</returns>
    public static IEnumerable<DataColumn> GetRealColumns(this DataTable dataTable) =>
      dataTable.Columns.Cast<DataColumn>().Where(col => NoArtificialField(col.ColumnName));

    public static bool NoArtificialField(this string columnName) => !ReaderConstants.ArtificialFields.Contains(columnName);

    /// <summary>
    ///   Combines all inner exceptions to one formatted string for logging.
    /// </summary>
    /// <param name="exception">The exception of type <see cref="Exception" /></param>
    /// <param name="maxDepth">The maximum depth.</param>
    /// <returns>A string with all inner messages of the error stack</returns>
    [DebuggerStepThrough]
    public static string InnerExceptionMessages(this Exception? exception, int maxDepth = 2)
    {
      if (exception is null)
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

    public static string NewLineString(this RecordDelimiterTypeEnum type) =>
      type switch
      {
        RecordDelimiterTypeEnum.Lf => "\n",
        RecordDelimiterTypeEnum.Cr => "\r",
        RecordDelimiterTypeEnum.Crlf => "\r\n",
        RecordDelimiterTypeEnum.Lfcr => "\n\r",
        RecordDelimiterTypeEnum.Rs => "\x1E",
        RecordDelimiterTypeEnum.Us => "▼",
        RecordDelimiterTypeEnum.Nl => "\x15",
        RecordDelimiterTypeEnum.None => string.Empty,
        _ => string.Empty
      };
#if !QUICK


#endif
    /// <summary>
    ///   Replaces placeholders with a text. The placeholder are identified surrounding { or [
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="placeholder">The placeholder.</param>
    /// <param name="replacement">The replacement.</param>
    /// <returns>The new text based on input</returns>
    [DebuggerStepThrough]
    public static string PlaceholderReplace2(this string input, in string placeholder, string replacement)
    {
      if (string.IsNullOrEmpty(replacement)) return input;

      var type = "{" + placeholder.Trim() + "}";
      if (input.IndexOf(type, StringComparison.OrdinalIgnoreCase) == -1)
      {
        type = "[" + placeholder.Trim() + "]";
        if (input.IndexOf(type, StringComparison.OrdinalIgnoreCase) == -1)
        {
          return input;
        }
      }
      return input.ReplaceCaseInsensitive(type, replacement);
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
    public static string PlaceholderReplace(this string input, in string placeholder, string replacement)
    {
      if (string.IsNullOrEmpty(replacement)) return input;

      var type = "{" + placeholder + "}";
      if (input.IndexOf(type, StringComparison.OrdinalIgnoreCase) == -1)
      {
        type = "#" + placeholder + "#";
        if (input.IndexOf(type, StringComparison.OrdinalIgnoreCase) == -1)
        {
          type = "#" + placeholder;
          if (!input.EndsWith(type, StringComparison.OrdinalIgnoreCase)
              && input.IndexOf(type + " ", StringComparison.OrdinalIgnoreCase) == -1)
            return input;
        }
      }

      // Not sure why this is in the code, where was it needed?
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

    public static string PlaceholderReplaceFormat(this string input, string placeholder, in string formatedDateTime)
    {
      // in case we have a placeholder with a formatting part e.G. {date:yyyy-MM-dd} we us
      // string.Format to process {0:...

      // General regex without matching the placeholder name is: (?:[{#])([^:\s}#]*)(:[^}]*)?(?:[}#\s])

      // Needs to start with # or { Ends with #, space or } May contain a Format description
      // starting with : ending with }
      var regEx = new Regex(
        @"(?:[{#])(" + Regex.Escape(placeholder) + @")(:[^}]*)?(?:[}#\s])",
        RegexOptions.IgnoreCase | RegexOptions.Singleline);

      return !regEx.IsMatch(input)
        ? PlaceholderReplace(input, placeholder, formatedDateTime)
        : string.Format(regEx.Replace(input, "{0$2}"), formatedDateTime);
    }

    /// <summary>
    ///   String replace function that is Case Insensitive
    /// </summary>
    /// <param name="original">The source</param>
    /// <param name="pattern">The text to replace</param>
    /// <param name="replacement">the character to which it should be changed</param>
    /// <returns>The source text with the replacement</returns>
    [DebuggerStepThrough]
    public static string ReplaceCaseInsensitive(this string original, in string? pattern, char replacement)
    {
      if (pattern is null || pattern.Length == 0)
        return original;

      var count = 0;
      var position0 = 0;
      int position1;

      var chars = new char[original.Length + Math.Max(0, original.Length / pattern.Length * (1 - pattern.Length))];

      var upperString = original.ToUpperInvariant();
      var upperPattern = pattern.ToUpperInvariant();
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
    public static string ReplaceCaseInsensitive(this string original, in string? pattern, in string replacement)
    {
      if (pattern is null || pattern.Length == 0)
        return original;

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
    /// Replaces characters, if the replacement is \0 the character will be removed
    /// </summary>
    /// <param name="inputValue"></param>
    /// <param name="old1"></param>
    /// <param name="new1"></param>
    /// <param name="old2"></param>
    /// <param name="new2"></param>
    /// <returns></returns>
    public static string ReplaceDefaults(this string inputValue, in char old1, in char new1, in char old2,
      in char new2)
    {
      if (inputValue.Length==0)
        return string.Empty;
      // Assume the text stays the same, it could only be shorter
      var result = new char[inputValue.Length];
      int pos = 0;
      // ReSharper disable once ForCanBeConvertedToForeach
      for (int i = 0; i < inputValue.Length; i++)
      {
        if (inputValue[i] == old1)
        {
          if (new1 != char.MinValue)
            result[pos++]=new1;
        }
        else if (inputValue[i] == old2)
        {
          if (new2 != char.MinValue)
            result[pos++]=new2;
        }
        else
          result[pos++] = inputValue[i];
      }
      return new string(result, 0, pos);
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
    public static string ReplaceDefaults(this string inputValue, in string old1, in string new1, in string old2,
      in string new2)
    {
      if (inputValue.Length==0)
        return string.Empty;

      if (old1.Length==1 && new1.Length==1 && old2.Length==1 && new2.Length==1)
        return ReplaceDefaults(inputValue, old1[0], new1[0], old2[0], new2[0]);

      var exchange1 = !string.IsNullOrEmpty(old1) && string.Compare(old1, new1, StringComparison.Ordinal) != 0;
      var exchange2 = !string.IsNullOrEmpty(old2) && string.Compare(old2, new2, StringComparison.Ordinal) != 0;

      if (exchange1 && exchange2 && string.Equals(new1, old2))
      {
        inputValue = inputValue.Replace(old1, "{\0}");
        inputValue = inputValue.Replace(old2, new2);
        return inputValue.Replace("{\0}", new1);
      }

      if (exchange1)
        inputValue = inputValue.Replace(old1, new1);
      if (exchange2)
        inputValue = inputValue.Replace(old2, new2);

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

      // get all placeholders in brackets
      var rgx = new Regex(@"\{[^\}]+\}");

      var placeholder = new Dictionary<string, string>();
      var props = obj.GetType().GetProperties().Where(prop => prop.GetMethod != null).ToList();

      // ReSharper disable once RedundantEnumerableCastCall
      foreach (var value in rgx.Matches(template).OfType<Match>().Select(x => x.Value))
      {
        if (string.IsNullOrEmpty(value) || placeholder.ContainsKey(value) || value.Length < 2)
          continue;

        var prop = props.FirstOrDefault(
          x => x.Name.Equals(value.Substring(1, value.Length - 2), StringComparison.OrdinalIgnoreCase));

        if (prop is null) continue;
        var val = Convert.ToString(prop.GetValue(obj));
        if (!string.IsNullOrEmpty(val))
          placeholder.Add(value, val);
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

      // get all placeholders in brackets
      var rgx = new Regex(@"\{[^\}]+\}");

      var placeholder = new Dictionary<string, string>();
      var index = 0;
      // ReSharper disable once RedundantEnumerableCastCall
      foreach (var value in rgx.Matches(template).OfType<Match>().Select(x => x.Value))
      {
        if (index >= values.Length)
          break;
        if (!placeholder.ContainsKey(value))
          placeholder.Add(value, values[index++]);
      }

      // replace them with the property value from setting
      template = placeholder.Aggregate(template, (current, pro) => current.ReplaceCaseInsensitive(pro.Key, pro.Value));

      return template.Replace("  ", " ");
    }

    public static void SetMaximum(this IProgress<ProgressInfo>? progress, long maximum)
    {
      if (!(progress is IProgressTime progressTime)) return;
      try
      {
        progressTime.Maximum = maximum;
      }
      catch (InvalidOperationException)
      {
        // ignore
      }
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

    public static char StringToChar(this string inputString) =>
      string.IsNullOrEmpty(inputString) ? char.MinValue : inputString[0];

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
      if (value < long.MinValue)
        return long.MinValue;

      return value.Equals(double.NaN) ? default : Convert.ToInt64(value);
    }

    public static long ToInt64(this decimal value)
    {
      if (value > long.MaxValue)
        return long.MaxValue;
      return value < long.MinValue ? long.MinValue : Convert.ToInt64(value);
    }

    public static string ToStringHandle0(this char input) =>
      input == char.MinValue ? string.Empty : input.ToString();


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
    public static bool CollectionEqual<T>(this IEnumerable<T> self, in IEnumerable<T>? other)
      where T : IEquatable<T>
    {
      if (other is null)
        return false;
      if (ReferenceEquals(other, self))
        return true;
      ICollection<T> selfCol = self.ToArray();
      if (other is Collection<T> || other is ICollection<T> || other is IReadOnlyCollection<T>)
      {
        var otherNum = other.Count();
        if (otherNum != selfCol.Count)
          return false;
        if (otherNum == 0) // both are empty
          return true;
      }

      foreach (var ot in other)
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (ot is null)
        {
          // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
          if (!selfCol.Any(x => x is null))
            return false;
        }
        else
        {
          if (!selfCol.Any(st => ot.Equals(st)))
            return false;
        }

      return true;
    }

    /// <summary>
    ///   Check if two enumerations are equal, the items need to be in the right order
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self">The collection.</param>
    /// <param name="other">The other collection.</param>
    /// <returns></returns>
    public static bool CollectionEqualWithOrder<T>(this IEnumerable<T> self, in IEnumerable<T>? other)
    {
      if (self is null)
        throw new ArgumentNullException(nameof(self));
      if (other is null)
        return false;

      if (ReferenceEquals(other, self))
        return true;

      // Shortcut if we have collections but different number of Items
      if (self is ICollection<T> selfCol && other is ICollection<T> otherCol && selfCol.Count != otherCol.Count)
        return false;

      // use Enumerators to compare the two collections
      using var selfEnum = self.GetEnumerator();
      using var otherEnum = other.GetEnumerator();
      IEqualityComparer<T> comparer = EqualityComparer<T>.Default;

      while (true)
      {
        // move to the next item
        var s = selfEnum.MoveNext();
        var o = otherEnum.MoveNext();
        if (!s && !o)
          return true;
        if (!s || !o)
          return false;
        if (selfEnum.Current is null || otherEnum.Current is null || !comparer.Equals(selfEnum.Current, otherEnum.Current))
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
        return collection.Cast<object>().Aggregate(
          731,
          (current, item) => (current * 397) ^ (item.GetHashCode() + order++));
      }
    }

#endif
  }
}