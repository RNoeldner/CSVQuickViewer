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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using JetBrains.Annotations;

namespace CsvTools
{
  /// <summary>
  ///   Collection of static functions for string
  /// </summary>
  [DebuggerStepThrough]
  public static class StringUtils
  {
    public static readonly char[] Spaces =
    {
      ' ', '\u00A0', '\u1680', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006', '\u2007', '\u2008',
      '\u2009', '\u200A', '\u200B', '\u202F', '\u205F', '\u3000', '\uFEFF'
    };

    /// <summary>
    ///   ; | CR LF Tab
    /// </summary>
    private static readonly char[] m_DelimiterChar = {';', '|', '\r', '\n', '\t'};

    /// <summary>
    ///   ; CR LF
    /// </summary>
    private static readonly char[] m_SplitChar = {';', '\r', '\n'};

    /// <summary>
    ///   Checks whether a column name text ends on the text ID or Ref
    /// </summary>
    /// <param name="columnName">The column name</param>
    /// <returns>The number of charters at the end that did match, 0 if it does not end on ID</returns>
    public static int AssumeIDColumn([CanBeNull] string columnName)
    {
      if (string.IsNullOrWhiteSpace(columnName))
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
      if (columnName.EndsWith("ID", StringComparison.Ordinal) || columnName.EndsWith("Id", StringComparison.Ordinal))
        return 2;

      return 0;
    }

    /// <summary>
    ///   Determines whether this text contains the another text
    /// </summary>
    /// <param name="text">The text to be checked </param>
    /// <param name="toCheck">To text find.</param>
    /// <param name="comp">The comparison.</param>
    /// <returns>
    ///   <c>true</c> if teh text does contains the check; otherwise, <c>false</c>.
    /// </returns>
    public static bool Contains([CanBeNull] this string text, [NotNull] string toCheck, StringComparison comp) =>
      text?.IndexOf(toCheck, comp) >= 0;

    /// <summary>
    ///   Counts the number of occurence of a pattern in a text.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="pattern">The pattern.</param>
    /// <returns></returns>
    public static int CountOccurence([NotNull] this string text, [CanBeNull] string pattern)
    {
      if (string.IsNullOrEmpty(pattern))
        return 0;
      var count = 0;
      var i = 0;
      while ((i = text.IndexOf(pattern, i, StringComparison.OrdinalIgnoreCase)) != -1)
      {
        i += pattern.Length;
        count++;
      }

      return count;
    }

    /// <summary>
    ///   Gets the a short representation of the text.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="length">The length.</param>
    /// <returns>The text with the maximum length, in case it has been cut off a … is added</returns>
    [NotNull]
    public static string GetShortDisplay([CanBeNull] string text, int length)
    {
      var withoutLineFeed = text?.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').Replace("  ", " ")
        .Replace("  ", " ");
      if (string.IsNullOrWhiteSpace(withoutLineFeed))
        return string.Empty;

      if (withoutLineFeed.Length <= length)
        return withoutLineFeed;
      withoutLineFeed = withoutLineFeed.Substring(0, length - 1);
      var spaceIndex = withoutLineFeed.LastIndexOf(" ", length - 1 - length / 8, StringComparison.Ordinal);
      if (spaceIndex > 1)
        return withoutLineFeed.Substring(0, spaceIndex) + "…";

      return withoutLineFeed + "…";
    }

    /// <summary>
    ///   Gets the trimmed value.
    /// </summary>
    /// <param name="val">The value.</param>
    /// <returns>An upper case version without leading or tailing space, if the input is null it returns an empty string.</returns>
    [NotNull]
    public static string GetTrimmedUpperValue([CanBeNull] object val)
    {
      if (val == null || val == DBNull.Value)
        return string.Empty;

      var strVal = val.ToString();
      return string.IsNullOrEmpty(strVal) ? string.Empty : strVal.Trim().ToUpperInvariant();
    }

    /// <summary>
    ///   All combination of \r \n will be made to a single replacement
    /// </summary>
    /// <param name="text">The Text</param>
    /// <param name="replace">The replacement value default is \n.</param>
    /// <returns>
    ///   The text with every combination of line feed replaced with <see cref="replace" />
    /// </returns>
    [NotNull]
    public static string HandleCRLFCombinations([NotNull] string text, [NotNull] string replace = "\n")
    {
      // Replace everything Unicode LINE SEPARATOR
      const string c_PlaceholderStr = "\u2028";
      const char c_PlaceholderChar = '\u2028';
      text = text.Replace("\r\n", c_PlaceholderStr);
      text = text.Replace("\n\r", c_PlaceholderStr);
      text = text.Replace('\r', c_PlaceholderChar);
      text = text.Replace('\n', c_PlaceholderChar);
      // now replace this with the desired replace (no matter if string or char)
      return text.Replace(c_PlaceholderStr, replace);
    }

    /// <summary>
    ///   Joins the strings
    /// </summary>
    /// <param name="parts">The parts to be joined.</param>
    /// <param name="joinWith">The join with.</param>
    /// <example>JoinParts(new [] {"My","","Test")=> My, Test</example>
    /// <remarks>Any empty string will be ignored.</remarks>
    /// <returns>A string</returns>
    public static string Join(this IEnumerable<string> parts, string joinWith = ", ")
    {
      if (parts == null)
        return string.Empty;

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
    ///   Joins the strings
    /// </summary>
    /// <param name="parts">The parts to be joined.</param>
    /// <param name="joinWith">The join with.</param>
    /// <example>JoinParts(new [] {"My","","Test")=> My, Test</example>
    /// <remarks>Any empty string will be ignored.</remarks>
    /// <returns>A string</returns>
    public static string JoinChar(this IEnumerable<string> parts, char joinWith = ',')
    {
      if (parts == null)
        return string.Empty;

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
    /// <param name="previousColumns">
    ///   A collection of already used names, these will not be changed
    /// </param>
    /// <param name="nameToAdd">The default name</param>
    /// <returns>The unique name</returns>
    [NotNull]
    public static string MakeUniqueInCollection([NotNull] ICollection<string> previousColumns,
      [NotNull] string nameToAdd)
    {
      if (nameToAdd is null)
        throw new ArgumentNullException(nameof(nameToAdd));

      if (!previousColumns.Contains(nameToAdd))
        return nameToAdd;

      // The name is present already

      // cut off any trailing numbers
      nameToAdd = nameToAdd.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '\t', ' ');

      // add 1 for the first add 2 for the second etc
      var counterAdd = 1;
      string fieldName;
      do
      {
        fieldName = nameToAdd + counterAdd++;
      } while (previousColumns.Contains(fieldName));

      return fieldName;
    }

    /// <summary>
    ///   Used to get text without control characters
    /// </summary>
    /// <param name="original">The original text.</param>
    /// <returns>The original text without control characters</returns>
    [NotNull]
    public static string NoControlCharacters([NotNull] this string original)
    {
      if (original is null)
        throw new ArgumentNullException(nameof(original));

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
    [NotNull]
    public static string NoSpecials([NotNull] this string original)
    {
      return ProcessByCategory(original,
        x => x == UnicodeCategory.LowercaseLetter || x == UnicodeCategory.UppercaseLetter ||
             x == UnicodeCategory.DecimalDigitNumber);
    }

    /// <summary>
    ///   Used to get only text representation without umlaut or accents, allowing only upper and
    ///   lower case characters no numbers
    /// </summary>
    /// <param name="original">The original text.</param>
    /// <returns>The text but Only Letters</returns>
    [NotNull]
    public static string OnlyText([NotNull] this string original)
    {
      return ProcessByCategory(original,
        x => x == UnicodeCategory.UppercaseLetter || x == UnicodeCategory.LowercaseLetter);
    }

    /// <summary>
    ///   Processes the each charter of the string by category, if the test function return false, the charter is omitted
    /// </summary>
    /// <param name="original">The original.</param>
    /// <param name="testFunction">The test function called on each individual char</param>
    /// <returns>A test with only allowed characters</returns>
    [NotNull]
    public static string ProcessByCategory([NotNull] string original,
      [NotNull] Func<UnicodeCategory, bool> testFunction)
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
    ///   Replace a search text with a replacement repeatedly,
    /// </summary>
    /// <param name="original"></param>
    /// <param name="search"></param>
    /// <param name="replace"></param>
    /// <returns>the text where all occurrences are replaced with the replace value</returns>
    /// <remarks>
    ///   Searching for two spaces and replacing with one space would lead to two spaces with regular replace it will
    ///   end up with one space here
    /// </remarks>
    [CanBeNull]
    [ContractAnnotation("original: null=>null; original:notnull=>notnull")]
    public static string RReplace([CanBeNull] this string original, [CanBeNull] string search, [NotNull] string replace)
    {
      if (string.IsNullOrEmpty(search) || search.Equals(replace, StringComparison.Ordinal) ||
          string.IsNullOrEmpty(original))
        return original;
      var ret = original;
      while (ret.Contains(search))
        ret = ret.Replace(search, replace);

      return ret;
    }

    /// <summary>
    ///   Checks if the provided text should be treated as NULL
    /// </summary>
    /// <param name="value">A string with the text</param>
    /// <param name="treatAsNull">
    ///   A semicolon separated list of texts that should be treated as NULL
    /// </param>
    /// <returns>True if the text is null, or empty or in the list of provided texts</returns>
    public static bool ShouldBeTreatedAsNull([CanBeNull] string value, [CanBeNull] string treatAsNull)
    {
      if (string.IsNullOrEmpty(treatAsNull))
        return false;
      return string.IsNullOrEmpty(value) || SplitByDelimiter(treatAsNull)
        .Any(part => value.Equals(part, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///   Splits the given string at fixed delimiters.
    /// </summary>
    /// <param name="inputValue">The string to be split.</param>
    /// <returns>String array with substrings, empty elements are removed</returns>
    [NotNull]
    [ItemNotNull]
    public static string[] SplitByDelimiter([CanBeNull] string inputValue)
    {
      return string.IsNullOrEmpty(inputValue)
        ? new string[] { }
        : inputValue.Split(m_DelimiterChar, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    ///   Splits the text into distinct texts, always adding the alwaysInclude Value.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="alwaysInclude">This text will always be included in the result.</param>
    /// <returns>A list of distinct value</returns>
    [NotNull]
    public static HashSet<string> SplitDistinct([NotNull] this string inputValue, [CanBeNull] string alwaysInclude)
    {
      var keyColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      if (!string.IsNullOrWhiteSpace(alwaysInclude))
        keyColumns.Add(alwaysInclude);

      foreach (var keyColumn in inputValue.Split(','))
        if (!string.IsNullOrWhiteSpace(keyColumn))
          keyColumns.Add(keyColumn.Trim());

      return keyColumns;
    }

    /// <summary>
    ///   Splits the given string at the semicolon
    /// </summary>
    /// <param name="inputValue">The string to be split.</param>
    /// <returns>String array with substrings, empty elements are removed</returns>
    [NotNull]
    [ItemNotNull]
    public static string[] SplitValidValues([CanBeNull] string inputValue) => string.IsNullOrEmpty(inputValue)
      ? new string[] { }
      : inputValue.Split(m_SplitChar, StringSplitOptions.RemoveEmptyEntries);

    /// <summary>
    ///   Escapes SQL names; does not include the brackets or quotes
    /// </summary>
    /// <param name="contents">The column or table name.</param>
    /// <returns>The names as it can be placed into brackets</returns>
    [NotNull]
    public static string SqlName([CanBeNull] this string contents) =>
      string.IsNullOrEmpty(contents) ? string.Empty : contents.Replace("]", "]]");

    /// <summary>
    ///   SQLs the quote, does not include the outer quotes
    /// </summary>
    /// <param name="contents">The contents.</param>
    /// <returns></returns>
    [NotNull]
    public static string SqlQuote([CanBeNull] this string contents) =>
      string.IsNullOrEmpty(contents) ? string.Empty : contents.Replace("'", "''");

    [NotNull]
    public static SecureString ToSecureString([NotNull] this string text)
    {
      if (text is null)
        throw new ArgumentNullException(nameof(text));
      var securePassword = new SecureString();

      foreach (var c in text)
        securePassword.AppendChar(c);

      securePassword.MakeReadOnly();
      return securePassword;
    }

    /// <summary>
    ///   Check if a text would match a filter value,
    /// </summary>
    /// <param name="item">The item of a list that should be checked</param>
    /// <param name="filter">Filter value, for OR separate words by space for AND separate words by +</param>
    /// <param name="stringComparison"></param>
    /// <Note>In case the filter is empty there is no filter it will always return true</Note>
    /// <returns>True if text matches</returns>
    public static bool PassesFilter([CanBeNull] this string item, [CanBeNull] string filter,
      StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
      if (string.IsNullOrEmpty(filter))
        return true;
      if (string.IsNullOrEmpty(item))
        return false;

      if (filter.IndexOf('+') <= -1)
        return filter.Split(new[] {' ', ',', ';'}, StringSplitOptions.RemoveEmptyEntries)
          .Any(part => item.IndexOf(part, stringComparison) != -1);
      var parts = filter.Split(new[] {'+'}, StringSplitOptions.RemoveEmptyEntries);

      // 1st part
      var all = item.IndexOf(parts[0], stringComparison) > -1;

      // and all other parts
      for (var index = 1; index < parts.Length && all; index++)
        if (item.IndexOf(parts[index], stringComparison) == -1)
          all = false;
      return all;
    }

    [NotNull]
    public static Tuple<string, bool> GetPossiblyConstant([CanBeNull] this string value)
    {
      if (string.IsNullOrEmpty(value))
        return new Tuple<string, bool>(string.Empty, false);
      if (value.Length > 2 && (
        value.StartsWith("\"", StringComparison.Ordinal) &&  value.EndsWith("\"", StringComparison.Ordinal))
        || (value.StartsWith("'", StringComparison.Ordinal) &&  value.EndsWith("'", StringComparison.Ordinal)))
        return new Tuple<string, bool>(value.Substring(1, value.Length - 2), true);
      return new Tuple<string, bool>(value, false);
    }
  }
}