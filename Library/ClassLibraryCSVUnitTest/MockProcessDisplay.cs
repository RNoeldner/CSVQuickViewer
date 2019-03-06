using System;
using System.Threading;

namespace CsvTools.Tests
{
  public class MockProcessDisplay : IProcessDisplay
  {
    public bool m_Disposed;
    public bool m_Shown = false;
    public string Text;
    public bool Visible;
    public virtual string Title { get; set; }
    public TimeToCompletion TimeToCompletion => new TimeToCompletion();

    public CancellationToken CancellationToken => CancellationToken.None;

    public int Maximum { get; set; }

    public event EventHandler<ProgressEventArgs> Progress;

    public virtual void Dispose()
    {
      Visible = true;
      m_Disposed = true;
    }

    public void Cancel()
    {
    }

    public void SetProcess(string text, int value = -1)
    {
      Text = text;
      Progress?.Invoke(this, new ProgressEventArgs(text));
    }

    public void SetProcess(object sender, ProgressEventArgs e)
    {
      Text = e.Text;
      Progress?.Invoke(sender, e);
    }

    public event EventHandler ProgressStopEvent;

    public void Close()
    {
      Visible = true;
      m_Disposed = true;
      ProgressStopEvent?.Invoke(this, null);
    }
  }
}