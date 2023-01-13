using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
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

    private static string ReadColumn(in ImprovedTextReader reader, char fieldDelimiter,
      char fieldQualifier, char escapePrefix, out bool eol)
    {
      eol = false;
      var stringBuilder = new StringBuilder(5);
      var quoted = false;
      var preData = true;
      var postData = false;
      var escaped = false;
      while (!reader.EndOfStream)
      {
        // Read a character
        var character = reader.Read();

        if (character == cCr || character == cLf)
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

          if (((character == cCr && nextChar == cLf) || (character == cLf && nextChar == cCr)) && quoted && !postData)
          {
            stringBuilder.Append((char) character);
            stringBuilder.Append((char) nextChar);
            continue;
          }
        }

        // Finished with reading the column by Delimiter or EOF
        if (character == fieldDelimiter && !escaped && (postData || !quoted))
          break;

        // Finished with reading the column by Linefeed
        if ((character == cCr || character == cLf) && (preData || postData || !quoted))
        {
          eol = true;
          break;
        }

        // Only check the characters if not past end of data
        if (postData)
          continue;

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
      return stringBuilder.ToString();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static ICollection<string> DelimitedRecord(in ImprovedTextReader reader, char fieldDelimiter,
      char fieldQualifier, char escapePrefix, string commentLine)
    {
      bool restart;
      bool endOfLine;
      string columnText;
      do
      {
        restart = false;
        columnText =
          ReadColumn(reader, fieldDelimiter, fieldQualifier, escapePrefix, out endOfLine);

        // An empty line does not have a first column
        if (columnText.Length == 0 && endOfLine)
        {
          // Return it as array of empty columns
          return Array.Empty<string>();
        }

        // Skip commented lines
        if (commentLine.Length > 0 && columnText.StartsWith(commentLine, StringComparison.Ordinal))
        {
          // read to the end of the line if not already there
          if (!endOfLine)
          {
            while (!reader.EndOfStream)
            {
              var character = reader.Read();
              if (character == cCr || character == cLf)
              {
                if (!reader.EndOfStream)
                {
                  var nextChar = reader.Peek();
                  if ((character == cCr && nextChar == cLf) || (character == cLf && nextChar == cCr))
                    reader.MoveNext();
                }

                break;
              }
            }
          }

          restart = true;
        }
      } while (restart);

      var columns = new List<string> { columnText };
      while (!endOfLine)
      {
        columnText = ReadColumn(reader, fieldDelimiter, fieldQualifier, escapePrefix, out endOfLine);
        columns.Add(columnText);
      }
      return columns.ToArray();
    }

    /// <summary>
    ///   Guesses the has header from stream.
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="codePageId">The code page identifier.</param>
    /// <param name="skipRows">The number of lines at beginning to disregard</param>
    /// <param name="commentLine">The comment line.</param>
    /// <param name="fieldDelimiter">The delimiter to separate columns</param>
    /// <param name="fieldQualifier">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
    /// <param name="escapePrefix">The start of an escape sequence to allow delimiter or qualifier in column</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns></returns>
    public static async Task<string> GuessHasHeader(this Stream stream,
      int codePageId,
      int skipRows,
      string commentLine,
      string fieldDelimiter,
      string fieldQualifier,
      string escapePrefix,
      CancellationToken cancellationToken)
    {
      using var reader = new ImprovedTextReader(stream,
        await stream.CodePageResolve(codePageId, cancellationToken).ConfigureAwait(false), skipRows);

      return await GuessHasHeaderAsync(reader, commentLine, fieldDelimiter.WrittenPunctuationToChar(), fieldQualifier.WrittenPunctuationToChar(), escapePrefix.WrittenPunctuationToChar(),
        cancellationToken).ConfigureAwait(false);
    }

    private static async Task<string> GetHeaderLine(ImprovedTextReader reader, string commentLine)
    {
      reader.ToBeginning();
      var headerLine = string.Empty;
      while (string.IsNullOrEmpty(headerLine) && !reader.EndOfStream)
      {
        headerLine = await reader.ReadLineAsync().ConfigureAwait(false);
        if (!string.IsNullOrEmpty(commentLine) && headerLine.TrimStart().StartsWith(commentLine, StringComparison.Ordinal))
          headerLine = string.Empty;
      }
      return headerLine;
    }
    /// <summary>Guesses the has header from reader.</summary>
    /// <param name="reader">The reader.</param>
    /// <param name="comment">The comment.</param>
    /// <param name="fieldDelimiter">The delimiter to separate columns</param>
    /// <param name="fieldQualifier">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
    /// <param name="escapePrefix">The start of an escape sequence to allow delimiter or qualifier in column</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>Explanation why there is no header, if empty the header was found</returns>
    public static async Task<string> GuessHasHeaderAsync(this ImprovedTextReader reader,
      string comment,
      char fieldDelimiter,
      char fieldQualifier,
      char escapePrefix,
      CancellationToken cancellationToken)
    {
      var headers = DelimitedRecord(reader, fieldDelimiter, fieldQualifier, escapePrefix, comment);

      // get the average field count looking at the header and 12 additional valid lines
      var fieldCount = headers.Count;

      // if there is only one column the header be number of letter and might be followed by a
      // single number
      if (fieldCount < 2)
      {
        var headerLine = await GetHeaderLine(reader, comment).ConfigureAwait(false);
        if (string.IsNullOrEmpty(headerLine))
          return "Empty Line";

        if (headerLine.NoControlCharacters().Length < headerLine.Replace("\t", "").Length)
          return $"Control Characters in Column {headerLine}";

        if (!(headerLine.Length > 2 && Regex.IsMatch(headerLine, @"^[a-zA-Z]+\d?$")))
          return $"Only one column: {headerLine}";
      }
      else
      {
        var counter = 1;
        while (counter++ < 12 && !cancellationToken.IsCancellationRequested && !reader.EndOfStream)
        {
          fieldCount += DelimitedRecord(reader, fieldDelimiter, fieldQualifier, escapePrefix, comment).Count;
        }

        var halfTheColumns = (int) Math.Ceiling(fieldCount / 2.0);
        if (counter > 3)
        {
          var avgFieldCount = fieldCount / (double) counter;
          // The average should not be smaller than the columns in the initial row
          if (avgFieldCount < headers.Count)
            avgFieldCount = headers.Count;
          halfTheColumns = (int) Math.Ceiling(avgFieldCount / 2.0);

          // Columns are only one or two char, does not look descriptive
          if (headers.Count(x => x.Length < 3) > halfTheColumns)
            return $"Headers '{string.Join("', '", headers.Where(x => x.Length < 3))}' very short";

          // use the same routine that is used in readers to determine the names of the columns
          var (_, numIssues) = BaseFileReader.AdjustColumnName(headers, (int) avgFieldCount, null);

          // looking at the warnings raised
          if (numIssues >= halfTheColumns || numIssues > 2)
            return $"{numIssues} header where empty, duplicate or too long";
        }

        var numeric = headers.Where(header => Regex.IsMatch(header, @"^\d+$")).ToList();
        var boolHead = headers.Where(header => StringConversion.StringToBooleanStrict(header, "1", "0") != null)
          .ToList();
        // allowed char are letters, digits and a predefined list of punctuation and symbols
        var specials = headers.Where(header =>
          Regex.IsMatch(header, @"[^\w\d\s\\" + Regex.Escape(@"/_*&%$[]()+-=#'<>@.!?") + "]")).ToList();
        if (numeric.Count + boolHead.Count + specials.Count >= halfTheColumns)
        {
          var msg = new StringBuilder();
          if (numeric.Count > 0)
          {
            msg.Append("Headers ");
            foreach (var header in numeric)
            {
              msg.Append("'");
              msg.Append(header.Trim('\"'));
              msg.Append("',");
            }

            msg.Length--;
            msg.Append(" numeric");
          }

          if (boolHead.Count > 0)
          {
            if (msg.Length > 0)
              msg.Append(" and ");
            msg.Append("Headers ");
            foreach (var header in boolHead)
            {
              msg.Append("'");
              msg.Append(header.Trim('\"'));
              msg.Append("',");
            }

            msg.Length--;
            msg.Append(" boolean");
          }

          if (specials.Count > 0)
          {
            if (msg.Length > 0)
              msg.Append(" and ");
            msg.Append("Headers ");
            foreach (var header in specials)
            {
              msg.Append("'");
              msg.Append(header.Trim('\"'));
              msg.Append("',");
            }

            msg.Length--;
            msg.Append(" with uncommon characters");
          }

          return msg.ToString();
        }
      }

      return string.Empty;
    }
  }
}