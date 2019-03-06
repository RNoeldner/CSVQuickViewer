using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CsvTools.Tests
{
  [TestClass]
  public class StringUtilsTests
  {
    [TestMethod]
    public void CountOccurance()
    {
      Assert.AreEqual(0, StringUtils.CountOccurance("", "."));
      Assert.AreEqual(1, StringUtils.CountOccurance(",.,", "."));
      Assert.AreEqual(2, StringUtils.CountOccurance(",.,", ","));
    }

    [TestMethod]
    public void Join()
    {
      Assert.AreEqual("", StringUtils.Join(new string[] { }, ","));
      Assert.AreEqual("2", StringUtils.Join(new string[] { "2" }, ","));
      Assert.AreEqual("2,3", StringUtils.Join(new string[] { "2", "3" }, ","));
      Assert.AreEqual("2; 3", StringUtils.Join(new string[] { "2", "3" }, "; "));
    }

    [TestMethod]
    public void ColumnNameEndsOnID()
    {
      Assert.AreEqual(0, StringUtils.AssumeIDColumn(null));
      Assert.AreEqual(0, StringUtils.AssumeIDColumn(" "));

      Assert.AreEqual(3, StringUtils.AssumeIDColumn("Rating ID"));
      Assert.AreEqual(2, StringUtils.AssumeIDColumn("RatingId"));
      Assert.AreEqual(0, StringUtils.AssumeIDColumn("Acid"));

      Assert.AreEqual(4, StringUtils.AssumeIDColumn("Rating Ref"));
      Assert.AreEqual(3, StringUtils.AssumeIDColumn("RatingRef"));

      Assert.AreEqual(5, StringUtils.AssumeIDColumn("Rating Text"));
      Assert.AreEqual(4, StringUtils.AssumeIDColumn("RatingText"));
      Assert.AreEqual(0, StringUtils.AssumeIDColumn("Videotext"));
    }

    [TestMethod]
    public void GetShortDisplay()
    {
      Assert.AreEqual(string.Empty, StringUtils.GetShortDisplay(null, 0));
      Assert.AreEqual("12345", StringUtils.GetShortDisplay("12345", 5));
      Assert.AreEqual("1234…", StringUtils.GetShortDisplay("123456789", 5));
      Assert.AreEqual("1234…", StringUtils.GetShortDisplay("1234 56789", 5));
      Assert.AreEqual("1234…", StringUtils.GetShortDisplay("1234 56789", 6));
      Assert.AreEqual("1234 5…", StringUtils.GetShortDisplay("1234 5 6789", 8));
    }

    [TestMethod]
    public void GetShortDisplaySQL()
    {
      Assert.AreEqual(string.Empty, StringUtils.GetShortDisplaySQL(null, 0));
      Assert.AreEqual("12345", StringUtils.GetShortDisplaySQL("12345", 5));
      Assert.AreEqual("12345…", StringUtils.GetShortDisplaySQL("123456789", 5));
      Assert.AreEqual("", StringUtils.GetShortDisplaySQL(null, 10));
      Assert.AreEqual("", StringUtils.GetShortDisplaySQL(string.Empty, 10));
      Assert.AreEqual("", StringUtils.GetShortDisplaySQL(" ", 10));
      Assert.AreEqual("1234567", StringUtils.GetShortDisplaySQL("1234567", 10));
      Assert.AreEqual("1234567890…", StringUtils.GetShortDisplaySQL("12345678901234567890", 10));
    }

    [TestMethod]
    public void GetShortDisplayFileNameLongFileName()
    {
      Assert.AreEqual(20,
        FileSystemUtils.GetShortDisplayFileName("ThisIsALongFileNameThatNeedsToBeShorter.txt", 20).Length);
    }

    [TestMethod]
    public void GetShortDisplayFileNameNull()
    {
      Assert.IsNull(FileSystemUtils.GetShortDisplayFileName(null, 100));
      Assert.AreEqual(string.Empty, FileSystemUtils.GetShortDisplayFileName(string.Empty, 100));
    }

    [TestMethod]
    public void GetShortDisplayOk()
    {
      var test = @"C:\Dir2\dir3\dir4\dir5\dir6\file.ext";

      Assert.AreEqual(test, FileSystemUtils.GetShortDisplayFileName(test, 100));
      Assert.AreEqual(test, FileSystemUtils.GetShortDisplayFileName(test, test.Length));
      Assert.AreEqual(@"C:\Dir2\…\dir5\dir6\file.ext", FileSystemUtils.GetShortDisplayFileName(test, test.Length - 1));

      var test2 = @"file.ext";
      Assert.AreEqual(test2, FileSystemUtils.GetShortDisplayFileName(test2, 80));

      var test3 = @"averylongfilenamethat needtobecut.ext";
      Assert.AreEqual(20, FileSystemUtils.GetShortDisplayFileName(test3, 20).Length);
    }

    [TestMethod]
    public void GetTrimmedUpperValue()
    {
      Assert.AreEqual(string.Empty, StringUtils.GetTrimmedUpperValue(null));
      Assert.AreEqual(string.Empty, StringUtils.GetTrimmedUpperValue(" "));
      Assert.AreEqual("AABB", StringUtils.GetTrimmedUpperValue("aabb"));
      Assert.AreEqual("AA BB", StringUtils.GetTrimmedUpperValue(" aa bb  "));
    }

    [TestMethod]
    public void GetTrimmedUpperValueTest()
    {
      Assert.AreEqual("", StringUtils.GetTrimmedUpperValue(null));
      Assert.AreEqual("", StringUtils.GetTrimmedUpperValue(DBNull.Value));
      Assert.AreEqual("", StringUtils.GetTrimmedUpperValue(string.Empty));
      Assert.AreEqual("", StringUtils.GetTrimmedUpperValue(" "));
      Assert.AreEqual("12", StringUtils.GetTrimmedUpperValue(12));
      Assert.AreEqual("A TEST", StringUtils.GetTrimmedUpperValue(" a Test "));
    }

    [TestMethod]
    public void HandleCRLFCombinationsTest()
    {
      Assert.AreEqual("+#+", StringUtils.HandleCRLFCombinations("\r#\n", "+"));
      Assert.AreEqual("+#+", StringUtils.HandleCRLFCombinations("\r\n#\n\r", "+"));
      Assert.AreEqual("++", StringUtils.HandleCRLFCombinations("\r\n\n\r", "+"));
      Assert.AreEqual("", StringUtils.HandleCRLFCombinations("", "+"));
    }

    [TestMethod]
    public void HtmlEncodeNull()
    {
      Assert.IsNull(HTMLStyle.HtmlEncode(null));
      Assert.AreEqual(string.Empty, HTMLStyle.HtmlEncode(string.Empty));
    }

    [TestMethod]
    public void HtmlEncodeOK()
    {
      var test = @"Only plain ACSII";
      Assert.AreEqual(test, HTMLStyle.HtmlEncode(test));
      Assert.AreEqual("&quot;", HTMLStyle.HtmlEncode("\""));
      Assert.AreEqual("&amp;", HTMLStyle.HtmlEncode("&"));

      Assert.IsTrue(
        "&aeuml;" == HTMLStyle.HtmlEncode("ä") ||
        "&#228;" == HTMLStyle.HtmlEncode("ä"));
    }

    [TestMethod]
    public void HtmlEncodeShortLineFeed()
    {
      Assert.AreEqual("Dies ist<br>Test", HTMLStyle.HtmlEncodeShort("Dies ist\r\nTest"));
      Assert.AreEqual("Dies ist<br>Test", HTMLStyle.HtmlEncodeShort("Dies ist\n\rTest"));
      Assert.AreEqual("Dies ist<br>Test", HTMLStyle.HtmlEncodeShort("Dies ist\rTest"));
      Assert.AreEqual("Dies ist<br>Test", HTMLStyle.HtmlEncodeShort("Dies ist\nTest"));
      Assert.AreEqual("Dies ist<br><br>Test", HTMLStyle.HtmlEncodeShort("Dies ist\r\rTest"));
    }

    [TestMethod]
    public void HtmlEncodeShortNull()
    {
      Assert.IsNull(HTMLStyle.HtmlEncodeShort(null));
      Assert.AreEqual(string.Empty, HTMLStyle.HtmlEncodeShort(string.Empty));
    }

    [TestMethod]
    public void HtmlEncodeShortOK()
    {
      var test = @"Only plain ACSII";
      Assert.AreEqual(test, HTMLStyle.HtmlEncodeShort(test));
      Assert.AreEqual("&quot;", HTMLStyle.HtmlEncodeShort("\""));
      Assert.AreEqual("&amp;", HTMLStyle.HtmlEncodeShort("&"));

      Assert.AreEqual("ä", HTMLStyle.HtmlEncodeShort("ä"));
    }

    [TestMethod]
    public void MakeUniqueInCollectionTest()
    {
      var lst = new List<string>();
      lst.Add("Value");
      lst.Add(null);
      lst.Add("");
      Assert.AreEqual("Value1", StringUtils.MakeUniqueInCollection(lst, "Value"));
      Assert.AreEqual("New", StringUtils.MakeUniqueInCollection(lst, "New"));
    }

    [TestMethod]
    public void NoSpecials()
    {
      Assert.AreEqual(string.Empty, StringUtils.NoSpecials(null));
      Assert.AreEqual(string.Empty, " ".NoSpecials());
      Assert.AreEqual("aabb", "aabb".NoSpecials());
      Assert.AreEqual("12", "12_&§$".NoSpecials());
      Assert.AreEqual("aabb", " aa_bb  ".NoSpecials());
    }

    [TestMethod]
    public void OnlyText()
    {
      Assert.AreEqual("Noldner", "Nöldner".OnlyText());
    }

    [TestMethod]
    public void OnlyTextNull()
    {
      Assert.AreEqual(string.Empty, StringUtils.OnlyText(null));
      Assert.AreEqual(string.Empty, string.Empty.OnlyText());
    }

    [TestMethod]
    public void SafeFileName()
    {
      Assert.AreEqual(@"", @"".SafePath());
      Assert.AreEqual(@"", FileSystemUtils.SafePath(null));

      Assert.AreEqual(@"c:\Users\rnoldner\Documents\Kunden\Sample\Settings.ValidationTask",
        @"c:\Users\rnoldner\Documents\Kunden\Sample\Settings.ValidationTask".SafePath());
      Assert.AreEqual(@"c:\Users\rnoldner\Documents\Kunden\Sample\Settings.ValidationTask",
        @"c:\Users\rnoldner\Documents\Kunden\Sample\Set:tings.Validation*Task".SafePath());
      Assert.AreEqual(@"c:\Users\rnoldner\Documents\Kunden\Sample\Settings.ValidationTask",
        @"c:\Users\rno>ldner\Documents\Kunden\Sample\Set:tings.Validat?ionTask".SafePath());
    }

    [TestMethod]
    public void SemicolonSplit()
    {
      Assert.AreEqual(0, StringUtils.SplitValidValues(null).Length);
      Assert.AreEqual(0, StringUtils.SplitValidValues("").Length);
      Assert.AreEqual(1, StringUtils.SplitValidValues("A,B").Length);
      Assert.AreEqual(2, StringUtils.SplitValidValues("A;B").Length);
      Assert.AreEqual("A", StringUtils.SplitValidValues("A;B")[0]);
      Assert.AreEqual("B", StringUtils.SplitValidValues("A;B")[1]);
    }

    [TestMethod]
    public void ShouldBeTreatedAsNull()
    {
      Assert.IsTrue(StringUtils.ShouldBeTreatedAsNull(null, "NULL"), "Value null");
      Assert.IsTrue(StringUtils.ShouldBeTreatedAsNull("", "NULL"), "");
      Assert.IsFalse(StringUtils.ShouldBeTreatedAsNull("nul", "NULL"), "nul");
      Assert.IsFalse(StringUtils.ShouldBeTreatedAsNull("isNull", "NULL"), "isNull");
      Assert.IsFalse(StringUtils.ShouldBeTreatedAsNull("[Null]", "NULL"), "[Null]");
      Assert.IsTrue(StringUtils.ShouldBeTreatedAsNull("null", "NULL"), "null");
      Assert.IsFalse(StringUtils.ShouldBeTreatedAsNull(" null ", "NULL"), " null ");
      Assert.IsTrue(StringUtils.ShouldBeTreatedAsNull("Null", "NULL"), "Null");

      Assert.IsTrue(StringUtils.ShouldBeTreatedAsNull("N/A", "NULL;N/A"), "N/A");
      Assert.IsTrue(StringUtils.ShouldBeTreatedAsNull("NULL", "NULL;N/A"), "NULL");
    }

    [TestMethod]
    public void SqlIsKeyword()
    {
      Assert.AreEqual(false, StringUtilsSQL.SqlIsKeyword(null));
      Assert.AreEqual(false, StringUtilsSQL.SqlIsKeyword(string.Empty));
      Assert.AreEqual(false, StringUtilsSQL.SqlIsKeyword("Raphael"));
      Assert.AreEqual(true, StringUtilsSQL.SqlIsKeyword("TABLE"));
      Assert.AreEqual(true, StringUtilsSQL.SqlIsKeyword("Table"));
    }

    [TestMethod]
    public void SqlNameNeedsQuoting()
    {
      Assert.AreEqual(false, StringUtilsSQL.SqlNameNeedsQuoting(null));
      Assert.AreEqual(false, StringUtilsSQL.SqlNameNeedsQuoting(string.Empty));
      Assert.AreEqual(false, StringUtilsSQL.SqlNameNeedsQuoting("Raphael"));
      Assert.AreEqual(true, StringUtilsSQL.SqlNameNeedsQuoting("Raphael.Noeldner"));
      Assert.AreEqual(true, StringUtilsSQL.SqlNameNeedsQuoting("Raphael Nöldner"));
      Assert.AreEqual(false, StringUtilsSQL.SqlNameNeedsQuoting("RaphaelNoeldner"));
      Assert.AreEqual(false, StringUtilsSQL.SqlNameNeedsQuoting("RaphaelNöldner"));
      Assert.AreEqual(true, StringUtilsSQL.SqlNameNeedsQuoting("TABLE"));
    }

    [TestMethod]
    public void SqlNameNeedsQuotingTest()
    {
      Assert.IsFalse(StringUtilsSQL.SqlNameNeedsQuoting(null));
      Assert.IsFalse(StringUtilsSQL.SqlNameNeedsQuoting(""));
      Assert.IsTrue(StringUtilsSQL.SqlNameNeedsQuoting("Test Value"));
      Assert.IsTrue(StringUtilsSQL.SqlNameNeedsQuoting("Test.Value"));
      Assert.IsTrue(StringUtilsSQL.SqlNameNeedsQuoting("1Test"));
    }

    [TestMethod]
    public void SQLNameNull()
    {
      Assert.AreEqual(string.Empty, StringUtilsSQL.SqlName(null));
      Assert.AreEqual(string.Empty, StringUtilsSQL.SqlName(string.Empty));
    }

    [TestMethod]
    public void SQLNameOK()
    {
      Assert.AreEqual("A", StringUtilsSQL.SqlName("A"));
      var ret = StringUtilsSQL.SqlName("TryInjection]");

      Assert.IsTrue(ret == "TryInjection\\]" || ret == "TryInjection]]");
    }

    [TestMethod]
    public void SqlNameSafe()
    {
      Assert.IsTrue(string.IsNullOrEmpty(StringUtilsSQL.SqlNameSafe(null)));
      Assert.IsTrue(string.IsNullOrEmpty(StringUtilsSQL.SqlNameSafe(string.Empty)));
      Assert.AreEqual("[Raphael.Noeldner]", StringUtilsSQL.SqlNameSafe("Raphael.Noeldner"));
      Assert.AreEqual("[Table]", StringUtilsSQL.SqlNameSafe("Table"));
    }

    [TestMethod]
    public void SQLQuoteNull()
    {
      Assert.AreEqual(string.Empty, StringUtilsSQL.SqlQuote(null));
      Assert.AreEqual(string.Empty, StringUtilsSQL.SqlQuote(string.Empty));
    }

    [TestMethod]
    public void SQLQuoteOK()
    {
      Assert.AreEqual("A", StringUtilsSQL.SqlQuote("A"));
      Assert.AreEqual("TryInjection]", StringUtilsSQL.SqlQuote("TryInjection]"));
      Assert.AreEqual("Try''Injection", StringUtilsSQL.SqlQuote("Try'Injection"));
    }

    [TestMethod]
    public void TDNull()
    {
      Assert.IsNull(HTMLStyle.AddTd(null, null));
      Assert.IsNull(HTMLStyle.AddTd(null));
      Assert.AreEqual(string.Empty, HTMLStyle.AddTd(string.Empty));
    }

    [TestMethod]
    public void TDOK()
    {
      Assert.AreEqual("AXB", HTMLStyle.AddTd("A{0}B", "X"));
      Assert.AreEqual("AX.YB", HTMLStyle.AddTd("A{0}.{1}B", "X", "Y"));
      Assert.AreEqual("<td>&gt;<br>&lt;</td>", HTMLStyle.AddTd("<td>{0}<br>{1}</td>", ">", "<"));
    }

    [TestMethod]
    public void TextToHtmlEncode()
    {
      Assert.IsNull(HTMLStyle.TextToHtmlEncode(null));
      Assert.AreEqual("", HTMLStyle.TextToHtmlEncode(""));
      Assert.AreEqual("This is a test", HTMLStyle.TextToHtmlEncode("This is a test"));
      Assert.AreEqual("This is a test", HTMLStyle.TextToHtmlEncode("This is a\ttest"));
      Assert.AreEqual("This is a<br>test", HTMLStyle.TextToHtmlEncode("This is a\ntest"));
    }
  }
}