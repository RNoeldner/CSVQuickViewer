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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools;

/// <summary>
/// Static call with methods for Header Detection
/// </summary>
public static class DetectionHeader
{
  /// <summary>
  ///   The carriage return character. Escape code is <c>\r</c>.
  /// </summary>
  private const char cCr = (char) 0x0d;

  /// <summary>
  ///   The line-feed character. Escape code is <c>\n</c>.
  /// </summary>
  private const char cLf = (char) 0x0a;

  /// <summary>
  /// Helper method to read columns from the file, taking care of commented Lines
  /// </summary>
  /// <param name="reader">The reader.</param>
  /// <param name="fieldDelimiter">The delimiter to separate columns</param>
  /// <param name="fieldQualifier">>Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
  /// <param name="escapePrefix">The start of an escape sequence to allow delimiter or qualifier in column</param>
  /// <param name="commentLine">The lineComment.</param>
  /// <returns>All columns of the next row</returns>
  public static List<string> DelimitedRecord(in ImprovedTextReader reader, char fieldDelimiter,
    char fieldQualifier, char escapePrefix, string commentLine)
  {
    bool restartLineCommented;
    bool endOfLine;
    string columnText;
    do
    {
      restartLineCommented = false;
      columnText = ReadColumn(reader, fieldDelimiter, fieldQualifier, escapePrefix, out endOfLine).column;

      // An empty line does not have a first column
      if (columnText.Length == 0 && endOfLine)
      {
        // Return it as array of empty columns
        return new List<string>();
      }

      // Skip commented lines
      if (commentLine.Length <= 0 ||
          !columnText.TrimStart().StartsWith(commentLine, StringComparison.Ordinal)) continue;
      restartLineCommented = true;
      // "Eat" the remaining columns of the commented line
      if (endOfLine) continue;
      while (!reader.EndOfStream)
      {
        var character = reader.Read();
        if (character != cCr && character != cLf)
          continue;
        if (!reader.EndOfStream)
        {
          var nextChar = reader.Peek();
          if ((character == cCr && nextChar == cLf) || (character == cLf && nextChar == cCr))
            reader.MoveNext();
        }

        break;
      }
    } while (restartLineCommented);

    var columns = new List<string> { columnText };
    while (!endOfLine)
      columns.Add(ReadColumn(reader, fieldDelimiter, fieldQualifier, escapePrefix, out endOfLine).column);
    return columns;
  }

  /// <summary>
  /// Get the raw header rows(s) from a text file, without any corrections
  /// </summary>
  /// <param name="stream"></param>
  /// <param name="codePageId">The code page identifier. UTF8 is 65001</param>
  /// <param name="skipLines">Number of lines that should be skipped at the beginning of the file</param>
  /// <param name="fieldDelimiterChar">The delimiter to separate columns</param>
  /// <param name="fieldQualifierChar">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
  /// <param name="escapePrefix">The start of an escape sequence to allow delimiter or qualifier in column</param>
  /// <param name="commentLine">The lineComment.</param>
  /// <returns></returns>
  public static string GetRawHeaderLine(this Stream stream, int codePageId, int skipLines,
    char fieldDelimiterChar,
    char fieldQualifierChar, char escapePrefix, string commentLine)
  {
    var sb = new StringBuilder();
    using var imp = new ImprovedTextReader(stream, codePageId, skipLines);
    var eol = false;
    do
    {
      sb.Append(ReadColumn(imp, fieldDelimiterChar, fieldQualifierChar, escapePrefix, out eol).raw);
    } while (!eol);

    if (sb.Length > 0)
      sb.Length--;
    return sb.ToString();
  }

  /// <summary>Guesses the has header from reader.</summary>
  /// <param name="reader">The reader.</param>
  /// <param name="fieldDelimiterChar">The delimiter to separate columns</param>
  /// <param name="fieldQualifierChar">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
  /// <param name="escapePrefixChar">The start of an escape sequence to allow delimiter or qualifier in column</param>
  /// <param name="lineComment">The lineComment.</param>
  /// <param name="cancellationToken">Cancellation token to stop a possibly long-running process</param>
  /// <returns>Explanation why there is no header, if empty the header was found</returns>
  public static async Task<(string message, bool hasHeader)> InspectHasHeaderAsync(this ImprovedTextReader reader,
    char fieldDelimiterChar,
    char fieldQualifierChar,
    char escapePrefixChar,
    string lineComment,
    CancellationToken cancellationToken)
  {
    var headers = DelimitedRecord(reader, fieldDelimiterChar, fieldQualifierChar, escapePrefixChar, lineComment);

    // Get rid of all trailing empty headers
    while (headers.Count >0 && string.IsNullOrEmpty(headers[headers.Count - 1]))
      headers.RemoveAt(headers.Count - 1);

    // get the average field count looking at the header and 12 additional valid lines
    var fieldCount = headers.Count;

    // if there is only one column the header be number of letter and might be followed by a
    // single number
    if (fieldCount < 2)
    {
      var headerLine = await InspectHeaderLineAsync(reader, lineComment).ConfigureAwait(false);
      if (string.IsNullOrEmpty(headerLine))
        return new("Empty Line", false);

      if (headerLine.NoControlCharacters().Length < headerLine.Replace("\t", "").Length)
        return new($"Control Characters in Column {headerLine}", false);

      if (!(headerLine.Length > 2 && Regex.IsMatch(headerLine, @"^[a-zA-Z]+\d?$")))
        return new($"Only one column: {headerLine}", false);
    }
    else
    {
      var counter = 1;
      while (counter++ < 12 && !cancellationToken.IsCancellationRequested && !reader.EndOfStream)
        fieldCount += DelimitedRecord(reader, fieldDelimiterChar, fieldQualifierChar, escapePrefixChar, lineComment).Count;

      var tooLong = headers.Where(header => header.Length > 128).ToList();
      var numEmpty = headers.Count(string.IsNullOrWhiteSpace);
      var notUnique = headers.GroupBy(x => x)
        .Where(x => x.Count() > 1).ToList();
      var numeric = headers.Where(header => Regex.IsMatch(header, @"^[+\-\(]?\d+([\.,]?\d+)?\)?$")).ToList();
      var dates = headers.Where(header => Regex.IsMatch(header, @"^\d{2,4}[\-/.][0123]?\d[\-/.][0123]?\d|[0123]?\d[\-/.][0123]?\d[\-/.]\d{2,4}?$")).ToList();
      var boolHead = headers.Where(header => header.AsSpan().StringToBoolean(ReadOnlySpan<char>.Empty, ReadOnlySpan<char>.Empty).HasValue)
        .ToList();
      var guidHeaders = headers.Where(header => header.AsSpan().StringToGuid().HasValue)
        .ToList();

      // allowed char are letters, digits and a predefined list of punctuation and symbols
      var specials = headers.Where(header =>
        Regex.IsMatch(header, @"[^\w\d\s\\" + Regex.Escape(@"/_*&%$€£¥[]()+-=#'""<>@.!?") + "]")).ToList();

      var msg = new StringBuilder();

      if (boolHead.Count > 0)
      {
        msg.Append("Header(s) ");
        msg.Append(boolHead.Join(", "));
        msg.Append(" boolean");
      }

      if (guidHeaders.Count > 0)
      {
        if (msg.Length > 0)
          msg.Append('\n');
        msg.Append("Header(s) ");
        msg.Append(guidHeaders.Join(", "));


        msg.Append(" Guid");
      }

      if (numeric.Count > 0)
      {
        if (msg.Length > 0)
          msg.Append('\n');
        msg.Append("Header(s) ");
        msg.Append(numeric.Join(", "));

        msg.Append(" numeric");
      }

      if (dates.Count > 0)
      {
        if (msg.Length > 0)
          msg.Append('\n');
        msg.Append("Header(s) ");
        msg.Append(dates.Join(", "));
        msg.Append(" dates");
      }

      if (specials.Count > 0)
      {
        if (msg.Length > 0)
          msg.Append('\n');
        msg.Append("Header(s) ");
        msg.Append(specials.Join(", "));
        msg.Append(" with uncommon characters");
      }

      if (numEmpty > 0)
      {
        if (msg.Length > 0)
          msg.Append('\n');
        msg.Append($"{numEmpty:N0} Header(s) empty");
      }

      if (notUnique.Count > 0)
      {
        if (msg.Length > 0)
          msg.Append('\n');
        msg.Append("Header(s) ");
        msg.Append(notUnique.Select(x => x.Key).Join(", "));
        msg.Append(" duplicate");
      }

      if (tooLong.Count > 0)
      {
        if (msg.Length > 0)
          msg.Append('\n');
        msg.Append("Header(s) ");
        msg.Append(tooLong.Select(x => x.Substring(0, 128) + "…").Join(", "));
        msg.Append(" too long");
      }

      var border = (int) Math.Ceiling(fieldCount / 2.0 / counter) - specials.Count;
      if (border < 3)
        border=3;

      return (msg.ToString(),
        tooLong.Count <= 0  && numeric.Count + dates.Count + boolHead.Count + numEmpty + guidHeaders.Count + specials.Count < border);
    }
    return (string.Empty, true);
  }

  private static async Task<string> InspectHeaderLineAsync(ImprovedTextReader reader, string commentLine)
  {
    reader.ToBeginning();
    var headerLine = string.Empty;
    while (string.IsNullOrEmpty(headerLine) && !reader.EndOfStream)
    {
      headerLine = await reader.ReadLineAsync(CancellationToken.None).ConfigureAwait(false);
      if (!string.IsNullOrEmpty(commentLine) && headerLine.TrimStart().StartsWith(commentLine, StringComparison.Ordinal))
        headerLine = string.Empty;
    }
    return headerLine;
  }
  private static (string column, string raw) ReadColumn(in ImprovedTextReader reader, char fieldDelimiter,
    char fieldQualifier, char escapePrefix, out bool eol)
  {
    eol = false;
    var stringBuilder = new StringBuilder(5);
    var rawData = new StringBuilder(5);
    var quoted = false;
    var preData = true;
    var escaped = false;
    while (!reader.EndOfStream)
    {
      // Read a character
      var character = reader.Read();
      rawData.Append((char) character);
      if (character is cCr or cLf)
      {
        var nextChar = 0;
        if (!reader.EndOfStream)
        {
          nextChar = reader.Peek();
          if ((character != cCr || nextChar != cLf) && (character != cLf || nextChar != cCr))
          {
            nextChar = 0;
          }
          else
          {
            reader.MoveNext();
          }
        }

        if (((character == cCr && nextChar == cLf) || (character == cLf && nextChar == cCr)) && quoted)
        {
          stringBuilder.Append((char) character);
          stringBuilder.Append((char) nextChar);
          continue;
        }
      }

      // Finished with reading the column by Delimiter or EOF
      if (character == fieldDelimiter && !escaped && (!quoted))
        break;

      // Finished with reading the column by Linefeed
      if (character is cCr or cLf && (preData || !quoted))
      {
        eol = true;
        break;
      }

      if (preData)
      {
        // whitespace preceding data
        if (character == ' ' || character == '\t' && fieldDelimiter != '\t')
        {
          stringBuilder.Append(character);
          continue;
        }

        // data is starting
        preData = false;

        // Can not be escaped here
        if (character == fieldQualifier && !escaped)
        {
          stringBuilder.Length = 0;
          // quoted data is starting
          quoted = true;
          continue;
        }

        goto append;
      }

      if (character == fieldQualifier && quoted && !escaped)
      {
        var peekNextChar = reader.Peek();

        // a "" should be regarded as " if the text is quoted
        if (peekNextChar == fieldQualifier)
        {
          // double quotes within quoted string means add a quote
          stringBuilder.Append(fieldQualifier);
          reader.MoveNext();

          // handling for "" that is not only representing a " but also closes the text
          continue;
        }
        quoted = false;
        continue;
      }

      append:
      if (escaped && (character == fieldQualifier || character == fieldDelimiter ||
                      character == escapePrefix))
        // remove the already added escape char
        stringBuilder.Length--;

      // all cases covered, character must be data
      stringBuilder.Append((char) character);

      escaped = !escaped && character == escapePrefix;
    } // While

    eol = eol || reader.EndOfStream;
    return (stringBuilder.ToString(), rawData.ToString());
  }
}