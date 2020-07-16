using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace CsvTools
{
  public class ResizeForm : Form
  {
    public ResizeForm()
    {
      try
      {
        var resources = new ComponentResourceManager(typeof(ResizeForm));
        Icon = (Icon) resources.GetObject("$this.Icon");
        SetFonts(this, SystemFonts.DialogFont);
        MouseWheel += FormMouseWheel;
      }
      catch (Exception)
      {
        //ignore
      }
    }

    private void FormMouseWheel(object sender, MouseEventArgs e)
    {
      if (e.Delta > 0)
        if (Font.Size < 11)
          SetFonts(this, new Font(Font.FontFamily, Font.Size + 1, Font.Style));
        else
          Console.Beep();

      if (e.Delta < 0)
        if (Font.Size > 4)
          SetFonts(this, new Font(Font.FontFamily, Font.Size - 1, Font.Style));
        else
          Console.Beep();
    }

    // new Font(SystemFonts.IconTitleFont.FontFamily, SystemFonts.IconTitleFont.Size - 2, SystemFonts.IconTitleFont.Style);

    /// <summary>
    ///   Recursively change the font of all controls, needed on Windows 8 / 2012
    /// </summary>
    /// <param name="container">A container control like a form or panel</param>
    /// <param name="font"></param>
    public static void SetFonts([NotNull] Control container, [NotNull] Font font)
    {
      if (!Equals(container.Font, font)) container.Font = font;

      foreach (Control ctrl in container.Controls)
        if (ctrl is ContainerControl cc)
        {
          SetFonts(cc, font);
        }
        else
        {
          if (Equals(ctrl.Font, font)) continue;
          ctrl.Font = font;
        }
    }
  }
}