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

using FastColoredTextBoxNS;
using System;
using System.Drawing;

namespace CsvTools
{
  /// <summary>
  ///   Only the most recently created Logger Display will get the log messages
  /// </summary>
  public class LoggerDisplay : FastColoredTextBox
  {
    private readonly TextStyle timestampStyle = new TextStyle(Brushes.Black, Brushes.LightSteelBlue, FontStyle.Regular);
    private readonly TextStyle infoStyle = new TextStyle(Brushes.Gray, null, FontStyle.Regular);
    private readonly TextStyle regStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);
    private readonly TextStyle warningStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
    private readonly TextStyle errorStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);

    private readonly Action<string, Logger.Level> m_PreviousLog = Logger.AddLog;
    private bool m_Disposed;

    private bool m_Initial = true;

    private string m_LastMessage = string.Empty;

    public LoggerDisplay()
    {
      // ReSharper disable once VirtualMemberCallInConstructor
      Multiline = true;
      ReadOnly = true;
      ShowLineNumbers = false;
      Logger.AddLog = AddLog;
    }

    public Logger.Level MinLevel { get; set; } = Logger.Level.Debug;

    public void AddLog(string text, Logger.Level level)
    {
      if (string.IsNullOrWhiteSpace(text) || m_LastMessage.Equals(text, StringComparison.Ordinal)
                                          || level < MinLevel) return;
      try
      {
        var appended = false;
        var posSlash = text.IndexOf('–', 0);
        if (posSlash != -1 && m_LastMessage.StartsWith(
          text.Substring(0, posSlash - 1).Trim(),
          StringComparison.Ordinal))
        {
          // add to previous item,
          AppendText(text.Substring(posSlash - 1), false, level);
          appended = true;
        }

        m_LastMessage = text;
        if (!appended)
        {
          if (level < Logger.Level.Warn)
            text = StringUtils.GetShortDisplay(StringUtils.HandleCRLFCombinations(text, " "), 120);
          if (!string.IsNullOrEmpty(text) && text != "\"\"")
            AppendText(text, true, level);
        }

        m_Initial = false;
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

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (m_Disposed) return;
      if (disposing)
      {
        m_Disposed = true;
        Logger.AddLog = m_PreviousLog;
      }

      try
      {
        base.Dispose(disposing);
      }
      catch (Exception)
      {
        // ignore, sometimes a cross thread exception is thrown
      }
    }

    private void AppendText(string text, bool timestamp, Logger.Level level)
    {
      this.SafeBeginInvoke(() =>
      {
        try
        {
          //some stuffs for best performance
          BeginUpdate();
          Selection.BeginUpdate();
          //remember user selection
          var userSelection = Selection.Clone();
          //add text with predefined style
          TextSource.CurrentTB = this;

          var style = regStyle;
          if (level < Logger.Level.Info)
            style = infoStyle;
          else if (level >= Logger.Level.Error)
            style = errorStyle;
          else if (level >= Logger.Level.Warn)
            style = warningStyle;

          if (timestamp)
          {
            if (m_Initial)
              AppendText($"{DateTime.Now:HH:mm:ss}  ", timestampStyle);
            else
              AppendText($"\n{DateTime.Now:HH:mm:ss}  ", timestampStyle);
          }
          AppendText(text, style);

          //restore user selection
          if (!userSelection.IsEmpty || userSelection.Start.iLine < LinesCount - 2)
          {
            Selection.Start = userSelection.Start;
            Selection.End = userSelection.End;
          }
          else
            GoEnd();//scroll to end of the text

          Selection.EndUpdate();
          EndUpdate();
        }
        catch
        {
          // ignore
        }
      });
    }
  }
}