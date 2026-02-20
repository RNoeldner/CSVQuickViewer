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
  public enum PunctuationType
  {
    None,
    Delimiter,
    Date,
    Time,
    Decimal,
    Grouping,
    Qualifier,
    Escape,
    List
  }

  private PunctuationType m_Type;

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

  private char[] m_Common = System.Array.Empty<char>();
  private char m_Character;

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

  public PunctuationTextBox()
  {
    BindingContextChanged += PunctuationTextBox_BindingContextChanged;
    Validating += PunctuationTextBox_Validating;
  }

  private void PunctuationTextBox_BindingContextChanged(object? sender, System.EventArgs e)
  {
    BindingContextChanged -= PunctuationTextBox_BindingContextChanged;
    SuspendLayout();
    AutoCompleteMode = AutoCompleteMode.SuggestAppend;
    AutoCompleteSource = AutoCompleteSource.CustomSource;
    AutoCompleteCustomSource.Clear();
    foreach (var chr in m_Common)
      AutoCompleteCustomSource.Add(chr.Text());
    ResumeLayout();
    BindingContextChanged += PunctuationTextBox_BindingContextChanged;
  }

  private void PunctuationTextBox_Validating(object? sender, CancelEventArgs e)
  {
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