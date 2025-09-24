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
  /// A dummy implementation of <see cref="IProgress{T}"/> that ignores progress reports.
  /// Useful when you need to pass a progress reporter but do not want to handle progress updates.
  /// </summary>
  public class DummyProgress : IProgress<ProgressInfo>
  {
    /// <summary>
    /// Reports progress information. This implementation ignores the reported value.
    /// </summary>
    /// <param name="value">The progress information to report.</param>
    public void Report(ProgressInfo value)
    {
      // Intentionally does nothing.
      // Uncomment the following line if you want to break in debugger during development.
      // Debugger.Break();
    }
  }
}