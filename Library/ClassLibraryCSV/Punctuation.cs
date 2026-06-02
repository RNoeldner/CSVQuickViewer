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
using System;

namespace CsvTools;

/// <summary>
/// Class to represent a character, but it does support recognition and conversion to written punctuation like "Tab"
/// </summary>
public static class Punctuation
{

  /// <summary>
  ///   Gets a descriptive text for a character
  /// </summary>
  public static string Description(this char character)
    => character switch
    {
      '\t' => "Horizontal Tab",
      ' ' => "Space",
      '\u00A0' => "Non-breaking space",
      '\\' => "Backslash \\",
      '/' => "Slash /",
      ',' => "Comma ,",
      ';' => "Semicolon ;",
      ':' => "Colon :",
      '.' => "Dot .",
      '|' => "Pipe |",
      '"' => "Quotation marks \"",
      '\'' => "Apostrophe '",
      '&' => "Ampersand &",
      '*' => "Asterisk *",
      '`' => "Tick Mark `",
      '?' => "Check mark ?",
      '\u001C' => "File Separator Char 28",
      '\u001D' => "Group Separator Char 29",
      '\u001E' => "Record Separator ?",
      '\u001F' => "Unit Separator ?",
      _ => character.ToString()
    };


  /// <summary>
  /// The printable text representation of the character, a tab will be shown as "Tab"
  /// </summary>
  public static string Text(this char character)
    => character switch
    {
      '\0' => string.Empty,
      '\t' => "Tab",
      '\r' => "CR",
      '\n' => "LF",
      ' ' => "Space",
      '\u00A0' => "NBSP",
      '\u001F' => "US",
      '\u001E' => "RS",
      '\u001D' => "GS",
      '\u001C' => "FS",
      _ => character.ToString()
    };

  /// <summary>
  /// Set the text if something did change, return true
  /// </summary>
  /// <param name="character">The storage</param>
  /// <param name="value"></param>
  /// <returns><c>true</c> if value is changed</returns>
  public static bool SetText(this ref char character, string? value)
  {
    if (character.Text().Equals(value, StringComparison.Ordinal))
      return false;
    character = FromText(value);
    return true;
  }

  /// <summary>
  /// Resolves a character from a string representation, supporting both literal characters 
  /// and descriptive keywords (e.g., "Tab", "NBSP", "Comma").
  /// </summary>
  /// <param name="inputString">The read-only character span to resolve.</param>
  /// <returns>
  /// The resolved <see cref="char"/>. Returns <see cref="char.MinValue"/> if the span is empty 
  /// or whitespace; otherwise, returns the first character of the span if no keyword match is found.
  /// </returns>
  /// <remarks>
  /// This method is optimized for .NET 27 high-performance scenarios:
  /// <list type="bullet">
  /// <item>Uses a length-based jump table to minimize string comparisons.</item>
  /// <item>Operates entirely on ReadOnlySpan to ensure zero heap allocations.</item>
  /// <item>Performs case-insensitive ordinal comparisons for descriptive keywords.</item>
  /// </list>
  /// </remarks>
  public static char FromText(this ReadOnlySpan<char> inputString)
  {
    // Do not trim early as a single /t would lost
    if (inputString.Length == 1)
      return inputString[0];

    inputString = inputString.Trim();

    if (inputString.IsEmpty)
      return char.MinValue;

    return inputString.Length switch
    {
      2 => CheckLen2(inputString),
      3 => CheckLen3(inputString),
      4 => CheckLen4(inputString),
      5 => CheckLen5(inputString),
      6 => CheckLen6(inputString),
      9 => CheckLen9(inputString),
      _ => CheckOther(inputString)
    };
  }


  static char CheckLen2(ReadOnlySpan<char> input) => input[0] switch
  {
    '\\' => input[1] switch
    {
      't' or 'T' => '\t',
      'r' or 'R' => '\r',
      'n' or 'N' => '\n',
      's' or 'S' => ' ',
      _ => input[0]
    },
    'C' or 'c' when input[1] is 'R' or 'r' => '\r',
    'L' or 'l' when input[1] is 'F' or 'f' => '\n',
    'U' or 'u' when input[1] is 'S' or 's' => '\u001F',
    'R' or 'r' when input[1] is 'S' or 's' => '\u001E',
    'G' or 'g' when input[1] is 'S' or 's' => '\u001D',
    'F' or 'f' when input[1] is 'S' or 's' => '\u001C',
    'A' or 'a' when input[1] is 'T' or 't' => '@',
    _ => input[0]
  };

  static char CheckLen3(ReadOnlySpan<char> input)
  {
    if (input.Equals("Tab", StringComparison.OrdinalIgnoreCase)) return '\t';
    if (input.Equals("Dot", StringComparison.OrdinalIgnoreCase)) return '.';
    return input[0];
  }

  static char CheckLen4(ReadOnlySpan<char> input)
  {
    if (input.Equals("hash", StringComparison.OrdinalIgnoreCase)) return '#';
    if (input.Equals("Star", StringComparison.OrdinalIgnoreCase)) return '*';
    if (input.Equals("Pipe", StringComparison.OrdinalIgnoreCase)) return '|';
    if (input.Equals("NBSP", StringComparison.OrdinalIgnoreCase)) return '\u00A0';
    if (input.Equals("tick", StringComparison.OrdinalIgnoreCase)) return '\'';
    return input[0];
  }

  static char CheckLen5(ReadOnlySpan<char> input)
  {
    if (input.Equals("Space", StringComparison.OrdinalIgnoreCase)) return ' ';
    if (input.Equals("Comma", StringComparison.OrdinalIgnoreCase)) return ',';
    if (input.Equals("Colon", StringComparison.OrdinalIgnoreCase)) return ':';
    if (input.Equals("Slash", StringComparison.OrdinalIgnoreCase)) return '/';
    if (input.Equals("sharp", StringComparison.OrdinalIgnoreCase)) return '#';
    if (input.Equals("amper", StringComparison.OrdinalIgnoreCase)) return '&';
    if (input.Equals("whirl", StringComparison.OrdinalIgnoreCase)) return '@';
    if (input.Equals("Point", StringComparison.OrdinalIgnoreCase)) return '.';
    if (input.Equals("Quote", StringComparison.OrdinalIgnoreCase)) return '"';
    return input[0];
  }

  static char CheckLen6(ReadOnlySpan<char> input)
  {
    if (input.Equals("monkey", StringComparison.OrdinalIgnoreCase)) return '@';
    if (input.Equals("Stroke", StringComparison.OrdinalIgnoreCase)) return '/';
    return input[0];
  }

  static char CheckLen9(ReadOnlySpan<char> input)
  {
    if (input.Equals("Line Feed", StringComparison.OrdinalIgnoreCase)) return '\n';
    if (input.Equals("Tabulator", StringComparison.OrdinalIgnoreCase)) return '\t';
    if (input.Equals("Full Stop", StringComparison.OrdinalIgnoreCase)) return '.';
    if (input.Equals("Tick Mark", StringComparison.OrdinalIgnoreCase)) return '`';
    if (input.Equals("Semicolon", StringComparison.OrdinalIgnoreCase)) return ';';
    if (input.Equals("backslash", StringComparison.OrdinalIgnoreCase)) return '\\';
    if (input.Equals("Asterisk", StringComparison.OrdinalIgnoreCase)) return '*';
    if (input.Equals("underbar", StringComparison.OrdinalIgnoreCase)) return '_';
    if (input.Equals("ampersand", StringComparison.OrdinalIgnoreCase)) return '&';
    return input[0];
  }

  static char CheckOther(ReadOnlySpan<char> input)
  {
    // 1. Structural/Qualifier Delimiters (Highest Probability)
    if (input.StartsWith("Doublequote", StringComparison.OrdinalIgnoreCase)) return '"';
    if (input.StartsWith("Singlequote", StringComparison.OrdinalIgnoreCase)) return '\'';

    // 2. Line Breaks (Very common in multi-line field configuration)
    if (input.Equals("LineFeed", StringComparison.OrdinalIgnoreCase)) return '\n';
    
    if (input.Equals("CarriageReturn", StringComparison.OrdinalIgnoreCase)) return '\r';
    if (input.Equals("Carriage Return", StringComparison.OrdinalIgnoreCase)) return '\r';

    // 3. Tab & Space Variants (Common alternative delimiters)
    if (input.StartsWith("Horizontal Tab", StringComparison.OrdinalIgnoreCase)) return '\t';
    
    if (input.Equals("NonBreakingSpace", StringComparison.OrdinalIgnoreCase)) return '\u00A0';
    if (input.Equals("Non Breaking Space", StringComparison.OrdinalIgnoreCase)) return '\u00A0';

    // 4. Common Punctuation/Symbols
    if (input.Equals("VerticalBar", StringComparison.OrdinalIgnoreCase)) return '|';
    if (input.Equals("Vertical bar", StringComparison.OrdinalIgnoreCase)) return '|';
    if (input.Equals("underscore", StringComparison.OrdinalIgnoreCase)) return '_';
    if (input.Equals("FullStop", StringComparison.OrdinalIgnoreCase)) return '.';

    if (input.Equals("BrokenBar", StringComparison.OrdinalIgnoreCase)) return '¦';
    if (input.Equals("TickMark", StringComparison.OrdinalIgnoreCase)) return '`';

    // 5. Low-Level Control Characters (Rarely used in modern CSVs)
    if (input.StartsWith("Unit separator", StringComparison.OrdinalIgnoreCase)) return '\u001F';
    if (input.StartsWith("Record separator", StringComparison.OrdinalIgnoreCase)) return '\u001E';
    if (input.StartsWith("Group separator", StringComparison.OrdinalIgnoreCase)) return '\u001D';
    if (input.StartsWith("File separator", StringComparison.OrdinalIgnoreCase)) return '\u001C';

    // 6. Fallback
    return input[0];
  }
}