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

#nullable enable

using FastColoredTextBoxNS;
using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   A Control to edit the quoting and visualize the result
  /// </summary>
  public class QuotingControl : UserControl
  {
    private readonly Style m_Delimiter = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
    private readonly Style m_DelimiterTab = new SyntaxHighlighterDelimitedText.SyntaxHighlightStyleTab(Pens.Blue, Brushes.AntiqueWhite);
    private readonly Style m_Pilcrow = new TextStyle(Brushes.Orange, null, FontStyle.Bold);
    private readonly Style m_Quote = new TextStyle(Brushes.Magenta, null, FontStyle.Regular);
    private readonly Style m_EscapedQuote = new TextStyle(Brushes.Black, Brushes.LightSteelBlue, FontStyle.Regular);
    private readonly Style m_Space = new SyntaxHighlighterDelimitedText.SyntaxHighlightStyleStyleSpace(Brushes.Blue, Brushes.AntiqueWhite);
    private readonly Regex m_SpaceRegex = new Regex(" ", RegexOptions.Singleline | RegexOptions.Compiled);
    private CheckBox? checkBoxAlternateQuoting;
    private CheckBox? checkBoxDuplicateQuotingToEscape;
    private CheckBox? checkBoxQualifyAlways;
    private CheckBox? checkBoxQualifyOnlyNeeded;
    private ComboBox? comboBoxTrim;
    private IContainer? components;
    private FastColoredTextBox? fastColoredTextBox;
    private FastColoredTextBox? fastColoredTextBox00;
    private FastColoredTextBox? fastColoredTextBox01;
    private FastColoredTextBox? fastColoredTextBox02;
    private FastColoredTextBox? fastColoredTextBox10;
    private FastColoredTextBox? fastColoredTextBox11;
    private FastColoredTextBox? fastColoredTextBox12;
    private Label? label1;
    private Label? label2;
    private Label? label3;
    private Label? label4;
    private Label? label5;
    private Label? labelNoQuotes;
    private ICsvFile? m_CsvFile;

    private ErrorProvider? m_ErrorProvider;

    private BindingSource? csvSettingBindingSource;

    private bool m_IsDisposed;

    private bool m_IsWriteSetting;
    private Label? m_LabelEscapeCharacter;
    private Label? m_LabelInfoQuoting;
    private Label? m_LabelQuote;
    private Label? m_LabelQuotePlaceholder;
    private Label? m_LabelTrim;
    private TableLayoutPanel? m_TableLayoutPanel;
    private TextBox? m_TextBoxEscape;
    private TextBox? m_TextBoxQuote;
    private TextBox? m_TextBoxQuotePlaceHolder;
    private Label? label6;
    private ToolTip? m_ToolTip;

    /// <summary>
    ///   CTOR of QuotingControl
    /// </summary>
    public QuotingControl()
    {
      InitializeComponent();
      comboBoxTrim!.Items.Add(new DisplayItem<int>(0, "None"));
      comboBoxTrim.Items.Add(new DisplayItem<int>(1, "Unquoted"));
      comboBoxTrim.Items.Add(new DisplayItem<int>(3, "All"));
      UpdateUI();
      QuoteOrDelimiterChange();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Bindable(false)]
    [Browsable(false)]
    public ICsvFile? CsvFile
    {
      get => m_CsvFile;

      set
      {
        m_CsvFile = value;
        if (m_CsvFile is null)
          return;

        csvSettingBindingSource!.DataSource = m_CsvFile;

        m_CsvFile.PropertyChanged += FormatPropertyChanged;
        m_CsvFile.PropertyChanged += SettingPropertyChanged;

        UpdateUI();
      }
    }

    /// <summary>
    ///   In case of a Write only setting things will be hidden
    /// </summary>
    public bool IsWriteSetting
    {
      get => m_IsWriteSetting;

      set
      {
        m_IsWriteSetting = value;
        m_LabelInfoQuoting!.Visible = !value;
        comboBoxTrim!.Enabled = !value;
        checkBoxAlternateQuoting!.Visible = !value;
        checkBoxQualifyAlways!.Visible = value;
        checkBoxQualifyOnlyNeeded!.Visible = value;
      }
    }

    /// <summary>
    ///   Dispose
    /// </summary>
    /// <param name="disposing">
    ///   true to release both managed and unmanaged resources; false to release only unmanaged resources.
    /// </param>
    protected override void Dispose(bool disposing)
    {
      if (m_IsDisposed) return;
      if (!disposing) return;
      m_IsDisposed = true;
      base.Dispose(true);
    }

    private void FormatPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(CsvFile.FieldDelimiterChar) ||
          e.PropertyName == nameof(CsvFile.DuplicateQualifierToEscape))
        QuoteOrDelimiterChange();
    }

    /// <summary>
    ///   Initializes the component.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.csvSettingBindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.m_ToolTip = new System.Windows.Forms.ToolTip(this.components);
      this.checkBoxDuplicateQuotingToEscape = new System.Windows.Forms.CheckBox();
      this.labelNoQuotes = new System.Windows.Forms.Label();
      this.checkBoxAlternateQuoting = new System.Windows.Forms.CheckBox();
      this.comboBoxTrim = new System.Windows.Forms.ComboBox();
      this.m_TextBoxQuotePlaceHolder = new System.Windows.Forms.TextBox();
      this.m_TextBoxQuote = new System.Windows.Forms.TextBox();
      this.m_ErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      this.fastColoredTextBox00 = new FastColoredTextBoxNS.FastColoredTextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.fastColoredTextBox12 = new FastColoredTextBoxNS.FastColoredTextBox();
      this.fastColoredTextBox02 = new FastColoredTextBoxNS.FastColoredTextBox();
      this.fastColoredTextBox11 = new FastColoredTextBoxNS.FastColoredTextBox();
      this.fastColoredTextBox01 = new FastColoredTextBoxNS.FastColoredTextBox();
      this.fastColoredTextBox10 = new FastColoredTextBoxNS.FastColoredTextBox();
      this.fastColoredTextBox = new FastColoredTextBoxNS.FastColoredTextBox();
      this.checkBoxQualifyAlways = new System.Windows.Forms.CheckBox();
      this.checkBoxQualifyOnlyNeeded = new System.Windows.Forms.CheckBox();
      this.m_LabelInfoQuoting = new System.Windows.Forms.Label();
      this.m_LabelTrim = new System.Windows.Forms.Label();
      this.m_TextBoxEscape = new System.Windows.Forms.TextBox();
      this.m_LabelQuotePlaceholder = new System.Windows.Forms.Label();
      this.m_LabelEscapeCharacter = new System.Windows.Forms.Label();
      this.m_LabelQuote = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.m_TableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.label5 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.csvSettingBindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_ErrorProvider)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBox00)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBox12)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBox02)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBox11)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBox01)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBox10)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBox)).BeginInit();
      this.m_TableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // csvSettingBindingSource
      // 
      this.csvSettingBindingSource.AllowNew = false;
      this.csvSettingBindingSource.DataSource = typeof(CsvTools.CsvFile);
      // 
      // checkBoxDuplicateQuotingToEscape
      // 
      this.checkBoxDuplicateQuotingToEscape.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.checkBoxDuplicateQuotingToEscape, 2);
      this.checkBoxDuplicateQuotingToEscape.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.csvSettingBindingSource, "DuplicateQualifierToEscape", true));
      this.checkBoxDuplicateQuotingToEscape.Location = new System.Drawing.Point(362, 26);
      this.checkBoxDuplicateQuotingToEscape.Margin = new System.Windows.Forms.Padding(2);
      this.checkBoxDuplicateQuotingToEscape.Name = "checkBoxDuplicateQuotingToEscape";
      this.checkBoxDuplicateQuotingToEscape.Size = new System.Drawing.Size(134, 17);
      this.checkBoxDuplicateQuotingToEscape.TabIndex = 27;
      this.checkBoxDuplicateQuotingToEscape.Text = "Repeated Qualification";
      this.m_ToolTip.SetToolTip(this.checkBoxDuplicateQuotingToEscape, "Assume a repeated quote in a qualified text represent a quote that does not end t" +
        "ext qualification");
      this.checkBoxDuplicateQuotingToEscape.UseVisualStyleBackColor = true;
      this.checkBoxDuplicateQuotingToEscape.CheckedChanged += new System.EventHandler(this.QuoteChanged);
      // 
      // labelNoQuotes
      // 
      this.labelNoQuotes.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.labelNoQuotes.AutoSize = true;
      this.labelNoQuotes.BackColor = System.Drawing.SystemColors.Info;
      this.labelNoQuotes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.labelNoQuotes.ForeColor = System.Drawing.SystemColors.InfoText;
      this.labelNoQuotes.Location = new System.Drawing.Point(592, 16);
      this.labelNoQuotes.Name = "labelNoQuotes";
      this.m_TableLayoutPanel.SetRowSpan(this.labelNoQuotes, 2);
      this.labelNoQuotes.Size = new System.Drawing.Size(146, 15);
      this.labelNoQuotes.TabIndex = 28;
      this.labelNoQuotes.Text = "Text can not contain qualifier";
      this.m_ToolTip.SetToolTip(this.labelNoQuotes, "Either “Context Sensitive Quoting”, “Repeated Quotes” or an “Escape Character” ne" +
        "ed to be defined to allow a quote to be part of the text");
      // 
      // checkBoxAlternateQuoting
      // 
      this.checkBoxAlternateQuoting.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxAlternateQuoting.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.checkBoxAlternateQuoting, 2);
      this.checkBoxAlternateQuoting.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.csvSettingBindingSource, "ContextSensitiveQualifier", true));
      this.checkBoxAlternateQuoting.Location = new System.Drawing.Point(363, 3);
      this.checkBoxAlternateQuoting.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.checkBoxAlternateQuoting.Name = "checkBoxAlternateQuoting";
      this.checkBoxAlternateQuoting.Size = new System.Drawing.Size(169, 17);
      this.checkBoxAlternateQuoting.TabIndex = 2;
      this.checkBoxAlternateQuoting.Text = "Context Sensitive Qualification";
      this.m_ToolTip.SetToolTip(this.checkBoxAlternateQuoting, "A quote is only regarded as closing quote if it is followed by linefeed or delimi" +
        "ter\r\nThis is a uncommon way of quoting but allows to parse incorrectly quoted fi" +
        "les");
      this.checkBoxAlternateQuoting.UseVisualStyleBackColor = true;
      this.checkBoxAlternateQuoting.Visible = false;
      this.checkBoxAlternateQuoting.CheckedChanged += new System.EventHandler(this.QuoteChanged);
      // 
      // comboBoxTrim
      // 
      this.comboBoxTrim.DisplayMember = "Display";
      this.comboBoxTrim.Dock = System.Windows.Forms.DockStyle.Top;
      this.comboBoxTrim.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxTrim.Location = new System.Drawing.Point(104, 74);
      this.comboBoxTrim.Margin = new System.Windows.Forms.Padding(3, 2, 3, 3);
      this.comboBoxTrim.Name = "comboBoxTrim";
      this.comboBoxTrim.Size = new System.Drawing.Size(253, 21);
      this.comboBoxTrim.TabIndex = 10;
      this.m_ToolTip.SetToolTip(this.comboBoxTrim, "None will preserve whitespace; Unquoted will remove white spaces if the column wa" +
        "s not quoted; All will remove white spaces even if the column was quoted");
      this.comboBoxTrim.ValueMember = "ID";
      this.comboBoxTrim.SelectedIndexChanged += new System.EventHandler(this.SetTrimming);
      // 
      // m_TextBoxQuotePlaceHolder
      // 
      this.m_TextBoxQuotePlaceHolder.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
      this.m_TextBoxQuotePlaceHolder.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.csvSettingBindingSource, "QualifierPlaceholder", true));
      this.m_TextBoxQuotePlaceHolder.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_TextBoxQuotePlaceHolder.Location = new System.Drawing.Point(104, 50);
      this.m_TextBoxQuotePlaceHolder.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_TextBoxQuotePlaceHolder.Name = "m_TextBoxQuotePlaceHolder";
      this.m_TextBoxQuotePlaceHolder.Size = new System.Drawing.Size(253, 20);
      this.m_TextBoxQuotePlaceHolder.TabIndex = 8;
      this.m_ToolTip.SetToolTip(this.m_TextBoxQuotePlaceHolder, "If this placeholder is part of the text it will be replaced with the quoting char" +
        "acter");
      this.m_TextBoxQuotePlaceHolder.TextChanged += new System.EventHandler(this.QuoteChanged);
      // 
      // m_TextBoxQuote
      // 
      this.m_TextBoxQuote.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.csvSettingBindingSource, "FieldQualifier", true));
      this.m_TextBoxQuote.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_TextBoxQuote.Location = new System.Drawing.Point(104, 2);
      this.m_TextBoxQuote.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_TextBoxQuote.Name = "m_TextBoxQuote";
      this.m_TextBoxQuote.Size = new System.Drawing.Size(253, 20);
      this.m_TextBoxQuote.TabIndex = 1;
      this.m_ToolTip.SetToolTip(this.m_TextBoxQuote, "Columns may be qualified with a character; usually these are \" the quotes are rem" +
        "oved by the reading applications.");
      this.m_TextBoxQuote.TextChanged += new System.EventHandler(this.QuoteChanged);
      // 
      // m_ErrorProvider
      // 
      this.m_ErrorProvider.ContainerControl = this;
      // 
      // fastColoredTextBox00
      // 
      this.fastColoredTextBox00.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
      this.fastColoredTextBox00.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(" +
    "?<range>:)\\s*(?<range>[^;]+);";
      this.fastColoredTextBox00.AutoScrollMinSize = new System.Drawing.Size(66, 14);
      this.fastColoredTextBox00.BackBrush = null;
      this.fastColoredTextBox00.CharHeight = 14;
      this.fastColoredTextBox00.CharWidth = 8;
      this.fastColoredTextBox00.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.fastColoredTextBox00.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.fastColoredTextBox00.Font = new System.Drawing.Font("Courier New", 9.75F);
      this.fastColoredTextBox00.IsReplaceMode = false;
      this.fastColoredTextBox00.Location = new System.Drawing.Point(405, 121);
      this.fastColoredTextBox00.Multiline = false;
      this.fastColoredTextBox00.Name = "fastColoredTextBox00";
      this.fastColoredTextBox00.Paddings = new System.Windows.Forms.Padding(0);
      this.fastColoredTextBox00.ReadOnly = true;
      this.fastColoredTextBox00.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.fastColoredTextBox00.ServiceColors = null;
      this.fastColoredTextBox00.ShowLineNumbers = false;
      this.fastColoredTextBox00.ShowScrollBars = false;
      this.fastColoredTextBox00.Size = new System.Drawing.Size(181, 18);
      this.fastColoredTextBox00.TabIndex = 29;
      this.fastColoredTextBox00.Text = "This is ";
      this.fastColoredTextBox00.Zoom = 100;
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.AutoSize = true;
      this.label1.ForeColor = System.Drawing.Color.Teal;
      this.label1.Location = new System.Drawing.Point(363, 118);
      this.label1.Name = "label1";
      this.label1.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.label1.Size = new System.Drawing.Size(36, 16);
      this.label1.TabIndex = 35;
      this.label1.Text = "Rec 1";
      // 
      // fastColoredTextBox12
      // 
      this.fastColoredTextBox12.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
      this.fastColoredTextBox12.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(" +
    "?<range>:)\\s*(?<range>[^;]+);";
      this.fastColoredTextBox12.AutoScrollMinSize = new System.Drawing.Size(106, 28);
      this.fastColoredTextBox12.BackBrush = null;
      this.fastColoredTextBox12.CharHeight = 14;
      this.fastColoredTextBox12.CharWidth = 8;
      this.fastColoredTextBox12.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.fastColoredTextBox12.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.fastColoredTextBox12.Font = new System.Drawing.Font("Courier New", 9.75F);
      this.fastColoredTextBox12.IsReplaceMode = false;
      this.fastColoredTextBox12.Location = new System.Drawing.Point(592, 169);
      this.fastColoredTextBox12.Name = "fastColoredTextBox12";
      this.fastColoredTextBox12.Paddings = new System.Windows.Forms.Padding(0);
      this.fastColoredTextBox12.ReadOnly = true;
      this.fastColoredTextBox12.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.fastColoredTextBox12.ServiceColors = null;
      this.fastColoredTextBox12.ShowLineNumbers = false;
      this.fastColoredTextBox12.Size = new System.Drawing.Size(191, 31);
      this.fastColoredTextBox12.TabIndex = 34;
      this.fastColoredTextBox12.Text = "Column with ¶\r\nLinefeed";
      this.fastColoredTextBox12.Zoom = 100;
      // 
      // fastColoredTextBox02
      // 
      this.fastColoredTextBox02.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
      this.fastColoredTextBox02.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(" +
    "?<range>:)\\s*(?<range>[^;]+);";
      this.fastColoredTextBox02.AutoScrollMinSize = new System.Drawing.Size(66, 14);
      this.fastColoredTextBox02.BackBrush = null;
      this.fastColoredTextBox02.CharHeight = 14;
      this.fastColoredTextBox02.CharWidth = 8;
      this.fastColoredTextBox02.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.fastColoredTextBox02.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.fastColoredTextBox02.Font = new System.Drawing.Font("Courier New", 9.75F);
      this.fastColoredTextBox02.IsReplaceMode = false;
      this.fastColoredTextBox02.Location = new System.Drawing.Point(405, 169);
      this.fastColoredTextBox02.Multiline = false;
      this.fastColoredTextBox02.Name = "fastColoredTextBox02";
      this.fastColoredTextBox02.Paddings = new System.Windows.Forms.Padding(0);
      this.fastColoredTextBox02.ReadOnly = true;
      this.fastColoredTextBox02.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.fastColoredTextBox02.ServiceColors = null;
      this.fastColoredTextBox02.ShowLineNumbers = false;
      this.fastColoredTextBox02.ShowScrollBars = false;
      this.fastColoredTextBox02.Size = new System.Drawing.Size(181, 18);
      this.fastColoredTextBox02.TabIndex = 33;
      this.fastColoredTextBox02.Text = "Example ";
      this.fastColoredTextBox02.Zoom = 100;
      // 
      // fastColoredTextBox11
      // 
      this.fastColoredTextBox11.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
      this.fastColoredTextBox11.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(" +
    "?<range>:)\\s*(?<range>[^;]+);";
      this.fastColoredTextBox11.AutoScrollMinSize = new System.Drawing.Size(154, 14);
      this.fastColoredTextBox11.BackBrush = null;
      this.fastColoredTextBox11.CharHeight = 14;
      this.fastColoredTextBox11.CharWidth = 8;
      this.fastColoredTextBox11.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.fastColoredTextBox11.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.fastColoredTextBox11.Font = new System.Drawing.Font("Courier New", 9.75F);
      this.fastColoredTextBox11.IsReplaceMode = false;
      this.fastColoredTextBox11.Location = new System.Drawing.Point(592, 145);
      this.fastColoredTextBox11.Multiline = false;
      this.fastColoredTextBox11.Name = "fastColoredTextBox11";
      this.fastColoredTextBox11.Paddings = new System.Windows.Forms.Padding(0);
      this.fastColoredTextBox11.ReadOnly = true;
      this.fastColoredTextBox11.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.fastColoredTextBox11.ServiceColors = null;
      this.fastColoredTextBox11.ShowLineNumbers = false;
      this.fastColoredTextBox11.ShowScrollBars = false;
      this.fastColoredTextBox11.Size = new System.Drawing.Size(191, 18);
      this.fastColoredTextBox11.TabIndex = 32;
      this.fastColoredTextBox11.Text = "Column with \" Quote";
      this.fastColoredTextBox11.Zoom = 100;
      // 
      // fastColoredTextBox01
      // 
      this.fastColoredTextBox01.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
      this.fastColoredTextBox01.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(" +
    "?<range>:)\\s*(?<range>[^;]+);";
      this.fastColoredTextBox01.AutoScrollMinSize = new System.Drawing.Size(98, 14);
      this.fastColoredTextBox01.BackBrush = null;
      this.fastColoredTextBox01.CharHeight = 14;
      this.fastColoredTextBox01.CharWidth = 8;
      this.fastColoredTextBox01.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.fastColoredTextBox01.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.fastColoredTextBox01.Font = new System.Drawing.Font("Courier New", 9.75F);
      this.fastColoredTextBox01.IsReplaceMode = false;
      this.fastColoredTextBox01.Location = new System.Drawing.Point(405, 145);
      this.fastColoredTextBox01.Multiline = false;
      this.fastColoredTextBox01.Name = "fastColoredTextBox01";
      this.fastColoredTextBox01.Paddings = new System.Windows.Forms.Padding(0);
      this.fastColoredTextBox01.ReadOnly = true;
      this.fastColoredTextBox01.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.fastColoredTextBox01.ServiceColors = null;
      this.fastColoredTextBox01.ShowLineNumbers = false;
      this.fastColoredTextBox01.ShowScrollBars = false;
      this.fastColoredTextBox01.Size = new System.Drawing.Size(181, 18);
      this.fastColoredTextBox01.TabIndex = 31;
      this.fastColoredTextBox01.Text = " a Trimming ";
      this.fastColoredTextBox01.Zoom = 100;
      // 
      // fastColoredTextBox10
      // 
      this.fastColoredTextBox10.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
      this.fastColoredTextBox10.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(" +
    "?<range>:)\\s*(?<range>[^;]+);";
      this.fastColoredTextBox10.AutoScrollMinSize = new System.Drawing.Size(186, 14);
      this.fastColoredTextBox10.BackBrush = null;
      this.fastColoredTextBox10.CharHeight = 14;
      this.fastColoredTextBox10.CharWidth = 8;
      this.fastColoredTextBox10.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.fastColoredTextBox10.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.fastColoredTextBox10.IsReplaceMode = false;
      this.fastColoredTextBox10.Location = new System.Drawing.Point(592, 121);
      this.fastColoredTextBox10.Multiline = false;
      this.fastColoredTextBox10.Name = "fastColoredTextBox10";
      this.fastColoredTextBox10.Paddings = new System.Windows.Forms.Padding(0);
      this.fastColoredTextBox10.ReadOnly = true;
      this.fastColoredTextBox10.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.fastColoredTextBox10.ServiceColors = null;
      this.fastColoredTextBox10.ShowLineNumbers = false;
      this.fastColoredTextBox10.ShowScrollBars = false;
      this.fastColoredTextBox10.Size = new System.Drawing.Size(191, 18);
      this.fastColoredTextBox10.TabIndex = 30;
      this.fastColoredTextBox10.TabLength = 1;
      this.fastColoredTextBox10.TabStop = false;
      this.fastColoredTextBox10.Text = "Column with:, Delimiter";
      this.fastColoredTextBox10.Zoom = 100;
      // 
      // fastColoredTextBox
      // 
      this.fastColoredTextBox.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
      this.fastColoredTextBox.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(" +
    "?<range>:)\\s*(?<range>[^;]+);";
      this.fastColoredTextBox.AutoScrollMinSize = new System.Drawing.Size(307, 56);
      this.fastColoredTextBox.BackBrush = null;
      this.fastColoredTextBox.CharHeight = 14;
      this.fastColoredTextBox.CharWidth = 8;
      this.m_TableLayoutPanel.SetColumnSpan(this.fastColoredTextBox, 2);
      this.fastColoredTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.fastColoredTextBox.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.fastColoredTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.fastColoredTextBox.IsReplaceMode = false;
      this.fastColoredTextBox.Location = new System.Drawing.Point(3, 121);
      this.fastColoredTextBox.Name = "fastColoredTextBox";
      this.fastColoredTextBox.Paddings = new System.Windows.Forms.Padding(0);
      this.fastColoredTextBox.ReadOnly = true;
      this.m_TableLayoutPanel.SetRowSpan(this.fastColoredTextBox, 3);
      this.fastColoredTextBox.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.fastColoredTextBox.ServiceColors = null;
      this.fastColoredTextBox.Size = new System.Drawing.Size(354, 87);
      this.fastColoredTextBox.TabIndex = 1;
      this.fastColoredTextBox.TabLength = 1;
      this.fastColoredTextBox.TabStop = false;
      this.fastColoredTextBox.Text = "\"This is \";Column with:, Delimiter¶\r\n a Trimming ;Column with \"\" Quote¶\r\nExample " +
    ";\"Column with ¶\r\nLinefeed\"";
      this.fastColoredTextBox.Zoom = 100;
      // 
      // checkBoxQualifyAlways
      // 
      this.checkBoxQualifyAlways.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxQualifyAlways.AutoSize = true;
      this.checkBoxQualifyAlways.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.csvSettingBindingSource, "QualifyAlways", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxQualifyAlways.Location = new System.Drawing.Point(592, 51);
      this.checkBoxQualifyAlways.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.checkBoxQualifyAlways.Name = "checkBoxQualifyAlways";
      this.checkBoxQualifyAlways.Size = new System.Drawing.Size(94, 17);
      this.checkBoxQualifyAlways.TabIndex = 3;
      this.checkBoxQualifyAlways.Text = "Qualify Always";
      this.checkBoxQualifyAlways.UseVisualStyleBackColor = true;
      this.checkBoxQualifyAlways.Visible = false;
      // 
      // checkBoxQualifyOnlyNeeded
      // 
      this.checkBoxQualifyOnlyNeeded.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxQualifyOnlyNeeded.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.checkBoxQualifyOnlyNeeded, 2);
      this.checkBoxQualifyOnlyNeeded.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.csvSettingBindingSource, "QualifyOnlyIfNeeded", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxQualifyOnlyNeeded.Location = new System.Drawing.Point(363, 51);
      this.checkBoxQualifyOnlyNeeded.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.checkBoxQualifyOnlyNeeded.Name = "checkBoxQualifyOnlyNeeded";
      this.checkBoxQualifyOnlyNeeded.Size = new System.Drawing.Size(132, 17);
      this.checkBoxQualifyOnlyNeeded.TabIndex = 4;
      this.checkBoxQualifyOnlyNeeded.Text = "Qualify Only If Needed";
      this.checkBoxQualifyOnlyNeeded.UseVisualStyleBackColor = true;
      this.checkBoxQualifyOnlyNeeded.Visible = false;
      // 
      // m_LabelInfoQuoting
      // 
      this.m_LabelInfoQuoting.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_LabelInfoQuoting.AutoSize = true;
      this.m_LabelInfoQuoting.BackColor = System.Drawing.SystemColors.Info;
      this.m_LabelInfoQuoting.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_LabelInfoQuoting, 3);
      this.m_LabelInfoQuoting.ForeColor = System.Drawing.SystemColors.InfoText;
      this.m_LabelInfoQuoting.Location = new System.Drawing.Point(363, 77);
      this.m_LabelInfoQuoting.Name = "m_LabelInfoQuoting";
      this.m_LabelInfoQuoting.Size = new System.Drawing.Size(225, 15);
      this.m_LabelInfoQuoting.TabIndex = 11;
      this.m_LabelInfoQuoting.Text = "Not possible to have leading or trailing spaces";
      // 
      // m_LabelTrim
      // 
      this.m_LabelTrim.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_LabelTrim.AutoSize = true;
      this.m_LabelTrim.Location = new System.Drawing.Point(12, 78);
      this.m_LabelTrim.Name = "m_LabelTrim";
      this.m_LabelTrim.Size = new System.Drawing.Size(86, 13);
      this.m_LabelTrim.TabIndex = 9;
      this.m_LabelTrim.Text = "Trimming Option:";
      // 
      // m_TextBoxEscape
      // 
      this.m_TextBoxEscape.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_TextBoxEscape.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.csvSettingBindingSource, "EscapePrefix", true));
      this.m_TextBoxEscape.Location = new System.Drawing.Point(104, 26);
      this.m_TextBoxEscape.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_TextBoxEscape.Name = "m_TextBoxEscape";
      this.m_TextBoxEscape.Size = new System.Drawing.Size(96, 20);
      this.m_TextBoxEscape.TabIndex = 6;
      this.m_TextBoxEscape.TextChanged += new System.EventHandler(this.QuoteChanged);
      // 
      // m_LabelQuotePlaceholder
      // 
      this.m_LabelQuotePlaceholder.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_LabelQuotePlaceholder.AutoSize = true;
      this.m_LabelQuotePlaceholder.Location = new System.Drawing.Point(32, 53);
      this.m_LabelQuotePlaceholder.Name = "m_LabelQuotePlaceholder";
      this.m_LabelQuotePlaceholder.Size = new System.Drawing.Size(66, 13);
      this.m_LabelQuotePlaceholder.TabIndex = 7;
      this.m_LabelQuotePlaceholder.Text = "Placeholder:";
      // 
      // m_LabelEscapeCharacter
      // 
      this.m_LabelEscapeCharacter.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_LabelEscapeCharacter.AutoSize = true;
      this.m_LabelEscapeCharacter.Location = new System.Drawing.Point(3, 29);
      this.m_LabelEscapeCharacter.Name = "m_LabelEscapeCharacter";
      this.m_LabelEscapeCharacter.Size = new System.Drawing.Size(95, 13);
      this.m_LabelEscapeCharacter.TabIndex = 5;
      this.m_LabelEscapeCharacter.Text = "Escape Character:";
      // 
      // m_LabelQuote
      // 
      this.m_LabelQuote.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_LabelQuote.AutoSize = true;
      this.m_LabelQuote.Location = new System.Drawing.Point(26, 5);
      this.m_LabelQuote.Name = "m_LabelQuote";
      this.m_LabelQuote.Size = new System.Drawing.Size(72, 13);
      this.m_LabelQuote.TabIndex = 0;
      this.m_LabelQuote.Text = "Text Qualifier:";
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.AutoSize = true;
      this.label2.ForeColor = System.Drawing.Color.Teal;
      this.label2.Location = new System.Drawing.Point(363, 142);
      this.label2.Name = "label2";
      this.label2.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.label2.Size = new System.Drawing.Size(36, 16);
      this.label2.TabIndex = 36;
      this.label2.Text = "Rec 2";
      // 
      // label3
      // 
      this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label3.AutoSize = true;
      this.label3.ForeColor = System.Drawing.Color.Teal;
      this.label3.Location = new System.Drawing.Point(363, 166);
      this.label3.Name = "label3";
      this.label3.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.label3.Size = new System.Drawing.Size(36, 16);
      this.label3.TabIndex = 37;
      this.label3.Text = "Rec 3";
      // 
      // m_TableLayoutPanel
      // 
      this.m_TableLayoutPanel.AutoSize = true;
      this.m_TableLayoutPanel.ColumnCount = 7;
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 16F));
      this.m_TableLayoutPanel.Controls.Add(this.label3, 3, 7);
      this.m_TableLayoutPanel.Controls.Add(this.label2, 3, 6);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelQuote, 0, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelEscapeCharacter, 0, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelQuotePlaceholder, 0, 2);
      this.m_TableLayoutPanel.Controls.Add(this.m_TextBoxQuote, 1, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_TextBoxEscape, 1, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_TextBoxQuotePlaceHolder, 1, 2);
      this.m_TableLayoutPanel.Controls.Add(this.comboBoxTrim, 1, 3);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelTrim, 0, 3);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelInfoQuoting, 3, 3);
      this.m_TableLayoutPanel.Controls.Add(this.checkBoxQualifyOnlyNeeded, 3, 2);
      this.m_TableLayoutPanel.Controls.Add(this.checkBoxAlternateQuoting, 3, 0);
      this.m_TableLayoutPanel.Controls.Add(this.labelNoQuotes, 5, 0);
      this.m_TableLayoutPanel.Controls.Add(this.checkBoxQualifyAlways, 5, 2);
      this.m_TableLayoutPanel.Controls.Add(this.checkBoxDuplicateQuotingToEscape, 3, 1);
      this.m_TableLayoutPanel.Controls.Add(this.fastColoredTextBox, 0, 5);
      this.m_TableLayoutPanel.Controls.Add(this.fastColoredTextBox10, 5, 5);
      this.m_TableLayoutPanel.Controls.Add(this.fastColoredTextBox01, 4, 6);
      this.m_TableLayoutPanel.Controls.Add(this.fastColoredTextBox11, 5, 6);
      this.m_TableLayoutPanel.Controls.Add(this.fastColoredTextBox02, 4, 7);
      this.m_TableLayoutPanel.Controls.Add(this.fastColoredTextBox12, 5, 7);
      this.m_TableLayoutPanel.Controls.Add(this.label1, 3, 5);
      this.m_TableLayoutPanel.Controls.Add(this.fastColoredTextBox00, 4, 5);
      this.m_TableLayoutPanel.Controls.Add(this.label5, 4, 4);
      this.m_TableLayoutPanel.Controls.Add(this.label4, 5, 4);
      this.m_TableLayoutPanel.Controls.Add(this.label6, 0, 4);
      this.m_TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.m_TableLayoutPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      this.m_TableLayoutPanel.RowCount = 9;
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.Size = new System.Drawing.Size(802, 317);
      this.m_TableLayoutPanel.TabIndex = 0;
      // 
      // label5
      // 
      this.label5.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.label5.AutoSize = true;
      this.label5.ForeColor = System.Drawing.Color.Teal;
      this.label5.Location = new System.Drawing.Point(470, 101);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(51, 13);
      this.label5.TabIndex = 35;
      this.label5.Text = "Column 1";
      // 
      // label4
      // 
      this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.label4.AutoSize = true;
      this.label4.ForeColor = System.Drawing.Color.Teal;
      this.label4.Location = new System.Drawing.Point(662, 101);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(51, 13);
      this.label4.TabIndex = 35;
      this.label4.Text = "Column 2";
      // 
      // label6
      // 
      this.label6.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.label6.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.label6, 2);
      this.label6.ForeColor = System.Drawing.Color.Teal;
      this.label6.Location = new System.Drawing.Point(143, 101);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(74, 13);
      this.label6.TabIndex = 35;
      this.label6.Text = "Delimited Text";
      // 
      // QuotingControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.m_TableLayoutPanel);
      this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.MinimumSize = new System.Drawing.Size(498, 0);
      this.Name = "QuotingControl";
      this.Size = new System.Drawing.Size(802, 317);
      ((System.ComponentModel.ISupportInitialize)(this.csvSettingBindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_ErrorProvider)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBox00)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBox12)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBox02)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBox11)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBox01)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBox10)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBox)).EndInit();
      this.m_TableLayoutPanel.ResumeLayout(false);
      this.m_TableLayoutPanel.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    private void QuoteChanged(object? sender, EventArgs e)
    {
      SetTrimming(sender, e);
      QuoteOrDelimiterChange();
    }

    private void QuoteOrDelimiterChange() =>
      this.SafeInvoke(
        () =>
        {
          if (m_CsvFile is null)
            return;
          labelNoQuotes!.Visible = !(m_CsvFile!.ContextSensitiveQualifier
                                     || m_CsvFile.DuplicateQualifierToEscape
                                     || m_CsvFile.EscapePrefixChar != '\0');

          m_ErrorProvider!.SetError(m_TextBoxQuote!, "");

          var quote = m_TextBoxQuote!.Text.WrittenPunctuationToChar();
          var delimiter = m_CsvFile?.FieldDelimiterChar ?? ',';

          if (quote != '\0' && quote != '\'' && quote != '\"')
            m_ErrorProvider.SetError(m_TextBoxQuote, "Unusual Quoting character");

          if (delimiter == quote)
            m_ErrorProvider.SetError(m_TextBoxQuote, "Delimiter and Quote have to be different");

          var delim = (delimiter == '\t') ? m_DelimiterTab : m_Delimiter;
          if (quote == '\0')
          {
            m_ErrorProvider.SetError(fastColoredTextBox10!, "Without quoting a delimiter can not be part of a column");
            m_ErrorProvider.SetError(fastColoredTextBox12!, "Without quoting a linefeed can not be part of a column");
          }
          else
          {
            m_ErrorProvider.SetError(fastColoredTextBox10!, null);
            m_ErrorProvider.SetError(fastColoredTextBox12!, null);
          }

          fastColoredTextBox10!.Text = $"Column with:";
          fastColoredTextBox10.AppendText(delimiter.ToString(), delim);
          fastColoredTextBox10.AppendText(" Delimiter");

          fastColoredTextBox12!.Text = "Column with ";
          fastColoredTextBox12.AppendText("¶", m_Pilcrow);
          fastColoredTextBox12.AppendText("\r\nLinefeed");

          fastColoredTextBox11!.Text = "Column with: ";
          fastColoredTextBox11.AppendText(quote.ToString(), m_Quote);
          fastColoredTextBox11.AppendText(" Quote");

          fastColoredTextBox!.Clear();
          var newToolTip = m_IsWriteSetting
                             ? "Start the column with a quote, if a quote is part of the text the quote is replaced with a placeholder."
                             : "If the placeholder is part of the text it will be replaced with the quoting character.";

          fastColoredTextBox.AppendText(quote.ToString(), m_Quote);
          fastColoredTextBox.AppendText("This is ");
          fastColoredTextBox.AppendText(quote.ToString(), m_Quote);
          fastColoredTextBox.AppendText(delimiter.ToString(), delim);
          fastColoredTextBox.AppendText(quote.ToString(), m_Quote);
          fastColoredTextBox.AppendText("Column with:");
          fastColoredTextBox.AppendText(delimiter.ToString(), delim);
          fastColoredTextBox.AppendText(" Delimiter");
          fastColoredTextBox.AppendText(quote.ToString(), m_Quote);
          fastColoredTextBox.AppendText("¶", m_Pilcrow);
          fastColoredTextBox.AppendText("\r\n");
          fastColoredTextBox.AppendText(quote.ToString(), m_Quote);
          fastColoredTextBox.AppendText(" a Trimming ");
          fastColoredTextBox.AppendText(quote.ToString(), m_Quote);
          fastColoredTextBox.AppendText(delimiter.ToString(), delim);
          fastColoredTextBox.AppendText(quote.ToString(), m_Quote);
          fastColoredTextBox.AppendText("Column with: ");

          if (string.IsNullOrEmpty(m_TextBoxQuotePlaceHolder!.Text) && !m_CsvFile!.DuplicateQualifierToEscape && string.IsNullOrEmpty(m_TextBoxEscape!.Text) &&
              !m_CsvFile.ContextSensitiveQualifier)
            m_ErrorProvider.SetError(fastColoredTextBox11,
              "The contained quote would lead to closing of the column unless placeholder, repeated quotes or context sensitive quoting is used.");
          else
            m_ErrorProvider.SetError(fastColoredTextBox11, null);

          if (!string.IsNullOrEmpty(m_TextBoxQuotePlaceHolder.Text) && quote != '\0')
          {
            newToolTip += m_IsWriteSetting
                            ? $"\r\nhello {quote} world ->{quote}hello {m_TextBoxQuotePlaceHolder.Text} world{quote}"
                            : $"\r\n{quote}hello {m_TextBoxQuotePlaceHolder.Text} world{quote} -> hello {quote} world";
            fastColoredTextBox.AppendText(m_TextBoxQuotePlaceHolder.Text, m_EscapedQuote);
          }
          else
          {
            if (m_CsvFile!.DuplicateQualifierToEscape)
              fastColoredTextBox.AppendText(new string(quote, 2), m_EscapedQuote);
            else if (m_CsvFile.ContextSensitiveQualifier || string.IsNullOrEmpty(m_TextBoxEscape!.Text))
              fastColoredTextBox.AppendText(new string(quote, 1), m_EscapedQuote);
            else
              fastColoredTextBox.AppendText(m_TextBoxEscape.Text + quote, m_EscapedQuote);
          }

          fastColoredTextBox.AppendText(" Quote");
          fastColoredTextBox.AppendText(quote.ToString(), m_Quote);
          fastColoredTextBox.AppendText("¶", m_Pilcrow);
          fastColoredTextBox.AppendText("\r\nExample ");
          fastColoredTextBox.AppendText(delimiter.ToString(), delim);
          fastColoredTextBox.AppendText(quote.ToString(), m_Quote);
          fastColoredTextBox.AppendText("Column with ");
          fastColoredTextBox.AppendText("¶", m_Pilcrow);
          fastColoredTextBox.AppendText("\r\nLinefeed");
          fastColoredTextBox.AppendText(quote.ToString(), m_Quote);

          fastColoredTextBox.Range.SetStyle(m_Space, m_SpaceRegex);
          fastColoredTextBox10.Range.SetStyle(m_Space, m_SpaceRegex);
          fastColoredTextBox11.Range.SetStyle(m_Space, m_SpaceRegex);
          fastColoredTextBox12.Range.SetStyle(m_Space, m_SpaceRegex);

          m_ToolTip!.SetToolTip(m_TextBoxQuotePlaceHolder, newToolTip);
        });

    private void SetCboTrim(TrimmingOption trim) =>
      comboBoxTrim!.SafeInvokeNoHandleNeeded(
        () =>
        {
          foreach (var ite in comboBoxTrim!.Items)
          {
            var item = (DisplayItem<int>) ite;
            if (item.ID != (int) trim) continue;
            comboBoxTrim.SelectedItem = ite;
            break;
          }
        });

    private void SettingPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(CsvFile.TrimmingOption))
        SetCboTrim(m_CsvFile!.TrimmingOption);
    }

    private void SetTrimming(object? sender, EventArgs e) =>
      this.SafeInvoke(
        () =>
        {
          Contract.Requires(comboBoxTrim != null);
          if (comboBoxTrim!.SelectedItem is null)
            return;

          checkBoxAlternateQuoting!.Enabled = !string.IsNullOrEmpty(m_TextBoxQuote!.Text);

          switch (((DisplayItem<int>) comboBoxTrim.SelectedItem).ID)
          {
            case 1:
              m_CsvFile!.TrimmingOption = TrimmingOption.Unquoted;
              m_LabelInfoQuoting!.Text =
                "Import of leading or training spaces possible, but the field has to be quoted";
              break;

            case 3:
              m_CsvFile!.TrimmingOption = TrimmingOption.All;
              break;

            default:
              m_CsvFile!.TrimmingOption = TrimmingOption.None;
              m_LabelInfoQuoting!.Text = "Leading or training spaces will stay as they are";
              break;
          }

          fastColoredTextBox00!.Text = "This is";
          if (m_CsvFile.TrimmingOption == TrimmingOption.Unquoted || m_CsvFile.TrimmingOption == TrimmingOption.None)
            fastColoredTextBox00.AppendText(" ");

          fastColoredTextBox01!.Clear();
          if (m_CsvFile.TrimmingOption == TrimmingOption.Unquoted || m_CsvFile.TrimmingOption == TrimmingOption.None)
            fastColoredTextBox01.AppendText(" ");
          fastColoredTextBox01.AppendText("a Trimming");
          if (m_CsvFile.TrimmingOption == TrimmingOption.Unquoted || m_CsvFile.TrimmingOption == TrimmingOption.None)
            fastColoredTextBox01.AppendText(" ");

          fastColoredTextBox02!.Text = "Example";
          if (m_CsvFile.TrimmingOption == TrimmingOption.None)
            fastColoredTextBox02.AppendText(" ");

          fastColoredTextBox00.Range.SetStyle(m_Space, m_SpaceRegex);
          fastColoredTextBox01.Range.SetStyle(m_Space, m_SpaceRegex);
          fastColoredTextBox02.Range.SetStyle(m_Space, m_SpaceRegex);
        });

    private void UpdateUI()
    {
      SetCboTrim(m_CsvFile?.TrimmingOption ?? TrimmingOption.Unquoted);
      QuoteOrDelimiterChange();
      SetTrimming(null, EventArgs.Empty);
    }
  }
}