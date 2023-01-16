using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using CsvTools;

namespace Maui
{
  public class PassingLogger : ILogger
  {
    private readonly Action<string> m_PassLog;
    private string m_LastMessage = string.Empty;
    public PassingLogger(Action<string> passLog)
    {
      m_PassLog = passLog;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
      if (!IsEnabled(logLevel))
        return;
      var text = formatter(state, exception).HandleCrlfCombinations(" ");
      if (string.IsNullOrWhiteSpace(text) && text != "\"\"" || m_LastMessage.Equals(text, StringComparison.Ordinal))
        return;
      m_PassLog(text);
      m_LastMessage = text;
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel > LogLevel.Debug;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullLogger.Instance.BeginScope(state);
  }

}
