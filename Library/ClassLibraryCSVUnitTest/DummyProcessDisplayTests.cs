using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class DummyProcessDisplayTests
  {
    [TestMethod]
    public void DummyProcessDisplayTest()
    {
      using (var dpd = new DummyProcessDisplay())
      {
        dpd.SetProcess("Test");
      }
    }

    [TestMethod]
    public void CancelTest()
    {
      using (var dpd = new DummyProcessDisplay())
      {
        dpd.Cancel();
        Assert.IsTrue(dpd.CancellationToken.IsCancellationRequested);
      }
    }

    [TestMethod]
    public void SetMaximum()
    {
      using (var dpd = new DummyProcessDisplay())
      {
        dpd.Maximum = 666;
        Assert.AreEqual(666, dpd.Maximum);

        dpd.Maximum = -1;
        Assert.AreEqual(-1, dpd.Maximum);
      }
    }

    [TestMethod]
    public void SetProcessTest()
    {
      using (var dpd = new DummyProcessDisplay())
      {
        dpd.SetProcess("Test");
      }
    }

    [TestMethod]
    public void SetProcessTest1()
    {
      using (var dpd = new DummyProcessDisplay())
      {
        dpd.Maximum = 5;
        dpd.SetProcess("Test", 100);
      }
    }

    [TestMethod]
    public void SetProcessTest2()
    {
      using (var dpd = new DummyProcessDisplay())
      {
        dpd.Maximum = 5;
        dpd.SetProcess(null, new ProgressEventArgs("Hallo", 2));
      }
    }
  }
}