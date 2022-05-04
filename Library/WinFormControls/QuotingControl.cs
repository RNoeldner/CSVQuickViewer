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
    private CheckBox? m_CheckBoxAlternateQuoting;
    private CheckBox? m_CheckBoxDuplicateQuotingToEscape;
    private CheckBox? m_CheckBoxQualifyAlways;
    private CheckBox? m_CheckBoxQualifyOnlyNeeded;
    private ComboBox? m_ComboBoxTrim;
    private IContainer? components;
    private FastColoredTextBox? m_FastColoredTextBox;
    private FastColoredTextBox? m_FastColoredTextBox00;
    private FastColoredTextBox? m_FastColoredTextBox01;
    private FastColoredTextBox? m_FastColoredTextBox02;
    private FastColoredTextBox? m_FastColoredTextBox10;
    private FastColoredTextBox? m_FastColoredTextBox11;
    private FastColoredTextBox? m_FastColoredTextBox12;
    private Label? m_Label1;
    private Label? m_Label2;
    private Label? m_Label3;
    private Label? m_Label4;
    private Label? m_Label5;
    private Label? m_LabelNoQuotes;
    private ICsvFile? m_CsvFile;

    private ErrorProvider? m_ErrorProvider;

    private BindingSource? m_CSVSettingBindingSource;

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
    private Label? m_Label6;
    private ToolTip? m_ToolTip;

    /// <summary>
    ///   CTOR of QuotingControl
    /// </summary>
    public QuotingControl()
    {
      InitializeComponent();
#pragma warning disable CS8602
      m_ComboBoxTrim.Items.Add(new DisplayItem<int>(0, "None"));
      m_ComboBoxTrim.Items.Add(new DisplayItem<int>(1, "Unquoted"));
      m_ComboBoxTrim.Items.Add(new DisplayItem<int>(3, "All"));
#pragma warning restore CS8602
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

        m_CSVSettingBindingSource!.DataSource = m_CsvFile;

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
        m_ComboBoxTrim!.Enabled = !value;
        m_CheckBoxAlternateQuoting!.Visible = !value;
        m_CheckBoxQualifyAlways!.Visible = value;
        m_CheckBoxQualifyOnlyNeeded!.Visible = value;
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
      this.m_CSVSettingBindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.m_ToolTip = new System.Windows.Forms.ToolTip(this.components);
      this.m_CheckBoxDuplicateQuotingToEscape = new System.Windows.Forms.CheckBox();
      this.m_LabelNoQuotes = new System.Windows.Forms.Label();
      this.m_CheckBoxAlternateQuoting = new System.Windows.Forms.CheckBox();
      this.m_ComboBoxTrim = new System.Windows.Forms.ComboBox();
      this.m_TextBoxQuotePlaceHolder = new System.Windows.Forms.TextBox();
      this.m_TextBoxQuote = new System.Windows.Forms.TextBox();
      this.m_ErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      this.m_FastColoredTextBox00 = new FastColoredTextBoxNS.FastColoredTextBox();
      this.m_Label1 = new System.Windows.Forms.Label();
      this.m_FastColoredTextBox12 = new FastColoredTextBoxNS.FastColoredTextBox();
      this.m_FastColoredTextBox02 = new FastColoredTextBoxNS.FastColoredTextBox();
      this.m_FastColoredTextBox11 = new FastColoredTextBoxNS.FastColoredTextBox();
      this.m_FastColoredTextBox01 = new FastColoredTextBoxNS.FastColoredTextBox();
      this.m_FastColoredTextBox10 = new FastColoredTextBoxNS.FastColoredTextBox();
      this.m_FastColoredTextBox = new FastColoredTextBoxNS.FastColoredTextBox();
      this.m_CheckBoxQualifyAlways = new System.Windows.Forms.CheckBox();
      this.m_CheckBoxQualifyOnlyNeeded = new System.Windows.Forms.CheckBox();
      this.m_LabelInfoQuoting = new System.Windows.Forms.Label();
      this.m_LabelTrim = new System.Windows.Forms.Label();
      this.m_TextBoxEscape = new System.Windows.Forms.TextBox();
      this.m_LabelQuotePlaceholder = new System.Windows.Forms.Label();
      this.m_LabelEscapeCharacter = new System.Windows.Forms.Label();
      this.m_LabelQuote = new System.Windows.Forms.Label();
      this.m_Label2 = new System.Windows.Forms.Label();
      this.m_Label3 = new System.Windows.Forms.Label();
      this.m_TableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.m_Label5 = new System.Windows.Forms.Label();
      this.m_Label4 = new System.Windows.Forms.Label();
      this.m_Label6 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.m_CSVSettingBindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_ErrorProvider)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_FastColoredTextBox00)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_FastColoredTextBox12)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_FastColoredTextBox02)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_FastColoredTextBox11)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_FastColoredTextBox01)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_FastColoredTextBox10)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_FastColoredTextBox)).BeginInit();
      this.m_TableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // m_CSVSettingBindingSource
      // 
      this.m_CSVSettingBindingSource.AllowNew = false;
      this.m_CSVSettingBindingSource.DataSource = typeof(CsvTools.CsvFile);
      // 
      // m_CheckBoxDuplicateQuotingToEscape
      // 
      this.m_CheckBoxDuplicateQuotingToEscape.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_CheckBoxDuplicateQuotingToEscape, 2);
      this.m_CheckBoxDuplicateQuotingToEscape.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.m_CSVSettingBindingSource, "DuplicateQualifierToEscape", true));
      this.m_CheckBoxDuplicateQuotingToEscape.Location = new System.Drawing.Point(378, 26);
      this.m_CheckBoxDuplicateQuotingToEscape.Margin = new System.Windows.Forms.Padding(2);
      this.m_CheckBoxDuplicateQuotingToEscape.Name = "m_CheckBoxDuplicateQuotingToEscape";
      this.m_CheckBoxDuplicateQuotingToEscape.Size = new System.Drawing.Size(134, 17);
      this.m_CheckBoxDuplicateQuotingToEscape.TabIndex = 27;
      this.m_CheckBoxDuplicateQuotingToEscape.Text = "Repeated Qualification";
      this.m_ToolTip.SetToolTip(this.m_CheckBoxDuplicateQuotingToEscape, "Assume a repeated quote in a qualified text represent a quote that does not end t" +
        "ext qualification");
      this.m_CheckBoxDuplicateQuotingToEscape.UseVisualStyleBackColor = true;
      this.m_CheckBoxDuplicateQuotingToEscape.CheckedChanged += new System.EventHandler(this.QuoteChanged);
      // 
      // m_LabelNoQuotes
      // 
      this.m_LabelNoQuotes.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_LabelNoQuotes.AutoSize = true;
      this.m_LabelNoQuotes.BackColor = System.Drawing.SystemColors.Info;
      this.m_LabelNoQuotes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.m_LabelNoQuotes.ForeColor = System.Drawing.SystemColors.InfoText;
      this.m_LabelNoQuotes.Location = new System.Drawing.Point(608, 16);
      this.m_LabelNoQuotes.Name = "m_LabelNoQuotes";
      this.m_TableLayoutPanel.SetRowSpan(this.m_LabelNoQuotes, 2);
      this.m_LabelNoQuotes.Size = new System.Drawing.Size(146, 15);
      this.m_LabelNoQuotes.TabIndex = 28;
      this.m_LabelNoQuotes.Text = "Text can not contain qualifier";
      this.m_ToolTip.SetToolTip(this.m_LabelNoQuotes, "Either “Context Sensitive Quoting”, “Repeated Quotes” or an “Escape Character” ne" +
        "ed to be defined to allow a quote to be part of the text");
      // 
      // m_CheckBoxAlternateQuoting
      // 
      this.m_CheckBoxAlternateQuoting.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_CheckBoxAlternateQuoting.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_CheckBoxAlternateQuoting, 2);
      this.m_CheckBoxAlternateQuoting.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.m_CSVSettingBindingSource, "ContextSensitiveQualifier", true));
      this.m_CheckBoxAlternateQuoting.Location = new System.Drawing.Point(379, 3);
      this.m_CheckBoxAlternateQuoting.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_CheckBoxAlternateQuoting.Name = "m_CheckBoxAlternateQuoting";
      this.m_CheckBoxAlternateQuoting.Size = new System.Drawing.Size(169, 17);
      this.m_CheckBoxAlternateQuoting.TabIndex = 2;
      this.m_CheckBoxAlternateQuoting.Text = "Context Sensitive Qualification";
      this.m_ToolTip.SetToolTip(this.m_CheckBoxAlternateQuoting, "A quote is only regarded as closing quote if it is followed by linefeed or delimi" +
        "ter\r\nThis is a uncommon way of quoting but allows to parse incorrectly quoted fi" +
        "les");
      this.m_CheckBoxAlternateQuoting.UseVisualStyleBackColor = true;
      this.m_CheckBoxAlternateQuoting.Visible = false;
      this.m_CheckBoxAlternateQuoting.CheckedChanged += new System.EventHandler(this.QuoteChanged);
      // 
      // m_ComboBoxTrim
      // 
      this.m_ComboBoxTrim.DisplayMember = "Display";
      this.m_ComboBoxTrim.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.m_ComboBoxTrim.Location = new System.Drawing.Point(104, 74);
      this.m_ComboBoxTrim.Margin = new System.Windows.Forms.Padding(3, 2, 3, 3);
      this.m_ComboBoxTrim.Name = "m_ComboBoxTrim";
      this.m_ComboBoxTrim.Size = new System.Drawing.Size(96, 21);
      this.m_ComboBoxTrim.TabIndex = 10;
      this.m_ToolTip.SetToolTip(this.m_ComboBoxTrim, "None will preserve whitespace; Unquoted will remove white spaces if the column wa" +
        "s not quoted; All will remove white spaces even if the column was quoted");
      this.m_ComboBoxTrim.ValueMember = "ID";
      this.m_ComboBoxTrim.SelectedIndexChanged += new System.EventHandler(this.SetTrimming);
      // 
      // m_TextBoxQuotePlaceHolder
      // 
      this.m_TextBoxQuotePlaceHolder.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
      this.m_TextBoxQuotePlaceHolder.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.m_CSVSettingBindingSource, "QualifierPlaceholder", true));
      this.m_TextBoxQuotePlaceHolder.Location = new System.Drawing.Point(104, 50);
      this.m_TextBoxQuotePlaceHolder.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_TextBoxQuotePlaceHolder.Name = "m_TextBoxQuotePlaceHolder";
      this.m_TextBoxQuotePlaceHolder.Size = new System.Drawing.Size(96, 20);
      this.m_TextBoxQuotePlaceHolder.TabIndex = 8;
      this.m_ToolTip.SetToolTip(this.m_TextBoxQuotePlaceHolder, "If this placeholder is part of the text it will be replaced with the quoting char" +
        "acter");
      this.m_TextBoxQuotePlaceHolder.TextChanged += new System.EventHandler(this.QuoteChanged);
      // 
      // m_TextBoxQuote
      // 
      this.m_TextBoxQuote.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.m_CSVSettingBindingSource, "FieldQualifier", true));
      this.m_TextBoxQuote.Location = new System.Drawing.Point(104, 2);
      this.m_TextBoxQuote.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_TextBoxQuote.Name = "m_TextBoxQuote";
      this.m_TextBoxQuote.Size = new System.Drawing.Size(96, 20);
      this.m_TextBoxQuote.TabIndex = 1;
      this.m_ToolTip.SetToolTip(this.m_TextBoxQuote, "Columns may be qualified with a character; usually these are \" the quotes are rem" +
        "oved by the reading applications.");
      this.m_TextBoxQuote.TextChanged += new System.EventHandler(this.QuoteChanged);
      // 
      // m_ErrorProvider
      // 
      this.m_ErrorProvider.ContainerControl = this;
      // 
      // m_FastColoredTextBox00
      // 
      this.m_FastColoredTextBox00.AutoCompleteBracketsList = new char[] {
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
      this.m_FastColoredTextBox00.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(" +
    "?<range>:)\\s*(?<range>[^;]+);";
      this.m_FastColoredTextBox00.AutoScrollMinSize = new System.Drawing.Size(66, 14);
      this.m_FastColoredTextBox00.BackBrush = null;
      this.m_FastColoredTextBox00.CharHeight = 14;
      this.m_FastColoredTextBox00.CharWidth = 8;
      this.m_FastColoredTextBox00.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.m_FastColoredTextBox00.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.m_FastColoredTextBox00.IsReplaceMode = false;
      this.m_FastColoredTextBox00.Location = new System.Drawing.Point(421, 121);
      this.m_FastColoredTextBox00.Multiline = false;
      this.m_FastColoredTextBox00.Name = "m_FastColoredTextBox00";
      this.m_FastColoredTextBox00.Paddings = new System.Windows.Forms.Padding(0);
      this.m_FastColoredTextBox00.ReadOnly = true;
      this.m_FastColoredTextBox00.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.m_FastColoredTextBox00.ServiceColors = null;
      this.m_FastColoredTextBox00.ShowLineNumbers = false;
      this.m_FastColoredTextBox00.ShowScrollBars = false;
      this.m_FastColoredTextBox00.Size = new System.Drawing.Size(181, 18);
      this.m_FastColoredTextBox00.TabIndex = 29;
      this.m_FastColoredTextBox00.Text = "This is ";
      this.m_FastColoredTextBox00.Zoom = 100;
      // 
      // m_Label1
      // 
      this.m_Label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_Label1.AutoSize = true;
      this.m_Label1.ForeColor = System.Drawing.Color.Teal;
      this.m_Label1.Location = new System.Drawing.Point(379, 118);
      this.m_Label1.Name = "m_Label1";
      this.m_Label1.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.m_Label1.Size = new System.Drawing.Size(36, 16);
      this.m_Label1.TabIndex = 35;
      this.m_Label1.Text = "Rec 1";
      // 
      // m_FastColoredTextBox12
      // 
      this.m_FastColoredTextBox12.AutoCompleteBracketsList = new char[] {
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
      this.m_FastColoredTextBox12.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(" +
    "?<range>:)\\s*(?<range>[^;]+);";
      this.m_FastColoredTextBox12.AutoScrollMinSize = new System.Drawing.Size(106, 28);
      this.m_FastColoredTextBox12.BackBrush = null;
      this.m_FastColoredTextBox12.CharHeight = 14;
      this.m_FastColoredTextBox12.CharWidth = 8;
      this.m_FastColoredTextBox12.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.m_FastColoredTextBox12.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.m_FastColoredTextBox12.IsReplaceMode = false;
      this.m_FastColoredTextBox12.Location = new System.Drawing.Point(608, 169);
      this.m_FastColoredTextBox12.Name = "m_FastColoredTextBox12";
      this.m_FastColoredTextBox12.Paddings = new System.Windows.Forms.Padding(0);
      this.m_FastColoredTextBox12.ReadOnly = true;
      this.m_FastColoredTextBox12.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.m_FastColoredTextBox12.ServiceColors = null;
      this.m_FastColoredTextBox12.ShowLineNumbers = false;
      this.m_FastColoredTextBox12.Size = new System.Drawing.Size(191, 31);
      this.m_FastColoredTextBox12.TabIndex = 34;
      this.m_FastColoredTextBox12.Text = "Column with ¶\r\nLinefeed";
      this.m_FastColoredTextBox12.Zoom = 100;
      // 
      // m_FastColoredTextBox02
      // 
      this.m_FastColoredTextBox02.AutoCompleteBracketsList = new char[] {
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
      this.m_FastColoredTextBox02.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(" +
    "?<range>:)\\s*(?<range>[^;]+);";
      this.m_FastColoredTextBox02.AutoScrollMinSize = new System.Drawing.Size(66, 14);
      this.m_FastColoredTextBox02.BackBrush = null;
      this.m_FastColoredTextBox02.CharHeight = 14;
      this.m_FastColoredTextBox02.CharWidth = 8;
      this.m_FastColoredTextBox02.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.m_FastColoredTextBox02.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.m_FastColoredTextBox02.IsReplaceMode = false;
      this.m_FastColoredTextBox02.Location = new System.Drawing.Point(421, 169);
      this.m_FastColoredTextBox02.Multiline = false;
      this.m_FastColoredTextBox02.Name = "m_FastColoredTextBox02";
      this.m_FastColoredTextBox02.Paddings = new System.Windows.Forms.Padding(0);
      this.m_FastColoredTextBox02.ReadOnly = true;
      this.m_FastColoredTextBox02.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.m_FastColoredTextBox02.ServiceColors = null;
      this.m_FastColoredTextBox02.ShowLineNumbers = false;
      this.m_FastColoredTextBox02.ShowScrollBars = false;
      this.m_FastColoredTextBox02.Size = new System.Drawing.Size(181, 18);
      this.m_FastColoredTextBox02.TabIndex = 33;
      this.m_FastColoredTextBox02.Text = "Example ";
      this.m_FastColoredTextBox02.Zoom = 100;
      // 
      // m_FastColoredTextBox11
      // 
      this.m_FastColoredTextBox11.AutoCompleteBracketsList = new char[] {
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
      this.m_FastColoredTextBox11.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(" +
    "?<range>:)\\s*(?<range>[^;]+);";
      this.m_FastColoredTextBox11.AutoScrollMinSize = new System.Drawing.Size(154, 14);
      this.m_FastColoredTextBox11.BackBrush = null;
      this.m_FastColoredTextBox11.CharHeight = 14;
      this.m_FastColoredTextBox11.CharWidth = 8;
      this.m_FastColoredTextBox11.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.m_FastColoredTextBox11.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.m_FastColoredTextBox11.IsReplaceMode = false;
      this.m_FastColoredTextBox11.Location = new System.Drawing.Point(608, 145);
      this.m_FastColoredTextBox11.Multiline = false;
      this.m_FastColoredTextBox11.Name = "m_FastColoredTextBox11";
      this.m_FastColoredTextBox11.Paddings = new System.Windows.Forms.Padding(0);
      this.m_FastColoredTextBox11.ReadOnly = true;
      this.m_FastColoredTextBox11.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.m_FastColoredTextBox11.ServiceColors = null;
      this.m_FastColoredTextBox11.ShowLineNumbers = false;
      this.m_FastColoredTextBox11.ShowScrollBars = false;
      this.m_FastColoredTextBox11.Size = new System.Drawing.Size(191, 18);
      this.m_FastColoredTextBox11.TabIndex = 32;
      this.m_FastColoredTextBox11.Text = "Column with \" Quote";
      this.m_FastColoredTextBox11.Zoom = 100;
      // 
      // m_FastColoredTextBox01
      // 
      this.m_FastColoredTextBox01.AutoCompleteBracketsList = new char[] {
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
      this.m_FastColoredTextBox01.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(" +
    "?<range>:)\\s*(?<range>[^;]+);";
      this.m_FastColoredTextBox01.AutoScrollMinSize = new System.Drawing.Size(98, 14);
      this.m_FastColoredTextBox01.BackBrush = null;
      this.m_FastColoredTextBox01.CharHeight = 14;
      this.m_FastColoredTextBox01.CharWidth = 8;
      this.m_FastColoredTextBox01.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.m_FastColoredTextBox01.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.m_FastColoredTextBox01.IsReplaceMode = false;
      this.m_FastColoredTextBox01.Location = new System.Drawing.Point(421, 145);
      this.m_FastColoredTextBox01.Multiline = false;
      this.m_FastColoredTextBox01.Name = "m_FastColoredTextBox01";
      this.m_FastColoredTextBox01.Paddings = new System.Windows.Forms.Padding(0);
      this.m_FastColoredTextBox01.ReadOnly = true;
      this.m_FastColoredTextBox01.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.m_FastColoredTextBox01.ServiceColors = null;
      this.m_FastColoredTextBox01.ShowLineNumbers = false;
      this.m_FastColoredTextBox01.ShowScrollBars = false;
      this.m_FastColoredTextBox01.Size = new System.Drawing.Size(181, 18);
      this.m_FastColoredTextBox01.TabIndex = 31;
      this.m_FastColoredTextBox01.Text = " a Trimming ";
      this.m_FastColoredTextBox01.Zoom = 100;
      // 
      // m_FastColoredTextBox10
      // 
      this.m_FastColoredTextBox10.AutoCompleteBracketsList = new char[] {
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
      this.m_FastColoredTextBox10.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(" +
    "?<range>:)\\s*(?<range>[^;]+);";
      this.m_FastColoredTextBox10.AutoScrollMinSize = new System.Drawing.Size(186, 14);
      this.m_FastColoredTextBox10.BackBrush = null;
      this.m_FastColoredTextBox10.CharHeight = 14;
      this.m_FastColoredTextBox10.CharWidth = 8;
      this.m_FastColoredTextBox10.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.m_FastColoredTextBox10.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.m_FastColoredTextBox10.Font = new System.Drawing.Font("Courier New", 9.75F);
      this.m_FastColoredTextBox10.IsReplaceMode = false;
      this.m_FastColoredTextBox10.Location = new System.Drawing.Point(608, 121);
      this.m_FastColoredTextBox10.Multiline = false;
      this.m_FastColoredTextBox10.Name = "m_FastColoredTextBox10";
      this.m_FastColoredTextBox10.Paddings = new System.Windows.Forms.Padding(0);
      this.m_FastColoredTextBox10.ReadOnly = true;
      this.m_FastColoredTextBox10.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.m_FastColoredTextBox10.ServiceColors = null;
      this.m_FastColoredTextBox10.ShowLineNumbers = false;
      this.m_FastColoredTextBox10.ShowScrollBars = false;
      this.m_FastColoredTextBox10.Size = new System.Drawing.Size(191, 18);
      this.m_FastColoredTextBox10.TabIndex = 30;
      this.m_FastColoredTextBox10.TabLength = 1;
      this.m_FastColoredTextBox10.TabStop = false;
      this.m_FastColoredTextBox10.Text = "Column with:, Delimiter";
      this.m_FastColoredTextBox10.Zoom = 100;
      // 
      // m_FastColoredTextBox
      // 
      this.m_FastColoredTextBox.AutoCompleteBracketsList = new char[] {
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
      this.m_FastColoredTextBox.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(" +
    "?<range>:)\\s*(?<range>[^;]+);";
      this.m_FastColoredTextBox.AutoScrollMinSize = new System.Drawing.Size(307, 56);
      this.m_FastColoredTextBox.BackBrush = null;
      this.m_FastColoredTextBox.CharHeight = 14;
      this.m_FastColoredTextBox.CharWidth = 8;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_FastColoredTextBox, 2);
      this.m_FastColoredTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.m_FastColoredTextBox.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.m_FastColoredTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_FastColoredTextBox.Font = new System.Drawing.Font("Courier New", 9.75F);
      this.m_FastColoredTextBox.IsReplaceMode = false;
      this.m_FastColoredTextBox.Location = new System.Drawing.Point(3, 121);
      this.m_FastColoredTextBox.Name = "m_FastColoredTextBox";
      this.m_FastColoredTextBox.Paddings = new System.Windows.Forms.Padding(0);
      this.m_FastColoredTextBox.ReadOnly = true;
      this.m_TableLayoutPanel.SetRowSpan(this.m_FastColoredTextBox, 3);
      this.m_FastColoredTextBox.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.m_FastColoredTextBox.ServiceColors = null;
      this.m_FastColoredTextBox.Size = new System.Drawing.Size(370, 87);
      this.m_FastColoredTextBox.TabIndex = 1;
      this.m_FastColoredTextBox.TabLength = 1;
      this.m_FastColoredTextBox.TabStop = false;
      this.m_FastColoredTextBox.Text = "\"This is \";Column with:, Delimiter¶\r\n a Trimming ;Column with \"\" Quote¶\r\nExample " +
    ";\"Column with ¶\r\nLinefeed\"";
      this.m_FastColoredTextBox.Zoom = 100;
      // 
      // m_CheckBoxQualifyAlways
      // 
      this.m_CheckBoxQualifyAlways.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_CheckBoxQualifyAlways.AutoSize = true;
      this.m_CheckBoxQualifyAlways.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.m_CSVSettingBindingSource, "QualifyAlways", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.m_CheckBoxQualifyAlways.Location = new System.Drawing.Point(608, 51);
      this.m_CheckBoxQualifyAlways.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_CheckBoxQualifyAlways.Name = "m_CheckBoxQualifyAlways";
      this.m_CheckBoxQualifyAlways.Size = new System.Drawing.Size(94, 17);
      this.m_CheckBoxQualifyAlways.TabIndex = 3;
      this.m_CheckBoxQualifyAlways.Text = "Qualify Always";
      this.m_CheckBoxQualifyAlways.UseVisualStyleBackColor = true;
      this.m_CheckBoxQualifyAlways.Visible = false;
      // 
      // m_CheckBoxQualifyOnlyNeeded
      // 
      this.m_CheckBoxQualifyOnlyNeeded.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_CheckBoxQualifyOnlyNeeded.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_CheckBoxQualifyOnlyNeeded, 2);
      this.m_CheckBoxQualifyOnlyNeeded.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.m_CSVSettingBindingSource, "QualifyOnlyIfNeeded", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.m_CheckBoxQualifyOnlyNeeded.Location = new System.Drawing.Point(379, 51);
      this.m_CheckBoxQualifyOnlyNeeded.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_CheckBoxQualifyOnlyNeeded.Name = "m_CheckBoxQualifyOnlyNeeded";
      this.m_CheckBoxQualifyOnlyNeeded.Size = new System.Drawing.Size(132, 17);
      this.m_CheckBoxQualifyOnlyNeeded.TabIndex = 4;
      this.m_CheckBoxQualifyOnlyNeeded.Text = "Qualify Only If Needed";
      this.m_CheckBoxQualifyOnlyNeeded.UseVisualStyleBackColor = true;
      this.m_CheckBoxQualifyOnlyNeeded.Visible = false;
      // 
      // m_LabelInfoQuoting
      // 
      this.m_LabelInfoQuoting.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_LabelInfoQuoting.AutoSize = true;
      this.m_LabelInfoQuoting.BackColor = System.Drawing.SystemColors.Info;
      this.m_LabelInfoQuoting.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_LabelInfoQuoting, 3);
      this.m_LabelInfoQuoting.ForeColor = System.Drawing.SystemColors.InfoText;
      this.m_LabelInfoQuoting.Location = new System.Drawing.Point(379, 77);
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
      this.m_TextBoxEscape.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.m_CSVSettingBindingSource, "EscapePrefix", true));
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
      // m_Label2
      // 
      this.m_Label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_Label2.AutoSize = true;
      this.m_Label2.ForeColor = System.Drawing.Color.Teal;
      this.m_Label2.Location = new System.Drawing.Point(379, 142);
      this.m_Label2.Name = "m_Label2";
      this.m_Label2.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.m_Label2.Size = new System.Drawing.Size(36, 16);
      this.m_Label2.TabIndex = 36;
      this.m_Label2.Text = "Rec 2";
      // 
      // m_Label3
      // 
      this.m_Label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_Label3.AutoSize = true;
      this.m_Label3.ForeColor = System.Drawing.Color.Teal;
      this.m_Label3.Location = new System.Drawing.Point(379, 166);
      this.m_Label3.Name = "m_Label3";
      this.m_Label3.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.m_Label3.Size = new System.Drawing.Size(36, 16);
      this.m_Label3.TabIndex = 37;
      this.m_Label3.Text = "Rec 3";
      // 
      // m_TableLayoutPanel
      // 
      this.m_TableLayoutPanel.AutoSize = true;
      this.m_TableLayoutPanel.ColumnCount = 5;
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.m_TableLayoutPanel.Controls.Add(this.m_Label3, 2, 7);
      this.m_TableLayoutPanel.Controls.Add(this.m_Label2, 2, 6);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelQuote, 0, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelEscapeCharacter, 0, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelQuotePlaceholder, 0, 2);
      this.m_TableLayoutPanel.Controls.Add(this.m_TextBoxQuote, 1, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_TextBoxEscape, 1, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_TextBoxQuotePlaceHolder, 1, 2);
      this.m_TableLayoutPanel.Controls.Add(this.m_ComboBoxTrim, 1, 3);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelTrim, 0, 3);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelInfoQuoting, 2, 3);
      this.m_TableLayoutPanel.Controls.Add(this.m_CheckBoxQualifyOnlyNeeded, 2, 2);
      this.m_TableLayoutPanel.Controls.Add(this.m_CheckBoxAlternateQuoting, 2, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelNoQuotes, 4, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_CheckBoxQualifyAlways, 4, 2);
      this.m_TableLayoutPanel.Controls.Add(this.m_CheckBoxDuplicateQuotingToEscape, 2, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_FastColoredTextBox, 0, 5);
      this.m_TableLayoutPanel.Controls.Add(this.m_FastColoredTextBox10, 4, 5);
      this.m_TableLayoutPanel.Controls.Add(this.m_FastColoredTextBox01, 3, 6);
      this.m_TableLayoutPanel.Controls.Add(this.m_FastColoredTextBox11, 4, 6);
      this.m_TableLayoutPanel.Controls.Add(this.m_FastColoredTextBox02, 3, 7);
      this.m_TableLayoutPanel.Controls.Add(this.m_FastColoredTextBox12, 4, 7);
      this.m_TableLayoutPanel.Controls.Add(this.m_Label1, 2, 5);
      this.m_TableLayoutPanel.Controls.Add(this.m_FastColoredTextBox00, 3, 5);
      this.m_TableLayoutPanel.Controls.Add(this.m_Label5, 3, 4);
      this.m_TableLayoutPanel.Controls.Add(this.m_Label4, 4, 4);
      this.m_TableLayoutPanel.Controls.Add(this.m_Label6, 0, 4);
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
      // m_Label5
      // 
      this.m_Label5.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.m_Label5.AutoSize = true;
      this.m_Label5.ForeColor = System.Drawing.Color.Teal;
      this.m_Label5.Location = new System.Drawing.Point(486, 101);
      this.m_Label5.Name = "m_Label5";
      this.m_Label5.Size = new System.Drawing.Size(51, 13);
      this.m_Label5.TabIndex = 35;
      this.m_Label5.Text = "Column 1";
      // 
      // m_Label4
      // 
      this.m_Label4.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.m_Label4.AutoSize = true;
      this.m_Label4.ForeColor = System.Drawing.Color.Teal;
      this.m_Label4.Location = new System.Drawing.Point(678, 101);
      this.m_Label4.Name = "m_Label4";
      this.m_Label4.Size = new System.Drawing.Size(51, 13);
      this.m_Label4.TabIndex = 35;
      this.m_Label4.Text = "Column 2";
      // 
      // m_Label6
      // 
      this.m_Label6.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.m_Label6.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_Label6, 2);
      this.m_Label6.ForeColor = System.Drawing.Color.Teal;
      this.m_Label6.Location = new System.Drawing.Point(151, 101);
      this.m_Label6.Name = "m_Label6";
      this.m_Label6.Size = new System.Drawing.Size(74, 13);
      this.m_Label6.TabIndex = 35;
      this.m_Label6.Text = "Delimited Text";
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
      ((System.ComponentModel.ISupportInitialize)(this.m_CSVSettingBindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_ErrorProvider)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_FastColoredTextBox00)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_FastColoredTextBox12)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_FastColoredTextBox02)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_FastColoredTextBox11)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_FastColoredTextBox01)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_FastColoredTextBox10)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_FastColoredTextBox)).EndInit();
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
          m_LabelNoQuotes!.Visible = !(m_CsvFile!.ContextSensitiveQualifier
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
            m_ErrorProvider.SetError(m_FastColoredTextBox10!, "Without quoting a delimiter can not be part of a column");
            m_ErrorProvider.SetError(m_FastColoredTextBox12!, "Without quoting a linefeed can not be part of a column");
          }
          else
          {
            m_ErrorProvider.SetError(m_FastColoredTextBox10!, null);
            m_ErrorProvider.SetError(m_FastColoredTextBox12!, null);
          }

          m_FastColoredTextBox10!.Text = $"Column with:";
          m_FastColoredTextBox10.AppendText(delimiter.ToString(), delim);
          m_FastColoredTextBox10.AppendText(" Delimiter");

          m_FastColoredTextBox12!.Text = "Column with ";
          m_FastColoredTextBox12.AppendText("¶", m_Pilcrow);
          m_FastColoredTextBox12.AppendText("\r\nLinefeed");

          m_FastColoredTextBox11!.Text = "Column with: ";
          m_FastColoredTextBox11.AppendText(quote.ToString(), m_Quote);
          m_FastColoredTextBox11.AppendText(" Quote");

          m_FastColoredTextBox!.Clear();
          var newToolTip = m_IsWriteSetting
                             ? "Start the column with a quote, if a quote is part of the text the quote is replaced with a placeholder."
                             : "If the placeholder is part of the text it will be replaced with the quoting character.";

          m_FastColoredTextBox.AppendText(quote.ToString(), m_Quote);
          m_FastColoredTextBox.AppendText("This is ");
          m_FastColoredTextBox.AppendText(quote.ToString(), m_Quote);
          m_FastColoredTextBox.AppendText(delimiter.ToString(), delim);
          m_FastColoredTextBox.AppendText(quote.ToString(), m_Quote);
          m_FastColoredTextBox.AppendText("Column with:");
          m_FastColoredTextBox.AppendText(delimiter.ToString(), delim);
          m_FastColoredTextBox.AppendText(" Delimiter");
          m_FastColoredTextBox.AppendText(quote.ToString(), m_Quote);
          m_FastColoredTextBox.AppendText("¶", m_Pilcrow);
          m_FastColoredTextBox.AppendText("\r\n");
          m_FastColoredTextBox.AppendText(quote.ToString(), m_Quote);
          m_FastColoredTextBox.AppendText(" a Trimming ");
          m_FastColoredTextBox.AppendText(quote.ToString(), m_Quote);
          m_FastColoredTextBox.AppendText(delimiter.ToString(), delim);
          m_FastColoredTextBox.AppendText(quote.ToString(), m_Quote);
          m_FastColoredTextBox.AppendText("Column with: ");

          if (string.IsNullOrEmpty(m_TextBoxQuotePlaceHolder!.Text) && !m_CsvFile!.DuplicateQualifierToEscape && string.IsNullOrEmpty(m_TextBoxEscape!.Text) &&
              !m_CsvFile.ContextSensitiveQualifier)
            m_ErrorProvider.SetError(m_FastColoredTextBox11,
              "The contained quote would lead to closing of the column unless placeholder, repeated quotes or context sensitive quoting is used.");
          else
            m_ErrorProvider.SetError(m_FastColoredTextBox11, null);

          if (!string.IsNullOrEmpty(m_TextBoxQuotePlaceHolder.Text) && quote != '\0')
          {
            newToolTip += m_IsWriteSetting
                            ? $"\r\nhello {quote} world ->{quote}hello {m_TextBoxQuotePlaceHolder.Text} world{quote}"
                            : $"\r\n{quote}hello {m_TextBoxQuotePlaceHolder.Text} world{quote} -> hello {quote} world";
            m_FastColoredTextBox.AppendText(m_TextBoxQuotePlaceHolder.Text, m_EscapedQuote);
          }
          else
          {
            if (m_CsvFile!.DuplicateQualifierToEscape)
              m_FastColoredTextBox.AppendText(new string(quote, 2), m_EscapedQuote);
            else if (m_CsvFile.ContextSensitiveQualifier || string.IsNullOrEmpty(m_TextBoxEscape!.Text))
              m_FastColoredTextBox.AppendText(new string(quote, 1), m_EscapedQuote);
            else
              m_FastColoredTextBox.AppendText(m_TextBoxEscape.Text + quote, m_EscapedQuote);
          }

          m_FastColoredTextBox.AppendText(" Quote");
          m_FastColoredTextBox.AppendText(quote.ToString(), m_Quote);
          m_FastColoredTextBox.AppendText("¶", m_Pilcrow);
          m_FastColoredTextBox.AppendText("\r\nExample ");
          m_FastColoredTextBox.AppendText(delimiter.ToString(), delim);
          m_FastColoredTextBox.AppendText(quote.ToString(), m_Quote);
          m_FastColoredTextBox.AppendText("Column with ");
          m_FastColoredTextBox.AppendText("¶", m_Pilcrow);
          m_FastColoredTextBox.AppendText("\r\nLinefeed");
          m_FastColoredTextBox.AppendText(quote.ToString(), m_Quote);

          m_FastColoredTextBox.Range.SetStyle(m_Space, m_SpaceRegex);
          m_FastColoredTextBox10.Range.SetStyle(m_Space, m_SpaceRegex);
          m_FastColoredTextBox11.Range.SetStyle(m_Space, m_SpaceRegex);
          m_FastColoredTextBox12.Range.SetStyle(m_Space, m_SpaceRegex);

          m_ToolTip!.SetToolTip(m_TextBoxQuotePlaceHolder, newToolTip);
        });

    private void SetCboTrim(TrimmingOptionEnum trim) =>
      m_ComboBoxTrim!.SafeInvokeNoHandleNeeded(
        () =>
        {
          foreach (var ite in m_ComboBoxTrim!.Items)
          {
            var item = (DisplayItem<int>) ite;
            if (item.ID != (int) trim) continue;
            m_ComboBoxTrim.SelectedItem = ite;
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
          Contract.Requires(m_ComboBoxTrim != null);
          if (m_ComboBoxTrim!.SelectedItem is null)
            return;

          m_CheckBoxAlternateQuoting!.Enabled = !string.IsNullOrEmpty(m_TextBoxQuote!.Text);

          switch (((DisplayItem<int>) m_ComboBoxTrim.SelectedItem).ID)
          {
            case 1:
              m_CsvFile!.TrimmingOption = TrimmingOptionEnum.Unquoted;
              m_LabelInfoQuoting!.Text =
                "Import of leading or training spaces possible, but the field has to be quoted";
              break;

            case 3:
              m_CsvFile!.TrimmingOption = TrimmingOptionEnum.All;
              break;

            default:
              m_CsvFile!.TrimmingOption = TrimmingOptionEnum.None;
              m_LabelInfoQuoting!.Text = "Leading or training spaces will stay as they are";
              break;
          }

          m_FastColoredTextBox00!.Text = "This is";
          if (m_CsvFile.TrimmingOption == TrimmingOptionEnum.Unquoted || m_CsvFile.TrimmingOption == TrimmingOptionEnum.None)
            m_FastColoredTextBox00.AppendText(" ");

          m_FastColoredTextBox01!.Clear();
          if (m_CsvFile.TrimmingOption == TrimmingOptionEnum.Unquoted || m_CsvFile.TrimmingOption == TrimmingOptionEnum.None)
            m_FastColoredTextBox01.AppendText(" ");
          m_FastColoredTextBox01.AppendText("a Trimming");
          if (m_CsvFile.TrimmingOption == TrimmingOptionEnum.Unquoted || m_CsvFile.TrimmingOption == TrimmingOptionEnum.None)
            m_FastColoredTextBox01.AppendText(" ");

          m_FastColoredTextBox02!.Text = "Example";
          if (m_CsvFile.TrimmingOption == TrimmingOptionEnum.None)
            m_FastColoredTextBox02.AppendText(" ");

          m_FastColoredTextBox00.Range.SetStyle(m_Space, m_SpaceRegex);
          m_FastColoredTextBox01.Range.SetStyle(m_Space, m_SpaceRegex);
          m_FastColoredTextBox02.Range.SetStyle(m_Space, m_SpaceRegex);
        });

    private void UpdateUI()
    {
      SetCboTrim(m_CsvFile?.TrimmingOption ?? TrimmingOptionEnum.Unquoted);
      QuoteOrDelimiterChange();
      SetTrimming(null, EventArgs.Empty);
    }
  }
}