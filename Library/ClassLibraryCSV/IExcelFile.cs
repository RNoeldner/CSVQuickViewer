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
using System.Runtime.InteropServices;

namespace CsvTools
{
  /// <summary>
  ///   Interface for the setting of an Excel file
  /// </summary>
  [ComVisible(true)]
  [Guid("492F68EA-7999-494C-A319-491988801812")]
  public interface IExcelFile : IFileSettingRemoteDownload, IEquatable<IExcelFile>
  {
    /// <summary>
    ///   Gets or sets a value indicating the types of typed values should be read or if text should
    ///   be read instead
    /// </summary>
    /// <value><c>true</c> write byte order mark; otherwise, <c>false</c>.</value>
    bool MixedTypes { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether the byte order mark should be written in Unicode files.
    /// </summary>
    /// <value><c>true</c> write byte order mark; otherwise, <c>false</c>.</value>
    string SheetName { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether the byte order mark should be written in Unicode files.
    /// </summary>
    /// <value><c>true</c> write byte order mark; otherwise, <c>false</c>.</value>
    string SheetRange { get; set; }

    /// <summary>
    ///   Gets an instance of an <see cref="IExcelFileReader" />
    /// </summary>
    IExcelFileReader GetExcelFileReader();
  }
}