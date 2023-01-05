using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public static class QualifierDetection
  {
    /// <summary>
    ///   Try to guess the new line sequence
    /// </summary>
    /// <param name="stream">The improved stream.</param>
    /// <param name="codePageId">The code page identifier.</param>
    /// <param name="skipRows">The skip rows.</param>
    /// <param name="fieldDelimiter">The field delimiter.</param>
    /// <param name="escapePrefix"></param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>The NewLine Combination used</returns>
    public static async Task<(string quoting, bool escapedQualifier, bool duplicateQualifier)> GuessQualifier(
      this Stream stream,
      int codePageId,
      int skipRows,
      string fieldDelimiter,
      string escapePrefix,
      CancellationToken cancellationToken)
    {
      using var textReader = new ImprovedTextReader(stream,
        await stream.CodePageResolve(codePageId, cancellationToken).ConfigureAwait(false), skipRows);
      var qualifier = GuessQualifier(textReader, fieldDelimiter, escapePrefix, new[] { '"', '\'' }, cancellationToken);
      return (qualifier.quoting != '\0' ? char.ToString(qualifier.quoting) : string.Empty, qualifier.escapedQualifier, qualifier.duplicateQualifier);
    }

    /// <summary>
    ///   Try to determine quote character, by looking at the file and doing a quick analysis
    /// </summary>
    /// <param name="textReader">The opened TextReader</param>
    /// <param name="delimiter">The char to be used as field delimiter</param>
    /// <param name="escape">Used to escape a delimiter or quoting char</param>
    /// ///
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>The most likely quoting char</returns>
    /// <remarks>
    ///   Any line feed ot carriage return will be regarded as field delimiter, a duplicate quoting will be regarded as
    ///   single quote, an \ escaped quote will be ignored
    /// </remarks>
    public static (char quoting, bool escapedQualifier, bool duplicateQualifier) GuessQualifier(
      ImprovedTextReader textReader,
      string delimiter,
      string escape,
      char[] possibleQuotes,
      CancellationToken cancellationToken)
    {
      if (textReader is null) throw new ArgumentNullException(nameof(textReader));
      var delimiterChar = delimiter.WrittenPunctuationToChar();
      var escapeChar = escape.WrittenPunctuationToChar();
      var counterTotal = new int[possibleQuotes.Length];
      var counterOpen = new int[possibleQuotes.Length];
      var counterClose = new int[possibleQuotes.Length];
      const char placeHolderText = 't';
      var textReaderPosition = new ImprovedTextReaderPositionStore(textReader);
      var filter = new StringBuilder();
      var last = -1;
      var hasEscapedQuotes = false;
      var hasRepeatedQuotes = false;
      // Read simplified text from file
      while (!textReaderPosition.AllRead() && filter.Length < 2000 && !cancellationToken.IsCancellationRequested)
      {
        var c = (char) textReader.Read();
        if (c == escapeChar)
        {
          if (!textReader.EndOfStream)
            hasEscapedQuotes|= possibleQuotes.Contains((char) textReader.Read());
          continue;
        }
        var isPossibleQuote = possibleQuotes.Contains(c);
        if (!isPossibleQuote)
        {
          if (c == '\r' || c == '\n')
            c = delimiterChar;
          else if (c != delimiterChar)
            c = placeHolderText;
        }
        if (last != c || isPossibleQuote)
        {
          hasRepeatedQuotes |= (last == c);
          filter.Append(c);
        }

        last = c;
      }

      // normalize this, line should start and end with delimiter 
      //  t","t","t",t,t,t't,t"t,t -> ,t","t","t",t,t,t't,t"t,t,,
      var line = delimiterChar + filter.ToString().Trim(delimiterChar) + delimiterChar + delimiterChar;
      for (var testIndex = 0; testIndex < possibleQuotes.Length; testIndex++)
        for (var index = 1; index < line.Length - 2; index++)
        {
          if (line[index] != possibleQuotes[testIndex])
            continue;
          counterTotal[testIndex]++;
          if (line[index - 1] == delimiterChar && (line[index + 1] == placeHolderText ||
                                                   (line[index + 1] == possibleQuotes[testIndex] &&
                                                    line[index + 2] != delimiterChar)))
            counterOpen[testIndex]++;
          if (line[index - 1] == placeHolderText && line[index + 1] == delimiterChar)
            counterClose[testIndex]++;
        }

      var max = 0;
      var res = '\0';
      for (var testIndex = 0; testIndex < possibleQuotes.Length; testIndex++)
      {
        if (counterOpen[testIndex] == 0)
          continue;
        // if we could not find a lot of the closing quotes, assume its wrong
        if (counterClose[testIndex] * 1.5 < counterOpen[testIndex])
        {
          Logger.Information("Could not find an matching number of opening and closing quotes for {qualifier}",
            possibleQuotes[testIndex].GetDescription());
          continue;
        }

        if (counterOpen[testIndex] <= max)
          continue;
        max = counterOpen[testIndex];
        res = possibleQuotes[testIndex];
      }

      // if we could not find opening and closing because we has a lot of ,", take the absolute numbers 
      if (max == 0)
      {
        Logger.Information("Using less accurate method to determine quoting");
        for (var testIndex = 0; testIndex < possibleQuotes.Length; testIndex++)
        {
          // need at least 2 
          if (counterTotal[testIndex] < 2 || counterTotal[testIndex] <= max)
            continue;
          max = counterTotal[testIndex];
          res = possibleQuotes[testIndex];
        }
      }

      if (max == 0 && counterTotal[0] == 0)
      {
        // if we have nothing but we did not see a " in the text at all, a quoting char does not hurt...
        res = possibleQuotes[0];
        Logger.Information("Column qualifier not found using: {qualifier}", res.GetDescription());
      }
      else if (max == 0)
      {
        Logger.Information("No Column Qualifier");
      }
      else
      {
        Logger.Information("Column Qualifier: {qualifier}", res.GetDescription());
      }

      return (res, hasEscapedQuotes, hasRepeatedQuotes);
    }
  }
}
