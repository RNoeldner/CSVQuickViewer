/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvTools.Tests;

[TestClass]
public class ExtensionsTests
{
  [TestMethod]
  [Timeout(2000)]
  public void CtrlATes()
  {
    using var frm = new Form();
    frm.Text = "Testing...";
    frm.Show();

    using (TextBox tb = new TextBox())
    {
      frm.Controls.Add(tb);
      tb.Text = "Some Text";
      frm.CtrlA(tb, new KeyEventArgs(Keys.Control | Keys.A));
      Assert.AreEqual("Some Text", tb.SelectedText);
      frm.Controls.Remove(tb);
    }

    using ListView lv = new ListView();
    frm.Controls.Add(lv);
    frm.CtrlA(lv, new KeyEventArgs(Keys.Control | Keys.A));
  }
       

  [TestMethod]
  [Timeout(2000)]
  public void LoadWindowStateTest()
  {
    Extensions.RunStaThread(() =>
    {
      using var formProgress = new FormProgress();
      formProgress.Show();
      var state = new WindowState(10, 10, 200, 200, FormWindowState.Normal, 27, "Test");
      var result1 = -1;
      var result2 = "Hello";
      formProgress.LoadWindowState(state, val => { result1 = val; }, val => { result2 = val; });
      Assert.AreEqual(state.CustomInt, result1);
      Assert.AreEqual(state.CustomText, result2);
    });
  }

  [TestMethod]
  [Timeout(2000)]
  public void RunWithHourglassAsyncTest()
  {
    var done = false;
    UnitTestStaticForms.ShowControl(() => new Button(), .1, async c => await c.RunWithHourglassAsync(async () => await Task.Run(() => done = true)));
    Assert.IsTrue(done);
  }

  [TestMethod]
  [Timeout(2000)]
  public void RunWithHourglassTest()
  {
    using var ctrl = new ToolStripButton();
    var done = false;
    ctrl.RunWithHourglass(() => done = true, null);
    Assert.IsTrue(done);
  }


  [TestMethod]
  [Timeout(2000)]
  public void ShowError()
  {
    CancellationTokenSource src = new CancellationTokenSource(TimeSpan.FromSeconds(1));
    Task.Run(()=> Extensions.RunStaThread(() =>
    {
      using var frm = new Form();
      frm.Text = "Testing...";
      frm.Show();
      frm.ShowError(new Exception("Exception Message"), "Text", 0.1);
      frm.Close();
    }), src.Token);
  }

  [TestMethod]
  [Timeout(2000)]
  public void StoreWindowStateTest()
  {
    Extensions.RunStaThread(() =>
    {
      using var formProgress = new FormProgress();
      formProgress.Show();
      var state1 = new WindowState(10, 10, formProgress.Width, formProgress.Height, FormWindowState.Normal, 27, "Test");
      var result1 = -1;
      formProgress.LoadWindowState(state1, val => { result1 = val; }, _ => { });

      var state2 = formProgress.StoreWindowState(result1, "World");
      // Assert.AreEqual(state1.CustomText, state2.CustomText);
      Assert.AreEqual(state1.CustomInt, state2.CustomInt, "CustomInt");
      Assert.AreEqual("World", state2.CustomText, "CustomText");
      //Assert.AreEqual(state1.Left, state2.Left, "Left");
      // Assert.AreEqual(state1.Width, state2.Width, "Width");
    });


  }
}