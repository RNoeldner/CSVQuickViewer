using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class ProgressEventArgsTimeTests
  {
    [TestMethod()]
    public void ProgressEventArgsTimeTest()
    {
      var evt = new ProgressEventArgsTime("text",123,new TimeSpan(0,0,0,2,20), .3d );
      Assert.AreEqual("text", evt.Text);
      Assert.AreEqual(123, evt.Value);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 2, 20), evt.EstimatedTimeRemaining);
      Assert.AreEqual(123, evt.Value);
      Assert.AreEqual(.3d, evt.Percent);
    }
  }
}