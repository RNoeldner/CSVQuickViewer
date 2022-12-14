/*
 * Copyright (C) 2014 Raphael Nöldner
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
using Newtonsoft.Json;
using System;
using System.Text;
using System.Xml;

namespace CsvTools
{
  /// <summary>
  ///   UserControl: CsvTextDisplay
  /// </summary>
  public partial class FormTextDisplay : ResizeForm
  {
    private ISyntaxHighlighter? m_HighLighter;
    private readonly string m_Source;
    private Language m_CurrentLang;

    private enum Language
    {
      None, Text, Json, Xml, HTML
    }

    private void HandleText(in Language newLang)
    {
      if (m_CurrentLang == newLang)
        return;

      try
      {
        m_CurrentLang = newLang;
        webBrowser.Visible = newLang == Language.HTML;
        textBox.Visible = !webBrowser.Visible;
        switch (newLang)
        {
          case Language.HTML:
          {
            webBrowser.DocumentText = m_Source.Trim('\"', '\'', ' ');
            radioButton4.Checked = true;
            break;
          }
          case Language.Xml:
          {
            XmlDocument doc = new();
            doc.LoadXml(m_Source.Trim('\"', '\'', ' '));

            var settings = new XmlWriterSettings { Indent = true, NewLineOnAttributes = true };
            var stringBuilder = new StringBuilder();
            using var xmlWriter = XmlWriter.Create(stringBuilder, settings);
            doc.Save(xmlWriter);
            textBox.Text = stringBuilder.ToString();
            radioButton3.Checked = true;
            break;
          }
          case Language.Json:
          {
            m_HighLighter ??= new SyntaxHighlighterJson(textBox);
            var t = JsonConvert.DeserializeObject<object>(m_Source.Trim('\"', '\''));
            textBox.Text = JsonConvert.SerializeObject(t, Newtonsoft.Json.Formatting.Indented);
            radioButton2.Checked = true;
            break;
          }
          case Language.Text:
          default:
            radioButton1.Checked = true;
            textBox.Text = m_Source;
            break;
        }
      }
      catch (Exception exception)
      {
        textBox.Text =
          $"Error trying to parse {newLang}: {exception.Message}\n\n{m_Source.Substring(0, Math.Min(m_Source.Length - 1, 400))}";
        m_CurrentLang = Language.Text;
      }
    }

    /// <summary>
    ///   CTOR CsvTextDisplay
    /// </summary>
    public FormTextDisplay(in string display)
    {
      InitializeComponent();
      m_Source = display;
    }

    private void HighlightVisibleRange()
    {
      try
      {
        //expand visible range (+- margin)
        var startLine = Math.Max(0, textBox.VisibleRange.Start.iLine - 20);
        var endLine = Math.Min(textBox.LinesCount - 1, textBox.VisibleRange.End.iLine + 20);
        var range = new FastColoredTextBoxNS.Range(textBox, 0, startLine, 0, endLine);
        switch (m_CurrentLang)
        {
          case Language.Xml:
            textBox.SyntaxHighlighter.XMLSyntaxHighlight(textBox.Range);
            break;
          case Language.Json:
            m_HighLighter?.Highlight(range);
            break;
        }
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "FormCsvTextDisplay.HighlightVisibleRange");
      }
    }

    private void TextBox_TextChangedDelayed(object? sender, FastColoredTextBoxNS.TextChangedEventArgs e) =>
      HighlightVisibleRange();

    private void TextBox_VisibleRangeChangedDelayed(object? sender, EventArgs e) => HighlightVisibleRange();

    private void radioButton1_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButton1.Checked)
        HandleText(Language.Text);
    }

    private void radioButton2_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButton2.Checked)
        HandleText(Language.Json);
    }

    private void radioButton3_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButton3.Checked)
        HandleText(Language.Xml);
    }

    private void radioButton4_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButton4.Checked)
        HandleText(Language.HTML);
    }

    private void FormTextDisplay_Shown(object sender, EventArgs e)
    {
      var check = m_Source.Substring(0, Math.Min(m_Source.Length - 1, 50)).TrimStart('"', '\'')
        .Replace(" ", "")
        .Replace("\r", "")
        .Replace("\n", "");

      if (check.StartsWith("<xml", StringComparison.OrdinalIgnoreCase) ||
          check.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
        HandleText(Language.Xml);
      else if (check.StartsWith("<html", StringComparison.OrdinalIgnoreCase))
        HandleText(Language.HTML);
      else if (check.StartsWith("{\"", StringComparison.OrdinalIgnoreCase))
        HandleText(Language.Json);
      else
        HandleText(Language.Text);
    }
  }
}