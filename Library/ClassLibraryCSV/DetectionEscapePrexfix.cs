using System;
using System.Collections.Generic;
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
    private static string GetPossibleEscapePrefix() => "\\/?";

    /// <summary>
    ///   Try to guess the used Escape Sequence, by looking at 500 lines 
    /// </summary>
    /// <param name="textReader">The improved text reader.</param>
    /// <param name="fieldDelimiterChar">The delimiter to separate columns</param>
    /// <param name="fieldQualifierChar">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>The Escape Prefix used</returns>    
    public static async Task<char> InspectEscapePrefixAsync(this ImprovedTextReader textReader,
      char fieldDelimiterChar, char fieldQualifierChar, CancellationToken cancellationToken)
    {
      if (textReader is null)
        throw new ArgumentNullException(nameof(textReader));

      // The characters that could be an escape, most likely its a \ 
      var checkedEscapeChars = GetPossibleEscapePrefix().ToCharArray();

      // build a list of all characters that would indicate a sequence
      var possibleEscaped = new HashSet<char>(checkedEscapeChars);

      if (fieldDelimiterChar != char.MinValue)
        possibleEscaped.Add(fieldDelimiterChar);
      if (fieldQualifierChar !=char.MinValue)
        possibleEscaped.Add(fieldQualifierChar);
      foreach (var escaped in DelimiterCounter.GetPossibleDelimiters())
        possibleEscaped.Add(escaped);
      foreach (var escaped in DetectionQualifier.GetPossibleQualifier())
        possibleEscaped.Add(escaped);

      var score =new int[checkedEscapeChars.Length];

      // Start where we are currently but wrap around
      var textReaderPosition = new ImprovedTextReaderPositionStore(textReader);
      for (int current = 0; current< 500 && !textReaderPosition.AllRead() && !cancellationToken.IsCancellationRequested; current++)
      {
        var line = (await textReader.ReadLineAsync().ConfigureAwait(false));
        // in case non of the possible escapes is in the line skip it...
        if (line.IndexOfAny(checkedEscapeChars)==-1)
          continue;
        // otherwise check each escape 
        for (int i = 0; i < checkedEscapeChars.Length; i++)
        {        
          var pos = line.IndexOf(checkedEscapeChars[i]);                    
          while (pos != -1 && pos < line.Length)
          {
            if (possibleEscaped.Contains(line[pos+1]))
              score[i]++;
            else
              score[i]--;
            pos = line.IndexOf(checkedEscapeChars[i], pos+1);
          }
        }        
      }
      
      var bestIndex = new Punctuation('\0');
      var bestScore = 0;
      for (int i = 0; i < checkedEscapeChars.Length; i++)
      { 
        if (bestScore<score[i] && score[i]>0)
        {
          bestIndex.Char = checkedEscapeChars[i];
          bestScore=score[i];
        }
      }
      if (bestScore > 0)
      {        
        Logger.Information("Escape : {comment}", bestIndex.Text);
        return bestIndex.Char;
      }
      Logger.Information("No Escape found");
      return char.MinValue;
    }
  }
}