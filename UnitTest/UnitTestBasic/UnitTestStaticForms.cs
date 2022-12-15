using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace CsvTools.Tests
{
  public static class UnitTestStaticForms
  {
    public static void ShowControl<T>(T ctrl, double waitBeforeActionSeconds = .2, Action<T>? toDo = null,
      double closeAfterSeconds = .2)
      where T : Control
    {
      using var frm = new TestForm();
      frm.AddOneControl(ctrl, closeAfterSeconds * 1000);
      ShowFormAndClose(frm, waitBeforeActionSeconds, f =>
        {
          // ReSharper disable once AccessToDisposedClosure
          toDo?.Invoke(ctrl);
        },
        frm.CancellationToken);
      ctrl.Dispose();
    }

    public static async Task ShowControlAsync<T>(T ctrl, double waitBeforeActionSeconds, Func<T, Task> toDo,
      double closeAfterSeconds = .2)
      where T : Control
    {
      using var frm = new TestForm();
      frm.AddOneControl(ctrl, closeAfterSeconds * 1000);
      await ShowFormAndCloseAsync(frm, waitBeforeActionSeconds, async f => await toDo.Invoke(ctrl),
        frm.CancellationToken);
      ctrl.Dispose();
    }

    public static void ShowFormAndClose<T>(
      T typed, double waitBeforeActionSeconds = 0, Action<T>? toDo = null, in CancellationToken token = default)
      where T : Form
    {
      var frm = typed as Form;
      var isClosed = false;
      frm.FormClosed += (s, o) =>
        isClosed = true;

      frm.TopMost = true;
      frm.ShowInTaskbar = false;
      try
      {
        frm.Show();
      }
      catch (Exception)
      {
        // ignore the form might be shown already
      }

      if (waitBeforeActionSeconds > 0 && !isClosed)
        WaitSomeTime(waitBeforeActionSeconds, token);

      if (!isClosed)
        toDo?.Invoke(typed);

      if (!isClosed)
        frm.Close();

      frm.Dispose();
    }

    public static async Task ShowFormAndCloseAsync<T>(
      T frm, double waitBeforeActionSeconds = 0, Func<T, Task>? toDo = null, CancellationToken token = default)
      where T : Form
    {
      var isClosed = false;
      frm.FormClosed += (s, o) =>
        isClosed = true;

      frm.TopMost = true;
      frm.ShowInTaskbar = false;
      try
      {
        frm.Show();
      }
      catch (Exception)
      {
        // ignore the form might be shown already
      }

      if (waitBeforeActionSeconds > 0 && !isClosed)
        WaitSomeTime(waitBeforeActionSeconds, token);

      if (toDo != null && !isClosed)
        await toDo.Invoke(frm);

      if (!isClosed)
        frm.Close();

      frm.Dispose();
    }

    [DebuggerStepThrough]
    public static void WaitSomeTime(double seconds, in CancellationToken token)
    {
      if (seconds <= 0)
        return;
      var sw = new Stopwatch();
      sw.Start();
      while (sw.Elapsed.TotalSeconds < seconds && !token.IsCancellationRequested)
      {
        Application.DoEvents();
        Thread.Sleep(10);
      }
    }
  }


   public sealed class TestForm : Form
  {
    private readonly CancellationTokenSource m_CancellationTokenSource;
    private readonly Timer m_TimerAutoClose = new Timer();

    public CancellationToken CancellationToken => m_CancellationTokenSource.Token;

    public TestForm()
    {
      SuspendLayout();
      m_CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(UnitTestStatic.Token);
      AutoScaleDimensions = new SizeF(8F, 16F);
      AutoScaleMode = AutoScaleMode.Font;
      BackColor = SystemColors.Control;
      ClientSize = new Size(895, 445);
      FormBorderStyle = FormBorderStyle.SizableToolWindow;
      Name = "TestForm";
      ShowInTaskbar = false;
      StartPosition = FormStartPosition.CenterScreen;
      Text = "TestForm";
      TopMost = true;
      FormClosing += TestForm_FormClosing;
      ResumeLayout(false);
    }

    public void AddOneControl(Control ctrl, double autoCloseMilliseconds = 10000)
    {
      SuspendLayout();
      Text = ctrl.GetType().FullName;
      ctrl.Dock = DockStyle.Fill;
      ctrl.Location = new Point(0, 0);
      ctrl.Size = new Size(790, 790);
      Controls.Add(ctrl);
      ResumeLayout(false);

      if (!(autoCloseMilliseconds > 0))
        return;
      m_TimerAutoClose.Interval = autoCloseMilliseconds;
      m_TimerAutoClose.Enabled = true;
      m_TimerAutoClose.Start();
      m_TimerAutoClose.Elapsed += (sender, args) =>
      {
        if (InvokeRequired)
          BeginInvoke((MethodInvoker) Close);
        else
          Close();
      };
    }

    private void TestForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      m_TimerAutoClose.Stop();
      m_CancellationTokenSource.Cancel();
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        m_CancellationTokenSource.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}