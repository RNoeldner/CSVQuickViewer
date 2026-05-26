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

namespace CsvTools;


/// <summary>
/// Provides data for progress-related events.
/// </summary>
/// <remarks>
/// This type is used to pass progress state, completion percentage,
/// and estimated remaining time to event subscribers.
/// </remarks>
public readonly struct ProgressChangedEventArgs : IEquatable<ProgressChangedEventArgs>
{
  /// <summary>
  /// Gets the current progress snapshot.
  /// </summary>
  public string Information { get; }

  /// <summary>
  /// Gets the completion percentage, expressed as a value from 0 to 100.
  /// </summary>
  public double Percent { get; }

  /// <summary>
  /// Gets the estimated remaining time for the operation.
  /// </summary>
  /// <remarks>
  /// A value of <see cref="TimeSpan.Zero"/> indicates completion.
  /// Implementations may provide a fallback value when an estimate is unavailable.
  /// </remarks>
  public TimeSpan EstimatedTimeRemaining { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="ProgressChangedEventArgs"/> struct.
  /// </summary>
  /// <param name="information">The current progress information.</param>
  /// <param name="percent">The completion percentage (0–1).</param>
  /// <param name="estimatedTimeRemaining">The estimated remaining time.</param>
  public ProgressChangedEventArgs(string information, double percent, TimeSpan estimatedTimeRemaining)
  {
    Information = information;
    Percent = percent;
    EstimatedTimeRemaining = estimatedTimeRemaining;
  }

  /// <inheritdoc />
  public bool Equals(ProgressChangedEventArgs other) =>
     Information.Equals(other.Information) &&
     Percent.Equals(other.Percent) &&
     EstimatedTimeRemaining.Equals(other.EstimatedTimeRemaining);

  /// <inheritdoc />
  public override bool Equals(object? obj) =>
      obj is ProgressChangedEventArgs other && Equals(other);

  /// <inheritdoc />
  public override int GetHashCode()
  {
    unchecked
    {
      int hash = 17;
      hash = hash* 23 + Information.GetHashCode();
      hash = hash* 23 + Percent.GetHashCode();
      hash = hash* 23 + EstimatedTimeRemaining.GetHashCode();
      return hash;
    }
  }
}