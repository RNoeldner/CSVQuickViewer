using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass()]
  public class PunctuationTests
  {
    [TestMethod]
    public void DescriptionText()
    {
      Assert.AreEqual(string.Empty, new Punctuation("").Text);
      Assert.AreEqual("Horizontal Tab", new Punctuation("\t").Description);
      Assert.AreEqual("Comma: ,", new Punctuation(",").Description);
      Assert.AreEqual("Pipe: |", new Punctuation("|").Description);
      Assert.AreEqual("Semicolon: ;", new Punctuation(";").Description);
      Assert.AreEqual("Colon: :", new Punctuation(":").Description);
      Assert.AreEqual("Quotation marks: \"", new Punctuation("\"").Description);
      Assert.AreEqual("Apostrophe: '", new Punctuation("'").Description);
      Assert.AreEqual("Space", new Punctuation(" ").Description);
      Assert.AreEqual("Backslash: \\", new Punctuation("\\").Description);
      Assert.AreEqual("Slash: /", new Punctuation("/").Description);
      Assert.AreEqual("Unit Separator: Char 31", new Punctuation("US").Description);
      Assert.AreEqual("Unit Separator: Char 31", new Punctuation("Unit Separator").Description);

      Assert.AreEqual("Group Separator: Char 29", new Punctuation("GS").Description);
      Assert.AreEqual("Record Separator: Char 30", new Punctuation("RS").Description);
      Assert.AreEqual("File Separator: Char 28", new Punctuation("FS").Description);
    }

    [TestMethod()]
    public void GetDescriptionTest()
    {
      Assert.AreEqual("Horizontal Tab", new Punctuation('\t').Description);
      Assert.AreEqual("Horizontal Tab", new Punctuation('\t').Text);
      Assert.AreEqual("Space", new Punctuation(' ').Description);
      Assert.IsTrue(new Punctuation("\\").Description.Contains("Backslash"));
      Assert.IsTrue(new Punctuation("'").Description.Contains("\'"));
    }

  }
}