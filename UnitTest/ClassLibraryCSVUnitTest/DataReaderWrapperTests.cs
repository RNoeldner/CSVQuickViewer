using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class DataReaderWrapperTests
  {
    private static CsvFile setting = UnitTestHelper.ReaderGetAllFormats();
    [TestMethod()]
    public async Task GetColumnIndexFromErrorColumnTest()
    {
      using (var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FileFormat.FieldDelimiter, setting.FileFormat.FieldQualifier, setting.FileFormat.EscapeCharacter, setting.RecordLimit, setting.AllowRowCombining, setting.FileFormat.AlternateQuoting, setting.FileFormat.CommentLine, setting.NumWarnings, setting.FileFormat.DuplicateQuotingToEscape, setting.FileFormat.NewLinePlaceholder, setting.FileFormat.DelimiterPlaceholder, setting.FileFormat.QuotePlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, process))
      {
        await reader.OpenAsync(process.CancellationToken);
        var wrapper = new DataReaderWrapper(reader);

        // await wrapper.ReadAsync(process.CancellationToken);

        Assert.AreEqual(0, wrapper.ReaderToDataTable(0));
      }
    }

    [TestMethod()]
    public async Task DepthTest()
    {
      using (var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FileFormat.FieldDelimiter, setting.FileFormat.FieldQualifier, setting.FileFormat.EscapeCharacter, setting.RecordLimit, setting.AllowRowCombining, setting.FileFormat.AlternateQuoting, setting.FileFormat.CommentLine, setting.NumWarnings, setting.FileFormat.DuplicateQuotingToEscape, setting.FileFormat.NewLinePlaceholder, setting.FileFormat.DelimiterPlaceholder, setting.FileFormat.QuotePlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, process))
      {
        await reader.OpenAsync(process.CancellationToken);
        var wrapper = new DataReaderWrapper(reader);
        Assert.AreEqual(9, wrapper.Depth);
      }
    }

    [TestMethod()]
    public async Task GetIntegerTest()
    {
      using (var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FileFormat.FieldDelimiter, setting.FileFormat.FieldQualifier, setting.FileFormat.EscapeCharacter, setting.RecordLimit, setting.AllowRowCombining, setting.FileFormat.AlternateQuoting, setting.FileFormat.CommentLine, setting.NumWarnings, setting.FileFormat.DuplicateQuotingToEscape, setting.FileFormat.NewLinePlaceholder, setting.FileFormat.DelimiterPlaceholder, setting.FileFormat.QuotePlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, process))
      {
        await reader.OpenAsync(process.CancellationToken);
        var wrapper = new DataReaderWrapper(reader);
        await wrapper.ReadAsync(process.CancellationToken);
        Assert.AreEqual((short) -22477, wrapper.GetInt16(1));
        Assert.AreEqual(-22477, wrapper.GetInt32(1));
        Assert.AreEqual(-22477L, wrapper.GetInt64(1));
      }
    }

    [TestMethod()]
    public async Task GetNumericTest()
    {
      using (var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FileFormat.FieldDelimiter, setting.FileFormat.FieldQualifier, setting.FileFormat.EscapeCharacter, setting.RecordLimit, setting.AllowRowCombining, setting.FileFormat.AlternateQuoting, setting.FileFormat.CommentLine, setting.NumWarnings, setting.FileFormat.DuplicateQuotingToEscape, setting.FileFormat.NewLinePlaceholder, setting.FileFormat.DelimiterPlaceholder, setting.FileFormat.QuotePlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, process))
      {
        await reader.OpenAsync(process.CancellationToken);
        var wrapper = new DataReaderWrapper(reader);
        await wrapper.ReadAsync(process.CancellationToken);

        Assert.AreEqual((float) -12086.66, wrapper.GetFloat(2));
        Assert.AreEqual(-12086.66, wrapper.GetDouble(2));
        Assert.AreEqual((decimal) -12086.66, wrapper.GetDecimal(2));
        Assert.AreEqual((-12086.66).ToString(), wrapper.GetString(2));
      }
    }

    [TestMethod()]
    public async Task GetGuidTest()
    {
      using (var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FileFormat.FieldDelimiter, setting.FileFormat.FieldQualifier, setting.FileFormat.EscapeCharacter, setting.RecordLimit, setting.AllowRowCombining, setting.FileFormat.AlternateQuoting, setting.FileFormat.CommentLine, setting.NumWarnings, setting.FileFormat.DuplicateQuotingToEscape, setting.FileFormat.NewLinePlaceholder, setting.FileFormat.DelimiterPlaceholder, setting.FileFormat.QuotePlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, process))
      {
        await reader.OpenAsync(process.CancellationToken);
        var wrapper = new DataReaderWrapper(reader);
        await wrapper.ReadAsync(process.CancellationToken);

        Assert.AreEqual(new Guid("1BD10E34-7D66-481B-A7E3-AE817B5BEE02"), wrapper.GetGuid(7));
        Assert.AreEqual("1BD10E34-7D66-481B-A7E3-AE817B5BEE02", wrapper.GetString(7).ToUpper());
      }
    }

    [TestMethod()]
    public async Task GetDateTimeTest()
    {
      using (var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FileFormat.FieldDelimiter, setting.FileFormat.FieldQualifier, setting.FileFormat.EscapeCharacter, setting.RecordLimit, setting.AllowRowCombining, setting.FileFormat.AlternateQuoting, setting.FileFormat.CommentLine, setting.NumWarnings, setting.FileFormat.DuplicateQuotingToEscape, setting.FileFormat.NewLinePlaceholder, setting.FileFormat.DelimiterPlaceholder, setting.FileFormat.QuotePlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, process))
      {
        await reader.OpenAsync(process.CancellationToken);
        var wrapper = new DataReaderWrapper(reader);
        await wrapper.ReadAsync(process.CancellationToken);
        await wrapper.ReadAsync(process.CancellationToken);
        Assert.IsTrue((new DateTime(2014, 05, 23) - wrapper.GetDateTime(0)).TotalSeconds < .5);
      }
    }

    [TestMethod()]
    public async Task GetNameTest()
    {
      using (var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FileFormat.FieldDelimiter, setting.FileFormat.FieldQualifier, setting.FileFormat.EscapeCharacter, setting.RecordLimit, setting.AllowRowCombining, setting.FileFormat.AlternateQuoting, setting.FileFormat.CommentLine, setting.NumWarnings, setting.FileFormat.DuplicateQuotingToEscape, setting.FileFormat.NewLinePlaceholder, setting.FileFormat.DelimiterPlaceholder, setting.FileFormat.QuotePlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, process))
      {
        await reader.OpenAsync(process.CancellationToken);
        var wrapper = new DataReaderWrapper(reader);
        await wrapper.ReadAsync(process.CancellationToken);
        // DateTime Integer Double Numeric String
        Assert.AreEqual("DateTime", wrapper.GetName(0));
        Assert.AreEqual("String", wrapper.GetName(4));
        Assert.AreEqual("TZ", wrapper.GetName(8));
      }
    }

    [TestMethod()]
    public async Task GetOrdinalTest()
    {
      using (var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FileFormat.FieldDelimiter, setting.FileFormat.FieldQualifier, setting.FileFormat.EscapeCharacter, setting.RecordLimit, setting.AllowRowCombining, setting.FileFormat.AlternateQuoting, setting.FileFormat.CommentLine, setting.NumWarnings, setting.FileFormat.DuplicateQuotingToEscape, setting.FileFormat.NewLinePlaceholder, setting.FileFormat.DelimiterPlaceholder, setting.FileFormat.QuotePlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, process))
      {
        await reader.OpenAsync(process.CancellationToken);
        var wrapper = new DataReaderWrapper(reader);
        await wrapper.ReadAsync(process.CancellationToken);
        // DateTime Integer Double Numeric String
        Assert.AreEqual(0, wrapper.GetOrdinal("DateTime"));
        Assert.AreEqual(4, wrapper.GetOrdinal("String"));
        Assert.AreEqual(8, wrapper.GetOrdinal("TZ"));
      }
    }

    [TestMethod()]
    public async Task GetBooleanTest()
    {
      using (var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FileFormat.FieldDelimiter, setting.FileFormat.FieldQualifier, setting.FileFormat.EscapeCharacter, setting.RecordLimit, setting.AllowRowCombining, setting.FileFormat.AlternateQuoting, setting.FileFormat.CommentLine, setting.NumWarnings, setting.FileFormat.DuplicateQuotingToEscape, setting.FileFormat.NewLinePlaceholder, setting.FileFormat.DelimiterPlaceholder, setting.FileFormat.QuotePlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, process))
      {
        await reader.OpenAsync(process.CancellationToken);
        var wrapper = new DataReaderWrapper(reader);
        await wrapper.ReadAsync(process.CancellationToken);

        Assert.IsTrue(wrapper.GetBoolean(6));
        await wrapper.ReadAsync(process.CancellationToken);
        await wrapper.ReadAsync(process.CancellationToken);
        await wrapper.ReadAsync(process.CancellationToken);
        Assert.IsFalse(wrapper.GetBoolean(6));
      }
    }

    [TestMethod()]
    public async Task GetFieldTypeTestAsync()
    {
      using (var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FileFormat.FieldDelimiter, setting.FileFormat.FieldQualifier, setting.FileFormat.EscapeCharacter, setting.RecordLimit, setting.AllowRowCombining, setting.FileFormat.AlternateQuoting, setting.FileFormat.CommentLine, setting.NumWarnings, setting.FileFormat.DuplicateQuotingToEscape, setting.FileFormat.NewLinePlaceholder, setting.FileFormat.DelimiterPlaceholder, setting.FileFormat.QuotePlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, process))
      {
        await reader.OpenAsync(process.CancellationToken);
        var wrapper = new DataReaderWrapper(reader);
        await wrapper.ReadAsync(process.CancellationToken);
        // depending on type this might be int32 or int64
        Assert.IsTrue(nameof(Int64) == wrapper.GetDataTypeName(1) || nameof(Int32) == wrapper.GetDataTypeName(1));
      }
    }

    [TestMethod()]
    public async Task MiscTest()
    {
      using (var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FileFormat.FieldDelimiter, setting.FileFormat.FieldQualifier, setting.FileFormat.EscapeCharacter, setting.RecordLimit, setting.AllowRowCombining, setting.FileFormat.AlternateQuoting, setting.FileFormat.CommentLine, setting.NumWarnings, setting.FileFormat.DuplicateQuotingToEscape, setting.FileFormat.NewLinePlaceholder, setting.FileFormat.DelimiterPlaceholder, setting.FileFormat.QuotePlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, process))
      {
        await reader.OpenAsync(process.CancellationToken);
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
    }

    [TestMethod()]
    public async Task GetValueTest()
    {
      using (var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FileFormat.FieldDelimiter, setting.FileFormat.FieldQualifier, setting.FileFormat.EscapeCharacter, setting.RecordLimit, setting.AllowRowCombining, setting.FileFormat.AlternateQuoting, setting.FileFormat.CommentLine, setting.NumWarnings, setting.FileFormat.DuplicateQuotingToEscape, setting.FileFormat.NewLinePlaceholder, setting.FileFormat.DelimiterPlaceholder, setting.FileFormat.QuotePlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, process))
      {
        await reader.OpenAsync(process.CancellationToken);
        var wrapper = new DataReaderWrapper(reader);
        await wrapper.ReadAsync(process.CancellationToken);

        Assert.AreEqual("-22477", wrapper.GetValue(1).ToString());
        Assert.AreEqual(-12086.66, wrapper.GetValue(2));
      }
    }

    [TestMethod()]
    public async Task IsDBNullTest()
    {
      using (var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FileFormat.FieldDelimiter, setting.FileFormat.FieldQualifier, setting.FileFormat.EscapeCharacter, setting.RecordLimit, setting.AllowRowCombining, setting.FileFormat.AlternateQuoting, setting.FileFormat.CommentLine, setting.NumWarnings, setting.FileFormat.DuplicateQuotingToEscape, setting.FileFormat.NewLinePlaceholder, setting.FileFormat.DelimiterPlaceholder, setting.FileFormat.QuotePlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, process))
      {
        await reader.OpenAsync(process.CancellationToken);
        var wrapper = new DataReaderWrapper(reader);
        await wrapper.ReadAsync(process.CancellationToken);

        // Date is empty but time column has a value
        Assert.IsFalse(wrapper.IsDBNull(0));
        await wrapper.ReadAsync(process.CancellationToken);
        await wrapper.ReadAsync(process.CancellationToken);
        await wrapper.ReadAsync(process.CancellationToken);
        await wrapper.ReadAsync(process.CancellationToken);
        // this row does not have date nor time
        Assert.IsTrue(wrapper.IsDBNull(0));
      }
    }

    [TestMethod()]
    public async Task GetSchemaTableTest()
    {
      using (var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FileFormat.FieldDelimiter, setting.FileFormat.FieldQualifier, setting.FileFormat.EscapeCharacter, setting.RecordLimit, setting.AllowRowCombining, setting.FileFormat.AlternateQuoting, setting.FileFormat.CommentLine, setting.NumWarnings, setting.FileFormat.DuplicateQuotingToEscape, setting.FileFormat.NewLinePlaceholder, setting.FileFormat.DelimiterPlaceholder, setting.FileFormat.QuotePlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, process))
      {
        await reader.OpenAsync(process.CancellationToken);
        var wrapper = new DataReaderWrapper(reader);

        Assert.AreEqual(10 - 1, wrapper.GetSchemaTable().Rows.Count);
      }
    }
  }
}