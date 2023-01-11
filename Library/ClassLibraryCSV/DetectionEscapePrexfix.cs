using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public static class DetectionEscapePrexfix
  {
    /// <summary>
    ///   Try to guess the new used Escape Sequence, by looking at 500 lines 
    /// </summary>
    /// <param name="stream">The improved stream.</param>
    /// <param name="codePageId">The code page identifier.</param>
    /// <param name="skipRows">The skip rows.</param>
    /// <param name="fieldDelimiter">The field delimiter.</param>
    /// <param name="fieldQualifier">The quoting char</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>The NewLine Combination used</returns>
    public static async Task<string> GuessEscapePrexfix(
      this Stream stream,
      int codePageId,
      int skipRows,
      string fieldDelimiter,
      string fieldQualifier,
      CancellationToken cancellationToken)
    {
      using var textReader = new ImprovedTextReader(stream,
        await stream.CodePageResolve(codePageId, cancellationToken).ConfigureAwait(false), skipRows);
      return await GuessEscapePrefixAsync(textReader, fieldDelimiter, fieldQualifier, cancellationToken).ConfigureAwait(false);

    }

    /// <summary>
    ///   Try to guess the used Escape Sequence, by looking at 500 lines 
    /// </summary>
    /// <param name="textReader">The improved text reader.</param>
    /// <param name="fieldDelimiter">The field delimiter.</param>    
    /// <param name="fieldQualifier">The quoting char</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>The Escape Prefix used</returns>    
    public static async Task<string> GuessEscapePrefixAsync(this ImprovedTextReader textReader, string fieldDelimiter, string fieldQualifier,
     CancellationToken cancellationToken)
    {
      if (textReader is null)
        throw new ArgumentNullException(nameof(textReader));

      var dicLookfor = new Dictionary<string, char>();

      // The characters that could be an escpae, most likly its a \ 
      var checkedEscapeChars = new[] { '\\', '/', '?' };

      // build a list of all chacaters that would indicate a sequence
      var possibleEscaped = new HashSet<char>(checkedEscapeChars);
      if (fieldDelimiter.Length>0)
        possibleEscaped.Add(fieldDelimiter.WrittenPunctuationToChar());
      if (fieldQualifier.Length>0)
        possibleEscaped.Add(fieldQualifier.WrittenPunctuationToChar());
      foreach (var escaped in (new[] { '#', '\'', '"', '\r', '\n', '\t', ',', ';', '|' }))
        possibleEscaped.Add(escaped);

      var counter = new Dictionary<char, int>();
      foreach (var escape in checkedEscapeChars)
      {
        counter.Add(escape, 0);
        // Escaped chars are comments, qoutes, linefeeds or delimiters
        foreach (var escaped in possibleEscaped)
          dicLookfor.Add(string.Empty+ escape + escaped, escape);
      }

      // Start where we are currently but wrap around
      var textReaderPosition = new ImprovedTextReaderPositionStore(textReader);
      for (int current = 0; current< 500 && !textReaderPosition.AllRead() && !cancellationToken.IsCancellationRequested; current++)
      {
        var line = (await textReader.ReadLineAsync().ConfigureAwait(false));
        // in case non of the possible escapes is in the line skip it...
        if (line.IndexOfAny(checkedEscapeChars)==-1)
          continue;
        // look closer if its possible an escaped sequence
        foreach (var d in dicLookfor)
        {
          if (line.IndexOf(d.Key, StringComparison.Ordinal)!=-1)
            counter[d.Value]++;
        }
      }

      var check = counter.OrderBy(x => x.Value).First();
      if (check.Value > 0)
      {
        Logger.Information("Escape : {comment}", check.Key.GetDescription());
        return check.Key.ToString();
      }

      Logger.Information("No Escape found");
      return string.Empty;
    }

    /// <summary>
    /// Checks if the set scap
    /// </summary>
    /// <param name="textReader"></param>
    /// <param name="delimiter"></param>
    /// <param name="quote"></param>
    /// <param name="escape"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    //public static bool IsEscapeUsed(
    //  this ImprovedTextReader textReader,
    //  string delimiter,
    //  string quote,
    //  string escape,
    //  CancellationToken cancellationToken)
    //{
    //  var delimiterChar = delimiter.WrittenPunctuationToChar();
    //  var quoteChar = quote.WrittenPunctuationToChar();
    //  var escapeChar = escape.WrittenPunctuationToChar();
    //  var textReaderPosition = new ImprovedTextReaderPositionStore(textReader);
    //  var counter = 0;
    //  while (!textReaderPosition.AllRead() && !cancellationToken.IsCancellationRequested && counter++< 65536)
    //  {
    //    var c = textReader.Read();
    //    if (c == escapeChar)
    //    {
    //      if (!textReader.EndOfStream)
    //      {
    //        c = textReader.Read();
    //        if (c == delimiterChar || c == quoteChar || c == escapeChar ||  c== '\n' ||  c== '\r')
    //          return true;
    //      }
    //    }
    //  }
    //  return false;
    //}
  }
}