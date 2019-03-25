using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class DummyProcessDisplayTests
  {
    [TestMethod]
    public void DummyProcessDisplayTest()
    {
      using (var processDisplay = new DummyProcessDisplay())
      {
        processDisplay.SetProcess("Test");
      }
    }

    [TestMethod]
    public void CancelTest()
    {
      using (var processDisplay = new DummyProcessDisplay())
      {
        processDisplay.Cancel();
        Assert.IsTrue(processDisplay.CancellationToken.IsCancellationRequested);
      }
    }

    [TestMethod]
    public void SetMaximum()
    {
      using (var processDisplay = new DummyProcessDisplay())
      {
        processDisplay.Maximum = 666;
        Assert.AreEqual(666, processDisplay.Maximum);

        processDisplay.Maximum = -1;
        Assert.AreEqual(-1, processDisplay.Maximum);
      }
    }

    [TestMethod]
    public void SetProcessTest()
    {
      using (var processDisplay = new DummyProcessDisplay())
      {
        processDisplay.SetProcess("Test");
      }
    }

    [TestMethod]
    public void SetProcessTest1()
    {
      using (var processDisplay = new DummyProcessDisplay())
      {
        processDisplay.Maximum = 5;
        processDisplay.SetProcess("Test", 100);
      }
    }

    [TestMethod]
    public void SetProcessTest2()
    {
      using (var processDisplay = new DummyProcessDisplay())
      {
        processDisplay.Maximum = 5;
        processDisplay.SetProcess(null, new ProgressEventArgs("Hallo", 2));
      }
    }
  }
}