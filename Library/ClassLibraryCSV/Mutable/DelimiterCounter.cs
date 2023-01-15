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
using System.Globalization;

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
    public readonly int[,] SeparatorsCount;
    public readonly int[] SeparatorScore;
    public int LastRow;
    private readonly char m_FieldQualifier;

    /// <summary>
    /// Get the chedked Delimiters, decided to make this non configrable as the detection process is very fast 
    /// </summary>
    /// <param name="disallowedDelimiter">You can passs in delimiters that should not be decteted, 
    /// if you know that a delimiter is definatly not c.</param>
    /// <returns>Tab, Comma, Semicolon, Various Pipes: |¦￤, Star, 
    /// Single Quote, Unicode Information Seperator 1, Unicode Information Seperator 2, Unicode Information Seperator 3, 
    /// Unicode Information Seperator 4 and whatever is defined in the current culture as ListSeparator
    /// </returns>
    public static string GetPossibleDelimiters(IEnumerable<char>? disallowedDelimiter = null)
    {
      const string cDefaultSeparators = "\t,;|¦￤*`\u001F\u001E\u001D\u001C";

      var Separators = cDefaultSeparators;
      var listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator[0];
      if (cDefaultSeparators.IndexOf(listSeparator) == -1)
        Separators += listSeparator;
      if (disallowedDelimiter != null)
        foreach (var delimiter in disallowedDelimiter)
          if (Separators.IndexOf(delimiter) != -1)
            Separators = Separators.Remove(Separators.IndexOf(delimiter), 1);
      return Separators;
    }

    /// <summary>
    ///  Creates an instance of a delimiter counter
    /// </summary>
    /// <param name="numRows">Number of rows to expect</param>
    /// <param name="disallowedDelimiter">You can passs in delimiters that should not be decteted, 
    /// if you know that a delimiter is definatly not c.</param>
    /// <param name="fieldQualifier">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
    public DelimiterCounter(int numRows, IEnumerable<char>? disallowedDelimiter, char fieldQualifier)
    {
      NumRows = numRows;
      m_FieldQualifier = fieldQualifier;
      Separators = GetPossibleDelimiters(disallowedDelimiter);
      SeparatorsCount = new int[Separators.Length, NumRows];
      SeparatorRows = new int[Separators.Length];
      SeparatorScore= new int[Separators.Length];
    }

    /// <summary>
    /// Main method called with the current char and the last char
    /// </summary>
    /// <param name="read">The charater to check</param>
    /// <param name="last">The previous char, this char allows scoring</param>
    /// <returns><c>true</c> if the char was a delimiter</returns>
    public bool CheckChar(char read, char last)
    {
      var index = Separators.IndexOf(read);
      if (index != -1)
      {
        if (SeparatorsCount[index, LastRow] == 0)
          SeparatorRows[index]++;

        ++SeparatorsCount[index, LastRow];
        // A separator its worth more if the previous char was the quote
        if (last == m_FieldQualifier)
          SeparatorScore[index] += 2;
        else if (last != read && last!=' ' && last!='\r'&& last!='\n')
          // its also worth something if previous char appears to be a text
          SeparatorScore[index]++;
        return true;
      }

      return false;
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

    private bool RowEmpty(int line)
    {
      for (var x = 0; x < Separators.Length; x++)
        if (SeparatorsCount[x, line] != 0)
          return false;
      return true;
    }
  }
}