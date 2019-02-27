using log4net.Appender;
using System;
using System.Windows.Forms;

namespace CsvTools
{
  public class TextBoxTraceListener : AppenderSkeleton
  {
    private string m_LastMessage = string.Empty;

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

    public void Clear()
    {
      AppenderTextBox.SafeBeginInvoke(() =>
      {
        AppenderTextBox.Text = string.Empty;
      });
      Extensions.ProcessUIElements();
    }

    public override bool Flush(int timeout)
    {
      Extensions.ProcessUIElements();
      return base.Flush(timeout);
    }

    protected override void Append(log4net.Core.LoggingEvent loggingEvent)
    {
      string text = loggingEvent.MessageObject.ToString();
      if (AppenderTextBox != null && text.Length > 0)
      {
        AppenderTextBox.SafeBeginInvoke(() =>
        {
          var appended = false;
          var posSlash = text.IndexOf('–', 0);
          if (posSlash != -1 && m_LastMessage.StartsWith(text.Substring(0, posSlash + 1), StringComparison.Ordinal))
          {

            AppenderTextBox.AppendText(text.Substring(posSlash - 1));
            appended = true;
          }
          m_LastMessage = text;
          if (!appended)
          {
            if (AppenderTextBox.Text.Length > 0)
              AppenderTextBox.AppendText(Environment.NewLine);
            AppenderTextBox.AppendText(StringUtils.HandleCRLFCombinations(RenderLoggingEvent(loggingEvent), " "));
          }
        });
        Extensions.ProcessUIElements();
      }
    }
  }
}