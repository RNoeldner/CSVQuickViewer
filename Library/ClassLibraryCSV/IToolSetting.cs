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

using System.Collections.Generic;

namespace CsvTools
{
  /// <summary>
  ///   Interface for general setting
  /// </summary>
  public interface IToolSetting
  {
    string DestinationTimeZone { get; }

    /// <summary>
    ///   Gets the input settings
    /// </summary>
    /// <value>
    ///   The input settings
    /// </value>
    ICollection<IFileSetting> Input { get; }

    /// <summary>
    ///   Gets the output settings
    /// </summary>
    /// <value>
    ///   The output settings
    /// </value>
    ICollection<IFileSetting> Output { get; }

    PGPKeyStorage PGPInformation { get; }

    /// <summary>
    ///   Gets the root folder of the Tool Setting
    /// </summary>
    /// <value>
    ///   The root folder.
    /// </value>
    string RootFolder { get; }

    ICache<string, ValidationResult> ValidationResultCache { get; }
  }
}