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
#nullable enable

namespace CsvTools
{
  /// <summary>
  ///   Enumeration for reasons why not to display values
  /// </summary>
  public enum BuildValueClustersResult
  {
    /// <summary>
    ///   The Value cluster has not been built yet
    /// </summary>
    NotRun,

    /// <summary>
    ///   Is the wrong type to generate Value Cluster
    /// </summary>
    WrongType,

    /// <summary>
    ///   The too many values
    /// </summary>
    TooManyValues,

    /// <summary>
    ///   The not one values
    /// </summary>
    NoValues,

    /// <summary>
    ///   The list is filled
    /// </summary>
    ListFilled,

    Error
  }
}