using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ProgressInfoTest
  {

    [TestMethod]
    public void CtorLong()
    {
      var test = new ProgressInfo("TestText", 10l);
      Assert.IsNotNull(test);
      Assert.AreEqual("TestText", test.Text);
      Assert.AreEqual(10l, test.Value);
    }

    [TestMethod]
    public void CtorFloat()
    {
      var test = new ProgressInfo("TestText", 2.8f);
      Assert.IsNotNull(test);
      Assert.AreEqual("TestText", test.Text);
      Assert.AreEqual(3l, test.Value);
    }

    [TestMethod]
    public void CtorText()
    {
      var test = new ProgressInfo("TestText1");
      Assert.IsNotNull(test);
      Assert.AreEqual("TestText1", test.Text);
      Assert.AreEqual(-1l, test.Value);
    }
  }
}