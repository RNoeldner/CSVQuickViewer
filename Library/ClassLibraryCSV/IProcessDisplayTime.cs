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
using System;

namespace CsvTools
{
  /// <summary>
  ///   Represents a progress reporter that tracks a value, supports a defined maximum,
  ///   and provides estimated time-to-completion based on observed progress velocity.
  /// </summary>
  public interface IProgressTime : IProgress<ProgressInfo>
  {
    /// <summary>
    ///   Gets or sets the maximum expected value for the progress reporter.
    ///   This value represents 100% completion.
    /// </summary>
    /// <value>
    ///   The maximum progress value. Must be greater than zero.
    /// </value>
    long Maximum { get; set; }

    /// <summary>
    ///   Provides time-to-completion estimation based on the current value,
    ///   the maximum, and the observed velocity of progress.
    /// </summary>
    /// <value>
    ///   The <see cref="TimeToCompletion"/> instance that calculates remaining time.
    /// </value>
    TimeToCompletion TimeToCompletion { get; }

    /// <summary>
    ///   Occurs whenever a new progress value is reported.
    ///   Subscribers can use this event to update UI or trigger other actions.
    /// </summary>
    /// <summary>Raised for each reported progress value.</summary>
    public Action<(ProgressInfo, TimeToCompletion)>? ProgressChanged { get; set; }
  }
}
