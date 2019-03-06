using CsvTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace CvsTool
{
  [TestClass]
  public class ExcelDataReaderTestAutomation
  {
    private string m_ApplicationDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\TestFiles";
    private ExcelFile m_ValidSetting = new ExcelFile();

    [TestMethod]
    public void ExcelDataReader_ReadDate()
    {
      ExcelFile setting = new ExcelFile();
      setting.FileName = System.IO.Path.Combine(m_ApplicationDirectory, "DateTimeCombineExcel.xlsx");
      setting.SheetName = "Combinations";
      setting.ColumnAdd(new Column() { Name = "Date", DataType = DataType.DateTime, DateFormat = @"dd/MM/yyyy HH:mm;HH:mm", DateSeparator = ".", TimeSeparator = ":" });
      setting.TreatTextAsNull = string.Empty;

      using (ExcelFileReaderInterop test = new ExcelFileReaderInterop())
      {
        test.Open(setting, System.Threading.CancellationToken.None, true);
        Assert.IsFalse(test.IsClosed);
        int row = 0;
        while (test.Read())
        {
          row++;
          if (test.GetString(0) == null || test.GetString(0).Equals("Null", StringComparison.OrdinalIgnoreCase))
            Assert.AreEqual(DBNull.Value, test.GetValue(2));
          else
          {
            var val = test.GetValue(2);
            Assert.IsNotNull(val);
            if (!test.GetValue(0).ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
              Assert.IsInstanceOfType(val, typeof(DateTime), "Did not return DateTime for row " + row.ToString());
          }
        }
      }
    }

    [TestMethod]
    public void ExcelDataReader_ReadDateTime()
    {
      ExcelFile setting = new ExcelFile();
      setting.FileName = System.IO.Path.Combine(m_ApplicationDirectory, "DateTimeCombineExcel.xlsx");
      setting.SheetName = "Combinations";
      setting.ColumnAdd(new Column() { Name = "Date", DataType = DataType.DateTime, DateFormat = @"dd/MM/yyyy HH:mm;HH:mm", TimePart = @"Time", TimePartFormat = @"HH:mm;dd/MM/yyyy HH:mm", DateSeparator = ".", TimeSeparator = ":" });
      setting.ColumnAdd(new Column() { Name = "Result", DataType = DataType.DateTime });

      using (ExcelFileReaderInterop test = new ExcelFileReaderInterop())
      {
        test.Open(setting, CancellationToken.None, true);
        Assert.IsFalse(test.IsClosed, "Can not open file");
        int row = 0;
        while (test.Read())
        {
          row++;
          if (test.GetString(0) == null && test.GetString(1) == null)
            Assert.AreEqual(DBNull.Value, test.GetValue(2));
          else
          {
            var val = test.GetValue(2);
            var res = test.GetValue(4);
            if (val != DBNull.Value && res != DBNull.Value)
              Assert.AreEqual((DateTime)res, (DateTime)val, "Wrong result for row " + row.ToString());
          }
        }
      }
    }

    [TestMethod]
    public void ExcelDataReaderCreate_SheetWrong()
    {
      m_ValidSetting.SheetName = "****";
      using (ExcelFileReaderInterop test = new ExcelFileReaderInterop())
      {
        try
        {
          test.Open(m_ValidSetting, CancellationToken.None, true);
        }
        catch (ApplicationException)
        { }
        catch (Exception)
        {
          Assert.Fail("Wrong Exception Type");
        }

      }
    }

    [TestMethod]
    public void ExcelDataReaderCreateOK()
    {
      using (ExcelFileReaderInterop test = new ExcelFileReaderInterop())
      {
        test.Open(m_ValidSetting, CancellationToken.None, true);
        Assert.IsFalse(test.IsClosed);
      }
    }

    [TestMethod]
    public void ExcelDataReaderGetSchemaTable_DataConversion()
    {
      ExcelFile setting = new ExcelFile();
      setting.FileName = System.IO.Path.Combine(m_ApplicationDirectory, "BasicExcel.xls");
      setting.SheetName = "Basic";
      Column columnFormat = new Column() { Name = "IsNativeLang", DataType = DataType.Boolean };
      setting.ColumnAdd(columnFormat);
      using (ExcelFileReaderInterop test = new ExcelFileReaderInterop())
      {
        test.Open(m_ValidSetting, CancellationToken.None, true);
        var dt = test.GetSchemaTable();
        foreach (System.Data.DataRow dataRow in dt.Rows)
        {
          if (columnFormat.Name.Equals((string)dataRow[System.Data.Common.SchemaTableColumn.ColumnName], StringComparison.OrdinalIgnoreCase))
          {
            Assert.ReferenceEquals(dataRow[System.Data.Common.SchemaTableColumn.DataType], columnFormat.DataType.GetNetType());
          }
        }
      }
    }

    [TestMethod]
    public void ExcelDataReaderGetSchemaTable_NoDataConversion()
    {
      ExcelFile setting = new ExcelFile();
      setting.FileName = System.IO.Path.Combine(m_ApplicationDirectory, "BasicExcel.xls");
      setting.SheetName = "Basic";

      using (ExcelFileReaderInterop test = new ExcelFileReaderInterop())
      {
        test.Open(setting, CancellationToken.None, true);

        var dt = test.GetSchemaTable();

        foreach (System.Data.DataRow dataRow in dt.Rows)
        {
          if (((string)dataRow[System.Data.Common.SchemaTableColumn.ColumnName]).Equals("IsNativeLang", StringComparison.OrdinalIgnoreCase))
          {
            Assert.ReferenceEquals(dataRow[System.Data.Common.SchemaTableColumn.DataType], typeof(bool));
          }
        }
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), TestMethod]
    public void ExcelDataReaderOpenClose()
    {
      using (ExcelFileReaderInterop test = new ExcelFileReaderInterop())
      {
        test.Open(m_ValidSetting, CancellationToken.None, true);

        Assert.IsFalse(test.IsClosed);
        test.Close();
        Assert.IsTrue(test.IsClosed);
      }
    }

    [TestMethod]
    public void ExcelDataReaderRangeNoTrim()
    {
      m_ValidSetting.SheetName = "Trimming";
      m_ValidSetting.SheetRange = "B4:D9";
      m_ValidSetting.TrimmingOption = TrimmingOption.None;
      using (ExcelFileReaderInterop test = new ExcelFileReaderInterop())
      {
        test.Open(m_ValidSetting, CancellationToken.None, true);

        test.Read();
        // 1 German Fluent TRUE 01/01/2001
        Assert.AreEqual("1", test.GetString(0));
        // The natural Excel format is double But m_ValidSetting defines it as int, so an int should
        // be returned

        Assert.AreEqual(1, (int)test.GetValue(0));
        Assert.IsFalse(test.IsDBNull(0));

        Assert.AreEqual("German\n", test.GetString(1));
        Assert.AreEqual("  Fluent", test.GetString(2));

        test.Read();
        test.Read();
        Assert.AreEqual(" 4 ", test.GetString(0));
        Assert.AreEqual((int)4, (int)test.GetValue(0));
        Assert.AreEqual("German\n\n  \n\n", test.GetString(1));
      }
    }

    [TestMethod]
    public void ExcelDataReaderRangeTrim()
    {
      m_ValidSetting.SheetName = "Trimming";
      m_ValidSetting.SheetRange = "B4:D9";
      m_ValidSetting.TrimmingOption = TrimmingOption.All;
      using (ExcelFileReaderInterop test = new ExcelFileReaderInterop())
      {
        test.Open(m_ValidSetting, CancellationToken.None, true);
        test.Read();
        // 1 German Fluent TRUE 01/01/2001
        Assert.AreEqual("1", test.GetString(0));
        // The natural Excel format is double But m_ValidSetting defines it as int, so an int should
        // be returned
        Assert.AreEqual(1, (int)test.GetValue(0));
        Assert.IsFalse(test.IsDBNull(0));

        Assert.AreEqual("German", test.GetValue(1));
        Assert.AreEqual("Fluent", test.GetValue(2));

        test.Read();
        test.Read();
        Assert.AreEqual("4", test.GetString(0));
        Assert.AreEqual(4, (int)test.GetValue(0));
        Assert.AreEqual("German", test.GetValue(1));
      }
    }

    [TestMethod]
    public void ExcelDataReaderRead()
    {
      using (ExcelFileReaderInterop test = new ExcelFileReaderInterop())
      {
        test.Open(m_ValidSetting, CancellationToken.None, true);
        test.Read();
        // 1 German Fluent TRUE 01/01/2001
        Assert.AreEqual("1", test.GetString(0));
        Assert.AreEqual(1d, test.GetDouble(0));
        // The natural Excel format is double But m_ValidSetting defines it as int, so an int should
        // be returned
        Assert.AreEqual(1, (int)test.GetValue(0));

        Assert.IsFalse(test.IsDBNull(0));

        Assert.AreEqual("German", test.GetString(1));
        Assert.AreEqual("Fluent", test.GetString(2));
        Assert.AreEqual("True", test.GetString(3), true);
        Assert.IsTrue(test.GetBoolean(3));
        Assert.IsInstanceOfType(test.GetValue(3), typeof(Boolean));

        Assert.AreEqual(string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:d}", new DateTime(2001, 01, 01)), test.GetString(4));
        Assert.IsInstanceOfType(test.GetValue(4), typeof(DateTime));
      }
    }

    [TestMethod]
    public void ExcelDataReaderRead_GetSheets()
    {
      string[] sheets;
      using (IExcelFileReader reader = new ExcelFileReaderInterop())
      {
        reader.Open(new ExcelFile() { FileName = System.IO.Path.Combine(m_ApplicationDirectory, "Small.xlsx") }, CancellationToken.None, false);
        sheets = reader.Sheets;
      }

      Assert.AreEqual(3, sheets.Length);
      bool contains1 = false;
      bool contains2 = false;
      foreach (string sheetname in sheets)
      {
        if (sheetname.Equals("Sheet1", StringComparison.OrdinalIgnoreCase))
          contains1 = true;

        if (sheetname.Equals("No2", StringComparison.OrdinalIgnoreCase))
          contains2 = true;
      }
      Assert.IsTrue(contains1);
      Assert.IsTrue(contains2);
    }

    [TestMethod]
    public void ExcelDataReaderRead_GetSheets2()
    {
      string[] sheets;
      using (IExcelFileReader reader = new ExcelFileReaderInterop())
      {
        reader.Open(new ExcelFile() { FileName = System.IO.Path.Combine(m_ApplicationDirectory, "BasicExcel.xls") }, CancellationToken.None, false);
        sheets = reader.Sheets;
      }      
      Assert.AreEqual(1, sheets.Length);
      Assert.AreEqual("Basic", sheets[0]);
    }

    [TestMethod]
    public void ExcelDataReaderReadTreatTextNullAsNull()
    {
      ExcelFile setting = new ExcelFile();
      setting.FileName = System.IO.Path.Combine(m_ApplicationDirectory, "BasicExcel.xls");
      setting.SheetName = "Basic";
      // setting.TreatTextNullAsNull = true;
      using (ExcelFileReaderInterop test = new ExcelFileReaderInterop())
      {
        test.Open(setting, CancellationToken.None, true);

        test.Read();
        Assert.AreEqual(1, test.GetInt32(0));
        Assert.AreEqual("German", test.GetString(1));
        Assert.AreEqual(new DateTime(1901, 1, 5), test.GetDateTime(2));
        Assert.AreEqual(276d, test.GetDouble(3));
        Assert.AreEqual(0.94, test.GetDouble(4));
        Assert.AreEqual(true, test.GetBoolean(5));
        test.Read();
        test.Read();
        Assert.AreEqual(4, test.GetInt32(0));
        Assert.IsTrue(test.IsDBNull(1));
        Assert.IsTrue(string.IsNullOrEmpty(test.GetString(1)));
        Assert.AreEqual(DBNull.Value, test.GetValue(1));
        Assert.IsTrue(test.IsDBNull(2));
        Assert.AreEqual(true, test.GetBoolean(5));
        test.Read();
        test.Read();
        Assert.IsTrue(string.IsNullOrEmpty(test.GetString(1)));
        Assert.AreEqual(DBNull.Value, test.GetValue(1));
      }
    }

    [TestInitialize]
    public void Init()
    {
      m_ValidSetting.FileName = System.IO.Path.Combine(m_ApplicationDirectory, "Small.xlsx");
      m_ValidSetting.SheetName = "Sheet1";
      m_ValidSetting.ColumnAdd(new Column() { Name = "EmpID", DataType = DataType.Integer });
    }
  }
}