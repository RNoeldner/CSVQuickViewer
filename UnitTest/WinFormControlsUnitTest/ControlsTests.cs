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
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
  public class ControlsTests
  {
    private static readonly DataTable m_DataTable = UnitTestStatic.GetDataTable(60);

    private readonly CsvFile m_CSVFile =
      new CsvFile(Path.Combine(FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles", "BasicCSV.txt"));

    [TestMethod]
    public void TimeZoneSelector()
    {
      using (var ctrl = new TimeZoneSelector())
      {
        UnitTestInitialize.ShowControl(ctrl);
      }
    }

    [TestMethod]
    public void QuotingControl()
    {
      using (var ctrl = new QuotingControl())
      {
        ctrl.CsvFile = new CsvFile();
        UnitTestInitialize.ShowControl(ctrl);
      }
    }

    [TestMethod]
    public void APICodePackWrapperOpen()
    {
      try
      {
        // Used to cancel after .2 seconds
        Task.Run(() => WindowsAPICodePackWrapper.Open(FileSystemUtils.ExecutableDirectoryName(), "Test", "*.cs", null))
          .WaitToCompleteTask(.2);
      }
      catch (COMException)
      {
      }
      catch (TimeoutException)
      {
      }
      catch (OperationCanceledException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail(
          $"Wrong exception got {ex.GetType().Name} expected OperationCanceledException : {ex.ExceptionMessages()}");
      }
    }

    [TestMethod]
    public void WindowsAPICodePackWrapperFolder()
    {
      try
      {
        // Used to cancel after .2 seconds
        Task.Run(() => { WindowsAPICodePackWrapper.Folder(FileSystemUtils.ExecutableDirectoryName(), "Test"); })
          .WaitToCompleteTask(.2);
      }
      catch (COMException)
      {
      }
      catch (TimeoutException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail(
          $"Wrong exception got {ex.GetType().Name} expected OperationCanceledException : {ex.ExceptionMessages()}");
      }
    }

    [TestMethod]
    public void WindowsAPICodePackWrapperSave()
    {
      try
      {
        // Used to cancel after .2 seconds
        Task.Run(() =>
        {
          WindowsAPICodePackWrapper.Save(FileSystemUtils.ExecutableDirectoryName(), "Test", "*.pdf", "*.pdf",
            "test.pdf");
        }).WaitToCompleteTask(.2);
      }
      catch (COMException)
      {
      }
      catch (TimeoutException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail(
          $"Wrong exception got {ex.GetType().Name} expected OperationCanceledException : {ex.ExceptionMessages()}");
      }
    }

    [TestMethod]
    public void MultiselectTreeView()
    {
      using (var treeView = new MultiselectTreeView())
      {
        Assert.AreEqual(0, treeView.SelectedTreeNode.Count);

        var treeNode = new TreeNode("Test") {Tag = "test"};
        treeView.Nodes.Add(treeNode);

        var treeNode2 = new TreeNode("Test2") {Tag = "test2"};
        treeNode.Nodes.Add(treeNode2);

        var firedAfter = false;
        var firedBefore = false;
        treeView.AfterSelect += (s, args) => { firedAfter = true; };
        treeView.BeforeSelect += (s, args) => { firedBefore = true; };

        UnitTestInitialize.ShowControl(treeView, .2, () =>
        {
          treeView.SelectedNode = treeNode2;
          treeNode.ExpandAll();
        });
        Assert.IsTrue(firedAfter);
        Assert.IsTrue(firedBefore);
      }
    }

    [TestMethod]
    public void FormSelectTimeZone()
    {
      using (var frm = new FormSelectTimeZone())
      {
        frm.TimeZoneID = TimeZoneInfo.Local.Id;
        frm.DestTimeZoneID = TimeZoneInfo.Local.Id;
        UnitTestInitialize.ShowFormAndClose(frm);
      }
    }

    [TestMethod]
    public void FormLimitSize()
    {
      using (var frm = new FrmLimitSize())
      {
        frm.RecordLimit = 1000;
        frm.Show();
        frm.RecordLimit = 20;
        UnitTestInitialize.ShowFormAndClose(frm);
      }
    }

    [TestMethod]
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

        UnitTestInitialize.ShowControl(ctrl);
      }
    }

    [TestMethod]
    public void CsvTextDisplayShow()
    {
      var ctrl = new CsvTextDisplay();
      UnitTestInitialize.ShowControl(ctrl, .1, async () => await ctrl.SetCsvFile(m_CSVFile));
    }

    [TestMethod]
    public void SearchShow() => UnitTestInitialize.ShowControl(new Search());

    [TestMethod]
    public void FillGuessSettingEditShow() => UnitTestInitialize.ShowControl(new FillGuessSettingEdit());

    [TestMethod]
    public void FilteredDataGridViewShow() => UnitTestInitialize.ShowControl(new FilteredDataGridView());


    [TestMethod]
    public void FilteredDataGridViewVariousMethods()
    {
      var ctrl = new FilteredDataGridView();
      using (var data = UnitTestStatic.GetDataTable(200))
      {
        ctrl.DataSource = data;
        UnitTestInitialize.ShowControl(new FilteredDataGridView(), 0.5d,
          () =>
          {
            ctrl.FrozenColumns = 1;
            ctrl.SetFilterMenu(0);
            ctrl.HighlightText = "HH";
            ctrl.SetRowHeight();
            ctrl.SetFilterMenu(1);
          });
      }

    }

    [TestMethod]
    public void FormColumnUI()
    {
      var col = new Column("ExamDate", DataType.DateTime);
      m_CSVFile.ColumnCollection.AddIfNew(col);
      using (var frm = new FormColumnUI(col, false, m_CSVFile, new FillGuessSettings(), false))
      {
        UnitTestInitialize.ShowFormAndClose(frm);
      }
    }

    [TestMethod]
    public void FormColumnUI_Opt1()
    {
      var col = new Column("ExamDate", DataType.DateTime);
      m_CSVFile.ColumnCollection.AddIfNew(col);
      using (var form = new FormColumnUI(col, false, m_CSVFile, new FillGuessSettings(), true))
      {
        form.ShowGuess = false;
        UnitTestInitialize.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    public void FormColumnUI_Opt2()
    {
      var col = new Column("ExamDate", DataType.DateTime);
      m_CSVFile.ColumnCollection.AddIfNew(col);
      using (var form = new FormColumnUI(col, false, m_CSVFile, new FillGuessSettings(), false))
      {
        UnitTestInitialize.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    public void FormColumnUI_ButtonGuessClick()
    {
      var col = new Column("ExamDate", DataType.DateTime);
      m_CSVFile.ColumnCollection.AddIfNew(col);

      using (var form = new FormColumnUI(col, false, m_CSVFile, new FillGuessSettings(), true))
      {
        UnitTestInitialize.ShowFormAndClose(form, .2, () => form.ButtonGuessClick(null, null));

        // open the reader file
        form.ButtonGuessClick(null, null);
        UnitTestInitialize.WaitSomeTime(.2);

        form.Close();
      }
    }

    [TestMethod]
    public void FormHierarchyDisplay()
    {
      using (var form = new FormHierarchyDisplay(m_DataTable, m_DataTable.Select()))
      {
        UnitTestInitialize.ShowFormAndClose(form, 0.1, () => form.BuildTree("int", "ID"));
      }
    }

    [TestMethod]
    public async Task FormHierarchyDisplay_DataWithCycleAsync()
    {
      DataTable dt;
      // load the csvFile FileWithHierarchy
      using (var processDisplay = new FormProcessDisplay("FileWithHierarchy"))
      {
        processDisplay.Show();
        var cvsSetting = new CsvFile(Path.Combine(FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles",
            "FileWithHierarchy_WithCyle.txt"))
          {FileFormat = {FieldDelimiter = "\t"}};
        using (var csvDataReader = new CsvFileReader(cvsSetting, null, processDisplay))
        {
          dt = await csvDataReader.GetDataTableAsync(0, false, false, true, processDisplay.CancellationToken);
        }
      }

      using (var form = new FormHierarchyDisplay(dt, m_DataTable.Select()))
      {
        UnitTestInitialize.ShowFormAndClose(form, .1, () => form.BuildTree("ReferenceID1", "ID"));
        form.Close();
      }
    }

    [TestMethod]
    public void FormDuplicatesDisplay()
    {
      using (var form = new FormDuplicatesDisplay(m_DataTable, m_DataTable.Select(), m_DataTable.Columns[0].ColumnName))
      {
        UnitTestInitialize.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    public void FormUniqueDisplay()
    {
      using (var form = new FormUniqueDisplay(m_DataTable, m_DataTable.Select(), m_DataTable.Columns[0].ColumnName))
      {
        UnitTestInitialize.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    public void FormShowMaxLength()
    {
      using (var form = new FormShowMaxLength(m_DataTable, m_DataTable.Select()))
      {
        UnitTestInitialize.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    public void FormDetail()
    {
      using (var processDisplay = new CustomProcessDisplay(CancellationToken.None, null))
      using (var form = new FormDetail(m_DataTable, null, null, true, false, 0, new FillGuessSettings(),
        processDisplay.CancellationToken))
      {
        UnitTestInitialize.ShowFormAndClose(form);
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

      UnitTestInitialize.ShowControl(new DataGridViewColumnFilterControl(col));
    }
  }
}