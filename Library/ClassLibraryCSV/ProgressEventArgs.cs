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
  ///   Argument for a ProgressEvent
  /// </summary>
  public class ProgressEventArgs : EventArgs
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="ProgressEventArgs" /> class.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="value">The progress value.</param>
    /// <param name="log"><c>True</c> if progress should be logged, <c>false</c> otherwise.</param>
    public ProgressEventArgs(in string text, long value = -1, bool log = false)
    {
      Text = text;
      Value = value;
      Log = log;
    }

    /// <summary>
    ///   Indicating if a progress should be logged or not
    /// </summary>
    public bool Log { get; }

    /// <summary>
    ///   Gets or sets the text.
    /// </summary>
    /// <value>The text.</value>
    public string Text { get; }

    /// <summary>
    ///   Gets or sets the value.
    /// </summary>
    /// <value>The value.</value>
    public long Value { get; }
  }

  public class ProgressEventArgsTime : ProgressEventArgs
  {
    public ProgressEventArgsTime(in string text, long value, in TimeSpan estimatedTimeRemaining, double percent) : base(text,
      value)
    {
      EstimatedTimeRemaining = estimatedTimeRemaining;
      Percent = percent;
    }

    public TimeSpan EstimatedTimeRemaining { get; }

    public double Percent { get; }
  }
}