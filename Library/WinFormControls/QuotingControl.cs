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

    private CheckBox m_CheckBoxAlternateQuoting;
    private ComboBox m_ComboBoxTrim;
    private CsvFile m_CsvFile;
    private ErrorProvider m_ErrorProvider;
    private BindingSource m_FileFormatBindingSource;
    private BindingSource m_FileSettingBindingSource;
    private bool m_IsWriteSetting;
    private Label m_Label1;
    private Label m_Label2;
    private Label m_Label3;
    private Label m_Label5;
    private Label m_Label8;
    private Label m_LabelEscapeCharacter;
    private Label m_LabelInfoQuoting;
    private Label m_LabelQuote;
    private Label m_LabelQuotePlaceholer;
    private Label m_LabelTrim;
    private CSVRichTextBox m_RichTextBox;
    private CSVRichTextBox m_RichTextBox00;
    private CSVRichTextBox m_RichTextBox01;
    private CSVRichTextBox m_RichTextBox02;
    private CSVRichTextBox m_RichTextBox10;
    private CSVRichTextBox m_RichTextBox11;
    private CSVRichTextBox m_RichTextBox12;
    private TableLayoutPanel m_TableLayoutPanel1;
    private TextBox m_TextBoxEscape;
    private TextBox m_TextBoxQuote;
    private TextBox m_TextBoxQuotePlaceHolder;
    private TableLayoutPanel tableLayoutPanel1;
    private TableLayoutPanel tableLayoutPanel2;
    private ToolTip m_ToolTip;

    /// <summary>
    ///   CTOR of QuotingControl
    /// </summary>
    public QuotingControl()
    {
      InitializeComponent();
      m_ComboBoxTrim.Items.Add(new DisplayItem<int>(0, "None"));
      m_ComboBoxTrim.Items.Add(new DisplayItem<int>(1, "Unquoted"));
      m_ComboBoxTrim.Items.Add(new DisplayItem<int>(3, "All"));
    }

    /// <summary>
    ///   The CSV File
    /// </summary>
    public CsvFile CsvFile
    {
      get => m_CsvFile;

      set
      {
        m_CsvFile = value;
        if (m_CsvFile == null) return;
        m_FileSettingBindingSource.DataSource = m_CsvFile;
        m_FileFormatBindingSource.DataSource = m_CsvFile.FileFormat;
        m_CsvFile.FileFormat.PropertyChanged += CsvFile_PropertyChanged;
        SetCboTrim(m_CsvFile.TrimmingOption);
        SetDelimiter();
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
        Contract.Assume(m_ComboBoxTrim != null);
        Contract.Assume(m_CheckBoxAlternateQuoting != null);
        m_IsWriteSetting = value;
        m_LabelInfoQuoting.Visible = !value;
        m_ComboBoxTrim.Enabled = !value;
        m_CheckBoxAlternateQuoting.Visible = !value;
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
      if (disposing) components?.Dispose();
      base.Dispose(disposing);
    }

    private void ComboBoxTrim_SelectedIndexChanged(object sender, EventArgs e)
    {
      Contract.Requires(m_ComboBoxTrim != null);
      if (m_ComboBoxTrim.SelectedItem == null) return;

      if (!m_IsWriteSetting)
      {
        m_RichTextBox00.Clear();
        m_RichTextBox01.Clear();
        m_RichTextBox02.Clear();
        switch (((DisplayItem<int>)m_ComboBoxTrim.SelectedItem).ID)
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
      }
    }

    /// <summary>
    ///   Initializes the component.
    /// </summary>
    private void InitializeComponent()
    {
      components = new System.ComponentModel.Container();
      m_LabelQuote = new System.Windows.Forms.Label();
      m_LabelQuotePlaceholer = new System.Windows.Forms.Label();
      m_TextBoxEscape = new System.Windows.Forms.TextBox();
      m_FileFormatBindingSource = new System.Windows.Forms.BindingSource(components);
      m_LabelEscapeCharacter = new System.Windows.Forms.Label();
      m_LabelTrim = new System.Windows.Forms.Label();
      m_TextBoxQuote = new System.Windows.Forms.TextBox();
      m_TextBoxQuotePlaceHolder = new System.Windows.Forms.TextBox();
      m_CheckBoxAlternateQuoting = new System.Windows.Forms.CheckBox();
      m_FileSettingBindingSource = new System.Windows.Forms.BindingSource(components);
      m_ToolTip = new System.Windows.Forms.ToolTip(components);
      m_ComboBoxTrim = new System.Windows.Forms.ComboBox();
      m_TableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      m_Label2 = new System.Windows.Forms.Label();
      m_RichTextBox02 = new CsvTools.CSVRichTextBox();
      m_RichTextBox01 = new CsvTools.CSVRichTextBox();
      m_RichTextBox00 = new CsvTools.CSVRichTextBox();
      m_RichTextBox10 = new CsvTools.CSVRichTextBox();
      m_RichTextBox11 = new CsvTools.CSVRichTextBox();
      m_RichTextBox12 = new CsvTools.CSVRichTextBox();
      m_Label1 = new System.Windows.Forms.Label();
      m_Label3 = new System.Windows.Forms.Label();
      m_LabelInfoQuoting = new System.Windows.Forms.Label();
      m_Label8 = new System.Windows.Forms.Label();
      m_ErrorProvider = new System.Windows.Forms.ErrorProvider(components);
      m_Label5 = new System.Windows.Forms.Label();
      m_RichTextBox = new CsvTools.CSVRichTextBox();
      tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      ((System.ComponentModel.ISupportInitialize)(m_FileFormatBindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(m_FileSettingBindingSource)).BeginInit();
      m_TableLayoutPanel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(m_ErrorProvider)).BeginInit();
      tableLayoutPanel1.SuspendLayout();
      tableLayoutPanel2.SuspendLayout();
      SuspendLayout();
      // 
      // m_LabelQuote
      // 
      m_LabelQuote.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
      m_LabelQuote.AutoSize = true;
      m_LabelQuote.Location = new System.Drawing.Point(59, 5);
      m_LabelQuote.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
      m_LabelQuote.Name = "m_LabelQuote";
      m_LabelQuote.Size = new System.Drawing.Size(39, 13);
      m_LabelQuote.TabIndex = 0;
      m_LabelQuote.Text = "Quote:";
      // 
      // m_LabelQuotePlaceholer
      // 
      m_LabelQuotePlaceholer.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
      m_LabelQuotePlaceholer.AutoSize = true;
      m_LabelQuotePlaceholer.Location = new System.Drawing.Point(32, 57);
      m_LabelQuotePlaceholer.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
      m_LabelQuotePlaceholer.Name = "m_LabelQuotePlaceholer";
      m_LabelQuotePlaceholer.Size = new System.Drawing.Size(66, 13);
      m_LabelQuotePlaceholer.TabIndex = 5;
      m_LabelQuotePlaceholer.Text = "Placeholder:";
      // 
      // m_TextBoxEscape
      // 
      m_TextBoxEscape.DataBindings.Add(new System.Windows.Forms.Binding("Text", m_FileFormatBindingSource, "EscapeCharacter", true));
      m_TextBoxEscape.Location = new System.Drawing.Point(104, 29);
      m_TextBoxEscape.Name = "m_TextBoxEscape";
      m_TextBoxEscape.Size = new System.Drawing.Size(45, 20);
      m_TextBoxEscape.TabIndex = 4;
      // 
      // m_FileFormatBindingSource
      // 
      m_FileFormatBindingSource.AllowNew = false;
      m_FileFormatBindingSource.DataSource = typeof(CsvTools.FileFormat);
      // 
      // m_LabelEscapeCharacter
      // 
      m_LabelEscapeCharacter.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
      m_LabelEscapeCharacter.AutoSize = true;
      m_LabelEscapeCharacter.Location = new System.Drawing.Point(3, 31);
      m_LabelEscapeCharacter.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
      m_LabelEscapeCharacter.Name = "m_LabelEscapeCharacter";
      m_LabelEscapeCharacter.Size = new System.Drawing.Size(95, 13);
      m_LabelEscapeCharacter.TabIndex = 3;
      m_LabelEscapeCharacter.Text = "Escape Character:";
      // 
      // m_LabelTrim
      // 
      m_LabelTrim.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
      m_LabelTrim.AutoSize = true;
      m_LabelTrim.Location = new System.Drawing.Point(12, 83);
      m_LabelTrim.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
      m_LabelTrim.Name = "m_LabelTrim";
      m_LabelTrim.Size = new System.Drawing.Size(86, 13);
      m_LabelTrim.TabIndex = 7;
      m_LabelTrim.Text = "Trimming Option:";
      // 
      // m_TextBoxQuote
      // 
      m_TextBoxQuote.DataBindings.Add(new System.Windows.Forms.Binding("Text", m_FileFormatBindingSource, "FieldQualifier", true));
      m_TextBoxQuote.Location = new System.Drawing.Point(104, 3);
      m_TextBoxQuote.Name = "m_TextBoxQuote";
      m_TextBoxQuote.Size = new System.Drawing.Size(45, 20);
      m_TextBoxQuote.TabIndex = 1;
      m_ToolTip.SetToolTip(m_TextBoxQuote, "Columns may be delimited with a quoting character; the quotes are removed by the " +
        "reading applications.");
      m_TextBoxQuote.TextChanged += new System.EventHandler(TextBoxQuote_TextChanged);
      // 
      // m_TextBoxQuotePlaceHolder
      // 
      m_TextBoxQuotePlaceHolder.AutoCompleteCustomSource.AddRange(new string[] {
            "{q}",
            "&quot;"});
      m_TextBoxQuotePlaceHolder.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
      m_TextBoxQuotePlaceHolder.DataBindings.Add(new System.Windows.Forms.Binding("Text", m_FileFormatBindingSource, "QuotePlaceholder", true));
      m_TextBoxQuotePlaceHolder.Location = new System.Drawing.Point(104, 55);
      m_TextBoxQuotePlaceHolder.Name = "m_TextBoxQuotePlaceHolder";
      m_TextBoxQuotePlaceHolder.Size = new System.Drawing.Size(45, 20);
      m_TextBoxQuotePlaceHolder.TabIndex = 6;
      m_ToolTip.SetToolTip(m_TextBoxQuotePlaceHolder, "If this placeholder is part of the text it will be replaced with the quoting char" +
        "acter");
      m_TextBoxQuotePlaceHolder.TextChanged += new System.EventHandler(TextBoxQuotePlaceHolder_TextChanged);
      // 
      // m_CheckBoxAlternateQuoting
      // 
      m_CheckBoxAlternateQuoting.AutoSize = true;
      m_CheckBoxAlternateQuoting.DataBindings.Add(new System.Windows.Forms.Binding("Checked", m_FileSettingBindingSource, "AlternateQuoting", true));
      m_CheckBoxAlternateQuoting.Location = new System.Drawing.Point(215, 3);
      m_CheckBoxAlternateQuoting.Name = "m_CheckBoxAlternateQuoting";
      m_CheckBoxAlternateQuoting.Size = new System.Drawing.Size(148, 17);
      m_CheckBoxAlternateQuoting.TabIndex = 2;
      m_CheckBoxAlternateQuoting.Text = "Context Sensitive Quoting";
      m_ToolTip.SetToolTip(m_CheckBoxAlternateQuoting, "A quote is only regarded as closing quote if it is followed by linefeed or delimi" +
        "ter");
      m_CheckBoxAlternateQuoting.UseVisualStyleBackColor = true;
      // 
      // m_FileSettingBindingSource
      // 
      m_FileSettingBindingSource.AllowNew = false;
      m_FileSettingBindingSource.DataSource = typeof(CsvTools.CsvFile);
      // 
      // m_ComboBoxTrim
      // 
      m_ComboBoxTrim.DisplayMember = "Display";
      m_ComboBoxTrim.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      m_ComboBoxTrim.Location = new System.Drawing.Point(104, 81);
      m_ComboBoxTrim.Name = "m_ComboBoxTrim";
      m_ComboBoxTrim.Size = new System.Drawing.Size(105, 21);
      m_ComboBoxTrim.TabIndex = 8;
      m_ToolTip.SetToolTip(m_ComboBoxTrim, "None will not remove whitespaces; Unquoted will remove whitespaces if the column " +
        "was not quoted; All will remove whitespaces even if the column was quoted");
      m_ComboBoxTrim.ValueMember = "ID";
      m_ComboBoxTrim.SelectedIndexChanged += new System.EventHandler(ComboBoxTrim_SelectedIndexChanged);
      // 
      // m_TableLayoutPanel1
      // 
      m_TableLayoutPanel1.BackColor = System.Drawing.SystemColors.Window;
      m_TableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Outset;
      m_TableLayoutPanel1.ColumnCount = 3;
      m_TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 17F));
      m_TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38.88889F));
      m_TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 61.11111F));
      m_TableLayoutPanel1.Controls.Add(m_Label2, 0, 0);
      m_TableLayoutPanel1.Controls.Add(m_RichTextBox02, 1, 2);
      m_TableLayoutPanel1.Controls.Add(m_RichTextBox01, 1, 1);
      m_TableLayoutPanel1.Controls.Add(m_RichTextBox00, 1, 0);
      m_TableLayoutPanel1.Controls.Add(m_RichTextBox10, 2, 0);
      m_TableLayoutPanel1.Controls.Add(m_RichTextBox11, 2, 1);
      m_TableLayoutPanel1.Controls.Add(m_RichTextBox12, 2, 2);
      m_TableLayoutPanel1.Controls.Add(m_Label1, 0, 1);
      m_TableLayoutPanel1.Controls.Add(m_Label3, 0, 2);
      m_TableLayoutPanel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
      m_TableLayoutPanel1.Location = new System.Drawing.Point(327, 3);
      m_TableLayoutPanel1.Name = "m_TableLayoutPanel1";
      m_TableLayoutPanel1.RowCount = 3;
      m_TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      m_TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      m_TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      m_TableLayoutPanel1.Size = new System.Drawing.Size(327, 100);
      m_TableLayoutPanel1.TabIndex = 2;
      // 
      // m_Label2
      // 
      m_Label2.AutoSize = true;
      m_Label2.ForeColor = System.Drawing.Color.Teal;
      m_Label2.Location = new System.Drawing.Point(5, 2);
      m_Label2.Name = "m_Label2";
      m_Label2.Size = new System.Drawing.Size(11, 18);
      m_Label2.TabIndex = 0;
      m_Label2.Text = "1\r\n";
      // 
      // m_RichTextBox02
      // 
      m_RichTextBox02.BorderStyle = System.Windows.Forms.BorderStyle.None;
      m_RichTextBox02.Delimiter = ',';
      m_RichTextBox02.DisplaySpace = true;
      m_RichTextBox02.Dock = System.Windows.Forms.DockStyle.Fill;
      m_RichTextBox02.Escape = '\\';
      m_RichTextBox02.Location = new System.Drawing.Point(21, 54);
      m_RichTextBox02.Margin = new System.Windows.Forms.Padding(0);
      m_RichTextBox02.Name = "m_RichTextBox02";
      m_RichTextBox02.Quote = '\"';
      m_RichTextBox02.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      m_RichTextBox02.Size = new System.Drawing.Size(117, 44);
      m_RichTextBox02.TabIndex = 7;
      m_RichTextBox02.Text = "Example ";
      m_RichTextBox02.WordWrap = false;
      // 
      // m_RichTextBox01
      // 
      m_RichTextBox01.BorderStyle = System.Windows.Forms.BorderStyle.None;
      m_RichTextBox01.Delimiter = ',';
      m_RichTextBox01.DisplaySpace = true;
      m_RichTextBox01.Dock = System.Windows.Forms.DockStyle.Fill;
      m_RichTextBox01.Escape = '\\';
      m_RichTextBox01.Location = new System.Drawing.Point(21, 28);
      m_RichTextBox01.Margin = new System.Windows.Forms.Padding(0);
      m_RichTextBox01.Name = "m_RichTextBox01";
      m_RichTextBox01.Quote = '\"';
      m_RichTextBox01.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      m_RichTextBox01.Size = new System.Drawing.Size(117, 24);
      m_RichTextBox01.TabIndex = 4;
      m_RichTextBox01.Text = " a Trimming ";
      m_RichTextBox01.WordWrap = false;
      // 
      // m_RichTextBox00
      // 
      m_RichTextBox00.BorderStyle = System.Windows.Forms.BorderStyle.None;
      m_RichTextBox00.Delimiter = ',';
      m_RichTextBox00.DisplaySpace = true;
      m_RichTextBox00.Dock = System.Windows.Forms.DockStyle.Fill;
      m_RichTextBox00.Escape = '\\';
      m_RichTextBox00.Location = new System.Drawing.Point(21, 2);
      m_RichTextBox00.Margin = new System.Windows.Forms.Padding(0);
      m_RichTextBox00.Name = "m_RichTextBox00";
      m_RichTextBox00.Quote = '\"';
      m_RichTextBox00.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      m_RichTextBox00.Size = new System.Drawing.Size(117, 24);
      m_RichTextBox00.TabIndex = 1;
      m_RichTextBox00.Text = "This is ";
      m_RichTextBox00.WordWrap = false;
      // 
      // m_RichTextBox10
      // 
      m_RichTextBox10.BorderStyle = System.Windows.Forms.BorderStyle.None;
      m_RichTextBox10.Delimiter = ',';
      m_RichTextBox10.DisplaySpace = true;
      m_RichTextBox10.Dock = System.Windows.Forms.DockStyle.Fill;
      m_RichTextBox10.Escape = '\\';
      m_RichTextBox10.Location = new System.Drawing.Point(140, 2);
      m_RichTextBox10.Margin = new System.Windows.Forms.Padding(0);
      m_RichTextBox10.Name = "m_RichTextBox10";
      m_RichTextBox10.Quote = '\"';
      m_RichTextBox10.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      m_RichTextBox10.Size = new System.Drawing.Size(185, 24);
      m_RichTextBox10.TabIndex = 2;
      m_RichTextBox10.Text = "Column with:, Delimiter";
      m_RichTextBox10.WordWrap = false;
      // 
      // m_RichTextBox11
      // 
      m_RichTextBox11.BorderStyle = System.Windows.Forms.BorderStyle.None;
      m_RichTextBox11.Delimiter = ',';
      m_RichTextBox11.DisplaySpace = true;
      m_RichTextBox11.Dock = System.Windows.Forms.DockStyle.Fill;
      m_RichTextBox11.Escape = '\\';
      m_RichTextBox11.Location = new System.Drawing.Point(140, 28);
      m_RichTextBox11.Margin = new System.Windows.Forms.Padding(0);
      m_RichTextBox11.Name = "m_RichTextBox11";
      m_RichTextBox11.Quote = '\"';
      m_RichTextBox11.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      m_RichTextBox11.Size = new System.Drawing.Size(185, 24);
      m_RichTextBox11.TabIndex = 5;
      m_RichTextBox11.Text = "Column with \" Quote";
      m_RichTextBox11.WordWrap = false;
      // 
      // m_RichTextBox12
      // 
      m_RichTextBox12.BorderStyle = System.Windows.Forms.BorderStyle.None;
      m_RichTextBox12.Delimiter = ',';
      m_RichTextBox12.DisplaySpace = true;
      m_RichTextBox12.Dock = System.Windows.Forms.DockStyle.Fill;
      m_RichTextBox12.Escape = '\\';
      m_RichTextBox12.Location = new System.Drawing.Point(140, 54);
      m_RichTextBox12.Margin = new System.Windows.Forms.Padding(0);
      m_RichTextBox12.Name = "m_RichTextBox12";
      m_RichTextBox12.Quote = '\"';
      m_RichTextBox12.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      m_RichTextBox12.Size = new System.Drawing.Size(185, 44);
      m_RichTextBox12.TabIndex = 8;
      m_RichTextBox12.Text = "Column with \nLinefeed";
      m_RichTextBox12.WordWrap = false;
      // 
      // m_Label1
      // 
      m_Label1.AutoSize = true;
      m_Label1.ForeColor = System.Drawing.Color.Teal;
      m_Label1.Location = new System.Drawing.Point(5, 28);
      m_Label1.Name = "m_Label1";
      m_Label1.Size = new System.Drawing.Size(11, 18);
      m_Label1.TabIndex = 3;
      m_Label1.Text = "2";
      // 
      // m_Label3
      // 
      m_Label3.AutoSize = true;
      m_Label3.ForeColor = System.Drawing.Color.Teal;
      m_Label3.Location = new System.Drawing.Point(5, 54);
      m_Label3.Name = "m_Label3";
      m_Label3.Size = new System.Drawing.Size(11, 18);
      m_Label3.TabIndex = 6;
      m_Label3.Text = "3";
      // 
      // m_LabelInfoQuoting
      // 
      m_LabelInfoQuoting.AutoSize = true;
      m_LabelInfoQuoting.BackColor = System.Drawing.SystemColors.Info;
      m_LabelInfoQuoting.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      m_LabelInfoQuoting.ForeColor = System.Drawing.SystemColors.InfoText;
      m_LabelInfoQuoting.Location = new System.Drawing.Point(215, 83);
      m_LabelInfoQuoting.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
      m_LabelInfoQuoting.Name = "m_LabelInfoQuoting";
      m_LabelInfoQuoting.Size = new System.Drawing.Size(225, 15);
      m_LabelInfoQuoting.TabIndex = 9;
      m_LabelInfoQuoting.Text = "Not possible to have leading or trailing spaces";
      m_LabelInfoQuoting.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // m_Label8
      // 
      m_Label8.AutoSize = true;
      m_Label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
      m_Label8.ForeColor = System.Drawing.Color.Teal;
      m_Label8.Location = new System.Drawing.Point(3, 0);
      m_Label8.Name = "m_Label8";
      m_Label8.Size = new System.Drawing.Size(18, 80);
      m_Label8.TabIndex = 0;
      m_Label8.Text = "1\r\n2\r\n3\r\n4";
      // 
      // m_ErrorProvider
      // 
      m_ErrorProvider.ContainerControl = this;
      // 
      // m_Label5
      // 
      m_Label5.AutoSize = true;
      tableLayoutPanel1.SetColumnSpan(m_Label5, 3);
      m_Label5.Location = new System.Drawing.Point(3, 224);
      m_Label5.Name = "m_Label5";
      m_Label5.Size = new System.Drawing.Size(336, 13);
      m_Label5.TabIndex = 10;
      m_Label5.Text = "Tab visualized as »   Linefeed visualized as ¶    Space visualized as ●";
      // 
      // m_RichTextBox
      // 
      m_RichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
      m_RichTextBox.Delimiter = ';';
      m_RichTextBox.DisplaySpace = true;
      m_RichTextBox.Escape = '>';
      m_RichTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
      m_RichTextBox.Location = new System.Drawing.Point(27, 3);
      m_RichTextBox.Name = "m_RichTextBox";
      m_RichTextBox.Quote = '\"';
      m_RichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      m_RichTextBox.Size = new System.Drawing.Size(294, 98);
      m_RichTextBox.TabIndex = 1;
      m_RichTextBox.Text = "\"This is \";Column with:, Delimiter\n a Trimming ;Column with \"\" Quote\nExample ;\"Co" +
    "lumn with \nLinefeed\"";
      m_RichTextBox.WordWrap = false;
      // 
      // tableLayoutPanel1
      // 
      tableLayoutPanel1.ColumnCount = 3;
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 4);
      tableLayoutPanel1.Controls.Add(m_Label5, 0, 5);
      tableLayoutPanel1.Controls.Add(m_LabelQuote, 0, 0);
      tableLayoutPanel1.Controls.Add(m_TextBoxQuote, 1, 0);
      tableLayoutPanel1.Controls.Add(m_LabelEscapeCharacter, 0, 1);
      tableLayoutPanel1.Controls.Add(m_TextBoxEscape, 1, 1);
      tableLayoutPanel1.Controls.Add(m_LabelInfoQuoting, 2, 3);
      tableLayoutPanel1.Controls.Add(m_CheckBoxAlternateQuoting, 2, 0);
      tableLayoutPanel1.Controls.Add(m_LabelQuotePlaceholer, 0, 2);
      tableLayoutPanel1.Controls.Add(m_TextBoxQuotePlaceHolder, 1, 2);
      tableLayoutPanel1.Controls.Add(m_ComboBoxTrim, 1, 3);
      tableLayoutPanel1.Controls.Add(m_LabelTrim, 0, 3);
      tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      tableLayoutPanel1.Name = "tableLayoutPanel1";
      tableLayoutPanel1.RowCount = 6;
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.Size = new System.Drawing.Size(670, 250);
      tableLayoutPanel1.TabIndex = 0;
      // 
      // tableLayoutPanel2
      // 
      tableLayoutPanel2.ColumnCount = 3;
      tableLayoutPanel1.SetColumnSpan(tableLayoutPanel2, 3);
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel2.Controls.Add(m_Label8, 0, 0);
      tableLayoutPanel2.Controls.Add(m_RichTextBox, 1, 0);
      tableLayoutPanel2.Controls.Add(m_TableLayoutPanel1, 2, 0);
      tableLayoutPanel2.Location = new System.Drawing.Point(3, 108);
      tableLayoutPanel2.Name = "tableLayoutPanel2";
      tableLayoutPanel2.RowCount = 1;
      tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel2.Size = new System.Drawing.Size(662, 113);
      tableLayoutPanel2.TabIndex = 100;
      // 
      // QuotingControl
      // 
      AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      Controls.Add(tableLayoutPanel1);
      Name = "QuotingControl";
      Size = new System.Drawing.Size(670, 250);
      ((System.ComponentModel.ISupportInitialize)(m_FileFormatBindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(m_FileSettingBindingSource)).EndInit();
      m_TableLayoutPanel1.ResumeLayout(false);
      m_TableLayoutPanel1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(m_ErrorProvider)).EndInit();
      tableLayoutPanel1.ResumeLayout(false);
      tableLayoutPanel1.PerformLayout();
      tableLayoutPanel2.ResumeLayout(false);
      tableLayoutPanel2.PerformLayout();
      ResumeLayout(false);

    }

    #endregion Vom Komponenten-Designer generierter Code

    private void CsvFile_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(FileFormat.FieldDelimiter)) SetDelimiter();
    }

    private void TextBoxQuote_TextChanged(object sender, EventArgs e)
    {
      ComboBoxTrim_SelectedIndexChanged(sender, e);
      SetToolTipPlaceholder(sender, e);
    }

    private void TextBoxQuotePlaceHolder_TextChanged(object sender, EventArgs e)
    {
      ComboBoxTrim_SelectedIndexChanged(sender, e);
      SetToolTipPlaceholder(sender, e);
    }

    private void SetCboTrim(TrimmingOption trim)
    {
      foreach (var ite in m_ComboBoxTrim.Items)
      {
        var item = (DisplayItem<int>)ite;
        if (item.ID == 0 && trim == TrimmingOption.None)
        {
          m_ComboBoxTrim.SelectedItem = ite;
          break;
        }

        if (item.ID == 1 && trim == TrimmingOption.Unquoted)
        {
          m_ComboBoxTrim.SelectedItem = ite;
          break;
        }

        if (item.ID != 3 || trim != TrimmingOption.All) continue;
        m_ComboBoxTrim.SelectedItem = ite;
        break;
      }
    }

    private void SetDelimiter()
    {
      try
      {
        var delimiter = m_CsvFile.FileFormat.FieldDelimiterChar;
        m_RichTextBox.Delimiter = delimiter;
        SetToolTipPlaceholder(null, null);
      }
      catch
      {
        // ignore
      }
    }

    private void SetToolTipPlaceholder(object sender, EventArgs e)
    {
      this.SafeInvoke(() =>
      {
        m_ErrorProvider.SetError(m_TableLayoutPanel1, "");
        m_ErrorProvider.SetError(m_TextBoxQuote, "");

        if (string.IsNullOrEmpty(m_TextBoxQuote.Text))
          m_ErrorProvider.SetError(m_TableLayoutPanel1, "Without quoting, the delimiter can not be part of a field");
        var quote = FileFormat.GetChar(m_TextBoxQuote.Text).ToString(System.Globalization.CultureInfo.CurrentCulture);

        if (quote != "\0" && quote != "'" && quote != "\"")
          m_ErrorProvider.SetError(m_TextBoxQuote, "Unusual Quoting character");

        if (m_RichTextBox.Delimiter == quote[0])
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
          m_RichTextBox10.Text = "Column with:" + m_RichTextBox.Delimiter + " Delimiter";
          m_RichTextBox12.Text = "Column with \nLinefeed";
        }

        m_RichTextBox11.Text = "Column with:" + quote + " Quote";
        // richTextBox11.Quote = quote[0];

        var newToolTip = m_IsWriteSetting
      ? "Start the column with a quote, if a quote is part of the text the quote is replaced with a placeholder."
      : "If the placeholder is part of the text it will be replaced with the quoting character.";

        var sampleText = quote + "This is " + quote + m_RichTextBox.Delimiter + quote + "Column with:" +
                         m_RichTextBox.Delimiter + " Delimiter" + quote + "\r\n" +
                         quote + " a Trimming " + quote + m_RichTextBox.Delimiter + quote + "Column with:{*} Quote" +
                         quote + "\r\n" +
                         "Example " + m_RichTextBox.Delimiter + quote + "Column with \r\nLinefeed" + quote;

        if (!string.IsNullOrEmpty(m_TextBoxQuotePlaceHolder.Text) && !string.IsNullOrEmpty(quote))
        {
          newToolTip += m_IsWriteSetting
            ? $"\r\nhello {quote} world ->{quote}hello {m_TextBoxQuotePlaceHolder.Text} world{quote}"
            : $"\r\n{quote}hello {m_TextBoxQuotePlaceHolder.Text} world{quote} -> hello {quote} world";

          sampleText = sampleText.Replace("{*}", m_TextBoxQuotePlaceHolder.Text);
        }

        if (m_CheckBoxAlternateQuoting.Checked)
        {
          sampleText = sampleText.Replace("{*}", quote);
        }
        else
        {
          sampleText = string.IsNullOrEmpty(m_TextBoxEscape.Text) ? sampleText.Replace("{*}", quote + quote) : sampleText.Replace("{*}", m_TextBoxEscape.Text + quote);
        }

        m_RichTextBox.Text = sampleText;
        m_RichTextBox.Quote = quote?[0] ?? '\0';

        m_ToolTip.SetToolTip(m_TextBoxQuotePlaceHolder, newToolTip);
      });
    }
  }
}