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
    private const string cReadID = "StructuredFileWriterTests";

    [TestInitialize]
    public void Init()
    {
      var readFile = new CsvFile(cReadID, UnitTestStatic.GetTestPath("BasicCSV.txt")) { CommentLine = "#" };
      readFile.ColumnCollection.Add(new Column("ExamDate", new ValueFormat(DataTypeEnum.DateTime, @"dd/MM/yyyy")));
      readFile.ColumnCollection.Add(new Column("Score", new ValueFormat(DataTypeEnum.Integer)));
      readFile.ColumnCollection.Add(new Column("Proficiency", new ValueFormat(DataTypeEnum.Numeric)));
      readFile.ColumnCollection.Add(new Column("IsNativeLang", new ValueFormat(DataTypeEnum.Boolean), ignore: true));
      UnitTestStatic.MimicSQLReader.AddSetting(readFile);
    }

    [TestMethod]
    public async Task StructuredFileWriterJsonEncodeTestAsync()
    {
      UnitTestStatic.MimicSql();
      var fileSetting =
        new JsonFile("Write", "StructuredFileOutputJSON.txt") { SqlStatement = cReadID, InOverview = true };

      var sb = new StringBuilder("{");
      var progress = new Progress<ProgressInfo>();

      var cols = await fileSetting.SqlStatement.GetColumnsSqlAsync(fileSetting.Timeout,
        UnitTestStatic.Token);
      // ReSharper disable once StringLiteralTypo
      fileSetting.Header = "{\"rowset\":[\n";

      // { "firstName":"John", "lastName":"Doe"},
      foreach (var col in cols)
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
        0,
        fileSetting.KeepUnencrypted,
        fileSetting.IdentifierInContainer,
        fileSetting.Footer,
        fileSetting.Header,
        fileSetting.EmptyAsNull,
        fileSetting.CodePageId,
        fileSetting.ByteOrderMark,
        fileSetting.ColumnCollection,
        "Test",
        fileSetting.Row,
        StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id);

      var result = await writer.WriteAsync(
        fileSetting.SqlStatement,
        fileSetting.Timeout,
        progress,
        UnitTestStatic.Token);
      Assert.AreEqual(7L, result);
    }

#if XmlSerialization
    [TestMethod]
    public async Task StructuredFileWriterXmlEncodeTest()
    {
      var fileSetting =
        new XmlFile("Write", "StructuredFileOutputXML.txt") { SqlStatement = cReadID, InOverview = true };
      var sb = new StringBuilder();
      var progress = new Progress<ProgressInfo>();
      var cols = await fileSetting.SqlStatement.GetColumnsSqlAsync(fileSetting.Timeout, UnitTestStatic.Token);

      sb.AppendLine("<?xml version=\"1.0\"?>\n");
      // ReSharper disable once StringLiteralTypo
      sb.AppendLine("<rowset>");
      fileSetting.Header = sb.ToString();
      sb.Clear();
      sb.AppendLine("  <row>");
      foreach (var col in cols)
        sb.AppendFormat(
          "    <{0}>{1}</{0}>\n",
          HtmlStyle.XmlElementName(col.Name),
          string.Format(CultureInfo.InvariantCulture, StructuredFileWriter.cFieldPlaceholderByName, col.Name));

      sb.AppendLine("  </row>");
      fileSetting.Row = sb.ToString();
      // ReSharper disable once StringLiteralTypo
      fileSetting.Footer = "</rowset>";

      var writer = new XmlFileWriter(
        fileSetting.ID,
        fileSetting.FullPath,
        fileSetting.KeyID,
        fileSetting.KeepUnencrypted,
        fileSetting.IdentifierInContainer,
        fileSetting.Footer,
        fileSetting.Header,
        fileSetting.CodePageId,
        fileSetting.ByteOrderMark,
        fileSetting.ColumnCollection,
        "Test",
        fileSetting.Row,
        StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id);
      await writer.WriteAsync(fileSetting.SqlStatement, fileSetting.Timeout, progress, UnitTestStatic.Token);
    }
#endif
  }
}