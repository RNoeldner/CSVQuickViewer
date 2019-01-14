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
  ///  Class to notify only after a given period of time
  /// </summary>
  public class IntervalAction
  {
    private double m_NotifyAfterSeconds;
    private DateTime m_LastNotification = DateTime.MinValue;

    /// <summary>
    ///  Initializes a new instance of the <see cref="IntervalAction" /> class.
    /// </summary>
    /// <remarks>If no notification period is set 1/5 a second is assumed</remarks>
    public IntervalAction()
    {
      m_NotifyAfterSeconds = 0.2;
    }

    /// <summary>
    ///  Initializes a new instance of the <see cref="IntervalAction" /> class.
    /// </summary>
    /// <param name="notifyAfterSeconds">Notify only after this time in seconds</param>
    public IntervalAction(double notifyAfterSeconds)
    {
      m_NotifyAfterSeconds = notifyAfterSeconds;
    }

    public double NotifyAfterSeconds { get => m_NotifyAfterSeconds; set => m_NotifyAfterSeconds = value; }


    /// <summary>
    ///  Invoke the given action if the set interval has passed
    /// </summary>
    /// <param name="action">the action to invoke</param>
    public void Invoke(Action action)
    {
      if (!((DateTime.Now - m_LastNotification).TotalSeconds > m_NotifyAfterSeconds)) return;
      m_LastNotification = DateTime.Now;
      action?.Invoke();
    }

    public void Invoke(Action<int> action, int value)
    {
      if (!((DateTime.Now - m_LastNotification).TotalSeconds > m_NotifyAfterSeconds)) return;
      m_LastNotification = DateTime.Now;
      action?.Invoke(value);
    }

    public void Invoke(Action<string, int> action, string text, int value)
    {
      if (!((DateTime.Now - m_LastNotification).TotalSeconds > m_NotifyAfterSeconds)) return;
      m_LastNotification = DateTime.Now;
      action?.Invoke(text, value);
    }
  }
}