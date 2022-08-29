using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass()]
  public class Markdown2HtmlFormatterTests
  {
    [TestMethod()]
    public void MarkupToHtmlFormatterTest()
    {
      var instance = new Markdown2HtmlFormatter();
      Assert.AreEqual("<p>This is <strong>bold</strong>. This is also <strong>bold</strong>.</p>", instance.FormatInputText("This is **bold**. This is also __bold__.", null));
      Assert.AreEqual("<p>This is <em>italic</em>. This is also <em>italic</em>.</p>", instance.FormatInputText("This is *italic*. This is also _italic_.", null));
      Assert.AreEqual("<h1>Header 1</h1>\n\n<h1>Header 1</h1>", instance.FormatInputText("#Header 1\nHeader 1\n========", null));
    }

    [TestMethod()]
    public void MarkupToHtmlFormatterTestWrite()
    {
      var instance = new Markdown2HtmlFormatter();
      Assert.AreEqual("Test", instance.Write("Test",null,null ));
    }

    [TestMethod()]
    public void MarkupToHtmlFormatterTestRaiseWarning()
    {
      var instance = new Markdown2HtmlFormatter();
      Assert.IsTrue(instance.RaiseWarning);
      instance.RaiseWarning = false;
      Assert.IsFalse(instance.RaiseWarning);
    }
  }
}