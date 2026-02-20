/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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

using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace CsvTools;

/// <inheritdoc cref="System.Windows.Forms.TextBox" />
/// <inheritdoc cref="Microsoft.Extensions.Logging.ILogger" />
/// <summary>
/// Displays log messages in real time and keeps track of the last displayed message
/// to optionally merge related output.
/// </summary>
public class LoggerDisplay : TextBox, ILogger
{
  // Stores the last printed message to detect incremental updates.
  private string m_LastMessage = string.Empty;

  public LoggerDisplay()
  {
    // Make the control suitable for multi-line log output.
#pragma warning disable MA0056 // Do not call overridable members in constructor
    Multiline = true;
    AllowDrop = false;
#pragma warning restore MA0056 // Do not call overridable members in constructor
  }

  /// <summary>
  /// Minimum log level to be displayed.
  /// </summary>
  [DefaultValue(LogLevel.Information)]
  public LogLevel MinLevel { get; set; } = LogLevel.Debug;

  /// <summary>
  /// Appends or merges log history entries into the TextBox.
  /// Must be called on UI Thread
  /// </summary>
  public void AddHistory(string text)
  {
    if (string.IsNullOrWhiteSpace(text))
      return;
    try
    {
      // Detect continuation marker (en dash first, hyphen fallback)
      int pos = text.IndexOf('–');
      if (pos < 0)
        pos = text.IndexOf('-');
      if (pos > 1 && m_LastMessage.StartsWith(text.Substring(0, pos - 1).Trim(), StringComparison.Ordinal))
      {
        AppendText(text.Substring(pos - 1).TrimStart());
        return;
      }
      // New log entry
      if (Text.Length>0)
        AppendText(Environment.NewLine);
      AppendText($"{DateTime.Now:HH:mm:ss} {text.TrimStart()}");
      m_LastMessage = text;
    }
    catch
    {
      // Fail silently – ensures UI logging never breaks application flow
    }
  }

#pragma warning disable CS8633
  public IDisposable BeginScope<TState>(TState state) => default!;
#pragma warning restore CS8633

  public bool IsEnabled(LogLevel logLevel) => logLevel >= MinLevel;

  /// <summary>
  /// Writes a log entry to the TextBox in a thread-safe manner.
  /// </summary>
  [DebuggerStepThrough]
  public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
    Func<TState, Exception?, string> formatter)
  {
    if (!IsEnabled(logLevel))
      return;

    var text = formatter(state, exception);

    // Avoid duplicate lines
    if (string.IsNullOrWhiteSpace(text) || m_LastMessage.Equals(text, StringComparison.Ordinal))
      return;

    // Ensure UI safety
    this.SafeInvoke(() => AddHistory(text));
  }

  public new void Clear() => this.SafeInvoke(() => Text = string.Empty);
}