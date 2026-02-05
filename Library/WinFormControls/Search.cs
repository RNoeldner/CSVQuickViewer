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

using System.Diagnostics.CodeAnalysis;

namespace CsvTools;

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

/// <summary>
///   A Control to show a search and raised appropriate events
/// </summary>
public class Search : UserControl
{
  /// <summary>
  ///   Required designer variable.
  /// </summary>
  private Container components;

  // private read only System.Timers.Timer m_TimerChange = new System.Timers.Timer();
  private Button m_BtnCancel = new Button();

  private Button m_BtnNext = new Button();

  private Button m_BtnPrevious = new Button();

  private int m_CurrentResult = -1;

  private Label m_LblResults = new Label();

  private int m_Results;

  private TextBox m_SearchTextBoxText;
  private Timer m_TimerChange;
  private TableLayoutPanel m_TableLayoutPanel;

  /// <summary>
  ///   Initializes a new instance of the <see cref="Search" /> class.
  /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
  public Search()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
  {
    InitializeComponent();
    Results = 0;
  }

  /// <summary>
  ///   Occurs when the next result should be shown
  /// </summary>
  public event EventHandler<SearchEventArgs>? OnResultChanged;

  /// <summary>
  ///   Occurs when the search text is changed.
  /// </summary>
  public event EventHandler<SearchEventArgs>? OnSearchChanged;

  /// <summary>
  ///   Occurs when the search should be cleared.
  /// </summary>
  public event EventHandler? OnSearchClear;

  /// <summary>
  ///   Gets or sets the number of found results.
  /// </summary>
  /// <value>The results.</value>
  public int Results
  {
    get => m_Results;

    set
    {
      m_Results = value;
      if (value == 0)
        CurrentResult = 0;
      else if (CurrentResult == 0)
        CurrentResult = 1;

      UpdateDisplay();
    }
  }

  /// <summary>
  ///   Gets or sets the current result.
  /// </summary>
  /// <value>The current result.</value>
  private int CurrentResult
  {
    get => m_CurrentResult;

    set
    {
      if (value > m_Results)
        value = m_Results;

      if (!m_CurrentResult.Equals(value))
      {
        m_CurrentResult = value;
        OnResultChanged?.SafeInvoke(this, new SearchEventArgs(m_SearchTextBoxText.Text, m_CurrentResult));
      }

      UpdateDisplay();
    }
  }

  /// <summary>
  ///   Clean up any resources being used.
  /// </summary>
  /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
  protected override void Dispose(bool disposing)
  {
    if (disposing)
      components?.Dispose();

    base.Dispose(disposing);
  }

  private void Cancel_Click(object? sender, EventArgs e)
  {
    OnSearchClear?.SafeInvoke(this);
    Hide();
  }

  /// <summary>
  ///   Required method for Designer support - do not modify the contents of this method with the
  ///   code editor.
  /// </summary>
  [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
  [SuppressMessage("ReSharper", "JoinDeclarationAndInitializer")]
  [SuppressMessage("ReSharper", "RedundantNameQualifier")]
  [SuppressMessage("ReSharper", "RedundantCast")]
  [SuppressMessage("ReSharper", "RedundantDelegateCreation")]
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
  private void InitializeComponent()
  {
    components = new Container();
    Label label1;
    var resources = new ComponentResourceManager(typeof(Search));
    m_BtnCancel = new Button();
    m_SearchTextBoxText = new TextBox();
    m_LblResults = new Label();
    m_BtnNext = new Button();
    m_BtnPrevious = new Button();
    m_TableLayoutPanel = new TableLayoutPanel();
    m_TimerChange = new Timer(components);
    label1 = new Label();
    m_TableLayoutPanel.SuspendLayout();
    SuspendLayout();
    // 
    // label1
    // 
    label1.Anchor = AnchorStyles.Left;
    label1.AutoSize = true;
    label1.Location = new Point(3, 6);
    label1.Name = "label1";
    label1.Size = new Size(30, 13);
    label1.TabIndex = 0;
    label1.Text = "Find:";
    // 
    // m_BtnCancel
    // 
    m_BtnCancel.Anchor = AnchorStyles.Left;
    m_BtnCancel.DialogResult = DialogResult.Cancel;
    m_BtnCancel.FlatAppearance.BorderColor = SystemColors.ButtonFace;
    m_BtnCancel.FlatStyle = FlatStyle.Flat;
    m_BtnCancel.Image = (Image) resources.GetObject("m_BtnCancel.Image");
    m_BtnCancel.Location = new Point(253, 3);
    m_BtnCancel.Name = "m_BtnCancel";
    m_BtnCancel.Size = new Size(19, 19);
    m_BtnCancel.TabIndex = 4;
    m_BtnCancel.UseVisualStyleBackColor = false;
    m_BtnCancel.Click += Cancel_Click;
    // 
    // m_SearchTextBoxText
    // 
    m_SearchTextBoxText.BorderStyle = BorderStyle.FixedSingle;
    m_SearchTextBoxText.Location = new Point(39, 3);
    m_SearchTextBoxText.MaxLength = 50;
    m_SearchTextBoxText.Name = "m_SearchTextBoxText";
    m_SearchTextBoxText.Size = new Size(98, 20);
    m_SearchTextBoxText.TabIndex = 1;
    m_SearchTextBoxText.TextChanged += SearchText_TextChanged;
    // 
    // m_LblResults
    // 
    m_LblResults.Anchor = AnchorStyles.Left;
    m_LblResults.AutoSize = true;
    m_LblResults.Location = new Point(163, 6);
    m_LblResults.Name = "m_LblResults";
    m_LblResults.Size = new Size(34, 13);
    m_LblResults.TabIndex = 0;
    m_LblResults.Text = "0 of 0";
    m_LblResults.TextChanged += LblResultsTextChanged;
    // 
    // m_BtnNext
    // 
    m_BtnNext.Anchor = AnchorStyles.Left;
    m_BtnNext.BackgroundImageLayout = ImageLayout.Center;
    m_BtnNext.FlatAppearance.BorderColor = SystemColors.ButtonFace;
    m_BtnNext.FlatStyle = FlatStyle.Flat;
    m_BtnNext.Image = (Image) resources.GetObject("m_BtnNext.Image");
    m_BtnNext.Location = new Point(228, 3);
    m_BtnNext.Name = "m_BtnNext";
    m_BtnNext.Size = new Size(19, 19);
    m_BtnNext.TabIndex = 6;
    m_BtnNext.UseVisualStyleBackColor = false;
    m_BtnNext.Click += Next_Click;
    // 
    // m_BtnPrevious
    // 
    m_BtnPrevious.Anchor = AnchorStyles.Left;
    m_BtnPrevious.BackgroundImageLayout = ImageLayout.Center;
    m_BtnPrevious.FlatAppearance.BorderColor = SystemColors.ButtonFace;
    m_BtnPrevious.FlatStyle = FlatStyle.Flat;
    m_BtnPrevious.Image = (Image) resources.GetObject("m_BtnPrevious.Image");
    m_BtnPrevious.Location = new Point(203, 3);
    m_BtnPrevious.Name = "m_BtnPrevious";
    m_BtnPrevious.Size = new Size(19, 19);
    m_BtnPrevious.TabIndex = 5;
    m_BtnPrevious.UseVisualStyleBackColor = false;
    m_BtnPrevious.Click += Previous_Click;
    // 
    // m_TableLayoutPanel
    // 
    m_TableLayoutPanel.ColumnCount = 6;
    m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
    m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
    m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
    m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
    m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
    m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
    m_TableLayoutPanel.Controls.Add(label1, 0, 0);
    m_TableLayoutPanel.Controls.Add(m_BtnCancel, 5, 0);
    m_TableLayoutPanel.Controls.Add(m_BtnNext, 4, 0);
    m_TableLayoutPanel.Controls.Add(m_SearchTextBoxText, 1, 0);
    m_TableLayoutPanel.Controls.Add(m_BtnPrevious, 3, 0);
    m_TableLayoutPanel.Controls.Add(m_LblResults, 2, 0);
    m_TableLayoutPanel.Dock = DockStyle.Top;
    m_TableLayoutPanel.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
    m_TableLayoutPanel.Location = new Point(0, 0);
    m_TableLayoutPanel.Name = "m_TableLayoutPanel";
    m_TableLayoutPanel.RowCount = 1;
    m_TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
    m_TableLayoutPanel.Size = new Size(275, 25);
    m_TableLayoutPanel.TabIndex = 7;
    // 
    // m_TimerChange
    // 
    m_TimerChange.Enabled = true;
    m_TimerChange.Interval = 200;
    m_TimerChange.Tick += m_TimerChange_Tick;
    // 
    // Search
    // 
    Controls.Add(m_TableLayoutPanel);
    Name = "Search";
    Size = new Size(275, 25);
    m_TableLayoutPanel.ResumeLayout(false);
    m_TableLayoutPanel.PerformLayout();
    ResumeLayout(false);
  }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
  private void LblResultsTextChanged(object? sender, EventArgs e)
  {
    m_LblResults.Left = 250 - m_LblResults.Width;
    m_SearchTextBoxText.Width = m_LblResults.Left - 76;
  }

  private void Next_Click(object? sender, EventArgs e)
  {
    if (m_CurrentResult < m_Results)
      CurrentResult++;
  }

  private void Previous_Click(object? sender, EventArgs e)
  {
    if (m_CurrentResult > 1)
      CurrentResult--;
  }

  private void SearchText_TextChanged(object? sender, EventArgs e)
  {
    m_TimerChange.Stop();
    m_TimerChange.Start();
  }

  private void UpdateDisplay()
  {
    m_SearchTextBoxText.SafeInvoke(() =>
    {
      if (m_CurrentResult > 0)
      {
        m_SearchTextBoxText.ForeColor = SystemColors.WindowText;
        m_LblResults.Text = $@"{m_CurrentResult} of {m_Results}";
      }
      else
      {
        m_SearchTextBoxText.ForeColor =
          string.IsNullOrEmpty(m_SearchTextBoxText.Text) ? SystemColors.WindowText : Color.Red;
        m_LblResults.Text = string.Empty;
      }

      m_BtnPrevious.Enabled = m_CurrentResult > 1;
      m_BtnNext.Enabled = m_Results > m_CurrentResult;
    });
  }
  private string m_LastSearch = string.Empty;
  private void m_TimerChange_Tick(object? sender, EventArgs e)
  {
    m_TimerChange.Enabled = false;
    var currentText = m_SearchTextBoxText.Text;
    // Only fire if the text actually changed (avoids firing on focus changes or non-text keys)
    if (currentText == m_LastSearch) return;
    try
    {
      // If empty, you might want to trigger Clear instead of Search
      if (string.IsNullOrEmpty(currentText))
      {
        OnSearchClear?.SafeInvoke(this);
        return;
      }
      OnSearchChanged?.SafeInvoke(this, new SearchEventArgs(currentText));
    }
    catch (Exception ex)
    {
      ParentForm!.ShowError(ex, "Error during search");
    }
    finally
    {
      m_TimerChange.Enabled = true;
      m_LastSearch = currentText;
    }
  }
}