/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
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
using System.Text;

namespace CsvTools.Tests
{
  [TestClass]
  public sealed class HtmlStyleTests
  {
    [TestMethod]
    public void AddHtmlCellTest()
    {
      var sb = new StringBuilder();

      HtmlStyle.AddHtmlCell(sb, "<{0}>", "1", string.Empty, false);
      Assert.AreEqual("<1>", sb.ToString());

      sb = new StringBuilder();
      HtmlStyle.AddHtmlCell(sb, "<{0}>", "1", "Error", false);
      Assert.AreEqual("<1>", sb.ToString());
    }

    [TestMethod]
    public void AddHtmlCellTestError()
    {
      var sb = new StringBuilder();
      HtmlStyle.AddHtmlCell(sb, "<{0}>", "Test", "Some Error", true);
      Assert.IsTrue(sb.ToString().StartsWith("<Test"));
      Assert.IsTrue(sb.ToString().Contains("Some Error"));
      Assert.IsTrue(sb.ToString().EndsWith(">"));
    }

    [TestMethod]
    public void AddHtmlCellTestErrorWarnings()
    {
      var errWar = "Some Error".AddMessage("Issue".AddWarningId());
      var sb = new StringBuilder();
      HtmlStyle.AddHtmlCell(sb, "<{0}>", "Test", errWar, true);
      Assert.IsTrue(sb.ToString().StartsWith("<Test"));
      Assert.IsTrue(sb.ToString().EndsWith(">"));
      Assert.IsTrue(sb.ToString().Contains("Issue"));
      Assert.IsTrue(sb.ToString().Contains("Some Error"));
    }

    [TestMethod]
    public void AddHtmlCellTestWarnings()
    {
      var sb = new StringBuilder();
      HtmlStyle.AddHtmlCell(sb, "<{0}>", "Test", "Issue".AddWarningId(), true);
      Assert.IsTrue(sb.ToString().StartsWith("<Test"));
      Assert.IsTrue(sb.ToString().Contains("Issue"));
      Assert.IsTrue(sb.ToString().EndsWith(">"));
    }

    [TestMethod]
    public void AddTdTest()
    {
      Assert.IsTrue(string.IsNullOrEmpty(HtmlStyle.AddTd(null)));
      Assert.IsTrue(string.IsNullOrEmpty(HtmlStyle.AddTd("")));
      Assert.AreEqual("<1>", HtmlStyle.AddTd("<{0}>", "1"));
    }

    [TestMethod]
    public void ConvertToHtmlFragment()
    {
      var style = HtmlStyle.Default;
      Assert.IsNotNull(style.ConvertToHtmlFragment("Hello"));
    }

    [TestMethod]
    public void ConvertToHtmlFragmentTest()
    {
      var style = new HtmlStyle("Dummy");
      Assert.IsNotNull(style.ConvertToHtmlFragment("Test"));
      Assert.IsTrue(style.ConvertToHtmlFragment("Test").StartsWith("Version:1.0", StringComparison.Ordinal));
    }

    [TestMethod]
    public void HtmlEncodeShortTest()
    {
      Assert.IsNull(HtmlStyle.HtmlEncodeShort(null));
      Assert.AreEqual("", HtmlStyle.HtmlEncodeShort(""));
      Assert.AreEqual("&lt;&gt;", HtmlStyle.HtmlEncodeShort("<>"));
      Assert.AreEqual("HTML", HtmlStyle.HtmlEncodeShort("HTML"));
      Assert.AreEqual("K&amp;N", HtmlStyle.HtmlEncodeShort("K&N"));
      Assert.AreEqual("Line1<br>Line2", HtmlStyle.HtmlEncodeShort("Line1\r\nLine2"));
    }

    [TestMethod]
    public void HtmlEncodeTest()
    {
      Assert.AreEqual("", HtmlStyle.HtmlEncode(""));
      Assert.AreEqual("HTML", HtmlStyle.HtmlEncode("HTML"));
      Assert.AreEqual("K&amp;N", HtmlStyle.HtmlEncode("K&N"));
      Assert.AreEqual("he sated &quot;Hello&quot;", HtmlStyle.HtmlEncode("he sated \"Hello\""));

      /*ö latin small letter o with diaeresis
       * &ouml; or &#246;
       */
      Assert.IsTrue(
        @"Raphael N&ouml;ldner" == HtmlStyle.HtmlEncode("Raphael Nöldner")
        || @"Raphael N&#246;ldner" == HtmlStyle.HtmlEncode("Raphael Nöldner"));
    }

    [TestMethod]
    public void JsonElementNameTest()
    {
      Assert.AreEqual("", HtmlStyle.JsonElementName(""));
      Assert.AreEqual("HTmL", HtmlStyle.JsonElementName("HTmL"));
      Assert.AreEqual("RaphaelNöldner", HtmlStyle.JsonElementName("Raphael Nöldner"));
      Assert.AreEqual("_12", HtmlStyle.XmlElementName("12"));
    }

    [TestMethod]
    public void TextToHtmlEncodeTest()
    {
      Assert.AreEqual("", HtmlStyle.TextToHtmlEncode(""));
      Assert.AreEqual("Raphael Nöldner", HtmlStyle.TextToHtmlEncode("Raphael  Nöldner"));
      Assert.AreEqual("Raphael Nöldner", HtmlStyle.TextToHtmlEncode("Raphael\t Nöldner"));
      Assert.AreEqual("Line1<br>Line2", HtmlStyle.TextToHtmlEncode("Line1\r\nLine2"));
    }

    [TestMethod]
    public void TextToHtmlFormatterTest()
    {
      var called = false;
      var fmter = new TextToHtmlFormatter();
      Assert.AreEqual("Hello", fmter.FormatInputText("Hello", _ => called = true));
      Assert.IsFalse(called);
      Assert.AreEqual("Hello World", fmter.FormatInputText("Hello\tWorld", _ => called = true));
      Assert.IsTrue(called);
    }

    [TestMethod]
    public void TextToHtmlFormatterWriteTest()
    {
      var fmter = new TextToHtmlFormatter();
      Assert.AreEqual("Hello", fmter.Write("Hello", null, null));
      Assert.AreEqual("<br>", fmter.Write("\r", null, null));
    }

    [TestMethod]
    public void TextToHtmlFullFormatterTest()
    {
      var called = false;
      var fmter = new TextToHtmlFullFormatter();
      Assert.AreEqual("Hello", fmter.FormatInputText("Hello", _ => called = true));
      Assert.IsFalse(called);
      Assert.AreEqual("&lt;&gt;", fmter.FormatInputText("<>", _ => called = true));
      Assert.IsTrue(called);
    }

    [TestMethod]
    public void TextUnescapeFormatterTest()
    {
      var called = false;
      var fmter = new TextUnescapeFormatter();
      Assert.AreEqual("Hello", fmter.FormatInputText("Hello", _ => called = true));
      Assert.IsFalse(called);
      Assert.AreEqual("\n\x0020\r", fmter.FormatInputText(@"\n\x0020\r", _ => called = true));
      Assert.IsTrue(called);
    }

    [TestMethod]
    public void XMLElementNameTest()
    {
      /*
          Element names are case-sensitive
          Element names must start with a letter or underscore
          Element names cannot start with the letters xml (or XML, or Xml, etc)
          Element names can contain letters, digits, hyphens, underscores, and periods
          Element names cannot contain spaces
      */
      Assert.AreEqual("", HtmlStyle.XmlElementName(""));
      Assert.AreEqual("HTmL", HtmlStyle.XmlElementName("HTmL"));
      Assert.AreEqual("HTML", HtmlStyle.XmlElementName("HT;ML"));
      Assert.AreEqual("RaphaelNoldner", HtmlStyle.XmlElementName("Raphael Nöldner"));
      Assert.AreEqual("_12", HtmlStyle.XmlElementName("12"));
    }
  }
}