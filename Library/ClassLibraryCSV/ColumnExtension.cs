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

using System.Text;

namespace CsvTools
{
  public static class ColumnExtension
	{
		/// <summary>
		///   Gets the description.
		/// </summary>
		/// <returns></returns>
		public static string GetTypeAndFormatDescription(this IColumn column, bool addTime = true)
		{
			var stringBuilder = new StringBuilder(column.ValueFormat.DataType.DataTypeDisplay());

			var shortDesc = column.ValueFormat.GetFormatDescription();
			if (shortDesc.Length > 0)
			{
				stringBuilder.Append(" (");
				stringBuilder.Append(shortDesc);
				stringBuilder.Append(")");
			}

			if (addTime && column.ValueFormat.DataType == DataTypeEnum.DateTime)
			{
				if (column.TimePart.Length > 0)
				{
					stringBuilder.Append(" + ");
					stringBuilder.Append(column.TimePart);
					if (column.TimePartFormat.Length > 0)
					{
						stringBuilder.Append(" (");
						stringBuilder.Append(column.TimePartFormat);
						stringBuilder.Append(")");
					}
				}

				if (column.TimeZonePart.Length > 0)
				{
					stringBuilder.Append(" - ");
					stringBuilder.Append(column.TimeZonePart);
				}
			}

			if (column.Ignore)
				stringBuilder.Append(" (Ignore)");

			return stringBuilder.ToString();
		}
	}
}