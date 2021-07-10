using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class DynamicDataRecordTest
  {
    [TestMethod()]
    public async Task GetDynamicMemberNames()
    {
      var setting = UnitTestHelper.ReaderGetAllFormats();
      using var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token);
      using var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FileFormat.FieldDelimiter, setting.FileFormat.FieldQualifier, setting.FileFormat.EscapeCharacter, setting.RecordLimit, setting.AllowRowCombining, setting.FileFormat.AlternateQuoting, setting.FileFormat.CommentLine, setting.NumWarnings, setting.FileFormat.DuplicateQuotingToEscape, setting.FileFormat.NewLinePlaceholder, setting.FileFormat.DelimiterPlaceholder, setting.FileFormat.QuotePlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, process);
      await reader.OpenAsync(UnitTestInitializeCsv.Token);
      await reader.ReadAsync();

      dynamic test = new DynamicDataRecord(reader);
      //Assert.AreEqual(-22477, test.Integer);

      await reader.ReadAsync();
      test = new DynamicDataRecord(reader);
      //Assert.AreEqual("zehzcv", test.String);
      //test.String = "2222";
      //Assert.AreEqual("2222", test.String);
    }
  }
}