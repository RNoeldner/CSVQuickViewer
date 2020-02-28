namespace CsvTools
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Windows.Forms;

  public partial class TimeZoneSelector : UserControl
  {
    public TimeZoneSelector() => InitializeComponent();

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Bindable(true)]
    [Browsable(true)]
    public string TimeZoneID
    {
      get => (string)comboBoxTimeZoneID.SelectedValue;
      set => comboBoxTimeZoneID.SelectedValue = value;
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

    private void TimeZoneSelector_Load(object sender, EventArgs e)
    {
      comboBoxTimeZoneID.ValueMember = "ID";
      comboBoxTimeZoneID.DisplayMember = "Display";

      var display = new List<DisplayItem<string>>
                      {
                        new DisplayItem<string>(
                          TimeZoneMapping.cIdLocal,
                          $"{TimeZoneInfo.Local.DisplayName} *[Local System]")
                      };

      foreach (var wintz in TimeZoneMapping.MappedSystemTimeZone())
        display.Add(new DisplayItem<string>(wintz.Value, wintz.Key.DisplayName));

      comboBoxTimeZoneID.DataSource = display;
    }
  }
}