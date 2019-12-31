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

using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   A Control to edit the quoting and visualize the result
  /// </summary>
  public class QuotingControl : UserControl
  {
    /// <summary>
    ///   The components
    /// </summary>
    private IContainer components;

    private CheckBox checkBoxAlternateQuoting;
    private ComboBox comboBoxTrim;
    private ICsvFile m_CsvFile;
    private ErrorProvider m_ErrorProvider;
    private BindingSource m_FileFormatBindingSource;
    private BindingSource m_FileSettingBindingSource;
    private bool m_IsWriteSetting;
    private Label m_Label1;
    private Label m_Label2;
    private Label m_Label3;
    private Label m_LabelEscapeCharacter;
    private Label m_LabelInfoQuoting;
    private Label m_LabelQuote;
    private Label m_LabelQuotePlaceholer;
    private Label m_LabelTrim;
    private CSVRichTextBox m_RichTextBoxSrc;
    private CSVRichTextBox m_RichTextBox00;
    private CSVRichTextBox m_RichTextBox01;
    private CSVRichTextBox m_RichTextBox02;
    private TextBox m_TextBoxEscape;
    private TextBox m_TextBoxQuote;
    private TextBox m_TextBoxQuotePlaceHolder;
    private TableLayoutPanel tableLayoutPanel1;
    private CheckBox checkBoxQualifyAlways;
    private CheckBox checkBoxQualifyOnlyNeeded;
    private Label label1;
    private Label label3;
    private Label label4;
    private Label label2;
    private CSVRichTextBox m_RichTextBox10;
    private CSVRichTextBox m_RichTextBox11;
    private CSVRichTextBox m_RichTextBox12;
    private ToolTip m_ToolTip;

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
    }

    private void UpdateUI()
    {
      SetCboTrim(m_CsvFile?.TrimmingOption ?? TrimmingOption.Unquoted);
      SetToolTipPlaceholder();
      QuoteChanged(this, null);
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

        m_FileSettingBindingSource.DataSource = m_CsvFile;
        m_FileFormatBindingSource.DataSource = m_CsvFile.FileFormat;

        m_CsvFile.FileFormat.PropertyChanged += FormatPropertyChanged;
        m_CsvFile.PropertyChanged += SettingPropertyChanged;

        UpdateUI();
      }
    }

    private void SettingPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(CsvFile.TrimmingOption))
        SetCboTrim(m_CsvFile.TrimmingOption);
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

    #region

    /// <summary>
    ///   Dispose
    /// </summary>
    /// <param name="disposing">
    ///   true to release both managed and unmanaged resources; false to release only unmanaged
    ///   resources.
    /// </param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
        components?.Dispose();

      base.Dispose(disposing);
    }

    private void SetTrimming(object sender, EventArgs e) => this.SafeInvoke(() =>
    {
      Contract.Requires(comboBoxTrim != null);
      if (comboBoxTrim.SelectedItem == null)
        return;

      checkBoxAlternateQuoting.Enabled = !string.IsNullOrEmpty(m_TextBoxQuote.Text);

      m_RichTextBox00.Clear();
      m_RichTextBox01.Clear();
      m_RichTextBox02.Clear();
      switch (((DisplayItem<int>)comboBoxTrim.SelectedItem).ID)
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
          m_LabelInfoQuoting.Text = "Import of leading or training spaces possible, but the field has to be quoted";
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

    /// <summary>
    ///   Initializes the component.
    /// </summary>
    private void InitializeComponent()
    {
      components = new System.ComponentModel.Container();
      System.Windows.Forms.Label m_Label5;
      m_RichTextBox10 = new CsvTools.CSVRichTextBox();
      m_LabelQuote = new System.Windows.Forms.Label();
      m_LabelQuotePlaceholer = new System.Windows.Forms.Label();
      m_TextBoxEscape = new System.Windows.Forms.TextBox();
      m_FileFormatBindingSource = new System.Windows.Forms.BindingSource(components);
      m_LabelEscapeCharacter = new System.Windows.Forms.Label();
      m_LabelTrim = new System.Windows.Forms.Label();
      m_TextBoxQuote = new System.Windows.Forms.TextBox();
      m_TextBoxQuotePlaceHolder = new System.Windows.Forms.TextBox();
      checkBoxAlternateQuoting = new System.Windows.Forms.CheckBox();
      m_FileSettingBindingSource = new System.Windows.Forms.BindingSource(components);
      m_ToolTip = new System.Windows.Forms.ToolTip(components);
      comboBoxTrim = new System.Windows.Forms.ComboBox();
      m_Label2 = new System.Windows.Forms.Label();
      m_RichTextBox02 = new CsvTools.CSVRichTextBox();
      m_RichTextBox01 = new CsvTools.CSVRichTextBox();
      m_RichTextBox00 = new CsvTools.CSVRichTextBox();
      m_RichTextBox11 = new CsvTools.CSVRichTextBox();
      m_RichTextBox12 = new CsvTools.CSVRichTextBox();
      m_Label1 = new System.Windows.Forms.Label();
      m_Label3 = new System.Windows.Forms.Label();
      m_LabelInfoQuoting = new System.Windows.Forms.Label();
      m_ErrorProvider = new System.Windows.Forms.ErrorProvider(components);
      tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      label3 = new System.Windows.Forms.Label();
      label4 = new System.Windows.Forms.Label();
      label2 = new System.Windows.Forms.Label();
      label1 = new System.Windows.Forms.Label();
      m_RichTextBoxSrc = new CsvTools.CSVRichTextBox();
      checkBoxQualifyAlways = new System.Windows.Forms.CheckBox();
      checkBoxQualifyOnlyNeeded = new System.Windows.Forms.CheckBox();
      m_Label5 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(m_FileFormatBindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(m_FileSettingBindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(m_ErrorProvider)).BeginInit();
      tableLayoutPanel1.SuspendLayout();
      SuspendLayout();
      //
      // m_Label5
      //
      m_Label5.AutoSize = true;
      tableLayoutPanel1.SetColumnSpan(m_Label5, 5);
      m_Label5.Location = new System.Drawing.Point(28, 274);
      m_Label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      m_Label5.Name = "m_Label5";
      m_Label5.Size = new System.Drawing.Size(490, 20);
      m_Label5.TabIndex = 26;
      m_Label5.Text = "Tab visualized as »   Linefeed visualized as ¶    Space visualized as ●";
      //
      // m_RichTextBox10
      //
      m_RichTextBox10.BackColor = System.Drawing.SystemColors.Window;
      m_RichTextBox10.BorderStyle = System.Windows.Forms.BorderStyle.None;
      m_RichTextBox10.Delimiter = ',';
      m_RichTextBox10.DisplaySpace = true;
      m_RichTextBox10.Dock = System.Windows.Forms.DockStyle.Top;
      m_RichTextBox10.Escape = '\\';
      m_RichTextBox10.Location = new System.Drawing.Point(644, 146);
      m_RichTextBox10.Margin = new System.Windows.Forms.Padding(0);
      m_RichTextBox10.Name = "m_RichTextBox10";
      m_RichTextBox10.Quote = '\"';
      m_RichTextBox10.ReadOnly = true;
      m_RichTextBox10.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      m_RichTextBox10.Size = new System.Drawing.Size(289, 32);
      m_RichTextBox10.TabIndex = 16;
      m_RichTextBox10.Text = "Column with:, Delimiter";
      m_RichTextBox10.WordWrap = false;
      //
      // m_LabelQuote
      //
      m_LabelQuote.Anchor = System.Windows.Forms.AnchorStyles.Right;
      m_LabelQuote.AutoSize = true;
      tableLayoutPanel1.SetColumnSpan(m_LabelQuote, 2);
      m_LabelQuote.Location = new System.Drawing.Point(40, 8);
      m_LabelQuote.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      m_LabelQuote.Name = "m_LabelQuote";
      m_LabelQuote.Size = new System.Drawing.Size(105, 20);
      m_LabelQuote.TabIndex = 0;
      m_LabelQuote.Text = "Text Qualifier:";
      //
      // m_LabelQuotePlaceholer
      //
      m_LabelQuotePlaceholer.Anchor = System.Windows.Forms.AnchorStyles.Right;
      m_LabelQuotePlaceholer.AutoSize = true;
      tableLayoutPanel1.SetColumnSpan(m_LabelQuotePlaceholer, 2);
      m_LabelQuotePlaceholer.Location = new System.Drawing.Point(49, 80);
      m_LabelQuotePlaceholer.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      m_LabelQuotePlaceholer.Name = "m_LabelQuotePlaceholer";
      m_LabelQuotePlaceholer.Size = new System.Drawing.Size(96, 20);
      m_LabelQuotePlaceholer.TabIndex = 7;
      m_LabelQuotePlaceholer.Text = "Placeholder:";
      //
      // m_TextBoxEscape
      //
      m_TextBoxEscape.DataBindings.Add(new System.Windows.Forms.Binding("Text", m_FileFormatBindingSource, "EscapeCharacter", true));
      m_TextBoxEscape.Dock = System.Windows.Forms.DockStyle.Top;
      m_TextBoxEscape.Location = new System.Drawing.Point(153, 41);
      m_TextBoxEscape.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      m_TextBoxEscape.Name = "m_TextBoxEscape";
      m_TextBoxEscape.Size = new System.Drawing.Size(291, 26);
      m_TextBoxEscape.TabIndex = 6;
      //
      // m_FileFormatBindingSource
      //
      m_FileFormatBindingSource.AllowNew = false;
      m_FileFormatBindingSource.DataSource = typeof(CsvTools.FileFormat);
      //
      // m_LabelEscapeCharacter
      //
      m_LabelEscapeCharacter.Anchor = System.Windows.Forms.AnchorStyles.Right;
      m_LabelEscapeCharacter.AutoSize = true;
      tableLayoutPanel1.SetColumnSpan(m_LabelEscapeCharacter, 2);
      m_LabelEscapeCharacter.Location = new System.Drawing.Point(4, 44);
      m_LabelEscapeCharacter.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      m_LabelEscapeCharacter.Name = "m_LabelEscapeCharacter";
      m_LabelEscapeCharacter.Size = new System.Drawing.Size(141, 20);
      m_LabelEscapeCharacter.TabIndex = 5;
      m_LabelEscapeCharacter.Text = "Escape Character:";
      //
      // m_LabelTrim
      //
      m_LabelTrim.Anchor = System.Windows.Forms.AnchorStyles.Right;
      m_LabelTrim.AutoSize = true;
      tableLayoutPanel1.SetColumnSpan(m_LabelTrim, 2);
      m_LabelTrim.Location = new System.Drawing.Point(17, 117);
      m_LabelTrim.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      m_LabelTrim.Name = "m_LabelTrim";
      m_LabelTrim.Size = new System.Drawing.Size(128, 20);
      m_LabelTrim.TabIndex = 9;
      m_LabelTrim.Text = "Trimming Option:";
      //
      // m_TextBoxQuote
      //
      m_TextBoxQuote.DataBindings.Add(new System.Windows.Forms.Binding("Text", m_FileFormatBindingSource, "FieldQualifier", true));
      m_TextBoxQuote.Dock = System.Windows.Forms.DockStyle.Top;
      m_TextBoxQuote.Location = new System.Drawing.Point(153, 5);
      m_TextBoxQuote.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      m_TextBoxQuote.Name = "m_TextBoxQuote";
      m_TextBoxQuote.Size = new System.Drawing.Size(291, 26);
      m_TextBoxQuote.TabIndex = 1;
      m_ToolTip.SetToolTip(m_TextBoxQuote, "Columns may be qualified with a character; usually these are \" the quotes are rem" +
        "oved by the reading applications.");
      m_TextBoxQuote.TextChanged += new System.EventHandler(QuoteChanged);
      //
      // m_TextBoxQuotePlaceHolder
      //
      m_TextBoxQuotePlaceHolder.AutoCompleteCustomSource.AddRange(new string[] {
            "{q}",
            "&quot;"});
      m_TextBoxQuotePlaceHolder.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
      m_TextBoxQuotePlaceHolder.DataBindings.Add(new System.Windows.Forms.Binding("Text", m_FileFormatBindingSource, "QuotePlaceholder", true));
      m_TextBoxQuotePlaceHolder.Dock = System.Windows.Forms.DockStyle.Top;
      m_TextBoxQuotePlaceHolder.Location = new System.Drawing.Point(153, 77);
      m_TextBoxQuotePlaceHolder.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      m_TextBoxQuotePlaceHolder.Name = "m_TextBoxQuotePlaceHolder";
      m_TextBoxQuotePlaceHolder.Size = new System.Drawing.Size(291, 26);
      m_TextBoxQuotePlaceHolder.TabIndex = 8;
      m_ToolTip.SetToolTip(m_TextBoxQuotePlaceHolder, "If this placeholder is part of the text it will be replaced with the quoting char" +
        "acter");
      m_TextBoxQuotePlaceHolder.TextChanged += new System.EventHandler(QuoteChanged);
      //
      // checkBoxAlternateQuoting
      //
      checkBoxAlternateQuoting.Anchor = System.Windows.Forms.AnchorStyles.Left;
      checkBoxAlternateQuoting.AutoSize = true;
      tableLayoutPanel1.SetColumnSpan(checkBoxAlternateQuoting, 3);
      checkBoxAlternateQuoting.DataBindings.Add(new System.Windows.Forms.Binding("Checked", m_FileSettingBindingSource, "AlternateQuoting", true));
      checkBoxAlternateQuoting.Location = new System.Drawing.Point(452, 6);
      checkBoxAlternateQuoting.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      checkBoxAlternateQuoting.Name = "checkBoxAlternateQuoting";
      checkBoxAlternateQuoting.Size = new System.Drawing.Size(218, 24);
      checkBoxAlternateQuoting.TabIndex = 2;
      checkBoxAlternateQuoting.Text = "Context Sensitive Quoting";
      m_ToolTip.SetToolTip(checkBoxAlternateQuoting, "A quote is only regarded as closing quote if it is followed by linefeed or delimi" +
        "ter");
      checkBoxAlternateQuoting.UseVisualStyleBackColor = true;
      checkBoxAlternateQuoting.Visible = false;
      //
      // m_FileSettingBindingSource
      //
      m_FileSettingBindingSource.AllowNew = false;
      m_FileSettingBindingSource.DataSource = typeof(CsvTools.CsvFile);
      //
      // comboBoxTrim
      //
      comboBoxTrim.DisplayMember = "Display";
      comboBoxTrim.Dock = System.Windows.Forms.DockStyle.Top;
      comboBoxTrim.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      comboBoxTrim.Location = new System.Drawing.Point(153, 113);
      comboBoxTrim.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      comboBoxTrim.Name = "comboBoxTrim";
      comboBoxTrim.Size = new System.Drawing.Size(291, 28);
      comboBoxTrim.TabIndex = 10;
      m_ToolTip.SetToolTip(comboBoxTrim, "None will not remove whitespace; Unquoted will remove white spaces if the column " +
        "was not quoted; All will remove white spaces even if the column was quoted");
      comboBoxTrim.ValueMember = "ID";
      comboBoxTrim.SelectedIndexChanged += new System.EventHandler(SetTrimming);
      //
      // m_Label2
      //
      m_Label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
      m_Label2.AutoSize = true;
      m_Label2.ForeColor = System.Drawing.Color.Teal;
      m_Label2.Location = new System.Drawing.Point(451, 152);
      m_Label2.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
      m_Label2.Name = "m_Label2";
      m_Label2.Size = new System.Drawing.Size(18, 20);
      m_Label2.TabIndex = 14;
      m_Label2.Text = "1\r\n";
      //
      // m_RichTextBox02
      //
      m_RichTextBox02.BackColor = System.Drawing.SystemColors.Window;
      m_RichTextBox02.BorderStyle = System.Windows.Forms.BorderStyle.None;
      m_RichTextBox02.Delimiter = ',';
      m_RichTextBox02.DisplaySpace = true;
      m_RichTextBox02.Dock = System.Windows.Forms.DockStyle.Top;
      m_RichTextBox02.Escape = '\\';
      m_RichTextBox02.Location = new System.Drawing.Point(472, 210);
      m_RichTextBox02.Margin = new System.Windows.Forms.Padding(0);
      m_RichTextBox02.Name = "m_RichTextBox02";
      m_RichTextBox02.Quote = '\"';
      m_RichTextBox02.ReadOnly = true;
      tableLayoutPanel1.SetRowSpan(m_RichTextBox02, 2);
      m_RichTextBox02.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      m_RichTextBox02.Size = new System.Drawing.Size(172, 62);
      m_RichTextBox02.TabIndex = 23;
      m_RichTextBox02.Text = "Example ";
      m_RichTextBox02.WordWrap = false;
      //
      // m_RichTextBox01
      //
      m_RichTextBox01.BackColor = System.Drawing.SystemColors.Window;
      m_RichTextBox01.BorderStyle = System.Windows.Forms.BorderStyle.None;
      m_RichTextBox01.Delimiter = ',';
      m_RichTextBox01.DisplaySpace = true;
      m_RichTextBox01.Dock = System.Windows.Forms.DockStyle.Top;
      m_RichTextBox01.Escape = '\\';
      m_RichTextBox01.Location = new System.Drawing.Point(472, 178);
      m_RichTextBox01.Margin = new System.Windows.Forms.Padding(0);
      m_RichTextBox01.Name = "m_RichTextBox01";
      m_RichTextBox01.Quote = '\"';
      m_RichTextBox01.ReadOnly = true;
      m_RichTextBox01.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      m_RichTextBox01.Size = new System.Drawing.Size(172, 32);
      m_RichTextBox01.TabIndex = 19;
      m_RichTextBox01.Text = " a Trimming ";
      m_RichTextBox01.WordWrap = false;
      //
      // m_RichTextBox00
      //
      m_RichTextBox00.BackColor = System.Drawing.SystemColors.Window;
      m_RichTextBox00.BorderStyle = System.Windows.Forms.BorderStyle.None;
      m_RichTextBox00.Delimiter = ',';
      m_RichTextBox00.DisplaySpace = true;
      m_RichTextBox00.Dock = System.Windows.Forms.DockStyle.Top;
      m_RichTextBox00.Escape = '\\';
      m_RichTextBox00.Location = new System.Drawing.Point(472, 146);
      m_RichTextBox00.Margin = new System.Windows.Forms.Padding(0);
      m_RichTextBox00.Name = "m_RichTextBox00";
      m_RichTextBox00.Quote = '\"';
      m_RichTextBox00.ReadOnly = true;
      m_RichTextBox00.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      m_RichTextBox00.Size = new System.Drawing.Size(172, 32);
      m_RichTextBox00.TabIndex = 15;
      m_RichTextBox00.Text = "This is ";
      m_RichTextBox00.WordWrap = false;
      //
      // m_RichTextBox11
      //
      m_RichTextBox11.BackColor = System.Drawing.SystemColors.Window;
      m_RichTextBox11.BorderStyle = System.Windows.Forms.BorderStyle.None;
      m_RichTextBox11.Delimiter = ',';
      m_RichTextBox11.DisplaySpace = true;
      m_RichTextBox11.Dock = System.Windows.Forms.DockStyle.Top;
      m_RichTextBox11.Escape = '\\';
      m_RichTextBox11.Location = new System.Drawing.Point(644, 178);
      m_RichTextBox11.Margin = new System.Windows.Forms.Padding(0);
      m_RichTextBox11.Name = "m_RichTextBox11";
      m_RichTextBox11.Quote = '\"';
      m_RichTextBox11.ReadOnly = true;
      m_RichTextBox11.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      m_RichTextBox11.Size = new System.Drawing.Size(289, 32);
      m_RichTextBox11.TabIndex = 20;
      m_RichTextBox11.Text = "Column with \" Quote";
      m_RichTextBox11.WordWrap = false;
      //
      // m_RichTextBox12
      //
      m_RichTextBox12.BackColor = System.Drawing.SystemColors.Window;
      m_RichTextBox12.BorderStyle = System.Windows.Forms.BorderStyle.None;
      m_RichTextBox12.Delimiter = ',';
      m_RichTextBox12.DisplaySpace = true;
      m_RichTextBox12.Dock = System.Windows.Forms.DockStyle.Top;
      m_RichTextBox12.Escape = '\\';
      m_RichTextBox12.Location = new System.Drawing.Point(644, 210);
      m_RichTextBox12.Margin = new System.Windows.Forms.Padding(0);
      m_RichTextBox12.Name = "m_RichTextBox12";
      m_RichTextBox12.Quote = '\"';
      m_RichTextBox12.ReadOnly = true;
      tableLayoutPanel1.SetRowSpan(m_RichTextBox12, 2);
      m_RichTextBox12.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      m_RichTextBox12.Size = new System.Drawing.Size(289, 62);
      m_RichTextBox12.TabIndex = 24;
      m_RichTextBox12.Text = "Column with \nLinefeed";
      m_RichTextBox12.WordWrap = false;
      //
      // m_Label1
      //
      m_Label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
      m_Label1.AutoSize = true;
      m_Label1.ForeColor = System.Drawing.Color.Teal;
      m_Label1.Location = new System.Drawing.Point(451, 184);
      m_Label1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
      m_Label1.Name = "m_Label1";
      m_Label1.Size = new System.Drawing.Size(18, 20);
      m_Label1.TabIndex = 18;
      m_Label1.Text = "2";
      //
      // m_Label3
      //
      m_Label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
      m_Label3.AutoSize = true;
      m_Label3.ForeColor = System.Drawing.Color.Teal;
      m_Label3.Location = new System.Drawing.Point(451, 232);
      m_Label3.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
      m_Label3.Name = "m_Label3";
      tableLayoutPanel1.SetRowSpan(m_Label3, 2);
      m_Label3.Size = new System.Drawing.Size(18, 20);
      m_Label3.TabIndex = 22;
      m_Label3.Text = "3";
      //
      // m_LabelInfoQuoting
      //
      m_LabelInfoQuoting.Anchor = System.Windows.Forms.AnchorStyles.Left;
      m_LabelInfoQuoting.AutoSize = true;
      m_LabelInfoQuoting.BackColor = System.Drawing.SystemColors.Info;
      m_LabelInfoQuoting.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      tableLayoutPanel1.SetColumnSpan(m_LabelInfoQuoting, 3);
      m_LabelInfoQuoting.ForeColor = System.Drawing.SystemColors.InfoText;
      m_LabelInfoQuoting.Location = new System.Drawing.Point(452, 116);
      m_LabelInfoQuoting.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      m_LabelInfoQuoting.Name = "m_LabelInfoQuoting";
      m_LabelInfoQuoting.Size = new System.Drawing.Size(332, 22);
      m_LabelInfoQuoting.TabIndex = 11;
      m_LabelInfoQuoting.Text = "Not possible to have leading or trailing spaces";
      //
      // m_ErrorProvider
      //
      m_ErrorProvider.ContainerControl = this;
      //
      // tableLayoutPanel1
      //
      tableLayoutPanel1.AutoSize = true;
      tableLayoutPanel1.ColumnCount = 6;
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.90994F));
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.09006F));
      tableLayoutPanel1.Controls.Add(m_LabelQuote, 0, 0);
      tableLayoutPanel1.Controls.Add(m_LabelEscapeCharacter, 0, 1);
      tableLayoutPanel1.Controls.Add(m_LabelQuotePlaceholer, 0, 2);
      tableLayoutPanel1.Controls.Add(m_Label5, 1, 8);
      tableLayoutPanel1.Controls.Add(label3, 0, 6);
      tableLayoutPanel1.Controls.Add(label4, 0, 7);
      tableLayoutPanel1.Controls.Add(label2, 0, 5);
      tableLayoutPanel1.Controls.Add(label1, 0, 4);
      tableLayoutPanel1.Controls.Add(m_RichTextBoxSrc, 1, 4);
      tableLayoutPanel1.Controls.Add(m_RichTextBox10, 5, 4);
      tableLayoutPanel1.Controls.Add(m_RichTextBox11, 5, 5);
      tableLayoutPanel1.Controls.Add(m_RichTextBox12, 5, 6);
      tableLayoutPanel1.Controls.Add(m_TextBoxQuote, 2, 0);
      tableLayoutPanel1.Controls.Add(m_TextBoxEscape, 2, 1);
      tableLayoutPanel1.Controls.Add(m_TextBoxQuotePlaceHolder, 2, 2);
      tableLayoutPanel1.Controls.Add(comboBoxTrim, 2, 3);
      tableLayoutPanel1.Controls.Add(m_LabelTrim, 0, 3);
      tableLayoutPanel1.Controls.Add(m_RichTextBox00, 4, 4);
      tableLayoutPanel1.Controls.Add(m_RichTextBox01, 4, 5);
      tableLayoutPanel1.Controls.Add(m_RichTextBox02, 4, 6);
      tableLayoutPanel1.Controls.Add(m_Label2, 3, 4);
      tableLayoutPanel1.Controls.Add(m_Label1, 3, 5);
      tableLayoutPanel1.Controls.Add(m_Label3, 3, 6);
      tableLayoutPanel1.Controls.Add(m_LabelInfoQuoting, 3, 3);
      tableLayoutPanel1.Controls.Add(checkBoxQualifyAlways, 3, 1);
      tableLayoutPanel1.Controls.Add(checkBoxQualifyOnlyNeeded, 3, 2);
      tableLayoutPanel1.Controls.Add(checkBoxAlternateQuoting, 3, 0);
      tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
      tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      tableLayoutPanel1.Name = "tableLayoutPanel1";
      tableLayoutPanel1.RowCount = 9;
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel1.Size = new System.Drawing.Size(933, 294);
      tableLayoutPanel1.TabIndex = 0;
      //
      // label3
      //
      label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
      label3.AutoSize = true;
      label3.ForeColor = System.Drawing.Color.Teal;
      label3.Location = new System.Drawing.Point(3, 216);
      label3.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
      label3.Name = "label3";
      label3.Size = new System.Drawing.Size(18, 20);
      label3.TabIndex = 21;
      label3.Text = "3";
      //
      // label4
      //
      label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
      label4.AutoSize = true;
      label4.ForeColor = System.Drawing.Color.Teal;
      label4.Location = new System.Drawing.Point(3, 248);
      label4.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
      label4.Name = "label4";
      label4.Size = new System.Drawing.Size(18, 20);
      label4.TabIndex = 25;
      label4.Text = "4";
      //
      // label2
      //
      label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
      label2.AutoSize = true;
      label2.ForeColor = System.Drawing.Color.Teal;
      label2.Location = new System.Drawing.Point(3, 184);
      label2.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
      label2.Name = "label2";
      label2.Size = new System.Drawing.Size(18, 20);
      label2.TabIndex = 17;
      label2.Text = "2";
      //
      // label1
      //
      label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
      label1.AutoSize = true;
      label1.ForeColor = System.Drawing.Color.Teal;
      label1.Location = new System.Drawing.Point(3, 152);
      label1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
      label1.Name = "label1";
      label1.Size = new System.Drawing.Size(18, 20);
      label1.TabIndex = 12;
      label1.Text = "1\r\n";
      //
      // m_RichTextBoxSrc
      //
      m_RichTextBoxSrc.BackColor = System.Drawing.SystemColors.Window;
      m_RichTextBoxSrc.BorderStyle = System.Windows.Forms.BorderStyle.None;
      tableLayoutPanel1.SetColumnSpan(m_RichTextBoxSrc, 2);
      m_RichTextBoxSrc.DataBindings.Add(new System.Windows.Forms.Binding("Delimiter", m_FileFormatBindingSource, "FieldDelimiterChar", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      m_RichTextBoxSrc.Delimiter = ';';
      m_RichTextBoxSrc.DisplaySpace = true;
      m_RichTextBoxSrc.Dock = System.Windows.Forms.DockStyle.Top;
      m_RichTextBoxSrc.Escape = '>';
      m_RichTextBoxSrc.Location = new System.Drawing.Point(24, 146);
      m_RichTextBoxSrc.Margin = new System.Windows.Forms.Padding(0);
      m_RichTextBoxSrc.Name = "m_RichTextBoxSrc";
      m_RichTextBoxSrc.Quote = '\"';
      m_RichTextBoxSrc.ReadOnly = true;
      tableLayoutPanel1.SetRowSpan(m_RichTextBoxSrc, 4);
      m_RichTextBoxSrc.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      m_RichTextBoxSrc.Size = new System.Drawing.Size(424, 128);
      m_RichTextBoxSrc.TabIndex = 13;
      m_RichTextBoxSrc.Text = "\"This is \";Column with:, Delimiter\n a Trimming ;Column with \"\" Quote\nExample ;\"Co" +
    "lumn with \nLinefeed\"";
      //
      // checkBoxQualifyAlways
      //
      checkBoxQualifyAlways.Anchor = System.Windows.Forms.AnchorStyles.Left;
      checkBoxQualifyAlways.AutoSize = true;
      tableLayoutPanel1.SetColumnSpan(checkBoxQualifyAlways, 3);
      checkBoxQualifyAlways.DataBindings.Add(new System.Windows.Forms.Binding("Checked", m_FileFormatBindingSource, "QualifyAlways", true));
      checkBoxQualifyAlways.Location = new System.Drawing.Point(452, 42);
      checkBoxQualifyAlways.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      checkBoxQualifyAlways.Name = "checkBoxQualifyAlways";
      checkBoxQualifyAlways.Size = new System.Drawing.Size(136, 24);
      checkBoxQualifyAlways.TabIndex = 3;
      checkBoxQualifyAlways.Text = "Qualify Always";
      checkBoxQualifyAlways.UseVisualStyleBackColor = true;
      checkBoxQualifyAlways.Visible = false;
      //
      // checkBoxQualifyOnlyNeeded
      //
      checkBoxQualifyOnlyNeeded.Anchor = System.Windows.Forms.AnchorStyles.Left;
      checkBoxQualifyOnlyNeeded.AutoSize = true;
      tableLayoutPanel1.SetColumnSpan(checkBoxQualifyOnlyNeeded, 3);
      checkBoxQualifyOnlyNeeded.DataBindings.Add(new System.Windows.Forms.Binding("Checked", m_FileFormatBindingSource, "QualifyOnlyIfNeeded", true));
      checkBoxQualifyOnlyNeeded.Location = new System.Drawing.Point(452, 78);
      checkBoxQualifyOnlyNeeded.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      checkBoxQualifyOnlyNeeded.Name = "checkBoxQualifyOnlyNeeded";
      checkBoxQualifyOnlyNeeded.Size = new System.Drawing.Size(192, 24);
      checkBoxQualifyOnlyNeeded.TabIndex = 4;
      checkBoxQualifyOnlyNeeded.Text = "Qualify Only If Needed";
      checkBoxQualifyOnlyNeeded.UseVisualStyleBackColor = true;
      checkBoxQualifyOnlyNeeded.Visible = false;
      //
      // QuotingControl
      //
      AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      Controls.Add(tableLayoutPanel1);
      Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      MinimumSize = new System.Drawing.Size(933, 0);
      Name = "QuotingControl";
      Size = new System.Drawing.Size(933, 330);
      ((System.ComponentModel.ISupportInitialize)(m_FileFormatBindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(m_FileSettingBindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(m_ErrorProvider)).EndInit();
      tableLayoutPanel1.ResumeLayout(false);
      tableLayoutPanel1.PerformLayout();
      ResumeLayout(false);
      PerformLayout();
    }

    #endregion Vom Komponenten-Designer generierter Code

    private void FormatPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(FileFormat.FieldDelimiter))
        SetToolTipPlaceholder();
    }

    private void QuoteChanged(object sender, EventArgs e)
    {
      SetTrimming(sender, e);
      SetToolTipPlaceholder();
    }

    private void SetCboTrim(TrimmingOption trim) => comboBoxTrim.SafeInvokeNoHandleNeeded(() =>
    {
      foreach (var ite in comboBoxTrim.Items)
      {
        var item = (DisplayItem<int>)ite;
        if (item.ID == (int)trim)
        {
          comboBoxTrim.SelectedItem = ite;
          break;
        }
      }
    });

    private void SetToolTipPlaceholder() => this.SafeInvoke(() =>
                                                                    {
                                                                      m_ErrorProvider.SetError(m_TextBoxQuote, "");

                                                                      var quote = FileFormat.GetChar(m_TextBoxQuote.Text).ToString(System.Globalization.CultureInfo.CurrentCulture);

                                                                      if (quote != "\0" && quote != "'" && quote != "\"")
                                                                        m_ErrorProvider.SetError(m_TextBoxQuote, "Unusual Quoting character");

                                                                      if (m_RichTextBoxSrc.Delimiter == quote[0])
                                                                        m_ErrorProvider.SetError(m_TextBoxQuote, "Delimiter and Quote have to be different");

                                                                      if (quote == "\0")
                                                                        quote = null;

                                                                      if (quote == null)
                                                                      {
                                                                        m_RichTextBox10.Text = "<Not possible>";
                                                                        m_RichTextBox10.SelectAll();
                                                                        m_RichTextBox10.SelectionColor = Color.Red;

                                                                        m_RichTextBox12.Text = "<Not possible>";
                                                                        m_RichTextBox12.SelectAll();
                                                                        m_RichTextBox12.SelectionColor = Color.Red;
                                                                      }
                                                                      else
                                                                      {
                                                                        m_RichTextBox10.Text = "Column with:" + m_RichTextBoxSrc.Delimiter + " Delimiter";
                                                                        m_RichTextBox12.Text = "Column with \nLinefeed";
                                                                      }

                                                                      m_RichTextBox11.Text = "Column with:" + quote + " Quote";
                                                                      // richTextBox11.Quote = quote[0];

                                                                      var newToolTip = m_IsWriteSetting
                                                                                                                                  ? "Start the column with a quote, if a quote is part of the text the quote is replaced with a placeholder."
                                                                                                                                  : "If the placeholder is part of the text it will be replaced with the quoting character.";

                                                                      var sampleText = quote + "This is " + quote + m_RichTextBoxSrc.Delimiter + quote + "Column with:" +
                                                                                       m_RichTextBoxSrc.Delimiter + " Delimiter" + quote + "\r\n" +
                                                                                       quote + " a Trimming " + quote + m_RichTextBoxSrc.Delimiter + quote + "Column with:{*} Quote" +
                                                                                       quote + "\r\n" +
                                                                                       "Example " + m_RichTextBoxSrc.Delimiter + quote + "Column with \r\nLinefeed" + quote;

                                                                      if (!string.IsNullOrEmpty(m_TextBoxQuotePlaceHolder.Text) && !string.IsNullOrEmpty(quote))
                                                                      {
                                                                        newToolTip += m_IsWriteSetting
                                                                          ? $"\r\nhello {quote} world ->{quote}hello {m_TextBoxQuotePlaceHolder.Text} world{quote}"
                                                                          : $"\r\n{quote}hello {m_TextBoxQuotePlaceHolder.Text} world{quote} -> hello {quote} world";

                                                                        sampleText = sampleText.Replace("{*}", m_TextBoxQuotePlaceHolder.Text);
                                                                      }

                                                                      if (checkBoxAlternateQuoting.Checked)
                                                                      {
                                                                        sampleText = sampleText.Replace("{*}", quote);
                                                                      }
                                                                      else
                                                                      {
                                                                        sampleText = string.IsNullOrEmpty(m_TextBoxEscape.Text) ? sampleText.Replace("{*}", quote + quote) : sampleText.Replace("{*}", m_TextBoxEscape.Text + quote);
                                                                      }

                                                                      m_RichTextBoxSrc.Text = sampleText;
                                                                      m_RichTextBoxSrc.Quote = quote?[0] ?? '\0';

                                                                      m_ToolTip.SetToolTip(m_TextBoxQuotePlaceHolder, newToolTip);
                                                                    });
  }
}