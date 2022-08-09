using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class PagedFileReaderTest
  {
    private readonly CsvFile m_Setting = UnitTestStatic.ReaderGetAllFormats();
    private static readonly TimeZoneChangeDelegate m_TimeZoneAdjust = StandardTimeZoneAdjust.ChangeTimeZone;

    [TestMethod()]
    public async Task MoveToLastPageFirstPageAsync()
    {
      const int pageSize = 20;
      const int numRec = 1065;
      // 1065 / 17
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows,
        m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FieldDelimiter,
        m_Setting.FieldQualifier, m_Setting.EscapePrefix, m_Setting.RecordLimit, m_Setting.AllowRowCombining,
        m_Setting.ContextSensitiveQualifier, m_Setting.CommentLine, m_Setting.NumWarnings,
        m_Setting.DuplicateQualifierToEscape, m_Setting.NewLinePlaceholder, m_Setting.DelimiterPlaceholder,
        m_Setting.QualifierPlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLfAsSpace,
        m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue,
        m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter,
        m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull,
        m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, m_TimeZoneAdjust, System.TimeZoneInfo.Local.Id);
      var test = new PagedFileReader(reader, pageSize);
      await test.OpenAsync(true, true, true, true, UnitTestStatic.Token);

      await test.MoveToLastPageAsync(UnitTestStatic.Token);
      Assert.AreEqual((numRec / pageSize) + 1, test.PageIndex);
      Assert.AreEqual(numRec % pageSize, test.Count);

      await test.MoveToFirstPageAsync(UnitTestStatic.Token);
      Assert.AreEqual(pageSize, test.Count);
    }

    [TestMethod()]
    public async Task MoveToNextPageAsync()
    {
      const int pageSize = 20;
      // 1065 / 17
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows,
        m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FieldDelimiter,
        m_Setting.FieldQualifier, m_Setting.EscapePrefix, m_Setting.RecordLimit, m_Setting.AllowRowCombining,
        m_Setting.ContextSensitiveQualifier, m_Setting.CommentLine, m_Setting.NumWarnings,
        m_Setting.DuplicateQualifierToEscape, m_Setting.NewLinePlaceholder, m_Setting.DelimiterPlaceholder,
        m_Setting.QualifierPlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLfAsSpace,
        m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue,
        m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter,
        m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull,
        m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, m_TimeZoneAdjust, System.TimeZoneInfo.Local.Id);
      var test = new PagedFileReader(reader, pageSize);
      await test.OpenAsync(true, true, true, true, UnitTestStatic.Token);
      Assert.AreEqual(pageSize, test.Count);

      var collectionChangedCalled = false;
      test.CollectionChanged += (_, _) => { collectionChangedCalled = true; };
      await test.MoveToNextPageAsync(UnitTestStatic.Token);
      Assert.IsTrue(collectionChangedCalled);
      Assert.AreEqual(pageSize, test.Count);
    }

    [TestMethod()]
    public async Task MoveToPreviousPageAsync()
    {
      const int pageSize = 33;
      // 1065 / 17
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows,
        m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FieldDelimiter,
        m_Setting.FieldQualifier, m_Setting.EscapePrefix, m_Setting.RecordLimit, m_Setting.AllowRowCombining,
        m_Setting.ContextSensitiveQualifier, m_Setting.CommentLine, m_Setting.NumWarnings,
        m_Setting.DuplicateQualifierToEscape, m_Setting.NewLinePlaceholder, m_Setting.DelimiterPlaceholder,
        m_Setting.QualifierPlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLfAsSpace,
        m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue,
        m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter,
        m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull,
        m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, m_TimeZoneAdjust, System.TimeZoneInfo.Local.Id);
      var test = new PagedFileReader(reader, pageSize);
      await test.OpenAsync(true, true, true, true, UnitTestStatic.Token);
      Assert.AreEqual(pageSize, test.Count);

      await test.MoveToNextPageAsync(UnitTestStatic.Token);
      Assert.AreEqual(pageSize, test.Count);
      await test.MoveToPreviousPageAsync(UnitTestStatic.Token);
      Assert.AreEqual(pageSize, test.Count);
    }

    [TestMethod()]
    public async Task PagedFileReaderOpen()
    {
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows,
        m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FieldDelimiter,
        m_Setting.FieldQualifier, m_Setting.EscapePrefix, m_Setting.RecordLimit, m_Setting.AllowRowCombining,
        m_Setting.ContextSensitiveQualifier, m_Setting.CommentLine, m_Setting.NumWarnings,
        m_Setting.DuplicateQualifierToEscape, m_Setting.NewLinePlaceholder, m_Setting.DelimiterPlaceholder,
        m_Setting.QualifierPlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLfAsSpace,
        m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue,
        m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter,
        m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull,
        m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, m_TimeZoneAdjust, System.TimeZoneInfo.Local.Id);
      var test = new PagedFileReader(reader, 20);
      await test.OpenAsync(true, true, true, true, UnitTestStatic.Token);
      Assert.AreEqual(20, test.Count);

      // get the value in row 1 for the property TZ
      // Assert.AreEqual("CET", testValue.TZ);

      test.Close();
    }
  }
}