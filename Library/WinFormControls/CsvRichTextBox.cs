/*
 * Copyright (C) 2014 Raphael Nöldner : http://CSVReshaper.com
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

using System.Text;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   Rich Text box that will highlight the delimiter and quote
  /// </summary>
  public class CSVRichTextBox : RichTextBox
  {
    private int m_CurrentColor = -1;
    private char m_Delimiter = ',';
    private bool m_DisplaySpace = true;
    private char m_Escape = '\\';
    private bool m_InQuote;
    private char m_Quote = '"';
    private string m_Text = string.Empty;

    /// <summary>
    ///   Field Delimiter Character
    /// </summary>
    public char Delimiter
    {
      set
      {
        if (m_Delimiter.Equals(value)) return;
        m_Delimiter = value;
        Rtf = GetRtfFromText(m_Text);
      }

      get => m_Delimiter;
    }

    /// <summary>
    ///   Set to true in order to display spaces a with a Dot so they are more visible
    /// </summary>
    public bool DisplaySpace
    {
      get => m_DisplaySpace;

      set
      {
        m_DisplaySpace = value;
        Rtf = GetRtfFromText(m_Text);
      }
    }

    /// <summary>
    ///   Field Escape Character for quotes in quotes
    /// </summary>
    public char Escape
    {
      set
      {
        if (m_Escape.Equals(value)) return;
        m_Escape = value;
        Rtf = GetRtfFromText(m_Text);
      }

      get => m_Escape;
    }

    /// <summary>
    ///   Field Quoting Character
    /// </summary>
    public char Quote
    {
      set
      {
        if (m_Quote.Equals(value)) return;
        m_Quote = value;
        Rtf = GetRtfFromText(m_Text);
      }

      get => m_Quote;
    }

    /// <summary>
    ///   Displayed Text
    /// </summary>
    public override string Text
    {
      get => m_Text;

      set
      {
        m_Text = value;
        Rtf = GetRtfFromText(m_Text);
      }
    }

    private void AddChar(StringBuilder rtf, int color, char character)
    {
      AddColor(rtf, color, character);
      if (character == '\t')
      {
        rtf.Append("»");
      }
      else if ((character == '\r' || character == '\n') && m_DisplaySpace)
      {
        rtf.Append("¶");
      }
      else
      {
        if (character == '\\' || character == '{' || character == '}')
          rtf.Append('\\');
        rtf.Append(character);
      }
    }

    private void AddColor(StringBuilder rtf, int color, char nextChar)
    {
      if (color == m_CurrentColor) return;
      rtf.AppendFormat(nextChar == '\\' ? "\\cf{0}" : "\\cf{0} ", color);
      m_CurrentColor = color;
    }

    private void AddText(StringBuilder rtf, int color, string text)
    {
      if (string.IsNullOrEmpty(text))
        return;
      AddColor(rtf, color, text[0]);
      rtf.Append(text);
    }

    private string GetRtfFromText(string inputString)
    {
      var rtf = new StringBuilder(@"{\rtf1\ansi\ansicpg1252\deff0\deflang1033");
      rtf.AppendLine(@"{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}");
      rtf.AppendLine(
        @"{\colortbl ;\red0\green0\blue0;\red255\green0\blue0;\red0\green0\blue255;\red255\green168\blue0;}");
      rtf.AppendLine(@"\viewkind4\uc1\pard\f0\fs24 ");
      try
      {
        if (!string.IsNullOrEmpty(inputString))
        {
          m_CurrentColor = -1;
          var curChar = '\0';
          m_InQuote = false;

          for (var pos = 0; pos < inputString.Length; pos++)
          {
            // get the charters and the surroundings
            var lastChar = curChar;
            curChar = inputString[pos];
            var nextChar = pos < inputString.Length - 1 ? inputString[pos + 1] : '\0';

            if (curChar == '\r' || curChar == '\n')
            {
              AddChar(rtf, 4, curChar);
              AddText(rtf, 1, "\\par\n");
              if (curChar == '\r' && nextChar == '\n' || curChar == '\n' && nextChar == '\r')
                pos++;
              continue;
            }

            if (m_DisplaySpace && curChar == ' ')
            {
              AddText(rtf, 4, "\\bullet");
              continue;
            }

            if (curChar == m_Delimiter && !m_InQuote)
            {
              AddChar(rtf, 2, curChar);
              continue;
            }

            if (curChar == m_Quote)
            {
              // Start m_InQuote
              if (!m_InQuote)
              {
                AddChar(rtf, 3, curChar);
                m_InQuote = true;
                continue;
              }

              // Stop quote but skip internal Quotes
              if (!(lastChar == m_Escape || nextChar == m_Quote))
              {
                AddChar(rtf, 3, curChar);
                m_InQuote = false;
                continue;
              }

              if (nextChar == m_Quote)
              {
                AddChar(rtf, 1, curChar);
                AddChar(rtf, 1, nextChar);
                pos++;
                continue;
              }
            }

            if (curChar >= 32 && curChar <= 127 || curChar == '\t')
              AddChar(rtf, 1, curChar);
            else
              // others need to be passed on with their decimal code
              AddText(rtf, 1, $"\\u{(int)curChar}?");
          }
        }
      }
      catch (System.Exception ex)
      {
        FindForm().ShowError(ex);
      }
      rtf.AppendLine(@"\par");
      rtf.AppendLine("}");
      return rtf.ToString();
    }
  }
}