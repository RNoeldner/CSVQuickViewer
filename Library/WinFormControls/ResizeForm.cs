#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools
{
  public class ResizeForm : Form
  {
    private readonly IFontConfig? m_FontConfig;

    public ResizeForm() : this(null)
    {
    }

    public ResizeForm(in IFontConfig? fontConfig)
    {
      m_FontConfig = fontConfig;
      SuspendLayout();
      ClientSize = new Size(400, 300);
      Icon = new ComponentResourceManager(typeof(ResizeForm)).GetObject("$this.Icon") as Icon;
      ResumeLayout(false);

      if (m_FontConfig != null)
      {
        m_FontConfig.PropertyChanged += FontSettingChanged;
        Load += (sender, _) => FontSettingChanged(sender, new PropertyChangedEventArgs(nameof(IFontConfig.Font)));
      }
    }

    private void FontSettingChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(IFontConfig.Font) ||
          e.PropertyName == nameof(IFontConfig.FontSize))
#pragma warning disable CA1416
        ChangeFont(new Font(m_FontConfig!.Font, m_FontConfig!.FontSize));
#pragma warning restore CA1416
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
        SetFonts(ctrl, newFont);
    }
  }
}