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
using System.Buffers;
using System.Globalization;

namespace CsvTools;

/// <summary>
/// Formatter to handle c# escaped text like \t or \r
/// </summary>
public class TextUnescapeFormatter : BaseColumnFormatter
{
  /// <summary>
  /// Static instance of the formatter
  /// </summary>
  public static readonly TextUnescapeFormatter Instance = new TextUnescapeFormatter();

  private static (int pos, int charValue) ParseHex(ReadOnlySpan<char> text, int startPos)
  {
    // Skip the indicator (\u or \x)
    int pos = startPos + 2;
    int maxPos = Math.Min(pos + 4, text.Length);
    int currentPos = pos;

    // 1. Identify how many characters are valid hex digits
    while (currentPos < maxPos)
    {
      char c = text[currentPos];
      if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'))
      {
        currentPos++;
      }
      else
      {
        break;
      }
    }

    // 2. If we found digits, parse the slice directly
    if (currentPos > pos)
    {
      var hexSlice = text.Slice(pos, currentPos - pos);

      if (int.TryParse(
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      hexSlice
#else
      hexSlice.ToString()
#endif
      , NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var charValue))
      {
        return (currentPos, charValue);
      }
    }

    return (-1, -1);
  }

  /// <summary>
  ///   Overwrite c escaped text to verbatim text similar to RegEx UnEscape
  /// </summary>
  /// <param name="text">The text possibly containing c escaped text.</param>    
  public static string Unescape(ReadOnlySpan<char> text)
  {
    if (text.IsEmpty) return string.Empty;

    if (text.IndexOf('\\') == -1)
      return text.ToString();

    // 1. Rent a buffer from the shared pool
    // Unescaping usually results in a SHORTER string, so text.Length is a safe max
    char[]? pooledArray = null;
    Span<char> buffer = text.Length <= 256
        ? stackalloc char[text.Length]
        : (pooledArray = ArrayPool<char>.Shared.Rent(text.Length));

    try
    {
      int writePos = 0;
      for (int i = 0; i < text.Length; i++)
      {
        if (text[i] == '\\' && i + 1 < text.Length)
        {
          char next = text[i + 1];
          bool handled = true;
          switch (next)
          {
            case 't': buffer[writePos++] = '\t'; break;
            case 'n': buffer[writePos++] = '\n'; break;
            case 'r': buffer[writePos++] = '\r'; break;
            case 'v': buffer[writePos++] = '\v'; break;
            case '\'': buffer[writePos++] = '\''; break;
            case '"': buffer[writePos++] = '\"'; break;
            case 'a': buffer[writePos++] = '\a'; break;
            case 'b': buffer[writePos++] = '\b'; break;
            case 'f': buffer[writePos++] = '\f'; break;
            case '\\': buffer[writePos++] = '\\'; break;
            case 'u':
            case 'x':
              var (pos, charValue) = ParseHex(text, i);
              if (pos != -1)
              {
                buffer[writePos++] = (char) charValue;
                i = pos - 2; // Jump past hex
              }
              else handled = false;
              break;
            default:
              handled = false;
              break;
          }

          if (handled)
          {
            i++; // Skip the escaped char
            continue;
          }
        }

        // Standard character
        buffer[writePos++] = text[i];
      }

      // 2. Return the sliced result as a string
      return buffer.Slice(0, writePos).ToString();
    }
    finally
    {
      // 3. Crucial: Return the buffer to the pool
      if (pooledArray != null)
        ArrayPool<char>.Shared.Return(pooledArray);
    }
  }

  /// <inheritdoc />
  public override string FormatInputText(ReadOnlySpan<char> inputSpan, Action<string>? handleWarning)
  {
    var output = Unescape(inputSpan);
    if (RaiseWarning && !inputSpan.SequenceEqual(output.AsSpan()))
      handleWarning?.Invoke("Unescaped text");
    return output;
  }
}