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
    /// <remarks>If no notification period is set 1/3 a second is assumed</remarks>
    public IntervalAction()
      : this(1 / 3d)
    {
    }

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
      if ((DateTime.Now - m_LastNotification).TotalSeconds < NotifyAfterSeconds)
        return;
      m_LastNotification = DateTime.Now;
      action.Invoke();
    }

    /// <summary>
    ///   Invoke the given action if the set interval has passed
    /// </summary>
    public void Invoke(in Action<long> action, long value)
    {
      if ((DateTime.Now - m_LastNotification).TotalSeconds < NotifyAfterSeconds)
        return;
      m_LastNotification = DateTime.Now;
      try
      {
        action.Invoke(value);
      }
      catch (ObjectDisposedException)
      {
        // ignore
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, nameof(IntervalAction));
      }
    }

    /// <summary>
    ///   Invoke the given action if the set interval has passed
    /// </summary>
    public void Invoke(in Action<long, string> action, long value, string value2)
    {
      if ((DateTime.Now - m_LastNotification).TotalSeconds < NotifyAfterSeconds)
        return;
      m_LastNotification = DateTime.Now;

      try
      {
        action.Invoke(value, value2);
      }
      catch (ObjectDisposedException)
      {
        // ignore
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, nameof(IntervalAction));
      }
    }

    /// <summary>
    ///   Invoke the given action if the set interval has passed
    /// </summary>
    public void Invoke(in Action<long, long> action, long value1, long value2)
    {
      if ((DateTime.Now - m_LastNotification).TotalSeconds < NotifyAfterSeconds)
        return;
      m_LastNotification = DateTime.Now;
      try
      {
        action.Invoke(value1, value2);
      }
      catch (ObjectDisposedException)
      {
        // ignore
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, nameof(IntervalAction));
      }
    }

    /// <summary>
    ///   Invoke the given action if the set interval has passed
    /// </summary>
    public void Invoke(in Action<long, long, long> action, long value1, long value2, long value3)
    {
      if ((DateTime.Now - m_LastNotification).TotalSeconds < NotifyAfterSeconds)
        return;
      m_LastNotification = DateTime.Now;
      try
      {
        action.Invoke(value1, value2, value3);
      }
      catch (ObjectDisposedException)
      {
        // ignore
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, nameof(IntervalAction));
      }
    }

    /// <summary>
    ///   Invoke the given action if the set interval has passed
    /// </summary>
    public void Invoke(in Action<string, long, bool> action, in string text, long value, bool log)
    {
      if ((DateTime.Now - m_LastNotification).TotalSeconds < NotifyAfterSeconds)
        return;
      m_LastNotification = DateTime.Now;
      action.Invoke(text, value, log);
    }

    public void Invoke(in IProcessDisplay? processDisplay, in string text, long value, bool log)
    {
      if (processDisplay is null)
        return;
      if ((DateTime.Now - m_LastNotification).TotalSeconds < NotifyAfterSeconds)
        return;
      m_LastNotification = DateTime.Now;
      processDisplay.SetProcess(text, value, log);
    }
  }
}