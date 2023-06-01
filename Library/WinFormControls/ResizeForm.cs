#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools
{

  public class ResizeForm : Form
  {
    private IFontConfig? m_FontConfig;

    public ResizeForm() : this(null)
    {
    }

    [Browsable(false)]
    [Bindable(false)]
    public IFontConfig? FontConfig
    {
      get => m_FontConfig;
      set
      {
        if (m_FontConfig != null)
          m_FontConfig.PropertyChanged -= FontSettingChanged;
        m_FontConfig = value;
        if (m_FontConfig != null)
        {
          m_FontConfig.PropertyChanged += FontSettingChanged;
          FontSettingChanged(value, new PropertyChangedEventArgs(nameof(IFontConfig.Font)));
        }
      }
    }

    protected ResizeForm(in IFontConfig? fontConfig)
    {
      InitializeComponent();
      FontConfig = fontConfig;
    }

    private void FontSettingChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (sender is IFontConfig conf && e.PropertyName is nameof(IFontConfig.Font) or nameof(IFontConfig.FontSize))
      {
        try
        {
          SuspendLayout();
          SetFonts(this, new Font(conf.Font, conf.FontSize));
        }
        catch
        {
          // ignore
        }
        finally
        {
          ResumeLayout();
          Refresh();
        }
      }
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
#pragma warning disable CS8600
    private void InitializeComponent()
    {
      ComponentResourceManager resources = new ComponentResourceManager(typeof(ResizeForm));
      this.SuspendLayout();
      // 
      // ResizeForm
      // 
      this.ClientSize = new System.Drawing.Size(514, 350);
      this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
      this.Name = "ResizeForm";
      this.ResumeLayout(false);
    }
#pragma warning restore CS8600
  }
}