using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class DisplayItemTests
  {
    [TestMethod]
    public void DisplayItemTest()
    {
      var i = new DisplayItem<int>(1, "Test");

      Assert.AreEqual(1, i.ID);
      Assert.AreEqual("Test", i.Display);
    }
  }
}