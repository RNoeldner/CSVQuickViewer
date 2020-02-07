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
    private readonly IContainer components = null;

    // private read only System.Timers.Timer m_TimerChange = new System.Timers.Timer();
    private Button m_BtnCancel;

    private Button m_BtnNext;

    private Button m_BtnPrevious;

    private int m_CurrentResult = -1;

    private Label m_LblResults;

    private int m_Results;

    private TextBox m_SearchTextBoxText;

    private TableLayoutPanel tableLayoutPanel1;

    /// <summary>
    ///   Initializes a new instance of the <see cref="Search" /> class.
    /// </summary>
    public Search()
    {
      InitializeComponent();

      // m_TimerChange.Elapsed += FilterValueChangedElapsed;
      // m_TimerChange.Interval = 10;
      // m_TimerChange.AutoReset = false;
      Results = 0;
    }

    /// <summary>
    ///   Occurs when the next result should be shown
    /// </summary>
    public event EventHandler<SearchEventArgs> OnResultChanged;

    /// <summary>
    ///   Occurs when the search text is changed.
    /// </summary>
    public event EventHandler<SearchEventArgs> OnSearchChanged;

    /// <summary>
    ///   Occurs when the search should be cleared.
    /// </summary>
    public event EventHandler OnSearchClear;

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

    private void Cancel_Click(object sender, EventArgs e)
    {
      // m_TimerChange.Stop();
      OnSearchClear?.Invoke(this, null);
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
      Label label1;
      ComponentResourceManager resources = new ComponentResourceManager(typeof(Search));
      this.m_BtnCancel = new Button();
      this.m_SearchTextBoxText = new TextBox();
      this.m_LblResults = new Label();
      this.m_BtnNext = new Button();
      this.m_BtnPrevious = new Button();
      this.tableLayoutPanel1 = new TableLayoutPanel();
      label1 = new Label();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();

      // label1
      label1.Anchor = AnchorStyles.Left;
      label1.AutoSize = true;
      label1.ForeColor = SystemColors.InfoText;
      label1.Location = new Point(3, 7);
      label1.Name = "label1";
      label1.Size = new Size(82, 20);
      label1.TabIndex = 0;
      label1.Text = "Find what:";

      // m_BtnCancel
      this.m_BtnCancel.BackColor = SystemColors.Info;
      this.m_BtnCancel.DialogResult = DialogResult.Cancel;
      this.m_BtnCancel.FlatAppearance.BorderColor = SystemColors.ButtonFace;
      this.m_BtnCancel.FlatStyle = FlatStyle.Flat;
      this.m_BtnCancel.Image = ((Image)(resources.GetObject("m_BtnCancel.Image")));
      this.m_BtnCancel.ImageAlign = ContentAlignment.BottomCenter;
      this.m_BtnCancel.Location = new Point(337, 3);
      this.m_BtnCancel.Name = "m_BtnCancel";
      this.m_BtnCancel.Size = new Size(24, 24);
      this.m_BtnCancel.TabIndex = 4;
      this.m_BtnCancel.UseVisualStyleBackColor = false;
      this.m_BtnCancel.Click += new EventHandler(this.Cancel_Click);

      // m_SearchTextBoxText
      this.m_SearchTextBoxText.Anchor = AnchorStyles.Left;
      this.m_SearchTextBoxText.BackColor = SystemColors.Info;
      this.m_SearchTextBoxText.BorderStyle = BorderStyle.FixedSingle;
      this.m_SearchTextBoxText.ForeColor = SystemColors.InfoText;
      this.m_SearchTextBoxText.Location = new Point(91, 4);
      this.m_SearchTextBoxText.MaxLength = 50;
      this.m_SearchTextBoxText.Name = "m_SearchTextBoxText";
      this.m_SearchTextBoxText.Size = new Size(106, 26);
      this.m_SearchTextBoxText.TabIndex = 1;
      this.m_SearchTextBoxText.TextChanged += new EventHandler(this.SearchText_TextChanged);

      // m_LblResults
      this.m_LblResults.Anchor = AnchorStyles.Left;
      this.m_LblResults.AutoSize = true;
      this.m_LblResults.ForeColor = SystemColors.InfoText;
      this.m_LblResults.Location = new Point(222, 7);
      this.m_LblResults.Name = "m_LblResults";
      this.m_LblResults.Size = new Size(49, 20);
      this.m_LblResults.TabIndex = 0;
      this.m_LblResults.Text = "0 of 0";
      this.m_LblResults.TextChanged += new EventHandler(this.LblResultsTextChanged);

      // m_BtnNext
      this.m_BtnNext.BackColor = SystemColors.Info;
      this.m_BtnNext.BackgroundImageLayout = ImageLayout.Center;
      this.m_BtnNext.FlatAppearance.BorderColor = SystemColors.ButtonFace;
      this.m_BtnNext.FlatStyle = FlatStyle.Flat;
      this.m_BtnNext.Image = ((Image)(resources.GetObject("m_BtnNext.Image")));
      this.m_BtnNext.ImageAlign = ContentAlignment.BottomCenter;
      this.m_BtnNext.Location = new Point(307, 3);
      this.m_BtnNext.Name = "m_BtnNext";
      this.m_BtnNext.Size = new Size(24, 24);
      this.m_BtnNext.TabIndex = 6;
      this.m_BtnNext.UseVisualStyleBackColor = false;
      this.m_BtnNext.Click += new EventHandler(this.Next_Click);

      // m_BtnPrevious
      this.m_BtnPrevious.BackColor = SystemColors.Info;
      this.m_BtnPrevious.BackgroundImageLayout = ImageLayout.Center;
      this.m_BtnPrevious.FlatAppearance.BorderColor = SystemColors.ButtonFace;
      this.m_BtnPrevious.FlatStyle = FlatStyle.Flat;
      this.m_BtnPrevious.Image = ((Image)(resources.GetObject("m_BtnPrevious.Image")));
      this.m_BtnPrevious.ImageAlign = ContentAlignment.BottomCenter;
      this.m_BtnPrevious.Location = new Point(277, 3);
      this.m_BtnPrevious.Name = "m_BtnPrevious";
      this.m_BtnPrevious.Size = new Size(24, 24);
      this.m_BtnPrevious.TabIndex = 5;
      this.m_BtnPrevious.UseVisualStyleBackColor = false;
      this.m_BtnPrevious.Click += new EventHandler(this.Previous_Click);

      // tableLayoutPanel1
      this.tableLayoutPanel1.ColumnCount = 6;
      this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
      this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
      this.tableLayoutPanel1.Controls.Add(label1, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.m_BtnCancel, 5, 0);
      this.tableLayoutPanel1.Controls.Add(this.m_BtnNext, 4, 0);
      this.tableLayoutPanel1.Controls.Add(this.m_SearchTextBoxText, 1, 0);
      this.tableLayoutPanel1.Controls.Add(this.m_BtnPrevious, 3, 0);
      this.tableLayoutPanel1.Controls.Add(this.m_LblResults, 2, 0);
      this.tableLayoutPanel1.Dock = DockStyle.Fill;
      this.tableLayoutPanel1.Location = new Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 1;
      this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
      this.tableLayoutPanel1.Size = new Size(364, 35);
      this.tableLayoutPanel1.TabIndex = 7;

      // Search
      this.BackColor = SystemColors.Info;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Name = "Search";
      this.Size = new Size(364, 35);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);
    }

    private void LblResultsTextChanged(object sender, EventArgs e)
    {
      m_LblResults.Left = 250 - m_LblResults.Width;
      m_SearchTextBoxText.Width = m_LblResults.Left - 76;
    }

    private void Next_Click(object sender, EventArgs e)
    {
      if (m_CurrentResult < m_Results)
        CurrentResult++;
    }

    private void Previous_Click(object sender, EventArgs e)
    {
      if (m_CurrentResult > 1)
        CurrentResult--;
    }

    private void SearchChanged() => OnSearchChanged?.Invoke(this, new SearchEventArgs(m_SearchTextBoxText.Text));

    private void SearchText_TextChanged(object sender, EventArgs e) =>
      FilterValueChangedElapsed(); // m_TimerChange.Stop();//m_TimerChange.Start();

    private void UpdateDisplay()
    {
      if (m_CurrentResult > 0)
      {
        m_SearchTextBoxText.ForeColor = SystemColors.WindowText;
        m_LblResults.Text = $"{m_CurrentResult} of {m_Results}";
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