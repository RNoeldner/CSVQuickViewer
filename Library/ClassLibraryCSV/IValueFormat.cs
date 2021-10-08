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
  public interface IValueFormat
	{
		DataType DataType { get; }

		string DateFormat { get; }

		/// <summary>
		///   The value will return the resulted Separator, passing in "Colon" will return ":"
		/// </summary>
		string DateSeparator { get; }

		/// <summary>
		///   The value will return the resulted Separator, passing in "Dot" will return "."
		/// </summary>
		string DecimalSeparator { get; }

		string DisplayNullAs { get; }

		/// <summary>
		///   Gets or sets the representation for false.
		/// </summary>
		string False { get; }

		/// <summary>
		///   The value will return the resulted Separator, passing in "Dot" will return "."
		/// </summary>
		string GroupSeparator { get; }

		/// <summary>
		///   Gets or sets the number format
		/// </summary>
		/// <value>The number format.</value>
		string NumberFormat { get; }

		/// <summary>
		///   Gets or sets the part for splitting.
		/// </summary>
		/// <value>The part starting with 1</value>
		int Part { get; }

		/// <summary>
		///   Gets or sets the splitter.
		/// </summary>
		/// <value>The splitter.</value>
		string PartSplitter { get; }

		/// <summary>
		///   Determine if a part should end with teh next splitter
		/// </summary>
		/// <value><c>true</c> if all of the remaining text should be returned in the part</value>
		bool PartToEnd { get; }

		/// <summary>
		///   The value will return the resulted Separator, passing in "Dot" will return "."
		/// </summary>
		string TimeSeparator { get; }

		/// <summary>
		///   Gets or sets the representation for true.
		/// </summary>
		string True { get; }
	}
}