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
using System.Collections.Generic;

namespace CsvTools
{
  /// <inheritdoc cref="IFileReader" />
  /// <summary>
  ///   Interface for a File Reader.
  /// </summary>
  public interface IFileReaderWithEvents : IFileReader
  {
    /// <summary>
    ///   Occurs when an open process failed, allowing the user to change the timeout or provide the
    ///   needed file etc.
    /// </summary>
    event EventHandler<RetryEventArgs>? OnAskRetry;

    /// <summary>
    ///   Event to be raised once the reader is opened, the column information is now known and
    ///   passed to the EventHandler
    /// </summary>
    event EventHandler<IReadOnlyCollection<IColumn>>? OpenFinished;

    /// <summary>
    ///   Event to be raised once the reader is finished reading the file
    /// </summary>
    event EventHandler? ReadFinished;
  }
}