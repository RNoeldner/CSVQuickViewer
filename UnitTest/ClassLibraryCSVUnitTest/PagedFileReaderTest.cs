using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
	public class PagedFileReaderTest
	{
		private readonly CsvFile m_Setting = UnitTestStatic.ReaderGetAllFormats();

		[TestMethod()]
		public async Task MoveToLastPageFirstPageAsync()
		{
			const int pageSize = 20;
			const int numRec = 1065;
			// 1065 / 17
      using var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token);
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows, m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FileFormat.FieldDelimiter, m_Setting.FileFormat.FieldQualifier, m_Setting.FileFormat.EscapeCharacter, m_Setting.RecordLimit, m_Setting.AllowRowCombining, m_Setting.FileFormat.AlternateQuoting, m_Setting.FileFormat.CommentLine, m_Setting.NumWarnings, m_Setting.FileFormat.DuplicateQuotingToEscape, m_Setting.FileFormat.NewLinePlaceholder, m_Setting.FileFormat.DelimiterPlaceholder, m_Setting.FileFormat.QuotePlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLFAsSpace, m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue, m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter, m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull, m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, process);
      using var test = new PagedFileReader(reader, pageSize, UnitTestInitializeCsv.Token);
      await test.OpenAsync(true, true, true, true);

      await test.MoveToLastPageAsync();
      Assert.AreEqual(numRec / pageSize +  1, test.PageIndex);
      Assert.AreEqual(numRec % pageSize, test.Count);

      await test.MoveToFirstPageAsync();
      Assert.AreEqual(pageSize, test.Count);
    }

		[TestMethod()]
		public async Task MoveToNextPageAsync()
		{
			const int pageSize = 20;
			// 1065 / 17
      using var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token);
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows, m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FileFormat.FieldDelimiter, m_Setting.FileFormat.FieldQualifier, m_Setting.FileFormat.EscapeCharacter, m_Setting.RecordLimit, m_Setting.AllowRowCombining, m_Setting.FileFormat.AlternateQuoting, m_Setting.FileFormat.CommentLine, m_Setting.NumWarnings, m_Setting.FileFormat.DuplicateQuotingToEscape, m_Setting.FileFormat.NewLinePlaceholder, m_Setting.FileFormat.DelimiterPlaceholder, m_Setting.FileFormat.QuotePlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLFAsSpace, m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue, m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter, m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull, m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, process);
      using var test = new PagedFileReader(reader, pageSize, UnitTestInitializeCsv.Token);
      await test.OpenAsync(true, true, true, true);
      Assert.AreEqual(pageSize, test.Count);

      bool collectionChangedCalled = false;
      test.CollectionChanged += (o, s) => { collectionChangedCalled = true; };
      await test.MoveToNextPageAsync();
      Assert.IsTrue(collectionChangedCalled);
      Assert.AreEqual(pageSize, test.Count);
    }

		[TestMethod()]
		public async Task MoveToPreviousPageAsync()
		{
			const int c_PageSize = 33;
			// 1065 / 17
      using var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token);
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows, m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FileFormat.FieldDelimiter, m_Setting.FileFormat.FieldQualifier, m_Setting.FileFormat.EscapeCharacter, m_Setting.RecordLimit, m_Setting.AllowRowCombining, m_Setting.FileFormat.AlternateQuoting, m_Setting.FileFormat.CommentLine, m_Setting.NumWarnings, m_Setting.FileFormat.DuplicateQuotingToEscape, m_Setting.FileFormat.NewLinePlaceholder, m_Setting.FileFormat.DelimiterPlaceholder, m_Setting.FileFormat.QuotePlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLFAsSpace, m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue, m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter, m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull, m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, process);
      using var test = new PagedFileReader(reader, c_PageSize, UnitTestInitializeCsv.Token);
      await test.OpenAsync(true, true, true, true);
      Assert.AreEqual(c_PageSize, test.Count);

      await test.MoveToNextPageAsync();
      Assert.AreEqual(c_PageSize, test.Count);
      await test.MoveToPreviousPageAsync();
      Assert.AreEqual(c_PageSize, test.Count);
    }

		[TestMethod()]
		public async Task PagedFileReaderOpen()
    {
      using var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token);
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows, m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FileFormat.FieldDelimiter, m_Setting.FileFormat.FieldQualifier, m_Setting.FileFormat.EscapeCharacter, m_Setting.RecordLimit, m_Setting.AllowRowCombining, m_Setting.FileFormat.AlternateQuoting, m_Setting.FileFormat.CommentLine, m_Setting.NumWarnings, m_Setting.FileFormat.DuplicateQuotingToEscape, m_Setting.FileFormat.NewLinePlaceholder, m_Setting.FileFormat.DelimiterPlaceholder, m_Setting.FileFormat.QuotePlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLFAsSpace, m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue, m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter, m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull, m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, process);
      var test = new PagedFileReader(reader, 20, UnitTestInitializeCsv.Token);
      await test.OpenAsync(true, true, true, true);
      Assert.AreEqual(20, test.Count);

      // get the value in row 1 for the property TZ
      dynamic testValue = test[0];
      // Assert.AreEqual("CET", testValue.TZ);

      test.Close();
      test.Dispose();
    }
	}
}