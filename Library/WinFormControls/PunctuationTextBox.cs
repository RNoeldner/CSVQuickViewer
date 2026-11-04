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
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace CsvTools;

[DefaultBindingProperty(nameof(Character))]
public sealed class PunctuationTextBox : TextBox
{
  private char m_Character;
  private char[] m_Common = System.Array.Empty<char>();
  private bool m_isDisposing;
  private bool m_isFormClosing;
  private PunctuationType m_Type;

  public PunctuationTextBox()
  {
    Validating += PunctuationTextBox_Validating;
    SuspendLayout();
    AutoCompleteMode = AutoCompleteMode.SuggestAppend;
    AutoCompleteSource = AutoCompleteSource.CustomSource;
    AutoCompleteCustomSource.Clear();
    foreach (var chr in m_Common)
      AutoCompleteCustomSource.Add(chr.Text());
    ResumeLayout();
  }

  /// <summary>
  /// Specifies the category of punctuation character allowed in the input field.
  /// Used to populate autocomplete options and enforce character validation rules.
  /// </summary>
  public enum PunctuationType
  {
    /// <summary>
    /// No character constraints or validation enforced.
    /// </summary>
    None,

    /// <summary>
    /// Characters used to separate columns in CSV data (e.g., comma, tab, semicolon).
    /// </summary>
    Delimiter,

    /// <summary>
    /// Characters used to separate date components (e.g., slash, hyphen, period).
    /// </summary>
    Date,

    /// <summary>
    /// Characters used to separate time components (e.g., colon, period).
    /// </summary>
    Time,

    /// <summary>
    /// Characters used to separate integer and fractional parts of a number (e.g., period, comma).
    /// </summary>
    Decimal,

    /// <summary>
    /// Characters used for digit grouping/thousands separation (e.g., comma, space, apostrophe).
    /// </summary>
    Grouping,

    /// <summary>
    /// Characters used to quote or qualify field strings containing delimiters (e.g., single or double quotes).
    /// </summary>
    Qualifier,

    /// <summary>
    /// Characters used to escape control sequences or literal qualifiers within data fields (e.g., backslash).
    /// </summary>
    Escape,

    /// <summary>
    /// Characters used to separate list elements within a single CSV field value.
    /// </summary>
    List
  }

  [Browsable(true)]
  [Bindable(true)]
  [DefaultValue(char.MinValue)]
  public char Character
  {
    get => m_Character;
    set
    {
      if (m_Character != value)
      {
        m_Character = value;
        Text = m_Character.Text();
      }
    }
  }

  [Browsable(true)]
  [Bindable(true)]
  [DefaultValue(PunctuationType.None)]
  public PunctuationType Type
  {
    get => m_Type;
    set
    {
      if (m_Type != value)
      {
        m_Type = value;
        if (m_Type == PunctuationType.Delimiter)
          m_Common = StaticCollections.DelimiterChars;
        else if (m_Type == PunctuationType.Qualifier)
          m_Common = StaticCollections.PossibleQualifiers;
        else if (m_Type == PunctuationType.Escape)
          m_Common = StaticCollections.EscapePrefixChars;
        else if (m_Type == PunctuationType.List)
          m_Common = StaticCollections.ListDelimiterChars;
        else if (m_Type == PunctuationType.Date)
          m_Common = StaticCollections.DateSeparatorChars;
        else if (m_Type == PunctuationType.Time)
          m_Common = StaticCollections.TimeSeparators;
        else if (m_Type == PunctuationType.Decimal)
          m_Common = StaticCollections.DecimalSeparatorChars;
        else if (m_Type == PunctuationType.Grouping)
          m_Common = StaticCollections.DecimalGroupingChars;
      }
    }
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      m_isDisposing = true;
    }
    base.Dispose(disposing);
  }

  protected override void OnParentChanged(System.EventArgs e)
  {
    base.OnParentChanged(e);
    if (Parent != null)
    {
      Form? form = FindForm();
      if (form != null)
      {
        form.FormClosing -= Form_FormClosing;
        form.FormClosing += Form_FormClosing;
      }
    }
  }

  private void Form_FormClosing(object? sender, FormClosingEventArgs e)
  {
    m_isFormClosing = true;
  }

  private void PunctuationTextBox_Validating(object? sender, CancelEventArgs e)
  {
    // Skip validation if the control is disposing, disposed, or the form is closing
    if (m_isDisposing || m_isFormClosing || IsDisposed)
    {
      return;
    }
    Character = Text.FromText();
    if (m_Common.Length > 0 && !m_Common.Contains(Character) && Character != char.MinValue)
    {
      e.Cancel = true;
      MessageBox.Show(
        $"{Text} is not supported for {m_Type}\n\nChoose one of:\n{m_Common.Select(x => x.Text()).Join(", ")}",
        $"Invalid {m_Type}", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, 1);
    }
    else
    {
      Text = Character.Text();
    }
  }
}