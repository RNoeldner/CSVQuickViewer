using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass()]
  public class FormProcessDisplayTests
  {
    [TestMethod()]
    public void FormProcessDisplayTest()
    {
      Assert.IsNotNull(new FormProcessDisplay());
    }

    [TestMethod()]
    public void FormProcessDisplayTest1()
    {
      var tokenSrc = new CancellationTokenSource();

      using (var frm = new FormProcessDisplay("Title", tokenSrc.Token))
      {
        Assert.AreEqual("Title", frm.Title);
        Assert.AreEqual(false, frm.CancellationTokenSource.IsCancellationRequested);
        tokenSrc.Cancel();
        Assert.AreEqual(true, frm.CancellationTokenSource.IsCancellationRequested);
      }
    }

    [TestMethod()]
    public void CancelTest()
    {
      var tokenSrc = new CancellationTokenSource();

      using (var frm = new FormProcessDisplay("Title", tokenSrc.Token))
      {
        Assert.AreEqual(false, frm.CancellationTokenSource.IsCancellationRequested);
        frm.Cancel();
        Assert.AreEqual(true, frm.CancellationTokenSource.IsCancellationRequested);
        Assert.AreEqual(false, tokenSrc.IsCancellationRequested);
      }
    }

    [TestMethod()]
    public void SetProcessTest()
    {
      using (var frm = new FormProcessDisplay())
      {
        frm.Show();
        frm.SetProcess("Hello World");
      }
    }

    [TestMethod()]
    public void SetProcessTest1()
    {
      using (var frm = new FormProcessDisplay())
      {
        frm.Maximum = -1;
        frm.Show();
        frm.SetProcess(this, new ProgressEventArgs("Hello", 10));
      }
    }

    [TestMethod()]
    public void DoHideTest()
    {
      using (var frm = new FormProcessDisplay())
      {
        frm.Show();
        frm.DoHide(this, new EventArgs());
      }
    }

    [TestMethod()]
    public void SetProcessTest2()
    {
      using (var frm = new FormProcessDisplay())
      {
        frm.Maximum = 80;

        frm.Show();
        int called = 10;
        frm.Progress += delegate (object sender, ProgressEventArgs e)
        {
          called = e.Value;
        };
        frm.SetProcess("Help", 20);

        Assert.AreEqual(20, called);
      }
    }
  }
}