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
namespace CsvTools;

/// <summary>
///   Enumeration of the different trimming options
/// </summary>
public enum TrimmingOptionEnum
{
  /// <summary>
  ///   No Trimming, all that is passed in will be returned, no matter if it is quoted or not
  /// </summary>
  None = 0,

  /// <summary>
  ///   Do trim unquoted Text, but do not trim quoted text, as quoted text is considered to be verbatim and should not be changed
  /// </summary>
  Unquoted = 1,

  /// <summary>
  ///   Trim everywhere — both outside and inside the quotes.
  /// </summary>
  All = 3
}