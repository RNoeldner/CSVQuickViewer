using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

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

namespace CsvTools.Tests
{
  [TestClass]
  public sealed class StructuredFileWriterTests
  {
    private const string c_ReadID = "StructuredFileWriterTests";

    [TestInitialize]
    public void Init()
    {
      var readFile = new CsvFile { ID = c_ReadID, FileName = UnitTestStatic.GetTestPath("BasicCSV.txt"), CommentLine = "#" };
      readFile.ColumnCollection.Add(new Column("ExamDate", @"dd/MM/yyyy"));
      readFile.ColumnCollection.Add(new Column("Score", DataType.Integer));
      readFile.ColumnCollection.Add(new Column("Proficiency", DataType.Numeric));
      readFile.ColumnCollection.Add(new Column("IsNativeLang", DataType.Boolean) { Ignore = true });
      UnitTestStatic.MimicSQLReader.AddSetting(readFile);
    }

    [TestMethod]
    public async Task StructuredFileWriterJSONEncodeTestAsync()
    {
      var fileSetting = new JsonFile { ID = "Write", FileName = "StructuredFileOutputJSON.txt", SqlStatement = c_ReadID, InOverview = true };

      var sb = new StringBuilder("{");
      var processDisplay = new CustomProcessDisplay();
      var cols = await DetermineColumnFormat.GetSqlColumnNamesAsync(
                   fileSetting.SqlStatement,
                   fileSetting.Timeout,
                   UnitTestStatic.Token);
      fileSetting.Header = "{\"rowset\":[\n";

      // { "firstName":"John", "lastName":"Doe"},
      foreach (var col in cols)
        sb.AppendFormat(
          "\"{0}\":\"{1}\", ",
          HTMLStyle.JsonElementName(col),
          string.Format(CultureInfo.InvariantCulture, StructuredFileWriter.cFieldPlaceholderByName, col));

      if (sb.Length > 1)
        sb.Length -= 2;
      sb.AppendLine("},");
      fileSetting.Row = sb.ToString();
      var writer = new JsonFileWriter(
        fileSetting.ID,
        fileSetting.FullPath,
        fileSetting.Recipient,
        fileSetting.KeepUnencrypted,
        fileSetting.IdentifierInContainer,
        fileSetting.Footer,
        fileSetting.Header,
        fileSetting.ColumnCollection,
        "Test",
        fileSetting.Row,
        processDisplay);

      var result = await writer.WriteAsync(
                     fileSetting.SqlStatement,
                     fileSetting.Timeout,
                     processDisplay,
                     UnitTestStatic.Token);
      Assert.AreEqual(7L, result);
    }

    [TestMethod]
    public async Task StructuredFileWriterXMLEncodeTest()
    {
      var fileSetting = new XmlFile { ID = "Write", FileName = "StructuredFileOutputXML.txt", SqlStatement = c_ReadID, InOverview = true };
      var sb = new StringBuilder();
      var processDisplay = new CustomProcessDisplay();
      var cols = await DetermineColumnFormat.GetSqlColumnNamesAsync(
                   fileSetting.SqlStatement,
                   fileSetting.Timeout,
                   UnitTestStatic.Token);
      sb.AppendLine("<?xml version=\"1.0\"?>\n");
      sb.AppendLine("<rowset>");
      fileSetting.Header = sb.ToString();
      sb.Clear();
      sb.AppendLine("  <row>");
      foreach (var col in cols)
        sb.AppendFormat(
          "    <{0}>{1}</{0}>\n",
          HTMLStyle.XmlElementName(col),
          string.Format(CultureInfo.InvariantCulture, StructuredFileWriter.cFieldPlaceholderByName, col));

      sb.AppendLine("  </row>");
      fileSetting.Row = sb.ToString();
      fileSetting.Footer = "</rowset>";

      var writer = new XmlFileWriter(
        fileSetting.ID,
        fileSetting.FullPath,
        fileSetting.Recipient,
        fileSetting.KeepUnencrypted,
        fileSetting.IdentifierInContainer,
        fileSetting.Footer,
        fileSetting.Header,
        fileSetting.ColumnCollection,
        "Test",
        fileSetting.Row,
        processDisplay);
      await writer.WriteAsync(fileSetting.SqlStatement, fileSetting.Timeout, processDisplay, UnitTestStatic.Token);
    }
  }
}