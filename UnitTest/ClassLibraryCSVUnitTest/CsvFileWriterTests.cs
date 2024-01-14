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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  [SuppressMessage("ReSharper", "UseAwaitUsing")]
  public class CsvFileWriterTests
  {

    static CsvFileWriterTests()
    {
      var readFile = new CsvFileDummy(UnitTestStatic.GetTestPath("BasicCSV.txt"))
      {
        FieldDelimiterChar = ',',
        CommentLine = "#"
      };

      readFile.ColumnCollection.Add(new Column("ExamDate",
        new ValueFormat(DataTypeEnum.DateTime, dateFormat: @"dd/MM/yyyy")));
      readFile.ColumnCollection.Add(new Column("Score", new ValueFormat(DataTypeEnum.Integer)));
      readFile.ColumnCollection.Add(new Column("Proficiency", new ValueFormat(DataTypeEnum.Numeric)));
      readFile.ColumnCollection.Add(
        new Column("IsNativeLang", new ValueFormat(DataTypeEnum.Boolean), ignore: true));
    }

    [TestMethod]
    [Timeout(2000)]
    public async Task TimeZoneConversionsAsync()
    {
      var writeFile = new CsvFileDummy(UnitTestStatic.GetTestPath("BasicCSVOut2tzc.txt"));
      writeFile.ColumnCollection.Add(new Column("ExamDate",
        new ValueFormat(DataTypeEnum.DateTime), timePart: "ExamTime"));
      writeFile.ColumnCollection.Add(new Column("Proficiency", ValueFormat.Empty, ignore: true));

      FileSystemUtils.FileDelete(writeFile.FileName);
      var setting = new CsvFileDummy(UnitTestStatic.GetTestPath("AllFormats.txt"))
      {
        HasFieldHeader = true,
        FieldDelimiterChar = '\t',
      };
      // columns from the file
      setting.ColumnCollection.AddRangeNoClone(
        new Column[]
        {
          new Column("DateTime", new ValueFormat(dataType: DataTypeEnum.DateTime, dateFormat: @"dd/MM/yyyy"), timePart: "Time", timePartFormat: "HH:mm:ss"),
          new Column("Integer", new ValueFormat(DataTypeEnum.Integer)),
          new Column("Numeric", new ValueFormat(DataTypeEnum.Numeric, decimalSeparator: ".")),
          new Column("Double", new ValueFormat(dataType: DataTypeEnum.Double, decimalSeparator: ".")),
          new Column("Boolean", new ValueFormat(DataTypeEnum.Boolean)),
          new Column("GUID", new ValueFormat(DataTypeEnum.Guid)),
          new Column("Time", new ValueFormat(dataType: DataTypeEnum.DateTime, dateFormat: "HH:mm:ss"), ignore: true)
        });

      writeFile.FieldDelimiterChar = '|';
      writeFile.ColumnCollection.Add(
        new Column("DateTime", new ValueFormat(DataTypeEnum.DateTime, dateFormat: "yyyyMMdd"),
          timePartFormat: @"hh:mm", timePart: "Time", timeZonePart: "TZ"));
      var writer = new CsvFileWriter(writeFile.FullPath, writeFile.HasFieldHeader, writeFile.ValueFormatWrite, writeFile.CodePageId, writeFile.ByteOrderMark,
        writeFile.ColumnCollection, writeFile.IdentifierInContainer, writeFile.Header, writeFile.Footer,
        "", writeFile.NewLine, writeFile.FieldDelimiterChar, writeFile.FieldQualifierChar, writeFile.EscapePrefixChar, writeFile.NewLinePlaceholder, writeFile.DelimiterPlaceholder,
        writeFile.QualifierPlaceholder, writeFile.QualifyAlways, writeFile.QualifyOnlyIfNeeded, writeFile.WriteFixedLength, StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, String.Empty, writeFile.KeepUnencrypted);

      using var reader = FunctionalDI.FileReaderWriterFactory.GetFileReader(setting, UnitTestStatic.Token);
      await reader.OpenAsync(UnitTestStatic.Token);

      var res = await writer.WriteAsync(reader, UnitTestStatic.Token);
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FileName));
      Assert.AreEqual(1065, res, "Records");
    }

    [TestMethod]
    public async Task Write()
    {
      var pd = new MockProgress();
      var writeFile = new CsvFileDummy(UnitTestStatic.GetTestPath("BasicCSVOut.txt"));
      writeFile.ColumnCollection.Add(new Column("ExamDate",
        new ValueFormat(DataTypeEnum.DateTime), timePart: "ExamTime"));
      writeFile.ColumnCollection.Add(new Column("Proficiency", ValueFormat.Empty, ignore: true));
      writeFile.FieldDelimiterChar = '|';

      FileSystemUtils.FileDelete(writeFile.FileName);

      var writer = new CsvFileWriter(writeFile.FullPath, writeFile.HasFieldHeader, writeFile.ValueFormatWrite, writeFile.CodePageId, writeFile.ByteOrderMark,
        writeFile.ColumnCollection, writeFile.IdentifierInContainer, writeFile.Header, writeFile.Footer,
        "", writeFile.NewLine, writeFile.FieldDelimiterChar, writeFile.FieldQualifierChar, writeFile.EscapePrefixChar, writeFile.NewLinePlaceholder, writeFile.DelimiterPlaceholder,
        writeFile.QualifierPlaceholder, writeFile.QualifyAlways, writeFile.QualifyOnlyIfNeeded, writeFile.WriteFixedLength, StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, string.Empty, writeFile.KeepUnencrypted);


      using var sqlReader = new DataTableWrapper(UnitTestStaticData.RandomDataTable(100));
      var res = await writer.WriteAsync(sqlReader, pd.CancellationToken);
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FileName));
      Assert.AreEqual(100, res);
    }

    [TestMethod, Timeout(2000)]
    public async Task WriteAllFormatsAsync()
    {
      var writeFile = new CsvFileDummy(UnitTestStatic.GetTestPath("BasicCSVOut2.txt"));
      writeFile.ColumnCollection.Add(new Column("ExamDate",
        new ValueFormat(DataTypeEnum.DateTime), timePart: "ExamTime"));
      writeFile.ColumnCollection.Add(new Column("Proficiency", ValueFormat.Empty, ignore: true));
      writeFile.FieldDelimiterChar = '|';

      FileSystemUtils.FileDelete(writeFile.FileName);


      var cf = new Column("DateTime", new ValueFormat(DataTypeEnum.DateTime, "yyyyMMdd"),
        timePartFormat: @"hh:mm", timePart: "Time", timeZonePart: "\"UTC\"");
      writeFile.ColumnCollection.Add(cf);
      var writer = new CsvFileWriter(writeFile.FullPath, writeFile.HasFieldHeader, writeFile.ValueFormatWrite, writeFile.CodePageId, writeFile.ByteOrderMark,
        writeFile.ColumnCollection, writeFile.IdentifierInContainer, writeFile.Header, writeFile.Footer,
        "", writeFile.NewLine, writeFile.FieldDelimiterChar, writeFile.FieldQualifierChar, writeFile.EscapePrefixChar, writeFile.NewLinePlaceholder, writeFile.DelimiterPlaceholder,
        writeFile.QualifierPlaceholder, writeFile.QualifyAlways, writeFile.QualifyOnlyIfNeeded, writeFile.WriteFixedLength,
        StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, string.Empty, writeFile.KeepUnencrypted);

      var setting = new CsvFileDummy(Path.Combine(UnitTestStatic.GetTestPath("AllFormats.txt")))
      {
        HasFieldHeader = true,
        FieldDelimiterChar = '\t',
      };
      // columns from the file
      setting.ColumnCollection.AddRangeNoClone(
        new Column[]
        {
          new Column("DateTime", new ValueFormat(dataType: DataTypeEnum.DateTime, dateFormat: @"dd/MM/yyyy"), timePart: "Time", timePartFormat: "HH:mm:ss"),
          new Column("Integer", new ValueFormat(DataTypeEnum.Integer)),
          new Column("Numeric", new ValueFormat(DataTypeEnum.Numeric, decimalSeparator: ".")),
          new Column("Double", new ValueFormat(dataType: DataTypeEnum.Double, decimalSeparator: ".")),
          new Column("Boolean", new ValueFormat(DataTypeEnum.Boolean)),
          new Column("GUID", new ValueFormat(DataTypeEnum.Guid)),
          new Column("Time", new ValueFormat(dataType: DataTypeEnum.DateTime, dateFormat: "HH:mm:ss"), ignore: true)
        });

      using var reader = FunctionalDI.FileReaderWriterFactory.GetFileReader(setting, UnitTestStatic.Token);
      await reader.OpenAsync(UnitTestStatic.Token);

      var res = await writer.WriteAsync(reader, UnitTestStatic.Token);
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

      var writeFile = new CsvFileDummy(UnitTestStatic.GetTestPath("Test.txt"));

      var writer = new CsvFileWriter(writeFile.FullPath, writeFile.HasFieldHeader, writeFile.ValueFormatWrite, writeFile.CodePageId, writeFile.ByteOrderMark,
        writeFile.ColumnCollection, writeFile.IdentifierInContainer, writeFile.Header, writeFile.Footer,
        "", writeFile.NewLine, writeFile.FieldDelimiterChar, writeFile.FieldQualifierChar, writeFile.EscapePrefixChar, writeFile.NewLinePlaceholder, writeFile.DelimiterPlaceholder,
        writeFile.QualifierPlaceholder, writeFile.QualifyAlways, writeFile.QualifyOnlyIfNeeded, writeFile.WriteFixedLength, StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, String.Empty, writeFile.KeepUnencrypted);

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

      var writeFile = new CsvFileDummy(UnitTestStatic.GetTestPath("Test.txt"));
      writeFile.ColumnCollection.Add(new Column("Text", new ValueFormat(DataTypeEnum.Integer)));
      writeFile.Header = "##This is a header for {FileName}";
      writeFile.Footer = "##This is a Footer\r\n{Records} in file";
      var count = 0;
      {
        var writer = new CsvFileWriter(writeFile.FullPath, writeFile.HasFieldHeader, writeFile.ValueFormatWrite, writeFile.CodePageId, writeFile.ByteOrderMark,
          writeFile.ColumnCollection, writeFile.IdentifierInContainer, writeFile.Header, writeFile.Footer,
          "", writeFile.NewLine, writeFile.FieldDelimiterChar, writeFile.FieldQualifierChar, writeFile.EscapePrefixChar, writeFile.NewLinePlaceholder, writeFile.DelimiterPlaceholder,
          writeFile.QualifierPlaceholder, writeFile.QualifyAlways, writeFile.QualifyOnlyIfNeeded, writeFile.WriteFixedLength, StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, String.Empty, writeFile.KeepUnencrypted);
        writer.Warning += (o, a) => { count++; };
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

      var writeFile = new CsvFileDummy(UnitTestStatic.GetTestPath("WriteFileLocked.txt"));
      FileSystemUtils.FileDelete(writeFile.FileName);
      using (var file = new StreamWriter(writeFile.FileName))
      {
        await file.WriteLineAsync("Hello");
        try
        {
          var writer = new CsvFileWriter(writeFile.FullPath, writeFile.HasFieldHeader, writeFile.ValueFormatWrite, writeFile.CodePageId, writeFile.ByteOrderMark,
            writeFile.ColumnCollection, writeFile.IdentifierInContainer, writeFile.Header, writeFile.Footer,
            "", writeFile.NewLine, writeFile.FieldDelimiterChar, writeFile.FieldQualifierChar, writeFile.EscapePrefixChar, writeFile.NewLinePlaceholder, writeFile.DelimiterPlaceholder,
            writeFile.QualifierPlaceholder, writeFile.QualifyAlways, writeFile.QualifyOnlyIfNeeded, writeFile.WriteFixedLength, StandardTimeZoneAdjust.ChangeTimeZone,
            TimeZoneInfo.Local.Id, String.Empty, writeFile.KeepUnencrypted);
          using var reader = new DataTableWrapper(dataTable);

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

var writeFile = new CsvFileDummy(UnitTestStatic.GetTestPath("BasicCSVOut.txt.gz"));
      writeFile.ColumnCollection.Add(new Column("ExamDate",
        new ValueFormat(DataTypeEnum.DateTime), timePart: "ExamTime"));
      writeFile.ColumnCollection.Add(new Column("Proficiency", ValueFormat.Empty, ignore: true));
      writeFile.FieldDelimiterChar = "|".FromText();

      FileSystemUtils.FileDelete(writeFile.FileName);

      var writer = new CsvFileWriter(writeFile.FullPath, writeFile.HasFieldHeader, writeFile.ValueFormatWrite, writeFile.CodePageId, writeFile.ByteOrderMark,
        writeFile.ColumnCollection, writeFile.IdentifierInContainer, writeFile.Header, writeFile.Footer,
        "", writeFile.NewLine, writeFile.FieldDelimiterChar, writeFile.FieldQualifierChar, writeFile.EscapePrefixChar, writeFile.NewLinePlaceholder, writeFile.DelimiterPlaceholder,
        writeFile.QualifierPlaceholder, writeFile.QualifyAlways, writeFile.QualifyOnlyIfNeeded, writeFile.WriteFixedLength, StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, string.Empty, writeFile.KeepUnencrypted);

      using var reader = new DataTableWrapper(UnitTestStaticData.RandomDataTable(100));

      var res = await writer.WriteAsync(reader, UnitTestStatic.Token);
      Assert.IsTrue(FileSystemUtils.FileExists(writeFile.FileName));
      Assert.AreEqual(100, res);
    }
  }
}