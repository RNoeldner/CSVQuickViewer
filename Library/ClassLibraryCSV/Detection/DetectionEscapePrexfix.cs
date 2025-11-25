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
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools;

/// <summary>
/// Static class with methods to detect the correct delimiter char
/// </summary>
public static class DetectionEscapePrefix
{
  /// <summary>
  ///   Try to guess the used Escape Sequence, by looking at 500 lines 
  /// </summary>
  /// <param name="textReader">The improved text reader.</param>
  /// <param name="fieldDelimiterChar">The delimiter to separate columns</param>
  /// <param name="fieldQualifierChar">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
  /// <param name="cancellationToken">Cancellation token to stop a possibly long-running process</param>
  /// <returns>The Escape Prefix used</returns>    
  public static async Task<char> InspectEscapePrefixAsync(this ImprovedTextReader textReader,
    char fieldDelimiterChar, char fieldQualifierChar, CancellationToken cancellationToken)
  {
    if (textReader is null)
      throw new ArgumentNullException(nameof(textReader));

    // The characters that could be an escape, most likely it's a \ 
    var checkedEscapeChars = StaticCollections.EscapePrefixChars;

    // build a list of all characters that would indicate a sequence
    var possibleEscaped = new HashSet<char>(checkedEscapeChars);

    if (fieldDelimiterChar != char.MinValue)
      possibleEscaped.Add(fieldDelimiterChar);
    if (fieldQualifierChar !=char.MinValue)
      possibleEscaped.Add(fieldQualifierChar);
    foreach (var escaped in StaticCollections.DelimiterChars)
      possibleEscaped.Add(escaped);
    foreach (var escaped in StaticCollections.PossibleQualifiers)
      possibleEscaped.Add(escaped);

    var score = new int[checkedEscapeChars.Length];

    // Start where we are currently but wrap around
    var textReaderPosition = new ImprovedTextReaderPositionStore(textReader);
    for (int current = 0; current< 500 && !textReaderPosition.AllRead() && !cancellationToken.IsCancellationRequested; current++)
    {
      var line = (await textReader.ReadLineAsync(cancellationToken).ConfigureAwait(false));
      // in case none of the possible escapes is in the line skip it...
      if (line.IndexOfAny(checkedEscapeChars)==-1)
        continue;
      // otherwise check each escape 
      for (int i = 0; i < checkedEscapeChars.Length; i++)
      {
        var pos = line.IndexOf(checkedEscapeChars[i]);
        while (pos != -1 && pos < line.Length-1)
        {
          if (possibleEscaped.Contains(line[pos+1]))
            // points if being followed by a char that is usually escaped
            score[i]+=2;
          else
            // minus point for not needed escape , is worth less than a proper sequence
            score[i]--;
          // look at next position
          pos = line.IndexOf(checkedEscapeChars[i], pos+1);
        }
      }
    }

    var bestIndex = char.MinValue;
    var bestScore = 0;
    for (int i = 0; i < checkedEscapeChars.Length; i++)
    {
      if (bestScore<score[i] && score[i]>0)
      {
        bestIndex = checkedEscapeChars[i];
        bestScore=score[i];
      }
    }
    return bestIndex;
  }
}