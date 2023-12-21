using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ReSharper disable UseAwaitUsing

namespace CsvTools.Tests
{
  [TestClass]
  public class WorkWithStream
  {
    [TestMethod]
    public async System.Threading.Tasks.Task AnalyseStreamAsyncFile()
    {
      var stream = FileSystemUtils.OpenRead(UnitTestStatic.GetTestPath("BasicCSV.txt"));

      ICollection<Column> determinedColumns;
      // Not closing the stream

      using var impStream = new ImprovedStream(new SourceAccess( stream, FileTypeEnum.Stream));
      var result = new InspectionResult();
      await impStream.UpdateInspectionResultAsync(result, false, true, true, true, true, true, true, false, true,
        Array.Empty<char>(), UnitTestStatic.Token);
      impStream.Seek(0, SeekOrigin.Begin);

      using (var reader = new CsvFileReader(impStream, result.CodePageId, result.SkipRows, result.HasFieldHeader,
               new ColumnCollection(), TrimmingOptionEnum.Unquoted, result.FieldDelimiter, result.FieldQualifier,
               result.EscapePrefix, 0, false, false, result.CommentLine, 0, true, string.Empty, string.Empty,
               string.Empty, true, false, true, false, false, false, false, false, false, true, true, "", false, 0,
               StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, true, false))
      {
        await reader.OpenAsync(UnitTestStatic.Token);
        var (info, columns) = await reader.FillGuessColumnFormatReaderAsyncReader(FillGuessSettings.Default,
          new ColumnCollection(), false, true, "null", UnitTestStatic.Token);
        determinedColumns = columns.ToList();
        Assert.AreEqual(6, determinedColumns.Count(), "Recognized columns");
        Assert.AreEqual(6, info.Count, "Information Lines");
      }

      impStream.Seek(0, SeekOrigin.Begin);

      using (var reader = new CsvFileReader(impStream, result.CodePageId, result.SkipRows, result.HasFieldHeader,
               determinedColumns, TrimmingOptionEnum.Unquoted, result.FieldDelimiter, result.FieldQualifier,
               result.EscapePrefix, 0, false, false, result.CommentLine, 0, true, string.Empty, string.Empty,
               string.Empty, true, false, true, false, false, false, false, false, false, true, true, "", false, 0,
               StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, true, false))
      {
        await reader.OpenAsync(UnitTestStatic.Token);
        Assert.AreEqual(6, reader.FieldCount);
      }
    }

    [TestMethod]
    public async System.Threading.Tasks.Task AnalyseStreamAsyncGZip()
    {
      using var stream = FileSystemUtils.OpenRead(UnitTestStatic.GetTestPath("BasicCSV.txt.gz"));
      ICollection<Column> determinedColumns;
      // Not closing the stream
      using var impStream = new ImprovedStream(new SourceAccess( stream, FileTypeEnum.GZip));
      var result = new InspectionResult();
      await impStream.UpdateInspectionResultAsync(result, false, true, true, true, true, true, true, false,
       false,         Array.Empty<char>(), UnitTestStatic.Token);

      impStream.Seek(0, SeekOrigin.Begin);

      using (var reader = new CsvFileReader(impStream, result.CodePageId, result.SkipRows, result.HasFieldHeader,
               new ColumnCollection(), TrimmingOptionEnum.Unquoted, result.FieldDelimiter, result.FieldQualifier,
               result.EscapePrefix, 0, false, false, result.CommentLine, 0, true, string.Empty, string.Empty,
               string.Empty, true, false, true, false, false, false, false, false, false, true, true, "", false, 0,
               StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, true, false))
      {
        await reader.OpenAsync(UnitTestStatic.Token);
        var (info, columns) = await reader.FillGuessColumnFormatReaderAsyncReader(FillGuessSettings.Default,
          new ColumnCollection(), false, true, "null", UnitTestStatic.Token);
        determinedColumns = columns.ToList();
        Assert.AreEqual(6, determinedColumns.Count(), "Recognized columns");
        Assert.AreEqual(6, info.Count, "Information Lines");
      }

      impStream.Seek(0, SeekOrigin.Begin);

      using (var reader = new CsvFileReader(impStream, result.CodePageId, result.SkipRows, result.HasFieldHeader,
               determinedColumns, TrimmingOptionEnum.Unquoted, result.FieldDelimiter, result.FieldQualifier,
               result.EscapePrefix, 0, false, false, result.CommentLine, 0, true, string.Empty, string.Empty,
               string.Empty, true, false, true, false, false, false, false, false, false, true, true, "", false, 0,
               StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, true, false))
      {
        await reader.OpenAsync(UnitTestStatic.Token);
        Assert.AreEqual(6, reader.FieldCount);
      }
    }
  }
}