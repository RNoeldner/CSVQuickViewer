﻿/*
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
  /// <inheritdoc />
  public sealed class FileReaderOpenException : FileReaderException
  {
    /// <inheritdoc />
    public FileReaderOpenException()
      : base(GetMessage(string.Empty))
    {
    }

    /// <inheritdoc />
    public FileReaderOpenException(string message)
      : base(GetMessage(message))
    {
    }

    /// <inheritdoc />
    public FileReaderOpenException(string message, Exception inner)
      : base(GetMessage(message), inner)
    {
    }
    
    private static string GetMessage(string message) =>
      message.Length > 0
        ? $"A file reader has to be opened before reading data. Please execute Open() or OpenAsync().\n{message}"
        : "A file reader has to be opened before reading data.";
  }
}
