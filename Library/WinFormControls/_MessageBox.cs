/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
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

namespace CsvTools
{
  using System.Drawing;
  using System.Windows.Forms;

  public static class _MessageBox
#pragma warning restore CA1707
  {
    // Identifiers should not contain underscores
    public static DialogResult Show(
      Form owner,
      string message,
      string title,
      MessageBoxButtons buttons = MessageBoxButtons.OKCancel,
      MessageBoxIcon icon = MessageBoxIcon.None,
      MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1,
      double timeout = 4.0)
    {
      using (var tm = new TimedMessage())
      {
        return tm.Show(owner, message, title, buttons, icon, defaultButton, timeout, null, null, null);
      }
    }

    public static DialogResult Show(
      Form owner,
      string message,
      string title,
      MessageBoxButtons buttons,
      MessageBoxIcon icon,
      MessageBoxDefaultButton defaultButton,
      double timeout,
      string button1Text,
      string button2Text,
      string button3Text)
    {
      using (var tm = new TimedMessage())
      {
        return tm.Show(
          owner,
          message,
          title,
          buttons,
          icon,
          defaultButton,
          timeout,
          button1Text,
          button2Text,
          button3Text);
      }
    }

    public static DialogResult ShowBig(
      Form owner,
      string message,
      string title,
      MessageBoxButtons buttons = MessageBoxButtons.OKCancel,
      MessageBoxIcon icon = MessageBoxIcon.None,
      MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1,
      double timeout = 4.0)
    {
      using (var tm = new TimedMessage())
      {
        tm.Size = new Size(600, 450);
        return tm.Show(owner, message, title, buttons, icon, defaultButton, timeout, null, null, null);
      }
    }
  }
}