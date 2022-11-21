using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass]
  public class WindowStateTests
  {
    [TestMethod]
    public void WindowStateTest1()
    {
      var test = new WindowState(10, 12, 14, 16, FormWindowState.Maximized);
      Assert.AreEqual(10, test.Left);
      Assert.AreEqual(12, test.Top);
      Assert.AreEqual(14, test.Width);
      Assert.AreEqual(16, test.Height);
    }
  }
}