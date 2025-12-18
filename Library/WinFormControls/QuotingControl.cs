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

using FastColoredTextBoxNS;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

#pragma warning disable CA1416

namespace CsvTools;

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
  private readonly Style m_SpaceStyle = new SyntaxHighlighterDelimitedText.SyntaxHighlightStyleStyleSpace(Brushes.Blue, Brushes.AntiqueWhite);
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
  private Timer m_TimerRebuilt;
  private ToolTip m_ToolTip;
  private SplitContainer m_SplitContainer;
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
        {
          if (m_CsvFile is INotifyPropertyChanged notify)
            notify.PropertyChanged -= FormatPropertyChanged;
        }
        if (m_CsvFile.Equals(value))
          return;
      }
      m_CsvFile = value;
      {
        if (m_CsvFile is INotifyPropertyChanged notify)
          notify.PropertyChanged += FormatPropertyChanged;
      }
      m_CsvSettingBindingSource.DataSource = m_CsvFile;
      // m_CsvSettingBindingSource.ResetBindings(false);
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
    components = new Container();
    var resources = new ComponentResourceManager(typeof(QuotingControl));
    m_SplitContainer = new SplitContainer();
    m_TableLayoutPanelText = new TableLayoutPanel();
    m_FastColoredTextBox = new FastColoredTextBox();
    m_Label6 = new Label();
    m_TableLayoutPanelColumns = new TableLayoutPanel();
    m_Label3 = new Label();
    m_FastColoredTextBox12 = new FastColoredTextBox();
    m_FastColoredTextBox02 = new FastColoredTextBox();
    m_FastColoredTextBox01 = new FastColoredTextBox();
    m_FastColoredTextBox11 = new FastColoredTextBox();
    m_FastColoredTextBox00 = new FastColoredTextBox();
    m_Label2 = new Label();
    m_FastColoredTextBox10 = new FastColoredTextBox();
    m_Label1 = new Label();
    m_Label5 = new Label();
    m_Label4 = new Label();
    m_ToolTip = new ToolTip(components);
    m_RadioButtonNeeded = new RadioButton();
    m_CsvSettingBindingSource = new BindingSource(components);
    m_RadioButtonAlways = new RadioButton();
    m_ComboBoxTrim = new ComboBox();
    m_CheckBoxAlternateQuoting = new CheckBox();
    m_CheckBoxDuplicateQuotingToEscape = new CheckBox();
    m_TextBoxQuote = new PunctuationTextBox();
    m_TextBoxQuotePlaceHolder = new TextBox();
    m_ErrorProvider = new ErrorProvider(components);
    m_TableLayoutPanel = new TableLayoutPanel();
    m_LabelQuote = new Label();
    m_LabelQuotePlaceholder = new Label();
    m_LabelTrim = new Label();
    m_TimerRebuilt = new Timer(components);
    ((ISupportInitialize) m_SplitContainer).BeginInit();
    m_SplitContainer.Panel1.SuspendLayout();
    m_SplitContainer.Panel2.SuspendLayout();
    m_SplitContainer.SuspendLayout();
    m_TableLayoutPanelText.SuspendLayout();
    ((ISupportInitialize) m_FastColoredTextBox).BeginInit();
    m_TableLayoutPanelColumns.SuspendLayout();
    ((ISupportInitialize) m_FastColoredTextBox12).BeginInit();
    ((ISupportInitialize) m_FastColoredTextBox02).BeginInit();
    ((ISupportInitialize) m_FastColoredTextBox01).BeginInit();
    ((ISupportInitialize) m_FastColoredTextBox11).BeginInit();
    ((ISupportInitialize) m_FastColoredTextBox00).BeginInit();
    ((ISupportInitialize) m_FastColoredTextBox10).BeginInit();
    ((ISupportInitialize) m_CsvSettingBindingSource).BeginInit();
    ((ISupportInitialize) m_ErrorProvider).BeginInit();
    m_TableLayoutPanel.SuspendLayout();
    SuspendLayout();
    // 
    // m_SplitContainer
    // 
    m_TableLayoutPanel.SetColumnSpan(m_SplitContainer, 4);
    m_SplitContainer.Dock = DockStyle.Top;
    m_SplitContainer.Location = new Point(3, 82);
    m_SplitContainer.Name = "m_SplitContainer";
    // 
    // m_SplitContainer.Panel1
    // 
    m_SplitContainer.Panel1.Controls.Add(m_TableLayoutPanelText);
    // 
    // m_SplitContainer.Panel2
    // 
    m_SplitContainer.Panel2.Controls.Add(m_TableLayoutPanelColumns);
    m_SplitContainer.Size = new Size(720, 114);
    m_SplitContainer.SplitterDistance = 335;
    m_SplitContainer.TabIndex = 5;
    // 
    // m_TableLayoutPanelText
    // 
    m_TableLayoutPanelText.AutoSize = true;
    m_TableLayoutPanelText.ColumnCount = 1;
    m_TableLayoutPanelText.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44.49112F));
    m_TableLayoutPanelText.Controls.Add(m_FastColoredTextBox, 0, 1);
    m_TableLayoutPanelText.Controls.Add(m_Label6, 0, 0);
    m_TableLayoutPanelText.Dock = DockStyle.Fill;
    m_TableLayoutPanelText.Location = new Point(0, 0);
    m_TableLayoutPanelText.Name = "m_TableLayoutPanelText";
    m_TableLayoutPanelText.RowCount = 4;
    m_TableLayoutPanelText.RowStyles.Add(new RowStyle());
    m_TableLayoutPanelText.RowStyles.Add(new RowStyle());
    m_TableLayoutPanelText.RowStyles.Add(new RowStyle());
    m_TableLayoutPanelText.RowStyles.Add(new RowStyle());
    m_TableLayoutPanelText.Size = new Size(335, 114);
    m_TableLayoutPanelText.TabIndex = 0;
    // 
    // m_FastColoredTextBox
    // 
    m_FastColoredTextBox.AllowDrop = false;
    m_FastColoredTextBox.AutoCompleteBracketsList = new char[]
{
    '(',
    ')',
    '{',
    '}',
    '[',
    ']',
    '"',
    '"',
    '\'',
    '\''
};
    m_FastColoredTextBox.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
    m_FastColoredTextBox.AutoScrollMinSize = new Size(307, 56);
    m_FastColoredTextBox.BackBrush = null;
    m_FastColoredTextBox.CharHeight = 14;
    m_FastColoredTextBox.CharWidth = 8;
    m_FastColoredTextBox.Cursor = Cursors.IBeam;
    m_FastColoredTextBox.DisabledColor = Color.FromArgb(  100,   180,   180,   180);
    m_FastColoredTextBox.Dock = DockStyle.Fill;
    m_FastColoredTextBox.IsReplaceMode = false;
    m_FastColoredTextBox.Location = new Point(3, 16);
    m_FastColoredTextBox.Name = "m_FastColoredTextBox";
    m_FastColoredTextBox.Paddings = new Padding(0);
    m_FastColoredTextBox.ReadOnly = true;
    m_TableLayoutPanelText.SetRowSpan(m_FastColoredTextBox, 3);
    m_FastColoredTextBox.SelectionColor = Color.FromArgb(  60,   0,   0,   255);
    m_FastColoredTextBox.Size = new Size(329, 95);
    m_FastColoredTextBox.TabIndex = 0;
    m_FastColoredTextBox.TabLength = 1;
    m_FastColoredTextBox.TabStop = false;
    m_FastColoredTextBox.Tag = "NoFontChange";
    m_FastColoredTextBox.Text = "\"This is \";Column with:, Delimiter¶\r\n a Trimming ;Column with \"\" Quote¶\r\nExample ;\"Column with ¶\r\nLinefeed\"";
    m_FastColoredTextBox.Zoom = 100;
    // 
    // m_Label6
    // 
    m_Label6.Anchor = AnchorStyles.None;
    m_Label6.AutoSize = true;
    m_Label6.ForeColor = Color.Teal;
    m_Label6.Location = new Point(130, 0);
    m_Label6.Name = "m_Label6";
    m_Label6.Size = new Size(74, 13);
    m_Label6.TabIndex = 35;
    m_Label6.Text = "Delimited Text";
    // 
    // m_TableLayoutPanelColumns
    // 
    m_TableLayoutPanelColumns.AutoSize = true;
    m_TableLayoutPanelColumns.ColumnCount = 3;
    m_TableLayoutPanelColumns.ColumnStyles.Add(new ColumnStyle());
    m_TableLayoutPanelColumns.ColumnStyles.Add(new ColumnStyle());
    m_TableLayoutPanelColumns.ColumnStyles.Add(new ColumnStyle());
    m_TableLayoutPanelColumns.Controls.Add(m_Label3, 0, 3);
    m_TableLayoutPanelColumns.Controls.Add(m_FastColoredTextBox12, 2, 3);
    m_TableLayoutPanelColumns.Controls.Add(m_FastColoredTextBox02, 1, 3);
    m_TableLayoutPanelColumns.Controls.Add(m_FastColoredTextBox01, 1, 2);
    m_TableLayoutPanelColumns.Controls.Add(m_FastColoredTextBox11, 2, 2);
    m_TableLayoutPanelColumns.Controls.Add(m_FastColoredTextBox00, 1, 1);
    m_TableLayoutPanelColumns.Controls.Add(m_Label2, 0, 2);
    m_TableLayoutPanelColumns.Controls.Add(m_FastColoredTextBox10, 2, 1);
    m_TableLayoutPanelColumns.Controls.Add(m_Label1, 0, 1);
    m_TableLayoutPanelColumns.Controls.Add(m_Label5, 1, 0);
    m_TableLayoutPanelColumns.Controls.Add(m_Label4, 2, 0);
    m_TableLayoutPanelColumns.Dock = DockStyle.Fill;
    m_TableLayoutPanelColumns.Location = new Point(0, 0);
    m_TableLayoutPanelColumns.Name = "m_TableLayoutPanelColumns";
    m_TableLayoutPanelColumns.RowCount = 4;
    m_TableLayoutPanelColumns.RowStyles.Add(new RowStyle());
    m_TableLayoutPanelColumns.RowStyles.Add(new RowStyle());
    m_TableLayoutPanelColumns.RowStyles.Add(new RowStyle());
    m_TableLayoutPanelColumns.RowStyles.Add(new RowStyle());
    m_TableLayoutPanelColumns.Size = new Size(381, 114);
    m_TableLayoutPanelColumns.TabIndex = 0;
    // 
    // m_Label3
    // 
    m_Label3.Anchor =  AnchorStyles.Top | AnchorStyles.Right;
    m_Label3.AutoSize = true;
    m_Label3.ForeColor = Color.Teal;
    m_Label3.Location = new Point(3, 61);
    m_Label3.Name = "m_Label3";
    m_Label3.Padding = new Padding(0, 3, 0, 0);
    m_Label3.Size = new Size(36, 16);
    m_Label3.TabIndex = 3;
    m_Label3.Text = "Rec 3";
    // 
    // m_FastColoredTextBox12
    // 
    m_FastColoredTextBox12.AllowDrop = false;
    m_FastColoredTextBox12.AutoCompleteBracketsList = new char[]
{
    '(',
    ')',
    '{',
    '}',
    '[',
    ']',
    '"',
    '"',
    '\'',
    '\''
};
    m_FastColoredTextBox12.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
    m_FastColoredTextBox12.AutoScrollMinSize = new Size(106, 28);
    m_FastColoredTextBox12.BackBrush = null;
    m_FastColoredTextBox12.CharHeight = 14;
    m_FastColoredTextBox12.CharWidth = 8;
    m_FastColoredTextBox12.Cursor = Cursors.IBeam;
    m_FastColoredTextBox12.DisabledColor = Color.FromArgb(  100,   180,   180,   180);
    m_FastColoredTextBox12.IsReplaceMode = false;
    m_FastColoredTextBox12.Location = new Point(143, 64);
    m_FastColoredTextBox12.Name = "m_FastColoredTextBox12";
    m_FastColoredTextBox12.Paddings = new Padding(0);
    m_FastColoredTextBox12.ReadOnly = true;
    m_FastColoredTextBox12.SelectionColor = Color.FromArgb(  60,   0,   0,   255);
    m_FastColoredTextBox12.ShowLineNumbers = false;
    m_FastColoredTextBox12.Size = new Size(197, 33);
    m_FastColoredTextBox12.TabIndex = 5;
    m_FastColoredTextBox12.Tag = "NoFontChange";
    m_FastColoredTextBox12.Text = "Column with ¶\r\nLinefeed";
    m_FastColoredTextBox12.Zoom = 100;
    // 
    // m_FastColoredTextBox02
    // 
    m_FastColoredTextBox02.AllowDrop = false;
    m_FastColoredTextBox02.AutoCompleteBracketsList = new char[]
{
    '(',
    ')',
    '{',
    '}',
    '[',
    ']',
    '"',
    '"',
    '\'',
    '\''
};
    m_FastColoredTextBox02.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
    m_FastColoredTextBox02.AutoScrollMinSize = new Size(66, 14);
    m_FastColoredTextBox02.BackBrush = null;
    m_FastColoredTextBox02.CharHeight = 14;
    m_FastColoredTextBox02.CharWidth = 8;
    m_FastColoredTextBox02.Cursor = Cursors.IBeam;
    m_FastColoredTextBox02.DisabledColor = Color.FromArgb(  100,   180,   180,   180);
    m_FastColoredTextBox02.IsReplaceMode = false;
    m_FastColoredTextBox02.Location = new Point(45, 64);
    m_FastColoredTextBox02.Multiline = false;
    m_FastColoredTextBox02.Name = "m_FastColoredTextBox02";
    m_FastColoredTextBox02.Paddings = new Padding(0);
    m_FastColoredTextBox02.ReadOnly = true;
    m_FastColoredTextBox02.SelectionColor = Color.FromArgb(  60,   0,   0,   255);
    m_FastColoredTextBox02.ShowLineNumbers = false;
    m_FastColoredTextBox02.ShowScrollBars = false;
    m_FastColoredTextBox02.Size = new Size(92, 33);
    m_FastColoredTextBox02.TabIndex = 4;
    m_FastColoredTextBox02.Tag = "NoFontChange";
    m_FastColoredTextBox02.Text = "Example ";
    m_FastColoredTextBox02.Zoom = 100;
    // 
    // m_FastColoredTextBox01
    // 
    m_FastColoredTextBox01.AllowDrop = false;
    m_FastColoredTextBox01.AutoCompleteBracketsList = new char[]
{
    '(',
    ')',
    '{',
    '}',
    '[',
    ']',
    '"',
    '"',
    '\'',
    '\''
};
    m_FastColoredTextBox01.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
    m_FastColoredTextBox01.AutoScrollMinSize = new Size(98, 14);
    m_FastColoredTextBox01.BackBrush = null;
    m_FastColoredTextBox01.CharHeight = 14;
    m_FastColoredTextBox01.CharWidth = 8;
    m_FastColoredTextBox01.Cursor = Cursors.IBeam;
    m_FastColoredTextBox01.DisabledColor = Color.FromArgb(  100,   180,   180,   180);
    m_FastColoredTextBox01.IsReplaceMode = false;
    m_FastColoredTextBox01.Location = new Point(45, 40);
    m_FastColoredTextBox01.Multiline = false;
    m_FastColoredTextBox01.Name = "m_FastColoredTextBox01";
    m_FastColoredTextBox01.Paddings = new Padding(0);
    m_FastColoredTextBox01.ReadOnly = true;
    m_FastColoredTextBox01.SelectionColor = Color.FromArgb(  60,   0,   0,   255);
    m_FastColoredTextBox01.ShowLineNumbers = false;
    m_FastColoredTextBox01.ShowScrollBars = false;
    m_FastColoredTextBox01.Size = new Size(92, 18);
    m_FastColoredTextBox01.TabIndex = 1;
    m_FastColoredTextBox01.Tag = "NoFontChange";
    m_FastColoredTextBox01.Text = " a Trimming ";
    m_FastColoredTextBox01.Zoom = 100;
    // 
    // m_FastColoredTextBox11
    // 
    m_FastColoredTextBox11.AllowDrop = false;
    m_FastColoredTextBox11.AutoCompleteBracketsList = new char[]
{
    '(',
    ')',
    '{',
    '}',
    '[',
    ']',
    '"',
    '"',
    '\'',
    '\''
};
    m_FastColoredTextBox11.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
    m_FastColoredTextBox11.AutoScrollMinSize = new Size(154, 14);
    m_FastColoredTextBox11.BackBrush = null;
    m_FastColoredTextBox11.CharHeight = 14;
    m_FastColoredTextBox11.CharWidth = 8;
    m_FastColoredTextBox11.Cursor = Cursors.IBeam;
    m_FastColoredTextBox11.DisabledColor = Color.FromArgb(  100,   180,   180,   180);
    m_FastColoredTextBox11.IsReplaceMode = false;
    m_FastColoredTextBox11.Location = new Point(143, 40);
    m_FastColoredTextBox11.Multiline = false;
    m_FastColoredTextBox11.Name = "m_FastColoredTextBox11";
    m_FastColoredTextBox11.Paddings = new Padding(0);
    m_FastColoredTextBox11.ReadOnly = true;
    m_FastColoredTextBox11.SelectionColor = Color.FromArgb(  60,   0,   0,   255);
    m_FastColoredTextBox11.ShowLineNumbers = false;
    m_FastColoredTextBox11.ShowScrollBars = false;
    m_FastColoredTextBox11.Size = new Size(197, 18);
    m_FastColoredTextBox11.TabIndex = 2;
    m_FastColoredTextBox11.Tag = "NoFontChange";
    m_FastColoredTextBox11.Text = "Column with \" Quote";
    m_FastColoredTextBox11.Zoom = 100;
    // 
    // m_FastColoredTextBox00
    // 
    m_FastColoredTextBox00.AllowDrop = false;
    m_FastColoredTextBox00.AutoCompleteBracketsList = new char[]
{
    '(',
    ')',
    '{',
    '}',
    '[',
    ']',
    '"',
    '"',
    '\'',
    '\''
};
    m_FastColoredTextBox00.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
    m_FastColoredTextBox00.AutoScrollMinSize = new Size(66, 14);
    m_FastColoredTextBox00.BackBrush = null;
    m_FastColoredTextBox00.CharHeight = 14;
    m_FastColoredTextBox00.CharWidth = 8;
    m_FastColoredTextBox00.Cursor = Cursors.IBeam;
    m_FastColoredTextBox00.DisabledColor = Color.FromArgb(  100,   180,   180,   180);
    m_FastColoredTextBox00.IsReplaceMode = false;
    m_FastColoredTextBox00.Location = new Point(45, 16);
    m_FastColoredTextBox00.Multiline = false;
    m_FastColoredTextBox00.Name = "m_FastColoredTextBox00";
    m_FastColoredTextBox00.Paddings = new Padding(0);
    m_FastColoredTextBox00.ReadOnly = true;
    m_FastColoredTextBox00.SelectionColor = Color.FromArgb(  60,   0,   0,   255);
    m_FastColoredTextBox00.ShowLineNumbers = false;
    m_FastColoredTextBox00.ShowScrollBars = false;
    m_FastColoredTextBox00.Size = new Size(92, 18);
    m_FastColoredTextBox00.TabIndex = 9;
    m_FastColoredTextBox00.Tag = "NoFontChange";
    m_FastColoredTextBox00.Text = "This is ";
    m_FastColoredTextBox00.Zoom = 100;
    // 
    // m_Label2
    // 
    m_Label2.Anchor =  AnchorStyles.Top | AnchorStyles.Right;
    m_Label2.AutoSize = true;
    m_Label2.ForeColor = Color.Teal;
    m_Label2.Location = new Point(3, 37);
    m_Label2.Name = "m_Label2";
    m_Label2.Padding = new Padding(0, 3, 0, 0);
    m_Label2.Size = new Size(36, 16);
    m_Label2.TabIndex = 0;
    m_Label2.Text = "Rec 2";
    // 
    // m_FastColoredTextBox10
    // 
    m_FastColoredTextBox10.AllowDrop = false;
    m_FastColoredTextBox10.AutoCompleteBracketsList = new char[]
{
    '(',
    ')',
    '{',
    '}',
    '[',
    ']',
    '"',
    '"',
    '\'',
    '\''
};
    m_FastColoredTextBox10.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
    m_FastColoredTextBox10.AutoScrollMinSize = new Size(186, 14);
    m_FastColoredTextBox10.BackBrush = null;
    m_FastColoredTextBox10.CharHeight = 14;
    m_FastColoredTextBox10.CharWidth = 8;
    m_FastColoredTextBox10.Cursor = Cursors.IBeam;
    m_FastColoredTextBox10.DisabledColor = Color.FromArgb(  100,   180,   180,   180);
    m_FastColoredTextBox10.IsReplaceMode = false;
    m_FastColoredTextBox10.Location = new Point(143, 16);
    m_FastColoredTextBox10.Multiline = false;
    m_FastColoredTextBox10.Name = "m_FastColoredTextBox10";
    m_FastColoredTextBox10.Paddings = new Padding(0);
    m_FastColoredTextBox10.ReadOnly = true;
    m_FastColoredTextBox10.SelectionColor = Color.FromArgb(  60,   0,   0,   255);
    m_FastColoredTextBox10.ShowLineNumbers = false;
    m_FastColoredTextBox10.ShowScrollBars = false;
    m_FastColoredTextBox10.Size = new Size(197, 18);
    m_FastColoredTextBox10.TabIndex = 10;
    m_FastColoredTextBox10.TabLength = 1;
    m_FastColoredTextBox10.TabStop = false;
    m_FastColoredTextBox10.Tag = "NoFontChange";
    m_FastColoredTextBox10.Text = "Column with:, Delimiter";
    m_FastColoredTextBox10.Zoom = 100;
    // 
    // m_Label1
    // 
    m_Label1.Anchor =  AnchorStyles.Top | AnchorStyles.Right;
    m_Label1.AutoSize = true;
    m_Label1.ForeColor = Color.Teal;
    m_Label1.Location = new Point(3, 13);
    m_Label1.Name = "m_Label1";
    m_Label1.Padding = new Padding(0, 3, 0, 0);
    m_Label1.Size = new Size(36, 16);
    m_Label1.TabIndex = 8;
    m_Label1.Text = "Rec 1";
    // 
    // m_Label5
    // 
    m_Label5.Anchor = AnchorStyles.None;
    m_Label5.AutoSize = true;
    m_Label5.ForeColor = Color.Teal;
    m_Label5.Location = new Point(65, 0);
    m_Label5.Name = "m_Label5";
    m_Label5.Size = new Size(51, 13);
    m_Label5.TabIndex = 6;
    m_Label5.Text = "Column 1";
    // 
    // m_Label4
    // 
    m_Label4.Anchor = AnchorStyles.None;
    m_Label4.AutoSize = true;
    m_Label4.ForeColor = Color.Teal;
    m_Label4.Location = new Point(235, 0);
    m_Label4.Name = "m_Label4";
    m_Label4.Size = new Size(51, 13);
    m_Label4.TabIndex = 7;
    m_Label4.Text = "Column 2";
    // 
    // m_RadioButtonNeeded
    // 
    m_RadioButtonNeeded.Anchor = AnchorStyles.Left;
    m_RadioButtonNeeded.AutoSize = true;
    m_RadioButtonNeeded.DataBindings.Add(new Binding("Checked", m_CsvSettingBindingSource, "QualifyOnlyIfNeeded", true));
    m_RadioButtonNeeded.Location = new Point(372, 4);
    m_RadioButtonNeeded.Name = "m_RadioButtonNeeded";
    m_RadioButtonNeeded.Size = new Size(131, 17);
    m_RadioButtonNeeded.TabIndex = 3;
    m_RadioButtonNeeded.TabStop = true;
    m_RadioButtonNeeded.Text = "Qualify Only If Needed";
    m_ToolTip.SetToolTip(m_RadioButtonNeeded, "Writing Text the content is quoted only if it's necessary ");
    m_RadioButtonNeeded.UseVisualStyleBackColor = true;
    m_RadioButtonNeeded.Visible = false;
    // 
    // m_CsvSettingBindingSource
    // 
    m_CsvSettingBindingSource.AllowNew = false;
    m_CsvSettingBindingSource.DataSource = typeof(ICsvFile);
    // 
    // m_RadioButtonAlways
    // 
    m_RadioButtonAlways.Anchor = AnchorStyles.Left;
    m_RadioButtonAlways.AutoSize = true;
    m_RadioButtonAlways.DataBindings.Add(new Binding("Checked", m_CsvSettingBindingSource, "QualifyAlways", true));
    m_RadioButtonAlways.Location = new Point(372, 30);
    m_RadioButtonAlways.Name = "m_RadioButtonAlways";
    m_RadioButtonAlways.Size = new Size(93, 17);
    m_RadioButtonAlways.TabIndex = 7;
    m_RadioButtonAlways.TabStop = true;
    m_RadioButtonAlways.Text = "Qualify Always";
    m_ToolTip.SetToolTip(m_RadioButtonAlways, "Writing Text the content is quoted even its is not required");
    m_RadioButtonAlways.UseVisualStyleBackColor = true;
    m_RadioButtonAlways.Visible = false;
    // 
    // m_ComboBoxTrim
    // 
    m_ComboBoxTrim.Anchor = AnchorStyles.Left;
    m_ComboBoxTrim.DataBindings.Add(new Binding("SelectedValue", m_CsvSettingBindingSource, "TrimmingOption", true, DataSourceUpdateMode.OnPropertyChanged));
    m_ComboBoxTrim.DisplayMember = "Display";
    m_ComboBoxTrim.DropDownStyle = ComboBoxStyle.DropDownList;
    m_ComboBoxTrim.Location = new Point(95, 55);
    m_ComboBoxTrim.Name = "m_ComboBoxTrim";
    m_ComboBoxTrim.Size = new Size(96, 21);
    m_ComboBoxTrim.TabIndex = 8;
    m_ToolTip.SetToolTip(m_ComboBoxTrim, "None will preserve whitespace; Unquoted will remove white spaces if the column was not quoted; All will remove white spaces even if the column was quoted");
    m_ComboBoxTrim.ValueMember = "ID";
    // 
    // m_CheckBoxAlternateQuoting
    // 
    m_CheckBoxAlternateQuoting.Anchor = AnchorStyles.Left;
    m_CheckBoxAlternateQuoting.AutoSize = true;
    m_CheckBoxAlternateQuoting.DataBindings.Add(new Binding("Checked", m_CsvSettingBindingSource, "ContextSensitiveQualifier", true, DataSourceUpdateMode.OnPropertyChanged));
    m_CheckBoxAlternateQuoting.Location = new Point(197, 4);
    m_CheckBoxAlternateQuoting.Name = "m_CheckBoxAlternateQuoting";
    m_CheckBoxAlternateQuoting.Size = new Size(169, 17);
    m_CheckBoxAlternateQuoting.TabIndex = 2;
    m_CheckBoxAlternateQuoting.Text = "Context Sensitive Qualification";
    m_ToolTip.SetToolTip(m_CheckBoxAlternateQuoting, "This is a uncommon way of quoting but allows to parse incorrectly quoted files, a quote is only regarded as closing quote if it is followed by linefeed or delimiter");
    m_CheckBoxAlternateQuoting.UseVisualStyleBackColor = true;
    // 
    // m_CheckBoxDuplicateQuotingToEscape
    // 
    m_CheckBoxDuplicateQuotingToEscape.Anchor = AnchorStyles.Left;
    m_CheckBoxDuplicateQuotingToEscape.AutoSize = true;
    m_CheckBoxDuplicateQuotingToEscape.DataBindings.Add(new Binding("Checked", m_CsvSettingBindingSource, "DuplicateQualifierToEscape", true));
    m_CheckBoxDuplicateQuotingToEscape.Location = new Point(197, 30);
    m_CheckBoxDuplicateQuotingToEscape.Name = "m_CheckBoxDuplicateQuotingToEscape";
    m_CheckBoxDuplicateQuotingToEscape.Size = new Size(134, 17);
    m_CheckBoxDuplicateQuotingToEscape.TabIndex = 6;
    m_CheckBoxDuplicateQuotingToEscape.Text = "Repeated Qualification";
    m_ToolTip.SetToolTip(m_CheckBoxDuplicateQuotingToEscape, "Assume a repeated quote in a qualified text represent a quote that does not end text qualification, usually either repeated quoting or escaped charters are used. ");
    m_CheckBoxDuplicateQuotingToEscape.UseVisualStyleBackColor = true;
    // 
    // m_TextBoxQuote
    // 
    m_TextBoxQuote.Anchor = AnchorStyles.Left;
    m_TextBoxQuote.AutoCompleteCustomSource.AddRange(new string[] { "\"", "'" });
    m_TextBoxQuote.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
    m_TextBoxQuote.AutoCompleteSource = AutoCompleteSource.CustomSource;
    m_TextBoxQuote.DataBindings.Add(new Binding("Character", m_CsvSettingBindingSource, "FieldQualifierChar", true));
    m_TextBoxQuote.Location = new Point(95, 3);
    m_TextBoxQuote.Name = "m_TextBoxQuote";
    m_TextBoxQuote.Size = new Size(96, 20);
    m_TextBoxQuote.TabIndex = 1;
    m_ToolTip.SetToolTip(m_TextBoxQuote, "Columns may be qualified with a character; usually these are \" the quotes are removed by the reading applications. This is needed in case a line feed or a delimiter is part of the column");
    m_TextBoxQuote.Type = PunctuationTextBox.PunctuationType.Qualifier;
    m_TextBoxQuote.TextChanged += TextBoxQuote_TextChanged;
    // 
    // m_TextBoxQuotePlaceHolder
    // 
    m_TextBoxQuotePlaceHolder.Anchor = AnchorStyles.Left;
    m_TextBoxQuotePlaceHolder.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
    m_TextBoxQuotePlaceHolder.DataBindings.Add(new Binding("Text", m_CsvSettingBindingSource, "QualifierPlaceholder", true));
    m_TextBoxQuotePlaceHolder.Location = new Point(95, 29);
    m_TextBoxQuotePlaceHolder.Name = "m_TextBoxQuotePlaceHolder";
    m_TextBoxQuotePlaceHolder.Size = new Size(96, 20);
    m_TextBoxQuotePlaceHolder.TabIndex = 5;
    // 
    // m_ErrorProvider
    // 
    m_ErrorProvider.ContainerControl = this;
    // 
    // m_TableLayoutPanel
    // 
    m_TableLayoutPanel.AutoSize = true;
    m_TableLayoutPanel.ColumnCount = 4;
    m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
    m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
    m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
    m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
    m_TableLayoutPanel.Controls.Add(m_SplitContainer, 0, 3);
    m_TableLayoutPanel.Controls.Add(m_LabelQuote, 0, 0);
    m_TableLayoutPanel.Controls.Add(m_TextBoxQuote, 1, 0);
    m_TableLayoutPanel.Controls.Add(m_CheckBoxAlternateQuoting, 2, 0);
    m_TableLayoutPanel.Controls.Add(m_CheckBoxDuplicateQuotingToEscape, 2, 1);
    m_TableLayoutPanel.Controls.Add(m_LabelQuotePlaceholder, 0, 1);
    m_TableLayoutPanel.Controls.Add(m_TextBoxQuotePlaceHolder, 1, 1);
    m_TableLayoutPanel.Controls.Add(m_LabelTrim, 0, 2);
    m_TableLayoutPanel.Controls.Add(m_ComboBoxTrim, 1, 2);
    m_TableLayoutPanel.Controls.Add(m_RadioButtonNeeded, 3, 0);
    m_TableLayoutPanel.Controls.Add(m_RadioButtonAlways, 3, 1);
    m_TableLayoutPanel.Dock = DockStyle.Top;
    m_TableLayoutPanel.Location = new Point(0, 0);
    m_TableLayoutPanel.Name = "m_TableLayoutPanel";
    m_TableLayoutPanel.RowCount = 4;
    m_TableLayoutPanel.RowStyles.Add(new RowStyle());
    m_TableLayoutPanel.RowStyles.Add(new RowStyle());
    m_TableLayoutPanel.RowStyles.Add(new RowStyle());
    m_TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
    m_TableLayoutPanel.Size = new Size(726, 199);
    m_TableLayoutPanel.TabIndex = 0;
    // 
    // m_LabelQuote
    // 
    m_LabelQuote.Anchor = AnchorStyles.Right;
    m_LabelQuote.AutoSize = true;
    m_LabelQuote.Location = new Point(17, 6);
    m_LabelQuote.Name = "m_LabelQuote";
    m_LabelQuote.Size = new Size(72, 13);
    m_LabelQuote.TabIndex = 0;
    m_LabelQuote.Text = "Text Qualifier:";
    // 
    // m_LabelQuotePlaceholder
    // 
    m_LabelQuotePlaceholder.Anchor = AnchorStyles.Right;
    m_LabelQuotePlaceholder.AutoSize = true;
    m_LabelQuotePlaceholder.Location = new Point(23, 32);
    m_LabelQuotePlaceholder.Name = "m_LabelQuotePlaceholder";
    m_LabelQuotePlaceholder.Size = new Size(66, 13);
    m_LabelQuotePlaceholder.TabIndex = 4;
    m_LabelQuotePlaceholder.Text = "Placeholder:";
    // 
    // m_LabelTrim
    // 
    m_LabelTrim.Anchor = AnchorStyles.Right;
    m_LabelTrim.AutoSize = true;
    m_LabelTrim.Location = new Point(3, 59);
    m_LabelTrim.Name = "m_LabelTrim";
    m_LabelTrim.Size = new Size(86, 13);
    m_LabelTrim.TabIndex = 7;
    m_LabelTrim.Text = "Trimming Option:";
    // 
    // m_TimerRebuilt
    // 
    m_TimerRebuilt.Interval = 200;
    m_TimerRebuilt.Tick += TimerRebuilt_Tick;
    // 
    // QuotingControl
    // 
    Controls.Add(m_TableLayoutPanel);
    MinimumSize = new Size(31, 0);
    Name = "QuotingControl";
    Size = new Size(726, 201);
    m_SplitContainer.Panel1.ResumeLayout(false);
    m_SplitContainer.Panel1.PerformLayout();
    m_SplitContainer.Panel2.ResumeLayout(false);
    m_SplitContainer.Panel2.PerformLayout();
    ((ISupportInitialize) m_SplitContainer).EndInit();
    m_SplitContainer.ResumeLayout(false);
    m_TableLayoutPanelText.ResumeLayout(false);
    m_TableLayoutPanelText.PerformLayout();
    ((ISupportInitialize) m_FastColoredTextBox).EndInit();
    m_TableLayoutPanelColumns.ResumeLayout(false);
    m_TableLayoutPanelColumns.PerformLayout();
    ((ISupportInitialize) m_FastColoredTextBox12).EndInit();
    ((ISupportInitialize) m_FastColoredTextBox02).EndInit();
    ((ISupportInitialize) m_FastColoredTextBox01).EndInit();
    ((ISupportInitialize) m_FastColoredTextBox11).EndInit();
    ((ISupportInitialize) m_FastColoredTextBox00).EndInit();
    ((ISupportInitialize) m_FastColoredTextBox10).EndInit();
    ((ISupportInitialize) m_CsvSettingBindingSource).EndInit();
    ((ISupportInitialize) m_ErrorProvider).EndInit();
    m_TableLayoutPanel.ResumeLayout(false);
    m_TableLayoutPanel.PerformLayout();
    ResumeLayout(false);
    PerformLayout();

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