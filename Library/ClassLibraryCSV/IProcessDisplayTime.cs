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
  ///   Interface for a progress
  /// </summary>
  public interface IProgressTime : IProgress<ProgressInfo>
  {
    /// <summary>
    ///   Gets or sets the maximum value for the Progress
    /// </summary>
    /// <value>
    ///   The maximum value.
    /// </value>
    long Maximum { get; set; }

    /// <summary>
    /// Time to completion, calculated on velocity Maxvalue and current value
    /// </summary>
    /// <value>
    /// The time to completion.
    /// </value>
    TimeToCompletion TimeToCompletion { get; }

    /// <summary>Raised for each reported progress value.</summary>
    public event EventHandler<ProgressInfo> ProgressChanged;
  }
}
