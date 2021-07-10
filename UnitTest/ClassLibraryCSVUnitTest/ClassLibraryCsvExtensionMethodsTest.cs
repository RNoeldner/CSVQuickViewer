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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
	public class ClassLibraryCsvExtensionMethodsTest
	{
		[TestMethod]
		public void GetDescription()
		{
			Assert.AreEqual(string.Empty, "".GetDescription());
			Assert.AreEqual("Horizontal Tab", "\t".GetDescription());
			Assert.AreEqual("Comma: ,", ",".GetDescription());
			Assert.AreEqual("Pipe: |", "|".GetDescription());
			Assert.AreEqual("Semicolon: ;", ";".GetDescription());
			Assert.AreEqual("Colon: :", ":".GetDescription());
			Assert.AreEqual("Quotation marks: \"", "\"".GetDescription());
			Assert.AreEqual("Apostrophe: '", "'".GetDescription());
			Assert.AreEqual("Space", " ".GetDescription());
			Assert.AreEqual("Backslash: \\", "\\".GetDescription());
			Assert.AreEqual("Slash: /", '/'.GetDescription());
			Assert.AreEqual("Unit Separator: Char 31", "US".WrittenPunctuation().GetDescription());
			Assert.AreEqual("Unit Separator: Char 31", "Unit Separator".WrittenPunctuation().GetDescription());
			Assert.AreEqual("Unit Separator: Char 31", "char(31)".WrittenPunctuation().GetDescription());

			Assert.AreEqual("Group Separator: Char 29", "GS".WrittenPunctuation().GetDescription());
			Assert.AreEqual("Record Separator: Char 30", "RS".WrittenPunctuation().GetDescription());
			Assert.AreEqual("File Separator: Char 28", "FS".WrittenPunctuation().GetDescription());
		}
	}
}