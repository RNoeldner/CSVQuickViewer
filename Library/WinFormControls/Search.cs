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

    private TextBox m_SearchTextBoxText = new TextBox();

    private readonly TableLayoutPanel m_TableLayoutPanel = new TableLayoutPanel();

    /// <summary>
    ///   Initializes a new instance of the <see cref="Search" /> class.
    /// </summary>
    public Search()
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
    private void InitializeComponent()
    {
      System.Windows.Forms.Label label1;
      var resources = new System.ComponentModel.ComponentResourceManager(typeof(Search));
      m_BtnCancel = new System.Windows.Forms.Button();
      m_SearchTextBoxText = new System.Windows.Forms.TextBox();
      m_LblResults = new System.Windows.Forms.Label();
      m_BtnNext = new System.Windows.Forms.Button();
      m_BtnPrevious = new System.Windows.Forms.Button();

      label1 = new System.Windows.Forms.Label();
      m_TableLayoutPanel.SuspendLayout();
      SuspendLayout();
      // label1
      label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
      label1.AutoSize = true;
      label1.ForeColor = System.Drawing.SystemColors.InfoText;
      label1.Location = new System.Drawing.Point(3, 9);
      label1.Name = "label1";
      label1.Size = new System.Drawing.Size(72, 17);
      label1.TabIndex = 0;
      label1.Text = "Find what:";
      // m_BtnCancel
      m_BtnCancel.Anchor = System.Windows.Forms.AnchorStyles.Left;
      m_BtnCancel.BackColor = System.Drawing.SystemColors.Info;
      m_BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      m_BtnCancel.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonFace;
      m_BtnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      m_BtnCancel.Image = ((System.Drawing.Image) (resources.GetObject("m_BtnCancel.Image")));
      m_BtnCancel.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
      m_BtnCancel.Location = new System.Drawing.Point(337, 5);
      m_BtnCancel.Name = "m_BtnCancel";
      m_BtnCancel.Size = new System.Drawing.Size(24, 24);
      m_BtnCancel.TabIndex = 4;
      m_BtnCancel.UseVisualStyleBackColor = false;
      m_BtnCancel.Click += new System.EventHandler(Cancel_Click);
      // m_SearchTextBoxText
      m_SearchTextBoxText.Anchor = System.Windows.Forms.AnchorStyles.Left;
      m_SearchTextBoxText.BackColor = System.Drawing.SystemColors.Info;
      m_SearchTextBoxText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      m_SearchTextBoxText.ForeColor = System.Drawing.SystemColors.InfoText;
      m_SearchTextBoxText.Location = new System.Drawing.Point(81, 6);
      m_SearchTextBoxText.MaxLength = 50;
      m_SearchTextBoxText.Name = "m_SearchTextBoxText";
      m_SearchTextBoxText.Size = new System.Drawing.Size(95, 22);
      m_SearchTextBoxText.TabIndex = 1;
      m_SearchTextBoxText.TextChanged += new System.EventHandler(SearchText_TextChanged);
      // m_LblResults
      m_LblResults.Anchor = System.Windows.Forms.AnchorStyles.Left;
      m_LblResults.AutoSize = true;
      m_LblResults.ForeColor = System.Drawing.SystemColors.InfoText;
      m_LblResults.Location = new System.Drawing.Point(227, 9);
      m_LblResults.Name = "m_LblResults";
      m_LblResults.Size = new System.Drawing.Size(44, 17);
      m_LblResults.TabIndex = 0;
      m_LblResults.Text = "0 of 0";
      m_LblResults.TextChanged += new System.EventHandler(LblResultsTextChanged);
      // m_BtnNext
      m_BtnNext.Anchor = System.Windows.Forms.AnchorStyles.Left;
      m_BtnNext.BackColor = System.Drawing.SystemColors.Info;
      m_BtnNext.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      m_BtnNext.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonFace;
      m_BtnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      m_BtnNext.Image = ((System.Drawing.Image) (resources.GetObject("m_BtnNext.Image")));
      m_BtnNext.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
      m_BtnNext.Location = new System.Drawing.Point(307, 5);
      m_BtnNext.Name = "m_BtnNext";
      m_BtnNext.Size = new System.Drawing.Size(24, 24);
      m_BtnNext.TabIndex = 6;
      m_BtnNext.UseVisualStyleBackColor = false;
      m_BtnNext.Click += new System.EventHandler(Next_Click);
      // m_BtnPrevious
      m_BtnPrevious.Anchor = System.Windows.Forms.AnchorStyles.Left;
      m_BtnPrevious.BackColor = System.Drawing.SystemColors.Info;
      m_BtnPrevious.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      m_BtnPrevious.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonFace;
      m_BtnPrevious.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      m_BtnPrevious.Image = ((System.Drawing.Image) (resources.GetObject("m_BtnPrevious.Image")));
      m_BtnPrevious.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
      m_BtnPrevious.Location = new System.Drawing.Point(277, 5);
      m_BtnPrevious.Name = "m_BtnPrevious";
      m_BtnPrevious.Size = new System.Drawing.Size(24, 24);
      m_BtnPrevious.TabIndex = 5;
      m_BtnPrevious.UseVisualStyleBackColor = false;
      m_BtnPrevious.Click += new System.EventHandler(Previous_Click);
      // m_TableLayoutPanel
      m_TableLayoutPanel.ColumnCount = 6;
      m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      m_TableLayoutPanel.Controls.Add(label1, 0, 0);
      m_TableLayoutPanel.Controls.Add(m_BtnCancel, 5, 0);
      m_TableLayoutPanel.Controls.Add(m_BtnNext, 4, 0);
      m_TableLayoutPanel.Controls.Add(m_SearchTextBoxText, 1, 0);
      m_TableLayoutPanel.Controls.Add(m_BtnPrevious, 3, 0);
      m_TableLayoutPanel.Controls.Add(m_LblResults, 2, 0);
      m_TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      m_TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      m_TableLayoutPanel.RowCount = 1;
      m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      m_TableLayoutPanel.Size = new System.Drawing.Size(364, 35);
      m_TableLayoutPanel.TabIndex = 7;
      // Search
      BackColor = System.Drawing.SystemColors.Info;
      Controls.Add(m_TableLayoutPanel);
      Name = "Search";
      Size = new System.Drawing.Size(364, 35);
      m_TableLayoutPanel.ResumeLayout(false);
      m_TableLayoutPanel.PerformLayout();
      ResumeLayout(false);
    }

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
    }
  }
}