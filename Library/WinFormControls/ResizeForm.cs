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
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools;

/// <summary>
/// Base form providing bounded, DPI-aware font scaling.
///
/// Responsibilities:
/// - Enforce a logical font size range of 9–20 points
/// - Apply font changes consistently to all child controls
/// - Support Ctrl+MouseWheel font zoom in discrete point steps
/// - Handle runtime DPI changes correctly (per-monitor DPI awareness)
/// </summary>
public class ResizeForm : Form
{
  /// <summary>
  /// Currently applied font instance.
  /// Stored to ensure proper disposal when font settings change.
  /// </summary>
  private Font? m_CurrentFont;

  /// <summary>
  /// Current logical font size in points (clamped to the allowed range).
  /// This value represents the zoom level independent of DPI.
  /// </summary>  
  private int m_CurrentFontSize = 10; // or from FontConfig
  private const int MinFontSize = 9;
  private const int MaxFontSize = 20;

  /// <summary>
  /// Shared font configuration instance.
  /// Changes to this instance propagate automatically to all forms using it.
  /// </summary>
  private IFontConfig m_FontConfig = CsvTools.FontConfig.Default;

  public ResizeForm()
  {
    InitializeComponent();
  }

  /// <summary>
  /// Assigning a new instance will immediately synchronize
  /// the form's zoom index and applied font.
  /// </summary>
  [Browsable(false)]
  [Bindable(false)]
  public IFontConfig FontConfig
  {
    get => m_FontConfig;
    set
    {
      m_FontConfig.PropertyChanged -= FontSettingChanged;
      m_FontConfig = value;
      m_FontConfig.PropertyChanged += FontSettingChanged;
      m_CurrentFontSize = FindClosestFontSize(m_FontConfig.FontSize);
      FontSettingChanged(value, new PropertyChangedEventArgs(nameof(IFontConfig.Font)));
    }
  }

  /// <summary>
  /// Applies a font to this form and all child controls.
  /// Provided for compatibility with existing code paths.
  /// </summary>
  public void SetFont(Font newFont) => SetFonts(this, newFont);

  /// <summary>
  /// Handles Ctrl+MouseWheel font zoom.
  /// Adjusts the logical font size in discrete point steps
  /// and updates the shared FontConfig.
  /// </summary>
  protected override void OnMouseWheel(MouseEventArgs e)
  {
    if ((ModifierKeys & Keys.Control) != 0)
    {
      var newSize = m_CurrentFontSize + (e.Delta > 0 ? 1 : -1);
      if (newSize<MinFontSize)
        newSize=MinFontSize;
      if (newSize > MaxFontSize)
        newSize=MaxFontSize;
      if (m_CurrentFontSize!= newSize)
      {
        using Graphics g = CreateGraphics();
        m_CurrentFontSize= newSize;
        FontConfig.FontSize = (m_CurrentFontSize * 72f) / g.DpiX;
      }
    }
    base.OnMouseWheel(e);
  }

  /// <summary>
  /// Recursively applies a font to the specified control hierarchy.
  /// Controls tagged with "NoFontChange" are excluded.
  /// </summary>
  private static void SetFonts(in Control control, in Font newFont)
  {
    if ("NoFontChange".Equals(control.Tag?.ToString()))
      return;
    if (!Equals(control.Font, newFont))
      control.Font = newFont;

    foreach (Control ctrl in control.Controls)
      SetFonts(ctrl, newFont);
  }

  /// <summary>
  /// Finds the closest logical font size (in points) within the allowed range
  /// that matches the specified font size at the current DPI.
  /// </summary>
  private int FindClosestFontSize(float value)
  {
    using Graphics g = CreateGraphics();
    int bestSize = 0;
    float bestDiff = Math.Abs((MinFontSize * 72f) / g.DpiX - value);

    for (int frontSize = MinFontSize; frontSize <= MaxFontSize; frontSize++)
    {
      float diff = Math.Abs((frontSize * 72f) / g.DpiX - value);
      if (diff < bestDiff)
      {
        bestDiff = diff;
        bestSize = frontSize;
      }
    }

    return bestSize;
  }

  /// <summary>
  /// Reacts to font or font-size changes in the shared FontConfig.
  /// Applies the new font recursively and disposes the previous font instance.
  /// </summary>
  private void FontSettingChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (sender is IFontConfig conf &&
        (e.PropertyName == nameof(IFontConfig.Font) || e.PropertyName == nameof(IFontConfig.FontSize)))
    {
      var newFont = new Font(conf.Font, conf.FontSize);
      m_CurrentFont?.Dispose();
      m_CurrentFont = newFont;

      this.SafeInvoke(() =>
      {
        SuspendLayout();
        try
        {
          SetFonts(this, m_CurrentFont);
        }
        catch
        {
          // ignore
        }
        finally
        {
          ResumeLayout(true);
          Refresh();
        }
      });
    }
  }

  private void InitializeComponent()
  {
    var resources = new ComponentResourceManager(typeof(ResizeForm));
    SuspendLayout();
    // 
    // ResizeForm
    // 
    AutoScaleDimensions = new SizeF(96F, 96F);
    AutoScaleMode = AutoScaleMode.Dpi;
    AutoSize = true;
    ClientSize = new Size(514, 350);
    Icon = (Icon) resources.GetObject("$this.Icon");
    Name = "ResizeForm";
    DpiChanged += OnDpiChanged;
    ResumeLayout(false);
  }

  /// <summary>
  /// Triggers layout recalculation after a DPI change.
  /// Font size remains logically unchanged and is re-rendered by WinForms.
  /// </summary>
  private void OnDpiChanged(object? sender, DpiChangedEventArgs e)
  {
    SuspendLayout();
    PerformLayout();
    ResumeLayout(true);
  }
}