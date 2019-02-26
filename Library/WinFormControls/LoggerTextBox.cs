using log4net;
using log4net.Repository.Hierarchy;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace CsvTools
{
  public class LoggerTextBox : TextBox
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly TextBoxTraceListener m_TextBoxTraceListener;    
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        var hierarchy = (Hierarchy)LogManager.GetRepository();
        hierarchy.Root.RemoveAppender(m_TextBoxTraceListener);
        m_TextBoxTraceListener.Close();
      }

      base.Dispose(disposing);
    }

    public void BeginSection(string text)
    {
      AppendText($"\n{text}\n");
      Extensions.ProcessUIElements();
    }

    public LoggerTextBox()
    {
      m_TextBoxTraceListener = new TextBoxTraceListener(this);
      Multiline = true;
      ScrollBars = ScrollBars.Both;
      KeyUp += base.FindForm().CtrlA;

      try
      {
        var hierarchy = (Hierarchy)LogManager.GetRepository();

        var patternLayout = new CustomPatternLayout
        {
          ConversionPattern = "%date{HH:mm:ss,fff}  %encodedmessage",
          IgnoresException = false
        };
        patternLayout.ActivateOptions();
        m_TextBoxTraceListener.Layout = patternLayout;
        m_TextBoxTraceListener.Threshold = log4net.Core.Level.Debug;
        m_TextBoxTraceListener.ActivateOptions();
        hierarchy.Root.AddAppender(m_TextBoxTraceListener);
        hierarchy.Configured = true;
      }
      catch (Exception ex)
      {
        Log.Error("Error setting the log file", ex);
      }
    }
  }
}