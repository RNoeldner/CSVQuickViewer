using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Globalization;
using System.IO;

namespace CsvTools.Tests
{
  [TestClass()]
  public class XmlFileWriterTests
  {
    [TestMethod()]
    public async Task XmlFileWriterTest()
    {
      using var dataTable = new DataTable { TableName = "DataTable", Locale = CultureInfo.InvariantCulture };
      dataTable.Columns.Add("ID", typeof(int));
      dataTable.Columns.Add("Text", typeof(string));
      for (var i = 0; i < 100; i++)
      {
        var row = dataTable.NewRow();
        row["ID"] = i;
        row["Text"] = "Text" + i.ToString(CultureInfo.CurrentCulture);
        dataTable.Rows.Add(row);
      }

      var fn = UnitTestStatic.GetTestPath("Test.xml");
      var cols = new[] { new Column("ID", new ValueFormat(DataTypeEnum.Integer)), new Column("Text"), };
      (var h, var r)=  XmlFileWriter.GetXMLHeaderAndRow(cols);

      var writer = new XmlFileWriter(fn, "", "<rowset/>", h, 65001, true, Array.Empty<Column>(),
          "Display", r, StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, string.Empty, false);
      using (var reader = new DataTableWrapper(dataTable))
      {
        using (var output = File.OpenWrite(fn))
          await writer.WriteReaderAsync(reader, output, UnitTestStatic.Token);
        File.Delete(fn);
      }
    }
  }
}