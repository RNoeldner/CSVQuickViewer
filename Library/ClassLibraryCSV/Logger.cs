using JetBrains.Annotations;

namespace CsvTools
{
#if NLog
  using NLog;
  using System.IO;
  using System.Text;
#else
  using Serilog;
  using Serilog.Core;
  using Serilog.Events;
  using System.Globalization;
  using Serilog.Formatting.Json;
  using System.Diagnostics;
  using System.Reflection;
#endif
  using System;
  using System.Collections.Generic;


#if !NLog

#endif
  /// <summary>
  ///   Abstraction to be able to switch Loggers
  /// </summary>
  public static class Logger
  {
#if NLog
    private static readonly NLog.Logger m_Logger = LogManager.GetCurrentClassLogger();
#else
    private static readonly UserInterfaceSink m_UserInterfaceSink = new UserInterfaceSink(CultureInfo.CurrentCulture);

    public static Action<string, Level> UILog
    {
      set
      {
        if (value == null) throw new ArgumentNullException(nameof(value));
        m_UserInterfaceSink.Loggers.Clear();
        m_UserInterfaceSink.Loggers.Add(value);
      }
    }

    public static void AddLog([NotNull] Action<string, Level> value)
    {
      if (value == null) throw new ArgumentNullException(nameof(value));
      m_UserInterfaceSink.Loggers.Add(value);
    }

    public static void RemoveLog([NotNull] Action<string, Level> value)
    {
      if (value == null) throw new ArgumentNullException(nameof(value));
      m_UserInterfaceSink.Loggers.Remove(value);
    }

#endif
    public enum Level
    {
      Debug = 30,
      Info = 40,
      Warn = 50,
      Error = 60
    }

    static Logger()
    {
      // Base configuration
      var loggerConfiguration = new LoggerConfiguration()
        .Filter.ByExcluding(logEvent => logEvent.Exception != null
                                        && (logEvent.Exception.GetType() == typeof(OperationCanceledException)
                                            || logEvent.Exception.GetType() == typeof(ObjectDisposedException)))
        // UI Logger
        .WriteTo.Sink(m_UserInterfaceSink);
      // File Logger
      var entryName = Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty;
      if (!string.IsNullOrEmpty(entryName))
      {
        var folder = Environment.ExpandEnvironmentVariables($"%LocalAppData%\\{entryName}\\");
        var addTextLog = FileSystemUtils.DirectoryExists(folder);
        if (!addTextLog)
        {
          try
          {
            FileSystemUtils.CreateDirectory(folder);
          }
          catch (Exception)
          {
            addTextLog = false;
          }
        }

        if (addTextLog)
        {
          loggerConfiguration = loggerConfiguration
            // Exceptions
            .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(le => le.Exception != null).WriteTo.File(
              folder + "ExceptionLog.txt", rollingInterval: RollingInterval.Month, retainedFileCountLimit: 3,
              outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}\t{Level}\t\"{Exception:l}\"{NewLine}"))
            //CSV
            .WriteTo.File(folder + "ApplicationLog.txt", rollingInterval: RollingInterval.Day,
              outputTemplate: "{Timestamp:HH:mm:ss}\t{Level:w3}\t{Message:l}{NewLine}")
            // Json
            .WriteTo.File(formatter: new JsonFormatter(renderMessage: true), path: folder + "ApplicationLog.json",
              rollingInterval: RollingInterval.Day);
        }
      }

      // Start logging
      Log.Logger = loggerConfiguration.CreateLogger();
      Log.Information("Application start");
    }

    public static void Debug(string message, params object[] args)
    {
      if (string.IsNullOrEmpty(message))
        return;

#if NLog
      m_Logger.Debug(message, args);
WriteLog(Level.Debug, null, message, args);
#else
      Log.Debug(message, args);
#endif
    }

    public static void Error(string message, params object[] args)
    {
      if (string.IsNullOrEmpty(message))
        return;
#if NLog
m_Logger.Error(message, args);
WriteLog(Level.Error, null, message, args);
#else
      Log.Error(message, args);
#endif
    }

    public static void Error(Exception exception, string message, params object[] args)
    {
      if (string.IsNullOrEmpty(message) && exception == null)
        return;
#if NLog
m_Logger.Error(exception, message, args);
WriteLog(Level.Error, exception, message, args);
#else
      Log.Error(exception?.Demystify(), message, args);
#endif
    }

    public static void Error(Exception exception) => Error(exception, exception.Message);

    public static void Information(string message, params object[] args)
    {
      if (string.IsNullOrEmpty(message))
        return;
#if NLog
m_Logger.Info(message, args);
WriteLog(Level.Info, null, message, args);
#else
      Log.Information(message, args);
#endif
    }

    public static void Warning(string message, params object[] args) => Warning(null, message, args);

    public static void Warning(Exception exception, string message, params object[] args)
    {
      if (string.IsNullOrEmpty(message) && exception == null)
        return;
#if NLog
m_Logger.Warn(exception, message, args);
 WriteLog(Level.Warn, exception, message, args);
#else
      Log.Warning(exception?.Demystify(), message, args);
#endif
    }

#if !NLog
    public class UserInterfaceSink : ILogEventSink
    {
      private readonly IFormatProvider _formatProvider;

      // By design: Only one Action to be called you can not have two or more destinations
      public readonly List<Action<string, Logger.Level>> Loggers = new List<Action<string, Level>>();

      public UserInterfaceSink(IFormatProvider formatProvider)
      {
        _formatProvider = formatProvider;
      }


      public void Emit(LogEvent logEvent)
      {
        if (Loggers.Count <= 0) return;
        Level level;
        switch (logEvent.Level)
        {
          case LogEventLevel.Verbose:
          case LogEventLevel.Debug:
            level = Level.Debug;
            break;
          case LogEventLevel.Information:
            level = Level.Info;
            break;
          case LogEventLevel.Warning:
            level = Level.Warn;
            break;
          default:
            level = Level.Error;
            break;
        }

        foreach (var logger in Loggers)
          logger(logEvent.RenderMessage(_formatProvider), level);
      }
    }
#else
    private static void WriteLog(Level lvl, Exception exception, string message, params object[] args)
    {
      if (ReplaceLog == null)
        return;

      try
      {

      LogLevel level;
      switch (lvl)
      {
        case Level.Debug:
          level = LogLevel.Debug;
          break;

        case Level.Info:
          level = LogLevel.Info;
          break;

        case Level.Warn:
          level = LogLevel.Warn;
          break;

        case Level.Error:
          level = LogLevel.Error;
          break;

        default:
          throw new ArgumentOutOfRangeException(nameof(lvl), lvl, null);
      }
        var logEnvent = new LogEventInfo(level, "screen", null, message, args, exception);
        ReplaceLog.Invoke(logEnvent.FormattedMessage, lvl);

      }
      catch (Exception)
      {
        // ignore
      }
    }
#endif
  }
}