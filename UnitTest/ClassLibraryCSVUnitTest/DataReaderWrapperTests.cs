using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class DataReaderWrapperTests
  {
    private static readonly CsvFile m_Setting = UnitTestStatic.ReaderGetAllFormats();

    [TestMethod()]
    public async Task GetColumnIndexFromErrorColumnTest()
    {
      var process = new CustomProcessDisplay();
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows,
        m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption,
        m_Setting.FieldDelimiter, m_Setting.FieldQualifier, m_Setting.EscapePrefix,
        m_Setting.RecordLimit, m_Setting.AllowRowCombining, m_Setting.ContextSensitiveQualifier,
        m_Setting.CommentLine, m_Setting.NumWarnings, m_Setting.DuplicateQualifierToEscape,
        m_Setting.NewLinePlaceholder, m_Setting.DelimiterPlaceholder,
        m_Setting.QualifierPlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLfAsSpace,
        m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue,
        m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter,
        m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull,
        m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, process);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
    }

    [TestMethod()]
    public async Task DepthTest()
    {
      var process = new CustomProcessDisplay();
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows, m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FieldDelimiter, m_Setting.FieldQualifier, m_Setting.EscapePrefix, m_Setting.RecordLimit, m_Setting.AllowRowCombining, m_Setting.ContextSensitiveQualifier, m_Setting.CommentLine, m_Setting.NumWarnings, m_Setting.DuplicateQualifierToEscape, m_Setting.NewLinePlaceholder, m_Setting.DelimiterPlaceholder, m_Setting.QualifierPlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLfAsSpace, m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue, m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter, m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull, m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, process);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      Assert.AreEqual(9, wrapper.Depth);
    }

    [TestMethod()]
    public async Task GetIntegerTest()
    {
      var process = new CustomProcessDisplay();
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows, m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FieldDelimiter, m_Setting.FieldQualifier, m_Setting.EscapePrefix, m_Setting.RecordLimit, m_Setting.AllowRowCombining, m_Setting.ContextSensitiveQualifier, m_Setting.CommentLine, m_Setting.NumWarnings, m_Setting.DuplicateQualifierToEscape, m_Setting.NewLinePlaceholder, m_Setting.DelimiterPlaceholder, m_Setting.QualifierPlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLfAsSpace, m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue, m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter, m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull, m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, process);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual((short) -22477, wrapper.GetInt16(1));
      Assert.AreEqual(-22477, wrapper.GetInt32(1));
      Assert.AreEqual(-22477L, wrapper.GetInt64(1));
    }

    [TestMethod()]
    public async Task GetNumericTest()
    {
      var process = new CustomProcessDisplay();
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows, m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FieldDelimiter, m_Setting.FieldQualifier, m_Setting.EscapePrefix, m_Setting.RecordLimit, m_Setting.AllowRowCombining, m_Setting.ContextSensitiveQualifier, m_Setting.CommentLine, m_Setting.NumWarnings, m_Setting.DuplicateQualifierToEscape, m_Setting.NewLinePlaceholder, m_Setting.DelimiterPlaceholder, m_Setting.QualifierPlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLfAsSpace, m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue, m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter, m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull, m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, process);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);

      Assert.AreEqual((float) -12086.66, wrapper.GetFloat(2));
      Assert.AreEqual(-12086.66, wrapper.GetDouble(2));
      Assert.AreEqual((decimal) -12086.66, wrapper.GetDecimal(2));
      Assert.AreEqual((-12086.66).ToString(), wrapper.GetString(2));
    }

    [TestMethod()]
    public async Task GetGuidTest()
    {
      var process = new CustomProcessDisplay();
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows, m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FieldDelimiter, m_Setting.FieldQualifier, m_Setting.EscapePrefix, m_Setting.RecordLimit, m_Setting.AllowRowCombining, m_Setting.ContextSensitiveQualifier, m_Setting.CommentLine, m_Setting.NumWarnings, m_Setting.DuplicateQualifierToEscape, m_Setting.NewLinePlaceholder, m_Setting.DelimiterPlaceholder, m_Setting.QualifierPlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLfAsSpace, m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue, m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter, m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull, m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, process);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);

      Assert.AreEqual(new Guid("1BD10E34-7D66-481B-A7E3-AE817B5BEE02"), wrapper.GetGuid(7));
      Assert.AreEqual("1BD10E34-7D66-481B-A7E3-AE817B5BEE02", wrapper.GetString(7).ToUpper());
    }

    [TestMethod()]
    public async Task GetDateTimeTest()
    {
      var process = new CustomProcessDisplay();
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows, m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FieldDelimiter, m_Setting.FieldQualifier, m_Setting.EscapePrefix, m_Setting.RecordLimit, m_Setting.AllowRowCombining, m_Setting.ContextSensitiveQualifier, m_Setting.CommentLine, m_Setting.NumWarnings, m_Setting.DuplicateQualifierToEscape, m_Setting.NewLinePlaceholder, m_Setting.DelimiterPlaceholder, m_Setting.QualifierPlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLfAsSpace, m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue, m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter, m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull, m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, process);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      Assert.IsTrue((new DateTime(2014, 05, 23) - wrapper.GetDateTime(0)).TotalSeconds < .5);
    }

    [TestMethod()]
    public async Task GetNameTest()
    {
      var process = new CustomProcessDisplay();
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows, m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FieldDelimiter, m_Setting.FieldQualifier, m_Setting.EscapePrefix, m_Setting.RecordLimit, m_Setting.AllowRowCombining, m_Setting.ContextSensitiveQualifier, m_Setting.CommentLine, m_Setting.NumWarnings, m_Setting.DuplicateQualifierToEscape, m_Setting.NewLinePlaceholder, m_Setting.DelimiterPlaceholder, m_Setting.QualifierPlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLfAsSpace, m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue, m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter, m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull, m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, process);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      // DateTime Integer Double Numeric String
      Assert.AreEqual("DateTime", wrapper.GetName(0));
      Assert.AreEqual("String", wrapper.GetName(4));
      Assert.AreEqual("TZ", wrapper.GetName(8));
    }

    [TestMethod()]
    public async Task GetOrdinalTest()
    {
      var process = new CustomProcessDisplay();
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows, m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FieldDelimiter, m_Setting.FieldQualifier, m_Setting.EscapePrefix, m_Setting.RecordLimit, m_Setting.AllowRowCombining, m_Setting.ContextSensitiveQualifier, m_Setting.CommentLine, m_Setting.NumWarnings, m_Setting.DuplicateQualifierToEscape, m_Setting.NewLinePlaceholder, m_Setting.DelimiterPlaceholder, m_Setting.QualifierPlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLfAsSpace, m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue, m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter, m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull, m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, process);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      // DateTime Integer Double Numeric String
      Assert.AreEqual(0, wrapper.GetOrdinal("DateTime"));
      Assert.AreEqual(4, wrapper.GetOrdinal("String"));
      Assert.AreEqual(8, wrapper.GetOrdinal("TZ"));
    }

    [TestMethod()]
    public async Task GetBooleanTest()
    {
      var process = new CustomProcessDisplay();
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows, m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FieldDelimiter, m_Setting.FieldQualifier, m_Setting.EscapePrefix, m_Setting.RecordLimit, m_Setting.AllowRowCombining, m_Setting.ContextSensitiveQualifier, m_Setting.CommentLine, m_Setting.NumWarnings, m_Setting.DuplicateQualifierToEscape, m_Setting.NewLinePlaceholder, m_Setting.DelimiterPlaceholder, m_Setting.QualifierPlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLfAsSpace, m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue, m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter, m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull, m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, process);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);

      Assert.IsTrue(wrapper.GetBoolean(6));
      await wrapper.ReadAsync(UnitTestStatic.Token);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      Assert.IsFalse(wrapper.GetBoolean(6));
    }

    [TestMethod()]
    public async Task GetFieldTypeTestAsync()
    {
      var process = new CustomProcessDisplay();
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows, m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FieldDelimiter, m_Setting.FieldQualifier, m_Setting.EscapePrefix, m_Setting.RecordLimit, m_Setting.AllowRowCombining, m_Setting.ContextSensitiveQualifier, m_Setting.CommentLine, m_Setting.NumWarnings, m_Setting.DuplicateQualifierToEscape, m_Setting.NewLinePlaceholder, m_Setting.DelimiterPlaceholder, m_Setting.QualifierPlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLfAsSpace, m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue, m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter, m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull, m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, process);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      // depending on type this might be int32 or int64
      Assert.IsTrue(nameof(Int64) == wrapper.GetDataTypeName(1) || nameof(Int32) == wrapper.GetDataTypeName(1));
    }

    [TestMethod()]
    public async Task MiscTest()
    {
      var process = new CustomProcessDisplay();
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows, m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FieldDelimiter, m_Setting.FieldQualifier, m_Setting.EscapePrefix, m_Setting.RecordLimit, m_Setting.AllowRowCombining, m_Setting.ContextSensitiveQualifier, m_Setting.CommentLine, m_Setting.NumWarnings, m_Setting.DuplicateQualifierToEscape, m_Setting.NewLinePlaceholder, m_Setting.DelimiterPlaceholder, m_Setting.QualifierPlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLfAsSpace, m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue, m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter, m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull, m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, process);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      Assert.IsTrue(wrapper.HasRows);
      try
      {
        var test = wrapper.GetEnumerator();
      }
      catch (NotImplementedException)
      {
        // ignore
      }

      try
      {
        wrapper.GetValues(new object[10]);
      }
      catch (NotImplementedException)
      {
        // ignore
      }
    }

    [TestMethod()]
    public async Task GetValueTest()
    {
      var process = new CustomProcessDisplay();
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows, m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FieldDelimiter, m_Setting.FieldQualifier, m_Setting.EscapePrefix, m_Setting.RecordLimit, m_Setting.AllowRowCombining, m_Setting.ContextSensitiveQualifier, m_Setting.CommentLine, m_Setting.NumWarnings, m_Setting.DuplicateQualifierToEscape, m_Setting.NewLinePlaceholder, m_Setting.DelimiterPlaceholder, m_Setting.QualifierPlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLfAsSpace, m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue, m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter, m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull, m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, process);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);

      Assert.AreEqual("-22477", wrapper.GetValue(1).ToString());
      Assert.AreEqual(-12086.66, wrapper.GetValue(2));
    }

    [TestMethod()]
    public async Task IsDBNullTest()
    {
      var process = new CustomProcessDisplay();
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows, m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FieldDelimiter, m_Setting.FieldQualifier, m_Setting.EscapePrefix, m_Setting.RecordLimit, m_Setting.AllowRowCombining, m_Setting.ContextSensitiveQualifier, m_Setting.CommentLine, m_Setting.NumWarnings, m_Setting.DuplicateQualifierToEscape, m_Setting.NewLinePlaceholder, m_Setting.DelimiterPlaceholder, m_Setting.QualifierPlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLfAsSpace, m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue, m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter, m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull, m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, process);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);

      // Date is empty but time column has a value
      Assert.IsFalse(wrapper.IsDBNull(0));
      await wrapper.ReadAsync(UnitTestStatic.Token);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      // this row does not have date nor time
      Assert.IsTrue(wrapper.IsDBNull(0));
    }

    [TestMethod()]
    public async Task GetSchemaTableTest()
    {
      var process = new CustomProcessDisplay();
      using var reader = new CsvFileReader(m_Setting.FullPath, m_Setting.CodePageId, m_Setting.SkipRows, m_Setting.HasFieldHeader, m_Setting.ColumnCollection, m_Setting.TrimmingOption, m_Setting.FieldDelimiter, m_Setting.FieldQualifier, m_Setting.EscapePrefix, m_Setting.RecordLimit, m_Setting.AllowRowCombining, m_Setting.ContextSensitiveQualifier, m_Setting.CommentLine, m_Setting.NumWarnings, m_Setting.DuplicateQualifierToEscape, m_Setting.NewLinePlaceholder, m_Setting.DelimiterPlaceholder, m_Setting.QualifierPlaceholder, m_Setting.SkipDuplicateHeader, m_Setting.TreatLfAsSpace, m_Setting.TreatUnknownCharacterAsSpace, m_Setting.TryToSolveMoreColumns, m_Setting.WarnDelimiterInValue, m_Setting.WarnLineFeed, m_Setting.WarnNBSP, m_Setting.WarnQuotes, m_Setting.WarnUnknownCharacter, m_Setting.WarnEmptyTailingColumns, m_Setting.TreatNBSPAsSpace, m_Setting.TreatTextAsNull, m_Setting.SkipEmptyLines, m_Setting.ConsecutiveEmptyRows, m_Setting.IdentifierInContainer, process);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);

      Assert.AreEqual(10 - 1, wrapper.GetSchemaTable().Rows.Count);
    }
  }
}