using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvTools
{

  public static class _MessageBox
#pragma warning restore CA1707 // Identifiers should not contain underscores
  {
    public static DialogResult Show(Form owner, string message, string title,
          MessageBoxButtons buttons = MessageBoxButtons.OKCancel,
          MessageBoxIcon icon = MessageBoxIcon.None,
          MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1,
          double timeout = 4.0)
    {
      using (var tm = new TimedMessage())
      {
        return tm.Show(owner, message, title, buttons, icon, defaultButton, timeout, null);
      }
    }

    public static DialogResult Show(Form owner, string message, string title,
         MessageBoxButtons buttons,
         MessageBoxIcon icon,
         MessageBoxDefaultButton defaultButton,
         double timeout,
         string button3Text)
    {
      using (var tm = new TimedMessage())
      {
        return tm.Show(owner, message, title, buttons, icon, defaultButton, timeout, button3Text);
      }
    }

    public static DialogResult ShowBig(Form owner, string message, string title,
         MessageBoxButtons buttons = MessageBoxButtons.OKCancel,
         MessageBoxIcon icon = MessageBoxIcon.None,
         MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1,
         double timeout = 4.0)
    {
      using (var tm = new TimedMessage())
      {
        tm.Size = new Size(600, 450);
        return tm.Show(owner, message, title, buttons, icon, defaultButton, timeout, null);
      }
    }
  }

}
