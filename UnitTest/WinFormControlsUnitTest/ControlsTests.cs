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
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pri.LongPath;

namespace CsvTools.Tests
{
  [TestClass]
  public class ControlsTests
  {
    private static readonly DataTable m_DataTable = UnitTestStatic.GetDataTable(50);

    private readonly CsvFile m_CSVFile =
      new CsvFile(Path.Combine(FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles", "BasicCSV.txt"));

    [TestMethod]
    public void QuotingControl()
    {
      using (var frm = new QuotingControl())
      {
        frm.Show();
        frm.CsvFile = new CsvFile();
        Thread.Sleep(200);
      }
    }

    [TestMethod]
    public void MultiselectTreeView()
    {
      using (var frm = new Form())
      using (var treeView = new MultiselectTreeView())
      {
        frm.Controls.Add(treeView);
        treeView.Dock = DockStyle.Fill;
        frm.Show();
        Assert.AreEqual(0, treeView.SelectedTreeNode.Count);

        var treeNode = new TreeNode("Test") {Tag = "test"};
        treeView.Nodes.Add(treeNode);

        var treeNode2 = new TreeNode("Test2") {Tag = "test2"};
        treeNode.Nodes.Add(treeNode2);

        Thread.Sleep(200);
      }
    }

    [TestMethod]
    public void FolderTree()
    {
      using (var frm = new FolderTree())
      {
        frm.Show();
        frm.SetCurrentPath("C:\\");
        frm.Refresh();
        //frm.ShowError(new ApplicationException("Exception"), "Title" );
        frm.Close();
      }
    }

    [TestMethod]
    public void FormSelectTimeZone()
    {
      using (var frm = new FormSelectTimeZone())
      {
        frm.ShowInTaskbar = false;
        frm.Show();
        Thread.Sleep(200);
        frm.Close();
      }
    }

    [TestMethod]
    public void FormLimitSize()
    {
      using (var frm = new FrmLimitSize())
      {
        frm.RecordLimit = 1000;
        frm.ShowInTaskbar = false;
        frm.Show();
        frm.RecordLimit = 20;
        Thread.Sleep(200);
      }
    }

    public void MessageBox_ShowBigRtf()
    {
      var rtfHelper = new RtfHelper();
      rtfHelper.AddParagraph("RTF \\ Table {Nice}");
      rtfHelper.AddTable(new[]
        {"Hello", "World", "", null, "A", "Table", "Test", null, "Another", "Row", "Long Column Text"});
      _MessageBox.ShowBigRtf(null, rtfHelper.Rtf, "RTF Text", MessageBoxButtons.OK, MessageBoxIcon.Information,
        MessageBoxDefaultButton.Button1, 2);
    }

    [TestMethod]
    public void TimedMessage()
    {
      using (var tm = new TimedMessage())
      {
        tm.Show(null, "This is my message", "Title1", MessageBoxButtons.OK, MessageBoxIcon.Asterisk,
          MessageBoxDefaultButton.Button1, 2, null, null, null);
      }

      using (var tm = new TimedMessage())
      {
        tm.Show(null, "This is another message\n with a linefeed", "Title12", MessageBoxButtons.YesNo,
          MessageBoxIcon.Error, MessageBoxDefaultButton.Button2, 2, null, null, null);
      }
    }

    [ClassCleanup]
    public static void TearDown() => m_DataTable.Dispose();

    [TestMethod]
    public void CsvRichTextBox()
    {
      using (var ctrl = new CSVRichTextBox())
      {
        ctrl.Text = @"This is a Test";
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

        ShowControl(ctrl);
      }
    }

    private void ShowControl(Control ctrl)
    {
      using (var frm = new Form())
      {
        frm.SuspendLayout();
        frm.Text = ctrl.GetType().FullName;
        frm.BackColor = SystemColors.Control;
        frm.ClientSize = new Size(800, 800);
        frm.ShowInTaskbar = false;
        frm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
        frm.StartPosition = FormStartPosition.CenterScreen;

        ctrl.Dock = DockStyle.Fill;
        ctrl.Location = new Point(0, 0);
        ctrl.Size = new Size(600, 600);
        frm.Controls.Add(ctrl);
        frm.TopMost = true;
        frm.ResumeLayout(false);

        frm.Show();
        frm.Focus();
        ctrl.Focus();
        Thread.Sleep(100);

        frm.Close();
      }
    }

    [TestMethod]
    public void CsvTextDisplayShow() => ShowControl(new CsvTextDisplay
    {
      CsvFile = m_CSVFile
    });

    [TestMethod]
    public void SearchShow() => ShowControl(new Search());

    [TestMethod]
    public void FillGuessSettingEditShow() => ShowControl(new FillGuessSettingEdit());

    [TestMethod]
    public void FilteredDataGridViewShow() => ShowControl(new FilteredDataGridView());

    [TestMethod]
    public void FormColumnUI()
    {
      var col = new Column("ExamDate", DataType.DateTime);
      m_CSVFile.ColumnCollection.AddIfNew(col);
      using (var frm = new FormColumnUI(col, false, m_CSVFile, new FillGuessSettings(), false))
      {
        frm.ShowInTaskbar = false;
        frm.Show();
        frm.Close();
      }
    }

    [TestMethod]
    public void FormColumnUI_Opt1()
    {
      var col = new Column("ExamDate", DataType.DateTime);
      m_CSVFile.ColumnCollection.AddIfNew(col);
      using (var form = new FormColumnUI(col, false, m_CSVFile, new FillGuessSettings(), true))
      {
        form.ShowInTaskbar = false;
        form.ShowGuess = false;
        form.Show();
        form.Close();
      }
    }

    [TestMethod]
    public void FormColumnUI_Opt2()
    {
      var col = new Column("ExamDate", DataType.DateTime);
      m_CSVFile.ColumnCollection.AddIfNew(col);
      using (var form = new FormColumnUI(col, false, m_CSVFile, new FillGuessSettings(), false))
      {
        form.ShowInTaskbar = false;
        form.Show();
        form.Close();
      }
    }

    [TestMethod]
    public void FormColumnUI_ButtonGuessClick()
    {
      var col = new Column("ExamDate", DataType.DateTime);
      m_CSVFile.ColumnCollection.AddIfNew(col);

      using (var form = new FormColumnUI(col, false, m_CSVFile, new FillGuessSettings(), true))
      {
        form.ShowInTaskbar = false;
        form.Show();
        // open the reader file
        form.ButtonGuessClick(null, null);

        form.Close();
      }
    }

    [TestMethod]
    public void FormProcessDisplay()
    {
      // Log
      using (var frm = new FormProcessDisplay("Test Logger", true, CancellationToken.None))
      {
        frm.Show();
        frm.Maximum = 100;
        for (var c = 0; c < 70; c++)
        {
          frm.SetProcess($"This is a text\nLine {c}", c, true);
          Extensions.ProcessUIElements(100);
        }
      }

      // marquee
      using (var frm = new FormProcessDisplay("Test Marquee", false, CancellationToken.None))
      {
        frm.Show();
        for (var c = 0; c < 100; c++)
        {
          frm.SetProcess($"This is a text\nLine {c}", c, true);
          Extensions.ProcessUIElements(20);
        }
      }

      // NoLog
      using (var frm = new FormProcessDisplay("Test", false, CancellationToken.None))
      {
        frm.Show();
        frm.Maximum = 100;
        for (var c = 0; c < 102; c++)
        {
          frm.SetProcess($"This is a text\nLine {c}", c, true);
          Extensions.ProcessUIElements(50);
        }
      }
    }

    [TestMethod]
    public void FormHierarchyDisplay()
    {
      using (var form = new FormHierarchyDisplay(m_DataTable, m_DataTable.Select()))
      {
        form.ShowInTaskbar = false;
        form.Show();
        form.Focus();
        form.BuildTree("int", "ID");
        form.Close();
      }
    }

    [TestMethod]
    public void FormHierarchyDisplay_DataWithCycle()
    {
      DataTable dt;
      // load the csvFile FileWithHierarchy
      using (var processDisplay = new FormProcessDisplay("FileWithHierarchy"))
      {
        processDisplay.Show();
        var cvsSetting = new CsvFile(Path.Combine(FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles",
          "FileWithHierarchy_WithCyle.txt")) {FileFormat = {FieldDelimiter = "\t"}};
        using (var csvDataReader = new CsvFileReader(cvsSetting, null, processDisplay))
        {
          dt = csvDataReader.Read2DataTable(processDisplay, 0);
        }
      }

      using (var form = new FormHierarchyDisplay(dt, m_DataTable.Select()))
      {
        form.ShowInTaskbar = false;
        form.Show();
        form.Focus();
        form.BuildTree("ReferenceID1", "ID");
        Application.DoEvents();
        Thread.Sleep(200);
        form.Close();
      }
    }

    [TestMethod]
    public void FormDuplicatesDisplay()
    {
      using (var form = new FormDuplicatesDisplay(m_DataTable, m_DataTable.Select(), m_DataTable.Columns[0].ColumnName))
      {
        form.ShowInTaskbar = false;
        form.Show();
        form.Close();
      }
    }

    [TestMethod]
    public void FormUniqueDisplay()
    {
      using (var form = new FormUniqueDisplay(m_DataTable, m_DataTable.Select(), m_DataTable.Columns[0].ColumnName))
      {
        form.ShowInTaskbar = false;
        form.Show();
        form.Close();
      }
    }

    [TestMethod]
    public void FormShowMaxLength()
    {
      using (var form = new FormShowMaxLength(m_DataTable, m_DataTable.Select()))
      {
        form.ShowInTaskbar = false;
        form.Show();
        form.Close();
      }
    }

    [TestMethod]
    public void FormDetail()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var form = new FormDetail(m_DataTable, null, null, true, false, 0, new FillGuessSettings(),
        processDisplay.CancellationToken))
      {
        form.ShowInTaskbar = false;
        form.Show();
        form.Close();
      }
    }

    [TestMethod]
    public void DataGridViewColumnFilterControl()
    {
      var col = new DataGridViewTextBoxColumn
      {
        ValueType = m_DataTable.Columns[0].DataType,
        Name = m_DataTable.Columns[0].ColumnName,
        DataPropertyName = m_DataTable.Columns[0].ColumnName
      };

      ShowControl(new DataGridViewColumnFilterControl(col));
    }
  }
}