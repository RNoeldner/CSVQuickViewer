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
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ExtensionsTests
  {
    [TestMethod]
    public void WaitToCompleteTaskTestCompletedAlready()
    {
      var executed = false;

      var task = System.Threading.Tasks.Task.Factory.StartNew(() => { executed = true; });
      task.Wait();
      Extensions.WaitToCompleteTask(task, 1, true, CancellationToken.None);
      Assert.IsTrue(executed);
    }

    [TestMethod]
    public void WaitToCompleteTaskTestCompleteInTime()
    {
      var executed = false;
      Assert.IsFalse(executed);
      var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
      {
        Thread.Sleep(100);
        executed = true;
      });
      Assert.IsFalse(executed);
      Extensions.WaitToCompleteTask(task, 1, true, CancellationToken.None);
      Assert.IsTrue(executed);
    }

    [TestMethod]
    public void WaitToCompleteTaskTestTimeout()
    {
      var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
      {
        Thread.Sleep(2000);
      });

      try
      {
        Extensions.WaitToCompleteTask(task, 1, true, CancellationToken.None);
        Assert.Fail("Timeout did not occur");
      }
      catch (TimeoutException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail($"Wrong exception got {ex.GetType().Name} expected TimeoutException");
      }
    }

    [TestMethod]
    public void WaitToCompleteTaskCancel()
    {
      using (var cts = new CancellationTokenSource())
      {
        var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
        {
          Thread.Sleep(3000);
        });

        try
        {
          // Cancel Token after 200 ms in other thread
          var task2 = System.Threading.Tasks.Task.Factory.StartNew(() =>
          {
            Thread.Sleep(500);
            cts.Cancel();
          });
          Extensions.WaitToCompleteTask(task, 1.5d, true, cts.Token);
          Assert.Fail("Timeout did not occur");
        }
        catch (AssertFailedException)
        {
          throw;
        }
        catch (OperationCanceledException)
        { }
        catch (Exception ex)
        {
          Assert.Fail($"Wrong exception got {ex.GetType().Name} expected OperationCanceledException");
        }
      }
    }

    [TestMethod]
    public void UpdateListViewColumnFormatTest()
    {
      using (var lv = new ListView())
      {
        var colFmt = new List<Column>();

        {
          var item = lv.Items.Add("Test");
          item.Selected = true;
        }
        lv.UpdateListViewColumnFormat(colFmt);
        Assert.AreEqual(0, lv.Items.Count);

        {
          lv.Items.Add("Test1");
          var item = lv.Items.Add("Test");
          item.Selected = true;
        }

        colFmt.Add(new Column { Name = "Test" });
        lv.UpdateListViewColumnFormat(colFmt);
      }
    }

    [TestMethod]
    public void RunWithTimeout()
    {
      var executed = false;
      Assert.IsFalse(executed);
      Extensions.RunWithTimeout((() => executed = true), 1, CancellationToken.None);
      Assert.IsTrue(executed);
    }

    [TestMethod]
    public void RunWithTimeoutErrorIsDefault()
    {
      var result = Extensions.RunWithTimeout<bool>(() =>
        {
          throw new ArgumentException("My Exception");          
        }, 1, CancellationToken.None);
      Assert.AreEqual(default(bool), result);
    }

    [TestMethod()]
    public void WriteBindingTest()
    {
      var obj = new DisplayItem<string>("15", "Text");
      using (var bindrc = new BindingSource
      {
        DataSource = obj
      })
      {
        var bind = new Binding("Text", bindrc, "ID", true);
        using (var crtl = new TextBox())
        {
          crtl.DataBindings.Add(bind);
          crtl.Text = "12";

          Assert.AreEqual(bind, crtl.GetTextBindng());
          crtl.WriteBinding();
        }
      }
    }

    [TestMethod()]
    public void DeleteFileQuestionTest() => Assert.AreEqual(true, ".\\Test.hshsh".DeleteFileQuestion(false));

    [TestMethod()]
    public void GetProcessDisplayTest()
    {
      var setting = new CsvFile()
      {
        FileName = "Folder\\This is a long file name that should be cut and fit into 80 chars.txt",
        ShowProgress = true
      };
      using (var prc = setting.GetProcessDisplay(null, true, System.Threading.CancellationToken.None))
      {
        Assert.IsTrue(prc is IProcessDisplay, "GetProcessDisplay With Logger");
      }
      using (var prc = setting.GetProcessDisplay(null, false, System.Threading.CancellationToken.None))
      {
        Assert.IsTrue(prc is IProcessDisplay, "GetProcessDisplay Without Logger");
      }

      var setting2 = new CsvFile()
      {
        FileName = "Folder\\This is a long file name that should be cut and fit into 80 chars.txt",
        ShowProgress = false
      };

      using (var prc = setting2.GetProcessDisplay(null, false, System.Threading.CancellationToken.None))
      {
        Assert.IsTrue(prc is IProcessDisplay, "GetProcessDisplay without UI");
      }
    }

    [TestMethod()]
    public void LoadWindowStateTest()
    {
      using (var value = new FormProcessDisplay())
      {
        value.Show();
        var state = new WindowState(new System.Drawing.Rectangle(10, 10, 200, 200), FormWindowState.Normal)
        {
          CustomInt = 27,
          CustomText = "Test"
        };
        var resul1 = -1;
        var result2 = "Hello";
        Extensions.LoadWindowState(value, state, (val) => { resul1 = val; }, (val) => { result2 = val; });
        Assert.AreEqual(state.CustomInt, resul1);
        Assert.AreEqual(state.CustomText, result2);
      }
    }

    [TestMethod()]
    public void SafeBeginInvokeTest()
    {
    }

    [TestMethod()]
    public void SafeInvokeTest()
    {
    }

    [TestMethod()]
    public void SafeInvokeNoHandleNeededTest()
    {
    }

    [TestMethod()]
    public void StoreWindowStateTest()
    {
      using (var value = new FormProcessDisplay())
      {
        value.Show();
        var state1 = new WindowState(new System.Drawing.Rectangle(10, 10, 400, 400), FormWindowState.Normal)
        {
          CustomInt = 27,
          CustomText = "Test"
        };
        var resul1 = -1;
        var result2 = "Hello";
        Extensions.LoadWindowState(value, state1, (val) => { resul1 = val; }, (val) => { result2 = val; });

        var state2 = Extensions.StoreWindowState(value, resul1, "World");
        Assert.AreEqual(state1.CustomInt, state2.CustomInt);
        Assert.AreEqual("World", state2.CustomText);
        Assert.AreEqual(state1.Left, state2.Left);
        Assert.AreEqual(state1.Width, state2.Width);
      }
    }

    [TestMethod()]
    public void UpdateListViewColumnFormatTest1()
    {
    }

    [TestMethod()]
    public void WriteFileWithInfoTest()
    {
    }
  }
}