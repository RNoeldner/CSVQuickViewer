using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  [SuppressMessage("ReSharper", "UseAwaitUsing")]
  public class ColumnErrorDictionaryTests
  {
    [TestMethod]
    public async Task ColumnErrorDictionaryTest1Async()
    {
      var setting = new CsvFileDummy(fileName: UnitTestStatic.GetTestPath("Sessions.txt"))
      {
        HasFieldHeader = true, ByteOrderMark = true, FieldDelimiterChar = '\t'
      };
      setting.ColumnCollection.Add(new Column("Start Date", ValueFormat.Empty, ignore: true));

      using var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows,
        setting.HasFieldHeader, setting.ColumnCollection,
        setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
        setting.RecordLimit, setting.AllowRowCombining,
        setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
        setting.NewLinePlaceholder,
        setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
        setting.TreatUnknownCharacterAsSpace,
        setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
        setting.WarnQuotes, setting.WarnUnknownCharacter,
        setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
        setting.ConsecutiveEmptyRows,
        setting.IdentifierInContainer, StandardTimeZoneAdjust.ChangeTimeZone, System.TimeZoneInfo.Local.Id, true, false);
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

      test1.Add(0, "Another");
      Assert.IsTrue(test1.Display.StartsWith("Message" + ErrorInformation.cSeparator) ||
                    test1.Display.EndsWith(ErrorInformation.cSeparator + "Message"),
        $"Search Message :{test1.Display}");
      Assert.IsTrue(test1.Display.StartsWith("Another" + ErrorInformation.cSeparator) ||
                    test1.Display.EndsWith(ErrorInformation.cSeparator + "Another"),
        $"Search Another :{test1.Display}");
    }
  }
}