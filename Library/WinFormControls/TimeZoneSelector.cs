namespace CsvTools
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Linq;
  using System.Windows.Forms;

  public partial class TimeZoneSelector : UserControl
  {
    public TimeZoneSelector()
    {
      InitializeComponent();

      var display = new List<DisplayItem<string>>
                      {
                        new DisplayItem<string>(
                          TimeZoneInfo.Local.Id,
                          $"{TimeZoneInfo.Local.DisplayName} *[Local System]")
                      };
      display.AddRange(
        TimeZoneInfo.GetSystemTimeZones().Select(wintz => new DisplayItem<string>(wintz.Id, wintz.DisplayName)));

      comboBoxTimeZoneID.DataSource = display;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Bindable(true)]
    [Browsable(true)]
    public string TimeZoneID
    {
      get => (string) comboBoxTimeZoneID.SelectedValue;
      set => comboBoxTimeZoneID.SelectedValue = value;
    }

    private void ButtonLocalTZ_Click(object sender, EventArgs e) => TimeZoneID = TimeZoneInfo.Local.Id;

    private void ComboBoxTimeZoneID_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (sender == null)
        return;

      if (!(sender is Control ctrl))
        return;
      var bind = ctrl.GetTextBinding();
      if (bind == null)
        return;
      bind.WriteValue();
      ctrl.Focus();
    }

    private void TimeZoneSelector_Load(object sender, EventArgs e) => comboBoxTimeZoneID.SelectedValue = TimeZoneInfo.Local.Id;
  }
}