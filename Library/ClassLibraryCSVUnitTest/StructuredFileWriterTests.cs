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
      var cf = m_ReadFile.ColumnCollection.AddIfNew(new Column { Name = "ExamDate", DataType = DataType.DateTime , DateFormat = @"dd/MM/yyyy" });            
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
        var cols = DetermineColumnFormat.GetWriterSourceColumns(writeFile, processDisplay);
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
        var cols = DetermineColumnFormat.GetWriterSourceColumns(writeFile, processDisplay);
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