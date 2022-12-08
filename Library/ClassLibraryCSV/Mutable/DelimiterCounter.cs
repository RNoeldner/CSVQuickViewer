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
  public sealed class DelimiterCounter
  {
    // Added INFORMATION SEPARATOR ONE to FOUR
    private const string cDefaultSeparators = "\t,;|¦￤*`\u001F\u001E\u001D\u001C";

    public readonly int NumRows;

    public readonly int[] SeparatorRows;

    public readonly string Separators;

    public readonly int[,] SeparatorsCount;

    public int LastRow;

    public DelimiterCounter(int numRows, IEnumerable<char>? disallowedDelimiter)
    {
      NumRows = numRows;
      var listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator[0];
      if (cDefaultSeparators.IndexOf(listSeparator) == -1)
        Separators = cDefaultSeparators + listSeparator;
      else
        Separators = cDefaultSeparators;
      if (disallowedDelimiter != null)
        foreach (var delim in disallowedDelimiter)
          if (Separators.IndexOf(delim) != -1)
            Separators = Separators.Remove(Separators.IndexOf(delim), 1);
      SeparatorsCount = new int[Separators.Length, NumRows];
      SeparatorRows = new int[Separators.Length];
    }

    public bool CheckChar(char readChar)
    {
      var index = Separators.IndexOf(readChar);
      if (index != -1)
      {
        if (SeparatorsCount[index, LastRow] == 0)
          SeparatorRows[index]++;
        ++SeparatorsCount[index, LastRow];
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