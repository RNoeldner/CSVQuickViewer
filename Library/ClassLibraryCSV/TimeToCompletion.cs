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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace CsvTools
{
  /// <summary>
  ///   Calculates the "Estimated Time of Completion" based on a rolling average of progress over time.
  ///   This is not thread safe, each thread should have its own TimeToCompletion.
  /// </summary>
  public class TimeToCompletion
  {
    public static readonly TimeSpan Max = TimeSpan.FromDays(2);
    private readonly long m_MaximumTicks;
    private readonly byte m_MinimumData;

    private readonly Queue<ProgressOverTime> m_Queue;
    private readonly Stopwatch m_Stopwatch;
    private ProgressOverTime m_FirstItem;
    private ProgressOverTime m_LastItem;
    private long m_TargetValue;

    /// <summary>
    ///   Initializes a new instance of the <see cref="TimeToCompletion" /> class.
    /// </summary>
    /// <param name="targetValue">The target value / maximum that would match 100%</param>
    /// <param name="minimumData">The number of entries to keep no matter how old the entry is.</param>
    /// <param name="storedSeconds">The number of seconds to remember for estimation purpose.</param>
    public TimeToCompletion(long targetValue = -1, byte minimumData = 10, double storedSeconds = 10.0)
    {
      m_MinimumData = minimumData;
      m_MaximumTicks = (long) (storedSeconds * Stopwatch.Frequency);
      m_Queue = new Queue<ProgressOverTime>();
      m_Stopwatch = new Stopwatch();      
      m_LastItem.Value = 0;
      TargetValue = targetValue;
    }

    /// <summary>
    ///   Gets the estimated time remaining to complete
    /// </summary>
    /// <value>The estimated time remaining.</value>
    public TimeSpan EstimatedTimeRemaining { get; private set; } = Max;

    public string EstimatedTimeRemainingDisplay => DisplayTimespan(EstimatedTimeRemaining);

    /// <summary>
    ///   Gets the estimated time remaining display with a leading separator (if needed).
    /// </summary>
    /// <value>The estimated time remaining display separator.</value>
    public string EstimatedTimeRemainingDisplaySeparator
    {
      get
      {
        if (EstimatedTimeRemaining >= Max)
          return string.Empty;
        return " - " + EstimatedTimeRemainingDisplay;
      }
    }

    /// <summary>
    ///   Gets the current percentage
    /// </summary>
    /// <value>Percent (usually between 0 and 1)</value>
    public double Percent { get; private set; }

    public string PercentDisplay =>
      string.Format(CultureInfo.CurrentCulture, Percent < 10 ? "{0:F1}%" : "{0:F0}%", Percent);

    /// <summary>
    ///   Gets or sets the target value / maximum that would match 100%.
    /// </summary>
    /// <value>The target value.</value>
    public long TargetValue
    {
      get => m_TargetValue;
      set
      {
        var newVal = value > 1 ? value : 1;
        m_TargetValue = newVal;
        m_Queue.Clear();
        EstimatedTimeRemaining = Max;
      }
    }

    /// <summary>
    ///   Gets or sets the current value in the process
    /// </summary>
    /// <value>The value.</value>
    public long Value
    {
      set
      {
        if (value < 0 || m_LastItem.Value == value || value > m_TargetValue)
          return;

        // Something strange happening we are going backwards, assume we are counting from the
        // beginning again (possibly reuse of the object)
        if (value < m_LastItem.Value)
          m_Queue.Clear();
        if (m_Queue.Count == 0)
          m_Stopwatch.Restart();
        // Remove old items
        var expired = m_Stopwatch.ElapsedTicks - m_MaximumTicks;
        while (m_Queue.Count > m_MinimumData && m_Queue.Peek().Tick < expired)
          m_FirstItem = m_Queue.Dequeue();

        // Queue this item
        m_LastItem.Tick = m_Stopwatch.ElapsedTicks;
        m_LastItem.Value = value;
        m_Queue.Enqueue(m_LastItem);
        if (m_Queue.Count == 1)
          m_FirstItem = m_LastItem;

        Percent = Math.Round((double) value / m_TargetValue * 100.0, 1, MidpointRounding.AwayFromZero);

        // Make sure we have enough items to estimate
        if (m_Queue.Count < 2 || m_FirstItem.Value == m_LastItem.Value)
        {
          EstimatedTimeRemaining = Max;
        }
        else
        {
          var finishedInTicks = (m_TargetValue - m_LastItem.Value) * (double) (m_LastItem.Tick - m_FirstItem.Tick)
                                / (m_LastItem.Value - m_FirstItem.Value);
          // Calculate the estimated finished time
          EstimatedTimeRemaining = finishedInTicks / Stopwatch.Frequency > .9
            ? TimeSpan.FromSeconds(finishedInTicks / Stopwatch.Frequency)
            : Max;
        }
      }
      get => m_LastItem.Value;
    }

    /// <summary>
    ///   Displays the timespan in a human readable format
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="cut2Sec">If true any value shorter than 2 seconds will be empty</param>
    /// <returns></returns>
    public static string DisplayTimespan(TimeSpan value, bool cut2Sec = true)
    {
      if (value == Max || (cut2Sec && value.TotalSeconds < 2) || value.TotalDays > 2)
        return string.Empty;
      if (value.TotalSeconds < 2 && !cut2Sec)
        return $"{value:s\\.ff} sec";
      if (value.TotalMinutes < 1)
        return $"{value:%s} sec";
      if (value.TotalHours < 1)
        return $"{value:m\\:ss} min";
      if (value.TotalHours < 24)
        return $"{value:h\\:mm} hrs";
      return $"{value.TotalDays:F1} days";
    }

    private struct ProgressOverTime
    {
      public long Tick;
      public long Value;
    }
  }
}