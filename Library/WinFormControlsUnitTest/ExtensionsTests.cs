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

using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ExtensionsTests
  {
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