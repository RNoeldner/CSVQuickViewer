/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */
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
    [Category("Action")] 
    public event EventHandler? ValueChanged;

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
      try
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
      catch (Exception e)
      {
        try { Logger.Warning(e, "SelectFont ctor"); } catch { };
      }
    }

    private void ComboBoxFont_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (m_UiChange)
        ValueChanged?.SafeInvoke(this);
    }

    private void ComboBoxSize_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (m_UiChange)
        ValueChanged?.SafeInvoke(this);
    }

    private void ButtonDefault_Click(object sender, EventArgs e)
    {
#pragma warning disable CA1416
      FontName = SystemFonts.DefaultFont.FontFamily.Name;
      FontSize = SystemFonts.DefaultFont.Size;
#pragma warning restore CA1416
      ValueChanged?.SafeInvoke(this);
    }
  }
}
