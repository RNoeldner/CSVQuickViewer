using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass]
  public class StructuredFileWriterTests
  {
    private readonly string m_ApplicationDirectory = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles";
#pragma warning disable CA1051 // Do not declare visible instance fields
    public MimicSQLReader mimicReader = new MimicSQLReader();
#pragma warning restore CA1051 // Do not declare visible instance fields

    [TestInitialize]
    public void Init()
    {
      var m_ReadFile = new CsvFile
      {
        ID = "Read",
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt")
      };
      var cf = m_ReadFile.ColumnAdd(new Column { Name = "ExamDate", DataType = DataType.DateTime });

      cf.DateFormat = @"dd/MM/yyyy";
      m_ReadFile.FileFormat.CommentLine = "#";
      m_ReadFile.ColumnAdd(new Column { Name = "Score", DataType = DataType.Integer });
      m_ReadFile.ColumnAdd(new Column { Name = "Proficiency", DataType = DataType.Numeric });

      m_ReadFile.ColumnAdd(new Column
      {
        Name = "IsNativeLang",
        DataType = DataType.Boolean,
        Ignore = true
      });

      mimicReader.AddSetting(m_ReadFile);
      ApplicationSetting.ToolSetting.Input.Clear();
      ApplicationSetting.ToolSetting.Input.Add(m_ReadFile);
    }

    [TestMethod]
    public void StructuredFileWriterJSONEncodeTest()
    {
      var writeFile = new StructuredFile
      {
        ID = "Write",
        FileName = Path.Combine(m_ApplicationDirectory, "StructuredFileOutputJSON.txt"),
        SqlStatement = "Read",
        InOverview = true,
        JSONEncode = true
      };
      var cols = DetermineColumnFormat.GetWriterSourceColumns(writeFile, CancellationToken.None);
      writeFile.Header = "{\"rowset\":[\n";
      var sb = new StringBuilder("{");
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
      var writer = new StructuredFileWriter(writeFile, CancellationToken.None);
      _ = writer.Write();
    }

    [TestMethod]
    public void StructuredFileWriterXMLEncodeTest()
    {
      var writeFile = new StructuredFile
      {
        ID = "Write",
        FileName = Path.Combine(m_ApplicationDirectory, "StructuredFileOutputXML.txt"),

        SqlStatement = "Read",
        InOverview = true,
        JSONEncode = false
      };
      var cols = DetermineColumnFormat.GetWriterSourceColumns(writeFile, CancellationToken.None);

      var sb = new StringBuilder();
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

      var writer = new StructuredFileWriter(writeFile, CancellationToken.None);
      _ = writer.Write();
    }
  }
}