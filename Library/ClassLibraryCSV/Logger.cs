
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
#endif
  using System;
  using Serilog.Formatting.Json;



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
    private static UserInterfaceSink m_UserInterfaceSink = new UserInterfaceSink(CultureInfo.CurrentCulture);
    public static Action<string, Level> AddLog
    {
      get => m_UserInterfaceSink.AddLog;
      set => m_UserInterfaceSink.AddLog = value;
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
      Log.Logger = new LoggerConfiguration()
         .Filter.ByExcluding(logEvent => logEvent.Exception != null
          && (logEvent.Exception.GetType() == typeof(OperationCanceledException)
          || logEvent.Exception.GetType() == typeof(ObjectDisposedException)))
         // UI
         .WriteTo.Sink(m_UserInterfaceSink)
         // Exceptions
         .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(le => le.Exception!=null).WriteTo.File("Exceptions.log"))
         //CSV
         .WriteTo.File("CSVFileValidator.log", fileSizeLimitBytes: 32768, outputTemplate: "{Timestamp:HH:mm:ss}\t{Level:u3}\t{Message}{NewLine}", rollOnFileSizeLimit: true, retainedFileCountLimit: 5)
         // Json
         .WriteTo.File(formatter: new JsonFormatter(), path: "CSVQuickViewer.json", fileSizeLimitBytes: 32768, rollOnFileSizeLimit: true, retainedFileCountLimit: 5)
        .CreateLogger();
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
      Log.Error(exception, message, args);
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
      Log.Warning(exception, message, args);
#endif

    }

#if !NLog
    private class UserInterfaceSink : ILogEventSink
    {
      private readonly IFormatProvider _formatProvider;
      public UserInterfaceSink(IFormatProvider formatProvider)
      {
        _formatProvider = formatProvider;
      }

      // By design: Only one Action to be called you can not have two or more destinations
      public Action<string, Logger.Level> AddLog;

      public void Emit(LogEvent logEvent)
      {
        if (AddLog!=null)
        {
          Logger.Level level;
          switch (logEvent.Level)
          {
            case LogEventLevel.Verbose:
            case LogEventLevel.Debug:
              level = Logger.Level.Debug;
              break;
            case LogEventLevel.Information:
              level = Logger.Level.Info;
              break;
            case LogEventLevel.Warning:
              level = Logger.Level.Warn;
              break;
            default:
              level = Logger.Level.Error;
              break;

          }
          AddLog(logEvent.RenderMessage(_formatProvider), level);
        }
      }
    }
#else
    private static void WriteLog(Level lvl, Exception exception, string message, params object[] args)
    {
      if (AddLog == null)
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
        AddLog.Invoke(logEnvent.FormattedMessage, lvl);

      }
      catch (Exception)
      {
        // ignore
      }
    }
#endif
  }
}