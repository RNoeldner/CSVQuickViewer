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


namespace CsvTools
{
  /// <summary>
  ///   Calculates the "Estimated Time of Completion" based on a rolling average of progress over time.
  ///   This is not thread safe, each thread should have its own TimeToCompletion.
  ///   Based on velocity of last 5 seconds the remaining time is calculated, the Percentage is based on real values though
  /// </summary>
  public sealed class TimeToCompletion
  {
    public static readonly TimeSpan Max = TimeSpan.FromDays(2);
    private readonly TimeSpan m_MaximumAge;
    private readonly byte m_MinimumData;
    private readonly Queue<ValueOverTime> m_Queue;
    private ValueOverTime m_LastItem;
    private long m_TargetValue;

    /// <summary>
    ///   Initializes a new instance of the <see cref="TimeToCompletion" /> class.
    /// </summary>
    /// <param name="targetValue">The target value / maximum that would match 100%</param>
    /// <param name="minimumData">The number of entries to keep no matter how old the entry is.</param>
    /// <param name="storedSeconds">The number of seconds to remember for estimation purpose.</param>
    public TimeToCompletion(long targetValue = -1, byte minimumData = 10, double storedSeconds = 15.0)
    {
      m_MinimumData = minimumData;
      m_MaximumAge = TimeSpan.FromSeconds(storedSeconds);
      m_Queue = new Queue<ValueOverTime>();
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
    ///   Gets the current percentage based on the really reported values
    /// </summary>
    /// <value>Percent (usually between 0 and 1)</value>
    public double Percent { get; private set; }

    /// <summary>
    ///   Gets the estimated percentage assuming a steady progress
    /// </summary>
    /// <value>Percent (usually between 0 and 1)</value>
    public double EstimatedPercent { get; private set; }

    public string PercentDisplay => Percent < 10 ? $"{Percent:F1}%" : $"{Percent:F0}%";


    public string EstimatedPercentDisplay => EstimatedPercent < 10 ? $"{EstimatedPercent:F1}%" : $"{EstimatedPercent:F0}%";

    /// <summary>
    ///   Gets or sets the target value / maximum that would match 100%.
    /// </summary>
    /// <value>The target value.</value>
    public long TargetValue
    {
      get => m_TargetValue;
      set
      {
        m_TargetValue = value > 1 ? value : 1;
        UpdateEstimates();
      }
    }

    private void UpdateEstimates()
    {
      // Make sure we have at least to entries to estimate
      if (m_Queue.Count < 2)
      {
        EstimatedTimeRemaining = Max;
      }
      else
      {
        // remove items from queue if not needed
        var expireBefore = DateTime.UtcNow - m_MaximumAge;
        var firstItem = m_Queue.Peek();
        while (m_Queue.Count > m_MinimumData && firstItem.DateTime < expireBefore)
          firstItem = m_Queue.Dequeue();

        // Percentage based on reported value
        Percent = Math.Round((double) m_LastItem.Value / m_TargetValue * 100.0, 1, MidpointRounding.AwayFromZero);

        // calculate the velocity based on first and last value
        var velocityBySecond = (m_LastItem.Value - firstItem.Value) / (m_LastItem.DateTime - firstItem.DateTime).TotalSeconds;
        // estimate current value based on last value and velocity
        var estimatedCurrentValue = m_LastItem.Value + (velocityBySecond * (DateTime.UtcNow - m_LastItem.DateTime).TotalSeconds);
        // assume estimate remaining time on assumed remaining values and velocity
        var finishedInSec = (m_TargetValue - estimatedCurrentValue) / velocityBySecond;

        // Percentage based on estimated value
        EstimatedPercent = Math.Round(estimatedCurrentValue / m_TargetValue * 100.0, 1, MidpointRounding.AwayFromZero);

        // Calculate the estimated finished time
        EstimatedTimeRemaining = ((finishedInSec > .9) && (finishedInSec < Max.TotalSeconds)) ? TimeSpan.FromSeconds(finishedInSec) : Max;
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

        var timeSineLastLog = (DateTime.UtcNow  - m_LastItem.DateTime);
        m_LastItem.DateTime = DateTime.UtcNow;
        m_LastItem.Value = value;

        // Do not queue very rapidly changing values so queue does not get too large
        // we want to sick with roughly 1/2 second
        if (m_Queue.Count < 2 ||  timeSineLastLog.TotalSeconds >= .5)
          m_Queue.Enqueue(m_LastItem);
        UpdateEstimates();
      }
      get => m_LastItem.Value;
    }

    /// <summary>
    ///   Displays the timespan in a human-readable format
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
      return value.TotalHours < 24 ? $"{value:h\\:mm} hrs" : $"{value.TotalDays:F1} days";
    }

    private struct ValueOverTime
    {
      public DateTime DateTime;
      public long Value;
    }
  }
}