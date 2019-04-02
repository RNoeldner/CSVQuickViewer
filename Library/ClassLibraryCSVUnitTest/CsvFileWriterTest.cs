using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CsvTools.Tests
{
  [TestClass]
  public class CsvFileWriterTest
  {
    private static CsvFile m_WriteFile;
    private static CsvFile m_ReadFile;

    private void Prc_Progress(object sender, ProgressEventArgs e)
    {
      throw new NotImplementedException();
    }

    [TestMethod]
    public void WriteFileLocked()
    {
      var dataTable = new DataTable
      {
        TableName = "DataTable",
        Locale = CultureInfo.InvariantCulture
      };
      dataTable.Columns.Add("ID", typeof(int));
      dataTable.Columns.Add("Text", typeof(string));
      for (int i = 0; i < 5; i++)
      {
        var row = dataTable.NewRow();
        row["ID"] = i;
        row["Text"] = i.ToString(CultureInfo.InvariantCulture);
        dataTable.Rows.Add(row);
      }
      var writeFile = new CsvFile
      {
        ID = "Test.txt",
        FileName = "WriteFileLocked.txt",
        InOverview = false,
        SqlStatement = "dummy"

      };
      FileSystemUtils.FileDelete(writeFile.FileName);
      using (System.IO.StreamWriter file = new System.IO.StreamWriter(writeFile.FullPath))
      {
        file.WriteLine("Hello");
        using (var processDisplay = new DummyProcessDisplay())
        {
          var writer = new CsvFileWriter(writeFile, processDisplay);

          writer.WriteDataTable(dataTable);
          Assert.IsTrue(!string.IsNullOrEmpty(writer.ErrorMessage));
        }
        file.WriteLine("World");
      }
      FileSystemUtils.FileDelete(writeFile.FileName);
    }

    [TestMethod]
    public void WriteDataTable()
    {
      var dataTable = new DataTable
      {
        TableName = "DataTable",
        Locale = CultureInfo.InvariantCulture
      };
      dataTable.Columns.Add("ID", typeof(int));
      dataTable.Columns.Add("Text", typeof(string));
      for (int i = 0; i < 100; i++)
      {
        var row = dataTable.NewRow();
        row["ID"] = i;
        row["Text"] = i.ToString(CultureInfo.CurrentCulture);
        dataTable.Rows.Add(row);
      }
      var writeFile = new CsvFile
      {
        ID = "Test.txt",
        FileName = "Test.txt",
        SqlStatement = "Hello"
      };
      using (var processDisplay = new DummyProcessDisplay())
      {

        var writer = new CsvFileWriter(writeFile, processDisplay);
        Assert.AreEqual(100, writer.WriteDataTable(dataTable));
      }
      Assert.IsTrue(File.Exists(writeFile.FullPath));
    }

    [TestInitialize]
    public void Init()
    {
      m_ReadFile = new CsvFile("BasicCSV.txt")
      {
        ID = "Read"
      };
      m_ReadFile.FileFormat.FieldDelimiter = ",";
      m_ReadFile.FileFormat.CommentLine = "#";

      var cf = m_ReadFile.ColumnCollection.AddIfNew(new Column { Name = "ExamDate", DataType = DataType.DateTime });
      cf.DateFormat = @"dd/MM/yyyy";
      m_ReadFile.ColumnCollection.AddIfNew(new Column { Name = "Score", DataType = DataType.Integer });
      m_ReadFile.ColumnCollection.AddIfNew(new Column { Name = "Proficiency", DataType = DataType.Numeric });
      m_ReadFile.ColumnCollection.AddIfNew(new Column { Name = "IsNativeLang", DataType = DataType.Boolean, Ignore = true });

      UnitTestInitialize.MimicSQLReader.AddSetting(m_ReadFile);

      m_WriteFile = new CsvFile { ID = "Write", SqlStatement = m_ReadFile.ID };

      m_WriteFile.ColumnCollection.AddIfNew(new Column { Name = "ExamDate", DataType = DataType.DateTime, DateFormat = @"MM/dd/yyyy", TimePart = "ExamTime" });
      m_WriteFile.ColumnCollection.AddIfNew(new Column { Name = "Proficiency", Ignore = true });
    }

    [TestMethod]
    public void Write()
    {
      var pd = new MockProcessDisplay();

      var writeFile = (CsvFile)m_WriteFile.Clone();
      writeFile.FileName = "BasicCSVOut.txt";
      FileSystemUtils.FileDelete(writeFile.FullPath);
      writeFile.FileFormat.FieldDelimiter = "|";

      var writer = new CsvFileWriter(writeFile, pd);
      Assert.IsTrue(string.IsNullOrEmpty(writer.ErrorMessage));

      var res = writer.Write();
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FullPath));
      Assert.AreEqual(7, res);
    }

    [TestMethod]
    public void WriteAllFormats()
    {
      var pd = new MockProcessDisplay();

      var writeFile = (CsvFile)m_WriteFile.Clone();
      writeFile.FileName = "BasicCSVOut2.txt";


      FileSystemUtils.FileDelete(writeFile.FullPath);
      var setting = Helper.ReaderGetAllFormats();

      UnitTestInitialize.MimicSQLReader.AddSetting(setting);
      writeFile.SqlStatement = setting.ID;
      writeFile.FileFormat.FieldDelimiter = "|";
      var cf = writeFile.ColumnCollection.AddIfNew(new Column { Name = "DateTime", DataType = DataType.DateTime });
      cf.DateFormat = "yyyyMMdd";
      cf.TimePartFormat = @"hh:mm";
      cf.TimePart = "Time";
      var writer = new CsvFileWriter(writeFile, pd);

      var res = writer.Write();
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FullPath));
      Assert.AreEqual(1065, res, "Records");
    }

    [TestMethod]
    public void WriteSameAsReader()
    {
      var writeFile = (CsvFile)m_WriteFile.Clone();
      writeFile.FileName = UnitTestInitialize.MimicSQLReader.ReadSettings.FirstOrDefault(x => x.ID == "Read").FileName;
      writeFile.FileFormat.FieldDelimiter = "|";
      using (var processDisplay = new DummyProcessDisplay())
      {
        var writer = new CsvFileWriter(writeFile, processDisplay);
        Assert.IsTrue(string.IsNullOrEmpty(writer.ErrorMessage));
        var res = writer.Write();
        Assert.IsFalse(string.IsNullOrEmpty(writer.ErrorMessage));
      }
    }

    [TestMethod]
    public void WriteSameAsReaderCritical()
    {
      var writeFile = (CsvFile)m_WriteFile.Clone();
      writeFile.FileName = UnitTestInitialize.MimicSQLReader.ReadSettings.FirstOrDefault(x => x.ID == "Read").FileName;
      writeFile.InOverview = true;
      writeFile.FileFormat.FieldDelimiter = "|";
      try
      {
        using (var processDisplay = new DummyProcessDisplay())
        {
          var writer = new CsvFileWriter(writeFile, processDisplay);
          var res = writer.Write();
        }
        Assert.Fail("No Exception");
      }
      catch (ApplicationException)
      {
      }
      catch (System.IO.IOException)
      {
      }
      catch (Exception)
      {
        Assert.Fail("Wrong exception");
      }
    }
  }
}