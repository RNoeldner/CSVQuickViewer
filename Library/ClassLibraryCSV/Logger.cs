#define NLog
using NLog;
using Pri.LongPath;
using System;


namespace CsvTools
{
  /// <summary>
  /// Abstraction to be able to switch Loggers
  /// </summary>
  public static class Logger
  {
    public static Action<string, Level> AddLog;

    public enum Level
    {
      Debug = 0,
      Info = 1,
      Warn = 2,
      Error = 3
    }
#if log4net
    private static readonly log4net.ILog m_Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#endif

#if NLog
    private static readonly NLog.Logger m_Logger = NLog.LogManager.GetCurrentClassLogger();
#endif

    public static void Configure(string fileName, Level level, string folder=null)
    {
#if NLog
      var config = new NLog.Config.LoggingConfiguration();
      var minLevel = LogLevel.Debug;
      if (level == Level.Info)
        minLevel = LogLevel.Info;
      else if (level == Level.Warn)
        minLevel = LogLevel.Warn;
      else if (level == Level.Error)
        minLevel = LogLevel.Error;

      
      var logfileRoot = new NLog.Targets.FileTarget("logfile") { FileName = fileName, Layout = "${longdate} ${level} ${message}  ${exception}" };
      // second log file
      if (folder != null)
      {
        var logfileFolder = new NLog.Targets.FileTarget("logfile") { FileName = Path.Combine(folder, fileName), Layout = "${longdate} ${level} ${message}  ${exception}" };
        config.AddRule(minLevel, LogLevel.Fatal, logfileFolder);
      }
      config.AddRule(minLevel, LogLevel.Fatal, logfileRoot);
    
      // Apply config           
      NLog.LogManager.Configuration = config;
#endif
    }

    public static void Warning(string message, Exception exception = null, params object[] args)
    {
#if log4net
      m_Logger.Warn(message,exception);
#endif
#if NLog
      m_Logger.Warn(exception, message, args);
#endif
      if (AddLog != null)
        try
        {
          AddLog.Invoke(message, Level.Warn);
        }
        catch (Exception)
        {
          // ignore
        }

    }
    public static void Debug(string message, params object[] args)
    {
      m_Logger.Info(message, args);
      if (AddLog != null)
        try
        {
          AddLog.Invoke(message, Level.Debug);
        }
        catch (Exception)
        {
          // ignore
        }
    }

    public static void Information(string message, params object[] args)
    {
      m_Logger.Info(message, args);
      if (AddLog != null)
        try
        {
          AddLog.Invoke(message, Level.Info);
        }
        catch (Exception)
        {
          // ignore
        }
    }

    public static void Error(string message, Exception exception = null, params object[] args)
    {
#if log4net
      m_Logger.Error(message, exception );
#endif
#if NLog
      m_Logger.Error(exception, message, args);
#endif
      if (AddLog != null)
        try
        {
          AddLog.Invoke(message, Level.Error);
        }
        catch (Exception)
        {
          // ignore
        }
    }

    public static void Error(Exception exception)
    {
      Error(exception.Message, exception);
    }
  }
}
