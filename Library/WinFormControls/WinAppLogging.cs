#nullable enable

using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace CsvTools
{
  public static class WinAppLogging
  {
    private static readonly UserInterfaceSink m_UserInterfaceSink = new UserInterfaceSink(CultureInfo.CurrentCulture);

    /// <summary>
    ///   Dummy Method to make sure the Constructor is called
    /// </summary>
    public static void Init()
    {
      try
      {
        var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name ??
                           Assembly.GetExecutingAssembly().GetName().Name;

        var folder = Environment.ExpandEnvironmentVariables($"%LocalAppData%\\{assemblyName}\\");
        if (!FileSystemUtils.DirectoryExists(folder))
        {
          try
          {
            FileSystemUtils.CreateDirectory(folder);
          }
          catch
          {
            // ignored
          }
        }

        var loggerConfiguration = new LoggerConfiguration()
          //.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
          .MinimumLevel.Information()
          .Enrich.FromLogContext()
          .Filter.ByExcluding(logEvent => logEvent.Exception != null
                                          && (logEvent.Exception.GetType() == typeof(OperationCanceledException) ||
                                              logEvent.Exception.GetType() == typeof(ObjectDisposedException)))
          // Pass on to UI logging
          .WriteTo.Sink(m_UserInterfaceSink)

          // File Exception logging
          .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(le => le.Exception != null)
              .WriteTo.File(folder + "ExceptionLog.txt", rollingInterval: RollingInterval.Month,
                retainedFileCountLimit: 3, encoding: Encoding.UTF8,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}\t{Level}\t\"{Exception:l}\"{NewLine}"),
            LogEventLevel.Error)

          // File Regular logging
          .WriteTo.File(folder + "ApplicationLog.txt", rollingInterval: RollingInterval.Day, encoding: Encoding.UTF8,
            outputTemplate: "{Timestamp:HH:mm:ss}\t{Level:w3}\t{Message:l}{NewLine}");


        // Json
        // .WriteTo.File(formatter: new JsonFormatter(renderMessage: true), path: folder + "ApplicationLog.json",
        //  rollingInterval: RollingInterval.Day);
        // Set the general Logger
        Logger.LoggerInstance =
          new SerilogLoggerProvider(loggerConfiguration.CreateLogger()).CreateLogger(nameof(CsvTools));
      }
      catch (Exception ex)
      {
        // ignore
        Debug.WriteLine(ex.Message);
      }
    }

    public static void RemoveLog(Microsoft.Extensions.Logging.ILogger value)
      => m_UserInterfaceSink.AdditionalLoggers.Remove(value ?? throw new ArgumentNullException(nameof(value)));

    public static void AddLog(Microsoft.Extensions.Logging.ILogger value)
      => m_UserInterfaceSink.AdditionalLoggers.Add(value ?? throw new ArgumentNullException(nameof(value)));

    [DebuggerStepThrough]
    public class UserInterfaceSink : ILogEventSink
    {
      public readonly List<Microsoft.Extensions.Logging.ILogger> AdditionalLoggers =
        new List<Microsoft.Extensions.Logging.ILogger>();

      // By design: Only one Action to be called you can not have two or more destinations
      private readonly IFormatProvider m_FormatProvider;

      public UserInterfaceSink(IFormatProvider formatProvider)
      {
        m_FormatProvider = formatProvider;
      }

      /// <inheritdoc cref="ILogEventSink"/>
      public void Emit(LogEvent logEvent)
      {
        if (AdditionalLoggers.Count <= 0)
          return;
        var level = logEvent.Level switch
        {
          LogEventLevel.Verbose => LogLevel.Trace,
          LogEventLevel.Debug => LogLevel.Debug,
          LogEventLevel.Warning => LogLevel.Warning,
          LogEventLevel.Error => LogLevel.Error,
          LogEventLevel.Fatal => LogLevel.Critical,
          _ => LogLevel.Information
        };

        foreach (var logger in AdditionalLoggers)
          logger.Log(level, logEvent.RenderMessage(m_FormatProvider));
      }
    }
  }
}