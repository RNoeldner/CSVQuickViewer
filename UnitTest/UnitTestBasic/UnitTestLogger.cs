using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;


namespace CsvTools
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

    public IDisposable BeginScope<TState>(TState state) => NullLogger.Instance.BeginScope(state);
  }
}
