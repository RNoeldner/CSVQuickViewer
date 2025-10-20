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
namespace CsvTools
{
  /// <summary>
  /// Setting for writing Json files
  /// </summary>
  public interface IJsonFile : IFileSettingPhysicalFile
  {
    /// <summary>
    ///   Template for a single data row, with fixed text and placeholders for the values
    /// </summary>
    string Row { get; set; }

    /// <summary>
    ///   If values is empty or null generate it as null instead of producing an empty string
    /// </summary>
    bool EmptyAsNull { get; set; }
  }
}
