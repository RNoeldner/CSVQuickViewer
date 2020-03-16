using System;
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools
{
  public class ResizeForm : Form
  {
    private static readonly Font m_Font = SystemFonts.DialogFont;

    // new Font(SystemFonts.IconTitleFont.FontFamily, SystemFonts.IconTitleFont.Size - 2, SystemFonts.IconTitleFont.Style);

    /// <summary>
    ///   Recursively change the font of all controls, needed on Windows 8 / 2012
    /// </summary>
    /// <param name="container">A container control like a form or panel</param>
    private void SetFonts(Control container)
    {
      if (!Equals(container.Font, SystemFonts.IconTitleFont))
      {
        Logger.Debug($"Changed font from {container.Font} to {m_Font} for Container : {container}");
        container.Font = SystemFonts.IconTitleFont;
      }

      foreach (Control ctrl in base.Controls)
      {
        if (ctrl is ContainerControl cc)
          SetFonts(cc);
        else
        {
          if (Equals(ctrl.Font, SystemFonts.IconTitleFont)) continue;
          Logger.Debug($"Changed Font from {ctrl.Font} to {SystemFonts.IconTitleFont} for Control : {ctrl}");
          ctrl.Font = SystemFonts.IconTitleFont;
        }
      }
    }

    protected ResizeForm()
    {
      //#if !NETCOREAPP3_1
      // 6.2 and 6.3 is Windows 8 / Windows Server 2012
      if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor > 1)
        SetFonts(this);
      //#endif
    }
  }
}