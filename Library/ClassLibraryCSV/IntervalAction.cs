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

using System;

namespace CsvTools
{
  /// <summary>
  ///   Class to notify only after a given period of time
  /// </summary>
  public sealed class IntervalAction
  {
    private DateTime m_LastNotification = DateTime.MinValue;

    /// <inheritdoc />
    /// <summary>
    ///   Initializes a new instance of the <see cref="T:CsvTools.IntervalAction" /> class.
    /// </summary>
    /// <remarks>If no notification period is set 1/4 a second is assumed</remarks>
    public IntervalAction()
      : this(.25d)
    {
    }

    /// <summary>Gets an IntervalAction for a Progress report</summary>
    /// <param name="progress">The progress, in case its null, null is returned</param>
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull("progress")]
#endif        
    public static IntervalAction? ForProgress(IProgress<ProgressInfo>? progress) => progress is null ? null : new IntervalAction();

    /// <summary>
    ///   Initializes a new instance of the <see cref="IntervalAction" /> class.
    /// </summary>
    /// <param name="notifyAfterSeconds">Notify only after this time in seconds</param>
    public IntervalAction(double notifyAfterSeconds) => NotifyAfterSeconds = notifyAfterSeconds;

    /// <summary>
    /// Gets or sets the value after how many seconds the action should be invoked again
    /// </summary>
    /// <value>
    /// The notify after seconds.
    /// </value>
    public double NotifyAfterSeconds { get; set; }

    /// <summary>
    ///   Invoke the given action if the set interval has passed
    /// </summary>
    /// <param name="action">the action to invoke</param>
    public void Invoke(in Action action)
    {
      // do nothing if the timespan between invokes is not reached
      if ((DateTime.UtcNow - m_LastNotification).TotalSeconds < NotifyAfterSeconds)
        return;

      m_LastNotification = DateTime.UtcNow;
      try
      {
        action.Invoke();
      }
      catch (ObjectDisposedException)
      {
        // ignore
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "IntervalAction.Invoke(()=> {MethodInfo})", action.Method);
      }
    }

    /// <summary>
    ///   Invoke progress on given interval
    /// </summary>
    /// <param name="action">The action to be done</param>
    /// <param name="number">The number parameter</param>
    public void Invoke(in Action<long> action, long number)
    {
      // do nothing if the timespan between invokes is not reached
      if ((DateTime.UtcNow - m_LastNotification).TotalSeconds < NotifyAfterSeconds)
        return;

      m_LastNotification = DateTime.UtcNow;
      try
      {
        action.Invoke(number);
      }
      catch (ObjectDisposedException)
      {
        // ignore
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "IntervalAction.Invoke(()=> {MethodInfo})", action.Method);
      }
    }

    /// <summary>
    ///   Invoke progress on given interval
    /// </summary>
    public void Invoke(in Action<string> action, in string txt)
    {
      // do nothing if the timespan between invokes is not reached
      if ((DateTime.UtcNow - m_LastNotification).TotalSeconds < NotifyAfterSeconds)
        return;

      m_LastNotification = DateTime.UtcNow;
      try
      {
        action.Invoke(txt);
      }
      catch (ObjectDisposedException)
      {
        // ignore
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "IntervalAction.Invoke(()=> {MethodInfo})", action.Method);
      }
    }

    /// <summary>
    ///   Invoke progress on given interval
    /// </summary>
    /// <param name="progress">The process display</param>
    /// <param name="text">The text to display.</param>
    /// <param name="value">The current progress value</param>
    public void Invoke(IProgress<ProgressInfo> progress, string text, long value) => Invoke(() => progress.Report(new ProgressInfo(text, value)));
  }
}