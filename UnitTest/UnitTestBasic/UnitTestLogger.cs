using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;


namespace CsvTools.Tests
{
  public class UnitTestLogger : ILogger
  {
    public readonly TestContext? Context;
    public string LastMessage;

    public UnitTestLogger(TestContext? context)
    {
      Context = context;
      LastMessage = string.Empty;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
      LastMessage = formatter.Invoke(state, exception);
      Context?.WriteLine($"{logLevel} - {LastMessage}");
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel > LogLevel.Debug;

#pragma warning disable CS8633
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullLogger.Instance.BeginScope(state);
#pragma warning restore CS8633
  }
}
