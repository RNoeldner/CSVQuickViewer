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
#nullable enable
using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CsvTools;

public partial class FindSkipRows : ResizeForm, INotifyPropertyChanged
{
  private string m_CommentLine;
  private char m_EscapePrefixChar;
  private char m_FieldDelimiterChar;
  private char m_FieldQualifierChar;
  private readonly string m_FileName;
  private readonly int m_CodePageId;
  private SyntaxHighlighterDelimitedText? m_HighLighter;
  private int m_SkipRows;

  public FindSkipRows() : this(string.Empty, 65001, 0, ',', '\0', '"', "##")
  {
  }

  private void AddCharBinding(TextBox textBoxDelimiter, string propertyName)
  {
    textBoxDelimiter.DataBindings.Add("Text", this, propertyName, true, DataSourceUpdateMode.OnPropertyChanged);
    textBoxDelimiter.DataBindings[0].Format += (s, e) =>
    {
      if (e.Value is char c)
        e.Value = c.ToString();
    };

    textBoxDelimiter.DataBindings[0].Parse += (s, e) =>
    {
      if (e.Value is string str && str.Length > 0)
        e.Value = str[0];
    };
  }

  public FindSkipRows(string fileName, int codePageId = 65001, int skipRows = 0, char fieldDelimiterChar = ',', char escapePrefixChar = '\0', char fieldQualifierChar = '"', string commentLine = "##")
  {
    m_FileName = fileName;
    m_CodePageId = codePageId;
    m_SkipRows =skipRows;
    m_FieldDelimiterChar=fieldDelimiterChar;
    m_EscapePrefixChar=escapePrefixChar;
    m_FieldQualifierChar=fieldQualifierChar;
    m_CommentLine=commentLine ?? string.Empty;
    InitializeComponent();

    AddCharBinding(charBoxDelimiter, nameof(FieldDelimiterChar));
    AddCharBinding(charBoxQuote, nameof(FieldQualifierChar));
    AddCharBinding(charBoxEscape, nameof(EscapePrefixChar));

    numericUpDownSkipRows.DataBindings.Add("Value", this, nameof(SkipRows), true, DataSourceUpdateMode.OnPropertyChanged);
    textBoxComment.DataBindings.Add("Text", this, nameof(CommentLine), true, DataSourceUpdateMode.OnPropertyChanged);

    var sa = new SourceAccess(fileName);
    if (sa.FileType != FileTypeEnum.Plain)
      throw new NotSupportedException("Any file that is not a plain text is not supported.");

    DifferentSyntaxHighlighter(this, EventArgs.Empty);
  }

  public event PropertyChangedEventHandler? PropertyChanged;

  public string CommentLine
  {
    get => m_CommentLine;
    set
    {
      var newVal = value?.Trim() ?? string.Empty;
      if (m_CommentLine == newVal)
        return; m_CommentLine=newVal;
      OnPropertyChanged(nameof(CommentLine));
    }
  }

  public char EscapePrefixChar
  {
    get => m_EscapePrefixChar; set
    {
      if (m_EscapePrefixChar==value)
        return;
      m_EscapePrefixChar=value;
      OnPropertyChanged(nameof(EscapePrefixChar));
    }
  }

  public char FieldDelimiterChar
  {
    get => m_FieldDelimiterChar; set
    {
      if (m_FieldDelimiterChar==value)
        return;
      m_FieldDelimiterChar=value;
      OnPropertyChanged(nameof(FieldDelimiterChar));
    }
  }

  public char FieldQualifierChar
  {
    get => m_FieldQualifierChar; set
    {
      if (m_FieldQualifierChar==value)
        return;
      m_FieldQualifierChar=value;
      OnPropertyChanged(nameof(FieldQualifierChar));
    }
  }

  public int SkipRows
  {
    get => m_SkipRows;
    set
    {
      if (m_SkipRows==value)
        return;
      m_SkipRows=value;
      OnPropertyChanged(nameof(SkipRows));
    }
  }

  protected virtual void OnPropertyChanged(string propertyName) =>
              PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
  private void ButtonSkipLine_Click(object? sender, EventArgs e)
  {
    using var formProgress = new FormProgress("Check", CancellationToken.None);
    formProgress.Show(this);
    formProgress.Report("Opening");
    using var stream = FunctionalDI.GetStream(new SourceAccess(m_FileName));
    using var streamReader = new ImprovedTextReader(stream, m_CodePageId);
    formProgress.Report("Inspecting");
    SkipRows = streamReader.InspectStartRow(charBoxDelimiter.Character, charBoxQuote.Character,
      charBoxEscape.Character, textBoxComment.Text,
      formProgress.CancellationToken);

    HighlightVisibleRange(SkipRows);
  }

  private void DifferentSyntaxHighlighter(object? sender, EventArgs e)
  {
    m_HighLighter?.Dispose();
    m_HighLighter = new SyntaxHighlighterDelimitedText(textBox, charBoxQuote.Character, charBoxDelimiter.Character,
      charBoxEscape.Character, textBoxComment.Text);
    HighlightVisibleRange(SkipRows);
  }

  private void FindSkipRows_Load(object? sender, EventArgs e)
  {
    textBox.OpenFile(m_FileName,
      Encoding.GetEncoding(m_CodePageId, new EncoderReplacementFallback("?"),
        new DecoderReplacementFallback("?")));
  }

  private void HighlightVisibleRange(int skipRows)
  {
    try
    {
      //expand visible range (+- margin)
      var startLine = Math.Max(skipRows, textBox.VisibleRange.Start.iLine - 20);
      var endLine = Math.Min(textBox.LinesCount - 1, textBox.VisibleRange.End.iLine + 100);
      if (startLine < endLine)
      {
        var range = new FastColoredTextBoxNS.Range(textBox, 0, startLine, 0, endLine);
        m_HighLighter!.Highlight(range);
        m_HighLighter!.SkipRows(skipRows);
      }
    }
    catch (Exception ex)
    {
      try { Logger.Warning(ex, "HighlightVisibleRange"); } catch { }
    }
  }
  private void NumericUpDownSkipRows_ValueChanged(object? sender, EventArgs e)
  {
    HighlightVisibleRange(Convert.ToInt32(numericUpDownSkipRows.Value));
  }
  private void TextBox_VisibleRangeChangedDelayed(object? sender, EventArgs e)
  {
    HighlightVisibleRange(SkipRows);
  }
}