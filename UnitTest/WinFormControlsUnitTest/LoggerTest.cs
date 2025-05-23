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
#nullable enable
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CsvTools.Tests
{
  [TestClass]
  public class LoggerTest
  {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private static TestContext m_TestContext;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [ClassInitialize]
    public static void SetupTests(TestContext testContext)
    {
      m_TestContext = testContext;
    }

    [TestMethod, Timeout(1000)]
    public void UILog()
    {
      var testLogger = new UnitTestLogger(m_TestContext);
      Logger.LoggerInstance = testLogger;
      
      Logger.Information("MyMessage1");
      Assert.AreEqual("MyMessage1", testLogger.LastMessage);
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
      Assert.AreEqual("This is it", testLogger.LastMessage);
      Logger.Error("");
      Logger.Error("This {is} it", "was");
      Assert.AreEqual("This was it", testLogger.LastMessage);
    }

    private class TestLogger : ILogger
    {
#pragma warning disable CS8603 
#pragma warning disable CS8633
      public IDisposable BeginScope<TState>(TState state) => default;
#pragma warning restore CS8633
#pragma warning restore CS8603 

      public bool IsEnabled(LogLevel logLevel) => true;

      public readonly List<string> Messages = new List<string>();

      public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
      {
        Messages.Add(formatter(state, exception));
      }
    }

    [TestMethod, Timeout(1000)]
    public void UserInterfaceSink()
    {
      var test = new WinAppLogging.UserInterfaceSink(CultureInfo.CurrentCulture);
      var logAction = new TestLogger();
      test.AdditionalLoggers.Add(logAction);
      test.Emit(new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, MessageTemplate.Empty, Array.Empty<LogEventProperty>()));

      Assert.AreEqual(1, logAction.Messages.Count);
    }


    [TestMethod, Timeout(1000)]
    public void AddLog_RemoveLog()
    {
      bool hadIssues = false;
      try
      {
        WinAppLogging.Init();
      }
      // Sometimes get the error System.IO.FileLoadException for Microsoft.Extensions.Logging.Abstractions...
      catch (System.IO.FileLoadException)
      {
        hadIssues = true;
      }
      catch (System.IO.FileNotFoundException)
      {
        hadIssues = true;
      }

      var logAction = new TestLogger();
      WinAppLogging.AddLog(logAction);

      Logger.Debug("");
      Logger.Debug(null);

      Logger.Error("MyMessage1");
      Logger.Information("");

      Logger.Warning("Hello {param1}", "World");
      Logger.Warning("");
      Logger.Warning(null);
      if (!hadIssues)
      {
        Assert.AreEqual(2, logAction.Messages.Count);
        Assert.AreEqual("Hello \"World\"", logAction.Messages.Last());
      }
      logAction.Messages.Clear();

      WinAppLogging.RemoveLog(logAction);
      Logger.Debug("MyMessage1");
      Assert.AreEqual(0, logAction.Messages.Count);
      if (hadIssues)
        Assert.Inconclusive("Issue with WinAppLogging.Init() check Microsoft.Extensions.Logging");
    }
  }
}