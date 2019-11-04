using System;

using NLog;
using NLog.Layouts;
using Pri.LongPath;

namespace CsvTools
{
  /// <summary>
  /// Abstraction to be able to switch Loggers
  /// </summary>
  public static class Logger
  {
    public static Action<string, Level> AddLog;

    private static readonly NLog.Logger m_Logger = NLog.LogManager.GetCurrentClassLogger();

    public enum Level
    {
      Debug = 30,
      Info = 40,
      Warn = 50,
      Error = 60,
      None = 100,
    }

    public static void Configure(string fileNameJson, Level level, string folder = null)
    {
      var config = new NLog.Config.LoggingConfiguration();
      if (level != Level.None)
      {
        var minLevel = LogLevel.Debug;
        if (level == Level.Info)
          minLevel = LogLevel.Info;
        else if (level == Level.Warn)
          minLevel = LogLevel.Warn;
        else if (level == Level.Error)
          minLevel = LogLevel.Error;

        if (!string.IsNullOrEmpty(fileNameJson))
        {
          var logfileRoot = new NLog.Targets.FileTarget("jsonFile") { FileName = fileNameJson };
          logfileRoot.Layout = new JsonLayout
          {
            Attributes =
          {
            new JsonAttribute("time", "${longdate}"),
            new JsonAttribute("type", "${exception:format=Type}"),
            new JsonAttribute("message", "${message}"),
             new JsonAttribute("properties", "${all-event-properties}"),
            new JsonAttribute("innerException", new JsonLayout
              {
                  Attributes =
                  {
                      new JsonAttribute("type", "${exception:format=:innerFormat=Type:MaxInnerExceptionLevel=2:InnerExceptionSeparator=}"),
                      new JsonAttribute("message", "${exception:format=:innerFormat=Message:MaxInnerExceptionLevel=2:InnerExceptionSeparator=}"),
                  },
                 RenderEmptyObject = false
              },
             true),
          }
          };
          config.AddRule(minLevel, LogLevel.Fatal, logfileRoot);
        }

        // second log file in folder
        if (folder != null)
        {
          var logfileFolder = new NLog.Targets.FileTarget("logfile2") { FileName = Path.Combine(folder, fileNameJson), Layout = "${longdate} ${level} ${message}  ${exception}" };
          config.AddRule(minLevel, LogLevel.Fatal, logfileFolder);
        }
        // otherwise add a console logger
        else
        {
          var logfileFolder = new NLog.Targets.ConsoleTarget("console");
          config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfileFolder);
        }
      }

      // Apply configuration
      NLog.LogManager.Configuration = config;
      Debug("Logging started");
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
      if (lvl == Level.Info)
        level = LogLevel.Info;
      else if (lvl == Level.Warn)
        level = LogLevel.Warn;
      else if (lvl == Level.Error)
        level = LogLevel.Error;

      try
      {
        var logEnvent = new LogEventInfo(level: level, loggerName: "screen", formatProvider: null, message: message, parameters: args, exception: exception);
        AddLog.Invoke(logEnvent.FormattedMessage, lvl);
      }
      catch (Exception)
      {
        // ignore
      }
    }
  }
}