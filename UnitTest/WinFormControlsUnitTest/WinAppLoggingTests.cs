using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CsvTools.Tests
{
  [TestClass()]
  public class WinAppLoggingTests
  {
    [TestMethod()]
    public void InitTest()
    {
      try
      {
        WinAppLogging.Init();
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        
      }
      
    }

    [TestMethod()]
    public void RemoveLogTest()
    {
      InitTest();
      var myLogger = new UnitTestLogger(null);
      WinAppLogging.AddLog(myLogger);
      Logger.Debug("Test");
      Assert.AreEqual("Test", myLogger.LastMessage);
      WinAppLogging.RemoveLog(myLogger);
      myLogger.LogDebug("");
      Logger.Debug("Test2");
      Assert.AreEqual("", myLogger.LastMessage);
    }

    [TestMethod()]
    public void AddLogTest()
    {
      InitTest();
      var myLogger = new UnitTestLogger(null);
      WinAppLogging.AddLog(myLogger);
      Logger.Error("Test");
      Assert.AreEqual("Test", myLogger.LastMessage);
    }
  }
}