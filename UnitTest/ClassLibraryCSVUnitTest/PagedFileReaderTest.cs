using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class PagedFileReaderTest
  {
    private CsvFile setting = UnitTestHelper.ReaderGetAllFormats();

    [TestMethod()]
    public async Task MoveToLastPageFirstPageAsync()
    {
      int pageSize = 20;
      int numRec = 1065;
      // 1065 / 17
      using (var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FileFormat.FieldDelimiter, setting.FileFormat.FieldQualifier, setting.FileFormat.EscapeCharacter, setting.RecordLimit, setting.AllowRowCombining, setting.FileFormat.AlternateQuoting, setting.FileFormat.CommentLine, setting.NumWarnings, setting.FileFormat.DuplicateQuotingToEscape, setting.FileFormat.NewLinePlaceholder, setting.FileFormat.DelimiterPlaceholder, setting.FileFormat.QuotePlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, process))
      {
        var test = new PagedFileReader(reader, pageSize, UnitTestInitializeCsv.Token);
        await test.OpenAsync(true, true, true, true);

        await test.MoveToLastPageAsync();
        Assert.AreEqual(numRec /pageSize + (numRec % pageSize ==0 ? 0 : 1), test.PageIndex);
        Assert.AreEqual(numRec % pageSize, test.Count);

        await test.MoveToFirstPageAsync();
        Assert.AreEqual(pageSize, test.Count);
      }
    }

    [TestMethod()]
    public async Task MoveToNextPageAsync()
    {
      int pageSize = 20;
      int numRec = 1065;
      // 1065 / 17
      using (var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FileFormat.FieldDelimiter, setting.FileFormat.FieldQualifier, setting.FileFormat.EscapeCharacter, setting.RecordLimit, setting.AllowRowCombining, setting.FileFormat.AlternateQuoting, setting.FileFormat.CommentLine, setting.NumWarnings, setting.FileFormat.DuplicateQuotingToEscape, setting.FileFormat.NewLinePlaceholder, setting.FileFormat.DelimiterPlaceholder, setting.FileFormat.QuotePlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, process))
      {
        var test = new PagedFileReader(reader, pageSize, UnitTestInitializeCsv.Token);
        await test.OpenAsync(true, true, true, true);
        Assert.AreEqual(pageSize, test.Count);

        bool collectionChangedCalled = false;
        test.CollectionChanged += (o, s) => { collectionChangedCalled = true; };
        await test.MoveToNextPageAsync();
        Assert.IsTrue(collectionChangedCalled);
        Assert.AreEqual(pageSize, test.Count);
      }
    }

    [TestMethod()]
    public async Task MoveToPreviousPageAsync()
    {
      int pageSize = 20;
      // 1065 / 17
      using (var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FileFormat.FieldDelimiter, setting.FileFormat.FieldQualifier, setting.FileFormat.EscapeCharacter, setting.RecordLimit, setting.AllowRowCombining, setting.FileFormat.AlternateQuoting, setting.FileFormat.CommentLine, setting.NumWarnings, setting.FileFormat.DuplicateQuotingToEscape, setting.FileFormat.NewLinePlaceholder, setting.FileFormat.DelimiterPlaceholder, setting.FileFormat.QuotePlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, process))
      {
        var test = new PagedFileReader(reader, pageSize, UnitTestInitializeCsv.Token);
        await test.OpenAsync(true, true, true, true);
        Assert.AreEqual(pageSize, test.Count);

        await test.MoveToNextPageAsync();
        Assert.AreEqual(pageSize, test.Count);
        await test.MoveToPreviousPageAsync();
        Assert.AreEqual(pageSize, test.Count);
      }
    }

    [TestMethod()]
    public async Task PagedFileReaderOpen()
    {
      using (var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FileFormat.FieldDelimiter, setting.FileFormat.FieldQualifier, setting.FileFormat.EscapeCharacter, setting.RecordLimit, setting.AllowRowCombining, setting.FileFormat.AlternateQuoting, setting.FileFormat.CommentLine, setting.NumWarnings, setting.FileFormat.DuplicateQuotingToEscape, setting.FileFormat.NewLinePlaceholder, setting.FileFormat.DelimiterPlaceholder, setting.FileFormat.QuotePlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, process))
      {
        var test = new PagedFileReader(reader, 20, UnitTestInitializeCsv.Token);
        await test.OpenAsync(true, true, true, true);
        Assert.AreEqual(20, test.Count);

        // get the value in row 1 for the property TZ
        dynamic testValue = test[0];
        Assert.AreEqual("CET", testValue.TZ);

        test.Close();
      }
    }
  }
}