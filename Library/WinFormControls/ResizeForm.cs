using JetBrains.Annotations;
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
      try
      {
        var resources = new ComponentResourceManager(typeof(ResizeForm));
        Icon = (Icon) resources.GetObject("$this.Icon");

#if !NETCOREAPP3_1
        // 6.2 and 6.3 is Windows 8 / Windows Server 2012
        if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor > 1)
          SetFonts(this, SystemFonts.DialogFont);
#endif

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

    private void InitializeComponent()
    {
      this.SuspendLayout();
      // 
      // ResizeForm
      // 
      this.ClientSize = new System.Drawing.Size(282, 253);
      this.Name = "ResizeForm";
      this.ResumeLayout(false);
    }
  }
}