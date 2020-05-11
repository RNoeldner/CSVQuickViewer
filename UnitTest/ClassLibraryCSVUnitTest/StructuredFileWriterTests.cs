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
      var readFile = new CsvFile { ID = c_ReadID, FileName = "BasicCSV.txt", FileFormat = { CommentLine = "#" } };
      readFile.ColumnCollection.AddIfNew(new Column("ExamDate", @"dd/MM/yyyy"));
      readFile.ColumnCollection.AddIfNew(new Column("Score", DataType.Integer));
      readFile.ColumnCollection.AddIfNew(new Column("Proficiency", DataType.Numeric));
      readFile.ColumnCollection.AddIfNew(new Column("IsNativeLang", DataType.Boolean) { Ignore = true });
      UnitTestInitialize.MimicSQLReader.AddSetting(readFile);
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
      using (var processDisplay = new DummyProcessDisplay())
      {
        var cols = DetermineColumnFormat.GetSourceColumnInformation(writeFile, processDisplay);
        writeFile.Header = "{\"rowset\":[\n";

        // { "firstName":"John", "lastName":"Doe"},
        foreach (var col in cols)
        {
          sb.AppendFormat("\"{0}\":\"{1}\", ", HTMLStyle.JsonElementName(col.Column.Name),
            string.Format(System.Globalization.CultureInfo.InvariantCulture, StructuredFileWriter.cFieldPlaceholderByName, col.Column.Name));
        }

        if (sb.Length > 1)
          sb.Length -= 2;
        sb.AppendLine("},");
        writeFile.Row = sb.ToString();
        var writer = new StructuredFileWriter(writeFile, TimeZoneInfo.Local.Id, processDisplay);
        await writer.WriteAsync();
      }
    }

    [TestMethod]
    public void StructuredFileWriterJSONEncodeTest()
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
      using (var processDisplay = new DummyProcessDisplay())
      {
        var cols = DetermineColumnFormat.GetSourceColumnInformation(writeFile, processDisplay);
        writeFile.Header = "{\"rowset\":[\n";

        // { "firstName":"John", "lastName":"Doe"},
        foreach (var col in cols)
        {
          sb.AppendFormat("\"{0}\":\"{1}\", ", HTMLStyle.JsonElementName(col.Column.Name),
            string.Format(System.Globalization.CultureInfo.InvariantCulture, StructuredFileWriter.cFieldPlaceholderByName, col.Column.Name));
        }

        if (sb.Length > 1)
          sb.Length -= 2;
        sb.AppendLine("},");
        writeFile.Row = sb.ToString();
        var writer = new StructuredFileWriter(writeFile, TimeZoneInfo.Local.Id, processDisplay);
        writer.Write();
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
      using (var processDisplay = new DummyProcessDisplay())
      {
        var cols = DetermineColumnFormat.GetSourceColumnInformation(writeFile, processDisplay);
        sb.AppendLine("<?xml version=\"1.0\"?>\n");
        sb.AppendLine("<rowset>");
        writeFile.Header = sb.ToString();
        sb.Clear();
        sb.AppendLine("  <row>");
        foreach (var col in cols)
        {
          sb.AppendFormat("    <{0}>{1}</{0}>\n", HTMLStyle.XmlElementName(col.Column.Name),
            string.Format(System.Globalization.CultureInfo.InvariantCulture, StructuredFileWriter.cFieldPlaceholderByName, col.Column.Name));
        }

        sb.AppendLine("  </row>");
        writeFile.Row = sb.ToString();
        writeFile.Footer = "</rowset>";

        var writer = new StructuredFileWriter(writeFile, TimeZoneInfo.Local.Id, processDisplay);
        await writer.WriteAsync();
      }
    }
  }
}