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
using System;
using System.ComponentModel;
using System.Text;

namespace CsvTools
{
  /// <summary>
  ///   UserControl: CsvTextDisplay
  /// </summary>
  public partial class FormTextDisplay : ResizeForm
  {
    private Language m_CurrentLang;
    private SyntaxHighlighterBase? m_HighLighter;

    /// <summary>
    ///   CTOR CsvTextDisplay
    /// </summary>
    public FormTextDisplay(in string display)
    {
      InitializeComponent();
      textBox.Text = display;
    }

    private enum Language
    {
      Text, Json, Xml, HTML
    }

    [Browsable(false)]
    [Bindable(false)]
    public Action<string>? SaveAction { get; set; }

    private void ButtonCancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
      SaveAction?.Invoke(textBox.Text);
      this.Close();
    }

    private void FormTextDisplay_Shown(object sender, EventArgs e)
    {
      var check = textBox.Text.Substring(0, Math.Min(textBox.Text.Length, 50)).TrimStart('"', '\'')
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
    }

    private void HandleText(in Language newLang)
    {
      if (m_CurrentLang == newLang)
        return;
      try
      {
        webBrowser.Visible = (newLang == Language.HTML);
        textBox.Visible = (newLang == Language.Text);
        fastColoredTextBoxRO.Visible = (newLang == Language.Json || newLang == Language.Xml);

        m_CurrentLang = newLang;
        switch (newLang)
        {
          case Language.HTML:
          {
            webBrowser.DocumentText = textBox.Text.Trim('\"', '\'', ' ');
            radioButtonHtml.Checked = true;
            break;
          }
          case Language.Xml:
          {
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(textBox.Text.Trim('\"', '\'', ' '));

            var settings = new System.Xml.XmlWriterSettings { Indent = true, NewLineOnAttributes = true };
            var stringBuilder = new StringBuilder();
            using var xmlWriter = System.Xml.XmlWriter.Create(stringBuilder, settings);
            doc.Save(xmlWriter);
            fastColoredTextBoxRO.Text = stringBuilder.ToString();
            radioButtonXml.Checked = true;
            break;
          }
          case Language.Json:
          {
            m_HighLighter ??= new SyntaxHighlighterJson(fastColoredTextBoxRO);
            var t = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(textBox.Text.Trim('\"', '\''));
            fastColoredTextBoxRO.Text = Newtonsoft.Json.JsonConvert.SerializeObject(t, Newtonsoft.Json.Formatting.Indented);
            radioButtonJson.Checked = true;
            break;
          }
          case Language.Text:
          default:
            radioButtonText.Checked = true;
            break;
        }
      }
      catch (Exception exception)
      {                
        fastColoredTextBoxRO.Visible=false;        
        webBrowser.Visible=false;
        textBox.Visible=true;
        radioButtonText.Checked = true;
        // fastColoredTextBoxRO.Text =textBox.Text;
        FindForm().ShowError(exception, $"Error trying to parse {newLang}: {exception.Message}");        
      }
    }

    private void HighlightVisibleRange()
    {
      try
      {
        var startLine = Math.Max(0, textBox.VisibleRange.Start.iLine - 20);
        var endLine = Math.Min(textBox.LinesCount - 1, textBox.VisibleRange.End.iLine + 20);
        var range = new FastColoredTextBoxNS.Range(fastColoredTextBoxRO, 0, startLine, 0, endLine);
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (m_CurrentLang == Language.Xml)
          textBox.SyntaxHighlighter.XMLSyntaxHighlight(range);
        else if (m_CurrentLang == Language.Json) m_HighLighter?.Highlight(range);
      }
      catch
      {
        // ignored
      }
    }

    private void RadioButton1_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButtonText.Checked)
        HandleText(Language.Text);
    }

    private void RadioButton2_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButtonJson.Checked)
        HandleText(Language.Json);
    }

    private void RadioButton3_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButtonXml.Checked)
        HandleText(Language.Xml);
    }

    private void RadioButton4_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButtonHtml.Checked)
        HandleText(Language.HTML);
    }

    private void TextBox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
    {
      buttonSave.Enabled = textBox.IsChanged;
    }

    private void TextBox_TextChangedDelayed(object? sender, FastColoredTextBoxNS.TextChangedEventArgs e) =>
                          HighlightVisibleRange();

    private void TextBox_VisibleRangeChangedDelayed(object? sender, EventArgs e) => HighlightVisibleRange();
  }
}