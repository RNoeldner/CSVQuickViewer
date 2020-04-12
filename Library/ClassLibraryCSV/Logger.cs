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

      Error = 60,

      None = 100,
    }

    public static void Configure(string fileNameJson, Level level, string fileNameText = null)
    {
      var config = LogManager.Configuration ?? new LoggingConfiguration();
      if (level != Level.None)
      {
        var minLevel = LogLevel.Debug;
        switch (level)
        {
          case Level.Info:
            minLevel = LogLevel.Info;
            break;
          case Level.Warn:
            minLevel = LogLevel.Warn;
            break;
          case Level.Error:
            minLevel = LogLevel.Error;
            break;
        }

        if (!string.IsNullOrEmpty(fileNameJson))
        {
          var existing = config.AllTargets.FirstOrDefault(x => x is FileTarget target && target.Layout is JsonLayout);
          if (existing == null)
          {
            var logfileRoot = new FileTarget("jsonFile")
                                {
                                  FileName = fileNameJson,
                                  ArchiveAboveSize = 2097152,
                                  Layout = new JsonLayout
                                             {
                                               Attributes =
                                                 {
                                                   new JsonAttribute("time", "${longdate}", false),
                                                   new JsonAttribute("level", "${level}", false),
                                                   new JsonAttribute("message", "${message}", true),

                                                   // new JsonAttribute("properties", "${all-event-properties}"),
                                                   new JsonAttribute(
                                                     "properties",
                                                     new JsonLayout
                                                       {
                                                         IncludeAllProperties = true,
                                                         MaxRecursionLimit = 2,
                                                         RenderEmptyObject = true
                                                       }),
                                                   new JsonAttribute("type", "${exception:format=Type}"),
                                                   new JsonAttribute(
                                                     "innerException",
                                                     new JsonLayout
                                                       {
                                                         Attributes =
                                                           {
                                                             new JsonAttribute(
                                                               "type",
                                                               "${exception:format=:innerFormat=Type:MaxInnerExceptionLevel=2:InnerExceptionSeparator=}"),
                                                             new JsonAttribute(
                                                               "message",
                                                               "${exception:format=:innerFormat=Message:MaxInnerExceptionLevel=2:InnerExceptionSeparator=}")
                                                           },
                                                         RenderEmptyObject = false
                                                       },
                                                     false),
                                                 }
                                             }
                                };
            config.AddRule(minLevel, LogLevel.Fatal, logfileRoot);
          }
          else
          {
            // TODO change the level            
          }
        }

        // second log file in folder
        if (!string.IsNullOrEmpty(fileNameText))
        {
          var logfileFolder = new FileTarget("logfile2") { FileName = fileNameText, ArchiveAboveSize = 2097152 };
          var layout = new CsvLayout()
                         {
                           Delimiter = CsvColumnDelimiterMode.Tab, WithHeader = true, Quoting = CsvQuotingMode.Auto
                         };
          layout.Columns.Add(
            new CsvColumn { Name = "Time", Layout = "${longdate}", Quoting = CsvQuotingMode.Nothing });
          layout.Columns.Add(new CsvColumn { Name = "Level", Layout = "${level}", Quoting = CsvQuotingMode.Nothing });
          layout.Columns.Add(new CsvColumn { Name = "Message", Layout = "${message}" });
          layout.Columns.Add(new CsvColumn { Name = "Exception", Layout = "${exception:format=toString}" });
          logfileFolder.Layout = layout;
          if (config.AllTargets.Any(x => x is FileTarget target && !(target.Layout is JsonLayout)))
            config.RemoveTarget("logfile2");
          config.AddRule(minLevel, LogLevel.Fatal, logfileFolder);
        }
      }

      // Apply configuration
      LogManager.Configuration = config;
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
        var logEnvent = new LogEventInfo(
          level: level,
          loggerName: "screen",
          formatProvider: null,
          message: message,
          parameters: args,
          exception: exception);
        AddLog.Invoke(logEnvent.FormattedMessage, lvl);
      }
      catch (Exception)
      {
        // ignore
      }
    }
  }
}