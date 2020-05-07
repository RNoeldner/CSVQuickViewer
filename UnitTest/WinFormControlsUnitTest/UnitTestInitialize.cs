using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public static class UnitTestInitialize
  {
    private static readonly string m_ApplicationDirectory = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles";
    
    public static void WaitSomeTime(double seconds)
    {
      var sw = new Stopwatch();
      sw.Start();
      while (sw.Elapsed.TotalSeconds < seconds)
      {
        FunctionalDI.SignalBackground?.Invoke();
        Thread.Sleep(50);
      }
    }

    public static void ShowFormAndClose(Form frm, double time = .2, Action toDo = null)
    {
      frm.TopMost = true;
      frm.ShowInTaskbar = false;
      frm.Show();
      frm.Focus();
      if (time > 0)
        UnitTestInitialize.WaitSomeTime(time);

      if (toDo != null)
      {
        toDo.Invoke();
        if (time > 0)
          UnitTestInitialize.WaitSomeTime(time);
      }

      frm.Close();
    }

    public static void ShowControl(Control ctrl, double time = .2, Action toDo = null)
    {
      using (var frm = new Form())
      {
        frm.SuspendLayout();
        frm.Text = ctrl.GetType().FullName;
        frm.BackColor = SystemColors.Control;
        frm.ClientSize = new Size(800, 800);

        frm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
        frm.StartPosition = FormStartPosition.CenterScreen;

        ctrl.Dock = DockStyle.Fill;
        ctrl.Location = new Point(0, 0);
        ctrl.Size = new Size(600, 600);
        frm.Controls.Add(ctrl);

        frm.ResumeLayout(false);

        ShowFormAndClose(frm, time, toDo);
      }
    }

    public static string GetTestPath(string fileName) =>
      Path.Combine(m_ApplicationDirectory, fileName.TrimStart(' ', '\\', '/'));

    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;

      FunctionalDI.SignalBackground = Application.DoEvents;


      Contract.ContractFailed += Contract_ContractFailed;
    }


    private static void Contract_ContractFailed(object sender, ContractFailedEventArgs e) => e.SetHandled();
  }
}