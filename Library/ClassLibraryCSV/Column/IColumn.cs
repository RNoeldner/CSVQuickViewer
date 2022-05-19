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
  ///  Interface to describe a column during read and write
  /// </summary>
  public interface IColumn : ICloneable, IEquatable<IColumn>
  {
    /// <summary>
    ///   Gets the column ordinal 
    /// </summary>
    int ColumnOrdinal { get; }

    /// <summary>
    ///   Indicating if the value or text should be converted
    /// </summary>
    bool Convert { get; }

    /// <summary>
    ///   Name of the column in the destination system
    /// </summary>
    string DestinationName { get; }

    /// <summary>
    ///   Indicating if the column should be ignored during read or write, no conversion is done if the column is ignored teh target will not show this column
    /// </summary>
    bool Ignore { get; }

    /// <summary>
    ///   Name of the column 
    /// </summary>
    string Name { get; }

    /// <summary>
    ///   For DateTime import you can combine a date and a time column into a single datetime column, to do this specify the time column on the column for the date
    /// </summary>
    string TimePart { get; }

    /// <summary>
    ///   For DateTime import you can combine a date and a time into a single datetime column, specify the format of the time here this can different from the format of the time column itself that is often ignored
    /// </summary>
    string TimePartFormat { get; }

    /// <summary>
    ///   For DateTime import you set a time zone, during read the value provided will be assumed to be of the timezone specified
    ///   During write the time will be converted into this time zone
    /// </summary>
    string TimeZonePart { get; }

    /// <summary>
    ///   Formatting option for values
    /// </summary>
    IValueFormat ValueFormat { get; }
  }
}