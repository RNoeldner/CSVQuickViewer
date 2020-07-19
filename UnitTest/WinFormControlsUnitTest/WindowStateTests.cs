using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class WindowStateTests
  {
    [TestMethod]
    public void WindowStateTest()
    {
      var test = new WindowState();
      Assert.AreEqual(0, test.Left);
      Assert.AreEqual(0, test.Top);
      Assert.AreEqual(0, test.Width);
      Assert.AreEqual(0, test.Height);
    }

    [TestMethod]
    public void WindowStateTest1()
    {
      var rect = new Rectangle(10, 12, 14, 16);
      var test = new WindowState(rect, FormWindowState.Maximized);
      Assert.AreEqual(10, test.Left);
      Assert.AreEqual(12, test.Top);
      Assert.AreEqual(14, test.Width);
      Assert.AreEqual(16, test.Height);
    }
  }
}