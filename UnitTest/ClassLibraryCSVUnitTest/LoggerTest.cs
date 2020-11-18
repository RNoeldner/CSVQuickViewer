/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CsvTools.Tests
{
  [TestClass]
  public class LoggerTest
  {
    [TestMethod]
    public void UILog()
    {
      //var jsonLogFileName = m_ApplicationDirectory + "\\Log.json";
      //Logger.Configure(jsonLogFileName, Logger.Level.Info, m_ApplicationDirectory + "\\text.log");
      var lastMessage = string.Empty;
      Logger.UILog = (param, level) => { lastMessage = param; };

      Logger.Debug("MyMessage1");
      Logger.Debug("");
      Logger.Debug(null);

      Logger.Information("MyMessage1");
      Logger.Information("");
      Logger.Information(null);

      Logger.Warning("Hello {param1}", "World");
      Logger.Warning("");
      Logger.Warning(null);
      Assert.AreEqual("Hello \"World\"", lastMessage);

      Logger.Warning("Pure {param1}", 1);
      Assert.AreEqual("Pure 1", lastMessage);

      Logger.Error(new Exception("Hello World"), "MyMessage2");
      Logger.Error(new Exception("This is it"));
      Logger.Error("");
      Logger.Error(null, null, null);
      Logger.Error("This {is} it", "was");
    }

    [TestMethod]
    public void AddLog_RemoveLog()
    {
      //var jsonLogFileName = m_ApplicationDirectory + "\\Log.json";
      //Logger.Configure(jsonLogFileName, Logger.Level.Info, m_ApplicationDirectory + "\\text.log");
      var lastMessage = string.Empty;
      Action<string, Logger.Level> logAction = (param, level) =>
      {
        lastMessage = param;
      };
      Logger.AddLog(logAction);

      Logger.Debug("MyMessage1");
      Logger.Debug("");
      Logger.Debug(null);

      Logger.Warning("Hello {param1}", "World");
      Logger.Warning("");
      Logger.Warning(null);
      Assert.AreEqual("Hello \"World\"", lastMessage);

      lastMessage = string.Empty;
      Logger.RemoveLog(logAction);
      Logger.Debug("MyMessage1");
      Assert.AreEqual(string.Empty, lastMessage);
    }

    [TestMethod]
    public void AddLog()
    {
      var lastMessage = string.Empty;
      Logger.AddLog((param, level) => { lastMessage = param; });

      try
      {
        Logger.AddLog(null);
      }
      catch (ArgumentNullException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail("Wrong Exception Type: " + ex.GetType());
      }
    }

    [TestMethod]
    public void RemoveLog()
    {
      Action<string, Logger.Level> logAction = (param, level) => { };

      Logger.AddLog(logAction);
      Logger.RemoveLog(logAction);

      try
      {
        Logger.RemoveLog(null);
      }
      catch (ArgumentNullException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail("Wrong Exception Type: " + ex.GetType());
      }

      try
      {
        Logger.RemoveLog(logAction);
      }
      catch (ArgumentException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail("Wrong Exception Type: " + ex.GetType());
      }
    }
  }
}