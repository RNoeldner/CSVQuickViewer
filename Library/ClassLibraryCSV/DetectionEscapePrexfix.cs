using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public static class DetectionEscapePrefix
  {
    /// <summary>
    /// \ / and ?
    /// </summary>
    public static string GetPossibleEscapePrefix() => "\\/?";

    /// <summary>
    ///   Try to guess the new used Escape Sequence, by looking at 300 lines 
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="codePageId">The code page identifier.</param>
    /// <param name="skipRows">The number of lines at beginning to disregard</param>
    /// <param name="fieldDelimiter">The delimiter to separate columns</param>
    /// <param name="fieldQualifier">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>The NewLine Combination used</returns>
    public static async Task<string> InspectEscapePrefixAsync(
      this Stream stream,
      int codePageId,
      int skipRows,
      string fieldDelimiter,
      string fieldQualifier,
      CancellationToken cancellationToken)
    {
      using var textReader = new ImprovedTextReader(stream,
        await stream.InspectCodePageAsync(codePageId, cancellationToken).ConfigureAwait(false), skipRows);
      return await InspectEscapePrefixAsync(textReader, fieldDelimiter, fieldQualifier, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///   Try to guess the used Escape Sequence, by looking at 500 lines 
    /// </summary>
    /// <param name="textReader">The improved text reader.</param>
    /// <param name="fieldDelimiter">The delimiter to separate columns</param>
    /// <param name="fieldQualifier">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>The Escape Prefix used</returns>    
    public static async Task<string> InspectEscapePrefixAsync(this ImprovedTextReader textReader, string fieldDelimiter, string fieldQualifier,
     CancellationToken cancellationToken)
    {
      if (textReader is null)
        throw new ArgumentNullException(nameof(textReader));

      var dicLookFor = new List<string>();

      // The characters that could be an escape, most likely its a \ 
      var checkedEscapeChars = GetPossibleEscapePrefix().ToCharArray();

      // build a list of all characters that would indicate a sequence
      var possibleEscaped = new HashSet<char>(checkedEscapeChars);

      if (fieldDelimiter.Length>0)
        possibleEscaped.Add(fieldDelimiter.WrittenPunctuationToChar());
      if (fieldQualifier.Length>0)
        possibleEscaped.Add(fieldQualifier.WrittenPunctuationToChar());
      foreach (var escaped in DelimiterCounter.GetPossibleDelimiters())
        possibleEscaped.Add(escaped);
      foreach (var escaped in DetectionQualifier.GetPossibleQualifier())
        possibleEscaped.Add(escaped);

      var counter = new Dictionary<char, int>();
      foreach (var escape in checkedEscapeChars)
      {
        counter.Add(escape, 0);
        // Escaped chars are comments, quotes, linefeed or delimiters
        dicLookFor.AddRange(possibleEscaped.Select(escaped => string.Empty + escape + escaped));
      }

      // Start where we are currently but wrap around
      var textReaderPosition = new ImprovedTextReaderPositionStore(textReader);
      for (int current = 0; current< 500 && !textReaderPosition.AllRead() && !cancellationToken.IsCancellationRequested; current++)
      {
        var line = (await textReader.ReadLineAsync().ConfigureAwait(false));
        // in case non of the possible escapes is in the line skip it...
        if (line.IndexOfAny(checkedEscapeChars)==-1)
          continue;
        // look closer if its possible an real escaped sequence
        foreach (var d in dicLookFor.Where(d => line.IndexOf(d, StringComparison.Ordinal)!=-1))
          counter[d[0]]++;
      }

      var bestScore = counter.OrderByDescending(x => x.Value).First();
      if (bestScore.Value > 0)
      {
        Logger.Information("Escape : {comment}", bestScore.Key.GetDescription());
        return bestScore.Key.ToString();
      }

      Logger.Information("No Escape found");
      return string.Empty;
    }
  }
}