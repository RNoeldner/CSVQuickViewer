using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass()]
  public class FontConfigTests
  {
    [TestMethod()]
    public void FontConfigTest()
    {
      var fc = new FontConfig("Times", 10.0F);
      Assert.AreEqual("Times", fc.Font);
      Assert.AreEqual(10.0F, fc.FontSize);
    }
  }
}