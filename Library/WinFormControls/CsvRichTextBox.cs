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
  using ScintillaNET;
  using System;
  using System.ComponentModel;
  using System.Drawing;
  using System.Windows.Forms;

  /// <summary>
  ///   Rich Text box that will highlight the delimiter and quote
  /// </summary>
  public class CSVRichTextBox : RichTextBox
  {
    private char m_Delimiter = ',';
    private bool m_DisplaySpace = true;
    private char m_Escape = '\\';
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
      try
      {
        Rtf = RtfHelper.RtfFromText(m_Text, m_DisplaySpace, m_Delimiter, m_Quote, m_Escape, true, 24);
      }
      catch (Exception ex)
      {
        FindForm().ShowError(ex);
      }
    }
  }

  public class CSVRichTextBox2 : Scintilla
  {
    private char m_Delimiter = ',';
    private bool m_DisplaySpace = true;
    private char m_Escape = '\\';
    private char m_Quote = '"';

    public CSVRichTextBox2()
    {
      Styles[1].ForeColor = Color.Blue;
      Styles[1].BackColor = Color.Yellow;
      Styles[2].ForeColor = Color.Blue;
      Styles[2].BackColor = Color.Orange;
      Styles[3].ForeColor = Color.Blue;
      Styles[4].BackColor = Color.LightGray;
      Margins[0].Width = 16;
      WhitespaceSize = 2;
      WrapMode = WrapMode.Char;
      WrapVisualFlags = WrapVisualFlags.End;
      UpdateUI += CSVRichTextBox2_UpdateUI;
      ViewWhitespace = WhitespaceMode.VisibleAlways;
      ViewEol = true;
    }

    public bool ShowLineNumber
    {
      get => Margins[0].Width == 32;
      set
      {
        foreach (var margin in Margins)
          margin.Width = 0;
        if (value)
          Margins[0].Width = 32;
        VScrollBar = value;
      }
    }

    public int SkipLines
    {
      get;
      set;
    }

    public bool WordWrap
    {
      get => WrapMode == WrapMode.Char;
      set => WrapMode = value ? WrapMode.Char : WrapMode.None;
    }

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
        ViewWhitespace = m_DisplaySpace ? WhitespaceMode.VisibleAlways : WhitespaceMode.Invisible;
        ViewEol = m_DisplaySpace;
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
      }
    }

    /// <summary>
    ///   Displayed Text
    /// </summary>
    [DefaultValue("")]
    public override string Text
    {
      get => base.Text;
      set
      {
        var newVal = value ?? string.Empty;
        if (Text.Equals(newVal))
          return;
        ReadOnly = false;
        base.Text = newVal;
        ReadOnly = true;
        if (newVal.Length < 64000)
          UpdateStyle(0, 64000);
      }
    }

    private void CSVRichTextBox2_UpdateUI(object sender, UpdateUIEventArgs e)
    {
      if (TextLength > 64000)
        UpdateStyle(Lines[FirstVisibleLine].Position,
          Lines[LinesOnScreen + FirstVisibleLine].Position + Lines[LinesOnScreen + FirstVisibleLine].Text.Length);
    }

    private void UpdateStyle(int startPos, int endPos)
    {
      if (startPos >= endPos)
        return;
      try
      {
        // Skipped lines are grey
        var endSkipPos = (SkipLines > 0) ? Lines[SkipLines].Position : TextLength;
        if (startPos < endSkipPos && SkipLines > 0)
        {
          StartStyling(0);
          SetStyling(endSkipPos, 4);
          if (startPos < endSkipPos)
            startPos = endSkipPos;
        }

        if (endPos > TextLength)
          endPos = TextLength;

        // Highlight delimiter and quotes and linefeed
        char last = '\0';
        for (var pos = startPos; pos < endPos; pos++)
        {
          var current = Text[pos];
          if (current == m_Delimiter && last != m_Escape)
          {
            StartStyling(pos);
            SetStyling(1, 1);
          }
          else if (current == m_Quote && last != m_Escape)
          {
            StartStyling(pos);
            SetStyling(1, 2);
          }
          else if (m_DisplaySpace && (current == '\r' || current == '\n'))
          {
            StartStyling(pos);
            SetStyling(1, 3);
          }

          last = current;
        }
      }
      catch (Exception ex)
      {
        FindForm().ShowError(ex);
      }
    }
  }
}