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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools;

public class ResizeForm : Form
{
  private IFontConfig m_FontConfig;

  [Browsable(false)]
  [Bindable(false)]
  public IFontConfig FontConfig
  {
    get => m_FontConfig;
    set
    {
      if (m_FontConfig != null)
        m_FontConfig.PropertyChanged -= FontSettingChanged;

      if (m_FontConfig != null)
      {
        m_FontConfig = value;
        m_FontConfig.PropertyChanged += FontSettingChanged;
        FontSettingChanged(value, new PropertyChangedEventArgs(nameof(IFontConfig.Font)));
      }
    }
  }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
  public ResizeForm()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
  {
    InitializeComponent();
    FontConfig = new FontConfig();
  }

  private void OnDpiChanged(object? sender, DpiChangedEventArgs e)
  {
    SuspendLayout();
    PerformLayout();
    ResumeLayout(true);
  }

  private void FontSettingChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (sender is IFontConfig conf &&
        (e.PropertyName == nameof(IFontConfig.Font) || e.PropertyName == nameof(IFontConfig.FontSize)))
    {
      this.SafeInvoke(() =>
      {
        try
        {
          SuspendLayout();
          SetFonts(this, new Font(conf.Font, conf.FontSize));
        }
        catch
        {
          // ignore
        }
        finally
        {
          ResumeLayout();
          Refresh();
        }
      });
    }
  }

  public void SetFont(Font newFont) => SetFonts(this, newFont);

  /// <summary>
  ///   Recursively change the font of all controls, needed on Windows 8 / 2012
  /// </summary>
  /// <param name="container">A container control like a form or panel</param>
  /// <param name="newFont">The font with size to use</param>
  private static void SetFonts(in Control container, in Font newFont)
  {
    if (!Equals(container.Font, newFont))
      container.Font = newFont;

    foreach (Control ctrl in container.Controls)
      SetFonts(ctrl, newFont);
  }

#pragma warning disable CS8600
  private void InitializeComponent()
  {
    var resources = new ComponentResourceManager(typeof(ResizeForm));
    SuspendLayout();
    // 
    // ResizeForm
    // 
    AutoScaleDimensions = new SizeF(96F, 96F);
    AutoScaleMode = AutoScaleMode.Dpi;
    ClientSize = new Size(514, 350);
    DpiChanged += OnDpiChanged;
    Icon = (Icon) resources.GetObject("$this.Icon");
    Name = "ResizeForm";
    ResumeLayout(false);
  }
#pragma warning restore CS8600
}