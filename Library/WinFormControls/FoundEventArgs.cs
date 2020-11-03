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

namespace CsvTools
{
  using System;
  using System.Windows.Forms;

  /// <summary>
  ///   Event Arguments for Finding a text in a DataGridView
  /// </summary>
  public class FoundEventArgs : EventArgs
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="FoundEventArgs" /> class.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="cell">The cell.</param>
    public FoundEventArgs(int index, DataGridViewCell cell)
    {
      Index = index;
      Cell = cell;
    }

#pragma warning disable CA1051 // Do not declare visible instance fields

    /// <summary>
    ///   The cell
    /// </summary>
    public readonly DataGridViewCell Cell;

    /// <summary>
    ///   The index
    /// </summary>
    public readonly int Index;

#pragma warning restore CA1051 // Do not declare visible instance fields
  }
}