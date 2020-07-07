using System;
using System.Diagnostics.CodeAnalysis;
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
    public void SetFonts(Control container)
    {
      if (!Equals(container.Font, m_Font))
      {
        Logger.Debug($"Changed font from {container.Font} to {m_Font} for Container : {container}");
        container.Font = m_Font;
      }

      foreach (Control ctrl in container.Controls)
      {
        if (ctrl is ContainerControl cc)
          SetFonts(cc);
        else
        {
          if (Equals(ctrl.Font, m_Font)) continue;
          Logger.Debug($"Changed Font from {ctrl.Font} to {SystemFonts.IconTitleFont} for Control : {ctrl}");
          ctrl.Font = m_Font;
        }
      }
    }

    public ResizeForm()
    {
      try
      {
#if !NETCOREAPP3_1
        // 6.2 and 6.3 is Windows 8 / Windows Server 2012
        if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor > 1)
          SetFonts(this);
#endif
        InitializeComponent();
      }
      catch (Exception)
      {
        //ignore
      }
    }

    [SuppressMessage("ReSharper", "ArrangeThisQualifier")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResizeForm));
      this.SuspendLayout();
      // ResizeForm
      this.ClientSize = new System.Drawing.Size(292, 253);
      this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
      this.ResumeLayout(false);
    }
  }
}