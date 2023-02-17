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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CsvTools
{
  /// <summary>
  ///   Collection of static functions for string
  /// </summary>
  [DebuggerStepThrough]
  public static class StringUtils
  {

    /// <summary>
    ///   Checks whether a column name text ends on the text ID or Ref
    /// </summary>
    /// <param name="columnName">The column name</param>
    /// <returns>
    ///   The number of charters at the end that did match, 0 if it does not end on ID
    /// </returns>
    public static int AssumeIdColumn(in string columnName)
    {
      if (columnName.TrimEnd().Length == 0)
        return 0;

      if (columnName.EndsWith(" Text", StringComparison.OrdinalIgnoreCase))
        return 5;
      if (columnName.EndsWith("Text", StringComparison.Ordinal))
        return 4;

      if (columnName.EndsWith(" Ref", StringComparison.OrdinalIgnoreCase))
        return 4;
      if (columnName.EndsWith("Ref", StringComparison.Ordinal))
        return 3;

      if (columnName.EndsWith(" ID", StringComparison.OrdinalIgnoreCase))
        return 3;
      if ((columnName.EndsWith("ID", StringComparison.Ordinal) || columnName.EndsWith("Id", StringComparison.Ordinal)) &&
          !columnName.EndsWith("GUID", StringComparison.OrdinalIgnoreCase))
        return 2;
      return 0;
    }

    public static int AssumeIDColumn(in ReadOnlySpan<char> columnName)
    {
      if (columnName.TrimEnd().Length == 0)
        return 0;

      if (columnName.EndsWith(" Text".AsSpan(), StringComparison.OrdinalIgnoreCase))
        return 5;
      if (columnName.EndsWith("Text".AsSpan(), StringComparison.Ordinal))
        return 4;

      if (columnName.EndsWith(" Ref".AsSpan(), StringComparison.OrdinalIgnoreCase))
        return 4;
      if (columnName.EndsWith("Ref".AsSpan(), StringComparison.Ordinal))
        return 3;

      if (columnName.EndsWith(" ID".AsSpan(), StringComparison.OrdinalIgnoreCase))
        return 3;
      if ((columnName.EndsWith("ID".AsSpan(), StringComparison.Ordinal) || columnName.EndsWith("Id".AsSpan(), StringComparison.Ordinal)) &&
          !columnName.EndsWith("GUID".AsSpan(), StringComparison.OrdinalIgnoreCase))
        return 2;
      return 0;
    }

    /// <summary>
    ///   Gets the a short representation of the text.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="length">The length.</param>
    /// <returns>The text with the maximum length, in case it has been cut off a … is added</returns>
    public static string GetShortDisplay(in string? text, int length)
    {
      var withoutLineFeed = text?.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').Replace("  ", " ")
        .Replace("  ", " ") ?? string.Empty;
      if (string.IsNullOrWhiteSpace(withoutLineFeed))
        return string.Empty;
      if (length < 1 || withoutLineFeed.Length <= length)
        return withoutLineFeed;
      withoutLineFeed = withoutLineFeed.Substring(0, length - 1);
      var spaceIndex = withoutLineFeed.LastIndexOf(" ", length - 1 - (length / 8), StringComparison.Ordinal);
      if (spaceIndex > 1)
        return withoutLineFeed.Substring(0, spaceIndex) + "…";

      return withoutLineFeed + "…";
    }

    /// <summary>
    ///   All combination of \r \n will be made to a single replacement
    /// </summary>
    /// <param name="text">The Text</param>
    /// <param name="replace">The replacement value default is \n.</param>
    /// <returns>
    ///   The text with every combination of line feed replaced with <see cref="replace" />
    /// </returns>
    public static string HandleCrlfCombinations(this string text, in string replace = "\n")
    {
      // Replace everything Unicode LINE SEPARATOR
      const string placeholderStr = "\u2028";
      const char placeholderChar = '\u2028';
      text = text.Replace("\r\n", placeholderStr);
      text = text.Replace("\n\r", placeholderStr);
      text = text.Replace('\r', placeholderChar);
      text = text.Replace('\n', placeholderChar);
      // now replace this with the desired replace (no matter if string or char)
      return text.Replace(placeholderStr, replace);
    }

    /// <summary>
    ///   Joins the strings
    /// </summary>
    /// <param name="parts">The parts to be joined.</param>
    /// <param name="joinWith">The join with.</param>
    /// <example>JoinParts(new [] {"My","","Test")=&gt; My, Test</example>
    /// <remarks>Any empty string will be ignored.</remarks>
    /// <returns>A string</returns>
    public static string Join(this IEnumerable<string> parts, in string joinWith)
    {
      var sb = new StringBuilder(100);
      foreach (var part in parts)
      {
        if (string.IsNullOrEmpty(part))
          continue;
        if (sb.Length > 0)
          sb.Append(joinWith);
        sb.Append(part);
      }
      return sb.ToString();
    }

    /// <summary>
    ///   Joins the strings
    /// </summary>
    /// <param name="parts">The parts to be joined.</param>
    /// <param name="joinWith">The join with.</param>
    /// <example>JoinParts(new [] {"My","","Test")=&gt; My, Test</example>
    /// <remarks>Any empty string will be ignored.</remarks>
    /// <returns>A string</returns>
    public static string Join(this IEnumerable<string> parts, char joinWith = ',')
    {
      var sb = new StringBuilder();
      foreach (var part in parts)
      {
        if (string.IsNullOrEmpty(part))
          continue;
        if (sb.Length > 0)
          sb.Append(joinWith);
        sb.Append(part);
      }

      return sb.ToString();
    }

    /// <summary>
    ///   Adds a counter to the name until the nae is unique ion the collection
    /// </summary>
    /// <param name="previousColumns">A collection of already used names, these will not be changed</param>
    /// <param name="nameToAdd">The default name</param>
    /// <returns>The unique name</returns>
    public static string MakeUniqueInCollection(this ICollection<string> previousColumns, in string nameToAdd)
    {
      if (nameToAdd is null)
        throw new ArgumentNullException(nameof(nameToAdd));

      if (!previousColumns.Contains(nameToAdd))
        return nameToAdd;

      // The name is present already

      // cut off any trailing numbers
      var nameNoNumber = nameToAdd.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '\t', ' ');

      // add 1 for the first add 2 for the second etc
      var counterAdd = 1;
      string fieldName;
      do
      {
        fieldName = nameNoNumber + counterAdd++;
      } while (previousColumns.Contains(fieldName));

      return fieldName;
    }

    /// <summary>
    ///   Used to get text without control characters
    /// </summary>
    /// <param name="original">The original text.</param>
    /// <returns>The original text without control characters</returns>
    public static string NoControlCharacters(this string original)
    {
      var chars = new char[original.Length];
      var count = 0;
      foreach (var c in from c in original
                        let oc = CharUnicodeInfo.GetUnicodeCategory(c)
                        where UnicodeCategory.Control != oc || c == '\r' || c == '\n'
                        select c)
        chars[count++] = c;

      return new string(chars, 0, count);
    }

    /// <summary>
    ///   Used to get only text representation without umlaut or accents, allowing upper and lower
    ///   case characters and numbers
    /// </summary>
    /// <param name="original">The original text.</param>
    /// <returns>The Text without special characters</returns>
    public static string NoSpecials(this string original) =>
      ProcessByCategory(
        original,
        x => x == UnicodeCategory.LowercaseLetter || x == UnicodeCategory.UppercaseLetter
                                                  || x == UnicodeCategory.DecimalDigitNumber);

    /// <summary>
    ///   Check if a text would match a filter value,
    /// </summary>
    /// <param name="item">The item of a list that should be checked</param>
    /// <param name="filter">
    ///   Filter value, for OR separate words by space for AND separate words by +
    /// </param>
    /// <param name="stringComparison"></param>
    /// <Note>In case the filter is empty there is no filter it will always return true</Note>
    /// <returns>True if text matches</returns>
    public static bool PassesFilter(
      this string? item,
      in string? filter,
      StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
    {
      if (filter is null || filter.Length == 0)
        return true;
      if (item is null || item.Length == 0)
        return false;

      return item.AsSpan().PassesFilter(filter.AsSpan(), stringComparison);
    }

    /// <summary>
    ///   Check if a text would match a filter value,
    /// </summary>
    /// <param name="item">The item of a list that should be checked</param>
    /// <param name="filter">
    ///   Filter value, for OR separate words by space for AND separate words by +
    /// </param>
    /// <param name="stringComparison"></param>
    /// <Note>In case the filter is empty there is no filter it will always return true</Note>
    /// <returns>True if text matches</returns>
    public static bool PassesFilter(
      this ReadOnlySpan<char> item,
      ReadOnlySpan<char> filter,
      StringComparison stringComparison)
    {
      if (filter.IsEmpty)
        return true;
      if (item.IsEmpty)
        return false;

      var slices = filter.GetSlices(new[] { '+', ' ', ',', ';' });
      // Any part that was started with + is required
      var requiredParts = new List<int>();
      for (int i = 0; i < slices.Count; i++)
      {
        if (slices[i].length!=0 && slices[i].start > 0)
        {
          if (filter[slices[i].start-1]=='+')
            requiredParts.Add(i);
        }
      }

      var allRequiredFound = true;
      foreach (var i in requiredParts)
      {
        if (item.IndexOf(filter.Slice(slices[i].start, slices[i].length), stringComparison) == -1)
        {
          allRequiredFound = false;
          break;
        }
      }

      
      if (!allRequiredFound)
        return allRequiredFound;

      bool hasTests = false;
      for (int i = 0; i < slices.Count; i++)
      {
        if (slices[i].length != 0 && !requiredParts.Contains(i))
        {
          hasTests = true;
          if (item.IndexOf(filter.Slice(slices[i].start, slices[i].length), stringComparison) != -1)
            return true;
        }
      }

      return (allRequiredFound && requiredParts.Count > 0) || !hasTests;
    }

    /// <summary>
    ///   Processes the each charter of the string by category, if the test function return false,
    ///   the charter is omitted
    /// </summary>
    /// <param name="original">The original.</param>
    /// <param name="testFunction">The test function called on each individual char</param>
    /// <returns>A test with only allowed characters</returns>
    public static string ProcessByCategory(this string original, Func<UnicodeCategory, bool> testFunction)
    {
      if (string.IsNullOrEmpty(original))
        return string.Empty;
      var normalizedString = original.Normalize(NormalizationForm.FormD);

      var chars = new char[normalizedString.Length];
      var count = 0;
      foreach (var c in from c in normalizedString
                        let oc = CharUnicodeInfo.GetUnicodeCategory(c)
                        where testFunction(oc)
                        select c)
        chars[count++] = c;

      return new string(chars, 0, count);
    }

    /// <summary>
    ///   Checks if the provided text should be treated as NULL
    /// </summary>
    /// <param name="value">A string with the text</param>
    /// <param name="treatAsNull">
    ///   A semicolon separated list of texts that should be treated as NULL
    /// </param>
    /// <returns>True if the text is null, or empty or in the list of provided texts</returns>
    public static bool ShouldBeTreatedAsNull(string? value, in string treatAsNull)
    {
      if (treatAsNull.Length == 0)
        return false;
      return value is null || value.Length == 0 ||
             treatAsNull.Split(StaticCollections.ListDelimiterChars, StringSplitOptions.RemoveEmptyEntries).Any(part => value.Equals(part, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///   Checks if the provided text should be treated as NULL
    /// </summary>
    /// <param name="value">A span with the text</param>
    /// <param name="treatAsNull">
    ///   A semicolon separated list of that should be treated as NULL
    /// </param>
    /// <returns>True if the text is null, or empty or in the list of provided texts</returns>
    public static bool ShouldBeTreatedAsNull(this ReadOnlySpan<char> value, ReadOnlySpan<char> treatAsNull)
    {
      if (treatAsNull.IsEmpty)
        return false;
      if (value.Length == 0)
        return true;
      foreach (var valueTuple in treatAsNull.GetSlices(StaticCollections.ListDelimiterChars))
      {
        if (value.Equals(treatAsNull.Slice(valueTuple.start, valueTuple.length), StringComparison.OrdinalIgnoreCase))
          return true;
      }
      return false;
    }

    /// <summary>
    ///   Splits the given string at the given set of delimiters to be used as replacement for Split"/>
    /// </summary>
    /// <param name="inputValue">The string to be split.</param>
    /// <param name="splitter">The characters to split with</param>
    /// <returns>List with the slit information empty elements are returned</returns>
    public static IReadOnlyList<(int start, int length)> GetSlices(this ReadOnlySpan<char> inputValue, ReadOnlySpan<char> splitter)
    {
      var retList = new List<(int start, int length)>();
      var start = 0;
      var length = inputValue.IndexOfAny(splitter);
      while (length != -1)
      {
        retList.Add((start, length));
        start += length+1;
        length = inputValue.Slice(start).IndexOfAny(splitter);
      }
      if (inputValue.Length != start)
        retList.Add((start, inputValue.Length - start));
      return retList;
    }

    /// <summary>
    ///   Escapes SQL names; does not include the brackets or quotes
    /// </summary>
    /// <param name="contents">The column or table name.</param>
    /// <returns>The names as it can be placed into brackets</returns>
    public static string SqlName(this string? contents) =>
      contents is null || contents.Length == 0 ? string.Empty : contents.Replace("]", "]]");

    /// <summary>
    ///   Handles quotes in SQLs, does not include the outer quotes
    /// </summary>
    /// <param name="contents">The contents.</param>
    public static string SqlQuote(this string? contents) =>
      contents is null || contents.Length == 0 ? string.Empty : contents.Replace("'", "''");

    /// <summary>
    ///   Read the value and determine if this could be a constant value (surrounded by " or ') or
    ///   if its a number; if not its assume is a reference to another field
    /// </summary>
    /// <param name="entry">A text that refers to another column or is possibly a constant</param>
    /// <param name="result">The constant value without the " or '</param>
    /// <returns><c>true</c> if a constant was found</returns>
    public static bool TryGetConstant(this string entry, out string result)
    {
      result = string.Empty;
      if (entry.Length == 0)
        return false;
      if (entry.Length > 2
          && ((entry[0] == '"' && entry[entry.Length-1]=='"')
           || (entry[0] == '\'' && entry[entry.Length-1]=='\'')))
      {
        result = entry.Substring(1, entry.Length - 2);
        return true;
      }

      if (!Regex.IsMatch(entry, @"-?[0-9]+\.?[0-9]*"))
        return false;

      result = entry;
      return true;
    }

    public static bool TryGetConstant(this ReadOnlySpan<char> entry, out ReadOnlySpan<char> result)
    {
      result = entry;
      if (entry.Length == 0)
        return false;

      if (entry.Length > 2
          && ((entry[0] == '"' && entry[entry.Length-1]=='"')
           || (entry[0] == '\'' && entry[entry.Length-1]=='\'')))
      {
        result = entry.Slice(1, entry.Length - 2);
        return true;
      }

#if NET7_0_OR_GREATER
      return Regex.IsMatch(entry, @"-?[0-9]+\.?[0-9]*");
#else
      return Regex.IsMatch(entry.ToString(), @"-?[0-9]+\.?[0-9]*");
#endif
    }

  }
}