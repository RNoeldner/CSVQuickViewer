using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass]
  public class IntervalActionTests
  {
    [TestMethod]
    public void InvokeTest()
    {
      var intervalAction = new IntervalAction();
      var called = 0;
      // First Call ok
      intervalAction.Invoke(() => called++);
      Assert.AreEqual(1, called);
      // First Call This time its not called because time was not sufficient
      intervalAction.Invoke(() => called++);
      Assert.AreEqual(1, called);
    }

    [TestMethod]
    public void InvokeTest2()
    {
      var intervalAction = new IntervalAction(.01);
      var called = 0;
      // First Call ok
      intervalAction.Invoke(() => called++);
      Assert.AreEqual(1, called);
      Thread.Sleep(110);
      // First Call This time its not called because time was not sufficient
      intervalAction.Invoke(() => called++);
      Assert.AreEqual(2, called);
    }
  }
}