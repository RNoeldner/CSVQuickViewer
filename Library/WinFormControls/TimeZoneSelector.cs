using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CsvTools
{
  public partial class TimeZoneSelector : UserControl
  {
    public string TimeZoneID
    {
      set => comboBoxTimeZoneID.SelectedValue = value;
      get => (string)comboBoxTimeZoneID.SelectedValue;
    }

    public TimeZoneSelector() => InitializeComponent();

    private void TimeZoneSelector_Load(object sender, EventArgs e)
    {
      comboBoxTimeZoneID.ValueMember = "ID";
      comboBoxTimeZoneID.DisplayMember = "Display";

      var display = new List<DisplayItem<string>>
      {
        new DisplayItem<string>(TimeZoneMapping.cIdLocal, $"{TimeZoneInfo.Local.DisplayName} *[Local System]")
      };

      foreach (var wintz in TimeZoneMapping.MappedSystemTimeZone())
        display.Add(new DisplayItem<string>(wintz.Value, wintz.Key.DisplayName));

      comboBoxTimeZoneID.DataSource = display;
    }

    private void ButtonLocalTZ_Click(object sender, EventArgs e) => TimeZoneID = TimeZoneMapping.cIdLocal;

    private void ComboBoxTimeZoneID_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (sender == null)
        return;

      if (!(sender is Control ctrl))
        return;
      var bind = ctrl.GetTextBindng();
      if (bind == null)
        return;
      bind.WriteValue();
      ctrl.Focus();
    }
  }
}