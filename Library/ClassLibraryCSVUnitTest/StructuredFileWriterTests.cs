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
using System.Text;

namespace CsvTools.Tests
{
  [TestClass]
  public class StructuredFileWriterTests
  {
    private const string ReadID = "StructuredFileWriterTests";

    [TestInitialize]
    public void Init()
    {
      var m_ReadFile = new CsvFile
      {
        ID = ReadID,
        FileName = "BasicCSV.txt"
      };
      m_ReadFile.FileFormat.CommentLine = "#";
      _ = m_ReadFile.ColumnCollection.AddIfNew(new Column { Name = "ExamDate", DataType = DataType.DateTime, DateFormat = @"dd/MM/yyyy" });
      m_ReadFile.ColumnCollection.AddIfNew(new Column { Name = "Score", DataType = DataType.Integer });
      m_ReadFile.ColumnCollection.AddIfNew(new Column { Name = "Proficiency", DataType = DataType.Numeric });
      m_ReadFile.ColumnCollection.AddIfNew(new Column { Name = "IsNativeLang", DataType = DataType.Boolean, Ignore = true });
      UnitTestInitialize.MimicSQLReader.AddSetting(m_ReadFile);
    }

    [TestMethod]
    public void StructuredFileWriterJSONEncodeTest()
    {
      var writeFile = new StructuredFile
      {
        ID = "Write",
        FileName = "StructuredFileOutputJSON.txt",
        SqlStatement = ReadID,
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
          sb.AppendFormat("\"{0}\":\"{1}\", ", HTMLStyle.JsonElementName(col.Header),
            string.Format(System.Globalization.CultureInfo.InvariantCulture, StructuredFileWriter.cFieldPlaceholderByName, col.Header));
        }

        if (sb.Length > 1)
          sb.Length -= 2;
        sb.AppendLine("},");
        writeFile.Row = sb.ToString();
        var writer = new StructuredFileWriter(writeFile, processDisplay);
        _ = writer.Write();
      }
    }

    [TestMethod]
    public void StructuredFileWriterXMLEncodeTest()
    {
      var writeFile = new StructuredFile
      {
        ID = "Write",
        FileName = "StructuredFileOutputXML.txt",
        SqlStatement = ReadID,
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
          sb.AppendFormat("    <{0}>{1}</{0}>\n", HTMLStyle.XmlElementName(col.Header),
            string.Format(System.Globalization.CultureInfo.InvariantCulture, StructuredFileWriter.cFieldPlaceholderByName, col.Header));
        }

        sb.AppendLine("  </row>");
        writeFile.Row = sb.ToString();
        writeFile.Footer = "</rowset>";

        var writer = new StructuredFileWriter(writeFile, processDisplay);
        _ = writer.Write();
      }
    }
  }
}