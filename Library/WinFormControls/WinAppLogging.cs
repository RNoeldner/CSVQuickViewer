using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace CsvTools
{
  public static class WinAppLogging
  {
    private static readonly UserInterfaceSink m_UserInterfaceSink = new UserInterfaceSink(CultureInfo.CurrentCulture);

    /// <summary>
    /// Dummy Method to make sure the Constructor is called
    /// </summary>
    public static void Init()
    {
      var loggerConfiguration = new LoggerConfiguration()
      //.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
      .Enrich.FromLogContext()
      .Filter.ByExcluding(logEvent => logEvent.Exception != null
                                      && (logEvent.Exception.GetType() == typeof(OperationCanceledException) ||
                                          logEvent.Exception.GetType() == typeof(ObjectDisposedException)))
      .WriteTo.Sink(m_UserInterfaceSink);

      try
      {
        // File Logger
        var entryName = Assembly.GetEntryAssembly()?.GetName().Name ?? Assembly.GetExecutingAssembly().GetName().Name;

        var folder = Environment.ExpandEnvironmentVariables($"%LocalAppData%\\{entryName}\\");
        var addTextLog = FileSystemUtils.DirectoryExists(folder);
        if (!addTextLog)
        {
          try
          {
            FileSystemUtils.CreateDirectory(folder);
            addTextLog = true;
          }
          catch
          {
            // ignored
          }
        }

        if (addTextLog)
        {
          loggerConfiguration = loggerConfiguration
            // Exceptions
            .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(le => le.Exception != null).WriteTo.File(
              folder + "ExceptionLog.txt", rollingInterval: RollingInterval.Month, retainedFileCountLimit: 3, encoding: Encoding.UTF8, 
              outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}\t{Level}\t\"{Exception:l}\"{NewLine}"), LogEventLevel.Error)
            //CSV
            .WriteTo.File(folder + "ApplicationLog.txt", rollingInterval: RollingInterval.Day, encoding: Encoding.UTF8, 
              outputTemplate: "{Timestamp:HH:mm:ss}\t{Level:w3}\t{Message:l}{NewLine}")
            // Json
            .WriteTo.File(formatter: new JsonFormatter(renderMessage: true), path: folder + "ApplicationLog.json",
              rollingInterval: RollingInterval.Day);
        }
      }
      catch
      {
        // ignore
      }

      // Set the general Logger
      Logger.LoggerInstance = new SerilogLoggerProvider(loggerConfiguration.CreateLogger()).CreateLogger(nameof(CsvTools));
    }

    public static void RemoveLog(Microsoft.Extensions.Logging.ILogger value)
        => m_UserInterfaceSink.AdditionalLoggers.Remove(value ?? throw new ArgumentNullException(nameof(value)));
    public static void AddLog(Microsoft.Extensions.Logging.ILogger value)
      => m_UserInterfaceSink.AdditionalLoggers.Add(value ?? throw new ArgumentNullException(nameof(value)));

    private class UserInterfaceSink : ILogEventSink
    {
      public readonly List<Microsoft.Extensions.Logging.ILogger> AdditionalLoggers = new List<Microsoft.Extensions.Logging.ILogger>();

      // By design: Only one Action to be called you can not have two or more destinations
      private readonly IFormatProvider m_FormatProvider;

      public UserInterfaceSink(IFormatProvider formatProvider)
      {
        m_FormatProvider = formatProvider;
      }

      public void Emit(LogEvent logEvent)
      {
        if (AdditionalLoggers.Count <= 0) return;
        LogLevel level;
        switch (logEvent.Level)
        {
          case LogEventLevel.Verbose:
          case LogEventLevel.Debug:
            level = LogLevel.Debug;
            break;

          case LogEventLevel.Information:
            level = LogLevel.Information;
            break;

          case LogEventLevel.Warning:
            level = LogLevel.Warning;
            break;

          default:
            level =  LogLevel.Error;
            break;
        }

        foreach (var logger in AdditionalLoggers)
          logger.Log(level, logEvent.RenderMessage(m_FormatProvider), level);
      }
    }
  }
}
