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
      var setting = UnitTestStatic.ReaderGetAllFormats();
      
      using var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows,
        setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FieldDelimiter,
        setting.FieldQualifier, setting.EscapePrefix, setting.RecordLimit, setting.AllowRowCombining,
        setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
        setting.NewLinePlaceholder, setting.DelimiterPlaceholder, setting.QualifierPlaceholder,
        setting.SkipDuplicateHeader, setting.TreatLfAsSpace, setting.TreatUnknownCharacterAsSpace,
        setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
        setting.WarnQuotes, setting.WarnUnknownCharacter, setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace,
        setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer,
        StandardTimeZoneAdjust.ChangeTimeZone, System.TimeZoneInfo.Local.Id);
      await reader.OpenAsync(UnitTestStatic.Token);
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