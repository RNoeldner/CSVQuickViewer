using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public static class DetectionLineComment
  {
    /// <summary>Checks if the comment line does make sense, or if its possibly better regarded as header row</summary>
    /// <param name="textReader">The text reader to read the data</param>
    /// <param name="commentLine">The characters for a comment line.</param>
    /// <param name="fieldDelimiterChar">The delimiter.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>true if the comment line seems to ne ok</returns>
    public static async Task<bool> InspectLineCommentIsValidAsync(
      this ImprovedTextReader textReader,
      string commentLine,
      char fieldDelimiterChar,
      CancellationToken cancellationToken)
    {
      // if there is no commentLine it can not be wrong if there is no delimiter it can not be wrong
      if (string.IsNullOrEmpty(commentLine) || fieldDelimiterChar==char.MinValue)
        return true;

      if (textReader is null) throw new ArgumentNullException(nameof(textReader));

      const int maxRows = 100;
      var row = 0;
      var lineCommented = 0;
      var parts = 0;
      var partsComment = -1;
      while (row < maxRows && !textReader.EndOfStream && !cancellationToken.IsCancellationRequested)
      {
        var line = (await textReader.ReadLineAsync().ConfigureAwait(false)).TrimStart();
        if (string.IsNullOrEmpty(line))
          continue;

        if (line.StartsWith(commentLine, StringComparison.Ordinal))
        {
          lineCommented++;
          if (partsComment == -1)
            partsComment = line.Count(x => x == fieldDelimiterChar);
        }
        else
        {
          if (line.IndexOf(fieldDelimiterChar) != -1)
          {
            parts += line.Count(x => x == fieldDelimiterChar);
            row++;
          }
        }
      }

      // if we could not find a commented line exit and assume the comment line is wrong.
      if (lineCommented == 0)
        return false;

      // in case we have 3 or more commented lines assume the comment was ok
      if (lineCommented > 2)
        return true;

      // since we did not properly parse the delimited text accounting for quoting (delimiter in
      // column or newline splitting columns) apply some variance to it
      return partsComment < Math.Round(parts * .9 / row) || partsComment > Math.Round(parts * 1.1 / row);
    }

    /// <summary>Guesses the line comment</summary>
    /// <param name="textReader">The text reader to read the data</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>The determined comment</returns>
    /// <exception cref="System.ArgumentNullException">textReader</exception>
    public static async Task<string> InspectLineCommentAsync(this ImprovedTextReader textReader,
      CancellationToken cancellationToken)
    {
      if (textReader is null)
        throw new ArgumentNullException(nameof(textReader));

      var starts =
        new[] { "<!--", "##", "//", "==", "\\\\", "''", "#", "/", "\\", "'", }.ToDictionary(test => test, _ => 0);

      // Comments are mainly at teh start of a file
      textReader.ToBeginning();
      for (int current = 0; current<50 && !textReader.EndOfStream && !cancellationToken.IsCancellationRequested; current++)
      {
        var line = (await textReader.ReadLineAsync().ConfigureAwait(false)).TrimStart();
        if (line.Length == 0)
          continue;
        foreach (var test in starts.Keys.Where(test => line.StartsWith(test, StringComparison.Ordinal)))
        {
          starts[test]++;
          // do not check further once a line is counted, by having ## before # a line starting with
          // ## will not be counted twice
          break;
        }
      }

      var maxCount = starts.Max(x => x.Value);
      if (maxCount > 0)
      {
        var check = starts.First(x => x.Value == maxCount);
        Logger.Information($"Comment Line: {check.Key}");
        return check.Key;
      }

      Logger.Information("No Comment Line");
      return string.Empty;
    }
  }
}