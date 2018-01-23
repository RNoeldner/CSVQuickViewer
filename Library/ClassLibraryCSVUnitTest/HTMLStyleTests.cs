using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace CsvTools.Tests
{
  [TestClass]
  public class HTMLStyleTests
  {
    [TestMethod]
    public void JSONEncodeTest()
    {
      Assert.IsNull(HTMLStyle.JsonEncode(null));
      Assert.AreEqual("\"\"", HTMLStyle.JsonEncode(""));
      Assert.AreEqual("\"JSON\"", HTMLStyle.JsonEncode("JSON"));
    }

    [TestMethod]
    public void HtmlEncodeShortTest()
    {
      Assert.IsNull(HTMLStyle.HtmlEncodeShort(null));
      Assert.AreEqual("", HTMLStyle.HtmlEncodeShort(""));
      Assert.AreEqual("HTML", HTMLStyle.HtmlEncodeShort("HTML"));
      Assert.AreEqual("K&amp;N", HTMLStyle.HtmlEncodeShort("K&N"));
      Assert.AreEqual("Line1<br>Line2", HTMLStyle.HtmlEncodeShort("Line1\r\nLine2"));
    }

    [TestMethod]
    public void HtmlEncodeTest()
    {
      Assert.IsNull(HTMLStyle.HtmlEncode(null));
      Assert.AreEqual("", HTMLStyle.HtmlEncode(""));
      Assert.AreEqual("HTML", HTMLStyle.HtmlEncode("HTML"));
      Assert.AreEqual("K&amp;N", HTMLStyle.HtmlEncode("K&N"));
      Assert.AreEqual("he sated &quot;Hello&quot;", HTMLStyle.HtmlEncode("he sated \"Hello\""));

      /*ö latin small letter o with diaeresis
       * &ouml; or &#246;
       */
      Assert.IsTrue(@"Raphael N&ouml;ldner" == HTMLStyle.HtmlEncode("Raphael Nöldner") ||
                    @"Raphael N&#246;ldner" == HTMLStyle.HtmlEncode("Raphael Nöldner"));
    }

    [TestMethod]
    public void JSONElementNameTest()
    {
      Assert.AreEqual("", HTMLStyle.JsonElementName(""));
      Assert.AreEqual("HTmL", HTMLStyle.JsonElementName("HTmL"));
      Assert.AreEqual("RaphaelNöldner", HTMLStyle.JsonElementName("Raphael Nöldner"));
      Assert.AreEqual("_12", HTMLStyle.XmlElementName("12"));
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
      Assert.AreEqual("", HTMLStyle.XmlElementName(""));
      Assert.AreEqual("HTmL", HTMLStyle.XmlElementName("HTmL"));
      Assert.AreEqual("HTML", HTMLStyle.XmlElementName("HT;ML"));
      Assert.AreEqual("RaphaelNoldner", HTMLStyle.XmlElementName("Raphael Nöldner"));
      Assert.AreEqual("_12", HTMLStyle.XmlElementName("12"));
    }

    [TestMethod]
    public void TextToHtmlEncodeTest()
    {
      Assert.IsNull(HTMLStyle.TextToHtmlEncode(null));
      Assert.AreEqual("", HTMLStyle.TextToHtmlEncode(""));
      Assert.AreEqual("Raphael Nöldner", HTMLStyle.TextToHtmlEncode("Raphael  Nöldner"));
      Assert.AreEqual("Raphael Nöldner", HTMLStyle.TextToHtmlEncode("Raphael\t Nöldner"));
      Assert.AreEqual("Line1<br>Line2", HTMLStyle.TextToHtmlEncode("Line1\r\nLine2"));
    }

    [TestMethod]
    public void AddTDTest()
    {
      Assert.IsNull(HTMLStyle.AddTd(null));
      Assert.AreEqual("", HTMLStyle.AddTd(""));
      Assert.AreEqual("<1>", HTMLStyle.AddTd("<{0}>", "1"));
    }

    [TestMethod]
    public void AddHTMLCellTest()
    {
      HTMLStyle.AddHtmlCell(null, null, null, null, true);
      var sb = new StringBuilder();

      HTMLStyle.AddHtmlCell(sb, "<{0}>", "1", null, false);
      Assert.AreEqual("<1>", sb.ToString());

      sb = new StringBuilder();
      HTMLStyle.AddHtmlCell(sb, "<{0}>", "1", "Error", false);
      Assert.AreEqual("<1>", sb.ToString());
    }

    [TestMethod]
    public void ConvertToHtmlFragmentTest()
    {
      var style = new HTMLStyle();
      Assert.IsNotNull(style.ConvertToHtmlFragment("Test"));
      Assert.IsTrue(style.ConvertToHtmlFragment("Test").StartsWith("Version:1.0", StringComparison.Ordinal));
    }
  }
}