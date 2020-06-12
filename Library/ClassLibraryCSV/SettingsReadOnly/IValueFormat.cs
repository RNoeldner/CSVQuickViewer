// /*
//  * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
//  *
//  * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
//  * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//  *
//  * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
//  * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
//  *
//  * You should have received a copy of the GNU Lesser Public License along with this program.
//  * If not, see http://www.gnu.org/licenses/ .
//  *
//  */

using JetBrains.Annotations;

namespace CsvTools
{
  public interface IValueFormat
  {
    DataType DataType { get; }
    [NotNull] string DateFormat { get; }
    [NotNull] string DateSeparator { get; }
    char DecimalSeparatorChar { get; }
    [NotNull] string DisplayNullAs { get; }
    [NotNull] string False { get; }
    char GroupSeparatorChar { get; }
    [NotNull] string NumberFormat { get; }
    [NotNull] string TimeSeparator { get; }
    [NotNull] string True { get; }
  }
}