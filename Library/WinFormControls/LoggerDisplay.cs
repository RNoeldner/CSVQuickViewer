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
  using System;
  using System.Drawing;
  using System.Windows.Forms;

  /// <summary>
  ///   Only the most recently created Logger Display will get the log messages
  /// </summary>
  public class LoggerDisplay : RichTextBox
  {
    private readonly Action<string, Logger.Level> m_PreviousLog = Logger.AddLog;

    private bool m_Initial = true;

    private string m_LastMessage = string.Empty;

    public LoggerDisplay()
    {
      // ReSharper disable once VirtualMemberCallInConstructor
      Multiline = true;
      KeyUp += FindForm().CtrlA;
      Logger.AddLog = AddLog;
    }

    public Logger.Level MinLevel { get; set; } = Logger.Level.Debug;

    public void AddLog(string text, Logger.Level level)
    {
      if (string.IsNullOrWhiteSpace(text) || m_LastMessage.Equals(text, StringComparison.Ordinal)
                                          || level < this.MinLevel) return;
      try
      {
        var appended = false;
        var posSlash = text.IndexOf('–', 0);
        if (posSlash != -1 && this.m_LastMessage.StartsWith(
              text.Substring(0, posSlash - 1).Trim(),
              StringComparison.Ordinal))
        {
          // add to previous item,
          this.AppendText(text.Substring(posSlash - 1), level);
          appended = true;
        }

        this.m_LastMessage = text;
        if (!appended)
        {
          if (level < Logger.Level.Warn)
            text = StringUtils.GetShortDisplay(StringUtils.HandleCRLFCombinations(text, " "), 120);
          this.AppendText($"{(this.m_Initial ? string.Empty : "\n")}{DateTime.Now:HH:mm:ss}  {text}", level);
        }

        this.m_Initial = false;
      }
      catch (Exception)
      {
        // ignore
      }
    }

    public new void Clear()
    {
      this.SafeBeginInvoke(() => { Text = string.Empty; });
      Extensions.ProcessUIElements();
    }

    protected override void Dispose(bool disposing)
    {
      try
      {
        Logger.AddLog = m_PreviousLog;
        base.Dispose(disposing);
      }
      catch (Exception e)
      {
        // ignore
      }
    }

    private void AppendText(string text, Logger.Level level)
    {
      if (string.IsNullOrEmpty(text))
        return;

      this.SafeBeginInvoke(
        () =>
          {
            try
            {
              var col = ForeColor;
              if (level < Logger.Level.Info)
                col = Color.Gray;
              if (level >= Logger.Level.Warn)
                col = Color.Blue;
              if (level >= Logger.Level.Error)
                col = Color.Red;

              SelectionStart = TextLength;
              if (col != ForeColor)
              {
                SelectionLength = 0;
                SelectionColor = col;
              }
              AppendText(text);
              Select(TextLength, 0);
              ScrollToCaret();

              if (col != ForeColor)
                SelectionColor = ForeColor;
            }
            catch
            {
              // ignore
            }
          });
      // Extensions.ProcessUIElements();
    }
  }
}