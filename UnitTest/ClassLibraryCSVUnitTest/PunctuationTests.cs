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
      Assert.AreEqual("Comma ,", new Punctuation(",").Description);
      Assert.AreEqual("Pipe |", new Punctuation("|").Description);
      Assert.AreEqual("Semicolon ;", new Punctuation(";").Description);
      Assert.AreEqual("Colon :", new Punctuation(":").Description);
      Assert.AreEqual("Quotation marks \"", new Punctuation("\"").Description);
      Assert.AreEqual("Apostrophe '", new Punctuation("'").Description);
      Assert.AreEqual("Space", new Punctuation(" ").Description);
      Assert.AreEqual("Backslash \\", new Punctuation("\\").Description);
      Assert.AreEqual("Slash /", new Punctuation("/").Description);

      Assert.IsTrue( new Punctuation("US").Description.StartsWith("Unit"));
      Assert.IsTrue( new Punctuation("Unit Separator").Description.StartsWith("Unit"));
      Assert.IsTrue(new Punctuation("GS").Description.StartsWith("Group"));
      Assert.IsTrue(new Punctuation("RS").Description.StartsWith("Record"));
      Assert.IsTrue(new Punctuation("FS").Description.StartsWith("File"));
    }

    [TestMethod()]
    public void GetDescriptionTest()
    {
      Assert.AreEqual("Horizontal Tab", new Punctuation('\t').Description);
      Assert.AreEqual("Tab", new Punctuation('\t').Text);
      Assert.AreEqual("Space", new Punctuation(' ').Description);
      Assert.IsTrue(new Punctuation("\\").Description.Contains("Backslash"));
      Assert.IsTrue(new Punctuation("'").Description.Contains("\'"));
    }

  }
}