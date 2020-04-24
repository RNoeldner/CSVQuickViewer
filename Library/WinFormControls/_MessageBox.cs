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

#pragma warning disable IDE1006 // Naming Styles

  public static class _MessageBox
#pragma warning restore IDE1006 // Naming Styles
  {
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

    /// <summary>
    ///   MessageBox for handling Choice for a number of dialogs, one button will be reserved to
    ///   answer for all option, if this is selected, it will be stored and returned in consecutive calls
    /// </summary>
    /// <param name="owner">The calling from, can be null</param>
    /// <param name="message">Dialog Message</param>
    /// <param name="title">Dialog Message</param>
    /// <param name="massChoice">
    ///   A class to maintain information if a default is chosen and how many dialogs might be presented
    /// </param>
    /// <param name="button1Text">By Default "Yes"</param>
    /// <param name="button2Text">By Default "No"</param>
    /// <returns>DialogResult.Yes or DialogResult.No</returns>
    public static DialogResult PersistentChoice(
      Form owner,
      string message,
      string title,
      PersistentChoice massChoice,
      string button1Text = "Yes",
      string button2Text = "No")
    {
      if (massChoice.Chosen)
        return massChoice.DialogResult;

      using (var tm = new TimedMessage())
      {
        var result = tm.Show(
          owner, message, title,
          // add a third button in case we expect followup dialogs
          massChoice.NumRecs > 1 ? MessageBoxButtons.YesNoCancel : MessageBoxButtons.YesNo,
          MessageBoxIcon.Question,
          // Depending on the massChoice Result, select the right button
          massChoice.DialogResult == DialogResult.Yes ? MessageBoxDefaultButton.Button1 : MessageBoxDefaultButton.Button2,
          4.0,
          // do not overwrite Button 1 or Button 2
          button1Text, button2Text,
          // but set Button 3 if needed
          massChoice.NumRecs > 1 ? $"{massChoice.DialogResult == DialogResult.Yes: button1Text : button2Text} To All ({massChoice.NumRecs})" : null);

        // Button3 results in Cancel and is the Mass choice
        if (result == DialogResult.Cancel)
        {
          massChoice.Chosen = true;
          return massChoice.DialogResult;
        }
        return result;
      }
    }

    public static DialogResult ShowBigRtf(
      Form owner,
      string messageRtf,
      string title,
      MessageBoxButtons buttons = MessageBoxButtons.OKCancel,
      MessageBoxIcon icon = MessageBoxIcon.None,
      MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1,
      double timeout = 4.0)
    {
      using (var tm = new TimedMessage())
      {
        tm.MessageRtf = messageRtf;
        tm.Size = new Size(600, 450);
        return tm.Show(owner, null, title, buttons, icon, defaultButton, timeout, null, null, null);
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