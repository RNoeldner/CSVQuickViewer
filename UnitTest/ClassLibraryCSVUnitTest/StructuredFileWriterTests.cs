using Microsoft.VisualStudio.TestTools.UnitTesting; /*

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

using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
	public class StructuredFileWriterTests
	{
		private const string c_ReadID = "StructuredFileWriterTests";

		[TestInitialize]
		public void Init()
		{
			var readFile = new CsvFile
			{
				ID = c_ReadID,
				FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"),
				FileFormat = { CommentLine = "#" }
			};
			readFile.ColumnCollection.Add(new Column("ExamDate", @"dd/MM/yyyy"));
			readFile.ColumnCollection.Add(new Column("Score", DataType.Integer));
			readFile.ColumnCollection.Add(new Column("Proficiency", DataType.Numeric));
			readFile.ColumnCollection.Add(new Column("IsNativeLang", DataType.Boolean) { Ignore = true });
			UnitTestInitializeCsv.MimicSQLReader.AddSetting(readFile);
		}

		[TestMethod]
		public async Task StructuredFileWriterJSONEncodeTestAsync()
		{
			var writeFile = new StructuredFile
			{
				ID = "Write",
				FileName = "StructuredFileOutputJSON.txt",
				SqlStatement = c_ReadID,
				InOverview = true,
				JSONEncode = true
			};

			var sb = new StringBuilder("{");
			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			{
				var cols = await DetermineColumnFormat.GetSqlColumnNamesAsync(writeFile.SqlStatement, writeFile.Timeout,
					processDisplay.CancellationToken);
				writeFile.Header = "{\"rowset\":[\n";

				// { "firstName":"John", "lastName":"Doe"},
				foreach (var col in cols)
					sb.AppendFormat("\"{0}\":\"{1}\", ", HTMLStyle.JsonElementName(col),
						string.Format(CultureInfo.InvariantCulture, StructuredFileWriter.cFieldPlaceholderByName, col));

				if (sb.Length > 1)
					sb.Length -= 2;
				sb.AppendLine("},");
				writeFile.Row = sb.ToString();
				var writer = new StructuredFileWriter(writeFile, processDisplay);
				var result =
					await writer.WriteAsync(writeFile.SqlStatement, writeFile.Timeout, t => processDisplay.SetProcess(t, 0, true),
						processDisplay.CancellationToken);
				Assert.AreEqual(7L, result);
			}
		}

		[TestMethod]
		public async Task StructuredFileWriterXMLEncodeTest()
		{
			var writeFile = new StructuredFile
			{
				ID = "Write",
				FileName = "StructuredFileOutputXML.txt",
				SqlStatement = c_ReadID,
				InOverview = true,
				JSONEncode = false
			};
			var sb = new StringBuilder();
			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			{
				var cols = await DetermineColumnFormat.GetSqlColumnNamesAsync(writeFile.SqlStatement, writeFile.Timeout,
					processDisplay.CancellationToken);
				sb.AppendLine("<?xml version=\"1.0\"?>\n");
				sb.AppendLine("<rowset>");
				writeFile.Header = sb.ToString();
				sb.Clear();
				sb.AppendLine("  <row>");
				foreach (var col in cols)
					sb.AppendFormat("    <{0}>{1}</{0}>\n", HTMLStyle.XmlElementName(col),
						string.Format(CultureInfo.InvariantCulture, StructuredFileWriter.cFieldPlaceholderByName, col));

				sb.AppendLine("  </row>");
				writeFile.Row = sb.ToString();
				writeFile.Footer = "</rowset>";

				var writer = new StructuredFileWriter(writeFile, processDisplay);
				await writer.WriteAsync(writeFile.SqlStatement, writeFile.Timeout, null, processDisplay.CancellationToken);
			}
		}
	}
}