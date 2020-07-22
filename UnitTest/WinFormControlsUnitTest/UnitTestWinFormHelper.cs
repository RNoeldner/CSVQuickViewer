using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public static class UnitTestWinFormHelper
  {
    [DebuggerStepThrough]
    public static void WaitSomeTime(double seconds)
    {
      var sw = new Stopwatch();
      sw.Start();
      while (sw.Elapsed.TotalSeconds < seconds)
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
      WaitSomeTime(.3);
      RunTaskTimeout(toDo, timeout);
      frm.Close();
    }

    public static void RunSTAThread(Action action)
    {
      if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
        action.Invoke();
      else
      {
        var runThread = new Thread(action.Invoke);
        runThread.SetApartmentState(ApartmentState.STA);
        runThread.Start();
        runThread.Join(20000);
      }
    }

    public static void ShowFormAndClose(Form frm, double time = .2, Action toDo = null)
    {
      frm.TopMost = true;
      frm.ShowInTaskbar = false;
      frm.Show();
      frm.Focus();
      if (time > 0)
        WaitSomeTime(time);

      if (toDo != null)
      {
        toDo.Invoke();
        if (time > 0)
          WaitSomeTime(time);
      }

      frm.Close();
    }


    public static void ShowControl(Control ctrl, double time = .2, Action toDo = null)
    {
      using (var frm = new TestForm())
      {
        frm.AddOneControl(ctrl);
        ShowFormAndClose(frm, time, toDo);
      }
    }
  }
}