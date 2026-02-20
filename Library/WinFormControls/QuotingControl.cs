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

namespace CsvTools;

/// <summary>
///   A Control to edit the quoting and visualize the result
/// </summary>
public class QuotingControl : UserControl, INotifyPropertyChanged
{
  private readonly Style m_Delimiter = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
  private readonly Style m_DelimiterTab = new SyntaxHighlighterDelimitedText.SyntaxHighlightStyleTab(Pens.Blue, Brushes.AntiqueWhite);
  private readonly Style m_EscapedQuoteStyle = new TextStyle(Brushes.Black, Brushes.LightSteelBlue, FontStyle.Regular);
  private readonly Style m_PilcrowStyle = new TextStyle(Brushes.Orange, null, FontStyle.Bold);
  private readonly Style m_QuoteStyle = new TextStyle(Brushes.Magenta, null, FontStyle.Regular);
  private readonly Regex m_SpaceRegex = new Regex(" ", RegexOptions.Singleline | RegexOptions.Compiled);
  private readonly Style m_SpaceStyle = new SyntaxHighlighterDelimitedText.SyntaxHighlightStyleStyleSpace(Brushes.Blue, Brushes.AntiqueWhite);
  private PunctuationTextBox charBoxFieldQualifierChar;
  private CheckBox checkBoxContextSensitiveQualifier;
  private CheckBox checkBoxDuplicateQualifierToEscape;
  private ComboBox comboBoxTrimmingOption;
  private IContainer components;
  private ErrorProvider? errorProvider;
  private FastColoredTextBox fastColoredTextBoxLeft;
  private Label labelHeaderCol1;
  private Label labelHeaderCol2;
  private Label labelHeaderLeft;
  private Label labelQuote;
  private Label labelQuotePlaceholder;
  private Label labelRec1;
  private Label labelRec2;
  private Label labelRec3;
  private Label labelTrim;
  private bool m_ContextSensitiveQualifier = false;
  private bool m_DuplicateQualifierToEscape;
  private char m_EscapePrefixChar = '\0';
  private FastColoredTextBox fastColoredTextBox00;
  private FastColoredTextBox fastColoredTextBox01;
  private FastColoredTextBox fastColoredTextBox02;
  private FastColoredTextBox fastColoredTextBox10;
  private FastColoredTextBox fastColoredTextBox11;
  private FastColoredTextBox fastColoredTextBox12;
  private char m_FieldDelimiterChar = ',';
  private char m_FieldQualifierChar = '"';
  private bool m_HasChanges = true;
  private bool m_IsWriteSetting;
  private string m_QualifierPlaceholder = "";
  private bool m_QualifyAlways = false;
  private bool m_QualifyOnlyIfNeeded = true;
  private TrimmingOptionEnum m_TrimmingOption = TrimmingOptionEnum.Unquoted;
  private RadioButton radioButtonQualifyAlways;
  private RadioButton radioButtonQualifyOnlyIfNeeded;
  private SplitContainer splitContainer;
  private TableLayoutPanel tableLayoutPanel;
  private TableLayoutPanel tableLayoutPanelColumns;
  private TableLayoutPanel tableLayoutPanelText;
  private TextBox textBoxQualifierPlaceholder;
  private Timer timerRebuilt;
  private ToolTip toolTip;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
  public QuotingControl()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
  {
    InitializeComponent();
    comboBoxTrimmingOption!.SetEnumDataSource(TrimmingOptionEnum.Unquoted);

#pragma warning disable CS8602 // Dereference of a possibly null reference.
    radioButtonQualifyOnlyIfNeeded.DataBindings.Add(new Binding("Checked", this, nameof(QualifyOnlyIfNeeded), false, DataSourceUpdateMode.OnPropertyChanged));
    radioButtonQualifyAlways.DataBindings.Add(new Binding("Checked", this, nameof(QualifyAlways), false, DataSourceUpdateMode.OnPropertyChanged));
    comboBoxTrimmingOption.DataBindings.Add(new Binding("SelectedValue", this, nameof(TrimmingOption), false, DataSourceUpdateMode.OnPropertyChanged));
    checkBoxContextSensitiveQualifier.DataBindings.Add(new Binding("Checked", this, nameof(ContextSensitiveQualifier), false, DataSourceUpdateMode.OnPropertyChanged));
    checkBoxDuplicateQualifierToEscape.DataBindings.Add(new Binding("Checked", this, nameof(DuplicateQualifierToEscape), false, DataSourceUpdateMode.OnPropertyChanged));
    charBoxFieldQualifierChar.DataBindings.Add(new Binding("Character", this, nameof(FieldQualifierChar), false, DataSourceUpdateMode.OnPropertyChanged));
    textBoxQualifierPlaceholder.DataBindings.Add(new Binding("Text", this, nameof(QualifierPlaceholder), false, DataSourceUpdateMode.OnPropertyChanged));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
  }

  public event PropertyChangedEventHandler? PropertyChanged;


  /// <summary>
  /// Helper function to add a bound QuotingControl, the form designer seems to be unable to handle this control it crashes or destroys teh control
  /// </summary>
  /// <param name="tableLayout"></param>
  /// <param name="column"></param>
  /// <param name="row"></param>
  /// <param name="span"></param>
  /// <param name="bindingSource"></param>
  /// <returns></returns>
  public static QuotingControl AddQuotingControl(TableLayoutPanel? tableLayout, int column, int row, int span, BindingSource? bindingSource)
  {
    var ctrl = new QuotingControl();
    if (bindingSource!=null)
    {
      ctrl.DataBindings.Add(new Binding(nameof(FieldDelimiterChar), bindingSource, nameof(FieldDelimiterChar), false, DataSourceUpdateMode.OnPropertyChanged));
      ctrl.DataBindings.Add(new Binding(nameof(EscapePrefixChar), bindingSource, nameof(EscapePrefixChar), false, DataSourceUpdateMode.OnPropertyChanged));
      ctrl.DataBindings.Add(new Binding(nameof(QualifyOnlyIfNeeded), bindingSource, nameof(QualifyOnlyIfNeeded), false, DataSourceUpdateMode.OnPropertyChanged));
      ctrl.DataBindings.Add(new Binding(nameof(QualifyAlways), bindingSource, nameof(QualifyAlways), false, DataSourceUpdateMode.OnPropertyChanged));
      ctrl.DataBindings.Add(new Binding(nameof(ContextSensitiveQualifier), bindingSource, nameof(ContextSensitiveQualifier), false, DataSourceUpdateMode.OnPropertyChanged));
      ctrl.DataBindings.Add(new Binding(nameof(DuplicateQualifierToEscape), bindingSource, nameof(DuplicateQualifierToEscape), false, DataSourceUpdateMode.OnPropertyChanged));
      ctrl.DataBindings.Add(new Binding(nameof(FieldQualifierChar), bindingSource, nameof(FieldQualifierChar), false, DataSourceUpdateMode.OnPropertyChanged));
      ctrl.DataBindings.Add(new Binding(nameof(QualifierPlaceholder), bindingSource, nameof(QualifierPlaceholder), false, DataSourceUpdateMode.OnPropertyChanged));
      ctrl.DataBindings.Add(new Binding(nameof(TrimmingOption), bindingSource, nameof(TrimmingOption), true));
    }
    ctrl.Dock = DockStyle.Fill;
    if (tableLayout!=null)
    {
      tableLayout.SuspendLayout();
      tableLayout.Controls.Add(ctrl, column, row);
      if (span>1)
        tableLayout.SetColumnSpan(ctrl, span);

      tableLayout.ResumeLayout(false);
      tableLayout.PerformLayout();
    }
    return ctrl;
  }


  [Bindable(true)]
  [Browsable(true)]
  public bool ContextSensitiveQualifier
  {
    get => m_ContextSensitiveQualifier;
    set
    {
      if (m_ContextSensitiveQualifier == value)
        return;

      m_ContextSensitiveQualifier = value;
      OnPropertyChanged(nameof(ContextSensitiveQualifier));
    }
  }

  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  [Bindable(true)]
  [Browsable(true)]
  public bool DuplicateQualifierToEscape
  {
    get => m_DuplicateQualifierToEscape;
    set
    {
      if (m_DuplicateQualifierToEscape == value)
        return;

      m_DuplicateQualifierToEscape = value;
      OnPropertyChanged(nameof(DuplicateQualifierToEscape));
    }
  }

  [DefaultValue('\0')]
  [Category("Appearance")]
  [Bindable(true)]
  [Browsable(true)]
  public char EscapePrefixChar
  {
    get => m_EscapePrefixChar;
    set
    {
      if (m_EscapePrefixChar == value)
        return;

      m_EscapePrefixChar = value;
      OnPropertyChanged(nameof(EscapePrefixChar));
    }
  }

  [DefaultValue(',')]
  [Category("Appearance")]
  [Bindable(true)]
  [Browsable(true)]
  public char FieldDelimiterChar
  {
    get => m_FieldDelimiterChar;
    set
    {
      if (m_FieldDelimiterChar == value)
        return;

      m_FieldDelimiterChar = value;
      OnPropertyChanged(nameof(FieldDelimiterChar));
    }
  }

  [DefaultValue('"')]
  [Category("Appearance")]
  [Bindable(true)]
  [Browsable(true)]
  public char FieldQualifierChar
  {
    get => m_FieldQualifierChar;
    set
    {
      if (m_FieldQualifierChar == value)
        return;

      m_FieldQualifierChar = value;
      OnPropertyChanged(nameof(FieldQualifierChar));
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
      comboBoxTrimmingOption.Visible = !value;
      checkBoxContextSensitiveQualifier.Visible = !value;
      labelTrim.Visible = !value;

      radioButtonQualifyAlways.Visible = value;
      radioButtonQualifyOnlyIfNeeded.Visible = value;

      if (value)
      {
        splitContainer.Panel2.Controls.Remove(tableLayoutPanelColumns);
        splitContainer.Panel1.Controls.Remove(tableLayoutPanelText);
        splitContainer.Panel2.Controls.Add(tableLayoutPanelText);
        splitContainer.Panel1.Controls.Add(tableLayoutPanelColumns);
        splitContainer.SplitterDistance = tableLayoutPanelColumns.Width;
      }
      else
      {
        splitContainer.Panel1.Controls.Remove(tableLayoutPanelColumns);
        splitContainer.Panel2.Controls.Remove(tableLayoutPanelText);
        splitContainer.Panel1.Controls.Add(tableLayoutPanelText);
        splitContainer.Panel2.Controls.Add(tableLayoutPanelColumns);
        splitContainer.SplitterDistance = tableLayoutPanelText.Width;
      }

      m_HasChanges = true;
    }
  }

  [DefaultValue("")]
  [Category("Appearance")]
  [Bindable(true)]
  [Browsable(true)]
  public string QualifierPlaceholder
  {
    get => m_QualifierPlaceholder;
    set
    {
      var newValue = value?.Trim() ?? string.Empty;
      if (string.Equals(m_QualifierPlaceholder, newValue, StringComparison.Ordinal))
        return;

      m_QualifierPlaceholder = newValue;
      OnPropertyChanged(nameof(QualifierPlaceholder));
    }
  }

  [DefaultValue(false)]
  [Category("Appearance")]
  [Bindable(true)]
  [Browsable(true)]
  public bool QualifyAlways
  {
    get => m_QualifyAlways;
    set
    {
      if (m_QualifyAlways == value)
        return;

      m_QualifyAlways = value;
      OnPropertyChanged(nameof(QualifyAlways));
    }
  }

  [DefaultValue(true)]
  [Category("Appearance")]
  [Bindable(true)]
  [Browsable(true)]
  public bool QualifyOnlyIfNeeded
  {
    get => m_QualifyOnlyIfNeeded;
    set
    {
      if (m_QualifyOnlyIfNeeded == value)
        return;

      m_QualifyOnlyIfNeeded = value;
      OnPropertyChanged(nameof(QualifyOnlyIfNeeded));
    }
  }

  [DefaultValue(TrimmingOptionEnum.Unquoted)]
  [Category("Appearance")]
  [Bindable(true)]
  [Browsable(true)]
  public TrimmingOptionEnum TrimmingOption
  {
    get => m_TrimmingOption;
    set
    {
      if (m_TrimmingOption == value)
        return;

      m_TrimmingOption = value;
      OnPropertyChanged(nameof(TrimmingOption));
    }
  }

  protected void OnPropertyChanged(string propertyName)
  {
    try
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    catch
    {
      // ignore
    }
    finally
    {
      m_HasChanges = true;
    }
  }

  /// <summary>
  ///   Initializes the component.
  /// </summary>
  [SuppressMessage("ReSharper", "RedundantNameQualifier")]
  [SuppressMessage("ReSharper", "RedundantCast")]
  [SuppressMessage("ReSharper", "RedundantDelegateCreation")]
  [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
  [SuppressMessage("ReSharper", "RedundantExplicitArrayCreation")]
  private void InitializeComponent()
  {
    components = new Container();
    splitContainer = new SplitContainer();
    tableLayoutPanelText = new TableLayoutPanel();
    fastColoredTextBoxLeft = new FastColoredTextBox();
    labelHeaderLeft = new Label();
    tableLayoutPanelColumns = new TableLayoutPanel();
    labelRec3 = new Label();
    fastColoredTextBox12 = new FastColoredTextBox();
    fastColoredTextBox02 = new FastColoredTextBox();
    fastColoredTextBox01 = new FastColoredTextBox();
    fastColoredTextBox11 = new FastColoredTextBox();
    fastColoredTextBox00 = new FastColoredTextBox();
    labelRec2 = new Label();
    fastColoredTextBox10 = new FastColoredTextBox();
    labelRec1 = new Label();
    labelHeaderCol1 = new Label();
    labelHeaderCol2 = new Label();
    toolTip = new ToolTip(components);
    radioButtonQualifyOnlyIfNeeded = new RadioButton();
    radioButtonQualifyAlways = new RadioButton();
    comboBoxTrimmingOption = new ComboBox();
    checkBoxContextSensitiveQualifier = new CheckBox();
    checkBoxDuplicateQualifierToEscape = new CheckBox();
    charBoxFieldQualifierChar = new PunctuationTextBox();
    textBoxQualifierPlaceholder = new TextBox();
    errorProvider = new ErrorProvider(components);
    tableLayoutPanel = new TableLayoutPanel();
    labelQuote = new Label();
    labelQuotePlaceholder = new Label();
    labelTrim = new Label();
    timerRebuilt = new Timer(components);
    ((ISupportInitialize) splitContainer).BeginInit();
    splitContainer.Panel1.SuspendLayout();
    splitContainer.Panel2.SuspendLayout();
    splitContainer.SuspendLayout();
    tableLayoutPanelText.SuspendLayout();
    ((ISupportInitialize) fastColoredTextBoxLeft).BeginInit();
    tableLayoutPanelColumns.SuspendLayout();
    ((ISupportInitialize) fastColoredTextBox12).BeginInit();
    ((ISupportInitialize) fastColoredTextBox02).BeginInit();
    ((ISupportInitialize) fastColoredTextBox01).BeginInit();
    ((ISupportInitialize) fastColoredTextBox11).BeginInit();
    ((ISupportInitialize) fastColoredTextBox00).BeginInit();
    ((ISupportInitialize) fastColoredTextBox10).BeginInit();
    ((ISupportInitialize) errorProvider).BeginInit();
    tableLayoutPanel.SuspendLayout();
    SuspendLayout();
    // 
    // splitContainer
    // 
    tableLayoutPanel.SetColumnSpan(splitContainer, 4);
    splitContainer.Dock = DockStyle.Top;
    splitContainer.Location = new Point(3, 82);
    splitContainer.Name = "splitContainer";
    // 
    // splitContainer.Panel1
    // 
    splitContainer.Panel1.Controls.Add(tableLayoutPanelText);
    // 
    // splitContainer.Panel2
    // 
    splitContainer.Panel2.Controls.Add(tableLayoutPanelColumns);
    splitContainer.Size = new Size(720, 114);
    splitContainer.SplitterDistance = 335;
    splitContainer.TabIndex = 5;
    // 
    // tableLayoutPanelText
    // 
    tableLayoutPanelText.AutoSize = true;
    tableLayoutPanelText.ColumnCount = 1;
    tableLayoutPanelText.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44.49112F));
    tableLayoutPanelText.Controls.Add(fastColoredTextBoxLeft, 0, 1);
    tableLayoutPanelText.Controls.Add(labelHeaderLeft, 0, 0);
    tableLayoutPanelText.Dock = DockStyle.Fill;
    tableLayoutPanelText.Location = new Point(0, 0);
    tableLayoutPanelText.Name = "tableLayoutPanelText";
    tableLayoutPanelText.RowCount = 4;
    tableLayoutPanelText.RowStyles.Add(new RowStyle());
    tableLayoutPanelText.RowStyles.Add(new RowStyle());
    tableLayoutPanelText.RowStyles.Add(new RowStyle());
    tableLayoutPanelText.RowStyles.Add(new RowStyle());
    tableLayoutPanelText.Size = new Size(335, 114);
    tableLayoutPanelText.TabIndex = 0;
    // 
    // fastColoredTextBoxLeft
    // 
    fastColoredTextBoxLeft.AllowDrop = false;
    fastColoredTextBoxLeft.AutoCompleteBracketsList = new char[]
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
    fastColoredTextBoxLeft.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
    fastColoredTextBoxLeft.AutoScrollMinSize = new Size(307, 56);
    fastColoredTextBoxLeft.BackBrush = null;
    fastColoredTextBoxLeft.CharHeight = 14;
    fastColoredTextBoxLeft.CharWidth = 8;
    fastColoredTextBoxLeft.Cursor = Cursors.IBeam;
    fastColoredTextBoxLeft.DisabledColor = Color.FromArgb(100, 180, 180, 180);
    fastColoredTextBoxLeft.Dock = DockStyle.Fill;
    fastColoredTextBoxLeft.IsReplaceMode = false;
    fastColoredTextBoxLeft.Location = new Point(3, 16);
    fastColoredTextBoxLeft.Name = "fastColoredTextBoxLeft";
    fastColoredTextBoxLeft.Paddings = new Padding(0);
    fastColoredTextBoxLeft.ReadOnly = true;
    tableLayoutPanelText.SetRowSpan(fastColoredTextBoxLeft, 3);
    fastColoredTextBoxLeft.SelectionColor = Color.FromArgb(60, 0, 0, 255);
    fastColoredTextBoxLeft.Size = new Size(329, 95);
    fastColoredTextBoxLeft.TabIndex = 0;
    fastColoredTextBoxLeft.TabLength = 1;
    fastColoredTextBoxLeft.TabStop = false;
    fastColoredTextBoxLeft.Tag = "NoFontChange";
    fastColoredTextBoxLeft.Text = "\"This is \";Column with:, Delimiter¶\r\n a Trimming ;Column with \"\" Quote¶\r\nExample ;\"Column with ¶\r\nLinefeed\"";
    fastColoredTextBoxLeft.Zoom = 100;
    // 
    // labelHeaderLeft
    // 
    labelHeaderLeft.Anchor = AnchorStyles.None;
    labelHeaderLeft.AutoSize = true;
    labelHeaderLeft.ForeColor = Color.Teal;
    labelHeaderLeft.Location = new Point(130, 0);
    labelHeaderLeft.Name = "labelHeaderLeft";
    labelHeaderLeft.Size = new Size(74, 13);
    labelHeaderLeft.TabIndex = 35;
    labelHeaderLeft.Text = "Delimited Text";
    // 
    // tableLayoutPanelColumns
    // 
    tableLayoutPanelColumns.AutoSize = true;
    tableLayoutPanelColumns.ColumnCount = 3;
    tableLayoutPanelColumns.ColumnStyles.Add(new ColumnStyle());
    tableLayoutPanelColumns.ColumnStyles.Add(new ColumnStyle());
    tableLayoutPanelColumns.ColumnStyles.Add(new ColumnStyle());
    tableLayoutPanelColumns.Controls.Add(labelRec3, 0, 3);
    tableLayoutPanelColumns.Controls.Add(fastColoredTextBox12, 2, 3);
    tableLayoutPanelColumns.Controls.Add(fastColoredTextBox02, 1, 3);
    tableLayoutPanelColumns.Controls.Add(fastColoredTextBox01, 1, 2);
    tableLayoutPanelColumns.Controls.Add(fastColoredTextBox11, 2, 2);
    tableLayoutPanelColumns.Controls.Add(fastColoredTextBox00, 1, 1);
    tableLayoutPanelColumns.Controls.Add(labelRec2, 0, 2);
    tableLayoutPanelColumns.Controls.Add(fastColoredTextBox10, 2, 1);
    tableLayoutPanelColumns.Controls.Add(labelRec1, 0, 1);
    tableLayoutPanelColumns.Controls.Add(labelHeaderCol1, 1, 0);
    tableLayoutPanelColumns.Controls.Add(labelHeaderCol2, 2, 0);
    tableLayoutPanelColumns.Dock = DockStyle.Fill;
    tableLayoutPanelColumns.Location = new Point(0, 0);
    tableLayoutPanelColumns.Name = "tableLayoutPanelColumns";
    tableLayoutPanelColumns.RowCount = 4;
    tableLayoutPanelColumns.RowStyles.Add(new RowStyle());
    tableLayoutPanelColumns.RowStyles.Add(new RowStyle());
    tableLayoutPanelColumns.RowStyles.Add(new RowStyle());
    tableLayoutPanelColumns.RowStyles.Add(new RowStyle());
    tableLayoutPanelColumns.Size = new Size(381, 114);
    tableLayoutPanelColumns.TabIndex = 0;
    // 
    // labelRec3
    // 
    labelRec3.Anchor =  AnchorStyles.Top | AnchorStyles.Right;
    labelRec3.AutoSize = true;
    labelRec3.ForeColor = Color.Teal;
    labelRec3.Location = new Point(3, 61);
    labelRec3.Name = "labelRec3";
    labelRec3.Padding = new Padding(0, 3, 0, 0);
    labelRec3.Size = new Size(36, 16);
    labelRec3.TabIndex = 3;
    labelRec3.Text = "Rec 3";
    // 
    // fastColoredTextBox12
    // 
    fastColoredTextBox12.AllowDrop = false;
    fastColoredTextBox12.AutoCompleteBracketsList = new char[]
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
    fastColoredTextBox12.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
    fastColoredTextBox12.AutoScrollMinSize = new Size(106, 28);
    fastColoredTextBox12.BackBrush = null;
    fastColoredTextBox12.CharHeight = 14;
    fastColoredTextBox12.CharWidth = 8;
    fastColoredTextBox12.Cursor = Cursors.IBeam;
    fastColoredTextBox12.DisabledColor = Color.FromArgb(100, 180, 180, 180);
    fastColoredTextBox12.IsReplaceMode = false;
    fastColoredTextBox12.Location = new Point(143, 64);
    fastColoredTextBox12.Name = "fastColoredTextBox12";
    fastColoredTextBox12.Paddings = new Padding(0);
    fastColoredTextBox12.ReadOnly = true;
    fastColoredTextBox12.SelectionColor = Color.FromArgb(60, 0, 0, 255);
    fastColoredTextBox12.ShowLineNumbers = false;
    fastColoredTextBox12.Size = new Size(197, 33);
    fastColoredTextBox12.TabIndex = 5;
    fastColoredTextBox12.Tag = "NoFontChange";
    fastColoredTextBox12.Text = "Column with ¶\r\nLinefeed";
    fastColoredTextBox12.Zoom = 100;
    // 
    // fastColoredTextBox02
    // 
    fastColoredTextBox02.AllowDrop = false;
    fastColoredTextBox02.AutoCompleteBracketsList = new char[]
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
    fastColoredTextBox02.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
    fastColoredTextBox02.AutoScrollMinSize = new Size(66, 14);
    fastColoredTextBox02.BackBrush = null;
    fastColoredTextBox02.CharHeight = 14;
    fastColoredTextBox02.CharWidth = 8;
    fastColoredTextBox02.Cursor = Cursors.IBeam;
    fastColoredTextBox02.DisabledColor = Color.FromArgb(100, 180, 180, 180);
    fastColoredTextBox02.IsReplaceMode = false;
    fastColoredTextBox02.Location = new Point(45, 64);
    fastColoredTextBox02.Multiline = false;
    fastColoredTextBox02.Name = "fastColoredTextBox02";
    fastColoredTextBox02.Paddings = new Padding(0);
    fastColoredTextBox02.ReadOnly = true;
    fastColoredTextBox02.SelectionColor = Color.FromArgb(60, 0, 0, 255);
    fastColoredTextBox02.ShowLineNumbers = false;
    fastColoredTextBox02.ShowScrollBars = false;
    fastColoredTextBox02.Size = new Size(92, 33);
    fastColoredTextBox02.TabIndex = 4;
    fastColoredTextBox02.Tag = "NoFontChange";
    fastColoredTextBox02.Text = "Example ";
    fastColoredTextBox02.Zoom = 100;
    // 
    // fastColoredTextBox01
    // 
    fastColoredTextBox01.AllowDrop = false;
    fastColoredTextBox01.AutoCompleteBracketsList = new char[]
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
    fastColoredTextBox01.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
    fastColoredTextBox01.AutoScrollMinSize = new Size(98, 14);
    fastColoredTextBox01.BackBrush = null;
    fastColoredTextBox01.CharHeight = 14;
    fastColoredTextBox01.CharWidth = 8;
    fastColoredTextBox01.Cursor = Cursors.IBeam;
    fastColoredTextBox01.DisabledColor = Color.FromArgb(100, 180, 180, 180);
    fastColoredTextBox01.IsReplaceMode = false;
    fastColoredTextBox01.Location = new Point(45, 40);
    fastColoredTextBox01.Multiline = false;
    fastColoredTextBox01.Name = "fastColoredTextBox01";
    fastColoredTextBox01.Paddings = new Padding(0);
    fastColoredTextBox01.ReadOnly = true;
    fastColoredTextBox01.SelectionColor = Color.FromArgb(60, 0, 0, 255);
    fastColoredTextBox01.ShowLineNumbers = false;
    fastColoredTextBox01.ShowScrollBars = false;
    fastColoredTextBox01.Size = new Size(92, 18);
    fastColoredTextBox01.TabIndex = 1;
    fastColoredTextBox01.Tag = "NoFontChange";
    fastColoredTextBox01.Text = " a Trimming ";
    fastColoredTextBox01.Zoom = 100;
    // 
    // fastColoredTextBox11
    // 
    fastColoredTextBox11.AllowDrop = false;
    fastColoredTextBox11.AutoCompleteBracketsList = new char[]
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
    fastColoredTextBox11.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
    fastColoredTextBox11.AutoScrollMinSize = new Size(154, 14);
    fastColoredTextBox11.BackBrush = null;
    fastColoredTextBox11.CharHeight = 14;
    fastColoredTextBox11.CharWidth = 8;
    fastColoredTextBox11.Cursor = Cursors.IBeam;
    fastColoredTextBox11.DisabledColor = Color.FromArgb(100, 180, 180, 180);
    fastColoredTextBox11.IsReplaceMode = false;
    fastColoredTextBox11.Location = new Point(143, 40);
    fastColoredTextBox11.Multiline = false;
    fastColoredTextBox11.Name = "fastColoredTextBox11";
    fastColoredTextBox11.Paddings = new Padding(0);
    fastColoredTextBox11.ReadOnly = true;
    fastColoredTextBox11.SelectionColor = Color.FromArgb(60, 0, 0, 255);
    fastColoredTextBox11.ShowLineNumbers = false;
    fastColoredTextBox11.ShowScrollBars = false;
    fastColoredTextBox11.Size = new Size(197, 18);
    fastColoredTextBox11.TabIndex = 2;
    fastColoredTextBox11.Tag = "NoFontChange";
    fastColoredTextBox11.Text = "Column with \" Quote";
    fastColoredTextBox11.Zoom = 100;
    // 
    // fastColoredTextBox00
    // 
    fastColoredTextBox00.AllowDrop = false;
    fastColoredTextBox00.AutoCompleteBracketsList = new char[]
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
    fastColoredTextBox00.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
    fastColoredTextBox00.AutoScrollMinSize = new Size(66, 14);
    fastColoredTextBox00.BackBrush = null;
    fastColoredTextBox00.CharHeight = 14;
    fastColoredTextBox00.CharWidth = 8;
    fastColoredTextBox00.Cursor = Cursors.IBeam;
    fastColoredTextBox00.DisabledColor = Color.FromArgb(100, 180, 180, 180);
    fastColoredTextBox00.IsReplaceMode = false;
    fastColoredTextBox00.Location = new Point(45, 16);
    fastColoredTextBox00.Multiline = false;
    fastColoredTextBox00.Name = "fastColoredTextBox00";
    fastColoredTextBox00.Paddings = new Padding(0);
    fastColoredTextBox00.ReadOnly = true;
    fastColoredTextBox00.SelectionColor = Color.FromArgb(60, 0, 0, 255);
    fastColoredTextBox00.ShowLineNumbers = false;
    fastColoredTextBox00.ShowScrollBars = false;
    fastColoredTextBox00.Size = new Size(92, 18);
    fastColoredTextBox00.TabIndex = 9;
    fastColoredTextBox00.Tag = "NoFontChange";
    fastColoredTextBox00.Text = "This is ";
    fastColoredTextBox00.Zoom = 100;
    // 
    // labelRec2
    // 
    labelRec2.Anchor =  AnchorStyles.Top | AnchorStyles.Right;
    labelRec2.AutoSize = true;
    labelRec2.ForeColor = Color.Teal;
    labelRec2.Location = new Point(3, 37);
    labelRec2.Name = "labelRec2";
    labelRec2.Padding = new Padding(0, 3, 0, 0);
    labelRec2.Size = new Size(36, 16);
    labelRec2.TabIndex = 0;
    labelRec2.Text = "Rec 2";
    // 
    // fastColoredTextBox10
    // 
    fastColoredTextBox10.AllowDrop = false;
    fastColoredTextBox10.AutoCompleteBracketsList = new char[]
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
    fastColoredTextBox10.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
    fastColoredTextBox10.AutoScrollMinSize = new Size(186, 14);
    fastColoredTextBox10.BackBrush = null;
    fastColoredTextBox10.CharHeight = 14;
    fastColoredTextBox10.CharWidth = 8;
    fastColoredTextBox10.Cursor = Cursors.IBeam;
    fastColoredTextBox10.DisabledColor = Color.FromArgb(100, 180, 180, 180);
    fastColoredTextBox10.IsReplaceMode = false;
    fastColoredTextBox10.Location = new Point(143, 16);
    fastColoredTextBox10.Multiline = false;
    fastColoredTextBox10.Name = "fastColoredTextBox10";
    fastColoredTextBox10.Paddings = new Padding(0);
    fastColoredTextBox10.ReadOnly = true;
    fastColoredTextBox10.SelectionColor = Color.FromArgb(60, 0, 0, 255);
    fastColoredTextBox10.ShowLineNumbers = false;
    fastColoredTextBox10.ShowScrollBars = false;
    fastColoredTextBox10.Size = new Size(197, 18);
    fastColoredTextBox10.TabIndex = 10;
    fastColoredTextBox10.TabLength = 1;
    fastColoredTextBox10.TabStop = false;
    fastColoredTextBox10.Tag = "NoFontChange";
    fastColoredTextBox10.Text = "Column with:, Delimiter";
    fastColoredTextBox10.Zoom = 100;
    // 
    // labelRec1
    // 
    labelRec1.Anchor =  AnchorStyles.Top | AnchorStyles.Right;
    labelRec1.AutoSize = true;
    labelRec1.ForeColor = Color.Teal;
    labelRec1.Location = new Point(3, 13);
    labelRec1.Name = "labelRec1";
    labelRec1.Padding = new Padding(0, 3, 0, 0);
    labelRec1.Size = new Size(36, 16);
    labelRec1.TabIndex = 8;
    labelRec1.Text = "Rec 1";
    // 
    // labelHeaderCol1
    // 
    labelHeaderCol1.Anchor = AnchorStyles.None;
    labelHeaderCol1.AutoSize = true;
    labelHeaderCol1.ForeColor = Color.Teal;
    labelHeaderCol1.Location = new Point(65, 0);
    labelHeaderCol1.Name = "labelHeaderCol1";
    labelHeaderCol1.Size = new Size(51, 13);
    labelHeaderCol1.TabIndex = 6;
    labelHeaderCol1.Text = "Column 1";
    // 
    // labelHeaderCol2
    // 
    labelHeaderCol2.Anchor = AnchorStyles.None;
    labelHeaderCol2.AutoSize = true;
    labelHeaderCol2.ForeColor = Color.Teal;
    labelHeaderCol2.Location = new Point(235, 0);
    labelHeaderCol2.Name = "labelHeaderCol2";
    labelHeaderCol2.Size = new Size(51, 13);
    labelHeaderCol2.TabIndex = 7;
    labelHeaderCol2.Text = "Column 2";
    // 
    // radioButtonQualifyOnlyIfNeeded
    // 
    radioButtonQualifyOnlyIfNeeded.Anchor = AnchorStyles.Left;
    radioButtonQualifyOnlyIfNeeded.AutoSize = true;
    radioButtonQualifyOnlyIfNeeded.Location = new Point(372, 4);
    radioButtonQualifyOnlyIfNeeded.Name = "radioButtonQualifyOnlyIfNeeded";
    radioButtonQualifyOnlyIfNeeded.Size = new Size(131, 17);
    radioButtonQualifyOnlyIfNeeded.TabIndex = 3;
    radioButtonQualifyOnlyIfNeeded.TabStop = true;
    radioButtonQualifyOnlyIfNeeded.Text = "Qualify Only If Needed";
    toolTip.SetToolTip(radioButtonQualifyOnlyIfNeeded, "Writing Text the content is quoted only if it's necessary ");
    radioButtonQualifyOnlyIfNeeded.UseVisualStyleBackColor = true;
    radioButtonQualifyOnlyIfNeeded.Visible = false;
    // 
    // radioButtonQualifyAlways
    // 
    radioButtonQualifyAlways.Anchor = AnchorStyles.Left;
    radioButtonQualifyAlways.AutoSize = true;
    radioButtonQualifyAlways.Location = new Point(372, 30);
    radioButtonQualifyAlways.Name = "radioButtonQualifyAlways";
    radioButtonQualifyAlways.Size = new Size(93, 17);
    radioButtonQualifyAlways.TabIndex = 7;
    radioButtonQualifyAlways.TabStop = true;
    radioButtonQualifyAlways.Text = "Qualify Always";
    toolTip.SetToolTip(radioButtonQualifyAlways, "Writing Text the content is quoted even its is not required");
    radioButtonQualifyAlways.UseVisualStyleBackColor = true;
    radioButtonQualifyAlways.Visible = false;
    // 
    // comboBoxTrimmingOption
    // 
    comboBoxTrimmingOption.Anchor = AnchorStyles.Left;
    comboBoxTrimmingOption.DisplayMember = "Display";
    comboBoxTrimmingOption.DropDownStyle = ComboBoxStyle.DropDownList;
    comboBoxTrimmingOption.Location = new Point(95, 55);
    comboBoxTrimmingOption.Name = "comboBoxTrimmingOption";
    comboBoxTrimmingOption.Size = new Size(96, 21);
    comboBoxTrimmingOption.TabIndex = 8;
    toolTip.SetToolTip(comboBoxTrimmingOption, "None will preserve whitespace; Unquoted will remove white spaces if the column was not quoted; All will remove white spaces even if the column was quoted");
    comboBoxTrimmingOption.ValueMember = "ID";
    // 
    // checkBoxContextSensitiveQualifier
    // 
    checkBoxContextSensitiveQualifier.Anchor = AnchorStyles.Left;
    checkBoxContextSensitiveQualifier.AutoSize = true;
    checkBoxContextSensitiveQualifier.Location = new Point(197, 4);
    checkBoxContextSensitiveQualifier.Name = "checkBoxContextSensitiveQualifier";
    checkBoxContextSensitiveQualifier.Size = new Size(169, 17);
    checkBoxContextSensitiveQualifier.TabIndex = 2;
    checkBoxContextSensitiveQualifier.Text = "Context Sensitive Qualification";
    toolTip.SetToolTip(checkBoxContextSensitiveQualifier, "This is a uncommon way of quoting but allows to parse incorrectly quoted files, a quote is only regarded as closing quote if it is followed by linefeed or delimiter");
    checkBoxContextSensitiveQualifier.UseVisualStyleBackColor = true;
    // 
    // checkBoxDuplicateQualifierToEscape
    // 
    checkBoxDuplicateQualifierToEscape.Anchor = AnchorStyles.Left;
    checkBoxDuplicateQualifierToEscape.AutoSize = true;
    checkBoxDuplicateQualifierToEscape.Location = new Point(197, 30);
    checkBoxDuplicateQualifierToEscape.Name = "checkBoxDuplicateQualifierToEscape";
    checkBoxDuplicateQualifierToEscape.Size = new Size(134, 17);
    checkBoxDuplicateQualifierToEscape.TabIndex = 6;
    checkBoxDuplicateQualifierToEscape.Text = "Repeated Qualification";
    toolTip.SetToolTip(checkBoxDuplicateQualifierToEscape, "Assume a repeated quote in a qualified text represent a quote that does not end text qualification, usually either repeated quoting or escaped charters are used. ");
    checkBoxDuplicateQualifierToEscape.UseVisualStyleBackColor = true;
    // 
    // charBoxFieldQualifierChar
    // 
    charBoxFieldQualifierChar.Anchor = AnchorStyles.Left;
    charBoxFieldQualifierChar.AutoCompleteCustomSource.AddRange(new string[] { "\"", "'" });
    charBoxFieldQualifierChar.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
    charBoxFieldQualifierChar.AutoCompleteSource = AutoCompleteSource.CustomSource;
    charBoxFieldQualifierChar.Location = new Point(95, 3);
    charBoxFieldQualifierChar.Name = "charBoxFieldQualifierChar";
    charBoxFieldQualifierChar.Size = new Size(96, 20);
    charBoxFieldQualifierChar.TabIndex = 1;
    toolTip.SetToolTip(charBoxFieldQualifierChar, "Columns may be qualified with a character; usually these are \" the quotes are removed by the reading applications. This is needed in case a line feed or a delimiter is part of the column");
    charBoxFieldQualifierChar.Type = PunctuationTextBox.PunctuationType.Qualifier;
    charBoxFieldQualifierChar.TextChanged += TextBoxQuote_TextChanged;
    // 
    // textBoxQualifierPlaceholder
    // 
    textBoxQualifierPlaceholder.Anchor = AnchorStyles.Left;
    textBoxQualifierPlaceholder.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
    textBoxQualifierPlaceholder.Location = new Point(95, 29);
    textBoxQualifierPlaceholder.Name = "textBoxQualifierPlaceholder";
    textBoxQualifierPlaceholder.Size = new Size(96, 20);
    textBoxQualifierPlaceholder.TabIndex = 5;
    toolTip.SetToolTip(textBoxQualifierPlaceholder, "Placeholder for Qualifiers in text");
    // 
    // errorProvider
    // 
    errorProvider.ContainerControl = this;
    // 
    // tableLayoutPanel
    // 
    tableLayoutPanel.AutoSize = true;
    tableLayoutPanel.ColumnCount = 4;
    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
    tableLayoutPanel.Controls.Add(splitContainer, 0, 3);
    tableLayoutPanel.Controls.Add(labelQuote, 0, 0);
    tableLayoutPanel.Controls.Add(charBoxFieldQualifierChar, 1, 0);
    tableLayoutPanel.Controls.Add(checkBoxContextSensitiveQualifier, 2, 0);
    tableLayoutPanel.Controls.Add(checkBoxDuplicateQualifierToEscape, 2, 1);
    tableLayoutPanel.Controls.Add(labelQuotePlaceholder, 0, 1);
    tableLayoutPanel.Controls.Add(textBoxQualifierPlaceholder, 1, 1);
    tableLayoutPanel.Controls.Add(labelTrim, 0, 2);
    tableLayoutPanel.Controls.Add(comboBoxTrimmingOption, 1, 2);
    tableLayoutPanel.Controls.Add(radioButtonQualifyOnlyIfNeeded, 3, 0);
    tableLayoutPanel.Controls.Add(radioButtonQualifyAlways, 3, 1);
    tableLayoutPanel.Dock = DockStyle.Top;
    tableLayoutPanel.Location = new Point(0, 0);
    tableLayoutPanel.Name = "tableLayoutPanel";
    tableLayoutPanel.RowCount = 4;
    tableLayoutPanel.RowStyles.Add(new RowStyle());
    tableLayoutPanel.RowStyles.Add(new RowStyle());
    tableLayoutPanel.RowStyles.Add(new RowStyle());
    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
    tableLayoutPanel.Size = new Size(708, 199);
    tableLayoutPanel.TabIndex = 0;
    // 
    // labelQuote
    // 
    labelQuote.Anchor = AnchorStyles.Right;
    labelQuote.AutoSize = true;
    labelQuote.Location = new Point(17, 6);
    labelQuote.Name = "labelQuote";
    labelQuote.Size = new Size(72, 13);
    labelQuote.TabIndex = 0;
    labelQuote.Text = "Text Qualifier:";
    // 
    // labelQuotePlaceholder
    // 
    labelQuotePlaceholder.Anchor = AnchorStyles.Right;
    labelQuotePlaceholder.AutoSize = true;
    labelQuotePlaceholder.Location = new Point(23, 32);
    labelQuotePlaceholder.Name = "labelQuotePlaceholder";
    labelQuotePlaceholder.Size = new Size(66, 13);
    labelQuotePlaceholder.TabIndex = 4;
    labelQuotePlaceholder.Text = "Placeholder:";
    // 
    // labelTrim
    // 
    labelTrim.Anchor = AnchorStyles.Right;
    labelTrim.AutoSize = true;
    labelTrim.Location = new Point(3, 59);
    labelTrim.Name = "labelTrim";
    labelTrim.Size = new Size(86, 13);
    labelTrim.TabIndex = 7;
    labelTrim.Text = "Trimming Option:";
    // 
    // timerRebuilt
    // 
    timerRebuilt.Enabled = true;
    timerRebuilt.Interval = 200;
    timerRebuilt.Tick += TimerRebuilt_Tick;
    // 
    // QuotingControl
    // 
    Controls.Add(tableLayoutPanel);
    MinimumSize = new Size(31, 0);
    Name = "QuotingControl";
    Size = new Size(708, 202);
    splitContainer.Panel1.ResumeLayout(false);
    splitContainer.Panel1.PerformLayout();
    splitContainer.Panel2.ResumeLayout(false);
    splitContainer.Panel2.PerformLayout();
    ((ISupportInitialize) splitContainer).EndInit();
    splitContainer.ResumeLayout(false);
    tableLayoutPanelText.ResumeLayout(false);
    tableLayoutPanelText.PerformLayout();
    ((ISupportInitialize) fastColoredTextBoxLeft).EndInit();
    tableLayoutPanelColumns.ResumeLayout(false);
    tableLayoutPanelColumns.PerformLayout();
    ((ISupportInitialize) fastColoredTextBox12).EndInit();
    ((ISupportInitialize) fastColoredTextBox02).EndInit();
    ((ISupportInitialize) fastColoredTextBox01).EndInit();
    ((ISupportInitialize) fastColoredTextBox11).EndInit();
    ((ISupportInitialize) fastColoredTextBox00).EndInit();
    ((ISupportInitialize) fastColoredTextBox10).EndInit();
    ((ISupportInitialize) errorProvider).EndInit();
    tableLayoutPanel.ResumeLayout(false);
    tableLayoutPanel.PerformLayout();
    ResumeLayout(false);
    PerformLayout();

  }
#pragma warning restore CS8622

  private void TextBoxQuote_TextChanged(object? sender, EventArgs e)
  {
    textBoxQualifierPlaceholder.Enabled =  !string.IsNullOrEmpty(charBoxFieldQualifierChar.Text);
    radioButtonQualifyOnlyIfNeeded.Enabled =  !string.IsNullOrEmpty(charBoxFieldQualifierChar.Text);
    radioButtonQualifyAlways.Enabled =  !string.IsNullOrEmpty(charBoxFieldQualifierChar.Text);
    checkBoxContextSensitiveQualifier.Enabled =  !string.IsNullOrEmpty(charBoxFieldQualifierChar.Text);
    checkBoxDuplicateQualifierToEscape.Enabled =  !string.IsNullOrEmpty(charBoxFieldQualifierChar.Text);

  }

  private void TimerRebuilt_Tick(object? sender, EventArgs e)
  {
    if (!m_HasChanges)
      return;
    m_HasChanges=false;
    try
    {
      checkBoxContextSensitiveQualifier.Enabled = FieldQualifierChar !=char.MinValue;

      var trimming = TrimmingOptionEnum.All;
      if (!m_IsWriteSetting && comboBoxTrimmingOption.SelectedIndex != -1)
      {
#pragma warning disable CS8605
        trimming = (TrimmingOptionEnum) comboBoxTrimmingOption.SelectedValue;
#pragma warning restore CS8605        
      }


      fastColoredTextBox00!.Text = "This is";
      if (trimming == TrimmingOptionEnum.Unquoted || trimming == TrimmingOptionEnum.None)
        fastColoredTextBox00.AppendText(" ");

      fastColoredTextBox01!.Clear();
      if (trimming == TrimmingOptionEnum.Unquoted || trimming == TrimmingOptionEnum.None)
        fastColoredTextBox01.AppendText(" ");
      fastColoredTextBox01.AppendText("a Trimming");
      if (trimming == TrimmingOptionEnum.Unquoted || trimming == TrimmingOptionEnum.None)
        fastColoredTextBox01.AppendText(" ");


      fastColoredTextBox02!.Text = "Example";
      if (trimming == TrimmingOptionEnum.None)
        fastColoredTextBox02.AppendText(" ");


      fastColoredTextBox00.Range.SetStyle(m_SpaceStyle, m_SpaceRegex);
      fastColoredTextBox01.Range.SetStyle(m_SpaceStyle, m_SpaceRegex);
      fastColoredTextBox02.Range.SetStyle(m_SpaceStyle, m_SpaceRegex);

      var canContainQuotes = !string.IsNullOrEmpty(QualifierPlaceholder)
                             || ContextSensitiveQualifier
                             || DuplicateQualifierToEscape
                             || EscapePrefixChar != char.MinValue
                             || FieldQualifierChar == char.MinValue;
      errorProvider!.SetError(checkBoxDuplicateQualifierToEscape, canContainQuotes ? null : "Text can not contain qualifier");
      errorProvider.SetError(checkBoxContextSensitiveQualifier, canContainQuotes ? null : "Text can not contain qualifier");
      errorProvider.SetError(fastColoredTextBox11!, canContainQuotes ? null : "The contained quote would cause closing of the column unless placeholder, repeated quotes or context sensitive quoting is used.");

      var delimiter = FieldDelimiterChar;
      var quote = FieldQualifierChar;

      errorProvider.SetError(charBoxFieldQualifierChar, (delimiter == quote) ? "Delimiter and Quote have to be different" : null);

      // ReSharper disable once IdentifierTypo
      var delim = (delimiter == '\t') ? m_DelimiterTab : m_Delimiter;
      errorProvider.SetError(fastColoredTextBox10!, (quote == char.MinValue) ? "Without quoting a delimiter can not be part of a column" : null);
      errorProvider.SetError(fastColoredTextBox12!, (quote == char.MinValue) ? "Without quoting a linefeed can not be part of a column" : null);

      fastColoredTextBox10!.Text = "Column with:";
      fastColoredTextBox10.AppendText(delimiter.ToStringHandle0(), delim);
      fastColoredTextBox10.AppendText(" Delimiter");

      fastColoredTextBox12!.Clear();
      fastColoredTextBox12.Text = "Column with ";
      fastColoredTextBox12.AppendText("¶", m_PilcrowStyle);
      fastColoredTextBox12.AppendText("\r\nLinefeed");

      fastColoredTextBox11!.Clear();
      fastColoredTextBox11.Text = "Column with: ";
      fastColoredTextBox11.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
      fastColoredTextBox11.AppendText(" Quote");

      fastColoredTextBoxLeft!.Clear();

      var newToolTip = m_IsWriteSetting
        ? "If this placeholder is part of the text it will be read as quoting character"
        : "If the quoting character is part of the text it will be represented by the placeholder";

      if (m_IsWriteSetting)
      {
        if (radioButtonQualifyAlways.Checked) fastColoredTextBoxLeft.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
        fastColoredTextBoxLeft.AppendText("This is");
        if (radioButtonQualifyAlways.Checked) fastColoredTextBoxLeft.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
      }
      else
      {
        fastColoredTextBoxLeft.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
        fastColoredTextBoxLeft.AppendText("This is ");
        fastColoredTextBoxLeft.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
      }

      fastColoredTextBoxLeft.AppendText(delimiter.ToString(), delim);
      fastColoredTextBoxLeft.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
      fastColoredTextBoxLeft.AppendText("Column with:");
      fastColoredTextBoxLeft.AppendText(delimiter.ToString(), delim);
      fastColoredTextBoxLeft.AppendText(" Delimiter");
      fastColoredTextBoxLeft.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
      fastColoredTextBoxLeft.AppendText("¶", m_PilcrowStyle);
      fastColoredTextBoxLeft.AppendText("\r\n");

      if (m_IsWriteSetting)
      {
        if (radioButtonQualifyAlways.Checked) fastColoredTextBoxLeft.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
        fastColoredTextBoxLeft.AppendText("a Trimming");
        if (radioButtonQualifyAlways.Checked) fastColoredTextBoxLeft.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
      }
      else
      {
        fastColoredTextBoxLeft.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
        fastColoredTextBoxLeft.AppendText(" a Trimming ");
        fastColoredTextBoxLeft.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
      }

      fastColoredTextBoxLeft.AppendText(delimiter.ToString(), delim);
      fastColoredTextBoxLeft.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
      fastColoredTextBoxLeft.AppendText("Column with: ");


      if (!string.IsNullOrEmpty(textBoxQualifierPlaceholder.Text) && quote == char.MinValue)
      {
        newToolTip += m_IsWriteSetting
          ? $"\r\nhello {quote} world ->{quote}hello {textBoxQualifierPlaceholder.Text} world{quote}"
          : $"\r\n{quote}hello {textBoxQualifierPlaceholder.Text} world{quote} -> hello {quote} world";
        fastColoredTextBoxLeft.AppendText(textBoxQualifierPlaceholder.Text, m_EscapedQuoteStyle);
      }
      else
      {
        if (DuplicateQualifierToEscape && EscapePrefixChar != char.MinValue &&
            !m_IsWriteSetting)
        {
          fastColoredTextBoxLeft.AppendText(new string(quote, 2) + " or " + EscapePrefixChar.ToStringHandle0() + quote.ToStringHandle0(),
            m_EscapedQuoteStyle);
        }
        else if (DuplicateQualifierToEscape)
          fastColoredTextBoxLeft.AppendText(new string(quote, 2), m_EscapedQuoteStyle);
        else if (EscapePrefixChar != char.MinValue)
          fastColoredTextBoxLeft.AppendText(EscapePrefixChar.ToStringHandle0() + quote.ToStringHandle0(), m_EscapedQuoteStyle);
        else
          fastColoredTextBoxLeft.AppendText(quote.ToStringHandle0(), m_EscapedQuoteStyle);
      }

      fastColoredTextBoxLeft.AppendText(" Quote");
      fastColoredTextBoxLeft.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
      fastColoredTextBoxLeft.AppendText("¶", m_PilcrowStyle);
      fastColoredTextBoxLeft.AppendText("\r\n");
      if (m_IsWriteSetting && radioButtonQualifyAlways.Checked)
      {
        fastColoredTextBoxLeft.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
        fastColoredTextBoxLeft.AppendText("Example");
        fastColoredTextBoxLeft.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
      }
      else
        fastColoredTextBoxLeft.AppendText("Example ");

      fastColoredTextBoxLeft.AppendText(delimiter.ToString(), delim);
      fastColoredTextBoxLeft.AppendText(quote.ToStringHandle0(), m_QuoteStyle);
      fastColoredTextBoxLeft.AppendText("Column with ");
      fastColoredTextBoxLeft.AppendText("¶", m_PilcrowStyle);
      fastColoredTextBoxLeft.AppendText("\r\nLinefeed");
      fastColoredTextBoxLeft.AppendText(quote.ToStringHandle0(), m_QuoteStyle);

      fastColoredTextBoxLeft.Range.SetStyle(m_SpaceStyle, m_SpaceRegex);
      fastColoredTextBox10.Range.SetStyle(m_SpaceStyle, m_SpaceRegex);
      fastColoredTextBox11.Range.SetStyle(m_SpaceStyle, m_SpaceRegex);
      fastColoredTextBox12.Range.SetStyle(m_SpaceStyle, m_SpaceRegex);

      toolTip!.SetToolTip(textBoxQualifierPlaceholder, newToolTip);
    }
    catch (Exception exception)
    {
      Extensions.ShowError(exception, "Quoting Control");
    }
  }
}