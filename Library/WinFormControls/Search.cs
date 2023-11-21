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

using System.Diagnostics.CodeAnalysis;

namespace CsvTools
{
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
    private readonly IContainer? components = null;

    // private read only System.Timers.Timer m_TimerChange = new System.Timers.Timer();
    private Button m_BtnCancel = new Button();

    private Button m_BtnNext = new Button();

    private Button m_BtnPrevious = new Button();

    private int m_CurrentResult = -1;

    private Label m_LblResults = new Label();

    private int m_Results;

    private TextBox m_SearchTextBoxText;

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
          OnResultChanged?.Invoke(this, new SearchEventArgs(m_SearchTextBoxText.Text, m_CurrentResult));
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
      // m_TimerChange.Stop();
      OnSearchClear?.Invoke(this, EventArgs.Empty);
      Hide();
    }

    /// <summary>
    ///   The delay for the filter change has elapsed.
    /// </summary>
    private void FilterValueChangedElapsed() => m_SearchTextBoxText.SafeInvoke(SearchChanged);

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
      System.Windows.Forms.Label label1;
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Search));
      this.m_BtnCancel = new System.Windows.Forms.Button();
      this.m_SearchTextBoxText = new System.Windows.Forms.TextBox();
      this.m_LblResults = new System.Windows.Forms.Label();
      this.m_BtnNext = new System.Windows.Forms.Button();
      this.m_BtnPrevious = new System.Windows.Forms.Button();
      this.m_TableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      label1 = new System.Windows.Forms.Label();
      this.m_TableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // label1
      // 
      label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
      label1.AutoSize = true;
      label1.Location = new System.Drawing.Point(3, 6);
      label1.Name = "label1";
      label1.Size = new System.Drawing.Size(30, 13);
      label1.TabIndex = 0;
      label1.Text = "Find:";
      // 
      // m_BtnCancel
      // 
      this.m_BtnCancel.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.m_BtnCancel.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonFace;
      this.m_BtnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.m_BtnCancel.Image = ((System.Drawing.Image)(resources.GetObject("m_BtnCancel.Image")));
      this.m_BtnCancel.Location = new System.Drawing.Point(325, 3);
      this.m_BtnCancel.Name = "m_BtnCancel";
      this.m_BtnCancel.Size = new System.Drawing.Size(24, 19);
      this.m_BtnCancel.TabIndex = 4;
      this.m_BtnCancel.UseVisualStyleBackColor = false;
      this.m_BtnCancel.Click += new System.EventHandler(this.Cancel_Click);
      // 
      // m_SearchTextBoxText
      // 
      this.m_SearchTextBoxText.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_SearchTextBoxText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.m_SearchTextBoxText.Location = new System.Drawing.Point(39, 3);
      this.m_SearchTextBoxText.MaxLength = 50;
      this.m_SearchTextBoxText.Name = "m_SearchTextBoxText";
      this.m_SearchTextBoxText.Size = new System.Drawing.Size(166, 20);
      this.m_SearchTextBoxText.TabIndex = 1;
      this.m_SearchTextBoxText.TextChanged += new System.EventHandler(this.SearchText_TextChanged);
      // 
      // m_LblResults
      // 
      this.m_LblResults.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_LblResults.AutoSize = true;
      this.m_LblResults.Location = new System.Drawing.Point(225, 6);
      this.m_LblResults.Name = "m_LblResults";
      this.m_LblResults.Size = new System.Drawing.Size(34, 13);
      this.m_LblResults.TabIndex = 0;
      this.m_LblResults.Text = "0 of 0";
      this.m_LblResults.TextChanged += new System.EventHandler(this.LblResultsTextChanged);
      // 
      // m_BtnNext
      // 
      this.m_BtnNext.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_BtnNext.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.m_BtnNext.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonFace;
      this.m_BtnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.m_BtnNext.Image = ((System.Drawing.Image)(resources.GetObject("m_BtnNext.Image")));
      this.m_BtnNext.Location = new System.Drawing.Point(295, 3);
      this.m_BtnNext.Name = "m_BtnNext";
      this.m_BtnNext.Size = new System.Drawing.Size(24, 19);
      this.m_BtnNext.TabIndex = 6;
      this.m_BtnNext.UseVisualStyleBackColor = false;
      this.m_BtnNext.Click += new System.EventHandler(this.Next_Click);
      // 
      // m_BtnPrevious
      // 
      this.m_BtnPrevious.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.m_BtnPrevious.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.m_BtnPrevious.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonFace;
      this.m_BtnPrevious.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.m_BtnPrevious.Image = ((System.Drawing.Image)(resources.GetObject("m_BtnPrevious.Image")));
      this.m_BtnPrevious.Location = new System.Drawing.Point(265, 3);
      this.m_BtnPrevious.Name = "m_BtnPrevious";
      this.m_BtnPrevious.Size = new System.Drawing.Size(24, 19);
      this.m_BtnPrevious.TabIndex = 5;
      this.m_BtnPrevious.UseVisualStyleBackColor = false;
      this.m_BtnPrevious.Click += new System.EventHandler(this.Previous_Click);
      // 
      // m_TableLayoutPanel
      // 
      this.m_TableLayoutPanel.ColumnCount = 6;
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.Controls.Add(label1, 0, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_BtnCancel, 5, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_BtnNext, 4, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_SearchTextBoxText, 1, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_BtnPrevious, 3, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_LblResults, 2, 0);
      this.m_TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      this.m_TableLayoutPanel.RowCount = 1;
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.Size = new System.Drawing.Size(352, 25);
      this.m_TableLayoutPanel.TabIndex = 7;
      // 
      // Search
      // 
      this.Controls.Add(this.m_TableLayoutPanel);
      this.Name = "Search";
      this.Size = new System.Drawing.Size(352, 25);
      this.m_TableLayoutPanel.ResumeLayout(false);
      this.m_TableLayoutPanel.PerformLayout();
      this.ResumeLayout(false);

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

    private void SearchChanged() => OnSearchChanged?.Invoke(this, new SearchEventArgs(m_SearchTextBoxText.Text));

    private void SearchText_TextChanged(object? sender, EventArgs e) =>
      FilterValueChangedElapsed();

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
  }
}