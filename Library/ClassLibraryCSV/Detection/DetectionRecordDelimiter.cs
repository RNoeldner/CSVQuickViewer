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
using System;
using System.Linq;
using System.Threading;

namespace CsvTools
{
  /// <summary>
  /// Static class with methods for RecordDelimiter Detection
  /// </summary>
  public static class DetectionRecordDelimiter
  {
    /// <summary>
    ///   Determine the new line sequence from TextReader
    /// </summary>
    /// <param name="textReader">The reader to read data from</param>
    /// <param name="fieldQualifierChar">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>The NewLine Combination used</returns>
    /// <returns>The NewLine Combination used</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static RecordDelimiterTypeEnum InspectRecordDelimiter(
          this ImprovedTextReader textReader, char fieldQualifierChar, CancellationToken cancellationToken)
    {
      if (textReader is null) throw new ArgumentNullException(nameof(textReader));
      const int numChars = 8192;

      var currentChar = 0;
      var quoted = false;

      const int cr = 0;
      const int lf = 1;
      const int crLf = 2;
      const int lfCr = 3;
      const int recSep = 4;
      const int unitSep = 5;
      const int nl = 5;

      int[] count = { 0, 0, 0, 0, 0, 0, 0 };

      // \r = CR (Carriage Return) \n = LF (Line Feed)
      var textReaderPosition = new ImprovedTextReaderPositionStore(textReader);
      while (currentChar < numChars && !textReaderPosition.AllRead() && !cancellationToken.IsCancellationRequested)
      {
        var readChar = textReader.Read();
        if (readChar == fieldQualifierChar)
        {
          if (quoted)
          {
            if (textReader.Peek() != fieldQualifierChar)
              quoted = false;
            else
              textReader.MoveNext();
          }
          else
          {
            quoted = true;
          }
        }

        if (quoted)
          continue;

        switch (readChar)
        {
          case 21:
            count[nl]++;
            continue;
          case 30:
            count[recSep]++;
            continue;
          case 31:
            count[unitSep]++;
            continue;
          case 10:
          {
            if (textReader.Peek() == 13)
            {
              textReader.MoveNext();
              count[lfCr]++;
            }
            else
            {
              count[lf]++;
            }

            currentChar++;
            break;
          }
          case 13:
          {
            if (textReader.Peek() == 10)
            {
              textReader.MoveNext();
              count[crLf]++;
            }
            else
            {
              count[cr]++;
            }

            break;
          }
        }

        currentChar++;
      }

      var maxCount = count.Max();
      if (maxCount == 0)
        return RecordDelimiterTypeEnum.None;

      var res = RecordDelimiterTypeEnum.None;
      if (count[nl] == maxCount)
        res = RecordDelimiterTypeEnum.Nl;
      else if (count[recSep] == maxCount)
        res = RecordDelimiterTypeEnum.Rs;
      else if (count[unitSep] == maxCount)
        res = RecordDelimiterTypeEnum.Us;
      else if (count[cr] == maxCount)
        res = RecordDelimiterTypeEnum.Cr;
      else if (count[lf] == maxCount)
        res = RecordDelimiterTypeEnum.Lf;
      else if (count[crLf] == maxCount)
        res = RecordDelimiterTypeEnum.Crlf;
      else if (count[lfCr] == maxCount)
        res = RecordDelimiterTypeEnum.Lfcr;
      Logger.Information($"Record Delimiter: {res.Description()}");
      return res;
    }
  }
}