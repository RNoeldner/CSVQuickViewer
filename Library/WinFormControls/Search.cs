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
  private IContainer components;
  private Button m_BtnCancel = new Button();
  private Button m_BtnNext = new Button();
  private Button m_BtnPrevious = new Button();
  private string m_SearchText = string.Empty;
  private TextBox m_SearchTextBoxText;
  private Timer m_TimerChange;
  private ToolTip toolTip;

  /// <summary>
  ///   Initializes a new instance of the <see cref="Search" /> class.
  /// </summary>
  public Search()
  {
    InitializeComponent();
  }

  /// <summary>
  ///   Occurs when the search should be cleared.
  /// </summary>
  public event EventHandler? OnSearchClear;

  /// <summary>
  ///   Occurs when the search text is changed.
  /// </summary>
  public event EventHandler? OnSearchNext;
  /// <summary>
  ///   Occurs when the Search backwards
  /// </summary>
  public event EventHandler? OnSearchPrev;

  public string SearchText
  {
    get => m_SearchText;
    set
    {
      if (m_SearchText.Equals(value, StringComparison.OrdinalIgnoreCase))
        return;
      m_SearchText = value;
      // If empty, you might want to trigger Clear instead of Search
      if (string.IsNullOrEmpty(m_SearchText))
      {
        OnSearchClear?.SafeInvoke(this);
        return;
      }
      OnSearchNext?.Invoke(this, EventArgs.Empty);
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

  private void BtnCancelClick(object? sender, EventArgs e)
  {
    Hide();
    OnSearchClear?.SafeInvoke(this);
  }

  private void BtnNextClick(object? sender, EventArgs e)
        => OnSearchNext?.Invoke(this, e);

  private void BtnPreviousClick(object? sender, EventArgs e)
      => OnSearchPrev?.Invoke(this, e);

  /// <summary>
  ///   Required method for Designer support - do not modify the contents of this method with the
  ///   code editor.
  /// </summary>
  private void InitializeComponent()
  {
    components = new Container();
    Label label1;
    var resources = new ComponentResourceManager(typeof(Search));
    m_BtnCancel = new Button();
    m_SearchTextBoxText = new TextBox();
    m_BtnNext = new Button();
    m_BtnPrevious = new Button();
    m_TimerChange = new Timer(components);
    toolTip = new ToolTip(components);
    label1 = new Label();
    SuspendLayout();
    // 
    // label1
    // 
    label1.AutoSize = true;
    label1.Location = new Point(3, 6);
    label1.Name = "label1";
    label1.Size = new Size(30, 13);
    label1.TabIndex = 0;
    label1.Text = "Find:";
    // 
    // m_BtnCancel
    // 
    m_BtnCancel.AccessibleName = "Close";
    m_BtnCancel.DialogResult = DialogResult.Cancel;
    m_BtnCancel.FlatAppearance.BorderColor = SystemColors.ButtonFace;
    m_BtnCancel.FlatStyle = FlatStyle.Flat;
    m_BtnCancel.Image = (Image) resources.GetObject("m_BtnCancel.Image");
    m_BtnCancel.Location = new Point(182, 3);
    m_BtnCancel.Name = "m_BtnCancel";
    m_BtnCancel.Size = new Size(19, 19);
    m_BtnCancel.TabIndex = 4;
    toolTip.SetToolTip(m_BtnCancel, "Close");
    m_BtnCancel.UseVisualStyleBackColor = false;
    m_BtnCancel.Click += BtnCancelClick;
    // 
    // m_SearchTextBoxText
    // 
    m_SearchTextBoxText.AccessibleName = "Search";
    m_SearchTextBoxText.BorderStyle = BorderStyle.FixedSingle;
    m_SearchTextBoxText.Location = new Point(39, 2);
    m_SearchTextBoxText.MaxLength = 50;
    m_SearchTextBoxText.Name = "m_SearchTextBoxText";
    m_SearchTextBoxText.Size = new Size(102, 20);
    m_SearchTextBoxText.TabIndex = 1;
    m_SearchTextBoxText.TextChanged += SearchText_TextChanged;
    // 
    // m_BtnNext
    // 
    m_BtnNext.AccessibleName = "Previous";
    m_BtnNext.BackgroundImageLayout = ImageLayout.Center;
    m_BtnNext.FlatAppearance.BorderColor = SystemColors.ButtonFace;
    m_BtnNext.Image = (Image) resources.GetObject("m_BtnNext.Image");
    m_BtnNext.Location = new Point(163, 3);
    m_BtnNext.Name = "m_BtnNext";
    m_BtnNext.Size = new Size(18, 19);
    m_BtnNext.TabIndex = 6;
    m_BtnNext.Text = "&N";
    toolTip.SetToolTip(m_BtnNext, "Find Next");
    m_BtnNext.UseVisualStyleBackColor = false;
    m_BtnNext.Click += BtnNextClick;
    // 
    // m_BtnPrevious
    // 
    m_BtnPrevious.AccessibleName = "Next";
    m_BtnPrevious.BackgroundImageLayout = ImageLayout.Center;
    m_BtnPrevious.FlatAppearance.BorderColor = SystemColors.ButtonFace;
    m_BtnPrevious.Image = (Image) resources.GetObject("m_BtnPrevious.Image");
    m_BtnPrevious.Location = new Point(144, 3);
    m_BtnPrevious.Name = "m_BtnPrevious";
    m_BtnPrevious.Size = new Size(19, 19);
    m_BtnPrevious.TabIndex = 5;
    toolTip.SetToolTip(m_BtnPrevious, "Find Previous");
    m_BtnPrevious.UseVisualStyleBackColor = false;
    m_BtnPrevious.Click += BtnPreviousClick;
    // 
    // m_TimerChange
    // 
    m_TimerChange.Enabled = true;
    m_TimerChange.Interval = 200;
    m_TimerChange.Tick += TimerChange_Tick;
    // 
    // Search
    // 
    Controls.Add(m_BtnCancel);
    Controls.Add(label1);
    Controls.Add(m_BtnNext);
    Controls.Add(m_BtnPrevious);
    Controls.Add(m_SearchTextBoxText);
    Name = "Search";
    Size = new Size(204, 24);
    ResumeLayout(false);
    PerformLayout();
  }

  private void SearchText_TextChanged(object? sender, EventArgs e)
  {
    m_TimerChange.Stop();
    m_TimerChange.Start();
  }

  private void TimerChange_Tick(object? sender, EventArgs e)
  {
    m_TimerChange.Enabled = false;
    try
    {
      SearchText = m_SearchTextBoxText.Text;
    }
    catch (Exception ex)
    {
      Extensions.ShowError(ex, "Error during search");
    }
    finally
    {
      m_TimerChange.Enabled = true;
    }
  }
}