using Microsoft.Win32;
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools
{
  public class ResizeForm : Form
  {
    private void SetFonts()
    {
      Font = SystemFonts.IconTitleFont;
      foreach (var ctrl in base.Controls)
      {
        if (ctrl is Control tc)
          tc.Font = SystemFonts.IconTitleFont;
      }
    }

    public ResizeForm()
    {
      Font = SystemFonts.IconTitleFont;
      SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);
      FormClosing += ResizeForm_FormClosing;
    }

    private void ResizeForm_FormClosing(object sender, FormClosingEventArgs e) => SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);

    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
      if (e.Category == UserPreferenceCategory.Window)
        SetFonts();
    }
  }
}
