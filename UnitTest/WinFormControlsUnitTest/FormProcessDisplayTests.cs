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
  public class FormProcessDisplayTests
  {
    [TestMethod]
    public void FormProcessCancel()
    {
      using (var frm = new FormProcessDisplay("Test Logger", true, UnitTestInitializeCsv.Token))
      {
        frm.ShowInTaskbar = true;
        frm.Show();
        UnitTestWinFormHelper.WaitSomeTime(.2, UnitTestInitializeCsv.Token);
        frm.Close();
      }
    }

    [TestMethod]
    [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
    public void FormProcessDisplay()
    {
      // Log
      using (var frm = new FormProcessDisplay("Test Logger", true, UnitTestInitializeCsv.Token))
      {
        frm.ShowInTaskbar = false;
        frm.Show();
        frm.Maximum = 100;
        var sentTime = new TimeSpan(0);
        frm.ProgressTime += (sender, time) => { sentTime = time.EstimatedTimeRemaining; };
        var end = 50;
        var step = 5;
        var wait = .1;
        for (var c = 0; c < end && !frm.CancellationToken.IsCancellationRequested; c += step)
        {
          frm.SetProcess($"This is a text\nLine {c}", c, true);
          UnitTestWinFormHelper.WaitSomeTime(wait, UnitTestInitializeCsv.Token);
        }

        // Left should be roughly .1 * 50 = 5 seconds  
        Assert.IsTrue(
          (wait * (end / step)) - .5 < sentTime.TotalSeconds && sentTime.TotalSeconds < (wait * (end / step)) + .5,
          $"Estimated time should be roughly {wait * (end / step)}s but is {sentTime.TotalSeconds}");
        frm.Close();
      }

      // marquee
      using (var frm = new FormProcessDisplay("Test Marquee", false, UnitTestInitializeCsv.Token))
      {
        frm.ShowInTaskbar = false;
        frm.Show();
        frm.Maximum = 0;
        for (var c = 0; c < 100 && !frm.CancellationToken.IsCancellationRequested; c += 5)
        {
          frm.SetProcess($"This is a text\nLine {c}", c, true);
          UnitTestWinFormHelper.WaitSomeTime(.1, frm.CancellationToken);
        }

        frm.Close();
      }

      // NoLog
      using (var frm = new FormProcessDisplay("Test", false, UnitTestInitializeCsv.Token))
      {
        frm.ShowInTaskbar = false;
        frm.Show();
        frm.Maximum = 100;
        for (var c = 0; c < 102 && !frm.CancellationToken.IsCancellationRequested; c += 4)
        {
          frm.SetProcess($"This is a text\nLine {c}", c, true);
          UnitTestWinFormHelper.WaitSomeTime(.1, frm.CancellationToken);
        }

        frm.Close();
      }
    }

    [TestMethod]
    public void FormProcessDisplayTest()
    {
      using (var value = new FormProcessDisplay())
      {
        Assert.IsNotNull(value);
      }
    }

    [TestMethod]
    public void FormProcessDisplayTest1()
    {
      using (var tokenSrc = new CancellationTokenSource())
      {
        using (var frm = new FormProcessDisplay("Title", false, tokenSrc.Token))
        {
          Assert.AreEqual("Title", frm.Title);
          Assert.AreEqual(false, frm.CancellationToken.IsCancellationRequested);
          tokenSrc.Cancel();
          Assert.AreEqual(true, frm.CancellationToken.IsCancellationRequested);
        }
      }
    }

    [TestMethod]
    public void CancelTest()
    {
      using (var tokenSrc = new CancellationTokenSource())
      {
        using (var frm = new FormProcessDisplay("Title", true, tokenSrc.Token))
        {
          Assert.AreEqual(false, frm.CancellationToken.IsCancellationRequested);
          frm.Close();
          Assert.AreEqual(true, frm.CancellationToken.IsCancellationRequested);
          Assert.AreEqual(false, tokenSrc.IsCancellationRequested);
        }
      }
    }

    [TestMethod]
    public void SetProcessTest()
    {
      using (var frm = new FormProcessDisplay())
      {
        frm.Show();
        frm.SetProcess("Hello World");
      }
    }

    [TestMethod]
    public void SetProcessTest1()
    {
      using (var frm = new FormProcessDisplay())
      {
        frm.Maximum = -1;
        frm.Show();
        frm.SetProcess(this, new ProgressEventArgs("Hello", 10));
      }
    }


    [TestMethod]
    public void DoHideTest()
    {
      using (var frm = new FormProcessDisplay())
      {
        frm.Show();
        frm.DoHide(this, new EventArgs());
      }
    }

    [TestMethod]
    public void SetProcessTest2()
    {
      using (var frm = new FormProcessDisplay())
      {
        frm.Maximum = 80;

        frm.Show();
        long called = 10;

        frm.Progress += delegate(object sender, ProgressEventArgs e) { called = e.Value; };

        frm.SetProcess("Help", 20, true);

        Assert.AreEqual(20, called);
      }
    }
  }
}