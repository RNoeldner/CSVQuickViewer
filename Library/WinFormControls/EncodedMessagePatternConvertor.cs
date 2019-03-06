using log4net.Util;
using System.Diagnostics;
using System.IO;

namespace CsvTools
{
  public class EncodedMessagePatternConvertor : PatternConverter
  {
    protected override void Convert(TextWriter writer, object state)
    {
      var loggingEvent = state as log4net.Core.LoggingEvent;
      if (string.IsNullOrEmpty(loggingEvent?.RenderedMessage)) return;

      // Replace newline characters with spaces
      var encodedMessage = StringUtils.HandleCRLFCombinations(loggingEvent.RenderedMessage);

      // Get only the innermost exception
      var ex = loggingEvent.ExceptionObject;
      System.Exception last = null;
      while (ex != null)
      {
        last = ex;
        ex = ex.InnerException;
      }
      encodedMessage += GetEntryForException(last);

      if (encodedMessage.IndexOfAny(new[] { '\r', '\n', '\t' }) != -1)
        writer.Write("\"" + encodedMessage.Replace("\"", "'") + "\"");
      else
        writer.Write(encodedMessage);
    }

    private static string GetEntryForException(System.Exception ex)
    {
      if (ex == null) return string.Empty;
      var message = $"\r {ex.GetType()}: {StringUtils.HandleCRLFCombinations(ex.Message)}";
      if (!string.IsNullOrEmpty(ex.StackTrace))
      {
        // Get lowest frame that is in CsvTools
        foreach (var frame in new StackTrace(ex, true).GetFrames())
        {
          try
          {
            var method = frame.GetMethod();
            if (method == null) continue;
            if (method.DeclaringType.Namespace.Equals("CsvTools", System.StringComparison.OrdinalIgnoreCase))
            {
              message += $"\r  at {method.DeclaringType.Name}.{method.Name}";
              var fileName = frame.GetFileName();
              if (!string.IsNullOrEmpty(fileName))
              {
                fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1, fileName.Length - fileName.LastIndexOf('\\') - 1);
                message += $" in {fileName}:line {frame.GetFileLineNumber()}";
              }
            }
          }
          catch
          {
          }
        }
      }
      return message;
    }
  }
}