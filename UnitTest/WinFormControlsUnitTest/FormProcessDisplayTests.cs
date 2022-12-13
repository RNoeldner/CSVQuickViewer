/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass]
  public class FormProgressTests
  {
    [TestMethod]
    [Timeout(3000)]
    public void FormProcessCancel()
    {
      using var formProgress = new FormProgress("Test Logger", true, UnitTestStatic.Token);
      formProgress.ShowInTaskbar = true;
      formProgress.Show();
      UnitTestStatic.WaitSomeTime(.2, UnitTestStatic.Token);
      formProgress.Close();
    }

    [TestMethod]
    [Timeout(20000)]
    [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
    public void FormProgress()
    {
      // Log
      using (var formProgress = new FormProgress("Test Logger", true, UnitTestStatic.Token))
      {
        formProgress.ShowInTaskbar = false;
        formProgress.Show();
        formProgress.Maximum = 100;
        var sentTime = new TimeSpan(0);
        formProgress.ProgressChanged += (obj, _) =>
        {
          sentTime = ((IProgressTime) obj).TimeToCompletion.EstimatedTimeRemaining;
        };
        var end = 50;
        var step = 5;
        var wait = .1;
        for (var c = 0; c < end && !formProgress.CancellationToken.IsCancellationRequested; c += step)
        {
          formProgress.Report(new ProgressInfo($"This is a text\nLine {c}", c));
          UnitTestStatic.WaitSomeTime(wait, UnitTestStatic.Token);
        }

        // Left should be roughly .1 * 50 = 5 seconds  
        Assert.IsTrue(
          (wait * (end / step)) - .5 < sentTime.TotalSeconds && sentTime.TotalSeconds < (wait * (end / step)) + .5,
          $"Estimated time should be roughly {wait * (end / step)}s but is {sentTime.TotalSeconds}");
        formProgress.Close();
      }

      // marquee
      using (var formProgress = new FormProgress("Test Marquee", false, UnitTestStatic.Token))
      {
        formProgress.ShowInTaskbar = false;
        formProgress.Show();
        formProgress.Maximum = 0;
        for (var c = 0; c < 100 && !formProgress.CancellationToken.IsCancellationRequested; c += 5)
        {
          formProgress.Report(new ProgressInfo($"This is a text\nLine {c}", c));
          UnitTestStatic.WaitSomeTime(.1, formProgress.CancellationToken);
        }

        formProgress.Close();
      }

      // NoLog
      using (var frm = new FormProgress("Test", false, UnitTestStatic.Token))
      {
        frm.ShowInTaskbar = false;
        frm.Show();
        frm.Maximum = 100;
        for (var c = 0; c < 102 && !frm.CancellationToken.IsCancellationRequested; c += 4)
        {
          frm.Report(new ProgressInfo($"This is a text\nLine {c}", c));
          UnitTestStatic.WaitSomeTime(.1, frm.CancellationToken);
        }

        frm.Close();
      }
    }

    [TestMethod]
    [Timeout(2000)]
    public void FormprogressTest()
    {
      using var formProgress = new FormProgress();
      Assert.IsNotNull(formProgress);
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormprogressTest1()
    {
      using var tokenSrc = new CancellationTokenSource();
      using var formProgress = new FormProgress("Title", false, tokenSrc.Token);
      Assert.AreEqual("Title", formProgress.Text);
      Assert.AreEqual(false, formProgress.CancellationToken.IsCancellationRequested);
      tokenSrc.Cancel();
      Assert.AreEqual(true, formProgress.CancellationToken.IsCancellationRequested);
    }

    [TestMethod]
    [Timeout(5000)]
    public void CancelTest()
    {
      using var tokenSrc = new CancellationTokenSource();
      using var formProgress = new FormProgress("Title", true, tokenSrc.Token);
      Assert.AreEqual(false, formProgress.CancellationToken.IsCancellationRequested);
      formProgress.Close();
      Assert.AreEqual(true, formProgress.CancellationToken.IsCancellationRequested);
      Assert.AreEqual(false, tokenSrc.IsCancellationRequested);
    }

    [TestMethod]
    [Timeout(2000)]
    public void SetProcessTest()
    {
      using var formProgress = new FormProgress();
      formProgress.Show();
      formProgress.SetProcess("Hello World");
    }

    [TestMethod]
    [Timeout(2000)]
    public void SetProcessTest2()
    {
      using var formProgress = new FormProgress();
      formProgress.Maximum = 80;

      formProgress.Show();
      long called = 10;

      formProgress.ProgressChanged += (_, e) => { called = e.Value; };

      formProgress.Report(new ProgressInfo("Help", 20));

      Assert.AreEqual(20, called);
    }
  }
}