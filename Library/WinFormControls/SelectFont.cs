using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace CsvTools
{
  public partial class SelectFont : UserControl
  {
    [Category("Action")] public event EventHandler? ValueChanged;
    
    private bool m_UiChange = true;

    [Browsable(true)]
    [Bindable(true)]
    [Category("Data")]
    [EditorBrowsable(EditorBrowsableState.Always)]
    [DefaultValue(10)]
    public float FontSize
    {
      get
      {
        if (comboBoxSize.SelectedItem is DisplayItem<float> diCurrent)
          return diCurrent.ID;
        return 10;
      }
      set
      {
        if (comboBoxSize.SelectedItem is DisplayItem<float> diCurrent)
          if (Math.Abs(value - diCurrent.ID) < .1)
            return;
        m_UiChange = false;

        comboBoxSize.SelectedItem = comboBoxSize.Items.OfType<DisplayItem<float>>().OrderBy(x => Math.Abs(x.ID - value))
          .First();

        m_UiChange = true;
      }
    }

    [Browsable(true)]
    [Bindable(true)]
    [Category("Data")]
    [EditorBrowsable(EditorBrowsableState.Always)]
    [DefaultValue("Tahoma")]
    public string FontName
    {
      get
      {
        return comboBoxFont.Text;
      }
      set
      {
#pragma warning disable CA1416
        var newValue = string.IsNullOrEmpty(value) ? SystemFonts.DefaultFont.FontFamily.Name : value;
#pragma warning restore CA1416
        if (comboBoxFont.Text == newValue)
          return;
        m_UiChange = false;
        comboBoxFont.Text = newValue;
        m_UiChange = true;
      }
    }


    public SelectFont()
    {
      InitializeComponent();
#pragma warning disable CA1416
      this.toolTip.SetToolTip(this.buttonDefault,
        $"Use system default font ({SystemFonts.DefaultFont.FontFamily.Name} - {SystemFonts.DefaultFont.Size}");


      comboBoxFont.BeginUpdate();
      using var col = new InstalledFontCollection();
      foreach (FontFamily fa in col.Families)
        if (fa.IsStyleAvailable(FontStyle.Regular))
          comboBoxFont.Items.Add(fa.Name);
      comboBoxFont.EndUpdate();

      comboBoxSize.BeginUpdate();
      using Graphics g = CreateGraphics();
      for (int pixel = 8; pixel < 24; pixel++)
        comboBoxSize.Items.Add(new DisplayItem<float>(pixel * 72 / g.DpiX,
          $"{pixel,2} Pixel - {pixel * 72 / g.DpiX} Points"));

      comboBoxSize.ValueMember = "ID";
      comboBoxSize.DisplayMember = "Display";
      comboBoxSize.EndUpdate();
#pragma warning restore CA1416
    }

    private void ComboBoxFont_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (m_UiChange)
        ValueChanged?.Invoke(this, e);
    }

    private void ComboBoxSize_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (m_UiChange)
        ValueChanged?.Invoke(this, e);
    }

    private void ButtonDefault_Click(object sender, EventArgs e)
    {
#pragma warning disable CA1416
      FontName = SystemFonts.DefaultFont.FontFamily.Name;
      FontSize = SystemFonts.DefaultFont.Size;
#pragma warning restore CA1416
      ValueChanged?.Invoke(this, e);
    }
  }
}