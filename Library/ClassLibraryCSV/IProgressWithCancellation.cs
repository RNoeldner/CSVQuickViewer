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
 */
using System;
using System.Threading;

namespace CsvTools
{
  /// <summary>
  /// Provides a progress-reporting interface that exposes a <see cref="CancellationToken"/>.
  /// </summary>
  /// <remarks>
  /// This interface unifies:
  /// <list type="bullet">
  ///   <item><description>Progress reporting via <see cref="IProgress{T}"/> using <see cref="ProgressInfo"/>.</description></item>
  ///   <item><description>Cancellation support through an associated <see cref="CancellationToken"/>.</description></item>
  ///   <item><description>Passing on report information to Logger</description></item>
  /// </list>
  ///
  /// Typical use cases include UI-bound progress displays and long-running operations
  /// that should support cooperative cancellation.
  /// </remarks>
  public interface IProgressWithCancellation : IProgress<ProgressInfo>
  {
    /// <summary>
    /// Gets the cancellation token associated with the progress operation.
    /// Implementations should respect this token to allow cooperative cancellation.
    /// </summary>
    public CancellationToken CancellationToken { get; }
  }
}
