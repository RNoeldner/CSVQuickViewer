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
	///   Calculates the "Estimated Time of Completion"
	///   based on a rolling average of progress over time.
	/// </summary>
	public class TimeToCompletion
	{
		private readonly long m_MaximumTicks;
		private readonly byte m_MinimumData;
		private readonly Queue<ProgressOverTime> m_Queue;
		private readonly Stopwatch m_Stopwatch = new Stopwatch();
		private ProgressOverTime m_FirstItem;
		private ProgressOverTime m_LastItem;
		private long m_TargetValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="TimeToCompletion"/> class.
		/// </summary>
		/// <param name="targetValue">The target value / maximum that would match 100%</param>
		/// <param name="minimumData">The number of entries before a estimation is done.</param>
		/// <param name="storedSeconds">The number of seconds to remember for estimation.</param>
		public TimeToCompletion(long targetValue = -1, byte minimumData = 4, double storedSeconds = 60.0)
		{
			m_MinimumData = minimumData;
			m_MaximumTicks = (long)(storedSeconds * Stopwatch.Frequency);
			m_Queue = new Queue<ProgressOverTime>(minimumData * 2);

			m_LastItem.Value = 0;
			TargetValue = targetValue;
		}

		/// <summary>
		/// Gets the estimated time remaining to complete
		/// </summary>
		/// <value>
		/// The estimated time remaining.
		/// </value>
		public TimeSpan EstimatedTimeRemaining { get; private set; } = TimeSpan.MaxValue;

		public string EstimatedTimeRemainingDisplay => DisplayTimespan(EstimatedTimeRemaining);

		/// <summary>
		/// Gets the estimated time remaining display with a leading separator (if needed).
		/// </summary>
		/// <value>
		/// The estimated time remaining display separator.
		/// </value>
		public string EstimatedTimeRemainingDisplaySeparator
		{
			get
			{
				if (EstimatedTimeRemaining == TimeSpan.MaxValue)
					return string.Empty;
				return " - " + EstimatedTimeRemainingDisplay;
			}
		}

		/// <summary>
		/// Gets the current percentage
		/// </summary>
		/// <value>
		/// Percent (usually between 0 and 1)
		/// </value>
		public double Percent { get; private set; }

		public string PercentDisplay => string.Format(CultureInfo.CurrentCulture, Percent < 10 ? "{0:F1}%" : "{0:F0}%", Percent);

		/// <summary>
		/// Gets or sets the target value / maximum that would match 100%.
		/// </summary>
		/// <value>
		/// The target value.
		/// </value>
		public long TargetValue
		{
			get => m_TargetValue;
			set
			{
				var newVal = value > 1 ? value : 1;

				m_TargetValue = newVal;
				m_Queue.Clear();

				m_Stopwatch.Reset();
				m_Stopwatch.Start();

				Value = 0;
			}
		}

		/// <summary>
		/// Gets or sets the current value in the process
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		public long Value
		{
			set
			{
				if (value < 0 || m_LastItem.Value == value || value > m_TargetValue)
					return;

				// Something strange happening we are going backwards, assume we are counting from the beginning again (possibly reuse of the object)
				if (value < m_LastItem.Value)
					m_Queue.Clear();

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

				Percent = Math.Round((double)value / m_TargetValue * 100.0, 1, MidpointRounding.AwayFromZero);

				// Make sure we have enough items to estimate

				if (m_Queue.Count < m_MinimumData || m_FirstItem.Value == m_LastItem.Value)
				{
					EstimatedTimeRemaining = TimeSpan.MaxValue;
				}
				else
				{
					var finishedInTicks = (m_TargetValue - m_LastItem.Value) * (double)(m_LastItem.Tick - m_FirstItem.Tick) /
																(m_LastItem.Value - m_FirstItem.Value);
					// Calculate the estimated finished time but add 5%
					EstimatedTimeRemaining = finishedInTicks / Stopwatch.Frequency > .9
						? TimeSpan.FromSeconds(finishedInTicks / Stopwatch.Frequency * 1.05)
						: TimeSpan.MaxValue;
				}
			}
			get => m_LastItem.Value;
		}

		/// <summary>
		/// Displays the timespan in a human readable format
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="cut2Sec">If true any value shorter than 2 seconds will be empty</param>
		/// <returns></returns>
		public static string DisplayTimespan(TimeSpan value, bool cut2Sec = true)
		{
			if (value == TimeSpan.MaxValue || (cut2Sec && value.TotalSeconds < 2))
				return string.Empty;
			if (value.TotalMinutes < 1)
				return $"{Math.Round(value.TotalSeconds, 0, MidpointRounding.AwayFromZero):0} sec";
			return value.TotalHours < 1
				? $"{value.Minutes:D2}:{value.Seconds:D2}"
				: $"{(int)value.TotalHours}:{value.Minutes:D2}:{value.Seconds:D2}";
		}

		private struct ProgressOverTime
		{
			public long Tick;
			public long Value;
		}
	}
}