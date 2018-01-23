using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class OldCsvFileTests
  {
    [TestMethod()]
    public void NewCsvFileTest()
    {
      var old1 = new OldCsvFile();
      old1.AlternateQuoting = true;
      old1.ByteOrderMark = true;
      old1.CodePageId = 5;
      Assert.AreEqual(5, old1.CodePageId);
      old1.CurrentEncoding = new UTF7Encoding();
      Assert.AreEqual(new UTF7Encoding(), old1.CurrentEncoding);
      old1.NumWarnings = 11;
      Assert.AreEqual(11, old1.NumWarnings);
      var col = new OldColumnFormat();
      col.DataType = DataType.Integer;
      old1.ColumnFormat.Add(col);

      var map = new OldFieldMapping();
      map.Source = "a";
      map.Destination = "b";

      old1.FieldMapping.Add(map);


      var new1 = old1.NewCsvFile();
      Assert.AreEqual(5, new1.CodePageId);
      Assert.AreEqual(new UTF7Encoding(), new1.CurrentEncoding);
      Assert.AreEqual(11, new1.NumWarnings);
      Assert.AreEqual(DataType.Integer, new1.Column.First().DataType);
     // Assert.AreEqual(map.Destination, new1.GetFieldMapping(map.Destination).TemplateField);
    }
  }
}