using log4net.Appender;
using System.Windows.Forms;

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

    public override bool Flush(int timeout)
    {
      Extensions.ProcessUIElements();
      return base.Flush(timeout);
    }

    protected override void Append(log4net.Core.LoggingEvent loggingEvent)
    {
      if (AppenderTextBox != null && loggingEvent.MessageObject.ToString().Length > 0)
      {
        AppenderTextBox.SafeBeginInvoke(() => AppenderTextBox.AppendText(StringUtils.HandleCRLFCombinations(RenderLoggingEvent(loggingEvent), " ") + "\r\n"  ));
        Extensions.ProcessUIElements();
      }
    }
  }
}