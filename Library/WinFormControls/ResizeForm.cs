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
      //ParentChanged += delegate(object? sender, EventArgs args)
      //{
      //  if (Parent != null)
      //    ChangeFont(Parent.Font);
      //};
      // SetFonts(this, SystemFonts.DefaultFont);
      ResumeLayout(false);
    }


    public void ChangeFont(Font newFont)
    {
      this.SafeInvoke(() =>
      {
        SuspendLayout();
        SetFonts(this, newFont);
        ResumeLayout();
        Refresh();
      });
    }

    /// <summary>
    ///   Recursively change the font of all controls, needed on Windows 8 / 2012
    /// </summary>
    /// <param name="container">A container control like a form or panel</param>
    /// <param name="newFont">The font with size to use</param>
    public static void SetFonts(in Control container, in Font newFont)
    {
      if (!Equals(container.Font, newFont))
        container.Font = newFont;

      foreach (Control ctrl in container.Controls)
      {
        // data grid special handling
        if (ctrl is DataGridView dgv)
          dgv.DefaultCellStyle.Font = newFont;
        else
          SetFonts(ctrl, newFont);
      }
    }
  }
}