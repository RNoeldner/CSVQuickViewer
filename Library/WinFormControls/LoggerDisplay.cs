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
using System.Drawing;

namespace CsvTools
{
  /// <inheritdoc cref="Microsoft.Extensions.Logging.ILogger" />
  /// <summary>
  ///   Only the most recently created Logger Display will get the log messages
  /// </summary>
  public class LoggerDisplay : FastColoredTextBox, ILogger
  {
    private readonly TextStyle errorStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);
    private readonly TextStyle infoStyle = new TextStyle(Brushes.Gray, null, FontStyle.Regular);
    private readonly TextStyle regStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);
    private readonly TextStyle timestampStyle = new TextStyle(Brushes.DimGray, Brushes.Lavender, FontStyle.Regular);
    private readonly TextStyle warningStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);

    private bool m_Disposed;

    private bool m_Initial = true;

    private string m_LastMessage = string.Empty;

    public LoggerDisplay()
    {
      Multiline = true;
      ReadOnly = true;
      ShowLineNumbers = false;
      WinAppLogging.AddLog(this);
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

    public IDisposable BeginScope<TState>(TState state) => default!;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= MinLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
      if (!IsEnabled(logLevel))
        return;      
      var text = (exception !=null)?formatter(state, exception):state?.ToString() ?? string.Empty;

      if (string.IsNullOrWhiteSpace(text) || m_LastMessage.Equals(text, StringComparison.Ordinal)) return;
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
            text = StringUtils.GetShortDisplay(text.HandleCRLFCombinations(" "), LimitLength);
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
      if (m_Disposed) return;
      if (disposing)
      {
        m_Disposed = true;
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

    private void AppendText(string text, bool timestamp, LogLevel level)
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
          if (level < LogLevel.Information)
            style = infoStyle;
          else if (level >= LogLevel.Error)
            style = errorStyle;
          else if (level >= LogLevel.Warning)
            style = warningStyle;

          if (timestamp)
          {
            AppendText(m_Initial ? $"{DateTime.Now:HH:mm:ss}" : $"\n{DateTime.Now:HH:mm:ss}", timestampStyle);
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
    }
  }
}