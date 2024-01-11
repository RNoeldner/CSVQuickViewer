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
using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public sealed class StructuredFileWriterTests
  {
    [TestMethod]
    public async Task StructuredFileWriterJsonEncodeTestAsync()
    {
      var fileSetting =
        new JsonFile("Write", "StructuredFileOutputJSON.txt") { InOverview = true };

      var sb = new StringBuilder("{");

      // ReSharper disable once StringLiteralTypo
      fileSetting.Header = "{\"rowset\":[\n";

      // { "firstName":"John", "lastName":"Doe"},
      var readFile = new CsvFile("ID", UnitTestStatic.GetTestPath("BasicCSV.txt")) { CommentLine = "#" };
      readFile.ColumnCollection.Add(new Column("ExamDate", new ValueFormat(DataTypeEnum.DateTime, @"dd/MM/yyyy")));
      readFile.ColumnCollection.Add(new Column("Score", new ValueFormat(DataTypeEnum.Integer)));
      readFile.ColumnCollection.Add(new Column("Proficiency", new ValueFormat(DataTypeEnum.Numeric)));
      readFile.ColumnCollection.Add(new Column("IsNativeLang", new ValueFormat(DataTypeEnum.Boolean), ignore: true));

      using var reader = FunctionalDI.FileReaderWriterFactory.GetFileReader(readFile, UnitTestStatic.Token);
      await reader.OpenAsync(UnitTestStatic.Token);

      foreach (var col in reader.GetColumnsOfReader())
        sb.AppendFormat(
          "\"{0}\":\"{1}\", ",
          HtmlStyle.JsonElementName(col.Name),
          string.Format(CultureInfo.InvariantCulture, StructuredFileWriter.cFieldPlaceholderByName, col.Name));

      if (sb.Length > 1)
        sb.Length -= 2;
      sb.AppendLine("},");
      fileSetting.Row = sb.ToString();
      var writer = new JsonFileWriter(
        fileSetting.ID,
        fileSetting.FullPath,
        fileSetting.IdentifierInContainer,
        fileSetting.Footer,
        fileSetting.Header,
        fileSetting.EmptyAsNull,
        fileSetting.CodePageId,
        fileSetting.ByteOrderMark,
        fileSetting.ColumnCollection,
        "Test",
        fileSetting.Row,
        StandardTimeZoneAdjust.ChangeTimeZone,
        TimeZoneInfo.Local.Id, string.Empty, fileSetting.KeepUnencrypted);

      var result = await writer.WriteAsync(reader, UnitTestStatic.Token);
      Assert.AreEqual(7L, result);
    }


    [TestMethod]
    public async Task StructuredFileWriterXmlEncodeTest()
    {
      var fileSetting = new XmlFile("Write", "StructuredFileOutputXML.txt") { InOverview = true };
      var sb = new StringBuilder();

      var readFile = new CsvFile("ID", UnitTestStatic.GetTestPath("BasicCSV.txt")) { CommentLine = "#" };
      readFile.ColumnCollection.Add(new Column("ExamDate", new ValueFormat(DataTypeEnum.DateTime, @"dd/MM/yyyy")));
      readFile.ColumnCollection.Add(new Column("Score", new ValueFormat(DataTypeEnum.Integer)));
      readFile.ColumnCollection.Add(new Column("Proficiency", new ValueFormat(DataTypeEnum.Numeric)));
      readFile.ColumnCollection.Add(new Column("IsNativeLang", new ValueFormat(DataTypeEnum.Boolean), ignore: true));

      using var reader = FunctionalDI.FileReaderWriterFactory.GetFileReader(readFile, UnitTestStatic.Token);
      await reader.OpenAsync(UnitTestStatic.Token);

      sb.AppendLine("<?xml version=\"1.0\"?>\n");
      // ReSharper disable once StringLiteralTypo
      sb.AppendLine("<rowset>");
      fileSetting.Header = sb.ToString();
      sb.Clear();
      sb.AppendLine("  <row>");
      foreach (var col in reader.GetColumnsOfReader())
        sb.AppendFormat(
          "    <{0}>{1}</{0}>\n",
          HtmlStyle.XmlElementName(col.Name),
          string.Format(CultureInfo.InvariantCulture, StructuredFileWriter.cFieldPlaceholderByName, col.Name));

      sb.AppendLine("  </row>");
      fileSetting.Row = sb.ToString();
      // ReSharper disable once StringLiteralTypo
      fileSetting.Footer = "</rowset>";

      var writer = new XmlFileWriter(
        fileSetting.FullPath,
        fileSetting.IdentifierInContainer,
        fileSetting.Footer,
        fileSetting.Header,
        fileSetting.CodePageId,
        fileSetting.ByteOrderMark,
        fileSetting.ColumnCollection,
        "Test",
        fileSetting.Row,
        StandardTimeZoneAdjust.ChangeTimeZone,
        TimeZoneInfo.Local.Id,
        string.Empty, fileSetting.KeepUnencrypted);
      await writer.WriteAsync(reader, UnitTestStatic.Token);
    }
  }
}