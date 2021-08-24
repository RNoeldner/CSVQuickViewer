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

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

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

      Logger.Debug("MyMessage1");
      Logger.Debug("");
      Logger.Debug(null);

      Logger.Information("MyMessage1");
      Logger.Information("");
      Logger.Information(null);

      Logger.Warning("Hello {param1}", "World");
      Logger.Warning("");
      Logger.Warning(null);

      Logger.Warning("Pure {param1}", 1);

      Logger.Error(new Exception("Hello World"), "MyMessage2");
      Logger.Error(new Exception("This is it"));
      Logger.Error("");
      Logger.Error(null, null, null);
      Logger.Error("This {is} it", "was");
    }

    private class TestLogger : ILogger
    {
      public IDisposable BeginScope<TState>(TState state) => default;

      public bool IsEnabled(LogLevel logLevel) => true;

      public readonly List<string> Messages = new List<string>();

      public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
      {
        Messages.Add(formatter(state, exception));
      }
    }

    [TestMethod]
    public void AddLog_RemoveLog()
    {
      //var jsonLogFileName = m_ApplicationDirectory + "\\Log.json";
      //Logger.Configure(jsonLogFileName, Logger.Level.Info, m_ApplicationDirectory + "\\text.log");
      WinAppLogging.Init();

      var logAction = new TestLogger();
      WinAppLogging.AddLog(logAction);

      Logger.Debug("MyMessage1");
      Logger.Debug("");
      Logger.Debug(null);

      Logger.Warning("Hello {param1}", "World");
      Logger.Warning("");
      Logger.Warning(null);
      Assert.AreEqual(2, logAction.Messages.Count);
      Assert.AreEqual("Hello \"World\"", logAction.Messages.Last());

      logAction.Messages.Clear();

      WinAppLogging.RemoveLog(logAction);
      Logger.Debug("MyMessage1");
      Assert.AreEqual(0, logAction.Messages.Count);
    }
  }
}