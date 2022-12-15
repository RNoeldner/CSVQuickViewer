#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools
{
  public class ResizeUserControl : UserControl
  {
    private IFontConfig? m_FontConfig;

    [Browsable(false)]
    [Bindable(false)]
    public IFontConfig FontConfig
    {
      get => m_FontConfig;
      set
      {
        if (m_FontConfig != null)
          m_FontConfig.PropertyChanged -= FontSettingChanged;
        m_FontConfig = value;
#pragma warning disable CA1416
        ChangeFont(new Font(m_FontConfig.Font, m_FontConfig.FontSize));
#pragma warning restore CA1416
        m_FontConfig.PropertyChanged += FontSettingChanged;
      }
    }

    private void FontSettingChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (m_FontConfig != null && e.PropertyName is nameof(IFontConfig.Font) or nameof(IFontConfig.FontSize))
#pragma warning disable CA1416
        ChangeFont(new Font(m_FontConfig.Font, m_FontConfig.FontSize));
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

    public void SetFont(Font newFont) =>
      SetFonts(this, newFont);

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
  }

  public class ResizeForm : Form
  {
    private IFontConfig? m_FontConfig;

    public ResizeForm() : this(null)
    {
    }

    [Browsable(false)]
    [Bindable(false)]
    public IFontConfig FontConfig
    {
      get => m_FontConfig;
      set
      {
        if (m_FontConfig != null)
          m_FontConfig.PropertyChanged -= FontSettingChanged;
        m_FontConfig = value;
#pragma warning disable CA1416
        ChangeFont(new Font(m_FontConfig.Font, m_FontConfig.FontSize));
#pragma warning restore CA1416
        m_FontConfig.PropertyChanged += FontSettingChanged;
      }
    }

    public ResizeForm(in IFontConfig? fontConfig)
    {
      m_FontConfig = fontConfig;
      SuspendLayout();
      ClientSize = new Size(400, 300);
      Icon = new ComponentResourceManager(typeof(ResizeForm)).GetObject("$this.Icon") as Icon;
      if (m_FontConfig != null)
#pragma warning disable CA1416
        SetFonts(this, new Font(m_FontConfig.Font, m_FontConfig.FontSize));
#pragma warning restore CA1416
      ResumeLayout(false);

      if (m_FontConfig != null)
        m_FontConfig.PropertyChanged += FontSettingChanged;
    }

    private void FontSettingChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (m_FontConfig != null && e.PropertyName is nameof(IFontConfig.Font) or nameof(IFontConfig.FontSize))
#pragma warning disable CA1416
        ChangeFont(new Font(m_FontConfig.Font, m_FontConfig.FontSize));
#pragma warning restore CA1416
    }

    public void ChangeFont(Font newFont)
    {
      this.SafeInvokeNoHandleNeeded(() =>
      {
        SuspendLayout();
        SetFonts(this, newFont);
        ResumeLayout();
        Refresh();
      });
    }

    public void SetFont(Font newFont) =>
      SetFonts(this, newFont);

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
  }
}