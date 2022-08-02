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
  public class CsvFileWriterTests
  {
    private static readonly CsvFile m_WriteFile;

    static CsvFileWriterTests()
    {
      var readFile = new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt")) { ID = "Read", FieldDelimiter = ",", CommentLine = "#" };

      readFile.ColumnCollection.Add(new Column("ExamDate",
        new ValueFormatMutable() { DataType = DataTypeEnum.DateTime, DateFormat = @"dd/MM/yyyy" }));
      readFile.ColumnCollection.Add(new Column("Score", DataTypeEnum.Integer));
      readFile.ColumnCollection.Add(new Column("Proficiency", DataTypeEnum.Numeric));
      readFile.ColumnCollection.Add(new Column("IsNativeLang", DataTypeEnum.Boolean) { Ignore = true });

      UnitTestStatic.MimicSQLReader.AddSetting(readFile);

      m_WriteFile = new CsvFile { ID = "Write", SqlStatement = readFile.ID };

      m_WriteFile.ColumnCollection.Add(new Column("ExamDate", @"MM/dd/yyyy") { TimePart = "ExamTime" });
      m_WriteFile.ColumnCollection.Add(new Column { Name = "Proficiency", Ignore = true });
    }

    [TestMethod]
    [Timeout(2000)]
    public async Task TimeZoneConversionsAsync()
    {
      var pd = new MockProcessDisplay();

      var writeFile = (CsvFile) m_WriteFile.Clone();
      writeFile.FileName = UnitTestStatic.GetTestPath("BasicCSVOut2tzc.txt");

      FileSystemUtils.FileDelete(writeFile.FileName);
      var setting = UnitTestStatic.ReaderGetAllFormats();

      UnitTestStatic.MimicSQLReader.AddSetting(setting);
      writeFile.SqlStatement = setting.ID;
      writeFile.FieldDelimiter = "|";
      writeFile.ColumnCollection.Add(
        new Column("DateTime", new ValueFormatMutable() { DataType = DataTypeEnum.DateTime, DateFormat = "yyyyMMdd" })
        {
          TimePartFormat = @"hh:mm",
          TimePart = "Time",
          TimeZonePart = "TZ"
        });
      var writer = new CsvFileWriter(writeFile.ID, writeFile.FullPath, writeFile.HasFieldHeader, writeFile.DefaultValueFormatWrite, writeFile.CodePageId,
        writeFile.ByteOrderMark, writeFile.ColumnCollection, 0, writeFile.KeepUnencrypted, writeFile.IdentifierInContainer,
        writeFile.Header, writeFile.Footer, "", writeFile.NewLine, writeFile.FieldDelimiterChar, writeFile.FieldQualifierChar, writeFile.EscapePrefixChar,
        writeFile.NewLinePlaceholder, writeFile.DelimiterPlaceholder, writeFile.QualifierPlaceholder, writeFile.QualifyAlways, writeFile.QualifyOnlyIfNeeded, StandardTimeZoneAdjust.ChangeTimeZone, System.TimeZoneInfo.Local.Id);
      UnitTestStatic.MimicSql();

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
      writeFile.FieldDelimiter = "|";

      var writer = new CsvFileWriter(writeFile.ID, writeFile.FullPath, writeFile.HasFieldHeader, writeFile.DefaultValueFormatWrite, writeFile.CodePageId,
        writeFile.ByteOrderMark, writeFile.ColumnCollection, 0, writeFile.KeepUnencrypted, writeFile.IdentifierInContainer,
        writeFile.Header, writeFile.Footer, "", writeFile.NewLine, writeFile.FieldDelimiterChar, writeFile.FieldQualifierChar, writeFile.EscapePrefixChar,
        writeFile.NewLinePlaceholder, writeFile.DelimiterPlaceholder, writeFile.QualifierPlaceholder, writeFile.QualifyAlways, writeFile.QualifyOnlyIfNeeded, StandardTimeZoneAdjust.ChangeTimeZone, System.TimeZoneInfo.Local.Id);

      UnitTestStatic.MimicSql();

      using var sqlReader = await FunctionalDI.SqlDataReader(
        writeFile.SqlStatement, null, 360, 0, pd.CancellationToken).ConfigureAwait(false);
      await sqlReader.OpenAsync(pd.CancellationToken).ConfigureAwait(false);

      var res = await writer.WriteAsync(sqlReader, pd.CancellationToken);
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FileName));
      Assert.AreEqual(7, res);
    }

    [TestMethod, Timeout(2000)]
    public async Task WriteAllFormatsAsync()
    {
      var pd = new MockProcessDisplay();

      var writeFile = (CsvFile) m_WriteFile.Clone();
      writeFile.FileName = UnitTestStatic.GetTestPath("BasicCSVOut2.txt");

      FileSystemUtils.FileDelete(writeFile.FileName);
      var setting = UnitTestStatic.ReaderGetAllFormats();

      UnitTestStatic.MimicSQLReader.AddSetting(setting);
      writeFile.SqlStatement = setting.ID;
      writeFile.FieldDelimiter = "|";

      var cf = new Column("DateTime", DataTypeEnum.DateTime)
      {
        TimePartFormat = @"hh:mm",
        TimePart = "Time",
        TimeZonePart = "\"UTC\"",
        ValueFormatMutable = { DateFormat = "yyyyMMdd" }
      };
      writeFile.ColumnCollection.Add(cf);
      var writer = new CsvFileWriter(writeFile.ID, writeFile.FullPath, writeFile.HasFieldHeader, writeFile.DefaultValueFormatWrite, writeFile.CodePageId,
        writeFile.ByteOrderMark, writeFile.ColumnCollection, 0, writeFile.KeepUnencrypted, writeFile.IdentifierInContainer,
        writeFile.Header, writeFile.Footer, "", writeFile.NewLine, writeFile.FieldDelimiterChar, writeFile.FieldQualifierChar, writeFile.EscapePrefixChar,
        writeFile.NewLinePlaceholder, writeFile.DelimiterPlaceholder, writeFile.QualifierPlaceholder, writeFile.QualifyAlways, writeFile.QualifyOnlyIfNeeded, StandardTimeZoneAdjust.ChangeTimeZone, System.TimeZoneInfo.Local.Id);
      UnitTestStatic.MimicSql();
      var res = await writer.WriteAsync(writeFile.SqlStatement, 360, null, pd.CancellationToken);
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FileName));
      Assert.AreEqual(1065, res, "Records");
    }

    [TestMethod, Timeout(2000)]
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

      var writeFile = new CsvFile { ID = "Test.txt", FileName = UnitTestStatic.GetTestPath("Test.txt"), SqlStatement = "Hello" };
      var processDisplay = new CustomProcessDisplay();

      var writer = new CsvFileWriter(writeFile.ID, writeFile.FullPath, writeFile.HasFieldHeader, writeFile.DefaultValueFormatWrite, writeFile.CodePageId,
        writeFile.ByteOrderMark, writeFile.ColumnCollection, writeFile.KeyID, writeFile.KeepUnencrypted, writeFile.IdentifierInContainer,
        writeFile.Header, writeFile.Footer, "", writeFile.NewLine, writeFile.FieldDelimiterChar, writeFile.FieldQualifierChar, writeFile.EscapePrefixChar,
        writeFile.NewLinePlaceholder, writeFile.DelimiterPlaceholder, writeFile.QualifierPlaceholder, writeFile.QualifyAlways, writeFile.QualifyOnlyIfNeeded, StandardTimeZoneAdjust.ChangeTimeZone, System.TimeZoneInfo.Local.Id);
      using var reader = new DataTableWrapper(dataTable);
      // await reader.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual(100, await writer.WriteAsync(reader, UnitTestStatic.Token));

      Assert.IsTrue(File.Exists(writeFile.FileName));
    }

    [TestMethod, Timeout(1000)]
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

      var writeFile = new CsvFile { ID = "Test.txt", FileName = UnitTestStatic.GetTestPath("Test.txt"), SqlStatement = "Hello" };
      writeFile.ColumnCollection.Add(new Column("Text", DataTypeEnum.Integer));
      writeFile.Header = "##This is a header for {FileName}";
      writeFile.Footer = "##This is a Footer\r\n{Records} in file";
      var count = 0;
      var processDisplay = new CustomProcessDisplay();
      {
        var writer = new CsvFileWriter(writeFile.ID, writeFile.FullPath, writeFile.HasFieldHeader, writeFile.DefaultValueFormatWrite, writeFile.CodePageId,
          writeFile.ByteOrderMark, writeFile.ColumnCollection, 0, writeFile.KeepUnencrypted, writeFile.IdentifierInContainer,
          writeFile.Header, writeFile.Footer, "", writeFile.NewLine, writeFile.FieldDelimiterChar, writeFile.FieldQualifierChar, writeFile.EscapePrefixChar,
          writeFile.NewLinePlaceholder, writeFile.DelimiterPlaceholder, writeFile.QualifierPlaceholder, writeFile.QualifyAlways, writeFile.QualifyOnlyIfNeeded, StandardTimeZoneAdjust.ChangeTimeZone, System.TimeZoneInfo.Local.Id);
        writer.Warning += (sender, e) => { count++; };
        UnitTestStatic.MimicSql();
        using (var reader = new DataTableWrapper(dataTable))
        {
          Assert.AreEqual(100, await writer.WriteAsync(reader, UnitTestStatic.Token), "Records");
        }

        Assert.AreEqual(100, count, "Warnings");
      }

      Assert.IsTrue(File.Exists(writeFile.FileName));
    }

    [TestMethod, Timeout(1000)]
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

      var writeFile = new CsvFile { ID = "Test.txt", FileName = UnitTestStatic.GetTestPath("WriteFileLocked.txt"), InOverview = false, SqlStatement = "dummy" };
      FileSystemUtils.FileDelete(writeFile.FileName);
      using (var file = new StreamWriter(writeFile.FileName))
      {
        await file.WriteLineAsync("Hello");
        try
        {
          var processDisplay = new CustomProcessDisplay();

          var writer = new CsvFileWriter(writeFile.ID, writeFile.FullPath, writeFile.HasFieldHeader, writeFile.DefaultValueFormatWrite, writeFile.CodePageId,
            writeFile.ByteOrderMark, writeFile.ColumnCollection, 0, writeFile.KeepUnencrypted, writeFile.IdentifierInContainer,
            writeFile.Header, writeFile.Footer, "", writeFile.NewLine, writeFile.FieldDelimiterChar, writeFile.FieldQualifierChar, writeFile.EscapePrefixChar,
            writeFile.NewLinePlaceholder, writeFile.DelimiterPlaceholder, writeFile.QualifierPlaceholder, writeFile.QualifyAlways,
            writeFile.QualifyOnlyIfNeeded, StandardTimeZoneAdjust.ChangeTimeZone, System.TimeZoneInfo.Local.Id);
          using var reader = new DataTableWrapper(dataTable);
          UnitTestStatic.MimicSql();
          await writer.WriteAsync(reader, UnitTestStatic.Token);

          Assert.Fail("Exception not thrown");
        }
        catch (FileWriterException)
        {
        }

        await file.WriteLineAsync("World");
      }

      FileSystemUtils.FileDelete(writeFile.FileName);
    }

    [TestMethod, Timeout(1000)]
    public async Task WriteGZipAsync()
    {
      var processDisplay = new MockProcessDisplay();

      var writeFile = (CsvFile) m_WriteFile.Clone();
      writeFile.FileName = UnitTestStatic.GetTestPath("BasicCSVOut.txt.gz");
      FileSystemUtils.FileDelete(writeFile.FileName);
      writeFile.FieldDelimiter = "|";

      var writer = new CsvFileWriter(writeFile.ID, writeFile.FullPath, writeFile.HasFieldHeader, writeFile.DefaultValueFormatWrite, writeFile.CodePageId,
        writeFile.ByteOrderMark, writeFile.ColumnCollection, 0, writeFile.KeepUnencrypted, writeFile.IdentifierInContainer,
        writeFile.Header, writeFile.Footer, "", writeFile.NewLine, writeFile.FieldDelimiterChar, writeFile.FieldQualifierChar, writeFile.EscapePrefixChar,
        writeFile.NewLinePlaceholder, writeFile.DelimiterPlaceholder, writeFile.QualifierPlaceholder, writeFile.QualifyAlways, writeFile.QualifyOnlyIfNeeded, StandardTimeZoneAdjust.ChangeTimeZone, System.TimeZoneInfo.Local.Id);
      UnitTestStatic.MimicSql();
      var res = await writer.WriteAsync(m_WriteFile.SqlStatement, 360, null, UnitTestStatic.Token);
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FileName));
      Assert.AreEqual(7, res);
    }
  }
}