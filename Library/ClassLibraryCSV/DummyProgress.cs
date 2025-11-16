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
using System.Threading;

namespace CsvTools
{
  /// <summary>
  /// A dummy implementation of <see cref="IProgress{T}"/> 
  /// that silently ignores all reported progress values.
  ///
  /// This is useful in scenarios where:
  /// <list type="bullet">
  /// <item><description>A method requires a progress reporter, but the caller has no interest in handling updates.</description></item>
  /// <item><description>You want to avoid creating conditional logic around optional progress reporting.</description></item>
  /// <item><description>You want a safe fallback that supports cancellation token exposure without UI or logging dependencies.</description></item>
  /// </list>
  /// </summary>
  /// <typeparam name="T">The type of progress updates being reported.</typeparam>
  public class DummyProgress : IProgressWithCancellation
  {
    /// <summary>
    /// Default instance
    /// </summary>
    public static DummyProgress Instance = new DummyProgress();

    /// <summary>
    /// Always returns <see cref="CancellationToken.None"/>, as this dummy implementation does not support cancellation.
    /// </summary>
    public CancellationToken CancellationToken => CancellationToken.None;

    /// <summary>
    /// Reports a progress update. This implementation intentionally ignores the value.
    /// </summary>
    /// <param name="value">The progress information to report. It is not processed.</param>
    public void Report(ProgressInfo value)
    {
      // Intentionally does nothing.
      // Uncomment the following line if you want to break in debugger during development.
      // Debugger.Break();
    }
  }
}