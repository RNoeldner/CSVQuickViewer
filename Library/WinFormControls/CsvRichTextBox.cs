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

namespace CsvTools
{
  using System;
  using System.ComponentModel;
  using System.Windows.Forms;

  /// <summary>
  ///   Rich Text box that will highlight the delimiter and quote
  /// </summary>
  public class CSVRichTextBox : RichTextBox
  {
    private char m_Delimiter = ',';
    private bool m_DisplaySpace = true;
    private char m_Escape = '\\';
    private bool m_InQuote;
    private char m_Quote = '"';
    private string m_Text = string.Empty;

    /// <summary>
    ///   Field Delimiter Character Default: ,
    /// </summary>
    [DefaultValue(',')]
    public char Delimiter
    {
      get => m_Delimiter;
      set
      {
        if (m_Delimiter.Equals(value))
          return;
        m_Delimiter = value;
        SetRtfFromText();
      }
    }

    /// <summary>
    ///   Set to true in order to display spaces a with a Dot so they are more visible
    /// </summary>
    [DefaultValue(true)]
    public bool DisplaySpace
    {
      get => m_DisplaySpace;

      set
      {
        if (m_DisplaySpace == value)
          return;
        m_DisplaySpace = value;
        SetRtfFromText();
      }
    }

    /// <summary>
    ///   Field Escape Character for quotes in quotes
    /// </summary>
    [DefaultValue('\\')]
    public char Escape
    {
      get => m_Escape;
      set
      {
        if (m_Escape.Equals(value))
          return;
        m_Escape = value;
        SetRtfFromText();
      }
    }

    /// <summary>
    ///   Field Quoting Character Default: "
    /// </summary>
    [DefaultValue('"')]
    public char Quote
    {
      get => m_Quote;
      set
      {
        if (m_Quote.Equals(value))
          return;
        m_Quote = value;
        SetRtfFromText();
      }
    }

    /// <summary>
    ///   Displayed Text
    /// </summary>
    [DefaultValue("")]
    public override string Text
    {
      get => m_Text;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_Text.Equals(newVal))
          return;
        m_Text = newVal;
        SetRtfFromText();
      }
    }

    private void SetRtfFromText()
    {
      var rtfHelper = new RtfHelper(m_DisplaySpace, 24);
      try
      {
        if (!string.IsNullOrEmpty(m_Text))
        {
          var curChar = '\0';
          m_InQuote = false;

          for (var pos = 0; pos < m_Text.Length; pos++)
          {
            // get the charters and the surroundings
            var lastChar = curChar;
            curChar = m_Text[pos];
            var nextChar = pos < m_Text.Length - 1 ? m_Text[pos + 1] : '\0';

            if (curChar == '\r' || curChar == '\n')
            {
              rtfHelper.AddChar(4, curChar);
              rtfHelper.AddRtf(1, "\\par\n");
              if (curChar == '\r' && nextChar == '\n' || curChar == '\n' && nextChar == '\r')
                pos++;
              continue;
            }

            if (m_DisplaySpace && curChar == ' ')
            {
              rtfHelper.AddRtf(4, "\\bullet");
              continue;
            }

            if (curChar == m_Delimiter && !m_InQuote)
            {
              rtfHelper.AddChar(2, curChar);
              continue;
            }

            if (curChar == m_Quote)
            {
              // Start m_InQuote
              if (!m_InQuote)
              {
                rtfHelper.AddChar(3, curChar);
                m_InQuote = true;
                continue;
              }

              // Stop quote but skip internal Quotes
              if (!(lastChar == m_Escape || nextChar == m_Quote))
              {
                rtfHelper.AddChar(3, curChar);
                m_InQuote = false;
                continue;
              }

              if (nextChar == m_Quote)
              {
                rtfHelper.AddChar(1, curChar);
                rtfHelper.AddChar(1, nextChar);
                pos++;
                continue;
              }
            }

            if (curChar >= 32 && curChar <= 127 || curChar == '\t')
              rtfHelper.AddChar(1, curChar);
            else

              // others need to be passed on with their decimal code
              rtfHelper.AddRtf(1, $"\\u{(int) curChar}?");
          }
        }

        Rtf = rtfHelper.Rtf;
      }
      catch (Exception ex)
      {
        FindForm().ShowError(ex);
      }
    }
  }
}