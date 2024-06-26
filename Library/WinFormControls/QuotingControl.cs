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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
// ReSharper disable LocalizableElement
// ReSharper disable StringLiteralTypo
#pragma warning disable CA1416

namespace CsvTools
{
  /// <summary>
  ///   A Control to edit the quoting and visualize the result
  /// </summary>
  public class QuotingControl : UserControl
  {
    private readonly Style m_Delimiter = new TextStyle(Brushes.Blue, null, FontStyle.Regular);

    private readonly Style m_DelimiterTab =
      new SyntaxHighlighterDelimitedText.SyntaxHighlightStyleTab(Pens.Blue, Brushes.AntiqueWhite);

    // ReSharper disable once IdentifierTypo
    private readonly Style m_PilcrowStyle = new TextStyle(Brushes.Orange, null, FontStyle.Bold);
    private readonly Style m_QuoteStyle = new TextStyle(Brushes.Magenta, null, FontStyle.Regular);
    private readonly Style m_EscapedQuoteStyle = new TextStyle(Brushes.Black, Brushes.LightSteelBlue, FontStyle.Regular);

    private readonly Style m_SpaceStyle =
      new SyntaxHighlighterDelimitedText.SyntaxHighlightStyleStyleSpace(Brushes.Blue, Brushes.AntiqueWhite);

    private readonly Regex m_SpaceRegex = new Regex(" ", RegexOptions.Singleline | RegexOptions.Compiled);
    private IContainer components;
    private FastColoredTextBox m_FastColoredTextBox;
    private FastColoredTextBox m_FastColoredTextBox00;
    private FastColoredTextBox m_FastColoredTextBox01;
    private FastColoredTextBox m_FastColoredTextBox02;
    private FastColoredTextBox m_FastColoredTextBox10;
    private FastColoredTextBox m_FastColoredTextBox11;
    private FastColoredTextBox m_FastColoredTextBox12;
    private Label m_Label1;
    private Label m_Label2;
    private Label m_Label3;
    private Label m_Label4;
    private Label m_Label5;
    private ICsvFile m_CsvFile;

    private ErrorProvider? m_ErrorProvider;

    private BindingSource m_CsvSettingBindingSource;

    private bool m_IsWriteSetting;
    private Label m_Label6;
    private TableLayoutPanel m_TableLayoutPanelText;
    private TableLayoutPanel m_TableLayoutPanel;
    private RadioButton m_RadioButtonNeeded;
    private RadioButton m_RadioButtonAlways;
    private Label m_LabelQuote;
    private Label m_LabelQuotePlaceholder;
    private PunctuationTextBox m_TextBoxQuote;
    private TextBox m_TextBoxQuotePlaceHolder;
    private ComboBox m_ComboBoxTrim;
    private Label m_LabelTrim;
    private CheckBox m_CheckBoxAlternateQuoting;
    private CheckBox m_CheckBoxDuplicateQuotingToEscape;
    private TableLayoutPanel m_TableLayoutPanelColumns;
    private SplitContainer m_SplitContainer;
    private Timer m_TimerRebuilt;
    private ToolTip m_ToolTip;
    private bool m_HasChanges;

    /// <summary>
    ///   CTOR of QuotingControl
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public QuotingControl()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
      InitializeComponent();
      m_ComboBoxTrim!.SetEnumDataSource(TrimmingOptionEnum.Unquoted);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Bindable(false)]
    [Browsable(false)]
    public ICsvFile CsvFile
    {
      get { return m_CsvFile; }
      set
      {
        if (m_CsvFile != null)
        {
          if (m_CsvFile is INotifyPropertyChanged notify1)
            notify1.PropertyChanged -= FormatPropertyChanged;
          if (m_CsvFile.Equals(value))
            return;
        }

        m_CsvFile = value;
        if (m_CsvFile is INotifyPropertyChanged notify2)
          notify2.PropertyChanged += FormatPropertyChanged;
        m_CsvSettingBindingSource!.DataSource = m_CsvFile;
        m_CsvSettingBindingSource.ResetBindings(false);
        m_HasChanges = true;
        m_TimerRebuilt.Enabled = true;
      }
    }

    /// <summary>
    ///   In case of a Write only setting things will be hidden
    /// </summary>    
    [DefaultValue(false)]
    [Category("Appearance")]
    [Bindable(true)]
    [Browsable(true)]
    public bool IsWriteSetting
    {
      get => m_IsWriteSetting;
      set
      {
        m_IsWriteSetting = value;
        m_ComboBoxTrim.Visible = !value;
        m_CheckBoxAlternateQuoting.Visible = !value;
        m_LabelTrim.Visible = !value;

        m_RadioButtonAlways.Visible = value;
        m_RadioButtonNeeded.Visible = value;

        if (value)

        {
          m_SplitContainer.Panel2.Controls.Remove(m_TableLayoutPanelColumns);
          m_SplitContainer.Panel1.Controls.Remove(m_TableLayoutPanelText);
          m_SplitContainer.Panel2.Controls.Add(m_TableLayoutPanelText);
          m_SplitContainer.Panel1.Controls.Add(m_TableLayoutPanelColumns);
          m_SplitContainer.SplitterDistance = m_TableLayoutPanelColumns.Width;
        }
        else
        {
          m_SplitContainer.Panel1.Controls.Remove(m_TableLayoutPanelColumns);
          m_SplitContainer.Panel2.Controls.Remove(m_TableLayoutPanelText);
          m_SplitContainer.Panel1.Controls.Add(m_TableLayoutPanelText);
          m_SplitContainer.Panel2.Controls.Add(m_TableLayoutPanelColumns);
          m_SplitContainer.SplitterDistance = m_TableLayoutPanelText.Width;
        }

        m_HasChanges = true;
      }
    }

    private void FormatPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(ICsvFile.EscapePrefixChar) ||
          e.PropertyName == nameof(ICsvFile.FieldQualifierChar) ||
          e.PropertyName == nameof(ICsvFile.FieldDelimiterChar) ||
          e.PropertyName == nameof(ICsvFile.TrimmingOption) ||
          e.PropertyName == nameof(ICsvFile.ContextSensitiveQualifier) ||
          e.PropertyName == nameof(ICsvFile.QualifierPlaceholder) ||
          e.PropertyName == nameof(ICsvFile.QualifyOnlyIfNeeded) ||
          e.PropertyName == nameof(ICsvFile.QualifyAlways) ||
          e.PropertyName == nameof(ICsvFile.DuplicateQualifierToEscape))
        m_HasChanges = true;
    }

    /// <summary>
    ///   Initializes the component.
    /// </summary>
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "RedundantCast")]
    [SuppressMessage("ReSharper", "RedundantDelegateCreation")]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    [SuppressMessage("ReSharper", "RedundantExplicitArrayCreation")]
#pragma warning disable CS8622
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuotingControl));
      this.m_ToolTip = new System.Windows.Forms.ToolTip(this.components);
      this.m_RadioButtonNeeded = new System.Windows.Forms.RadioButton();
      this.m_CsvSettingBindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.m_RadioButtonAlways = new System.Windows.Forms.RadioButton();
      this.m_ComboBoxTrim = new System.Windows.Forms.ComboBox();
      this.m_CheckBoxAlternateQuoting = new System.Windows.Forms.CheckBox();
      this.m_CheckBoxDuplicateQuotingToEscape = new System.Windows.Forms.CheckBox();
      this.m_TextBoxQuote = new CsvTools.PunctuationTextBox();
      this.m_TextBoxQuotePlaceHolder = new System.Windows.Forms.TextBox();
      this.m_ErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      this.m_FastColoredTextBox00 = new FastColoredTextBoxNS.FastColoredTextBox();
      this.m_Label1 = new System.Windows.Forms.Label();
      this.m_FastColoredTextBox12 = new FastColoredTextBoxNS.FastColoredTextBox();
      this.m_FastColoredTextBox02 = new FastColoredTextBoxNS.FastColoredTextBox();
      this.m_FastColoredTextBox11 = new FastColoredTextBoxNS.FastColoredTextBox();
      this.m_FastColoredTextBox01 = new FastColoredTextBoxNS.FastColoredTextBox();
      this.m_FastColoredTextBox10 = new FastColoredTextBoxNS.FastColoredTextBox();
      this.m_FastColoredTextBox = new FastColoredTextBoxNS.FastColoredTextBox();
      this.m_Label2 = new System.Windows.Forms.Label();
      this.m_Label3 = new System.Windows.Forms.Label();
      this.m_Label5 = new System.Windows.Forms.Label();
      this.m_Label4 = new System.Windows.Forms.Label();
      this.m_Label6 = new System.Windows.Forms.Label();
      this.m_TableLayoutPanelText = new System.Windows.Forms.TableLayoutPanel();
      this.m_TableLayoutPanelColumns = new System.Windows.Forms.TableLayoutPanel();
      this.m_TableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.m_LabelQuote = new System.Windows.Forms.Label();
      this.m_LabelQuotePlaceholder = new System.Windows.Forms.Label();
      this.m_LabelTrim = new System.Windows.Forms.Label();
      this.m_SplitContainer = new System.Windows.Forms.SplitContainer();
      this.m_TimerRebuilt = new System.Windows.Forms.Timer(this.components);
      ((System.ComponentModel.ISupportInitialize) (this.m_CsvSettingBindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (this.m_ErrorProvider)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (this.m_FastColoredTextBox00)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (this.m_FastColoredTextBox12)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (this.m_FastColoredTextBox02)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (this.m_FastColoredTextBox11)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (this.m_FastColoredTextBox01)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (this.m_FastColoredTextBox10)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (this.m_FastColoredTextBox)).BeginInit();
      this.m_TableLayoutPanelText.SuspendLayout();
      this.m_TableLayoutPanelColumns.SuspendLayout();
      this.m_TableLayoutPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) (this.m_SplitContainer)).BeginInit();
      this.m_SplitContainer.Panel1.SuspendLayout();
      this.m_SplitContainer.Panel2.SuspendLayout();
      this.m_SplitContainer.SuspendLayout();
      this.SuspendLayout();
      // 
      // m_RadioButtonNeeded
      // 
      this.m_RadioButtonNeeded.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_RadioButtonNeeded.AutoSize = true;
      this.m_RadioButtonNeeded.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.m_CsvSettingBindingSource, nameof(ICsvFile.QualifyOnlyIfNeeded), true));
      this.m_RadioButtonNeeded.Location = new System.Drawing.Point(372, 3);
      this.m_RadioButtonNeeded.Name = "m_RadioButtonNeeded";
      this.m_RadioButtonNeeded.Size = new System.Drawing.Size(131, 17);
      this.m_RadioButtonNeeded.TabIndex = 3;
      this.m_RadioButtonNeeded.TabStop = true;
      this.m_RadioButtonNeeded.Text = "Qualify Only If Needed";
      this.m_ToolTip.SetToolTip(this.m_RadioButtonNeeded, "Writing Text the content is quoted only if it’s necessary ");
      this.m_RadioButtonNeeded.UseVisualStyleBackColor = true;
      this.m_RadioButtonNeeded.Visible = false;
      // 
      // m_CsvSettingBindingSource
      // 
      this.m_CsvSettingBindingSource.AllowNew = false;
      this.m_CsvSettingBindingSource.DataSource = typeof(CsvTools.ICsvFile);
      // 
      // m_RadioButtonAlways
      // 
      this.m_RadioButtonAlways.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_RadioButtonAlways.AutoSize = true;
      this.m_RadioButtonAlways.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.m_CsvSettingBindingSource, nameof(ICsvFile.QualifyAlways), true));
      this.m_RadioButtonAlways.Location = new System.Drawing.Point(372, 27);
      this.m_RadioButtonAlways.Name = "m_RadioButtonAlways";
      this.m_RadioButtonAlways.Size = new System.Drawing.Size(93, 17);
      this.m_RadioButtonAlways.TabIndex = 7;
      this.m_RadioButtonAlways.TabStop = true;
      this.m_RadioButtonAlways.Text = "Qualify Always";
      this.m_ToolTip.SetToolTip(this.m_RadioButtonAlways, "Writing Text the content is quoted even its is not required");
      this.m_RadioButtonAlways.UseVisualStyleBackColor = true;
      this.m_RadioButtonAlways.Visible = false;
      // 
      // m_ComboBoxTrim
      // 
      this.m_ComboBoxTrim.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_ComboBoxTrim.DataBindings.Add(new System.Windows.Forms.Binding("SelectedValue", this.m_CsvSettingBindingSource, nameof(ICsvFile.TrimmingOption), true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.m_ComboBoxTrim.DisplayMember = "Display";
      this.m_ComboBoxTrim.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.m_ComboBoxTrim.Location = new System.Drawing.Point(95, 50);
      this.m_ComboBoxTrim.Margin = new System.Windows.Forms.Padding(3, 2, 3, 3);
      this.m_ComboBoxTrim.Name = "m_ComboBoxTrim";
      this.m_ComboBoxTrim.Size = new System.Drawing.Size(96, 21);
      this.m_ComboBoxTrim.TabIndex = 8;
      this.m_ToolTip.SetToolTip(this.m_ComboBoxTrim, "None will preserve whitespace; Unquoted will remove white spaces if the column wa" +
        "s not quoted; All will remove white spaces even if the column was quoted");
      this.m_ComboBoxTrim.ValueMember = "ID";
      // 
      // m_CheckBoxAlternateQuoting
      // 
      this.m_CheckBoxAlternateQuoting.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_CheckBoxAlternateQuoting.AutoSize = true;
      this.m_CheckBoxAlternateQuoting.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.m_CsvSettingBindingSource, nameof(ICsvFile.ContextSensitiveQualifier), true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.m_CheckBoxAlternateQuoting.Location = new System.Drawing.Point(197, 3);
      this.m_CheckBoxAlternateQuoting.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_CheckBoxAlternateQuoting.Name = "m_CheckBoxAlternateQuoting";
      this.m_CheckBoxAlternateQuoting.Size = new System.Drawing.Size(169, 17);
      this.m_CheckBoxAlternateQuoting.TabIndex = 2;
      this.m_CheckBoxAlternateQuoting.Text = "Context Sensitive Qualification";
      this.m_ToolTip.SetToolTip(this.m_CheckBoxAlternateQuoting, "This is a uncommon way of quoting but allows to parse incorrectly quoted files, a" +
        " quote is only regarded as closing quote if it is followed by linefeed or delimi" +
        "ter");
      this.m_CheckBoxAlternateQuoting.UseVisualStyleBackColor = true;
      // 
      // m_CheckBoxDuplicateQuotingToEscape
      // 
      this.m_CheckBoxDuplicateQuotingToEscape.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_CheckBoxDuplicateQuotingToEscape.AutoSize = true;
      this.m_CheckBoxDuplicateQuotingToEscape.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.m_CsvSettingBindingSource, nameof(ICsvFile.DuplicateQualifierToEscape), true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.m_CheckBoxDuplicateQuotingToEscape.Location = new System.Drawing.Point(196, 27);
      this.m_CheckBoxDuplicateQuotingToEscape.Margin = new System.Windows.Forms.Padding(2);
      this.m_CheckBoxDuplicateQuotingToEscape.Name = "m_CheckBoxDuplicateQuotingToEscape";
      this.m_CheckBoxDuplicateQuotingToEscape.Size = new System.Drawing.Size(134, 17);
      this.m_CheckBoxDuplicateQuotingToEscape.TabIndex = 6;
      this.m_CheckBoxDuplicateQuotingToEscape.Text = "Repeated Qualification";
      this.m_ToolTip.SetToolTip(this.m_CheckBoxDuplicateQuotingToEscape, "Assume a repeated quote in a qualified text represent a quote that does not end t" +
        "ext qualification, usually either repeated quoting or escaped charters are used." +
        " ");
      this.m_CheckBoxDuplicateQuotingToEscape.UseVisualStyleBackColor = true;
      // 
      // m_TextBoxQuote
      // 
      this.m_TextBoxQuote.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_TextBoxQuote.AutoCompleteCustomSource.AddRange(new string[] {
            "\"",
            "\'"});
      this.m_TextBoxQuote.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
      this.m_TextBoxQuote.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
      this.m_TextBoxQuote.DataBindings.Add(new System.Windows.Forms.Binding("Character", this.m_CsvSettingBindingSource, nameof(ICsvFile.FieldQualifierChar), true));
      this.m_TextBoxQuote.Location = new System.Drawing.Point(95, 2);
      this.m_TextBoxQuote.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_TextBoxQuote.Name = "m_TextBoxQuote";
      this.m_TextBoxQuote.Size = new System.Drawing.Size(96, 20);
      this.m_TextBoxQuote.TabIndex = 1;
      this.m_ToolTip.SetToolTip(this.m_TextBoxQuote, "Columns may be qualified with a character; usually these are \" the quotes are rem" +
        "oved by the reading applications. This is needed in case a line feed or a delimi" +
        "ter is part of the column");
      this.m_TextBoxQuote.Type = CsvTools.PunctuationTextBox.PunctuationType.Qualifier;
      this.m_TextBoxQuote.TextChanged += new System.EventHandler(this.TextBoxQuote_TextChanged);
      // 
      // m_TextBoxQuotePlaceHolder
      // 
      this.m_TextBoxQuotePlaceHolder.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_TextBoxQuotePlaceHolder.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
      this.m_TextBoxQuotePlaceHolder.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.m_CsvSettingBindingSource, nameof(ICsvFile.QualifierPlaceholder), true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.m_TextBoxQuotePlaceHolder.Location = new System.Drawing.Point(95, 26);
      this.m_TextBoxQuotePlaceHolder.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_TextBoxQuotePlaceHolder.Name = "m_TextBoxQuotePlaceHolder";
      this.m_TextBoxQuotePlaceHolder.Size = new System.Drawing.Size(96, 20);
      this.m_TextBoxQuotePlaceHolder.TabIndex = 5;
      // 
      // m_ErrorProvider
      // 
      this.m_ErrorProvider.ContainerControl = this;
      // 
      // m_FastColoredTextBox00
      // 
      this.m_FastColoredTextBox00.AllowDrop = false;
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
      this.m_FastColoredTextBox00.DisabledColor = System.Drawing.Color.FromArgb(((int) (((byte) (100)))), ((int) (((byte) (180)))), ((int) (((byte) (180)))), ((int) (((byte) (180)))));
      this.m_FastColoredTextBox00.IsReplaceMode = false;
      this.m_FastColoredTextBox00.Location = new System.Drawing.Point(45, 16);
      this.m_FastColoredTextBox00.Multiline = false;
      this.m_FastColoredTextBox00.Name = "m_FastColoredTextBox00";
      this.m_FastColoredTextBox00.Paddings = new System.Windows.Forms.Padding(0);
      this.m_FastColoredTextBox00.ReadOnly = true;
      this.m_FastColoredTextBox00.SelectionColor = System.Drawing.Color.FromArgb(((int) (((byte) (60)))), ((int) (((byte) (0)))), ((int) (((byte) (0)))), ((int) (((byte) (255)))));
      this.m_FastColoredTextBox00.ShowLineNumbers = false;
      this.m_FastColoredTextBox00.ShowScrollBars = false;
      this.m_FastColoredTextBox00.Size = new System.Drawing.Size(92, 18);
      this.m_FastColoredTextBox00.TabIndex = 9;
      this.m_FastColoredTextBox00.Text = "This is ";
      this.m_FastColoredTextBox00.Zoom = 100;
      // 
      // m_Label1
      // 
      this.m_Label1.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_Label1.AutoSize = true;
      this.m_Label1.ForeColor = System.Drawing.Color.Teal;
      this.m_Label1.Location = new System.Drawing.Point(3, 13);
      this.m_Label1.Name = "m_Label1";
      this.m_Label1.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.m_Label1.Size = new System.Drawing.Size(36, 16);
      this.m_Label1.TabIndex = 8;
      this.m_Label1.Text = "Rec 1";
      // 
      // m_FastColoredTextBox12
      // 
      this.m_FastColoredTextBox12.AllowDrop = false;
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
      this.m_FastColoredTextBox12.DisabledColor = System.Drawing.Color.FromArgb(((int) (((byte) (100)))), ((int) (((byte) (180)))), ((int) (((byte) (180)))), ((int) (((byte) (180)))));
      this.m_FastColoredTextBox12.IsReplaceMode = false;
      this.m_FastColoredTextBox12.Location = new System.Drawing.Point(143, 64);
      this.m_FastColoredTextBox12.Name = "m_FastColoredTextBox12";
      this.m_FastColoredTextBox12.Paddings = new System.Windows.Forms.Padding(0);
      this.m_FastColoredTextBox12.ReadOnly = true;
      this.m_FastColoredTextBox12.SelectionColor = System.Drawing.Color.FromArgb(((int) (((byte) (60)))), ((int) (((byte) (0)))), ((int) (((byte) (0)))), ((int) (((byte) (255)))));
      this.m_FastColoredTextBox12.ShowLineNumbers = false;
      this.m_FastColoredTextBox12.Size = new System.Drawing.Size(197, 33);
      this.m_FastColoredTextBox12.TabIndex = 5;
      this.m_FastColoredTextBox12.Text = "Column with ¶\r\nLinefeed";
      this.m_FastColoredTextBox12.Zoom = 100;
      // 
      // m_FastColoredTextBox02
      // 
      this.m_FastColoredTextBox02.AllowDrop = false;
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
      this.m_FastColoredTextBox02.DisabledColor = System.Drawing.Color.FromArgb(((int) (((byte) (100)))), ((int) (((byte) (180)))), ((int) (((byte) (180)))), ((int) (((byte) (180)))));
      this.m_FastColoredTextBox02.IsReplaceMode = false;
      this.m_FastColoredTextBox02.Location = new System.Drawing.Point(45, 64);
      this.m_FastColoredTextBox02.Multiline = false;
      this.m_FastColoredTextBox02.Name = "m_FastColoredTextBox02";
      this.m_FastColoredTextBox02.Paddings = new System.Windows.Forms.Padding(0);
      this.m_FastColoredTextBox02.ReadOnly = true;
      this.m_FastColoredTextBox02.SelectionColor = System.Drawing.Color.FromArgb(((int) (((byte) (60)))), ((int) (((byte) (0)))), ((int) (((byte) (0)))), ((int) (((byte) (255)))));
      this.m_FastColoredTextBox02.ShowLineNumbers = false;
      this.m_FastColoredTextBox02.ShowScrollBars = false;
      this.m_FastColoredTextBox02.Size = new System.Drawing.Size(92, 33);
      this.m_FastColoredTextBox02.TabIndex = 4;
      this.m_FastColoredTextBox02.Text = "Example ";
      this.m_FastColoredTextBox02.Zoom = 100;
      // 
      // m_FastColoredTextBox11
      // 
      this.m_FastColoredTextBox11.AllowDrop = false;
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
      this.m_FastColoredTextBox11.DisabledColor = System.Drawing.Color.FromArgb(((int) (((byte) (100)))), ((int) (((byte) (180)))), ((int) (((byte) (180)))), ((int) (((byte) (180)))));
      this.m_FastColoredTextBox11.IsReplaceMode = false;
      this.m_FastColoredTextBox11.Location = new System.Drawing.Point(143, 40);
      this.m_FastColoredTextBox11.Multiline = false;
      this.m_FastColoredTextBox11.Name = "m_FastColoredTextBox11";
      this.m_FastColoredTextBox11.Paddings = new System.Windows.Forms.Padding(0);
      this.m_FastColoredTextBox11.ReadOnly = true;
      this.m_FastColoredTextBox11.SelectionColor = System.Drawing.Color.FromArgb(((int) (((byte) (60)))), ((int) (((byte) (0)))), ((int) (((byte) (0)))), ((int) (((byte) (255)))));
      this.m_FastColoredTextBox11.ShowLineNumbers = false;
      this.m_FastColoredTextBox11.ShowScrollBars = false;
      this.m_FastColoredTextBox11.Size = new System.Drawing.Size(197, 18);
      this.m_FastColoredTextBox11.TabIndex = 2;
      this.m_FastColoredTextBox11.Text = "Column with \" Quote";
      this.m_FastColoredTextBox11.Zoom = 100;
      // 
      // m_FastColoredTextBox01
      // 
      this.m_FastColoredTextBox01.AllowDrop = false;
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
      this.m_FastColoredTextBox01.DisabledColor = System.Drawing.Color.FromArgb(((int) (((byte) (100)))), ((int) (((byte) (180)))), ((int) (((byte) (180)))), ((int) (((byte) (180)))));
      this.m_FastColoredTextBox01.IsReplaceMode = false;
      this.m_FastColoredTextBox01.Location = new System.Drawing.Point(45, 40);
      this.m_FastColoredTextBox01.Multiline = false;
      this.m_FastColoredTextBox01.Name = "m_FastColoredTextBox01";
      this.m_FastColoredTextBox01.Paddings = new System.Windows.Forms.Padding(0);
      this.m_FastColoredTextBox01.ReadOnly = true;
      this.m_FastColoredTextBox01.SelectionColor = System.Drawing.Color.FromArgb(((int) (((byte) (60)))), ((int) (((byte) (0)))), ((int) (((byte) (0)))), ((int) (((byte) (255)))));
      this.m_FastColoredTextBox01.ShowLineNumbers = false;
      this.m_FastColoredTextBox01.ShowScrollBars = false;
      this.m_FastColoredTextBox01.Size = new System.Drawing.Size(92, 18);
      this.m_FastColoredTextBox01.TabIndex = 1;
      this.m_FastColoredTextBox01.Text = " a Trimming ";
      this.m_FastColoredTextBox01.Zoom = 100;
      // 
      // m_FastColoredTextBox10
      // 
      this.m_FastColoredTextBox10.AllowDrop = false;
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
      this.m_FastColoredTextBox10.DisabledColor = System.Drawing.Color.FromArgb(((int) (((byte) (100)))), ((int) (((byte) (180)))), ((int) (((byte) (180)))), ((int) (((byte) (180)))));
      this.m_FastColoredTextBox10.IsReplaceMode = false;
      this.m_FastColoredTextBox10.Location = new System.Drawing.Point(143, 16);
      this.m_FastColoredTextBox10.Multiline = false;
      this.m_FastColoredTextBox10.Name = "m_FastColoredTextBox10";
      this.m_FastColoredTextBox10.Paddings = new System.Windows.Forms.Padding(0);
      this.m_FastColoredTextBox10.ReadOnly = true;
      this.m_FastColoredTextBox10.SelectionColor = System.Drawing.Color.FromArgb(((int) (((byte) (60)))), ((int) (((byte) (0)))), ((int) (((byte) (0)))), ((int) (((byte) (255)))));
      this.m_FastColoredTextBox10.ShowLineNumbers = false;
      this.m_FastColoredTextBox10.ShowScrollBars = false;
      this.m_FastColoredTextBox10.Size = new System.Drawing.Size(197, 18);
      this.m_FastColoredTextBox10.TabIndex = 10;
      this.m_FastColoredTextBox10.TabLength = 1;
      this.m_FastColoredTextBox10.TabStop = false;
      this.m_FastColoredTextBox10.Text = "Column with:, Delimiter";
      this.m_FastColoredTextBox10.Zoom = 100;
      // 
      // m_FastColoredTextBox
      // 
      this.m_FastColoredTextBox.AllowDrop = false;
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
      this.m_FastColoredTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.m_FastColoredTextBox.DisabledColor = System.Drawing.Color.FromArgb(((int) (((byte) (100)))), ((int) (((byte) (180)))), ((int) (((byte) (180)))), ((int) (((byte) (180)))));
      this.m_FastColoredTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_FastColoredTextBox.IsReplaceMode = false;
      this.m_FastColoredTextBox.Location = new System.Drawing.Point(3, 16);
      this.m_FastColoredTextBox.Name = "m_FastColoredTextBox";
      this.m_FastColoredTextBox.Paddings = new System.Windows.Forms.Padding(0);
      this.m_FastColoredTextBox.ReadOnly = true;
      this.m_TableLayoutPanelText.SetRowSpan(this.m_FastColoredTextBox, 3);
      this.m_FastColoredTextBox.SelectionColor = System.Drawing.Color.FromArgb(((int) (((byte) (60)))), ((int) (((byte) (0)))), ((int) (((byte) (0)))), ((int) (((byte) (255)))));
      this.m_FastColoredTextBox.Size = new System.Drawing.Size(362, 95);
      this.m_FastColoredTextBox.TabIndex = 0;
      this.m_FastColoredTextBox.TabLength = 1;
      this.m_FastColoredTextBox.TabStop = false;
      this.m_FastColoredTextBox.Text = "\"This is \";Column with:, Delimiter¶\r\n a Trimming ;Column with \"\" Quote¶\r\nExample " +
    ";\"Column with ¶\r\nLinefeed\"";
      this.m_FastColoredTextBox.Zoom = 100;
      // 
      // m_Label2
      // 
      this.m_Label2.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_Label2.AutoSize = true;
      this.m_Label2.ForeColor = System.Drawing.Color.Teal;
      this.m_Label2.Location = new System.Drawing.Point(3, 37);
      this.m_Label2.Name = "m_Label2";
      this.m_Label2.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.m_Label2.Size = new System.Drawing.Size(36, 16);
      this.m_Label2.TabIndex = 0;
      this.m_Label2.Text = "Rec 2";
      // 
      // m_Label3
      // 
      this.m_Label3.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_Label3.AutoSize = true;
      this.m_Label3.ForeColor = System.Drawing.Color.Teal;
      this.m_Label3.Location = new System.Drawing.Point(3, 61);
      this.m_Label3.Name = "m_Label3";
      this.m_Label3.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.m_Label3.Size = new System.Drawing.Size(36, 16);
      this.m_Label3.TabIndex = 3;
      this.m_Label3.Text = "Rec 3";
      // 
      // m_Label5
      // 
      this.m_Label5.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.m_Label5.AutoSize = true;
      this.m_Label5.ForeColor = System.Drawing.Color.Teal;
      this.m_Label5.Location = new System.Drawing.Point(65, 0);
      this.m_Label5.Name = "m_Label5";
      this.m_Label5.Size = new System.Drawing.Size(51, 13);
      this.m_Label5.TabIndex = 6;
      this.m_Label5.Text = "Column 1";
      // 
      // m_Label4
      // 
      this.m_Label4.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.m_Label4.AutoSize = true;
      this.m_Label4.ForeColor = System.Drawing.Color.Teal;
      this.m_Label4.Location = new System.Drawing.Point(221, 0);
      this.m_Label4.Name = "m_Label4";
      this.m_Label4.Size = new System.Drawing.Size(51, 13);
      this.m_Label4.TabIndex = 7;
      this.m_Label4.Text = "Column 2";
      // 
      // m_Label6
      // 
      this.m_Label6.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.m_Label6.AutoSize = true;
      this.m_Label6.ForeColor = System.Drawing.Color.Teal;
      this.m_Label6.Location = new System.Drawing.Point(147, 0);
      this.m_Label6.Name = "m_Label6";
      this.m_Label6.Size = new System.Drawing.Size(74, 13);
      this.m_Label6.TabIndex = 35;
      this.m_Label6.Text = "Delimited Text";
      // 
      // m_TableLayoutPanelText
      // 
      this.m_TableLayoutPanelText.ColumnCount = 1;
      this.m_TableLayoutPanelText.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 44.49112F));
      this.m_TableLayoutPanelText.Controls.Add(this.m_FastColoredTextBox, 0, 1);
      this.m_TableLayoutPanelText.Controls.Add(this.m_Label6, 0, 0);
      this.m_TableLayoutPanelText.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_TableLayoutPanelText.Location = new System.Drawing.Point(0, 0);
      this.m_TableLayoutPanelText.Name = "m_TableLayoutPanelText";
      this.m_TableLayoutPanelText.RowCount = 4;
      this.m_TableLayoutPanelText.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanelText.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanelText.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanelText.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanelText.Size = new System.Drawing.Size(368, 114);
      this.m_TableLayoutPanelText.TabIndex = 0;
      // 
      // m_TableLayoutPanelColumns
      // 
      this.m_TableLayoutPanelColumns.ColumnCount = 3;
      this.m_TableLayoutPanelColumns.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanelColumns.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanelColumns.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanelColumns.Controls.Add(this.m_Label3, 0, 3);
      this.m_TableLayoutPanelColumns.Controls.Add(this.m_FastColoredTextBox12, 2, 3);
      this.m_TableLayoutPanelColumns.Controls.Add(this.m_FastColoredTextBox02, 1, 3);
      this.m_TableLayoutPanelColumns.Controls.Add(this.m_FastColoredTextBox01, 1, 2);
      this.m_TableLayoutPanelColumns.Controls.Add(this.m_FastColoredTextBox11, 2, 2);
      this.m_TableLayoutPanelColumns.Controls.Add(this.m_FastColoredTextBox00, 1, 1);
      this.m_TableLayoutPanelColumns.Controls.Add(this.m_Label2, 0, 2);
      this.m_TableLayoutPanelColumns.Controls.Add(this.m_FastColoredTextBox10, 2, 1);
      this.m_TableLayoutPanelColumns.Controls.Add(this.m_Label1, 0, 1);
      this.m_TableLayoutPanelColumns.Controls.Add(this.m_Label5, 1, 0);
      this.m_TableLayoutPanelColumns.Controls.Add(this.m_Label4, 2, 0);
      this.m_TableLayoutPanelColumns.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_TableLayoutPanelColumns.Location = new System.Drawing.Point(0, 0);
      this.m_TableLayoutPanelColumns.Name = "m_TableLayoutPanelColumns";
      this.m_TableLayoutPanelColumns.RowCount = 4;
      this.m_TableLayoutPanelColumns.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanelColumns.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanelColumns.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanelColumns.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanelColumns.Size = new System.Drawing.Size(353, 114);
      this.m_TableLayoutPanelColumns.TabIndex = 0;
      // 
      // m_TableLayoutPanel
      // 
      this.m_TableLayoutPanel.AutoSize = true;
      this.m_TableLayoutPanel.ColumnCount = 4;
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelQuote, 0, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_TextBoxQuote, 1, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_CheckBoxAlternateQuoting, 2, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_CheckBoxDuplicateQuotingToEscape, 2, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelQuotePlaceholder, 0, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_TextBoxQuotePlaceHolder, 1, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelTrim, 0, 2);
      this.m_TableLayoutPanel.Controls.Add(this.m_ComboBoxTrim, 1, 2);
      this.m_TableLayoutPanel.Controls.Add(this.m_RadioButtonNeeded, 3, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_RadioButtonAlways, 3, 1);
      this.m_TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.m_TableLayoutPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      this.m_TableLayoutPanel.RowCount = 3;
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.m_TableLayoutPanel.Size = new System.Drawing.Size(726, 74);
      this.m_TableLayoutPanel.TabIndex = 0;
      // 
      // m_LabelQuote
      // 
      this.m_LabelQuote.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_LabelQuote.AutoSize = true;
      this.m_LabelQuote.Location = new System.Drawing.Point(17, 5);
      this.m_LabelQuote.Name = "m_LabelQuote";
      this.m_LabelQuote.Size = new System.Drawing.Size(72, 13);
      this.m_LabelQuote.TabIndex = 0;
      this.m_LabelQuote.Text = "Text Qualifier:";
      // 
      // m_LabelQuotePlaceholder
      // 
      this.m_LabelQuotePlaceholder.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_LabelQuotePlaceholder.AutoSize = true;
      this.m_LabelQuotePlaceholder.Location = new System.Drawing.Point(23, 29);
      this.m_LabelQuotePlaceholder.Name = "m_LabelQuotePlaceholder";
      this.m_LabelQuotePlaceholder.Size = new System.Drawing.Size(66, 13);
      this.m_LabelQuotePlaceholder.TabIndex = 4;
      this.m_LabelQuotePlaceholder.Text = "Placeholder:";
      // 
      // m_LabelTrim
      // 
      this.m_LabelTrim.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_LabelTrim.AutoSize = true;
      this.m_LabelTrim.Location = new System.Drawing.Point(3, 54);
      this.m_LabelTrim.Name = "m_LabelTrim";
      this.m_LabelTrim.Size = new System.Drawing.Size(86, 13);
      this.m_LabelTrim.TabIndex = 7;
      this.m_LabelTrim.Text = "Trimming Option:";
      // 
      // m_SplitContainer
      // 
      this.m_SplitContainer.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_SplitContainer.Location = new System.Drawing.Point(0, 74);
      this.m_SplitContainer.Name = "m_SplitContainer";
      // 
      // m_SplitContainer.Panel1
      // 
      this.m_SplitContainer.Panel1.Controls.Add(this.m_TableLayoutPanelText);
      // 
      // m_SplitContainer.Panel2
      // 
      this.m_SplitContainer.Panel2.Controls.Add(this.m_TableLayoutPanelColumns);
      this.m_SplitContainer.Size = new System.Drawing.Size(726, 114);
      this.m_SplitContainer.SplitterDistance = 368;
      this.m_SplitContainer.SplitterWidth = 5;
      this.m_SplitContainer.TabIndex = 4;
      // 
      // m_TimerRebuilt
      // 
      this.m_TimerRebuilt.Interval = 200;
      this.m_TimerRebuilt.Tick += new System.EventHandler(this.TimerRebuilt_Tick);
      // 
      // QuotingControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.m_SplitContainer);
      this.Controls.Add(this.m_TableLayoutPanel);
      this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.MinimumSize = new System.Drawing.Size(498, 0);
      this.Name = "QuotingControl";
      this.Size = new System.Drawing.Size(726, 220);
      ((System.ComponentModel.ISupportInitialize) (this.m_CsvSettingBindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (this.m_ErrorProvider)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (this.m_FastColoredTextBox00)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (this.m_FastColoredTextBox12)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (this.m_FastColoredTextBox02)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (this.m_FastColoredTextBox11)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (this.m_FastColoredTextBox01)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (this.m_FastColoredTextBox10)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (this.m_FastColoredTextBox)).EndInit();
      this.m_TableLayoutPanelText.ResumeLayout(false);
      this.m_TableLayoutPanelText.PerformLayout();
      this.m_TableLayoutPanelColumns.ResumeLayout(false);
      this.m_TableLayoutPanelColumns.PerformLayout();
      this.m_TableLayoutPanel.ResumeLayout(false);
      this.m_TableLayoutPanel.PerformLayout();
      this.m_SplitContainer.Panel1.ResumeLayout(false);
      this.m_SplitContainer.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize) (this.m_SplitContainer)).EndInit();
      this.m_SplitContainer.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }
#pragma warning restore CS8622

    private void TimerRebuilt_Tick(object sender, EventArgs e)
    {
      if (!m_HasChanges)
        return;
      try
      {
        m_TimerRebuilt.Enabled = false;
        m_CheckBoxAlternateQuoting.Enabled = m_CsvFile.FieldQualifierChar !=char.MinValue;

        var trimming = TrimmingOptionEnum.All;
        if (!m_IsWriteSetting)
        {
          if (m_ComboBoxTrim.SelectedIndex != -1)
#pragma warning disable CS8605
            trimming = (TrimmingOptionEnum) m_ComboBoxTrim.SelectedValue;
#pragma warning restore CS8605        
        }


        m_FastColoredTextBox00!.Text = "This is";
        if (trimming == TrimmingOptionEnum.Unquoted || trimming == TrimmingOptionEnum.None)
          m_FastColoredTextBox00.AppendText(" ");

        m_FastColoredTextBox01!.Clear();
        if (trimming == TrimmingOptionEnum.Unquoted || trimming == TrimmingOptionEnum.None)
          m_FastColoredTextBox01.AppendText(" ");
        m_FastColoredTextBox01.AppendText("a Trimming");
        if (trimming == TrimmingOptionEnum.Unquoted || trimming == TrimmingOptionEnum.None)
          m_FastColoredTextBox01.AppendText(" ");


        m_FastColoredTextBox02!.Text = "Example";
        if (trimming == TrimmingOptionEnum.None)
          m_FastColoredTextBox02.AppendText(" ");


        m_FastColoredTextBox00.Range.SetStyle(m_SpaceStyle, m_SpaceRegex);
        m_FastColoredTextBox01.Range.SetStyle(m_SpaceStyle, m_SpaceRegex);
        m_FastColoredTextBox02.Range.SetStyle(m_SpaceStyle, m_SpaceRegex);

        var canContainQuotes = !string.IsNullOrEmpty(m_CsvFile.QualifierPlaceholder)
                             || m_CsvFile.ContextSensitiveQualifier
                             || m_CsvFile.DuplicateQualifierToEscape
                             || m_CsvFile.EscapePrefixChar != char.MinValue
                             || m_CsvFile.FieldQualifierChar == char.MinValue;
        m_ErrorProvider!.SetError(m_CheckBoxDuplicateQuotingToEscape, canContainQuotes ? null : "Text can not contain qualifier");
        m_ErrorProvider.SetError(m_CheckBoxAlternateQuoting, canContainQuotes ? null : "Text can not contain qualifier");
        m_ErrorProvider.SetError(m_FastColoredTextBox11!, canContainQuotes ? null : "The contained quote would cause closing of the column unless placeholder, repeated quotes or context sensitive quoting is used.");

        var delimiter = m_CsvFile.FieldDelimiterChar;
        var quote = m_CsvFile.FieldQualifierChar;

        m_ErrorProvider.SetError(m_TextBoxQuote, (delimiter == quote) ? "Delimiter and Quote have to be different" : null);

        // ReSharper disable once IdentifierTypo
        var delim = (delimiter == '\t') ? m_DelimiterTab : m_Delimiter;
        m_ErrorProvider.SetError(m_FastColoredTextBox10!, (quote == char.MinValue) ? "Without quoting a delimiter can not be part of a column" : null);
        m_ErrorProvider.SetError(m_FastColoredTextBox12!, (quote == char.MinValue) ? "Without quoting a linefeed can not be part of a column" : null);

        m_FastColoredTextBox10!.Text = "Column with:";
        m_FastColoredTextBox10.AppendText(delimiter.ToStringHandle0(), delim);
        m_FastColoredTextBox10.AppendText(" Delimiter");

        m_FastColoredTextBox12!.Clear();
        m_FastColoredTextBox12.Text = "Column with ";
        m_FastColoredTextBox12.AppendText("¶", m_PilcrowStyle);
        m_FastColoredTextBox12.AppendText("\r\nLinefeed");

        m_FastColoredTextBox11!.Clear();
        m_FastColoredTextBox11.Text = "Column with: ";
        m_FastColoredTextBox11.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
        m_FastColoredTextBox11.AppendText(" Quote");

        m_FastColoredTextBox!.Clear();

        var newToolTip = m_IsWriteSetting
            ? "If this placeholder is part of the text it will be read as quoting character"
            : "If the quoting character is part of the text it will be represented by the placeholder";

        if (m_IsWriteSetting)
        {
          if (m_RadioButtonAlways.Checked) m_FastColoredTextBox.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
          m_FastColoredTextBox.AppendText("This is");
          if (m_RadioButtonAlways.Checked) m_FastColoredTextBox.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
        }
        else
        {
          m_FastColoredTextBox.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
          m_FastColoredTextBox.AppendText("This is ");
          m_FastColoredTextBox.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
        }

        m_FastColoredTextBox.AppendText(delimiter.ToString(), delim);
        m_FastColoredTextBox.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
        m_FastColoredTextBox.AppendText("Column with:");
        m_FastColoredTextBox.AppendText(delimiter.ToString(), delim);
        m_FastColoredTextBox.AppendText(" Delimiter");
        m_FastColoredTextBox.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
        m_FastColoredTextBox.AppendText("¶", m_PilcrowStyle);
        m_FastColoredTextBox.AppendText("\r\n");

        if (m_IsWriteSetting)
        {
          if (m_RadioButtonAlways.Checked) m_FastColoredTextBox.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
          m_FastColoredTextBox.AppendText("a Trimming");
          if (m_RadioButtonAlways.Checked) m_FastColoredTextBox.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
        }
        else
        {
          m_FastColoredTextBox.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
          m_FastColoredTextBox.AppendText(" a Trimming ");
          m_FastColoredTextBox.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
        }

        m_FastColoredTextBox.AppendText(delimiter.ToString(), delim);
        m_FastColoredTextBox.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
        m_FastColoredTextBox.AppendText("Column with: ");


        if (!string.IsNullOrEmpty(m_TextBoxQuotePlaceHolder.Text) && quote == char.MinValue)
        {
          newToolTip += m_IsWriteSetting
            ? $"\r\nhello {quote} world ->{quote}hello {m_TextBoxQuotePlaceHolder.Text} world{quote}"
            : $"\r\n{quote}hello {m_TextBoxQuotePlaceHolder.Text} world{quote} -> hello {quote} world";
          m_FastColoredTextBox.AppendText(m_TextBoxQuotePlaceHolder.Text, m_EscapedQuoteStyle);
        }
        else
        {
          if (m_CsvFile.DuplicateQualifierToEscape && m_CsvFile.EscapePrefixChar != char.MinValue &&
              !m_IsWriteSetting)
          {
            m_FastColoredTextBox.AppendText(new string(quote, 2) + " or " + m_CsvFile.EscapePrefixChar.ToStringHandle0() + quote.ToStringHandle0(),
              m_EscapedQuoteStyle);
          }
          else if (m_CsvFile.DuplicateQualifierToEscape)
            m_FastColoredTextBox.AppendText(new string(quote, 2), m_EscapedQuoteStyle);
          else if (m_CsvFile.EscapePrefixChar != char.MinValue)
            m_FastColoredTextBox.AppendText(m_CsvFile.EscapePrefixChar.ToStringHandle0() + quote.ToStringHandle0(), m_EscapedQuoteStyle);
          else
            m_FastColoredTextBox.AppendText(quote.ToStringHandle0(), m_EscapedQuoteStyle);
        }

        m_FastColoredTextBox.AppendText(" Quote");
        m_FastColoredTextBox.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
        m_FastColoredTextBox.AppendText("¶", m_PilcrowStyle);
        m_FastColoredTextBox.AppendText("\r\n");
        if (m_IsWriteSetting && m_RadioButtonAlways.Checked)
        {
          m_FastColoredTextBox.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
          m_FastColoredTextBox.AppendText("Example");
          m_FastColoredTextBox.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
        }
        else
          m_FastColoredTextBox.AppendText("Example ");

        m_FastColoredTextBox.AppendText(delimiter.ToString(), delim);
        m_FastColoredTextBox.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
        m_FastColoredTextBox.AppendText("Column with ");
        m_FastColoredTextBox.AppendText("¶", m_PilcrowStyle);
        m_FastColoredTextBox.AppendText("\r\nLinefeed");
        m_FastColoredTextBox.AppendText(quote.ToStringHandle0(), m_QuoteStyle);

        m_FastColoredTextBox.Range.SetStyle(m_SpaceStyle, m_SpaceRegex);
        m_FastColoredTextBox10.Range.SetStyle(m_SpaceStyle, m_SpaceRegex);
        m_FastColoredTextBox11.Range.SetStyle(m_SpaceStyle, m_SpaceRegex);
        m_FastColoredTextBox12.Range.SetStyle(m_SpaceStyle, m_SpaceRegex);

        m_ToolTip!.SetToolTip(m_TextBoxQuotePlaceHolder, newToolTip);

        m_HasChanges = false;
      }
      catch (Exception exception)
      {
        ParentForm?.ShowError(exception, "Quoting Control");
      }
      finally
      {
        m_TimerRebuilt.Enabled = true;
      }
    }

    private void TextBoxQuote_TextChanged(object sender, EventArgs e)
    {
      m_TextBoxQuotePlaceHolder.Enabled =  !string.IsNullOrEmpty(m_TextBoxQuote.Text);
      m_RadioButtonNeeded.Enabled =  !string.IsNullOrEmpty(m_TextBoxQuote.Text);
      m_RadioButtonAlways.Enabled =  !string.IsNullOrEmpty(m_TextBoxQuote.Text);
      m_CheckBoxAlternateQuoting.Enabled =  !string.IsNullOrEmpty(m_TextBoxQuote.Text);
      m_CheckBoxDuplicateQuotingToEscape.Enabled =  !string.IsNullOrEmpty(m_TextBoxQuote.Text);

    }
  }
}