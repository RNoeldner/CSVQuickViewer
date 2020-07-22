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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ControlsTests
  {
    [TestMethod]
    public void HTMLDisplay()
    {
      UnitTestWinFormHelper.RunSTAThread(() =>
      {
        using (var tm = new TimedMessage())
        {
          var stringBuilder = HTMLStyle.StartHTMLDoc(SystemColors.Control);
          stringBuilder.Append(string.Format(ApplicationSetting.HTMLStyle.H2, HTMLStyle.TextToHtmlEncode("Sample")));
          stringBuilder.Append(string.Format(ApplicationSetting.HTMLStyle.H2, HTMLStyle.TextToHtmlEncode("Sample2")));

          stringBuilder.AppendLine(ApplicationSetting.HTMLStyle.TableOpen);
          stringBuilder.AppendLine(ApplicationSetting.HTMLStyle.TROpen);
          for (var index = 1; index <= 10; index++)
          {
            stringBuilder.AppendLine(string.Format(ApplicationSetting.HTMLStyle.TD,
              HTMLStyle.TextToHtmlEncode("Test " + index.ToString())));
            if (index % 4 == 0)
            {
              stringBuilder.AppendLine(ApplicationSetting.HTMLStyle.TRClose);
            }
          }

          stringBuilder.AppendLine(ApplicationSetting.HTMLStyle.TRClose);
          stringBuilder.AppendLine(ApplicationSetting.HTMLStyle.TableClose);
          stringBuilder.AppendLine(ApplicationSetting.HTMLStyle.TableClose);
          stringBuilder.AppendLine(ApplicationSetting.HTMLStyle.TRClose);
          stringBuilder.AppendLine(ApplicationSetting.HTMLStyle.TableClose);
          tm.Html = stringBuilder.ToString();

          tm.Size = new Size(600, 450);
          UnitTestWinFormHelper.ShowFormAndClose(tm, 2);
        }
      });
    }

    [TestMethod]
    public void TextDisplay()
    {
      UnitTestWinFormHelper.RunSTAThread(() =>
      {
        using (var tm = new TimedMessage())
        {
          tm.Message = "Found values\n\nDMS_Test_RN_Mat\tDMS_Test_RN_Mat\tDMS_Test_RN_Mat\tDMS_Test_RN_Mat\n" +
                       "DMS_Test_RN_Mat\tDMS_Test_RN_Mat\tDMS_Test_RN_Mat\tDMS_Test_RN_Mat\n\nNote: Text has been cut off after 15 characters";

          tm.Size = new Size(600, 450);
          UnitTestWinFormHelper.ShowFormAndClose(tm, 2);
        }
      });
    }

    [TestMethod]
    public void TimeZoneSelector()
    {
      using (var ctrl = new TimeZoneSelector())
      {
        UnitTestWinFormHelper.ShowControl(ctrl);
      }
    }

    [TestMethod]
    public void ResizeForm()
    {
      using (var frm = new ResizeForm())
      {
        UnitTestWinFormHelper.ShowFormAndClose(frm, .2, () =>
        {
          CsvTools.ResizeForm.SetFonts(frm, SystemFonts.DialogFont);
        });
      }
    }

    [TestMethod]
    public void QuotingControl()
    {
      using (var ctrl = new QuotingControl())
      {
        ctrl.CsvFile = new CsvFile();
        UnitTestWinFormHelper.ShowControl(ctrl);
      }
    }

    [TestMethod]
    public void APICodePackWrapperOpen()
    {
      try
      {
        UnitTestWinFormHelper.RunTaskTimeout(token => Task.Run(() =>
          WindowsAPICodePackWrapper.Open(FileSystemUtils.ExecutableDirectoryName(), "Test", "*.cs", null), token), 1);
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
        UnitTestWinFormHelper.RunTaskTimeout(
          token => Task.Run(
            () => { WindowsAPICodePackWrapper.Folder(FileSystemUtils.ExecutableDirectoryName(), "Test"); }, token), 1);
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
        UnitTestWinFormHelper.RunTaskTimeout(
          token => Task.Run(() =>
          {
            WindowsAPICodePackWrapper.Save(FileSystemUtils.ExecutableDirectoryName(), "Test", "*.pdf", "*.pdf", false,
              "test.pdf");
          }, token), 1);
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
      UnitTestWinFormHelper.RunSTAThread(() =>
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

          UnitTestWinFormHelper.ShowControl(treeView, .2, () =>
          {
            treeView.PressKey(Keys.Control | Keys.A);
            treeView.PressKey(Keys.Control | Keys.C);
            // ReSharper disable once AccessToDisposedClosure
            treeView.SelectedNode = treeNode2;
            treeNode.ExpandAll();
          });
          Assert.IsTrue(firedAfter);
          Assert.IsTrue(firedBefore);
        }
      });
    }

    [TestMethod]
    public void FormSelectTimeZone()
    {
      using (var frm = new FormSelectTimeZone())
      {
        frm.TimeZoneID = TimeZoneInfo.Local.Id;
        frm.DestTimeZoneID = TimeZoneInfo.Local.Id;
        UnitTestWinFormHelper.ShowFormAndClose(frm);
      }
    }

    [TestMethod]
    public void TimedMessage()
    {
      UnitTestWinFormHelper.RunSTAThread(() =>
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
      });
    }

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

        UnitTestWinFormHelper.ShowControl(ctrl);
      }
    }

    [TestMethod]
    public async Task CsvTextDisplayShow()
    {
      var ctrl = new CsvTextDisplay();

      using (var frm = new TestForm())
      {
        frm.AddOneControl(ctrl);
        frm.Show();
        await ctrl.SetCsvFile(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"), '"', '\t', '\0', 65001);

        Extensions.ProcessUIElements(500);
        frm.SafeInvoke(() => frm.Close());
      }
    }

    [TestMethod]
    public void SearchShow() => UnitTestWinFormHelper.ShowControl(new Search());

    [TestMethod]
    public void FillGuessSettingEditShow() => UnitTestWinFormHelper.ShowControl(new FillGuessSettingEdit());

    [TestMethod]
    public void FilteredDataGridViewShow() => UnitTestWinFormHelper.ShowControl(new FilteredDataGridView());

    [TestMethod]
    public void FilteredDataGridViewVariousMethods()
    {
      var ctrl = new FilteredDataGridView();
      using (var data = UnitTestStatic.GetDataTable(200))
      {
        ctrl.DataSource = data;
        UnitTestWinFormHelper.ShowControl(new FilteredDataGridView(), 0.5d,
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
    public void FormHierarchyDisplay()
    {
      using (var dataTable = UnitTestStatic.GetDataTable(60))
      using (var form = new FormHierarchyDisplay(dataTable, dataTable.Select()))
      {
        UnitTestWinFormHelper.ShowFormAndClose(form, 0.1, () => form.BuildTree("int", "ID"));
      }
    }

    [TestMethod]
    public async Task FormHierarchyDisplay_DataWithCycleAsync()
    {
      using (var dataTable = UnitTestStatic.GetDataTable(60))
      {
        // load the csvFile FileWithHierarchy
        using (var processDisplay = new FormProcessDisplay("FileWithHierarchy"))
        {
          processDisplay.Show();
          var cvsSetting = new CsvFile(Path.Combine(FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles",
            "FileWithHierarchy_WithCyle.txt"))
          { FileFormat = { FieldDelimiter = "\t" } };
          using (var csvDataReader = new CsvFileReader(cvsSetting, processDisplay))
          {
            var dt = await csvDataReader.GetDataTableAsync(0, false, true, false, false, false, null, null,
              processDisplay.CancellationToken);

            using (var form = new FormHierarchyDisplay(dt, dataTable.Select()))
            {
              UnitTestWinFormHelper.ShowFormAndClose(form, .1, () => form.BuildTree("ReferenceID1", "ID"));
              form.Close();
            }
          }
        }
      }
    }

    [TestMethod]
    public void FormDuplicatesDisplay()
    {
      using (var dataTable = UnitTestStatic.GetDataTable(60))
      using (var form = new FormDuplicatesDisplay(dataTable, dataTable.Select(), dataTable.Columns[0].ColumnName))
      {
        UnitTestWinFormHelper.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    public void FormUniqueDisplay()
    {
      using (var dataTable = UnitTestStatic.GetDataTable(60))
      using (var form = new FormUniqueDisplay(dataTable, dataTable.Select(), dataTable.Columns[0].ColumnName))
      {
        UnitTestWinFormHelper.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    public void FormShowMaxLength()
    {
      using (var dataTable = UnitTestStatic.GetDataTable(60))
      using (var form = new FormShowMaxLength(dataTable, dataTable.Select()))
      {
        UnitTestWinFormHelper.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    public void FormDetail()
    {
      using (var dataTable = UnitTestStatic.GetDataTable(60))
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var form = new FormDetail(dataTable, null, null, true, false, 0, new FillGuessSettings(),
        processDisplay.CancellationToken))
      {
        UnitTestWinFormHelper.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    public void DataGridViewColumnFilterControl()
    {
      using (var dataTable = UnitTestStatic.GetDataTable(60))
      {
        var col = new DataGridViewTextBoxColumn
        {
          ValueType = dataTable.Columns[0].DataType,
          Name = dataTable.Columns[0].ColumnName,
          DataPropertyName = dataTable.Columns[0].ColumnName
        };

        UnitTestWinFormHelper.ShowControl(new DataGridViewColumnFilterControl(col));
      }
    }
  }
}