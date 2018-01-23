using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass]
  public class TimeToCompletionTests
  {
    [TestMethod]
    public void TimeToCompletionTest()
    {
      var test = new TimeToCompletion();
      Assert.IsNotNull(test);
      test.TargetValue = 20;
      Assert.AreEqual(20, test.TargetValue);
      Assert.AreEqual(string.Empty, test.EstimatedTimeRemainingDisplaySeperator);
      Assert.AreEqual(string.Empty, test.EstimatedTimeRemainingDisplay);

      test.Value = 1;
      Assert.AreEqual(1, test.Value);
      Assert.AreEqual(5, test.Percent);
      Thread.Sleep(100);
      test.Value = 2;
      Assert.AreEqual(2, test.Value);
      Assert.AreEqual(10, test.Percent);
      Thread.Sleep(100);
      test.Value = 3;
      Assert.AreEqual("15%", test.PercentDisplay);
      Thread.Sleep(100);
      test.Value = 4;
      Thread.Sleep(100);
      test.Value = 5;
      Assert.AreEqual(5, test.Value);
      Assert.AreEqual(25, test.Percent);

      Assert.IsTrue(test.EstimatedTimeRemaining.TotalSeconds > 0.0);
      Assert.IsTrue(test.EstimatedTimeRemaining.TotalSeconds < 5.0);
      Assert.AreNotEqual(string.Empty, test.EstimatedTimeRemainingDisplaySeperator);
      Assert.AreNotEqual(string.Empty, test.EstimatedTimeRemainingDisplay);
    }
  }
}