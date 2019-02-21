using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass]
  public class CsvDataReaderPGPFilesTest
  {
    [TestMethod]
    public void ReadGZip()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = true,
        AlternateQuoting = true
      };
      setting.FileName = "TestFiles\\BasicCSV.txt.gz";
      setting.ColumnAdd(new Column
      {
        Name = "ExamDate",
        DataType = DataType.DateTime,
        DateFormat = @"dd/MM/yyyy"
      });
      setting.ColumnAdd(new Column
      {
        Name = "ID",
        DataType = DataType.Integer
      });
      setting.ColumnAdd(new Column
      {
        Name = "IsNativeLang",
        DataType = DataType.Boolean
      });
      using (var test = new CsvFileReader(setting))
      {
        test.Open(true, CancellationToken.None);
        int row = 0;
        while (test.Read())
          row++;
        Assert.AreEqual(row, test.RecordNumber);
        Assert.AreEqual(7, row);
      }
    }

    [TestMethod]
    public void ReadPGP()

    {
      var setting = new CsvFile
      {
        HasFieldHeader = true,
        AlternateQuoting = true
      };
      PGPKeyStorageTestHelper.SetApplicationSetting();

      setting.FileName = "TestFiles\\BasicCSV.pgp";
      setting.ColumnAdd(new Column
      {
        Name = "ExamDate",
        DataType = DataType.DateTime,
        DateFormat = @"dd/MM/yyyy"
      });
      setting.ColumnAdd(new Column
      {
        Name = "ID",
        DataType = DataType.Integer
      });
      setting.ColumnAdd(new Column
      {
        Name = "IsNativeLang",
        DataType = DataType.Boolean
      });
      using (var test = new CsvFileReader(setting))
      {
        test.Open(true, CancellationToken.None);
        int row = 0;
        while (test.Read())
          row++;
        Assert.AreEqual(row, test.RecordNumber);
        Assert.AreEqual(7, row);
      }
    }

    [TestMethod]
    public void ReadGPG()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = true,
        AlternateQuoting = true
      };
      PGPKeyStorageTestHelper.SetApplicationSetting();

      setting.FileName = "TestFiles\\BasicCSV.pgp";
      setting.ColumnAdd(new Column
      {
        Name = "ExamDate",
        DataType = DataType.DateTime,
        DateFormat = @"dd/MM/yyyy"
      });
      setting.ColumnAdd(new Column
      {
        Name = "ID",
        DataType = DataType.Integer
      });
      setting.ColumnAdd(new Column
      {
        Name = "IsNativeLang",
        DataType = DataType.Boolean
      });
      using (var test = new CsvFileReader(setting))
      {
        test.Open(true, CancellationToken.None);
        int row = 0;
        while (test.Read())
          row++;
        Assert.AreEqual(row, test.RecordNumber);
        Assert.AreEqual(7, row);
      }
    }
  }
}