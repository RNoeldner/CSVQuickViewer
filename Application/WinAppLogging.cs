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

namespace CsvTools;

public class QuotingEnricher : ILogEventEnricher
{
  private static char[] needQuoting = new[] { '\t', '\n', '\r' };
  public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
  {

    string QuoteAndEscape(string? value)
    {
      if (string.IsNullOrEmpty(value))
        return string.Empty;
      // Escape internal quotes by doubling them, then wrap in quotes
      return value!.IndexOfAny(needQuoting)!=-1 ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
    }

    logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(
      "QuotedMessage", QuoteAndEscape(logEvent.MessageTemplate.Text)));

    logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(
      "QuotedException", QuoteAndEscape(logEvent.Exception?.ToString())));
  }
}

/// <summary>
/// Provides centralized setup for application logging using Serilog and Microsoft.Extensions.Logging.
/// Writes structured logs to disk and optionally forwards them to registered UI loggers.
/// </summary>
public static class WinAppLogging
{
  // Shared sink instance forwarding log messages to the UI layer
  private static readonly UserInterfaceSink m_UserInterfaceSink = new UserInterfaceSink(CultureInfo.CurrentCulture);

  /// <summary>
  /// Initializes the application logging pipeline.
  /// Creates the log directory under AppData\Roaming\<AppName>\Logs
  /// and configures Serilog sinks for file and UI output.
  /// </summary>
  public static void Init()
  {
    try
    {
      var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name ??
                         Assembly.GetExecutingAssembly().GetName().Name;

      // This should go to AppData\Roaming\Validator\
      var folder = Environment.ExpandEnvironmentVariables($"%AppData%\\{assemblyName}\\Logs\\");
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
        .MinimumLevel.Information()
        .Enrich.With<QuotingEnricher>()
        .Filter.ByExcluding(logEvent => logEvent.Exception != null
                                        && (logEvent.Exception.GetType() == typeof(OperationCanceledException) ||
                                            logEvent.Exception.GetType() == typeof(ObjectDisposedException)))
        // Pass on to UI logging
        .WriteTo.Sink(m_UserInterfaceSink)

        // File Exception logging
        .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(le => le.Exception != null)
            .WriteTo.File(folder + "ExceptionLog.txt", rollingInterval: RollingInterval.Month,
              retainedFileCountLimit: 2, encoding: Encoding.UTF8,
              outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}\t{Level}\t{QuotedException}{NewLine}"),
          LogEventLevel.Error)

        // File Regular logging
        .WriteTo.File(folder + "ApplicationLog.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30, encoding: Encoding.UTF8,
          outputTemplate: "{Timestamp:HH:mm:ss}\t{Level:w3}\t{QuotedMessage}{NewLine}");

      // Set the general Logger
      Logger.LoggerInstance = new SerilogLoggerProvider(loggerConfiguration.CreateLogger()).CreateLogger(nameof(CsvTools));
    }
    catch (Exception ex)
    {
      // ignore
      Debug.WriteLine(ex.Message);
    }
  }

  /// <summary>
  /// Removes a UI logger from the list of additional log targets.
  /// </summary>
  public static void RemoveLog(Microsoft.Extensions.Logging.ILogger value)
    => m_UserInterfaceSink.AdditionalLoggers.Remove(value ?? throw new ArgumentNullException(nameof(value)));

  /// <summary>
  /// Adds a UI logger to receive forwarded log messages.
  /// </summary>
  public static void AddLog(Microsoft.Extensions.Logging.ILogger value)
    => m_UserInterfaceSink.AdditionalLoggers.Add(value ?? throw new ArgumentNullException(nameof(value)));

  /// <summary>
  /// Custom Serilog sink used to forward messages to registered <see cref="ILogger"/> instances.
  /// Typically used to display logs in a GUI control or other in-memory consumer.
  /// </summary>
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