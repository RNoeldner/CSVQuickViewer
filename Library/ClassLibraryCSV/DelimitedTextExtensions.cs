using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace CsvTools
{
  internal class DelimitedTextExtensions
  {
    /// <summary>
    ///   The carriage return character. Escape code is <c>\r</c>.
    /// </summary>
    private const char cCr = (char) 0x0d;

    /// <summary>
    ///   The line-feed character. Escape code is <c>\n</c>.
    /// </summary>
    private const char cLf = (char) 0x0a;

    private static string ReadNextColumn(in ImprovedTextReader reader, char fieldDelimiter,
      char fieldQualifier, char escapePrefix, out bool eol, Action<int>? actionForChar)
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
            stringBuilder.Append(character);
            stringBuilder.Append(nextChar);
            continue;
          }
        }

        // Finished with reading the column by Delimiter or EOF
        if ((character == fieldDelimiter && !escaped && (postData || !quoted)) || reader.EndOfStream)
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
        }

        append:
        if (escaped && (character == fieldQualifier || character == fieldDelimiter ||
                        character == escapePrefix))
          // remove the already added escape char
          stringBuilder.Length--;

        actionForChar?.Invoke(character);
        // all cases covered, character must be data
        stringBuilder.Append(character);

        escaped = !escaped && character == escapePrefix;
      } // While

      eol = eol || reader.EndOfStream;
      return stringBuilder.ToString();
    }

    public static ICollection<string> DelimitedRecord(in ImprovedTextReader reader, char fieldDelimiter,
      char fieldQualifier, char escapePrefix, string commentLine, Action<int>? actionForChar)
    {
      bool restart;
      var endOfLine = false;
      string columnText;
      do
      {
        restart = false;
        columnText =
          ReadNextColumn(reader, fieldDelimiter, fieldQualifier, escapePrefix, out endOfLine, actionForChar);

        // An empty line does not have a first column
        if (columnText.Length == 0 && endOfLine)
        {
          // Return it as array of empty columns
          return Array.Empty<string>();
        }

        // Skip commented lines
        if (commentLine.Length > 0 && columnText.StartsWith(commentLine, StringComparison.Ordinal))
        {
          // A commented line does start with the comment
          if (endOfLine)
            endOfLine = false;

          // read to the end of the line
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

          restart = true;
        }
      } while (restart);

      var columns = new List<string>();

      while (!endOfLine)
      {
        columns.Add(columnText);
        columnText = ReadNextColumn(reader, fieldDelimiter, fieldQualifier, escapePrefix,
          out endOfLine);
      }

      return columns.ToArray();
    }
  }
}
