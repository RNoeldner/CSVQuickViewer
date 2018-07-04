using System.Windows.Forms;
using log4net.Appender;

namespace CsvTools
{
  public class TextBoxTraceListener : AppenderSkeleton
  {
    public TextBoxTraceListener()
    {
    }

    public TextBoxTraceListener(TextBox textBox)
    {
      AppenderTextBox = textBox;
    }

    public TextBox AppenderTextBox
    {
      get;
      set;
    }

    protected override void Append(log4net.Core.LoggingEvent loggingEvent)
    {
      if (AppenderTextBox != null)
        AppenderTextBox.SafeBeginInvoke(() => AppenderTextBox.AppendText(RenderLoggingEvent(loggingEvent)));
    }

    public override bool Flush(int timeout)
    {
      Extensions.ProcessUIElements();
      return base.Flush(timeout);
    }
  }
}