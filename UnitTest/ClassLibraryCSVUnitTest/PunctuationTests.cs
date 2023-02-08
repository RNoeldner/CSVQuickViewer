using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass()]
  public class PunctuationTests
  {
    [TestMethod]
    public void DescriptionText()
    {
      Assert.AreEqual(string.Empty, "".FromText().Text());
      Assert.AreEqual("Horizontal Tab", "\t".FromText().Description());
      Assert.AreEqual("Comma ,", ",".FromText().Description());
      Assert.AreEqual("Pipe |", "|".FromText().Description());
      Assert.AreEqual("Semicolon ;", ";".FromText().Description());
      Assert.AreEqual("Colon :", ":".FromText().Description());
      Assert.AreEqual("Quotation marks \"", "\"".FromText().Description());
      Assert.AreEqual("Apostrophe '", "'".FromText().Description());
      Assert.AreEqual("Space", " ".FromText().Description());
      Assert.AreEqual("Backslash \\", "\\".FromText().Description());
      Assert.AreEqual("Slash /", "/".FromText().Description());

      Assert.IsTrue( "US".FromText().Description().StartsWith("Unit"));
      Assert.IsTrue( "Unit Separator".FromText().Description().StartsWith("Unit"));
      Assert.IsTrue("GS".FromText().Description().StartsWith("Group"));
      Assert.IsTrue("RS".FromText().Description().StartsWith("Record"));
      Assert.IsTrue("FS".FromText().Description().StartsWith("File"));
    }

    [TestMethod()]
    public void GetDescriptionTest()
    {
      Assert.AreEqual("Horizontal Tab", '\t'.Description());
      Assert.AreEqual("Tab", '\t'.Text());
      Assert.AreEqual("Space", ' '.Description());
      Assert.IsTrue("\\".FromText().Description().Contains("Backslash"));
      Assert.IsTrue("'".FromText().Description().Contains("\'"));
    }

  }
}