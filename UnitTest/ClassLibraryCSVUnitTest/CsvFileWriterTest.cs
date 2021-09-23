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
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class CsvFileWriterTest
  {
    private static readonly CsvFile m_ReadFile;

    private static readonly CsvFile m_WriteFile;

    static CsvFileWriterTest()
    {
      m_ReadFile =
        new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt"))
        {
          ID = "Read",
          FileFormat = { FieldDelimiter = ",", CommentLine = "#" }
        };

      m_ReadFile.ColumnCollection.Add(new Column("ExamDate",
        new ValueFormatMutable() { DataType = DataType.DateTime, DateFormat = @"dd/MM/yyyy" }));
      m_ReadFile.ColumnCollection.Add(new Column("Score", DataType.Integer));
      m_ReadFile.ColumnCollection.Add(new Column("Proficiency", DataType.Numeric));
      m_ReadFile.ColumnCollection.Add(new Column("IsNativeLang", DataType.Boolean) { Ignore = true });

      UnitTestStatic.MimicSQLReader.AddSetting(m_ReadFile);

      m_WriteFile = new CsvFile { ID = "Write", SqlStatement = m_ReadFile.ID };

      m_WriteFile.ColumnCollection.Add(new Column("ExamDate", @"MM/dd/yyyy") { TimePart = "ExamTime" });
      m_WriteFile.ColumnCollection.Add(new Column { Name = "Proficiency", Ignore = true });
    }

    [TestMethod]
    [Timeout(2000)]
    public async Task TimeZoneConversionsAsync()
    {
      var pd = new MockProcessDisplay();

      var writeFile = (CsvFile) m_WriteFile.Clone();
      writeFile.FileName =  UnitTestStatic.GetTestPath("BasicCSVOut2tzc.txt");

      FileSystemUtils.FileDelete(writeFile.FileName);
      var setting = UnitTestStatic.ReaderGetAllFormats();

      UnitTestStatic.MimicSQLReader.AddSetting(setting);
      writeFile.SqlStatement = setting.ID;
      writeFile.FileFormat.FieldDelimiter = "|";
      writeFile.ColumnCollection.Add(
        new Column("DateTime", new ValueFormatMutable() { DataType = DataType.DateTime, DateFormat = "yyyyMMdd" })
        {
          TimePartFormat = @"hh:mm",
          TimePart = "Time",
          TimeZonePart = "TZ"
        });
      var writer = new CsvFileWriter(writeFile.ID, writeFile.FullPath, writeFile.HasFieldHeader, writeFile.FileFormat.ValueFormatMutable, writeFile.FileFormat, writeFile.CodePageId,
        writeFile.ByteOrderMark, writeFile.ColumnCollection, writeFile.Recipient, writeFile.KeepUnencrypted, writeFile.IdentifierInContainer,
        writeFile.Header, writeFile.Footer);

      var res = await writer.WriteAsync(writeFile.SqlStatement, 360, null, pd.CancellationToken);
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FileName));
      Assert.AreEqual(1065, res, "Records");
    }

    [TestMethod]
    public async Task Write()
    {
      var pd = new MockProcessDisplay();

      var writeFile = (CsvFile) m_WriteFile.Clone();
      writeFile.FileName = UnitTestStatic.GetTestPath("BasicCSVOut.txt");
      FileSystemUtils.FileDelete(writeFile.FileName);
      writeFile.FileFormat.FieldDelimiter = "|";

      var writer = new CsvFileWriter(writeFile.ID, writeFile.FullPath, writeFile.HasFieldHeader, writeFile.FileFormat.ValueFormatMutable, writeFile.FileFormat, writeFile.CodePageId,
        writeFile.ByteOrderMark, writeFile.ColumnCollection, writeFile.Recipient, writeFile.KeepUnencrypted, writeFile.IdentifierInContainer,
        writeFile.Header, writeFile.Footer);

      var res = await writer.WriteAsync(writeFile.SqlStatement, 360, null, pd.CancellationToken);
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FileName));
      Assert.AreEqual(7, res);
    }

    [TestMethod]
    public async Task WriteAllFormatsAsync()
    {
      var pd = new MockProcessDisplay();

      var writeFile = (CsvFile) m_WriteFile.Clone();
      writeFile.FileName = UnitTestStatic.GetTestPath("BasicCSVOut2.txt");

      FileSystemUtils.FileDelete(writeFile.FileName);
      var setting = UnitTestStatic.ReaderGetAllFormats();

      UnitTestStatic.MimicSQLReader.AddSetting(setting);
      writeFile.SqlStatement = setting.ID;
      writeFile.FileFormat.FieldDelimiter = "|";

      var cf = new Column("DateTime", DataType.DateTime)
      {
        TimePartFormat = @"hh:mm",
        TimePart = "Time",
        TimeZonePart = "\"UTC\""
      };
      cf.ValueFormatMutable.DateFormat = "yyyyMMdd";
      writeFile.ColumnCollection.Add(cf);
      var writer = new CsvFileWriter(writeFile.ID, writeFile.FullPath, writeFile.HasFieldHeader, writeFile.FileFormat.ValueFormatMutable, writeFile.FileFormat, writeFile.CodePageId,
        writeFile.ByteOrderMark, writeFile.ColumnCollection, writeFile.Recipient, writeFile.KeepUnencrypted, writeFile.IdentifierInContainer,
        writeFile.Header, writeFile.Footer);

      var res = await writer.WriteAsync(writeFile.SqlStatement, 360, null, pd.CancellationToken);
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FileName));
      Assert.AreEqual(1065, res, "Records");
    }

    [TestMethod]
    public async Task WriteDataTableAsync()
    {
      using var dataTable = new DataTable { TableName = "DataTable", Locale = CultureInfo.InvariantCulture };
      dataTable.Columns.Add("ID", typeof(int));
      dataTable.Columns.Add("Text", typeof(string));
      for (var i = 0; i < 100; i++)
      {
        var row = dataTable.NewRow();
        row["ID"] = i;
        row["Text"] = i.ToString(CultureInfo.CurrentCulture);
        dataTable.Rows.Add(row);
      }

      var writeFile = new CsvFile { ID = "Test.txt", FileName =  UnitTestStatic.GetTestPath("Test.txt"), SqlStatement = "Hello" };
      using (var processDisplay = new CustomProcessDisplay(UnitTestStatic.Token))
      {
        var writer = new CsvFileWriter(writeFile.ID, writeFile.FullPath, writeFile.HasFieldHeader, writeFile.FileFormat.ValueFormatMutable, writeFile.FileFormat, writeFile.CodePageId,
        writeFile.ByteOrderMark, writeFile.ColumnCollection, writeFile.Recipient, writeFile.KeepUnencrypted, writeFile.IdentifierInContainer,
        writeFile.Header, writeFile.Footer);
        using var reader = new DataTableWrapper(dataTable);
        // await reader.OpenAsync(processDisplay.CancellationToken);
        Assert.AreEqual(100, await writer.WriteAsync(reader, processDisplay.CancellationToken));
      }

      Assert.IsTrue(File.Exists(writeFile.FileName));
    }

    [TestMethod]
    public async Task WriteDataTableHandleIssuesAsync()
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

      var writeFile = new CsvFile { ID = "Test.txt", FileName =  UnitTestStatic.GetTestPath("Test.txt"), SqlStatement = "Hello" };
      writeFile.ColumnCollection.Add(new Column("Text", DataType.Integer));
      writeFile.Header = "##This is a header for {FileName}";
      writeFile.Footer = "##This is a Footer\r\n{Records} in file";
      var count = 0;
      using (var processDisplay = new CustomProcessDisplay(UnitTestStatic.Token))
      {
        var writer = new CsvFileWriter(writeFile.ID, writeFile.FullPath, writeFile.HasFieldHeader, writeFile.FileFormat.ValueFormatMutable, writeFile.FileFormat, writeFile.CodePageId,
        writeFile.ByteOrderMark, writeFile.ColumnCollection, writeFile.Recipient, writeFile.KeepUnencrypted, writeFile.IdentifierInContainer,
        writeFile.Header, writeFile.Footer);
        writer.Warning += (sender, e) => { count++; };
        using (var reader = new DataTableWrapper(dataTable))
        {
          // await reader.OpenAsync(processDisplay.CancellationToken);
          Assert.AreEqual(100, await writer.WriteAsync(reader, processDisplay.CancellationToken), "Records");
        }

        Assert.AreEqual(100, count, "Warnings");
      }

      Assert.IsTrue(File.Exists(writeFile.FileName));
    }

    [TestMethod]
    public async Task WriteFileLocked()
    {
      using var dataTable = new DataTable { TableName = "DataTable", Locale = CultureInfo.InvariantCulture };
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
        FileName =  UnitTestStatic.GetTestPath("WriteFileLocked.txt"),
        InOverview = false,
        SqlStatement = "dummy"
      };
      FileSystemUtils.FileDelete(writeFile.FileName);
      using (var file = new StreamWriter(writeFile.FileName))
      {
        await file.WriteLineAsync("Hello");
        try
        {
          using (var processDisplay = new CustomProcessDisplay(UnitTestStatic.Token))
          {
            var writer = new CsvFileWriter(writeFile.ID, writeFile.FullPath, writeFile.HasFieldHeader, writeFile.FileFormat.ValueFormatMutable, writeFile.FileFormat, writeFile.CodePageId,
        writeFile.ByteOrderMark, writeFile.ColumnCollection, writeFile.Recipient, writeFile.KeepUnencrypted, writeFile.IdentifierInContainer,
        writeFile.Header, writeFile.Footer);
            using var reader = new DataTableWrapper(dataTable);
            await writer.WriteAsync(reader, processDisplay.CancellationToken);
          }

          Assert.Fail("Exception not thrown");
        }
        catch (FileWriterException)
        {
        }

        await file.WriteLineAsync("World");
      }

      FileSystemUtils.FileDelete(writeFile.FileName);
    }

    [TestMethod]
    public async Task WriteGZipAsync()
    {
      var pd = new MockProcessDisplay();

      var writeFile = (CsvFile) m_WriteFile.Clone();
      writeFile.FileName = UnitTestStatic.GetTestPath("BasicCSVOut.txt.gz");
      FileSystemUtils.FileDelete(writeFile.FileName);
      writeFile.FileFormat.FieldDelimiter = "|";

      var writer = new CsvFileWriter(writeFile.ID, writeFile.FullPath, writeFile.HasFieldHeader, writeFile.FileFormat.ValueFormatMutable, writeFile.FileFormat, writeFile.CodePageId,
        writeFile.ByteOrderMark, writeFile.ColumnCollection, writeFile.Recipient, writeFile.KeepUnencrypted, writeFile.IdentifierInContainer,
        writeFile.Header, writeFile.Footer);

      var res = await writer.WriteAsync(m_WriteFile.SqlStatement, 360, null, pd.CancellationToken);
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FileName));
      Assert.AreEqual(7, res);
    }
  }
}