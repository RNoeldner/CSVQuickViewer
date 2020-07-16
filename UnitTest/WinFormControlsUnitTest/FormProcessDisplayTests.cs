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

using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        UnitTestWinFormHelper.WaitSomeTime(.2);
        frm.Close();
      }
    }

    [TestMethod]
    public void FormProcessDisplay()
    {
      // Log
      using (var frm = new FormProcessDisplay("Test Logger", true, UnitTestInitializeCsv.Token))
      {
        frm.ShowInTaskbar = false;
        frm.Show();
        frm.Maximum = 100;
        for (var c = 0; c < 70 && !frm.CancellationToken.IsCancellationRequested; c += 5)
        {
          frm.SetProcess($"This is a text\nLine {c}", c, true);
          UnitTestWinFormHelper.WaitSomeTime(.1);
        }
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
          UnitTestWinFormHelper.WaitSomeTime(.1);
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
          UnitTestWinFormHelper.WaitSomeTime(.1);
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
          Assert.AreEqual(false, frm.CancellationTokenSource.IsCancellationRequested);
          tokenSrc.Cancel();
          Assert.AreEqual(true, frm.CancellationTokenSource.IsCancellationRequested);
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
          Assert.AreEqual(false, frm.CancellationTokenSource.IsCancellationRequested);
          frm.Close();
          Assert.AreEqual(true, frm.CancellationTokenSource.IsCancellationRequested);
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
        frm.Progress += delegate (object sender, ProgressEventArgs e) { called = e.Value; };
        frm.SetProcess("Help", 20, true);

        Assert.AreEqual(20, called);
      }
    }
  }
}