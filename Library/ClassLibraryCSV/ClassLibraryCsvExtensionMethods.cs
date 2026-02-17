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

namespace CsvTools;

/// <summary>
/// Delegate for a routine that handles time zone conversion between two identifiers.
/// </summary>
/// <param name="input">The <see cref="DateTime"/> value to be converted.</param>
/// <param name="sourceTimeZone">The source time zone identifier.</param>
/// <param name="destinationTimeZone">The target time zone identifier.</param>
/// <param name="handleWarning">
/// Optional action invoked to report warnings, such as unknown or invalid time zones.
/// </param>
/// <returns>The converted <see cref="DateTime"/> value in the target time zone.</returns>
#pragma warning disable MA0048 // File name must match type name
public delegate DateTime TimeZoneChangeDelegate(in DateTime input, string sourceTimeZone, string destinationTimeZone, Action<string>? handleWarning);
#pragma warning restore MA0048 // File name must match type name

/// <summary>
/// Provides extension methods used within the CSV Class Library.
/// </summary>
public static class ClassLibraryCsvExtensionMethods
{
  // Standardized Regex to capture content from any supported delimiter
  // Groups: 1=Full Match, 2=Content (Key), 3=Optional Format (after :)
  // Updated Regex to support {{ }}, { }, [ ], ( ), <: > and # #  #word
  // It uses a conditional (?(group)then|else) to enforce the correct closing tag.
  private static readonly Regex m_UnifiedPlaceholderRegEx = new Regex(
      // 1. Match the opening and content, then use a branch reset or alternation for the close
      @"(?:" +
          @"\{\{\s*(?<content>.*?)(?:\s*:\s*(?<format>[^}]+))?\s*\}\}|" +   // {{key:fmt}}
          @"\{(?<!\{\{)\s*(?<content>.*?)(?:\s*:\s*(?<format>[^}]+))?\s*\}|" + // {key:fmt}
          @"\[\s*(?<content>.*?)(?:\s*:\s*(?<format>[^\]]+))?\s*\]|" +      // [key:fmt]
          @"\(\s*(?<content>.*?)(?:\s*:\s*(?<format>[^\)]+))?\s*\)|" +      // (key:fmt)
          @"<:\s*(?<content>.*?)(?:\s*:\s*(?<format>[^>]+))?\s*>|" +        // <:key:fmt>
          @"#\s*(?<content>.*?)(?:\s*:\s*(?<format>[^#\s]+))?\s*(?:#|(?=\s|$))" + // #key:fmt# or #key
      @")",
      RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

  /// <summary>
  /// Determines if the file should be treated as a "deflate" compressed file based on its extension.
  /// </summary>
  /// <param name="fileName">The name or path of the file.</param>
  /// <returns><c>true</c> if the extension is .cmp or .dfl; otherwise, <c>false</c>.</returns>
  public static bool AssumeDeflate(this string fileName) =>
    fileName.EndsWith(".cmp", StringComparison.OrdinalIgnoreCase)
    || fileName.EndsWith(".dfl", StringComparison.OrdinalIgnoreCase);

  /// <summary>Determines if the file is a delimited text file based on its extension.</summary>
  /// <param name="fileName">The name or path of the file.</param>
  public static bool AssumeDelimited(this string fileName) =>
    fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) || AssumeDelimited1(fileName);

  /// <summary>Determines if the file is a common delimited format (.csv, .tab, .tsv).</summary>
  /// <param name="fileName">The name or path of the file.</param>
  public static bool AssumeDelimited1(this string fileName) =>
    fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) ||
    fileName.EndsWith(".tab", StringComparison.OrdinalIgnoreCase) ||
    fileName.EndsWith(".tsv", StringComparison.OrdinalIgnoreCase);

  /// <summary>
  /// Determines if the file should be treated as a GZip compressed file based on its extension.
  /// </summary>
  /// <param name="fileName">The name or path of the file.</param>
  public static bool AssumeGZip(this string fileName) =>
    fileName.EndsWith(".gz", StringComparison.OrdinalIgnoreCase)
    || fileName.EndsWith(".gzip", StringComparison.OrdinalIgnoreCase);

  /// <summary>
  /// Determines if the file should be treated as a PGP/GPG encrypted file based on its extension.
  /// </summary>
  /// <param name="fileName">The name or path of the file.</param>
  public static bool AssumePgp(this string fileName) =>
    fileName.EndsWith(".pgp", StringComparison.OrdinalIgnoreCase)
    || fileName.EndsWith(".gpg", StringComparison.OrdinalIgnoreCase);

  /// <summary>
  /// Checks if a string contains supported placeholder patterns: {xxx}, (xxx), #xxx#, or &lt;:xxx&gt;.
  /// </summary>
  /// <param name="input">The text to inspect for placeholders.</param>
  /// <returns><c>true</c> if a potential placeholder pattern is detected.</returns>
  public static bool AssumePlaceholderPresent(this string input)
  {
    if (string.IsNullOrEmpty(input))
      return false;
    int start;

    // Check for pairs: {}, [], ()
    if ((start = input.IndexOf('{')) != -1 && input.IndexOf('}', start + 1) != -1)
      return true;

    if ((start = input.IndexOf('[')) != -1 && input.IndexOf(']', start + 1) != -1)
      return true;

    if ((start = input.IndexOf('(')) != -1 && input.IndexOf(')', start + 1) != -1)
      return true;

    // Check for <: >
    if ((start = input.IndexOf("<:", StringComparison.Ordinal)) != -1 && input.IndexOf('>', start + 2) != -1)
      return true;

    // Check for # (Handles both #paired# and #open placeholders closed by space)
    start = input.IndexOf('#');
    // Returns true if # exists and is not the very last character
    if (start != -1 && start < input.Length - 1)
      return true;

    return false;
  }

  /// <summary>
  /// Determines if the file should be treated as a standard ZIP archive based on its extension.
  /// </summary>
  /// <param name="fileName">The name or path of the file.</param>  
  public static bool AssumeZip(this string fileName) =>
    fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);

  /// <summary>
  /// Creates a new list containing cloned copies of all items in the source collection.
  /// </summary>
  /// <param name="self">The source collection of cloneable items.</param>
  /// <returns>A new <see cref="IReadOnlyCollection{T}"/> containing the cloned entries.</returns>
  public static IReadOnlyCollection<T> Clone<T>(this IReadOnlyCollection<T> self) where T : ICloneable
  {
    var result = new List<T>(self.Count);
    self.CollectionCopyClone(result);
    return result;
  }

  /// <summary>
  /// Clears the target collection and populates it with cloned copies of elements from the source.
  /// </summary>
  /// <typeparam name="T">The element type, which must implement <see cref="ICloneable"/>.</typeparam>
  /// <param name="self">The source sequence of items to clone.</param>
  /// <param name="other">The target collection to be updated. Does nothing if null.</param>
  [DebuggerStepThrough]
  public static void CollectionCopyClone<T>(this IEnumerable<T> self, ICollection<T>? other)
    where T : ICloneable
  {
    if (other is null) return;
    other.Clear();
    foreach (var item in self)
      other.Add((T) item.Clone());
  }

  /// <summary>
  /// Clears the target collection and copies value-type elements from the source.
  /// </summary>
  /// <typeparam name="T">The value type.</typeparam>
  /// <param name="self">The source sequence.</param>
  /// <param name="other">The target collection. Does nothing if null.</param>
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
  /// Compares two collections for equality where the order of items does not matter.
  /// </summary>
  /// <param name="self">The first collection to compare.</param>
  /// <param name="other">The second collection to compare.</param>
  /// <returns><c>true</c> if both collections contain the same elements (regardless of order); otherwise, <c>false</c>.</returns>
  public static bool CollectionEqual<T>(this IEnumerable<T> self, in IEnumerable<T>? other)
    where T : IEquatable<T>
  {
    if (other is null || self is null)
      return false;

    ICollection<T> selfCol = self.ToArray();
    if (other is Collection<T> || other is ICollection<T> || other is IReadOnlyCollection<T>)
    {
      var otherNum = other.Count();
      if (otherNum != selfCol.Count)
        return false;
      if (otherNum == 0) // Both are empty
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
  /// Compares two enumerations for equality where elements must appear in the same order.
  /// </summary>
  /// <param name="self">The first sequence.</param>
  /// <param name="other">The second sequence.</param>
  /// <returns><c>true</c> if both sequences are equal in length and content order.</returns>
  public static bool CollectionEqualWithOrder<T>(this IEnumerable<T> self, in IEnumerable<T>? other)
  {
    if (self is null)
      throw new ArgumentNullException(nameof(self));
    if (other is null)
      return false;

    // Optimization: check counts if both are collections
    if (self is ICollection<T> selfCol && other is ICollection<T> otherCol && selfCol.Count != otherCol.Count)
      return false;

    using var selfEnum = self.GetEnumerator();
    using var otherEnum = other.GetEnumerator();
    IEqualityComparer<T> comparer = EqualityComparer<T>.Default;

    while (true)
    {
      var s = selfEnum.MoveNext();
      var o = otherEnum.MoveNext();

      if (!s && !o) return true; // Reached end of both
      if (!s || !o) return false; // Length mismatch

      if (selfEnum.Current is null && otherEnum.Current is null)
        continue;

      if (!comparer.Equals(selfEnum.Current, otherEnum.Current))
        return false;
    }
  }

  /// <summary>
  /// Generates a hash code for a collection where the order of items does not affect the result.
  /// </summary>
  /// <param name="collection">The collection to hash.</param>
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

  /// <summary>
  /// Returns the number of items in the enumeration, utilizing <see cref="ICollection.Count"/> if available.
  /// </summary>
  /// <param name="items">The items to count.</param>
  [DebuggerStepThrough]
  public static int Count(this IEnumerable? items) =>
    items switch
    {
      null => 0,
      ICollection col => col.Count,
      _ => Enumerable.Count(items.Cast<object>())
    };

  /// <summary>
  /// Retrieves the descriptive text for an Enum value via the <see cref="DescriptionAttribute"/>.
  /// </summary>
  /// <param name="value">The enum value.</param>
  /// <returns>The description if available; otherwise, the string representation of the value.</returns>
  public static string Description(this Enum value)
  {
    var fieldInfo = value.GetType().GetField(value.ToString());
    if (fieldInfo == null) return value.ToString();

    var attribute = fieldInfo.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
    return attribute?.Description ?? value.ToString();
  }

  /// <summary>
  /// Gets a display-friendly text for an Enum, preferring <see cref="ShortDescription"/> before falling back to <see cref="Description"/>.
  /// </summary>
  public static string Display(this Enum value)
  {
    var shortDescription = value.ShortDescription();
    return (shortDescription.Length > 0) ? shortDescription : value.Description();
  }

  /// <summary>
  /// Concatenates the current and inner exception messages into a single string.
  /// </summary>
  /// <param name="exception">The exception to process.</param>
  /// <param name="maxDepth">The maximum recursion depth for inner exceptions.</param>
  /// <returns>A newline-separated string of all error messages in the stack.</returns>
  [DebuggerStepThrough]
  public static string ExceptionMessages(this Exception exception, int maxDepth = 3)
  {
    var sb = new StringBuilder();

    if (exception is AggregateException ae)
    {
      foreach (var ie in ae.Flatten().InnerExceptions)
      {
        if (sb.Length > 0)
          sb.Append('\n');
        sb.Append(ie.Message);
      }
      return sb.ToString();
    }

    sb.Append(exception.Message);
    if (exception.InnerException is null || maxDepth <= 1) return sb.ToString();

    sb.Append('\n');
    sb.Append(exception.InnerExceptionMessages(maxDepth - 1));

    return sb.ToString();
  }

  /// <summary>
  /// Maps a .NET Type to a library-specific <see cref="DataTypeEnum"/>.
  /// </summary>
  /// <param name="type">The .NET Type.</param>
  /// <returns>The corresponding <see cref="DataTypeEnum"/>.</returns>
  public static DataTypeEnum GetDataType(this Type type)
    => Type.GetTypeCode(type) switch
    {
      TypeCode.Boolean => DataTypeEnum.Boolean,
      TypeCode.DateTime => DataTypeEnum.DateTime,
      TypeCode.Single => DataTypeEnum.Double,
      TypeCode.Double => DataTypeEnum.Double,
      TypeCode.Decimal => DataTypeEnum.Numeric,
      TypeCode.SByte => DataTypeEnum.Integer,
      TypeCode.Byte => DataTypeEnum.Integer,
      TypeCode.Int16 => DataTypeEnum.Integer,
      TypeCode.UInt16 => DataTypeEnum.Integer,
      TypeCode.Int32 => DataTypeEnum.Integer,
      TypeCode.UInt32 => DataTypeEnum.Integer,
      TypeCode.Int64 => DataTypeEnum.Integer,
      TypeCode.UInt64 => DataTypeEnum.Integer,
      TypeCode.Object when type.ToString().Equals("System.Image", StringComparison.Ordinal) => DataTypeEnum.Binary,
      TypeCode.Object when type.ToString().Equals("System.TimeSpan", StringComparison.Ordinal) => DataTypeEnum.DateTime,
      TypeCode.Object when type.ToString().Equals("System.Guid", StringComparison.Ordinal) => DataTypeEnum.Guid,
      _ => DataTypeEnum.String
    };

  /// <summary>
  /// Generates a clean identifier from a file path by removing date/time patterns and special characters.
  /// </summary>
  /// <param name="path">The full path to the file.</param>
  /// <returns>A sanitized string suitable for use as an ID.</returns>
  public static string GetIdFromFileName(this string path)
  {
    if (string.IsNullOrWhiteSpace(path)) return "id";

    // 1. Extract the filename
    var fileName = FileSystemUtils.SplitPath(path).FileNameWithoutExtension;

    // 2. Define patterns (Using readable Regex strings)
    const string timeSep = "[:\\-_]?";
    const string dateSep = @"[\/\.\-_]?";

    const string hour = @"(?:2[0-3]|[01]\d)";
    const string minSec = "[0-5][0-9]";
    const string amPm = @"(?:(?:\s|_)?(?:AM|PM))?";

    const string year = @"(?:19\d{2}|20\d{2})";
    const string month = "(?:0[1-9]|1[012])";
    const string day = @"(?:0[1-9]|[12]\d|3[01])";

    // Strip Dates: YYYYMMDD / MMDDYYYY / DDMMYYYY
    fileName = Regex.Replace(
        fileName,
        $"{dateSep}(?:{year}{dateSep}{month}{dateSep}{day}|{month}{dateSep}{day}{dateSep}{year}|{day}{dateSep}{month}{dateSep}{year})",
        string.Empty,
        RegexOptions.Singleline | RegexOptions.ExplicitCapture);

    // Strip Times: e.g., 3_53_34_AM
    fileName = Regex.Replace(
        fileName,
        $"{dateSep}{hour}{timeSep}{minSec}(?:{timeSep}{minSec})?{amPm}",
        string.Empty,
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.ExplicitCapture);

    // 3. Remove all other special characters, keeping only letters, numbers, and basic connectors
    fileName = fileName.ProcessByCategory(x =>
         x is UnicodeCategory.UppercaseLetter
           or UnicodeCategory.LowercaseLetter
           or UnicodeCategory.DecimalDigitNumber
           or UnicodeCategory.ConnectorPunctuation
           or UnicodeCategory.DashPunctuation
           or UnicodeCategory.OtherPunctuation);

    // 3. UNIVERSAL CLEANUP: Collapse ANY repeating punctuation, symbol, or whitespace
    // Pattern explanation:
    // ([\p{P}\p{S}\s]) : Match any Punctuation, Symbol, or Whitespace and group it
    // \1+              : Match the same character found in group 1, one or more times
    fileName = Regex.Replace(fileName, @"([\p{P}\p{S}\s])\1+", "$1");

    // 4. Final Sanitize: Remove specific problematic chars and trim
    // Now that duplicates are gone, we can safely trim or replace specific leftovers
    return fileName.Replace(".", "_") // Example: convert double dots (now single) to underscore
                   .Trim('_', '-', '.', ' ', '\t');
  }

  /// <summary>
  /// Returns the standard .NET Type associated with a specific <see cref="DataTypeEnum"/>.
  /// </summary>
  /// <param name="dt">The library data type.</param>
  /// <returns>The corresponding .NET Type.</returns>
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
  /// Filters the columns of a DataTable to return only "real" (non-artificial) data columns.
  /// </summary>
  /// <param name="dataTable">The DataTable to filter.</param>
  /// <returns>An enumeration of non-artificial <see cref="DataColumn"/> objects.</returns>
  public static IEnumerable<DataColumn> GetRealColumns(this DataTable dataTable) =>
    dataTable.Columns.Cast<DataColumn>().Where(col => NoArtificialField(col.ColumnName));

  /// <summary>
  /// Generates a case-insensitive hash code for a string identifier.
  /// </summary>
  /// <param name="name">The name to hash.</param>
  public static int IdentifierHash(this string name)
    => StringComparer.InvariantCultureIgnoreCase.GetHashCode(name);

  /// <summary>
  /// Recursively combines inner exception messages into a formatted string.
  /// </summary>
  /// <param name="exception">The root exception.</param>
  /// <param name="maxDepth">Maximum depth to traverse.</param>
  /// <returns>A string containing inner error messages.</returns>
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

      if (sb.Length == 0)
        sb.Append(exception.Message);
      return sb.ToString();
    }
    catch
    {
      return string.Empty;
    }
  }

  /// <summary>
  /// Moves an item from one index to another within an <see cref="IList{T}"/>.
  /// </summary>
  /// <param name="list">The target list.</param>
  /// <param name="oldIndex">The current index of the item.</param>
  /// <param name="newIndex">The target index to move the item to.</param>
  [DebuggerStepThrough]
  public static void Move<T>(this IList<T> list, int oldIndex, int newIndex) where T : notnull
  {
    if (oldIndex == newIndex)
      return;
    T removedItem = list[oldIndex];
    list.RemoveAt(oldIndex);
    list.Insert(newIndex, removedItem);
  }

  /// <summary>
  /// Returns the string representation of a <see cref="RecordDelimiterTypeEnum"/>.
  /// </summary>
  /// <param name="type">The delimiter type.</param>    
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
      _ => string.Empty
    };

  /// <summary>
  /// Checks if a column name is considered "artificial" (metadata generated by the reader).
  /// </summary>
  /// <param name="columnName">The name of the column.</param>    
  public static bool NoArtificialField(this string columnName) =>
    !ReaderConstants.ArtificialFields.Contains(columnName, StringComparer.OrdinalIgnoreCase);

  /// <summary>
  /// Replaces a named placeholder with a replacement string, searching for various delimiters like {x}, #x#, (x).
  /// </summary>
  /// <param name="input">The source text.</param>
  /// <param name="placeholder">The name of the placeholder (without delimiters).</param>
  /// <param name="replacement">The text to inject.</param>
  /// <returns>The modified string.</returns>
  public static string PlaceholderReplace(this string input, string placeholder, string replacement)
  {
    if (string.IsNullOrEmpty(placeholder) || !input.Contains(placeholder, StringComparison.OrdinalIgnoreCase))
      return input;

    // This handles all delimiters + optional formatting found in the string for this specific key
    return m_UnifiedPlaceholderRegEx.Replace(input, match =>
    {
      var key = match.Groups["content"].Value.Trim();
      if (key.Equals(placeholder, StringComparison.OrdinalIgnoreCase))
      {
        var format = match.Groups["format"].Value.Trim();
        // Try to treat replacement if format is present
        if (!string.IsNullOrEmpty(format))
        {
          if (DateTime.TryParse(replacement, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dtCurrentCulture)) return ApplyPlaceholderFormat(dtCurrentCulture, format);
          if (double.TryParse(replacement, NumberStyles.Any, CultureInfo.CurrentCulture, out var dblCurrentCulture)) return ApplyPlaceholderFormat(dblCurrentCulture, format);

          if (DateTime.TryParse(replacement, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dtInvariant)) return ApplyPlaceholderFormat(dtInvariant, format);
          if (double.TryParse(replacement, NumberStyles.Any, CultureInfo.InvariantCulture, out var dblInvariantCulture)) return ApplyPlaceholderFormat(dblInvariantCulture, format);
          return ApplyPlaceholderFormat(replacement, format);
        }
        return replacement;
      }
      return match.Value;
    });
  }

  /// <summary>
  /// Replaces placeholders within a template string using properties from the provided object.
  /// </summary>
  /// <remarks>
  /// This method supports multiple delimiter types (e.g., {Prop}, {{Prop}}, [Prop], #Prop) 
  /// and optional formatting (e.g., {Date:yyyy-MM-dd}). 
  /// <para>
  /// <b>Note:</b> The text inside the placeholder (the "key") must match the name of a public 
  /// property on the source object for a replacement to occur.
  /// </para>
  /// </remarks>
  /// <param name="template">The string containing placeholders (matching property names) to be replaced.</param>
  /// <param name="obj">The source object whose properties will provide the replacement values.</param>
  /// <returns>
  /// A string where all valid placeholders matching property names have been replaced. 
  /// If a placeholder name does not match any property on the object, it is left unchanged.
  /// </returns>
  [DebuggerStepThrough]
  public static string PlaceholderReplaceWithPropertyValues(this string template, object obj)
  {
    // FAST CHECK: If no '{', '[', '(', '#', or '<:' exists, return immediately.
    if (!template.AssumePlaceholderPresent()) return template;

    // Get all available properties in teh object
    var props = obj.GetType().GetProperties().Where(p => p.GetMethod != null).ToList();

    // handle all possible placeholders
    return m_UnifiedPlaceholderRegEx.Replace(template, match =>
    {
      var placeHolderName = match.Groups["content"].Value.Trim();
      var format = match.Groups["format"].Value.Trim();

      var prop = props.FirstOrDefault(p => p.Name.Equals(placeHolderName, StringComparison.OrdinalIgnoreCase));
      if (prop == null) return match.Value; // Keep original if no property found

      return ApplyPlaceholderFormat(prop.GetValue(obj), format);
    });
  }

  /// <summary>
  /// Replaces all occurrences of a pattern with a replacement string, ignoring case.
  /// </summary>
  [DebuggerStepThrough]
  public static string ReplaceCaseInsensitive(this string original, string? pattern, string replacement)
  {
    if (pattern is null || pattern.Length == 0)
      return original;

    if (replacement.Equals(pattern, StringComparison.Ordinal))
      return original;

#if NET8_0_OR_GREATER
    return original.Replace(pattern, replacement, StringComparison.OrdinalIgnoreCase);
#else
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
#endif
  }

  /// <summary>
  /// Efficiently replaces specific characters in a string. If the replacement is <c>\0</c>, the character is removed.
  /// </summary>
  public static string ReplaceDefaults(this string inputValue, in char old1, in char new1, in char old2,
    in char new2)
  {
    if (inputValue.Length == 0)
      return string.Empty;

    var result = new char[inputValue.Length];
    int pos = 0;
    for (int i = 0; i < inputValue.Length; i++)
    {
      if (inputValue[i] == old1)
      {
        if (new1 != char.MinValue)
          result[pos++] = new1;
      }
      else if (inputValue[i] == old2)
      {
        if (new2 != char.MinValue)
          result[pos++] = new2;
      }
      else
        result[pos++] = inputValue[i];
    }

    return new string(result, 0, pos);
  }

  /// <summary>
  /// Replaces occurrences of two different strings within a source string.
  /// </summary>
  public static string ReplaceDefaults(this string inputValue, string old1, string new1, string old2,
    string new2)
  {
    if (inputValue.Length == 0)
      return string.Empty;

    if (old1.Length == 1 && new1.Length == 1 && old2.Length == 1 && new2.Length == 1)
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
  /// Sets the maximum value on a progress reporter if it implements <see cref="IProgressTime"/>.
  /// </summary>
  public static void SetMaximum(this IProgress<ProgressInfo>? progress, long maximum)
  {
    if (!(progress is IProgressTime progressTime)) return;
    progressTime.Maximum = maximum;
  }

  /// <summary>
  /// Retrieves a short descriptive text for an Enum value via the <see cref="ShortDescriptionAttribute"/>.
  /// </summary>
  /// <returns>The short description or the value's name if the attribute is missing.</returns>
  public static string ShortDescription(this Enum value)
  {
    var fieldInfo = value.GetType().GetField(value.ToString());
    if (fieldInfo == null) return value.ToString();

    var attribute = fieldInfo.GetCustomAttribute(typeof(ShortDescriptionAttribute)) as ShortDescriptionAttribute;
    return attribute?.ShortDescription ?? value.ToString();
  }

  /// <summary>
  /// Recursively traverses the exception stack to return the message of the innermost exception.
  /// </summary>
  [DebuggerStepThrough]
  public static string SourceExceptionMessage(this Exception exception)
  {
    var loop = exception;
    while (loop.InnerException != null)
      loop = loop.InnerException;

    return loop.Message;
  }

  /// <summary>
  /// Returns the first character of a string, or <see cref="char.MinValue"/> (\0) if empty.
  /// </summary>
  public static char StringToChar(this string inputString) =>
    string.IsNullOrEmpty(inputString) ? char.MinValue : inputString[0];

  /// <summary>
  /// Converts a ulong to an int, clamping to <see cref="int.MaxValue"/> if out of range.
  /// </summary>
  public static int ToInt(this ulong value) => value > int.MaxValue ? int.MaxValue : Convert.ToInt32(value);

  /// <summary>
  /// Converts a long to an int, clamping to the nearest valid integer boundary.
  /// </summary>
  public static int ToInt(this long value)
  {
    if (value > int.MaxValue)
      return int.MaxValue;
    return value < int.MinValue ? int.MinValue : Convert.ToInt32(value);
  }

  /// <summary>
  /// Converts a decimal to an int, clamping to the nearest valid integer boundary.
  /// </summary>
  public static int ToInt(this decimal value)
  {
    if (value > int.MaxValue)
      return int.MaxValue;
    return value < int.MinValue ? int.MinValue : Convert.ToInt32(value);
  }

  /// <summary>
  /// Converts a double to an int, clamping to the nearest valid integer boundary.
  /// </summary>
  public static int ToInt(this double value)
  {
    if (value > int.MaxValue)
      return int.MaxValue;
    return value < int.MinValue ? int.MinValue : Convert.ToInt32(value);
  }

  /// <summary>
  /// Converts a double to a long, clamping to the nearest valid long boundary.
  /// </summary>
  public static long ToInt64(this double value)
  {
    if (value > long.MaxValue)
      return long.MaxValue;
    if (value < long.MinValue)
      return long.MinValue;

    return value.Equals(double.NaN) ? default : Convert.ToInt64(value);
  }

  /// <summary>
  /// Converts a decimal to a long, clamping to the nearest valid long boundary.
  /// </summary>
  public static long ToInt64(this decimal value)
  {
    if (value > long.MaxValue)
      return long.MaxValue;
    return value < long.MinValue ? long.MinValue : Convert.ToInt64(value);
  }

  /// <summary>
  /// Converts a character to a string, returning an empty string if the character is <see cref="char.MinValue"/>.
  /// </summary>
  public static string ToStringHandle0(this char input) =>
    input == char.MinValue ? string.Empty : input.ToString();

  /// <summary>
  /// Central logic to format a raw value based on a placeholder format string.
  /// </summary>
  private static string ApplyPlaceholderFormat(object? value, string? format)
  {
    if (value == null) return string.Empty;
    if (string.IsNullOrEmpty(format)) return value.ToString() ?? string.Empty;

    if (value is DateTime dt)
    {
      // Sanitize and handle custom week logic globally
#pragma warning disable MA0089 // Optimize string method usage
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      var fixedFormat = format.Replace("YY", "yy").Replace("W", "w");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore MA0089 // Optimize string method usage
      if (fixedFormat.Contains("w", StringComparison.OrdinalIgnoreCase))
      {
        var weekNum = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
#pragma warning disable MA0089 // Optimize string method usage
        fixedFormat = fixedFormat.Replace("ww", weekNum.ToString("D2", CultureInfo.InvariantCulture))
                                 .Replace("w", weekNum.ToString(CultureInfo.InvariantCulture));
#pragma warning restore MA0089 // Optimize string method usage
      }
      return dt.ToString(fixedFormat, CultureInfo.CurrentCulture);
    }

    if (value is IFormattable formattable)
      return formattable.ToString(format, CultureInfo.CurrentCulture);

    return value.ToString() ?? string.Empty;
  }
}