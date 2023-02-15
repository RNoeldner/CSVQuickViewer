/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
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
using System.Collections.Generic;
using System.Linq;

namespace CsvTools
{
  /// <summary>
  /// Helper to count the text delimiters in a file.
  /// </summary>
  public sealed class DelimiterCounter
  {

    // Added INFORMATION SEPARATOR ONE to FOUR
    public readonly int NumRows;
    public readonly int[] SeparatorRows;
    public readonly string Separators;
    public readonly int[] SeparatorScore;
    public readonly int[,] SeparatorsCount;
    public int LastRow;
    private readonly char m_FieldQualifier;

    /// <summary>
    ///  Creates an instance of a delimiter counter
    /// </summary>
    /// <param name="numRows">Number of rows to expect</param>
    /// <param name="disallowedDelimiter">You can pass in delimiters that should not be detected, 
    /// if you know that a delimiter is defiantly not suitable.</param>
    /// <param name="fieldQualifier">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
    public DelimiterCounter(int numRows, IEnumerable<char>? disallowedDelimiter, char fieldQualifier)
    {
      NumRows = numRows;
      m_FieldQualifier = fieldQualifier;
      Separators = new string((disallowedDelimiter == null ? StaticCollections.DelimiterChars : StaticCollections.DelimiterChars.Where(x => !disallowedDelimiter.Contains(x))).ToArray());
      SeparatorsCount = new int[Separators.Length, NumRows];
      SeparatorRows = new int[Separators.Length];
      SeparatorScore= new int[Separators.Length];
    }

    public int FilledRows
    {
      get
      {
        while (LastRow > 1 && RowEmpty(LastRow - 1))
          LastRow--;

        var res = 0;
        for (var line = 0; line < LastRow; line++)
          if (!RowEmpty(line))
            res++;
        return res;
      }
    }

    /// <summary>
    /// Main method called with the current char and the last char
    /// </summary>
    /// <param name="read">The character to check</param>
    /// <param name="last">The previous char, this char allows scoring</param>
    /// <returns><c>true</c> if the char was a delimiter</returns>
    public bool CheckChar(char read, char last)
    {
      var index = Separators.IndexOf(read);
      if (index == -1)
        return false;

      if (SeparatorsCount[index, LastRow] == 0)
        SeparatorRows[index]++;

      ++SeparatorsCount[index, LastRow];
      // A separator its worth more if the previous char was the quote
      if (last == m_FieldQualifier)
        SeparatorScore[index] += 2;
      else if (last != read && last!=' ' && last!='\r' && last!='\n')
        // its also worth something if previous char appears to be a text
        SeparatorScore[index]++;

      return true;
    }

    private bool RowEmpty(int line)
    {
      for (var x = 0; x < Separators.Length; x++)
        if (SeparatorsCount[x, line] != 0)
          return false;
      return true;
    }
  }
}