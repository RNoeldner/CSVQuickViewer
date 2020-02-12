using System;
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools
{
  public class ResizeForm : Form
  {
    private static readonly Font myfont = SystemFonts.IconTitleFont;
      // new Font(SystemFonts.IconTitleFont.FontFamily, SystemFonts.IconTitleFont.Size - 2, SystemFonts.IconTitleFont.Style);

    /// <summary>
    /// Recursivly change the font of all controls, needed on Windows 8 / 2012
    /// </summary>
    /// <param name="container">A container control like a form or panel</param>
    private void SetFonts(ContainerControl container)
    {
      if (container.Font != SystemFonts.IconTitleFont)
      {
        Logger.Debug($"Changed font from {container.Font} to {myfont} for Container : {container}");
        container.Font = SystemFonts.IconTitleFont;
      }

      foreach (Control ctrl in base.Controls)
      {
        if (ctrl is ContainerControl cc)
          SetFonts(cc);
        else
        {
          if (ctrl.Font != SystemFonts.IconTitleFont)
          {
            Logger.Debug($"Changed Font from {ctrl.Font} to {SystemFonts.IconTitleFont} for Control : {ctrl}");
            ctrl.Font = SystemFonts.IconTitleFont;
          }
        }
      }
    }

    public ResizeForm()
    {
      // 6.1 is Windows 7, 6.2 and 6.3 is Windows 8, Windows 10 is 10.x 
      if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor > 1)
        SetFonts(this);
    }
  }
}
