namespace CsvTools
{
  using System;
  using System.Linq;

  using NLog;
  using NLog.Config;
  using NLog.Layouts;
  using NLog.Targets;

  /// <summary>
  ///   Abstraction to be able to switch Loggers
  /// </summary>
  public static class Logger
  {
    public static Action<string, Level> AddLog;

    private static readonly NLog.Logger m_Logger = LogManager.GetCurrentClassLogger();

    public enum Level
    {
      Debug = 30,
      Info = 40,
      Warn = 50,
      Error = 60
    }

    public static void Debug(string message, params object[] args)
    {
      if (string.IsNullOrEmpty(message))
        return;
      m_Logger.Debug(message, args);
      WriteLog(Level.Debug, null, message, args);
    }

    public static void Error(string message, params object[] args)
    {
      if (string.IsNullOrEmpty(message))
        return;
      m_Logger.Error(message, args);
      WriteLog(Level.Error, null, message, args);
    }

    public static void Error(Exception exception, string message, params object[] args)
    {
      if (string.IsNullOrEmpty(message) && exception == null)
        return;
      m_Logger.Error(exception, message, args);
      WriteLog(Level.Error, exception, message, args);
    }

    public static void Error(Exception exception) => Error(exception, exception.Message);

    public static void Information(string message, params object[] args)
    {
      if (string.IsNullOrEmpty(message))
        return;
      m_Logger.Info(message, args);
      WriteLog(Level.Info, null, message, args);
    }

    public static void Warning(string message, params object[] args) => Warning(null, message, args);

    public static void Warning(Exception exception, string message, params object[] args)
    {
      if (string.IsNullOrEmpty(message) && exception == null)
        return;
      m_Logger.Warn(exception, message, args);
      WriteLog(Level.Warn, exception, message, args);
    }

    private static void WriteLog(Level lvl, Exception exception, string message, params object[] args)
    {
      if (AddLog == null)
        return;

      var level = LogLevel.Debug;
      switch (lvl)
      {
        case Level.Info:
          level = LogLevel.Info;
          break;

        case Level.Warn:
          level = LogLevel.Warn;
          break;

        case Level.Error:
          level = LogLevel.Error;
          break;
      }

      try
      {
        var logEnvent = new LogEventInfo(level, "screen", null, message, args, exception);
        AddLog.Invoke(logEnvent.FormattedMessage, lvl);
      }
      catch (Exception)
      {
        // ignore
      }
    }
  }
}