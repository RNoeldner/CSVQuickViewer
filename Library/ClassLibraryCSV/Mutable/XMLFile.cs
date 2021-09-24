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
  ///   Setting file for XML files, its an implementation of <see cref="StructuredFile" />
  /// </summary>
  [Serializable]
	public class XMLFile : StructuredFile, IXMLFile

	{
		/// <summary>
		///   Initializes a new instance of the <see cref="StructuredFile" /> class.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		public XMLFile(string fileName)
			: base(fileName)
		{
		}

		/// <summary>
		///   Initializes a new instance of the <see cref="StructuredFile" /> class.
		/// </summary>
		public XMLFile()
			: this(string.Empty)
		{
		}

		public override object Clone()
		{
			var other = new XMLFile();
			CopyTo(other);
			return other;
		}

		public override bool Equals(IFileSetting? other) =>
			other is IXMLFile json && BaseSettingsEquals(json as StructuredFile);
	}
}