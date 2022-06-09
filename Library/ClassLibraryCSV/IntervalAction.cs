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

using System;

namespace CsvTools
{
  /// <summary>
  ///   Class to notify only after a given period of time
  /// </summary>
  public class IntervalAction
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

#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull("processDisplay")]
#endif

    public static IntervalAction? ForProcessDisplay(IProcessDisplay? processDisplay) => (processDisplay is null) ? null : new IntervalAction();

    /// <summary>
    ///   Initializes a new instance of the <see cref="IntervalAction" /> class.
    /// </summary>
    /// <param name="notifyAfterSeconds">Notify only after this time in seconds</param>
    public IntervalAction(double notifyAfterSeconds) => NotifyAfterSeconds = notifyAfterSeconds;

    public double NotifyAfterSeconds { get; set; }

    /// <summary>
    ///   Invoke the given action if the set interval has passed
    /// </summary>
    /// <param name="action">the action to invoke</param>
    public void Invoke(in Action action)
    {
      if (action is null || (DateTime.Now - m_LastNotification).TotalSeconds < NotifyAfterSeconds)
        return;
      m_LastNotification = DateTime.Now;
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
    ///   Invoke ProcessDisplay on given intervall
    /// </summary>
    /// <param name="processDisplay">The process display</param>
    /// <param name="text">The text to display.</param>
    /// <param name="value">The current progress value</param>
    /// <param name="log"><c>True</c> if progress should be logged, <c>false</c> otherwise.</param>
    public void Invoke(IProcessDisplay processDisplay, string text, long value, bool log) => Invoke(() => processDisplay.SetProcess(text, value, log));
  }
}