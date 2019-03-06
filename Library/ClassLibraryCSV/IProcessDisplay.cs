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
using System.Threading;

namespace CsvTools
{
  /// <summary>
  ///  Interface for an ProcessDisplay
  /// </summary>
  public interface IProcessDisplay : IDisposable
  {
    /// <summary>
    ///  Event handler called as progress should be displayed
    /// </summary>
    event EventHandler<ProgressEventArgs> Progress;

    /// <summary>
    ///  Gets or sets the cancellation token.
    /// </summary>
    /// <value>
    ///  The cancellation token.
    /// </value>
    CancellationToken CancellationToken { get; }

    /// <summary>
    ///  Gets or sets the maximum value for the Progress
    /// </summary>
    /// <value>
    ///  The maximum value.
    /// </value>
    int Maximum { get; set; }

    /// <summary>
    ///  To be called if the display should be closed, this will cancel any processing
    /// </summary>
    void Cancel();

    /// <summary>
    ///  Event to be called if the display should be updated
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void SetProcess(object sender, ProgressEventArgs e);

    string Title { get; set; }

    /// <summary>
    ///  Sets the process display
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="value">The current progress</param>
    void SetProcess(string text, int value = -1);
  }
}