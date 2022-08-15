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
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvTools.Tests
{
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
    [Timeout(3000)]
    public void GetprogressTest()
    {
      var setting = new CsvFile { FileName = "Folder\\This is a long file name that should be cut and fit into 80 chars.txt", ShowProgress = true };
      using (var prc = setting.GetProgress(null, true, UnitTestStatic.Token))
      {
        Assert.IsNotNull(prc, "Getprogress With Logger");
      }

      using (var prc = setting.GetProgress(null, false, UnitTestStatic.Token))
      {
        Assert.IsNotNull(prc, "Getprogress Without Logger");
      }

      using var frm = new Form();
      frm.Text = "Testing...";
      frm.Show();
      var csv = new CsvFile() { ShowProgress = true };
      Assert.IsInstanceOfType(csv.GetProgress(frm, true, UnitTestStatic.Token), typeof(FormProgress));
      csv.ShowProgress = false;
      Assert.IsNotInstanceOfType(csv.GetProgress(frm, true, UnitTestStatic.Token), typeof(FormProgress));
    }

    [TestMethod]
    [Timeout(3000)]
    public void GetprogressTestNoShow()
    {
      var setting2 = new CsvFile { FileName = "Folder\\This is a long file name that should be cut and fit into 80 chars.txt", ShowProgress = false };
      using var prc = setting2.GetProgress(null, false, UnitTestStatic.Token);
      Assert.IsNull(prc, "Getprogress without UI");
    }

    [TestMethod]
    [Timeout(2000)]
    public void LoadWindowStateTest()
    {
      using var formProgress = new FormProgress();
      formProgress.Show();
      var state = new WindowState(new Rectangle(10, 10, 200, 200), FormWindowState.Normal) { CustomInt = 27, CustomText = "Test" };
      var result1 = -1;
      var result2 = "Hello";
      formProgress.LoadWindowState(state, val => { result1 = val; }, val => { result2 = val; });
      Assert.AreEqual(state.CustomInt, result1);
      Assert.AreEqual(state.CustomText, result2);
    }

    [TestMethod]
    [Timeout(2000)]
    public async Task RunWithHourglassAsyncTest()
    {
      using var ctrl = new ToolStripButton();
      var done = false;
      await ctrl.RunWithHourglassAsync(async () => await Task.Run(() => done = true), null);
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
    [Timeout(4000)]
    public void ShowError()
    {
      using var frm = new Form();
      frm.Text = "Testing...";
      frm.Show();
      frm.ShowError(new Exception(), "Text", 1.0);
    }

    [TestMethod]
    [Timeout(2000)]
    public void StoreWindowStateTest()
    {
      using var formProgress = new FormProgress();
      formProgress.Show();
      var state1 = new WindowState(new Rectangle(10, 10, formProgress.Width, formProgress.Height),
        FormWindowState.Normal)
      { CustomInt = 27, CustomText = "Test" };
      var result1 = -1;
      formProgress.LoadWindowState(state1, val => { result1 = val; }, val => { });

      var state2 = formProgress.StoreWindowState(result1, "World");
      // Assert.AreEqual(state1.CustomText, state2.CustomText);
      Assert.AreEqual(state1.CustomInt, state2.CustomInt, "CustomInt");
      Assert.AreEqual("World", state2.CustomText, "CustomText");
      //Assert.AreEqual(state1.Left, state2.Left, "Left");
      Assert.AreEqual(state1.Width, state2.Width, "Width");
    }

    [TestMethod]
    [Timeout(2000)]
    public void WriteBindingTest()
    {
      var obj = new DisplayItem<string>("15", "Text");
      using var bindingSource = new BindingSource { DataSource = obj };
      var bind = new Binding("Text", bindingSource, "ID", true);
      using var textBoxBox = new TextBox();
      textBoxBox.DataBindings.Add(bind);
      textBoxBox.Text = "12";

      Assert.AreEqual(bind, textBoxBox.GetTextBinding());
      textBoxBox.WriteBinding();
    }
  }
}