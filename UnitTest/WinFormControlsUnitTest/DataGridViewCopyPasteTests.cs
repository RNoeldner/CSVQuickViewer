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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass]
  public class DataGridViewCopyPasteTests
  {
    [TestMethod]
    [Timeout(2000)]
    public void SelectedDataIntoClipboardAllTest()
    {
      Extensions.RunSTAThread(() =>
      {
        using (var dgv = new DataGridView())
        {
          dgv.AutoGenerateColumns = true;
          using (var dt = UnitTestStatic.GetDataTable())
          {
            dgv.DataSource = dt;
            using (var frm = new Form())
            {
              frm.Controls.Add(dgv);
              frm.Show();
              dgv.SelectAll();
              dgv.SelectedDataIntoClipboard(true, false, UnitTestInitializeCsv.Token);
            }
          }
        }
      });
    }

    [TestMethod]
    [Timeout(3000)]
    public void CopySelectedRowsIntoClipboard()
    {
      Extensions.RunSTAThread(() =>
      {
        using (var dgv = new DataGridView())
        {
          dgv.AutoGenerateColumns = true;
          using (var dt = UnitTestStatic.GetDataTable())
          {
            dgv.DataSource = dt;
            using (var frm = new Form())
            {
              frm.Controls.Add(dgv);
              frm.Show();
              dgv.Rows[1].Selected = true;
              dgv.Rows[2].Selected = true;
              dgv.Rows[3].Selected = true;
              Clipboard.Clear();
              try
              {
                dgv.SelectedDataIntoClipboard(false, true, UnitTestInitializeCsv.Token);

                var dataObject = Clipboard.GetDataObject();
                Assert.IsNotNull(dataObject);
                Assert.IsNotNull(dataObject.GetData(DataFormats.Html));
                Assert.IsNotNull(dataObject.GetData(DataFormats.Text));
              }
              catch (ExternalException)
              {
                Assert.Inconclusive("ExternalException while copying data to Clipboard");
              }
            }
          }
        }
      });
    }

    [TestMethod]
    [Timeout(2000)]
    public void CopySelectedColumnsIntoClipboard()
    {
      Extensions.RunSTAThread(() =>
      {
        using (var dgv = new DataGridView())
        {
          dgv.AutoGenerateColumns = true;
          using (var dt = UnitTestStatic.GetDataTable())
          {
            dgv.DataSource = dt;
            using (var frm = new Form())
            {
              frm.Controls.Add(dgv);
              frm.Show();
              dgv.Columns[1].Selected = true;
              dgv.Columns[2].Selected = true;
              try
              {
                Clipboard.Clear();
                dgv.SelectedDataIntoClipboard(true, false, UnitTestInitializeCsv.Token);
                var dataObject = Clipboard.GetDataObject();
                Assert.IsNotNull(dataObject);
                Assert.IsNotNull(dataObject.GetData(DataFormats.Text));
              }
              catch (ExternalException e)
              {
                Console.WriteLine(e);
                Assert.Inconclusive(e.Message);
              }
            }
          }
        }
      });
    }

    [TestMethod]
    [Timeout(2000)]
    public void SelectedDataIntoClipboardTest()
    {
      Extensions.RunSTAThread(() =>
      {
        using (var dgv = new DataGridView())
        {
          dgv.AutoGenerateColumns = true;
          using (var dt = UnitTestStatic.GetDataTable())
          {
            dgv.DataSource = dt;
            using (var frm = new Form())
            {
              frm.Controls.Add(dgv);
              frm.Show();
              try
              {
                Clipboard.Clear();
                dgv.SelectedDataIntoClipboard(true, false, UnitTestInitializeCsv.Token);
                var dataObject = Clipboard.GetDataObject();
                Assert.IsNotNull(dataObject);
                Assert.IsNotNull(dataObject.GetData(DataFormats.Text));
              }
              catch (ExternalException)
              {
                Assert.Inconclusive("Exception thrown but this can happen");
              }
              catch (Exception ex)
              {
                Assert.Fail($"Wrong exception {ex.Message}");
              }
            }
          }
        }
      });
    }
  }
}