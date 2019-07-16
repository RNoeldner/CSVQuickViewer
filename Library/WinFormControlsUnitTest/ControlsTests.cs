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

using System.Data;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pri.LongPath;

namespace CsvTools.Tests
{
  [TestClass]
  public class ControlsTests
  {
    private static readonly DataTable m_DataTable = UnitTestStatic.GetDataTable(50);
    private readonly string m_ApplicationDirectory = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles";

    [TestMethod]
    public void FormLimitSize()
    {
      using (var frm = new FrmLimitSize())
      {
        frm.RecordLimit = 1000;
        frm.Show();
        frm.RecordLimit = 20;
        System.Threading.Thread.Sleep(200);
      }
    }

    [ClassCleanup]
    public static void TearDown() => m_DataTable.Dispose();

    [TestMethod]
    public void CsvRichTextBox()
    {
      using (var ctrl = new CSVRichTextBox())
      {
        ctrl.Text = "This is a Test";
        Assert.AreEqual("This is a Test", ctrl.Text);

        ctrl.Delimiter = ';';
        Assert.AreEqual(';', ctrl.Delimiter);

        ctrl.DisplaySpace = true;
        Assert.IsTrue(ctrl.DisplaySpace);
        ctrl.DisplaySpace = false;
        Assert.IsFalse(ctrl.DisplaySpace);

        ctrl.Escape = '#';
        Assert.AreEqual('#', ctrl.Escape);

        ctrl.Quote = '?';
        Assert.AreEqual('?', ctrl.Quote);
      }
    }

    [TestMethod]
    public void CsvTextDisplay()
    {
      var file = new CsvFile("BasicCSV.txt");
      using (var ctrl = new CsvTextDisplay())
      {
        ctrl.CsvFile = file;
        ctrl.Show();
      }
    }

    [TestMethod]
    public void FormColumnUI()
    {
      var csvFile = new CsvFile("BasicCSV.txt");
      var col = new Column { Name = "ExamDate", DataType = DataType.DateTime };
      csvFile.ColumnCollection.AddIfNew(col);
      using (var ctrl = new FormColumnUI(col, false, csvFile))
      {
        ctrl.Show();
        ctrl.Close();
      }
    }

    [TestMethod]
    public void FormColumnUI_Opt1()
    {
      var csvFile = new CsvFile(Path.Combine(m_ApplicationDirectory, "BasicCSV.txt"));
      var col = new Column { Name = "ExamDate", DataType = DataType.DateTime };
      csvFile.ColumnCollection.AddIfNew(col);
      using (var ctrl = new FormColumnUI(col, false, csvFile))
      {
        ctrl.ShowGuess = false;
        ctrl.Show();
        ctrl.Close();
      }
    }

    [TestMethod]
    public void FormColumnUI_Opt2()
    {
      var csvFile = new CsvFile(Path.Combine(m_ApplicationDirectory, "BasicCSV.txt"));
      var col = new Column { Name = "ExamDate", DataType = DataType.DateTime };
      csvFile.ColumnCollection.AddIfNew(col);
      using (var ctrl = new FormColumnUI(col, false, csvFile))
      {
        ctrl.ShowIgnore = false;
        ctrl.Show();
        ctrl.Close();
      }
    }

    [TestMethod]
    public void FormColumnUI_ButtonGuessClick()
    {
      var csvFile = new CsvFile(Path.Combine(m_ApplicationDirectory, "BasicCSV.txt"));
      var col = new Column { Name = "ExamDate", DataType = DataType.DateTime };
      csvFile.ColumnCollection.AddIfNew(col);

      using (var ctrl = new FormColumnUI(col, false, csvFile))
      {
        ctrl.Show();
        // open the reader file
        ctrl.ButtonGuessClick(null, null);

        ctrl.Close();
      }
    }

    [TestMethod]
    public void FormHierachyDisplay()
    {
      using (var ctrl = new FormHierachyDisplay(m_DataTable, m_DataTable.Select()))
      {
        ctrl.Show();
        ctrl.Close();
      }
    }

    [TestMethod]
    public void FormDuplicatesDisplay()
    {
      using (var ctrl = new FormDuplicatesDisplay(m_DataTable, m_DataTable.Select(), m_DataTable.Columns[0].ColumnName))
      {
        ctrl.Show();
        ctrl.Close();
      }
    }

    [TestMethod]
    public void FormUniqueDisplay()
    {
      using (var ctrl = new FormUniqueDisplay(m_DataTable, m_DataTable.Select(), m_DataTable.Columns[0].ColumnName))
      {
        ctrl.Show();
        ctrl.Close();
      }
    }

    [TestMethod]
    public void FormKeyFile()
    {
      using (var ctrl = new FormKeyFile("Test", true))
      {
        ctrl.Show();
        ctrl.Close();
      }
    }

    [TestMethod]
    public void FormShowMaxLength()
    {
      using (var ctrl = new FormShowMaxLength(m_DataTable, m_DataTable.Select()))
      {
        ctrl.Show();
        ctrl.Close();
      }
    }

    [TestMethod]
    public void FormDetail()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var ctrl = new FormDetail(m_DataTable, null, null, true, false, 0, processDisplay.CancellationToken))
      {
        ctrl.Show();
        ctrl.Close();
      }
    }

    [TestMethod]
    public void DataGridViewColumnFilterControl()
    {
      var col = new DataGridViewTextBoxColumn
      {
        ValueType = m_DataTable.Columns[0].DataType,
        Name = m_DataTable.Columns[0].ColumnName,
        DataPropertyName = m_DataTable.Columns[0].ColumnName,
        Tag = m_DataTable.Columns[0].DataType
      };

      using (var ctrl = new DataGridViewColumnFilterControl(m_DataTable.Columns[0].DataType, col))
      {
        ctrl.Show();
        ctrl.FocusInput();
      }
    }

    [TestMethod]
    public void FormPassphrase()
    {
      using (var ctrl = new FormPassphrase("Test"))
      {
        ctrl.Show();
        ctrl.Close();
      }
    }
  }
}