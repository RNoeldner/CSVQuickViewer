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
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   A Control to show a search and raised appropriate events
  /// </summary>
  public class Search : UserControl
  {
    //private read only System.Timers.Timer m_TimerChange = new System.Timers.Timer();
    private Button m_BtnCancel;

    private Button m_BtnNext;
    private Button m_BtnPrevious;

    /// <summary>
    ///   Required designer variable.
    /// </summary>
    private readonly IContainer components = null;

    private Label m_LblResults;

    private int m_CurrentResult = -1;

    private int m_Results;

    private TextBox m_SearchTextBoxText;

    /// <summary>
    ///   Initializes a new instance of the <see cref="Search" /> class.
    /// </summary>
    public Search()
    {
      InitializeComponent();
      //m_TimerChange.Elapsed += FilterValueChangedElapsed;
      //m_TimerChange.Interval = 10;
      //m_TimerChange.AutoReset = false;
      Results = 0;
    }

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
    ///   Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      //if (m_TimerChange != null)
      //{
      //  m_TimerChange.Stop();
      //  m_TimerChange.Dispose();
      //}
      if (disposing)
        components?.Dispose();

      base.Dispose(disposing);
    }

    private void Cancel_Click(object sender, EventArgs e)
    {
      //m_TimerChange.Stop();
      OnSearchClear?.Invoke(this, null);
      Hide();
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

    /// <summary>
    ///   The delay for the filter change has elapsed.
    /// </summary>
    private void FilterValueChangedElapsed() => m_SearchTextBoxText.SafeInvoke(SearchChanged);

    private void LblResultsTextChanged(object sender, EventArgs e)
    {
      m_LblResults.Left = 250 - m_LblResults.Width;
      m_SearchTextBoxText.Width = m_LblResults.Left - 76;
    }

    private void SearchChanged() => OnSearchChanged?.Invoke(this, new SearchEventArgs(m_SearchTextBoxText.Text));

    private void SearchText_TextChanged(object sender, EventArgs e) => FilterValueChangedElapsed();//m_TimerChange.Stop();//m_TimerChange.Start();

    private void UpdateDisplay()
    {
      if (m_CurrentResult > 0)
      {
        m_SearchTextBoxText.ForeColor = SystemColors.WindowText;
        m_LblResults.Text = $"{m_CurrentResult} of {m_Results}";
      }
      else
      {
        m_SearchTextBoxText.ForeColor = string.IsNullOrEmpty(m_SearchTextBoxText.Text) ? SystemColors.WindowText : Color.Red;
        m_LblResults.Text = string.Empty;
      }

      m_BtnPrevious.Enabled = m_CurrentResult > 1;
      m_BtnNext.Enabled = m_Results > m_CurrentResult;
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///   Required method for Designer support - do not modify the contents of this method with the
    ///   code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.Windows.Forms.Label label1;
      m_BtnCancel = new System.Windows.Forms.Button();
      m_SearchTextBoxText = new System.Windows.Forms.TextBox();
      m_LblResults = new System.Windows.Forms.Label();
      m_BtnNext = new System.Windows.Forms.Button();
      m_BtnPrevious = new System.Windows.Forms.Button();
      label1 = new System.Windows.Forms.Label();
      SuspendLayout();
      //
      // label1
      //
      label1.AutoSize = true;
      label1.ForeColor = System.Drawing.SystemColors.InfoText;
      label1.Location = new System.Drawing.Point(3, 6);
      label1.Name = "label1";
      label1.Size = new System.Drawing.Size(82, 20);
      label1.TabIndex = 0;
      label1.Text = "Find what:";
      //
      // m_BtnCancel
      //
      m_BtnCancel.BackColor = System.Drawing.SystemColors.Info;
      m_BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      m_BtnCancel.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonFace;
      m_BtnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      m_BtnCancel.Image = global::CsvToolLib.Resources.Close;
      m_BtnCancel.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
      m_BtnCancel.Location = new System.Drawing.Point(296, 2);
      m_BtnCancel.Name = "m_BtnCancel";
      m_BtnCancel.Size = new System.Drawing.Size(24, 24);
      m_BtnCancel.TabIndex = 4;
      m_BtnCancel.UseVisualStyleBackColor = false;
      m_BtnCancel.Click += new System.EventHandler(Cancel_Click);
      //
      // m_SearchTextBoxText
      //
      m_SearchTextBoxText.BackColor = System.Drawing.SystemColors.Info;
      m_SearchTextBoxText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      m_SearchTextBoxText.ForeColor = System.Drawing.SystemColors.InfoText;
      m_SearchTextBoxText.Location = new System.Drawing.Point(84, 3);
      m_SearchTextBoxText.MaxLength = 50;
      m_SearchTextBoxText.Name = "m_SearchTextBoxText";
      m_SearchTextBoxText.Size = new System.Drawing.Size(102, 26);
      m_SearchTextBoxText.TabIndex = 1;
      m_SearchTextBoxText.TextChanged += new System.EventHandler(SearchText_TextChanged);
      //
      // m_LblResults
      //
      m_LblResults.AutoSize = true;
      m_LblResults.ForeColor = System.Drawing.SystemColors.InfoText;
      m_LblResults.Location = new System.Drawing.Point(195, 7);
      m_LblResults.Name = "m_LblResults";
      m_LblResults.Size = new System.Drawing.Size(49, 20);
      m_LblResults.TabIndex = 0;
      m_LblResults.Text = "0 of 0";
      m_LblResults.TextChanged += new System.EventHandler(LblResultsTextChanged);
      //
      // m_BtnNext
      //
      m_BtnNext.BackColor = System.Drawing.SystemColors.Info;
      m_BtnNext.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      m_BtnNext.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonFace;
      m_BtnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      m_BtnNext.Image = global::CsvToolLib.Resources.Down;
      m_BtnNext.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
      m_BtnNext.Location = new System.Drawing.Point(273, 2);
      m_BtnNext.Name = "m_BtnNext";
      m_BtnNext.Size = new System.Drawing.Size(24, 24);
      m_BtnNext.TabIndex = 6;
      m_BtnNext.UseVisualStyleBackColor = false;
      m_BtnNext.Click += new System.EventHandler(Next_Click);
      //
      // m_BtnPrevious
      //
      m_BtnPrevious.BackColor = System.Drawing.SystemColors.Info;
      m_BtnPrevious.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      m_BtnPrevious.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonFace;
      m_BtnPrevious.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      m_BtnPrevious.Image = global::CsvToolLib.Resources.Up;
      m_BtnPrevious.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
      m_BtnPrevious.Location = new System.Drawing.Point(250, 2);
      m_BtnPrevious.Name = "m_BtnPrevious";
      m_BtnPrevious.Size = new System.Drawing.Size(24, 24);
      m_BtnPrevious.TabIndex = 5;
      m_BtnPrevious.UseVisualStyleBackColor = false;
      m_BtnPrevious.Click += new System.EventHandler(Previous_Click);
      //
      // Search
      //
      BackColor = System.Drawing.SystemColors.Info;
      Controls.Add(m_BtnNext);
      Controls.Add(m_BtnPrevious);
      Controls.Add(m_BtnCancel);
      Controls.Add(m_SearchTextBoxText);
      Controls.Add(m_LblResults);
      Controls.Add(label1);
      Name = "Search";
      Size = new System.Drawing.Size(325, 30);
      ResumeLayout(false);
      PerformLayout();
    }

    #endregion Windows Form Designer generated code
  }

  /// <summary>
  ///   Event Arguments for Finding a text in a DataGridView
  /// </summary>
  public class FoundEventArgs : EventArgs
  {
#pragma warning disable CA1051 // Do not declare visible instance fields

    /// <summary>
    ///   The cell
    /// </summary>
    public DataGridViewCell Cell;

    /// <summary>
    ///   The index
    /// </summary>
    public int Index;

#pragma warning restore CA1051 // Do not declare visible instance fields

    /// <summary>
    ///   Initializes a new instance of the <see cref="FoundEventArgs" /> class.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="cell">The cell.</param>
    public FoundEventArgs(int index, DataGridViewCell cell)
    {
      Index = index;
      Cell = cell;
    }
  }

  /// <summary>
  ///   Event Arguments for the Search
  /// </summary>
  public class SearchEventArgs : EventArgs
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="SearchEventArgs" /> class.
    /// </summary>
    /// <param name="searchText">Text to search</param>
    /// <param name="result">Number of the result to focus</param>
    public SearchEventArgs(string searchText, int result = 1)
    {
      SearchText = searchText;
      Result = result;
    }

    /// <summary>
    ///   Gets or sets the result to be shown
    /// </summary>
    /// <value>The result.</value>
    public int Result { get; }

    /// <summary>
    ///   Gets or sets the search text.
    /// </summary>
    /// <value>The search text.</value>
    public string SearchText { get; }
  }
}