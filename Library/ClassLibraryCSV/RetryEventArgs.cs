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
/// Event Args for Retry Questions
/// </summary>
/// <seealso cref="System.EventArgs" />
public sealed class RetryEventArgs : EventArgs
{
  /// <summary>
  /// Initializes a new instance of the <see cref="RetryEventArgs"/> class.
  /// </summary>
  /// <param name="ex">The ex.</param>
  public RetryEventArgs(Exception ex) => Exception = ex;

  /// <summary>
  /// Gets the exception.
  /// </summary>
  /// <value>
  /// The exception.
  /// </value>
  public Exception Exception { get; }

  /// <summary>
  /// Gets or sets a value indicating whether this <see cref="RetryEventArgs"/> is retry.
  /// </summary>
  /// <value>
  ///   <c>true</c> if it should try and retry; otherwise, <c>false</c>.
  /// </value>
  public bool Retry { get; set; }
}