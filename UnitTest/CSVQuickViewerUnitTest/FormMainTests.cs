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

using System.Drawing;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class FormMainTests
  {
    //public void GetButtons(Control rootControl, List<Component> btns)
    //{
    //  foreach (Control ctrl in rootControl.Controls)
    //  {
    //    if (ctrl is Button)
    //      btns.Add(ctrl);
    //    else if (ctrl is ToolStrip ts)
    //    {
    //      foreach (ToolStripItem i in ts.Items)
    //      {
    //        if (i is ToolStripButton)
    //          btns.Add(i);
    //      }
    //    }
    //    else if (ctrl.HasChildren)
    //      GetButtons(ctrl, btns);
    //  }
    //}

    //[TestMethod]
    //public void CheckEvents()
    //{
    //  Extensions.RunSTAThread(() =>
    //  {
    //    using (var frm = new FormMain(Path.Combine(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt.gz"))))
    //    {
    //      Thread.Sleep(500);
    //      //UnitTestWinFormHelper.WaitSomeTime(2, UnitTestInitializeCsv.Token);
    //      var btns = new List<Component>();
    //      GetButtons(frm, btns);

    //      foreach (var btn in btns)
    //      {
    //        var events = (EventHandlerList) typeof(Component).GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(btn, null);
    //        // check OnClick Event
    //        var field = btn.GetType().GetField("EventClick", BindingFlags.NonPublic | BindingFlags.Static);
    //        if (field != null)
    //        {
    //          var clickhandlers = events[field.GetValue(btn)];
    //          Assert.IsTrue(clickhandlers.GetInvocationList().Count()>0);
    //        }
    //      }
    //    }
    //  });
    //}

    [TestMethod]
    [Timeout(20000)]
    public async System.Threading.Tasks.Task FormMain_BasicCSVAsync()
    {
      using (var frm = new FormMain(new ViewSettings()))
      {
        frm.Size = new Size(800, 600);
        await UnitTestWinFormHelper.ShowFormAndCloseAsync(frm, .2, frm.LoadCsvFile(Path.Combine(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt.gz"))));

        Assert.IsNotNull(frm.DataTable);
        Assert.AreEqual(7, frm.DataTable.Rows.Count);
      }
    }

    [TestMethod]
    //[Timeout(20000)]
    public async System.Threading.Tasks.Task FormMain_AllFormatsPipeAsync()
    {
      using (var frm = new FormMain(new ViewSettings()))
      {
        await UnitTestWinFormHelper.ShowFormAndCloseAsync(frm, .1, frm.LoadCsvFile(UnitTestInitializeCsv.GetTestPath("AllFormatsPipe.txt")));
        Assert.IsNotNull(frm.DataTable);
        // 45 records, one of the lines has a linefeed
        Assert.IsTrue(frm.DataTable.Rows.Count>=46);
      }
    }
  }
}