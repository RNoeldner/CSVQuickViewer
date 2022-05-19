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
  ///   Interface for an ProcessDisplay
  /// </summary>
  public interface IProcessDisplay
  {
    string Title { get; set; }

    /// <summary>
    ///   Event handler called as progress should be displayed
    /// </summary>
    event EventHandler<ProgressEventArgs>? Progress;

    /// <summary>
    ///   Event to be called if the display should be updated
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void SetProcess(object? sender, ProgressEventArgs e);

    /// <summary>
    ///   Sets the process display
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="value">The current progress</param>
    /// <param name="log"><c>True</c> if progress should be logged, <c>false</c> otherwise.</param>
    void SetProcess(string text, long value, bool log);
  }
}