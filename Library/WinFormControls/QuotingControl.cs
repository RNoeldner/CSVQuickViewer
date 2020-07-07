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

using System.Diagnostics.CodeAnalysis;

namespace CsvTools
{
  using System;
  using System.ComponentModel;
  using System.Diagnostics.Contracts;
  using System.Drawing;
  using System.Globalization;
  using System.Windows.Forms;

  /// <summary>
  ///   A Control to edit the quoting and visualize the result
  /// </summary>
  public class QuotingControl : UserControl
  {
    private CheckBox checkBoxAlternateQuoting;

    private CheckBox checkBoxQualifyAlways;

    private CheckBox checkBoxQualifyOnlyNeeded;

    private ComboBox comboBoxTrim;

    private Label m_Label_1;

    private Label m_Label_2;

    private Label m_Label_3;

    private Label m_Label_4;

    private ICsvFile m_CsvFile;

    private ErrorProvider m_ErrorProvider;

    private BindingSource m_FileFormatBindingSource;

    private bool m_IsWriteSetting;

    private Label m_Label1;

    private Label m_Label2;

    private Label m_Label3;

    private Label m_LabelEscapeCharacter;

    private Label m_LabelInfoQuoting;

    private Label m_LabelQuote;

    private Label m_LabelQuotePlaceholder;

    private Label m_LabelTrim;

    private CSVRichTextBox m_RichTextBox00;

    private CSVRichTextBox m_RichTextBox01;

    private CSVRichTextBox m_RichTextBox02;

    private CSVRichTextBox m_RichTextBox10;

    private CSVRichTextBox m_RichTextBox11;

    private CSVRichTextBox m_RichTextBox12;

    private CSVRichTextBox m_RichTextBoxSrc;

    private TextBox m_TextBoxEscape;

    private TextBox m_TextBoxQuote;

    private TextBox m_TextBoxQuotePlaceHolder;

    private ToolTip m_ToolTip;
    private IContainer components;
    private CheckBox checkBoxDuplicateQuotingToEscape;
    private Label labelNoQuotes;
    private TableLayoutPanel m_TableLayoutPanel;

    /// <summary>
    ///   CTOR of QuotingControl
    /// </summary>
    public QuotingControl()
    {
      InitializeComponent();
      comboBoxTrim.Items.Add(new DisplayItem<int>(0, "None"));
      comboBoxTrim.Items.Add(new DisplayItem<int>(1, "Unquoted"));
      comboBoxTrim.Items.Add(new DisplayItem<int>(3, "All"));
      UpdateUI();
      SetToolTipPlaceholder();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Bindable(false)]
    [Browsable(false)]
    public ICsvFile CsvFile
    {
      get => m_CsvFile;

      set
      {
        m_CsvFile = value;
        if (m_CsvFile == null)
          return;

        m_FileFormatBindingSource.DataSource = m_CsvFile.FileFormat;

        m_CsvFile.FileFormat.PropertyChanged += FormatPropertyChanged;
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
        Contract.Assume(m_LabelInfoQuoting != null);
        Contract.Assume(comboBoxTrim != null);
        Contract.Assume(checkBoxAlternateQuoting != null);
        m_IsWriteSetting = value;
        m_LabelInfoQuoting.Visible = !value;
        comboBoxTrim.Enabled = !value;
        checkBoxAlternateQuoting.Visible = !value;
        checkBoxQualifyAlways.Visible = value;
        checkBoxQualifyOnlyNeeded.Visible = value;
      }
    }

    private bool m_IsDisposed;

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

    private void FormatPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(FileFormat.FieldDelimiter) ||
          e.PropertyName == nameof(FileFormat.DuplicateQuotingToEscape))
        SetToolTipPlaceholder();
    }

    /// <summary>
    ///   Initializes the component.
    /// </summary>
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "LocalizableElement")]
    [SuppressMessage("ReSharper", "RedundantDelegateCreation")]
    private void InitializeComponent()
    {
      System.Windows.Forms.Label m_Label5;
      this.m_LabelQuote = new System.Windows.Forms.Label();
      this.m_LabelQuotePlaceholder = new System.Windows.Forms.Label();
      this.m_TextBoxEscape = new System.Windows.Forms.TextBox();
      this.m_FileFormatBindingSource = new System.Windows.Forms.BindingSource();
      this.m_LabelEscapeCharacter = new System.Windows.Forms.Label();
      this.m_LabelTrim = new System.Windows.Forms.Label();
      this.m_TextBoxQuote = new System.Windows.Forms.TextBox();
      this.m_TextBoxQuotePlaceHolder = new System.Windows.Forms.TextBox();
      this.checkBoxAlternateQuoting = new System.Windows.Forms.CheckBox();
      this.m_ToolTip = new System.Windows.Forms.ToolTip();
      this.comboBoxTrim = new System.Windows.Forms.ComboBox();
      this.checkBoxDuplicateQuotingToEscape = new System.Windows.Forms.CheckBox();
      this.labelNoQuotes = new System.Windows.Forms.Label();
      this.m_Label2 = new System.Windows.Forms.Label();
      this.m_Label1 = new System.Windows.Forms.Label();
      this.m_Label3 = new System.Windows.Forms.Label();
      this.m_LabelInfoQuoting = new System.Windows.Forms.Label();
      this.m_ErrorProvider = new System.Windows.Forms.ErrorProvider();
      this.m_TableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.m_Label_3 = new System.Windows.Forms.Label();
      this.m_Label_4 = new System.Windows.Forms.Label();
      this.m_Label_2 = new System.Windows.Forms.Label();
      this.m_Label_1 = new System.Windows.Forms.Label();
      this.m_RichTextBoxSrc = new CsvTools.CSVRichTextBox();
      this.m_RichTextBox10 = new CsvTools.CSVRichTextBox();
      this.m_RichTextBox11 = new CsvTools.CSVRichTextBox();
      this.m_RichTextBox12 = new CsvTools.CSVRichTextBox();
      this.m_RichTextBox00 = new CsvTools.CSVRichTextBox();
      this.m_RichTextBox01 = new CsvTools.CSVRichTextBox();
      this.m_RichTextBox02 = new CsvTools.CSVRichTextBox();
      this.checkBoxQualifyOnlyNeeded = new System.Windows.Forms.CheckBox();
      this.checkBoxQualifyAlways = new System.Windows.Forms.CheckBox();
      m_Label5 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.m_FileFormatBindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_ErrorProvider)).BeginInit();
      this.m_TableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // m_Label5
      // 
      m_Label5.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(m_Label5, 5);
      m_Label5.Location = new System.Drawing.Point(20, 179);
      m_Label5.Name = "m_Label5";
      m_Label5.Size = new System.Drawing.Size(336, 13);
      m_Label5.TabIndex = 26;
      m_Label5.Text = "Tab visualized as »   Linefeed visualized as ¶    Space visualized as ●";
      // 
      // m_LabelQuote
      // 
      this.m_LabelQuote.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_LabelQuote.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_LabelQuote, 2);
      this.m_LabelQuote.Location = new System.Drawing.Point(26, 5);
      this.m_LabelQuote.Name = "m_LabelQuote";
      this.m_LabelQuote.Size = new System.Drawing.Size(72, 13);
      this.m_LabelQuote.TabIndex = 0;
      this.m_LabelQuote.Text = "Text Qualifier:";
      // 
      // m_LabelQuotePlaceholder
      // 
      this.m_LabelQuotePlaceholder.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_LabelQuotePlaceholder.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_LabelQuotePlaceholder, 2);
      this.m_LabelQuotePlaceholder.Location = new System.Drawing.Point(32, 53);
      this.m_LabelQuotePlaceholder.Name = "m_LabelQuotePlaceholder";
      this.m_LabelQuotePlaceholder.Size = new System.Drawing.Size(66, 13);
      this.m_LabelQuotePlaceholder.TabIndex = 7;
      this.m_LabelQuotePlaceholder.Text = "Placeholder:";
      // 
      // m_TextBoxEscape
      // 
      this.m_TextBoxEscape.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_TextBoxEscape.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.m_FileFormatBindingSource, "EscapeCharacter", true));
      this.m_TextBoxEscape.Location = new System.Drawing.Point(104, 26);
      this.m_TextBoxEscape.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_TextBoxEscape.Name = "m_TextBoxEscape";
      this.m_TextBoxEscape.Size = new System.Drawing.Size(96, 20);
      this.m_TextBoxEscape.TabIndex = 6;
      this.m_TextBoxEscape.TextChanged += new System.EventHandler(this.QuoteChanged);
      // 
      // m_FileFormatBindingSource
      // 
      this.m_FileFormatBindingSource.AllowNew = false;
      this.m_FileFormatBindingSource.DataSource = typeof(CsvTools.FileFormat);
      // 
      // m_LabelEscapeCharacter
      // 
      this.m_LabelEscapeCharacter.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_LabelEscapeCharacter.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_LabelEscapeCharacter, 2);
      this.m_LabelEscapeCharacter.Location = new System.Drawing.Point(3, 29);
      this.m_LabelEscapeCharacter.Name = "m_LabelEscapeCharacter";
      this.m_LabelEscapeCharacter.Size = new System.Drawing.Size(95, 13);
      this.m_LabelEscapeCharacter.TabIndex = 5;
      this.m_LabelEscapeCharacter.Text = "Escape Character:";
      // 
      // m_LabelTrim
      // 
      this.m_LabelTrim.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_LabelTrim.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_LabelTrim, 2);
      this.m_LabelTrim.Location = new System.Drawing.Point(12, 78);
      this.m_LabelTrim.Name = "m_LabelTrim";
      this.m_LabelTrim.Size = new System.Drawing.Size(86, 13);
      this.m_LabelTrim.TabIndex = 9;
      this.m_LabelTrim.Text = "Trimming Option:";
      // 
      // m_TextBoxQuote
      // 
      this.m_TextBoxQuote.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.m_FileFormatBindingSource, "FieldQualifier", true));
      this.m_TextBoxQuote.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_TextBoxQuote.Location = new System.Drawing.Point(104, 2);
      this.m_TextBoxQuote.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_TextBoxQuote.Name = "m_TextBoxQuote";
      this.m_TextBoxQuote.Size = new System.Drawing.Size(186, 20);
      this.m_TextBoxQuote.TabIndex = 1;
      this.m_ToolTip.SetToolTip(this.m_TextBoxQuote, "Columns may be qualified with a character; usually these are \" the quotes are rem" +
        "oved by the reading applications.");
      this.m_TextBoxQuote.TextChanged += new System.EventHandler(this.QuoteChanged);
      // 
      // m_TextBoxQuotePlaceHolder
      // 
      this.m_TextBoxQuotePlaceHolder.AutoCompleteCustomSource.AddRange(new string[] {
            "{q}",
            "&quot;"});
      this.m_TextBoxQuotePlaceHolder.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
      this.m_TextBoxQuotePlaceHolder.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.m_FileFormatBindingSource, "QuotePlaceholder", true));
      this.m_TextBoxQuotePlaceHolder.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_TextBoxQuotePlaceHolder.Location = new System.Drawing.Point(104, 50);
      this.m_TextBoxQuotePlaceHolder.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_TextBoxQuotePlaceHolder.Name = "m_TextBoxQuotePlaceHolder";
      this.m_TextBoxQuotePlaceHolder.Size = new System.Drawing.Size(186, 20);
      this.m_TextBoxQuotePlaceHolder.TabIndex = 8;
      this.m_ToolTip.SetToolTip(this.m_TextBoxQuotePlaceHolder, "If this placeholder is part of the text it will be replaced with the quoting char" +
        "acter");
      this.m_TextBoxQuotePlaceHolder.TextChanged += new System.EventHandler(this.QuoteChanged);
      // 
      // checkBoxAlternateQuoting
      // 
      this.checkBoxAlternateQuoting.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxAlternateQuoting.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.checkBoxAlternateQuoting, 2);
      this.checkBoxAlternateQuoting.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.m_FileFormatBindingSource, "AlternateQuoting", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxAlternateQuoting.Location = new System.Drawing.Point(296, 3);
      this.checkBoxAlternateQuoting.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.checkBoxAlternateQuoting.Name = "checkBoxAlternateQuoting";
      this.checkBoxAlternateQuoting.Size = new System.Drawing.Size(148, 17);
      this.checkBoxAlternateQuoting.TabIndex = 2;
      this.checkBoxAlternateQuoting.Text = "Context Sensitive Quoting";
      this.m_ToolTip.SetToolTip(this.checkBoxAlternateQuoting, "A quote is only regarded as closing quote if it is followed by linefeed or delimi" +
        "ter");
      this.checkBoxAlternateQuoting.UseVisualStyleBackColor = true;
      this.checkBoxAlternateQuoting.Visible = false;
      // 
      // comboBoxTrim
      // 
      this.comboBoxTrim.DisplayMember = "Display";
      this.comboBoxTrim.Dock = System.Windows.Forms.DockStyle.Top;
      this.comboBoxTrim.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxTrim.Location = new System.Drawing.Point(104, 74);
      this.comboBoxTrim.Margin = new System.Windows.Forms.Padding(3, 2, 3, 3);
      this.comboBoxTrim.Name = "comboBoxTrim";
      this.comboBoxTrim.Size = new System.Drawing.Size(186, 21);
      this.comboBoxTrim.TabIndex = 10;
      this.m_ToolTip.SetToolTip(this.comboBoxTrim, "None will not remove whitespace; Unquoted will remove white spaces if the column " +
        "was not quoted; All will remove white spaces even if the column was quoted");
      this.comboBoxTrim.ValueMember = "ID";
      this.comboBoxTrim.SelectedIndexChanged += new System.EventHandler(this.SetTrimming);
      // 
      // checkBoxDuplicateQuotingToEscape
      // 
      this.checkBoxDuplicateQuotingToEscape.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.checkBoxDuplicateQuotingToEscape, 2);
      this.checkBoxDuplicateQuotingToEscape.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.m_FileFormatBindingSource, "DuplicateQuotingToEscape", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxDuplicateQuotingToEscape.Location = new System.Drawing.Point(295, 26);
      this.checkBoxDuplicateQuotingToEscape.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.checkBoxDuplicateQuotingToEscape.Name = "checkBoxDuplicateQuotingToEscape";
      this.checkBoxDuplicateQuotingToEscape.Size = new System.Drawing.Size(113, 17);
      this.checkBoxDuplicateQuotingToEscape.TabIndex = 27;
      this.checkBoxDuplicateQuotingToEscape.Text = "Repeated Quoting";
      this.m_ToolTip.SetToolTip(this.checkBoxDuplicateQuotingToEscape, "Assume a repeated quote in a qualified text represent a quote that does not end t" +
        "ext qualification");
      this.checkBoxDuplicateQuotingToEscape.UseVisualStyleBackColor = true;
      // 
      // labelNoQuotes
      // 
      this.labelNoQuotes.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.labelNoQuotes.AutoSize = true;
      this.labelNoQuotes.BackColor = System.Drawing.SystemColors.Info;
      this.labelNoQuotes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.labelNoQuotes.ForeColor = System.Drawing.SystemColors.InfoText;
      this.labelNoQuotes.Location = new System.Drawing.Point(452, 16);
      this.labelNoQuotes.Name = "labelNoQuotes";
      this.m_TableLayoutPanel.SetRowSpan(this.labelNoQuotes, 2);
      this.labelNoQuotes.Size = new System.Drawing.Size(142, 15);
      this.labelNoQuotes.TabIndex = 28;
      this.labelNoQuotes.Text = "Text can not contain Qalifier";
      this.m_ToolTip.SetToolTip(this.labelNoQuotes, "Either “Context Sensitive Quoting”, “Repeated Quotes” or an “Escape Character” ne" +
        "ed to be defined to allow a quote to be part of the text");
      // 
      // m_Label2
      // 
      this.m_Label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_Label2.AutoSize = true;
      this.m_Label2.ForeColor = System.Drawing.Color.Teal;
      this.m_Label2.Location = new System.Drawing.Point(295, 102);
      this.m_Label2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.m_Label2.Name = "m_Label2";
      this.m_Label2.Size = new System.Drawing.Size(13, 13);
      this.m_Label2.TabIndex = 14;
      this.m_Label2.Text = "1\r\n";
      // 
      // m_Label1
      // 
      this.m_Label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_Label1.AutoSize = true;
      this.m_Label1.ForeColor = System.Drawing.Color.Teal;
      this.m_Label1.Location = new System.Drawing.Point(295, 123);
      this.m_Label1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.m_Label1.Name = "m_Label1";
      this.m_Label1.Size = new System.Drawing.Size(13, 13);
      this.m_Label1.TabIndex = 18;
      this.m_Label1.Text = "2";
      // 
      // m_Label3
      // 
      this.m_Label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_Label3.AutoSize = true;
      this.m_Label3.ForeColor = System.Drawing.Color.Teal;
      this.m_Label3.Location = new System.Drawing.Point(295, 153);
      this.m_Label3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.m_Label3.Name = "m_Label3";
      this.m_TableLayoutPanel.SetRowSpan(this.m_Label3, 2);
      this.m_Label3.Size = new System.Drawing.Size(13, 13);
      this.m_Label3.TabIndex = 22;
      this.m_Label3.Text = "3";
      // 
      // m_LabelInfoQuoting
      // 
      this.m_LabelInfoQuoting.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_LabelInfoQuoting.AutoSize = true;
      this.m_LabelInfoQuoting.BackColor = System.Drawing.SystemColors.Info;
      this.m_LabelInfoQuoting.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_LabelInfoQuoting, 3);
      this.m_LabelInfoQuoting.ForeColor = System.Drawing.SystemColors.InfoText;
      this.m_LabelInfoQuoting.Location = new System.Drawing.Point(296, 77);
      this.m_LabelInfoQuoting.Name = "m_LabelInfoQuoting";
      this.m_LabelInfoQuoting.Size = new System.Drawing.Size(225, 15);
      this.m_LabelInfoQuoting.TabIndex = 11;
      this.m_LabelInfoQuoting.Text = "Not possible to have leading or trailing spaces";
      // 
      // m_ErrorProvider
      // 
      this.m_ErrorProvider.ContainerControl = this;
      // 
      // m_TableLayoutPanel
      // 
      this.m_TableLayoutPanel.AutoSize = true;
      this.m_TableLayoutPanel.ColumnCount = 6;
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 51.15865F));
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 48.84135F));
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelQuote, 0, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelEscapeCharacter, 0, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelQuotePlaceholder, 0, 2);
      this.m_TableLayoutPanel.Controls.Add(m_Label5, 1, 8);
      this.m_TableLayoutPanel.Controls.Add(this.m_Label_3, 0, 6);
      this.m_TableLayoutPanel.Controls.Add(this.m_Label_4, 0, 7);
      this.m_TableLayoutPanel.Controls.Add(this.m_Label_2, 0, 5);
      this.m_TableLayoutPanel.Controls.Add(this.m_Label_1, 0, 4);
      this.m_TableLayoutPanel.Controls.Add(this.m_RichTextBoxSrc, 1, 4);
      this.m_TableLayoutPanel.Controls.Add(this.m_RichTextBox10, 5, 4);
      this.m_TableLayoutPanel.Controls.Add(this.m_RichTextBox11, 5, 5);
      this.m_TableLayoutPanel.Controls.Add(this.m_RichTextBox12, 5, 6);
      this.m_TableLayoutPanel.Controls.Add(this.m_TextBoxQuote, 2, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_TextBoxEscape, 2, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_TextBoxQuotePlaceHolder, 2, 2);
      this.m_TableLayoutPanel.Controls.Add(this.comboBoxTrim, 2, 3);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelTrim, 0, 3);
      this.m_TableLayoutPanel.Controls.Add(this.m_RichTextBox00, 4, 4);
      this.m_TableLayoutPanel.Controls.Add(this.m_RichTextBox01, 4, 5);
      this.m_TableLayoutPanel.Controls.Add(this.m_RichTextBox02, 4, 6);
      this.m_TableLayoutPanel.Controls.Add(this.m_Label2, 3, 4);
      this.m_TableLayoutPanel.Controls.Add(this.m_Label1, 3, 5);
      this.m_TableLayoutPanel.Controls.Add(this.m_Label3, 3, 6);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelInfoQuoting, 3, 3);
      this.m_TableLayoutPanel.Controls.Add(this.checkBoxQualifyOnlyNeeded, 3, 2);
      this.m_TableLayoutPanel.Controls.Add(this.checkBoxAlternateQuoting, 3, 0);
      this.m_TableLayoutPanel.Controls.Add(this.labelNoQuotes, 5, 0);
      this.m_TableLayoutPanel.Controls.Add(this.checkBoxQualifyAlways, 5, 2);
      this.m_TableLayoutPanel.Controls.Add(this.checkBoxDuplicateQuotingToEscape, 3, 1);
      this.m_TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.m_TableLayoutPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      this.m_TableLayoutPanel.RowCount = 9;
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.Size = new System.Drawing.Size(633, 192);
      this.m_TableLayoutPanel.TabIndex = 0;
      // 
      // m_Label_3
      // 
      this.m_Label_3.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_Label_3.AutoSize = true;
      this.m_Label_3.ForeColor = System.Drawing.Color.Teal;
      this.m_Label_3.Location = new System.Drawing.Point(2, 142);
      this.m_Label_3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.m_Label_3.Name = "m_Label_3";
      this.m_Label_3.Size = new System.Drawing.Size(13, 13);
      this.m_Label_3.TabIndex = 21;
      this.m_Label_3.Text = "3";
      // 
      // m_Label_4
      // 
      this.m_Label_4.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_Label_4.AutoSize = true;
      this.m_Label_4.ForeColor = System.Drawing.Color.Teal;
      this.m_Label_4.Location = new System.Drawing.Point(2, 161);
      this.m_Label_4.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.m_Label_4.Name = "m_Label_4";
      this.m_Label_4.Size = new System.Drawing.Size(13, 13);
      this.m_Label_4.TabIndex = 25;
      this.m_Label_4.Text = "4";
      // 
      // m_Label_2
      // 
      this.m_Label_2.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_Label_2.AutoSize = true;
      this.m_Label_2.ForeColor = System.Drawing.Color.Teal;
      this.m_Label_2.Location = new System.Drawing.Point(2, 123);
      this.m_Label_2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.m_Label_2.Name = "m_Label_2";
      this.m_Label_2.Size = new System.Drawing.Size(13, 13);
      this.m_Label_2.TabIndex = 17;
      this.m_Label_2.Text = "2";
      // 
      // m_Label_1
      // 
      this.m_Label_1.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_Label_1.AutoSize = true;
      this.m_Label_1.ForeColor = System.Drawing.Color.Teal;
      this.m_Label_1.Location = new System.Drawing.Point(2, 102);
      this.m_Label_1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.m_Label_1.Name = "m_Label_1";
      this.m_Label_1.Size = new System.Drawing.Size(13, 13);
      this.m_Label_1.TabIndex = 12;
      this.m_Label_1.Text = "1\r\n";
      // 
      // m_RichTextBoxSrc
      // 
      this.m_RichTextBoxSrc.BackColor = System.Drawing.SystemColors.Window;
      this.m_RichTextBoxSrc.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_RichTextBoxSrc, 2);
      this.m_RichTextBoxSrc.DataBindings.Add(new System.Windows.Forms.Binding("Delimiter", this.m_FileFormatBindingSource, "FieldDelimiterChar", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.m_RichTextBoxSrc.Delimiter = ';';
      this.m_RichTextBoxSrc.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_RichTextBoxSrc.Escape = '>';
      this.m_RichTextBoxSrc.Location = new System.Drawing.Point(17, 98);
      this.m_RichTextBoxSrc.Margin = new System.Windows.Forms.Padding(0);
      this.m_RichTextBoxSrc.Name = "m_RichTextBoxSrc";
      this.m_RichTextBoxSrc.ReadOnly = true;
      this.m_TableLayoutPanel.SetRowSpan(this.m_RichTextBoxSrc, 4);
      this.m_RichTextBoxSrc.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      this.m_RichTextBoxSrc.Size = new System.Drawing.Size(276, 81);
      this.m_RichTextBoxSrc.TabIndex = 13;
      this.m_RichTextBoxSrc.Text = "\"This is \";Column with:, Delimiter\n a Trimming ;Column with \"\" Quote\nExample ;\"Co" +
    "lumn with \nLinefeed\"";
      // 
      // m_RichTextBox10
      // 
      this.m_RichTextBox10.BackColor = System.Drawing.SystemColors.Window;
      this.m_RichTextBox10.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.m_RichTextBox10.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_RichTextBox10.Location = new System.Drawing.Point(449, 98);
      this.m_RichTextBox10.Margin = new System.Windows.Forms.Padding(0);
      this.m_RichTextBox10.Name = "m_RichTextBox10";
      this.m_RichTextBox10.ReadOnly = true;
      this.m_RichTextBox10.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      this.m_RichTextBox10.Size = new System.Drawing.Size(184, 21);
      this.m_RichTextBox10.TabIndex = 16;
      this.m_RichTextBox10.Text = "Column with:, Delimiter";
      this.m_RichTextBox10.WordWrap = false;
      // 
      // m_RichTextBox11
      // 
      this.m_RichTextBox11.BackColor = System.Drawing.SystemColors.Window;
      this.m_RichTextBox11.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.m_RichTextBox11.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_RichTextBox11.Location = new System.Drawing.Point(449, 119);
      this.m_RichTextBox11.Margin = new System.Windows.Forms.Padding(0);
      this.m_RichTextBox11.Name = "m_RichTextBox11";
      this.m_RichTextBox11.ReadOnly = true;
      this.m_RichTextBox11.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      this.m_RichTextBox11.Size = new System.Drawing.Size(184, 21);
      this.m_RichTextBox11.TabIndex = 20;
      this.m_RichTextBox11.Text = "Column with \" Quote";
      this.m_RichTextBox11.WordWrap = false;
      // 
      // m_RichTextBox12
      // 
      this.m_RichTextBox12.BackColor = System.Drawing.SystemColors.Window;
      this.m_RichTextBox12.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.m_RichTextBox12.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_RichTextBox12.Location = new System.Drawing.Point(449, 140);
      this.m_RichTextBox12.Margin = new System.Windows.Forms.Padding(0);
      this.m_RichTextBox12.Name = "m_RichTextBox12";
      this.m_RichTextBox12.ReadOnly = true;
      this.m_TableLayoutPanel.SetRowSpan(this.m_RichTextBox12, 2);
      this.m_RichTextBox12.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      this.m_RichTextBox12.Size = new System.Drawing.Size(184, 39);
      this.m_RichTextBox12.TabIndex = 24;
      this.m_RichTextBox12.Text = "Column with \nLinefeed";
      this.m_RichTextBox12.WordWrap = false;
      // 
      // m_RichTextBox00
      // 
      this.m_RichTextBox00.BackColor = System.Drawing.SystemColors.Window;
      this.m_RichTextBox00.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.m_RichTextBox00.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_RichTextBox00.Location = new System.Drawing.Point(310, 98);
      this.m_RichTextBox00.Margin = new System.Windows.Forms.Padding(0);
      this.m_RichTextBox00.Name = "m_RichTextBox00";
      this.m_RichTextBox00.ReadOnly = true;
      this.m_RichTextBox00.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      this.m_RichTextBox00.Size = new System.Drawing.Size(139, 21);
      this.m_RichTextBox00.TabIndex = 15;
      this.m_RichTextBox00.Text = "This is ";
      this.m_RichTextBox00.WordWrap = false;
      // 
      // m_RichTextBox01
      // 
      this.m_RichTextBox01.BackColor = System.Drawing.SystemColors.Window;
      this.m_RichTextBox01.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.m_RichTextBox01.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_RichTextBox01.Location = new System.Drawing.Point(310, 119);
      this.m_RichTextBox01.Margin = new System.Windows.Forms.Padding(0);
      this.m_RichTextBox01.Name = "m_RichTextBox01";
      this.m_RichTextBox01.ReadOnly = true;
      this.m_RichTextBox01.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      this.m_RichTextBox01.Size = new System.Drawing.Size(139, 21);
      this.m_RichTextBox01.TabIndex = 19;
      this.m_RichTextBox01.Text = " a Trimming ";
      this.m_RichTextBox01.WordWrap = false;
      // 
      // m_RichTextBox02
      // 
      this.m_RichTextBox02.BackColor = System.Drawing.SystemColors.Window;
      this.m_RichTextBox02.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.m_RichTextBox02.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_RichTextBox02.Location = new System.Drawing.Point(310, 140);
      this.m_RichTextBox02.Margin = new System.Windows.Forms.Padding(0);
      this.m_RichTextBox02.Name = "m_RichTextBox02";
      this.m_RichTextBox02.ReadOnly = true;
      this.m_TableLayoutPanel.SetRowSpan(this.m_RichTextBox02, 2);
      this.m_RichTextBox02.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      this.m_RichTextBox02.Size = new System.Drawing.Size(139, 39);
      this.m_RichTextBox02.TabIndex = 23;
      this.m_RichTextBox02.Text = "Example ";
      this.m_RichTextBox02.WordWrap = false;
      // 
      // checkBoxQualifyOnlyNeeded
      // 
      this.checkBoxQualifyOnlyNeeded.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxQualifyOnlyNeeded.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.checkBoxQualifyOnlyNeeded, 2);
      this.checkBoxQualifyOnlyNeeded.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.m_FileFormatBindingSource, "QualifyOnlyIfNeeded", true));
      this.checkBoxQualifyOnlyNeeded.Location = new System.Drawing.Point(296, 51);
      this.checkBoxQualifyOnlyNeeded.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.checkBoxQualifyOnlyNeeded.Name = "checkBoxQualifyOnlyNeeded";
      this.checkBoxQualifyOnlyNeeded.Size = new System.Drawing.Size(132, 17);
      this.checkBoxQualifyOnlyNeeded.TabIndex = 4;
      this.checkBoxQualifyOnlyNeeded.Text = "Qualify Only If Needed";
      this.checkBoxQualifyOnlyNeeded.UseVisualStyleBackColor = true;
      this.checkBoxQualifyOnlyNeeded.Visible = false;
      // 
      // checkBoxQualifyAlways
      // 
      this.checkBoxQualifyAlways.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxQualifyAlways.AutoSize = true;
      this.checkBoxQualifyAlways.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.m_FileFormatBindingSource, "QualifyAlways", true));
      this.checkBoxQualifyAlways.Location = new System.Drawing.Point(452, 51);
      this.checkBoxQualifyAlways.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.checkBoxQualifyAlways.Name = "checkBoxQualifyAlways";
      this.checkBoxQualifyAlways.Size = new System.Drawing.Size(94, 17);
      this.checkBoxQualifyAlways.TabIndex = 3;
      this.checkBoxQualifyAlways.Text = "Qualify Always";
      this.checkBoxQualifyAlways.UseVisualStyleBackColor = true;
      this.checkBoxQualifyAlways.Visible = false;
      // 
      // QuotingControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.m_TableLayoutPanel);
      this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.MinimumSize = new System.Drawing.Size(498, 0);
      this.Name = "QuotingControl";
      this.Size = new System.Drawing.Size(633, 206);
      ((System.ComponentModel.ISupportInitialize)(this.m_FileFormatBindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_ErrorProvider)).EndInit();
      this.m_TableLayoutPanel.ResumeLayout(false);
      this.m_TableLayoutPanel.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    private void QuoteChanged(object sender, EventArgs e)
    {
      SetTrimming(sender, e);
      SetToolTipPlaceholder();
    }

    private void SetCboTrim(TrimmingOption trim) =>
      comboBoxTrim.SafeInvokeNoHandleNeeded(
        () =>
          {
            foreach (var ite in comboBoxTrim.Items)
            {
              var item = (DisplayItem<int>) ite;
              if (item.ID == (int) trim)
              {
                comboBoxTrim.SelectedItem = ite;
                break;
              }
            }
          });

    private void SettingPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(CsvFile.TrimmingOption))
        SetCboTrim(m_CsvFile.TrimmingOption);
    }

    private void SetToolTipPlaceholder() =>
      this.SafeInvoke(
        () =>
          {
            labelNoQuotes.Visible = !(m_CsvFile.FileFormat.AlternateQuoting
                                      || m_CsvFile.FileFormat.DuplicateQuotingToEscape
                                      || m_CsvFile.FileFormat.EscapeCharacterChar != '\0');

            m_ErrorProvider.SetError(m_TextBoxQuote, "");

            var quote = FileFormat.GetChar(m_TextBoxQuote.Text).ToString(CultureInfo.CurrentCulture);

            if (quote != "\0" && quote != "'" && quote != "\"")
              m_ErrorProvider.SetError(m_TextBoxQuote, "Unusual Quoting character");

            if (m_RichTextBoxSrc.Delimiter == quote[0])
              m_ErrorProvider.SetError(m_TextBoxQuote, "Delimiter and Quote have to be different");

            if (quote == "\0")
              quote = null;

            if (quote == null)
            {
              m_RichTextBox10.Text = @"<Not possible>";
              m_RichTextBox10.SelectAll();
              m_RichTextBox10.SelectionColor = Color.Red;

              m_RichTextBox12.Text = @"<Not possible>";
              m_RichTextBox12.SelectAll();
              m_RichTextBox12.SelectionColor = Color.Red;
            }
            else
            {
              m_RichTextBox10.Text = $@"Column with:{m_RichTextBoxSrc.Delimiter} Delimiter";
              m_RichTextBox12.Text = @"Column with 
Linefeed";
            }

            m_RichTextBox11.Text = $@"Column with:{quote} Quote";


            // richTextBox11.Quote = quote[0];

            var newToolTip = m_IsWriteSetting
                               ? "Start the column with a quote, if a quote is part of the text the quote is replaced with a placeholder."
                               : "If the placeholder is part of the text it will be replaced with the quoting character.";

            var sampleText = quote + "This is " + quote + m_RichTextBoxSrc.Delimiter + quote + "Column with:"
                             + m_RichTextBoxSrc.Delimiter + " Delimiter" + quote + "\r\n" + quote + " a Trimming "
                             + quote + m_RichTextBoxSrc.Delimiter + quote + "Column with:{*} Quote" + quote + "\r\n"
                             + "Example " + m_RichTextBoxSrc.Delimiter + quote + "Column with \r\nLinefeed" + quote;

            if (!string.IsNullOrEmpty(m_TextBoxQuotePlaceHolder.Text) && !string.IsNullOrEmpty(quote))
            {
              newToolTip += m_IsWriteSetting
                              ? $"\r\nhello {quote} world ->{quote}hello {m_TextBoxQuotePlaceHolder.Text} world{quote}"
                              : $"\r\n{quote}hello {m_TextBoxQuotePlaceHolder.Text} world{quote} -> hello {quote} world";

              sampleText = sampleText.Replace("{*}", m_TextBoxQuotePlaceHolder.Text);
            }

            if (m_CsvFile.FileFormat.AlternateQuoting)
            {
              sampleText = !m_CsvFile.FileFormat.DuplicateQuotingToEscape
                ? sampleText.Replace("{*}", quote)
                : sampleText.Replace("{*}", quote + quote);
            }
            else
            {
              sampleText = string.IsNullOrEmpty(m_TextBoxEscape.Text)
                             ? sampleText.Replace("{*}", quote + quote)
                             : sampleText.Replace("{*}", m_TextBoxEscape.Text + quote);
            }

            m_RichTextBoxSrc.Text = sampleText;
            m_RichTextBoxSrc.Quote = quote?[0] ?? '\0';

            m_ToolTip.SetToolTip(m_TextBoxQuotePlaceHolder, newToolTip);
          });

    [SuppressMessage("ReSharper", "LocalizableElement")]
    private void SetTrimming(object sender, EventArgs e) =>
      this.SafeInvoke(
        () =>
          {
            Contract.Requires(comboBoxTrim != null);
            if (comboBoxTrim.SelectedItem == null)
              return;

            checkBoxAlternateQuoting.Enabled = !string.IsNullOrEmpty(m_TextBoxQuote.Text);

            m_RichTextBox00.Clear();
            m_RichTextBox01.Clear();
            m_RichTextBox02.Clear();
            switch (((DisplayItem<int>) comboBoxTrim.SelectedItem).ID)
            {
              case 1:
                m_CsvFile.TrimmingOption = TrimmingOption.Unquoted;

                m_RichTextBox00.SelectionColor = Color.Black;
                m_RichTextBox00.Text = "This is ";
                m_RichTextBox00.Select(m_RichTextBox00.TextLength - 1, 1);
                m_RichTextBox00.SelectionBackColor = Color.Yellow;

                m_RichTextBox01.Text = " a Trimming ";
                m_RichTextBox01.Select(0, 1);
                m_RichTextBox01.SelectionBackColor = Color.Yellow;
                m_RichTextBox01.SelectionColor = Color.Orange;
                m_RichTextBox01.Select(m_RichTextBox01.TextLength - 1, 1);
                m_RichTextBox01.SelectionBackColor = Color.Yellow;
                m_RichTextBox02.Text = "Example";
                m_LabelInfoQuoting.Text =
                  "Import of leading or training spaces possible, but the field has to be quoted";
                break;

              case 3:
                m_CsvFile.TrimmingOption = TrimmingOption.All;

                m_RichTextBox00.Text = "This is";
                m_RichTextBox01.Text = "a Trimming";
                m_RichTextBox02.Text = "Example";
                m_LabelInfoQuoting.Text = "Not possible to have leading nor trailing spaces";
                break;

              default:
                m_CsvFile.TrimmingOption = TrimmingOption.None;

                m_RichTextBox00.Text = "This is ";
                m_RichTextBox00.Select(m_RichTextBox00.TextLength - 1, 1);
                m_RichTextBox00.SelectionBackColor = Color.Yellow;

                m_RichTextBox01.Text = " a Trimming ";
                m_RichTextBox01.Select(0, 1);
                m_RichTextBox01.SelectionBackColor = Color.Yellow;
                m_RichTextBox01.SelectionColor = Color.Orange;
                m_RichTextBox01.Select(m_RichTextBox01.TextLength - 1, 1);
                m_RichTextBox01.SelectionBackColor = Color.Yellow;

                m_RichTextBox02.Text = "Example ";
                m_RichTextBox02.Select(m_RichTextBox02.TextLength - 1, 1);
                m_RichTextBox02.SelectionBackColor = Color.Yellow;

                m_LabelInfoQuoting.Text = "Leading or training spaces will stay as they are";
                break;
            }
          });

    private void UpdateUI()
    {
      SetCboTrim(m_CsvFile?.TrimmingOption ?? TrimmingOption.Unquoted);
      SetToolTipPlaceholder();
      QuoteChanged(this, null);
    }
  }
}