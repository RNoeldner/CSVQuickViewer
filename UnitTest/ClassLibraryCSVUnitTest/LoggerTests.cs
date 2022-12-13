using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CsvTools.Tests
{
  [TestClass()]
  public class LoggerTests
  {
    [TestMethod()]
    public void DebugTest()
    {
      Logger.Debug("Message1");
      Assert.AreEqual("Message1", UnitTestStatic.LastLogMessage);
      Logger.Debug("");
    }

    [TestMethod()]
    public void ErrorTest()
    {
      Logger.Error("Message2");
      Assert.AreEqual("Message2", UnitTestStatic.LastLogMessage);
      Logger.Error("");
    }

    [TestMethod()]
    public void ErrorTest1()
    {
      Logger.Error(new Exception("Test"), "Message3" );
      Assert.AreEqual("Message3", UnitTestStatic.LastLogMessage);
    }

    [TestMethod()]
    public void InformationTest()
    {
      Logger.Information("Message4");
      Assert.AreEqual("Message4", UnitTestStatic.LastLogMessage);
      Logger.Information("");
    }

    [TestMethod()]
    public void InformationTest1()
    {
      Logger.Information(new Exception("Test2"), "Message5" );
      Assert.AreEqual("Message5", UnitTestStatic.LastLogMessage);
    }

    [TestMethod()]
    public void WarningTest()
    {
      Logger.Warning("Message6");
      Assert.AreEqual("Message6", UnitTestStatic.LastLogMessage);
    }

    [TestMethod()]
    public void WarningTest1()
    {
      Logger.Warning(new Exception("Test7"), "Message7" );
      Assert.AreEqual("Message7", UnitTestStatic.LastLogMessage);

    }
  }
}