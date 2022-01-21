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
using System.Globalization;
using System.Text;

namespace CsvTools
{
  public class TextUnescapeFormatter : IColumnFormatter
  {
    public bool RaiseWarning { get; set; } = true;

    private static Tuple<int, int> ParseHex(in string text, int startPos)
    {
      var hex = new StringBuilder();
      var pos = startPos+2;
      // up to 4 byte escape 
      while (pos<startPos + 6 && pos< text.Length)
      {
        if ((text[pos] >= '0' && text[pos] <= '9')
          || (text[pos] >= 'A' && text[pos] <= 'F')
          || (text[pos] >= 'a' && text[pos] <= 'f'))
          hex.Append(text[pos]);
        else
          break;
        pos++;
      }

      if (hex.Length>0)
        // get the hex number         
        if (int.TryParse(hex.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int charValue))
          return new Tuple<int, int>(pos, charValue);

      return new Tuple<int, int>(-1, -1);
    }

    /// <summary>
    ///   Replace c escapted text to varbatim text similar to RegEx Unescape
    /// </summary>
    /// <param name="text">The text possibly containing c escaped text.</param>    
    public static string Unescape(in string text)
    {
      if (text is null) throw new ArgumentNullException(nameof(text));
      if (text.IndexOf('\\') ==-1)
        return text;

      var retValue = text.Replace("\\t", "\t").Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\v", "\v").Replace("\\'", "\'").Replace("\\\"", "\"").Replace("\\a", "\a").Replace("\\b", "\b").Replace("\\f", "\f");

      int posEncoded = retValue.IndexOf("\\u");
      while (posEncoded != -1)
      {
        (var pos, var charValue) = ParseHex(retValue, posEncoded);
        if (pos!=-1)
        {
          retValue = retValue.Replace(retValue.Substring(posEncoded, pos-posEncoded), ((char) charValue).ToString());
          posEncoded -= 2;
        }

        posEncoded = retValue.IndexOf("\\u", posEncoded+2);
      }

      posEncoded = retValue.IndexOf("\\x");
      while (posEncoded != -1)
      {
        (var pos, var charValue) = ParseHex(retValue, posEncoded);
        if (pos!=-1)
        {
          retValue = retValue.Replace(retValue.Substring(posEncoded, pos-posEncoded), ((char) charValue).ToString());
          posEncoded -= 2;
        }

        posEncoded = retValue.IndexOf("\\x", posEncoded+2);
      }

      return retValue;
    }

    public string FormatText(in string inputString, Action<string>? handleWarning)
    {
      var output = Unescape(inputString);
      if (RaiseWarning && !inputString.Equals(output, StringComparison.Ordinal))
        handleWarning?.Invoke($"Unescaped text");
      return output;
    }
  }
}