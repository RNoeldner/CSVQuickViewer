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
  [Flags]
	public enum FilterTypeEnum
	{
		None = 0,

		// Display rows that have no error nor warning
		ShowIssueFree = 1,

		// Display rows that have a warning
		ShowWarning = 2,

		// Display rows that have an error
		ShowErrors = 4,

		// Display rows that have an error or a warning
		ErrorsAndWarning = 2 + 4,

		All = 1 + 2 + 4,

		// A true error is an error that has proper error information, in some cases only a placeholder text is stored as the real message is not known
		OnlyTrueErrors = 8
	}
}