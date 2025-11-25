/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace CsvTools.Tests;

[TestClass]
public class StringUtilsTests
{
  [TestMethod]
  public void Join()
  {
    var test = new[] { "this", "is", "a" }.Join(", ");
    Assert.AreEqual("this, is, a", test);

    var test2 = new[] { "Hello", "World" }.Join('|');
    Assert.AreEqual("Hello|World", test2);

    var test3 = new List<string>().Join("*");
    Assert.AreEqual("", test3);

    Assert.AreEqual("", new string[] { }.Join(","));
    Assert.AreEqual("2", new[] { "2" }.Join(","));
    Assert.AreEqual("2,3", new[] { "2", "3" }.Join(","));
    Assert.AreEqual("2; 3", new[] { "2", "3" }.Join("; "));
  }

  [TestMethod]
  public void PassesFilter()
  {
    Assert.AreEqual(true, "".PassesFilter(""));
    Assert.AreEqual(true, "This is a test".PassesFilter("test"));
    Assert.AreEqual(true, "This is a test".PassesFilter("This"));
    Assert.AreEqual(true, "This is a test".PassesFilter("This +test"));
    Assert.AreEqual(true, "This is a test".PassesFilter("The+test"));
    Assert.AreEqual(false, "This is a test".PassesFilter("+The+test"));
    Assert.AreEqual(true, "This is a test".PassesFilter("+"));
  }

  [TestMethod]
  public void ColumnNameEndsOnID()
  {
    Assert.AreEqual(0, StringUtils.AssumeIdColumn(" "));

    Assert.AreEqual(3, StringUtils.AssumeIdColumn("Rating ID"));
    Assert.AreEqual(2, StringUtils.AssumeIdColumn("RatingId"));
    Assert.AreEqual(0, StringUtils.AssumeIdColumn("Acid"));

    Assert.AreEqual(4, StringUtils.AssumeIdColumn("Rating Ref"));
    Assert.AreEqual(3, StringUtils.AssumeIdColumn("RatingRef"));

    Assert.AreEqual(5, StringUtils.AssumeIdColumn("Rating Text"));
    Assert.AreEqual(4, StringUtils.AssumeIdColumn("RatingText"));
    Assert.AreEqual(0, StringUtils.AssumeIdColumn("Videotext"));
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
  public void GetShortDisplayFileNameLongFileName() => Assert.AreEqual(20,
    FileSystemUtils.GetShortDisplayFileName("ThisIsALongFileNameThatNeedsToBeShorter.txt", 20).Length);

  [TestMethod]
  public void GetShortDisplayFileNameNull() =>
    Assert.AreEqual(string.Empty, FileSystemUtils.GetShortDisplayFileName(string.Empty, 100));

  [TestMethod]
  public void GetShortDisplayOk()
  {
    var test = Path.Combine(@"C:" + Path.DirectorySeparatorChar, "Dir2", "dir3", "dir4", "dir5", "dir6", "file.ext");
    Assert.AreEqual(test, FileSystemUtils.GetShortDisplayFileName(test, test.Length));
#if Windows
      Assert.AreEqual($"C:{Path.DirectorySeparatorChar}Dir2{Path.DirectorySeparatorChar}…{Path.DirectorySeparatorChar}dir5{Path.DirectorySeparatorChar}dir6{Path.DirectorySeparatorChar}file.ext", FileSystemUtils.GetShortDisplayFileName(test, test.Length - 1));
#endif
    var test2 = @"file.ext";
    Assert.AreEqual(test2, FileSystemUtils.GetShortDisplayFileName(test2));

    var test3 = @"averylongfilenamethat needtobecut.ext";
    Assert.AreEqual(20, FileSystemUtils.GetShortDisplayFileName(test3, 20).Length);
  }

  [TestMethod]
  public void HandleCrlfCombinationsTest()
  {
    Assert.AreEqual("+#+", "\r#\n".HandleCrlfCombinations("+"));
    Assert.AreEqual("+#+", "\r\n#\n\r".HandleCrlfCombinations("+"));
    Assert.AreEqual("++", "\r\n\n\r".HandleCrlfCombinations("+"));
    Assert.AreEqual("", "".HandleCrlfCombinations("+"));
  }

  [TestMethod]
  public void HtmlEncodeNull() =>
    // Assert.IsNull(HTMLStyle.HtmlEncode(null));
    Assert.AreEqual(string.Empty, HtmlStyle.HtmlEncode(string.Empty));

  [TestMethod]
  public void HtmlEncodeOk()
  {
    var test = @"Only plain ACSII";
    Assert.AreEqual(test, HtmlStyle.HtmlEncode(test));
    Assert.AreEqual("&quot;", HtmlStyle.HtmlEncode("\""));
    Assert.AreEqual("&amp;", HtmlStyle.HtmlEncode("&"));

    Assert.IsTrue(
      "&aeuml;" == HtmlStyle.HtmlEncode("ä") ||
      "&#228;" == HtmlStyle.HtmlEncode("ä"));
  }

  [TestMethod]
  public void HtmlEncodeShortLineFeed()
  {
    Assert.AreEqual("Dies ist<br>Test", HtmlStyle.HtmlEncodeShort("Dies ist\r\nTest"));
    Assert.AreEqual("Dies ist<br>Test", HtmlStyle.HtmlEncodeShort("Dies ist\n\rTest"));
    Assert.AreEqual("Dies ist<br>Test", HtmlStyle.HtmlEncodeShort("Dies ist\rTest"));
    Assert.AreEqual("Dies ist<br>Test", HtmlStyle.HtmlEncodeShort("Dies ist\nTest"));
    Assert.AreEqual("Dies ist<br><br>Test", HtmlStyle.HtmlEncodeShort("Dies ist\r\rTest"));
  }

  [TestMethod]
  public void HtmlEncodeShortNull()
  {
    Assert.IsNull(HtmlStyle.HtmlEncodeShort(null));
    Assert.AreEqual(string.Empty, HtmlStyle.HtmlEncodeShort(string.Empty));
  }

  [TestMethod]
  public void HtmlEncodeShortOk()
  {
    var test = @"Only plain ACSII";
    Assert.AreEqual(test, HtmlStyle.HtmlEncodeShort(test));
    Assert.AreEqual("&quot;", HtmlStyle.HtmlEncodeShort("\""));
    Assert.AreEqual("&amp;", HtmlStyle.HtmlEncodeShort("&"));

    Assert.AreEqual("ä", HtmlStyle.HtmlEncodeShort("ä"));
  }

  [TestMethod]
  public void MakeUniqueInCollectionTest()
  {
#pragma warning disable CS8625 
    var lst = new List<string> { "Value", null, "" };
#pragma warning restore CS8625 
    Assert.AreEqual("Value1", lst.MakeUniqueInCollection("Value"));
    Assert.AreEqual("New", lst.MakeUniqueInCollection("New"));
  }

  [TestMethod]
  [Timeout(200)]
  public void NoSpecials()
  {
    Assert.AreEqual(string.Empty, " ".NoSpecials());
    Assert.AreEqual("aabb", "aabb".NoSpecials());
    Assert.AreEqual("12", "12_&§$".NoSpecials());
    Assert.AreEqual("aabb", " aa_bb  ".NoSpecials());
  }
  [TestMethod]
  [Timeout(200)]
  public void NoSpecials2()
  {
    Assert.AreEqual(string.Empty, " ".AsSpan().NoSpecials().ToString());
    Assert.AreEqual("aabb", "aabb".AsSpan().NoSpecials().ToString());
    Assert.AreEqual("12", "12_&§$".AsSpan().NoSpecials().ToString());
    Assert.AreEqual("aabb", " aa_bb  ".AsSpan().NoSpecials().ToString());
  }
  [TestMethod]
  [Timeout(200)]
  public void SqlName()
  {
    Assert.AreEqual(@"", @"".SqlName());
    Assert.AreEqual(@"ValidationTask", @"ValidationTask".SqlName());
    Assert.AreEqual(@"Validation]]Task", @"Validation]Task".SqlName());
  }

  [TestMethod]
  [Timeout(200)]
  public void SqlName2()
  {
    Assert.AreEqual(@"", @"".AsSpan().SqlName().ToString());
    Assert.AreEqual(@"ValidationTask", @"ValidationTask".AsSpan().SqlName().ToString());
    Assert.AreEqual(@"Validation]]Task", @"Validation]Task".AsSpan().SqlName().ToString());
  }

  [TestMethod]
  [Timeout(200)]
  public void SqlQuote()
  {
    Assert.AreEqual(@"", @"".SqlQuote());
    Assert.AreEqual(@"ValidationTask", @"ValidationTask".SqlQuote());
    Assert.AreEqual(@"Validation''Task", @"Validation'Task".SqlQuote());
  }
  [TestMethod]
  [Timeout(200)]
  public void SqlQuote2()
  {
    Assert.AreEqual(@"", @"".AsSpan().SqlQuote().ToString());
    Assert.AreEqual(@"ValidationTask", @"ValidationTask".AsSpan().SqlQuote().ToString());
    Assert.AreEqual(@"Validation''Task", @"Validation'Task".AsSpan().SqlQuote().ToString());
  }
  [TestMethod]
  [Timeout(200)]
  public void SafeFileName()
  {
    Assert.AreEqual(@"", @"".SafePath());
#pragma warning disable CS8625 
    Assert.AreEqual(@"", FileSystemUtils.SafePath(null));
#pragma warning restore CS8625 

    Assert.AreEqual(@"c:\Users\rnoldner\Documents\Kunden\Sample\Settings.ValidationTask",
      @"c:\Users\rnoldner\Documents\Kunden\Sample\Settings.ValidationTask".SafePath());
#if Windows
      Assert.AreEqual(@"c:\Users\rnoldner\Documents\Kunden\Sample\Settings.ValidationTask",
        @"c:\Users\rnoldner\Documents\Kunden\Sample\Set:tings.Validation*Task".SafePath());
      Assert.AreEqual(@"c:\Users\rnoldner\Documents\Kunden\Sample\Settings.ValidationTask",
        @"c:\Users\rno>ldner\Documents\Kunden\Sample\Set:tings.Validat?ionTask".SafePath());
#endif
  }

  [TestMethod]
  public void ShouldBeTreatedAsNull()
  {
#pragma warning disable 8625
    Assert.IsTrue(StringUtils.ShouldBeTreatedAsNull(null, "NULL"), "Value null");
#pragma warning restore 8625
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
  public void TdNull()
  {
    Assert.IsTrue(string.IsNullOrEmpty(HtmlStyle.AddTd(null, null)));
    Assert.IsTrue(string.IsNullOrEmpty(HtmlStyle.AddTd(null)));
    Assert.AreEqual(string.Empty, HtmlStyle.AddTd(string.Empty));
  }

  [TestMethod]
  public void Tdok()
  {
    Assert.AreEqual("AXB", HtmlStyle.AddTd("A{0}B", "X"));
    Assert.AreEqual("AX.YB", HtmlStyle.AddTd("A{0}.{1}B", "X", "Y"));
    Assert.AreEqual("<td>&gt;<br>&lt;</td>", HtmlStyle.AddTd("<td>{0}<br>{1}</td>", ">", "<"));
  }

  [TestMethod]
  public void TextToHtmlEncode()
  {
    Assert.AreEqual("", HtmlStyle.TextToHtmlEncode(""));
    Assert.AreEqual("This is a test", HtmlStyle.TextToHtmlEncode("This is a test"));
    Assert.AreEqual("This is a test", HtmlStyle.TextToHtmlEncode("This is a\ttest"));
    Assert.AreEqual("This is a<br>test", HtmlStyle.TextToHtmlEncode("This is a\ntest"));
  }

  [TestMethod()]
  public void AssumeIdColumnTest()
  {
    Assert.AreEqual(2, StringUtils.AssumeIdColumn("testId".AsSpan()));
    Assert.AreEqual(3, StringUtils.AssumeIdColumn(" id".AsSpan()));
  }

  [TestMethod()]
  public void ToSecureStringTest()
  {
    var enc = "testId".ToSecureString();
    Assert.IsInstanceOfType("testId".ToSecureString(), typeof(System.Security.SecureString));
    Assert.AreEqual("testId", enc.GetText());

    try
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
      StringUtils.ToSecureString(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
    catch (ArgumentNullException)
    {
    }
    catch (Exception ex)
    {
      Assert.Fail("Wrong Exception Type: " + ex.GetType());
    }
  }

  [TestMethod()]
  public void NoControlCharactersTest()
  {
    Assert.AreEqual("Test", "Test".AsSpan().NoControlCharacters().ToString());
    Assert.AreEqual("Test", "Te\tst".AsSpan().NoControlCharacters().ToString());
    Assert.AreEqual("Test", "T\be\tst".AsSpan().NoControlCharacters().ToString());
  }

  [TestMethod()]
  public void TryGetConstantTest()
  {
    Assert.IsTrue("'15'".AsSpan().TryGetConstant(out var result));
    Assert.AreEqual("15", result.ToString());

    Assert.IsTrue("\"-115.6\"".AsSpan().TryGetConstant(out var result2));
    Assert.AreEqual("-115.6", result2.ToString());
  }
}