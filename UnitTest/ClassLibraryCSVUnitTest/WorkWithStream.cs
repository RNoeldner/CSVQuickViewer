using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class WorkWithStream
  {
    [TestMethod]
    public async System.Threading.Tasks.Task AnalyseStreamAsync()
    {
      var stream = FileSystemUtils.OpenRead(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"));

      var columnCollection = new ColumnCollection();
      CsvHelper.DetectionResult result;

      // Not closing the stream
      using (var impStream = new ImprovedStream(stream, true, true))
      using (IProcessDisplay process = new CsvTools.CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        result = await CsvHelper.RefreshCsvFileAsync(impStream, process, false, true, true, true, true, true, false);
        impStream.Seek(0, System.IO.SeekOrigin.Begin);

        using (var reader = new CsvFileReader(impStream, result.CodePageId, result.SkipRows, result.HasFieldHeader, columnCollection, TrimmingOption.Unquoted, result.FieldDelimiter, result.FieldQualifier, result.EscapeCharacterChar, 0, false, false, result.CommentLine, 0, true, string.Empty, string.Empty, string.Empty, true, false, true, false, false, false, false, false, false, true, true, "NULL", true, 4))
        {
          await reader.OpenAsync(process.CancellationToken);
          var res1 = await reader.FillGuessColumnFormatReaderAsyncReader(new FillGuessSettings(), columnCollection, false, true, "null", process.CancellationToken).ConfigureAwait(false);
          Assert.AreEqual(6, columnCollection.Count, "Recognized columns");
          Assert.AreEqual(6, res1.Count, "Information Lines");
        }
      }

      // Now closing the stream
      using (var impStream = new ImprovedStream(stream, true, false))
      using (IProcessDisplay process = new CsvTools.CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        using (var reader = new CsvFileReader(impStream, result.CodePageId, result.SkipRows, result.HasFieldHeader, columnCollection, TrimmingOption.Unquoted, result.FieldDelimiter, result.FieldQualifier, result.EscapeCharacterChar, 0, false, false, result.CommentLine, 0, true, string.Empty, string.Empty, string.Empty, true, false, true, false, false, false, false, false, false, true, true, "NULL", true, 4))
        {
          await reader.OpenAsync(process.CancellationToken);
          Assert.AreEqual(6, reader.FieldCount);
        }
      }
    }
  }
}