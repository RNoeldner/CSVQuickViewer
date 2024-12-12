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

#nullable enable

using FastColoredTextBoxNS;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;

namespace CsvTools
{
  /// <inheritdoc cref="Microsoft.Extensions.Logging.ILogger" />
  /// <summary>
  ///   Only the most recently created Logger Display will get the log messages
  /// </summary>
  public class LoggerDisplay : FastColoredTextBox, ILogger
  {
#pragma warning disable CA1416
    private readonly TextStyle m_ErrorStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);
    private readonly TextStyle m_InfoStyle = new TextStyle(Brushes.Gray, null, FontStyle.Regular);
    private readonly TextStyle m_RegStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);
    private readonly TextStyle m_TimestampStyle = new TextStyle(Brushes.DimGray, Brushes.Lavender, FontStyle.Regular);
    private readonly TextStyle m_WarningStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
#pragma warning restore CA1416
    // private DateTime m_LastDisplay = DateTime.UtcNow;
    private bool m_Initial = true;

    private string m_LastMessage = string.Empty;

    public LoggerDisplay()
    {
      Multiline = true;
      ReadOnly = true;
      ShowLineNumbers = false;
      AllowDrop = false;
      WinAppLogging.AddLog(this);
    }

    /// <inheritdoc />
    public sealed override bool AllowDrop
    {
      get { return base.AllowDrop; }
      set { base.AllowDrop = value; }
    }

    [DefaultValue(LogLevel.Information)]
    public LogLevel MinLevel
    {
      get;
      set;
    } = LogLevel.Debug;

    [DefaultValue(120)]
    public int LimitLength
    {
      get;
      set;
    } = 120;

#pragma warning disable CS8633
    public IDisposable BeginScope<TState>(TState state) => default!;
#pragma warning restore CS8633

    public bool IsEnabled(LogLevel logLevel) => logLevel >= MinLevel;

    [DebuggerStepThrough]
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
      Func<TState, Exception?, string> formatter)
    {
      if (!IsEnabled(logLevel))
        return;
      var text = formatter(state, exception);

      if (string.IsNullOrWhiteSpace(text) || m_LastMessage.Equals(text, StringComparison.Ordinal))
        return;
      try
      {
        var appended = false;
        var posSlash = text.IndexOf('–', 0);
        if (posSlash != -1 && m_LastMessage.StartsWith(
              text.Substring(0, posSlash - 1).Trim(),
              StringComparison.Ordinal))
        {
          // add to previous item,
          AppendText(text.Substring(posSlash - 1), false, logLevel);
          appended = true;
        }

        m_LastMessage = text;
        if (!appended)
        {
          if (logLevel < LogLevel.Warning)
            text = StringUtils.GetShortDisplay(text.HandleCrlfCombinations(" "), LimitLength);
          if (!string.IsNullOrEmpty(text) && text != "\"\"")
            AppendText(text, true, logLevel);
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
      if (disposing)
      {
        WinAppLogging.RemoveLog(this);
      }

      try
      {
        this.SafeBeginInvoke(() => base.Dispose(disposing));
      }
      catch (Exception)
      {
        // ignore, sometimes a cross thread exception is thrown
      }
    }

    public void AppendText(string text, bool timestamp, LogLevel level)
    {
      this.SafeInvoke(() =>
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

          var style = m_RegStyle;
          if (level < LogLevel.Information)
            style = m_InfoStyle;
          else if (level >= LogLevel.Error)
            style = m_ErrorStyle;
          else if (level >= LogLevel.Warning)
            style = m_WarningStyle;

          if (timestamp)
          {
            AppendText(m_Initial ? $"{DateTime.Now:HH:mm:ss}" : $"\n{DateTime.Now:HH:mm:ss}", m_TimestampStyle);
            AppendText(" ");
          }

          AppendText(text, style);

          //restore user selection
          if (!userSelection.IsEmpty || userSelection.Start.iLine < LinesCount - 2)
          {
            Selection.Start = userSelection.Start;
            Selection.End = userSelection.End;
          }
          else
          {
            Selection.Start = new Place(0, Lines.Count - 1);
            DoCaretVisible();
          }

          Selection.EndUpdate();
          EndUpdate();
        }
        catch
        {
          // ignore
        }
      });

      //if ((DateTime.UtcNow -m_LastDisplay).TotalMilliseconds>500)
      //{
      //  Extensions.ProcessUIElements();
      //  m_LastDisplay = DateTime.UtcNow;
      //}
    }

  }
}