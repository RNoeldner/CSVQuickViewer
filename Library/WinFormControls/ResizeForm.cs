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
/// Base form that supports DPI-awareness, recursive font scaling, and optional global font changes.
/// </summary>
public class ResizeForm : Form
{
  private IFontConfig m_FontConfig = CsvTools.FontConfig.Default;

  /// <summary>
  /// The font configuration shared across multiple forms.
  /// Adjusting FontSize here will propagate automatically to other forms using the same instance.
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
      FontSettingChanged(value, new PropertyChangedEventArgs(nameof(IFontConfig.Font)));
    }
  }

  public ResizeForm()
  {
    InitializeComponent();
  }

  /// <summary>
  /// Handles DPI changes.
  /// </summary>
  private void OnDpiChanged(object? sender, DpiChangedEventArgs e)
  {
    SuspendLayout();
    PerformLayout();
    ResumeLayout(true);
  }

  /// <summary>
  /// Responds to changes in the FontConfig (font name or size)
  /// </summary>
  private void FontSettingChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (sender is IFontConfig conf &&
        (e.PropertyName == nameof(IFontConfig.Font) || e.PropertyName == nameof(IFontConfig.FontSize)))
    {
      var newFont = new Font(conf.Font, conf.FontSize);
      this.SafeInvoke(() =>
      {
        SuspendLayout();
        try
        {
          SetFonts(this, newFont);
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
  /// <summary>
  /// Optional: Mouse wheel zoom
  /// Ctrl+MouseWheel adjusts the zoom factor for the font
  /// </summary>
  protected override void OnMouseWheel(MouseEventArgs e)
  {
    if ((ModifierKeys & Keys.Control) != 0)
    {
      const float step = 0.5f;
      float delta = e.Delta > 0 ? step : -step;
      float newSize = FontConfig.FontSize + delta;
      FontConfig.FontSize = Math.Max(6f, Math.Min(32f, newSize));
      return;
    }

    base.OnMouseWheel(e);
  }

  public void SetFont(Font newFont) => SetFonts(this, newFont);


  /// <summary>
  /// Recursively set the font for this form and all child controls
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
}