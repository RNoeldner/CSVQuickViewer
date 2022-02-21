using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass()]
  public class MarkupToHtmlFormatterTests
  {
    [TestMethod()]
    public void MarkupToHtmlFormatterTest()
    {
      var instance = new MarkupToHtmlFormatter();      
      Assert.AreEqual("<p>This is <strong>bold</strong>. This is also <strong>bold</strong>.</p>", instance.FormatText("This is **bold**. This is also __bold__.", null));
      Assert.AreEqual("<p>This is <em>italic</em>. This is also <em>italic</em>.</p>", instance.FormatText("This is *italic*. This is also _italic_.", null));
      Assert.AreEqual("<h1>Header 1</h1>\n\n<h1>Header 1</h1>", instance.FormatText("#Header 1\nHeader 1\n========", null));


    }
  }
}