using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class ColumnErrorDictionaryTests
  {
    [TestMethod]
    public async Task ColumnErrorDictionaryTest1Async()
    {
      var setting = new CsvFile { FileName = UnitTestStatic.GetTestPath("Sessions.txt"), HasFieldHeader = true, ByteOrderMark = true, FieldDelimiter = "\t" };
      setting.ColumnCollection.Add(new Column("Start Date") { Ignore = true });

      var processDisplay = new CustomProcessDisplay();
      using var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection,
        setting.TrimmingOption, setting.FieldDelimiter, setting.FieldQualifier, setting.EscapePrefix, setting.RecordLimit, setting.AllowRowCombining,
        setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape, setting.NewLinePlaceholder,
        setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace, setting.TreatUnknownCharacterAsSpace,
        setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter,
        setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows,
        setting.IdentifierInContainer, StandardTimeZoneAdjust.ChangeTimeZone, System.TimeZoneInfo.Local.Id, processDisplay);
      await reader.OpenAsync(UnitTestStatic.Token);
      var test1 = new ColumnErrorDictionary(reader);
      Assert.IsNotNull(test1);

      // Message in ignored column
      reader.HandleWarning(0, "Msg1");
      reader.HandleError(1, "Msg2");

      Assert.AreEqual("Msg2", test1.Display);
    }

    [TestMethod]
    public void AddTest()
    {
      var test1 = new ColumnErrorDictionary();
      Assert.IsNotNull(test1);
      test1.Add(0, "Message");
      Assert.AreEqual("Message", test1.Display);
      test1.Add(0, "Another Message");
      Assert.AreEqual("Message" + ErrorInformation.cSeparator + "Another Message", test1.Display);
    }
  }
}