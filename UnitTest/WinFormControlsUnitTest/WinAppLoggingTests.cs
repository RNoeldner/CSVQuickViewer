/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael N�ldner
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */
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
