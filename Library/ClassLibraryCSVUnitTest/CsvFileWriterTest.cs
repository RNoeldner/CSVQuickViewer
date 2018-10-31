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
    private readonly CsvFile m_ReadFile = new CsvFile();
    private CsvFile m_WriteFile = new CsvFile();

    [TestMethod]
    public void ProcessDisplay()
    {
      var Schema = GetShema();
      var writer = new CsvFileWriter(m_WriteFile, CancellationToken.None);
      IProcessDisplay prc = new MockProcessDisplay
      {
        Maximum = 100
      };
      writer.ProcessDisplay = prc;
      int num = 0;
      prc.Progress += delegate (object sender, ProgressEventArgs e)
      {
        num = e.Value;
      };
      writer.HandleProgress("hello", 56);
      Assert.AreEqual(56, num);
    }

    private void Prc_Progress(object sender, ProgressEventArgs e)
    {
      throw new NotImplementedException();
    }

    [TestMethod]
    public void GetColumnInformationDT()
    {
      var Schema = GetShema();
      var cols = new CsvFileWriter(m_WriteFile, CancellationToken.None).GetColumnInformation(Schema, null);

      Assert.AreEqual(6, cols.Count());
      Assert.AreEqual("ID", cols.First().Header, "ID");
      Assert.AreEqual("LangCodeID", cols.ToArray()[1].Header, "LangCodeID");
      Assert.AreEqual("ExamDate", cols.ToArray()[2].Header, "ExamDate");
      Assert.AreEqual("ExamTime", cols.ToArray()[3].Header, "ExamTime");
    }

    [TestMethod]
    public void GetColumnInformationNoDT()
    {
      var cols = new CsvFileWriter(m_WriteFile, CancellationToken.None).GetColumnInformation(null, null);

      Assert.AreEqual(6, cols.Count());
      Assert.AreEqual("ID", cols.First().Header, "ID");
      Assert.AreEqual("LangCodeID", cols.ToArray()[1].Header, "LangCodeID");
      Assert.AreEqual("ExamDate", cols.ToArray()[2].Header, "ExamDate");
      Assert.AreEqual("ExamTime", cols.ToArray()[3].Header, "ExamTime");
    }

    [TestMethod]
    public void GetColumnInformationWithReadFormat()
    {
      var Schema = GetShema();
      var writer = new CsvFileWriter(m_WriteFile, CancellationToken.None);

      var cols = writer.GetColumnInformation(Schema, m_ReadFile);

      Assert.IsNull(writer.ErrorMessage, "Error Message");
      // IsNativeLang is gone
      Assert.AreEqual(5, cols.Count(), "Column Count");
      Assert.AreEqual("ID", cols.First().Header, "ID");
      Assert.AreEqual("LangCodeID", cols.ToArray()[1].Header, "LangCodeID");
      Assert.AreEqual("ExamDate", cols.ToArray()[2].Header, "ExamDate");
      Assert.AreEqual("ExamTime", cols.ToArray()[3].Header, "ExamTime");
    }

    [TestMethod]
    public void GetDataReaderSchema()
    {
      var Schema = GetShema();
      Assert.AreEqual(6, Schema.Rows.Count, "ColumnCount");
      Assert.AreEqual("ID", Schema.Rows[0]["ColumnName"], "Column1");
    }

    [TestMethod]
    public void GetDataReaderSchemaNull()
    {
      try
      {
        var Schema = new CsvFileWriter(null, CancellationToken.None).GetDataReaderSchema();
      }
      catch (ArgumentNullException)
      {
      }
      catch (NullReferenceException)
      {
      }
      catch
      {
        Assert.Fail();
      }
    }

    [TestMethod]
    public void GetSourceDataTableWriteFile0()
    {
      var writeFile = new CsvFile();
      writeFile.ID = "Write";
      writeFile.SourceSetting = "Read";

      var dt = new CsvFileWriter(writeFile, CancellationToken.None).GetSourceDataTable(10);
      Assert.AreEqual(7, dt.Rows.Count);
    }

    [TestMethod]
    public void GetSourceDataTableWriteFile1()
    {
      var writeFile = new CsvFile();
      writeFile.ID = "Write";
      writeFile.SourceSetting = "Read";

      var dt = new CsvFileWriter(m_WriteFile, CancellationToken.None).GetSourceDataTable(1);
      Assert.AreEqual(1, dt.Rows.Count);
    }

    [TestMethod]
    public void GetSourceDataTableWriteFileAllFormats()
    {
      var writeFile = new CsvFile
      {
        ID = "Write",
        SourceSetting = Helper.ReaderGetAllFormats(ApplicationSetting.ToolSetting).ID
      };

      var dt = new CsvFileWriter(writeFile, CancellationToken.None).GetSourceDataTable(0);
      Assert.AreEqual(1065, dt.Rows.Count);
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
        row["Text"] = i.ToString();
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

        var writer = new CsvFileWriter(writeFile, CancellationToken.None);
        writer.WriteDataTable(dataTable);

        Assert.IsTrue(!string.IsNullOrEmpty(writer.ErrorMessage));
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
        row["Text"] = i.ToString();
        dataTable.Rows.Add(row);
      }
      var writeFile = new CsvFile
      {
        ID = "Test.txt",
        FileName = Path.Combine(m_ApplicationDirectory, "Test.txt")
      };
      var writer = new CsvFileWriter(writeFile, CancellationToken.None);
      writer.WriteDataTable(dataTable);

      Assert.IsTrue(File.Exists(writeFile.FileName));
    }

    [TestInitialize]
    public void Init()
    {
      m_ReadFile.ID = "Read";
      m_ReadFile.FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt");
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

      m_WriteFile = new CsvFile();
      m_WriteFile.ID = "Write";
      m_WriteFile.SourceSetting = "Read";

      var add = new Column();
      add.Name = "ExamDate";
      add.DataType = DataType.DateTime;
      add.DateFormat = @"MM/dd/yyyy";
      add.TimePart = "ExamTime";
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

      writeFile.SourceSetting = "Read";
      writeFile.FileFormat.FieldDelimiter = "|";

      var writer = new CsvFileWriter(writeFile, CancellationToken.None);
      Assert.IsTrue(string.IsNullOrEmpty(writer.ErrorMessage));
      writer.Progress += pd.SetProcess;
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

      writeFile.SourceSetting = Helper.ReaderGetAllFormats(ApplicationSetting.ToolSetting).ID;

      writeFile.FileFormat.FieldDelimiter = "|";
      var cf = writeFile.ColumnAdd(new Column { Name = "DateTime", DataType = DataType.DateTime });
      cf.DateFormat = "yyyyMMdd";
      cf.TimePartFormat = @"hh:mm";
      cf.TimePart = "Time";
      var writer = new CsvFileWriter(writeFile, CancellationToken.None);
      writer.Progress += pd.SetProcess;
      var res = writer.Write();
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FileName));
      Assert.AreEqual(1065, res, "Records");
    }

    [TestMethod]
    [ExpectedException(typeof(ApplicationException))]
    public void WriteSameAsReader()
    {
      var writeFile = new CsvFile();
      writeFile.ID = "Write";
      writeFile.FileName = m_ReadFile.FileName;
      writeFile.SourceSetting = "Read";
      writeFile.FileFormat.FieldDelimiter = "|";

      var writer = new CsvFileWriter(writeFile, CancellationToken.None);

      var res = writer.Write();
      Assert.AreEqual(0, res);
    }

    [TestMethod]
    [ExpectedException(typeof(ApplicationException))]
    public void WriteSameAsReaderCritical()
    {
      var writeFile = new CsvFile();
      writeFile.ID = "Write";
      writeFile.FileName = m_ReadFile.FileName;
      writeFile.SourceSetting = "Read";
      writeFile.InOverview = true;
      writeFile.FileFormat.FieldDelimiter = "|";

      var writer = new CsvFileWriter(writeFile, CancellationToken.None);

      var res = writer.Write();
    }

    private DataTable GetShema()
    {
      return new CsvFileWriter(m_WriteFile, CancellationToken.None).GetDataReaderSchema();
    }
  }
}