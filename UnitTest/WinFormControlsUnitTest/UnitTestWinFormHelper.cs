using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Control = System.Windows.Forms.Control;

namespace CsvTools.Tests
{
  [TestClass]
  public static class UnitTestWinFormHelper
  {
    [DebuggerStepThrough]
    public static void WaitSomeTime(double seconds, CancellationToken token)
    {
      var sw = new Stopwatch();
      sw.Start();
      while (sw.Elapsed.TotalSeconds < seconds && !token.IsCancellationRequested)
      {
        Application.DoEvents();
        FunctionalDI.SignalBackground?.Invoke();
        Thread.Sleep(10);
      }
    }

    public static void RunTaskTimeout(Func<CancellationToken, Task> toDo, double timeout = 1)
    {
      using (var source = CancellationTokenSource.CreateLinkedTokenSource(UnitTestInitializeCsv.Token))
      {
        Task.WhenAny(toDo.Invoke(source.Token), Task.Delay(TimeSpan.FromSeconds(timeout), source.Token));
        source.Cancel();
      }
    }

    public static void ShowFormAndClose(Form frm, double timeout, [NotNull] Func<CancellationToken, Task> toDo)
    {
      frm.TopMost = true;
      frm.ShowInTaskbar = false;
      frm.Show();
      frm.Focus();
      WaitSomeTime(.5, UnitTestInitializeCsv.Token);
      RunTaskTimeout(toDo, timeout);
      WaitSomeTime(.5, UnitTestInitializeCsv.Token);
      frm.Close();
    }

    public static async Task ShowFormAndCloseAsync(Form frm, double time, [NotNull] Task toDo)
    {
      frm.TopMost = true;
      frm.ShowInTaskbar = false;
      frm.Show();
      frm.Focus();
      WaitSomeTime(time, UnitTestInitializeCsv.Token);
      await toDo;
      WaitSomeTime(time, UnitTestInitializeCsv.Token);
      frm.Close();
    }

    public static void ShowFormAndClose(Form frm, double before = .2, Action toDo = null)
    => ShowFormAndClose(frm, before, toDo, before, UnitTestInitializeCsv.Token);

    private static void ShowFormAndClose(Form frm, double before, Action toDo, double after, CancellationToken token)
    {
      frm.TopMost = true;
      frm.ShowInTaskbar = false;
      frm.Show();
      frm.Focus();
      if (before > 0)
        WaitSomeTime(before, token);

      if (toDo != null)
      {
        toDo.Invoke();
        if (after > 0)
          WaitSomeTime(after, token);
      }

      frm.Close();
    }

    public static void ShowControl(Control ctrl, double before = .2, Action toDo = null, double after = .2)
    {
      using (var cts = CancellationTokenSource.CreateLinkedTokenSource(UnitTestInitializeCsv.Token))
      {
        using (var frm = new TestForm())
        {
          frm.Closing += (s, e) => cts.Cancel();
          frm.AddOneControl(ctrl);
          ShowFormAndClose(frm, before, toDo, after, cts.Token);
        }
      }
    }
  }
}