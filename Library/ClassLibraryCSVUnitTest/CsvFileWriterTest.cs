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

    private void Prc_Progress(object sender, ProgressEventArgs e) => throw new NotImplementedException();

    [TestMethod]
    public void WriteFileLocked()
    {
      using (var dataTable = new DataTable
      {
        TableName = "DataTable",
        Locale = CultureInfo.InvariantCulture
      })
      {
        dataTable.Columns.Add("ID", typeof(int));
        dataTable.Columns.Add("Text", typeof(string));
        for (var i = 0; i < 5; i++)
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
        using (var file = new System.IO.StreamWriter(writeFile.FullPath))
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
    }

    [TestMethod]
    public void WriteDataTable()
    {
      using (var dataTable = new DataTable
      {
        TableName = "DataTable",
        Locale = CultureInfo.InvariantCulture
      })
      {
        dataTable.Columns.Add("ID", typeof(int));
        dataTable.Columns.Add("Text", typeof(string));
        for (var i = 0; i < 100; i++)
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
    }

    [TestMethod]
    public void WriteDataTableHandleIssues()
    {
      using (var dataTable = new DataTable
      {
        TableName = "DataTable",
        Locale = CultureInfo.InvariantCulture
      })
      {
        dataTable.Columns.Add("ID", typeof(int));
        dataTable.Columns.Add("Text", typeof(string));
        for (var i = 0; i < 100; i++)
        {
          var row = dataTable.NewRow();
          row["ID"] = i;
          row["Text"] = "Text" + i.ToString(CultureInfo.CurrentCulture);
          dataTable.Rows.Add(row);
        }
        var writeFile = new CsvFile
        {
          ID = "Test.txt",
          FileName = "Test.txt",
          SqlStatement = "Hello"
        };
        writeFile.ColumnCollection.Add(new Column() { Name = "Text", DataType = DataType.Integer });
        writeFile.Header = "##This is a header for {FileName}";
        writeFile.Footer = "##This is a Footer\r\n{Records} in file";
        var count = 0;
        using (var processDisplay = new DummyProcessDisplay())
        {
          var writer = new CsvFileWriter(writeFile, processDisplay);
          writer.Warning += (object sender, WarningEventArgs e) => { count++; };
          Assert.AreEqual(100, writer.WriteDataTable(dataTable), "Records");
          Assert.AreEqual(100, count, "Warnings");
        }
        Assert.IsTrue(File.Exists(writeFile.FullPath));
      }
    }

    [TestMethod]
    public void GetSourceTableTable()
    {
      using (var dataTable = new DataTable
      {
        TableName = "DataTable",
        Locale = CultureInfo.InvariantCulture
      })
      {
        dataTable.Columns.Add("ID", typeof(int));
        dataTable.Columns.Add("Text", typeof(string));
        for (var i = 0; i < 100; i++)
        {
          var row = dataTable.NewRow();
          row["ID"] = i;
          row["Text"] = "Text" + i.ToString(CultureInfo.CurrentCulture);
          dataTable.Rows.Add(row);
        }
        var writeFile = new CsvFile
        {
          ID = "TestXYZ.txt",
          FileName = "Test.txt",
          SqlStatement = "Hello2"
        };
        UnitTestInitialize.MimicSQLReader.AddSetting("Hello2", dataTable);

        using (var processDisplay = new DummyProcessDisplay())
        {
          var writer = new CsvFileWriter(writeFile, processDisplay);
          var dt = writer.GetSourceDataTable(10);
          Assert.AreEqual(2, dt.Columns.Count);
        }
      }
    }

    [TestMethod]
    public void GetSchemaReader()
    {
      using (var dataTable = new DataTable { TableName = "DataTable", Locale = CultureInfo.InvariantCulture })
      {
        dataTable.Columns.Add("ID", typeof(int));
        dataTable.Columns.Add("Text", typeof(string));
        for (var i = 0; i < 100; i++)
        {
          var row = dataTable.NewRow();
          row["ID"] = i;
          row["Text"] = "Text" + i.ToString(CultureInfo.CurrentCulture);
          dataTable.Rows.Add(row);
        }
        var writeFile = new CsvFile
        {
          ID = "TestXYZ.txt",
          FileName = "Test.txt",
          SqlStatement = "SELECT * FROM Hello2"
        };
        UnitTestInitialize.MimicSQLReader.AddSetting("Hello2", dataTable);

        using (var processDisplay = new DummyProcessDisplay())
        {
          var writer = new CsvFileWriter(writeFile, processDisplay);
          var dt = writer.GetSchemaReader();
        }
      }
    }

    private void Writer_Warning(object sender, WarningEventArgs e) => throw new NotImplementedException();

    [TestInitialize]
    public void Init()
    {
      m_ReadFile = new CsvFile("BasicCSV.txt") { ID = "Read", FileFormat = { FieldDelimiter = ",", CommentLine = "#" } };

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
    public void WriteGZip()
    {
      var pd = new MockProcessDisplay();

      var writeFile = (CsvFile)m_WriteFile.Clone();
      writeFile.FileName = "BasicCSVOut.txt.gz";
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
      cf.TimeZonePart = "\"UTC\"";
      var writer = new CsvFileWriter(writeFile, pd);

      var res = writer.Write();
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FullPath));
      Assert.AreEqual(1065, res, "Records");
    }

    [TestMethod]
    public void TimeZoneConversions()
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
      cf.TimeZonePart = "TZ";
      var writer = new CsvFileWriter(writeFile, pd);

      var res = writer.Write();
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FullPath));
      Assert.AreEqual(1065, res, "Records");
    }

    [TestMethod]
    public void WriteSameAsReader()
    {
      var writeFile = (CsvFile)m_WriteFile.Clone();
      writeFile.FileName = UnitTestInitialize.MimicSQLReader.ReadSettings.OfType<IFileSettingPhysicalFile>().FirstOrDefault(x => x.ID == "Read").FileName;
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
      writeFile.FileName = UnitTestInitialize.MimicSQLReader.ReadSettings.OfType<IFileSettingPhysicalFile>().FirstOrDefault(x => x.ID == "Read").FileName;
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
      catch (FileWriterException)
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