using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class WorkWithStream
  {
    [TestMethod]
    public async System.Threading.Tasks.Task AnalyseStreamAsyncFile()
    {
      var stream = FileSystemUtils.OpenRead(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"));
      var result = new DelimitedFileDetectionResult("stream");
      ICollection<IColumn> determinedColumns;
      // Not closing the stream
      using (var impStream = new ImprovedStream(stream, true))
      using (IProcessDisplay process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        await CsvHelper.UpdateDetectionResultFromStream(impStream, result, process, false, true, true, true, true, true, false);
        impStream.Seek(0, System.IO.SeekOrigin.Begin);

        using (var reader = new CsvFileReader(impStream, result.CodePageId, result.SkipRows, result.HasFieldHeader,
          new ColumnCollection(), TrimmingOption.Unquoted, result.FieldDelimiter, result.FieldQualifier,
          result.EscapeCharacterChar, 0, false, false, result.CommentLine, 0, true, string.Empty, string.Empty,
          string.Empty, true, false, true, false, false, false, false, false, false, true, true))
        {
          await reader.OpenAsync(process.CancellationToken);
          var (info, columns) = await reader.FillGuessColumnFormatReaderAsyncReader(new FillGuessSettings(),
            new ColumnCollection(), false, true, "null", process.CancellationToken);
          determinedColumns = columns.ToList();
          Assert.AreEqual(6, determinedColumns.Count(), "Recognized columns");
          Assert.AreEqual(6, info.Count, "Information Lines");
        }
      }

      // Now closing the stream
      using (var impStream = new ImprovedStream(stream, true))
      using (IProcessDisplay process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        using (var reader = new CsvFileReader(impStream, result.CodePageId, result.SkipRows, result.HasFieldHeader,
          determinedColumns, TrimmingOption.Unquoted, result.FieldDelimiter, result.FieldQualifier,
          result.EscapeCharacterChar, 0, false, false, result.CommentLine, 0, true, string.Empty, string.Empty,
          string.Empty, true, false, true, false, false, false, false, false, false, true, true))
        {
          await reader.OpenAsync(process.CancellationToken);
          Assert.AreEqual(6, reader.FieldCount);
        }
      }
    }

    [TestMethod]
    public async System.Threading.Tasks.Task AnalyseStreamAsyncGZip()
    {
      using (var stream = FileSystemUtils.OpenRead(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt.gz")))
      {
        ICollection<IColumn> determinedColumns;
        var result = new DelimitedFileDetectionResult("steam");
        // Not closing the stream
        using (var impStream = new ImprovedStream(stream, true, SourceAccess.FileTypeEnum.GZip))
        using (IProcessDisplay process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
        {
          await CsvHelper.UpdateDetectionResultFromStream(impStream, result, process, false, true, true, true, true, true, false);
          impStream.Seek(0, System.IO.SeekOrigin.Begin);

          using (var reader = new CsvFileReader(impStream, result.CodePageId, result.SkipRows, result.HasFieldHeader,
            new ColumnCollection(), TrimmingOption.Unquoted, result.FieldDelimiter, result.FieldQualifier,
            result.EscapeCharacterChar, 0, false, false, result.CommentLine, 0, true, string.Empty, string.Empty,
            string.Empty, true, false, true, false, false, false, false, false, false, true, true))
          {
            await reader.OpenAsync(process.CancellationToken);
            var (info, columns) = await reader.FillGuessColumnFormatReaderAsyncReader(new FillGuessSettings(),
              new ColumnCollection(), false, true, "null", process.CancellationToken);
            determinedColumns = columns.ToList();
            Assert.AreEqual(6, determinedColumns.Count(), "Recognized columns");
            Assert.AreEqual(6, info.Count, "Information Lines");
          }
        }

        // Now closing the stream
        using (var impStream = new ImprovedStream(stream, true, SourceAccess.FileTypeEnum.GZip))
        using (IProcessDisplay process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
        {
          using (var reader = new CsvFileReader(impStream, result.CodePageId, result.SkipRows, result.HasFieldHeader,
            determinedColumns, TrimmingOption.Unquoted, result.FieldDelimiter, result.FieldQualifier,
            result.EscapeCharacterChar, 0, false, false, result.CommentLine, 0, true, string.Empty, string.Empty,
            string.Empty, true, false, true, false, false, false, false, false, false, true, true))
          {
            await reader.OpenAsync(process.CancellationToken);
            Assert.AreEqual(6, reader.FieldCount);
          }
        }
      }
    }
  }
}