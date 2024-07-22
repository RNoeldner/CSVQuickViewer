#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools
{
  public class ResizeForm : Form
  {
    private IFontConfig m_FontConfig;

    [Browsable(false)]
    [Bindable(false)]
    public IFontConfig FontConfig
    {
      get => m_FontConfig;
      set
      {
        if (m_FontConfig != null)
          m_FontConfig.PropertyChanged -= FontSettingChanged;

        if (m_FontConfig != null)
        {
          m_FontConfig = value;
          m_FontConfig.PropertyChanged += FontSettingChanged;
          FontSettingChanged(value, new PropertyChangedEventArgs(nameof(IFontConfig.Font)));
        }
      }
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public ResizeForm()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
      InitializeComponent();
      FontConfig = new FontConfig();
    }

    private void FontSettingChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (sender is IFontConfig conf &&
          (e.PropertyName == nameof(IFontConfig.Font) || e.PropertyName == nameof(IFontConfig.FontSize)))
      {
        this.SafeInvoke(() =>
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
        });
      }
    }

    public void SetFont(Font newFont) => SetFonts(this, newFont);

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