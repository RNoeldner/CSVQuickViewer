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

namespace CsvTools.Tests
{
  using System;
  using System.Data;
  using System.Globalization;
  using System.IO;
  using System.Threading.Tasks;

  using Microsoft.VisualStudio.TestTools.UnitTesting;

  using DataTableReader = CsvTools.DataTableReader;

  [TestClass]
  public class CsvFileWriterTest
  {
    private static CsvFile m_ReadFile;

    private static CsvFile m_WriteFile;

    [TestInitialize]
    public void Init()
    {
      m_ReadFile =
        new CsvFile("BasicCSV.txt") { ID = "Read", FileFormat = { FieldDelimiter = ",", CommentLine = "#" } };

      m_ReadFile.ColumnCollection.AddIfNew(new Column("ExamDate", new ValueFormat(DataType.DateTime) { DateFormat = @"dd/MM/yyyy" }));
      m_ReadFile.ColumnCollection.AddIfNew(new Column("Score", DataType.Integer));
      m_ReadFile.ColumnCollection.AddIfNew(new Column("Proficiency", DataType.Numeric));
      m_ReadFile.ColumnCollection.AddIfNew(new Column("IsNativeLang", DataType.Boolean) { Ignore = true });

      UnitTestInitialize.MimicSQLReader.AddSetting(m_ReadFile);

      m_WriteFile = new CsvFile { ID = "Write", SqlStatement = m_ReadFile.ID };

      m_WriteFile.ColumnCollection.AddIfNew(new Column("ExamDate", @"MM/dd/yyyy") { TimePart = "ExamTime" });
      m_WriteFile.ColumnCollection.AddIfNew(new Column { Name = "Proficiency", Ignore = true });
    }

    [TestMethod]
    public async Task TimeZoneConversionsAsync()
    {
      var pd = new MockProcessDisplay();

      var writeFile = (CsvFile)m_WriteFile.Clone();
      writeFile.FileName = "BasicCSVOut2tzc.txt";

      FileSystemUtils.FileDelete(writeFile.FullPath);
      var setting = Helper.ReaderGetAllFormats();

      UnitTestInitialize.MimicSQLReader.AddSetting(setting);
      writeFile.SqlStatement = setting.ID;
      writeFile.FileFormat.FieldDelimiter = "|";
      writeFile.ColumnCollection.AddIfNew(
        new Column("DateTime", new ValueFormat(DataType.DateTime) {DateFormat = "yyyyMMdd"})
        {
          TimePartFormat = @"hh:mm",
          TimePart = "Time",
          TimeZonePart = "TZ"
        });
      var writer = new CsvFileWriter(writeFile, TimeZoneInfo.Local.Id, BaseSettings.ZeroTime, BaseSettings.ZeroTime, pd);

      var res = await writer.WriteAsync(pd.CancellationToken);
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FullPath));
      Assert.AreEqual(1065, res, "Records");
    }

    [TestMethod]
    public async Task Write()
    {
      var pd = new MockProcessDisplay();

      var writeFile = (CsvFile)m_WriteFile.Clone();
      writeFile.FileName = "BasicCSVOut.txt";
      FileSystemUtils.FileDelete(writeFile.FullPath);
      writeFile.FileFormat.FieldDelimiter = "|";

      var writer = new CsvFileWriter(writeFile, TimeZoneInfo.Local.Id, BaseSettings.ZeroTime, BaseSettings.ZeroTime, pd);
      Assert.IsTrue(string.IsNullOrEmpty(writer.ErrorMessage));

      var res = await writer.WriteAsync(pd.CancellationToken);
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FullPath));
      Assert.AreEqual(7, res);
    }

    [TestMethod]
    public async Task WriteAllFormatsAsync()
    {
      var pd = new MockProcessDisplay();

      var writeFile = (CsvFile)m_WriteFile.Clone();
      writeFile.FileName = "BasicCSVOut2.txt";

      FileSystemUtils.FileDelete(writeFile.FullPath);
      var setting = Helper.ReaderGetAllFormats();

      UnitTestInitialize.MimicSQLReader.AddSetting(setting);
      writeFile.SqlStatement = setting.ID;
      writeFile.FileFormat.FieldDelimiter = "|";

      var cf = new Column("DateTime", DataType.DateTime)
      {
        TimePartFormat = @"hh:mm", TimePart = "Time", TimeZonePart = "\"UTC\""
      };
      cf.ValueFormatMutable.DateFormat = "yyyyMMdd";
      writeFile.ColumnCollection.AddIfNew(cf);
      var writer = new CsvFileWriter(writeFile, TimeZoneInfo.Local.Id, BaseSettings.ZeroTime, BaseSettings.ZeroTime, pd);

      var res = await writer.WriteAsync(pd.CancellationToken);
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FullPath));
      Assert.AreEqual(1065, res, "Records");
    }

    [TestMethod]
    public async Task WriteDataTableAsync()
    {
      using (var dataTable = new DataTable { TableName = "DataTable", Locale = CultureInfo.InvariantCulture })
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

        var writeFile = new CsvFile { ID = "Test.txt", FileName = "Test.txt", SqlStatement = "Hello" };
        using (var processDisplay = new DummyProcessDisplay())
        {
          var writer = new CsvFileWriter(writeFile, TimeZoneInfo.Local.Id, BaseSettings.ZeroTime, BaseSettings.ZeroTime, processDisplay);
          using (var reader = new DataTableReader(dataTable, "dummy", processDisplay))
          {
            await reader.OpenAsync(processDisplay.CancellationToken);
            Assert.AreEqual(100, await writer.WriteAsync(reader, processDisplay.CancellationToken));
          }
            
        }

        Assert.IsTrue(File.Exists(writeFile.FullPath));
      }
    }

    [TestMethod]
    public async Task WriteDataTableHandleIssuesAsync()
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

        var writeFile = new CsvFile { ID = "Test.txt", FileName = "Test.txt", SqlStatement = "Hello" };
        writeFile.ColumnCollection.Add(new Column("Text", DataType.Integer));
        writeFile.Header = "##This is a header for {FileName}";
        writeFile.Footer = "##This is a Footer\r\n{Records} in file";
        var count = 0;
        using (var processDisplay = new DummyProcessDisplay())
        {
          var writer = new CsvFileWriter(writeFile, TimeZoneInfo.Local.Id, BaseSettings.ZeroTime, BaseSettings.ZeroTime, processDisplay);
          writer.Warning += (object sender, WarningEventArgs e) => { count++; };
          using (var reader = new DataTableReader(dataTable, "dummy", processDisplay))
          {
            await reader.OpenAsync(processDisplay.CancellationToken);
            Assert.AreEqual(100, await writer.WriteAsync(reader, processDisplay.CancellationToken), "Records");
          }
            
          Assert.AreEqual(100, count, "Warnings");
        }

        Assert.IsTrue(File.Exists(writeFile.FullPath));
      }
    }

    [TestMethod]
    public async Task WriteFileLocked()
    {
      using (var dataTable = new DataTable { TableName = "DataTable", Locale = CultureInfo.InvariantCulture })
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
        FileSystemUtils.FileDelete(writeFile.FullPath);
        using (var file = new StreamWriter(writeFile.FullPath))
        {
          await file.WriteLineAsync("Hello");
          using (var processDisplay = new DummyProcessDisplay())
          {
            var writer = new CsvFileWriter(writeFile, TimeZoneInfo.Local.Id, BaseSettings.ZeroTime, BaseSettings.ZeroTime, processDisplay);
            using (var reader = new DataTableReader(dataTable, "dummy", processDisplay))
              await writer.WriteAsync(reader, processDisplay.CancellationToken);

            Assert.IsTrue(!string.IsNullOrEmpty(writer.ErrorMessage));
          }

          await file.WriteLineAsync("World");
        }

        FileSystemUtils.FileDelete(writeFile.FileName);
      }
    }

    [TestMethod]
    public async Task WriteGZipAsync()
    {
      var pd = new MockProcessDisplay();

      var writeFile = (CsvFile)m_WriteFile.Clone();
      writeFile.FileName = "BasicCSVOut.txt.gz";
      FileSystemUtils.FileDelete(writeFile.FullPath);
      writeFile.FileFormat.FieldDelimiter = "|";

      var writer = new CsvFileWriter(writeFile, TimeZoneInfo.Local.Id, BaseSettings.ZeroTime, BaseSettings.ZeroTime, pd);
      Assert.IsTrue(string.IsNullOrEmpty(writer.ErrorMessage));

      var res = await writer.WriteAsync(pd.CancellationToken);
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FullPath));
      Assert.AreEqual(7, res);
    }

    private void Prc_Progress(object sender, ProgressEventArgs e) => throw new NotImplementedException();

    private void Writer_Warning(object sender, WarningEventArgs e) => throw new NotImplementedException();
  }
}