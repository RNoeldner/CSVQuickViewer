using log4net.Appender;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools
{
  public class LogAppenderTextBox : AppenderSkeleton
  {
    private string m_LastMessage = string.Empty;

    public LogAppenderTextBox()
    {
    }

    public LogAppenderTextBox(TextBoxBase textBox)
    {
      AppenderTextBox = textBox;
    }

    public TextBoxBase AppenderTextBox
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

    public void AppendText(string text, log4net.Core.Level level)
    {
      var col = AppenderTextBox.ForeColor;
      if (level.Value < log4net.Core.Level.Info.Value)
        col = Color.Gray;
      if (level.Value >= log4net.Core.Level.Warn.Value)
        col = Color.Blue;
      if (level.Value >= log4net.Core.Level.Error.Value)
        col = Color.Red;

      AppenderTextBox.SelectionStart = AppenderTextBox.TextLength;
      if (col != AppenderTextBox.ForeColor && AppenderTextBox is RichTextBox rtb)
      {
        AppenderTextBox.SelectionLength = 0;
        rtb.SelectionColor = col;
      }
      AppenderTextBox.ScrollToCaret();
      AppenderTextBox.AppendText(StringUtils.HandleCRLFCombinations(text, " "));

      if (col != AppenderTextBox.ForeColor && AppenderTextBox is RichTextBox rtb2)
        rtb2.SelectionColor = AppenderTextBox.ForeColor;
    }

    protected override void Append(log4net.Core.LoggingEvent loggingEvent)
    {
      string text = loggingEvent.MessageObject.ToString();
      if (AppenderTextBox != null && text.Length > 0 && !m_LastMessage.Equals(text, StringComparison.Ordinal))
      {
        AppenderTextBox.SafeBeginInvoke(() =>
        {

          var appended = false;
          var posSlash = text.IndexOf('–', 0);
          if (posSlash != -1 && m_LastMessage.StartsWith(text.Substring(0, posSlash + 1), StringComparison.Ordinal))
          {
            // add to previous item,
            AppendText(text.Substring(posSlash - 1), loggingEvent.Level);
            appended = true;
          }
          m_LastMessage = text;
          if (!appended)
          {
            if (AppenderTextBox.Text.Length > 0)
              AppenderTextBox.AppendText(Environment.NewLine);
            AppenderTextBox.AppendText($"{loggingEvent.TimeStamp:HH:mm:ss,fff}\t");
            AppendText(text, loggingEvent.Level);
          }
        });
        Extensions.ProcessUIElements();
      }
    }
  }
}