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
      {
        if (ctrl is DataGridView dgv)
          dgv.DefaultCellStyle.Font = font;
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
}