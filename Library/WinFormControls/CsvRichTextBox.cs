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

  
}