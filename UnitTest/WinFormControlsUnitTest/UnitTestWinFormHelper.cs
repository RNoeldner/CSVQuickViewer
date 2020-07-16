using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
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