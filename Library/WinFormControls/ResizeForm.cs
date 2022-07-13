#nullable enable

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools
{
  public class ResizeForm : Form
  {
    public ResizeForm()
    {
      SuspendLayout();
      ClientSize = new Size(400, 300);
      Icon = new ComponentResourceManager(typeof(ResizeForm)).GetObject("$this.Icon") as Icon;
      ResumeLayout(false);

      try
      {
        MouseWheel += FormMouseWheel;
        SetZoom();
      }
      catch (Exception)
      {
        //ignore
      }
    }

    private static float m_FontSize = SystemFonts.DefaultFont.Size;

    public static float FontSize
    {
      get => m_FontSize;
      set
      {
        if (value is > 2 and < 14)
          m_FontSize = value;
        else
          Console.Beep();
      }
    }

    private void SetZoom() => SetFonts(this, new Font(SystemFonts.DialogFont.FontFamily, m_FontSize, FontStyle.Regular));
    

    private void FormMouseWheel(object? sender, MouseEventArgs e)
    {
      if (ModifierKeys != Keys.Control)
        return;
      if (e.Delta > 0)
        FontSize += 0.5F;
      else if (e.Delta < 0)
        FontSize -= 0.5F;
      SetZoom();

    }

    /// <summary>
    ///   Recursively change the font of all controls, needed on Windows 8 / 2012
    /// </summary>
    /// <param name="container">A container control like a form or panel</param>
    /// <param name="font">The font with size to use</param>
    public static void SetFonts(Control container, Font font)
    {
      if (!Equals(container.Font, font)) 
        container.Font = font;

      foreach (Control ctrl in container.Controls)
        if (ctrl is ContainerControl cc)
          SetFonts(cc, font);
        else
        {
          if (Equals(ctrl.Font, font)) 
            continue;
          ctrl.Font = font;
        }
    }
  }
}