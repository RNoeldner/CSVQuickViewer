using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass]
  public class CsvFileWriterTest
  {
    private readonly string m_ApplicationDirectory = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles";

    private CsvFile m_WriteFile = new CsvFile();
    public MimicSQLReader mimicReader = new MimicSQLReader();

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
        FileName = Path.Combine(m_ApplicationDirectory, "WriteFileLocked.txt"),
        InOverview = false
      };
      FileSystemUtils.FileDelete(writeFile.FileName);
      using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(writeFile.FileName))
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
        FileName = Path.Combine(m_ApplicationDirectory, "Test.txt")
      };
      using (var processDisplay = new DummyProcessDisplay())
      {
        var writer = new CsvFileWriter(writeFile, processDisplay);
        writer.WriteDataTable(dataTable);
      }
      Assert.IsTrue(File.Exists(writeFile.FileName));
    }

    [TestInitialize]
    public void Init()
    {
      CsvFile m_ReadFile = new CsvFile
      {
        ID = "Read",
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt")
      };
      m_ReadFile.FileFormat.FieldDelimiter = ",";
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

      ApplicationSetting.ToolSetting.Input.Add(m_ReadFile);
      mimicReader.AddSetting(m_ReadFile);

      m_WriteFile = new CsvFile
      {
        ID = "Write",
        SqlStatement = m_ReadFile.ID
      };

      var add = new Column
      {
        Name = "ExamDate",
        DataType = DataType.DateTime,
        DateFormat = @"MM/dd/yyyy",
        TimePart = "ExamTime"
      };
      m_WriteFile.ColumnAdd(add);

      m_WriteFile.ColumnAdd(new Column
      {
        Name = "Proficiency",
        Ignore = true
      });
    }

    [TestMethod]
    public void Write()
    {
      var pd = new MockProcessDisplay();

      var writeFile = new CsvFile
      {
        ID = "Write",
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSVOut.txt")
      };

      FileSystemUtils.FileDelete(writeFile.FileName);

      writeFile.SqlStatement = "Read";
      writeFile.FileFormat.FieldDelimiter = "|";

      var writer = new CsvFileWriter(writeFile, pd);
      Assert.IsTrue(string.IsNullOrEmpty(writer.ErrorMessage));

      var res = writer.Write();
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FileName));
      Assert.AreEqual(7, res);
    }

    [TestMethod]
    public void WriteAllFormats()
    {
      var pd = new MockProcessDisplay();

      var writeFile = new CsvFile
      {
        ID = "Write",
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSVOut2.txt")
      };

      FileSystemUtils.FileDelete(writeFile.FileName);
      var setting = Helper.ReaderGetAllFormats(ApplicationSetting.ToolSetting);
      mimicReader.AddSetting(setting);
      writeFile.SqlStatement = setting.ID;

      writeFile.FileFormat.FieldDelimiter = "|";
      var cf = writeFile.ColumnAdd(new Column { Name = "DateTime", DataType = DataType.DateTime });
      cf.DateFormat = "yyyyMMdd";
      cf.TimePartFormat = @"hh:mm";
      cf.TimePart = "Time";
      var writer = new CsvFileWriter(writeFile, pd);

      var res = writer.Write();
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FileName));
      Assert.AreEqual(1065, res, "Records");
    }

    [TestMethod]
    public void WriteSameAsReader()
    {
      var writeFile = new CsvFile
      {
        ID = "Write",
        FileName = mimicReader.ReadSettings.FirstOrDefault(x => x.ID == "Read").FileName,
        SqlStatement = "Read"
      };
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
      var writeFile = new CsvFile
      {
        ID = "Write",
        FileName = mimicReader.ReadSettings.FirstOrDefault(x => x.ID == "Read").FileName,
        SqlStatement = "Read",
        InOverview = true
      };
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