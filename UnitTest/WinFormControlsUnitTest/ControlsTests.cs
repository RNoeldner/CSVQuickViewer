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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass]
  public class ControlsTests
  {
    [TestMethod]
    [Timeout(8000)]
    public void PersistentChoice()
    {
      var pc = new PersistentChoice(DialogResult.Yes);
      _MessageBox.PersistentChoice(null, "message", "Title", pc, "Yes", "no");
    }

    [TestMethod]
    [Timeout(10000)]
    public void HTMLDisplay()
    {
      Extensions.RunSTAThread(() =>
      {
        using (var tm = new TimedMessage())
        {
          var stringBuilder = HTMLStyle.StartHTMLDoc(SystemColors.Control, "");
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
    [Timeout(10000)]
    public void TextDisplay()
    {
      Extensions.RunSTAThread(() =>
      {
        using (var tm = new TimedMessage())
        {
          tm.Message = "Found values\rLine2\nDMS_Test_RN_Mat\tDMS_Test_RN_Mat\tDMS_Test_RN_Mat\tDMS_Test_RN_Mat\n" +
                       "DMS_Test_RN_Mat\tDMS_Test_RN_Mat\tDMS_Test_RN_Mat\tDMS_Test_RN_Mat\n\nNote: Text has been cut off after 15 characters";

          tm.Size = new Size(600, 450);
          UnitTestWinFormHelper.ShowFormAndClose(tm, 2);
        }
      });
    }


    [TestMethod]
    [Timeout(5000)]
    public void ResizeForm()
    {
      using (var frm = new ResizeForm())
      {
        UnitTestWinFormHelper.ShowFormAndClose(frm, .2, (from) =>
        {
          CsvTools.ResizeForm.SetFonts(from, SystemFonts.DialogFont);
        });
      }
    }

    [TestMethod]
    [Timeout(5000)]
    public void QuotingControl()
    {
      using (var ctrl = new QuotingControl())
      {
        ctrl.CsvFile = new CsvFile();
        UnitTestWinFormHelper.ShowControl(ctrl);
      }
    }

    [TestMethod]
    public void APICodePackWrapperFolder()
    {
      try
      {
        UnitTestWinFormHelper.RunTaskTimeout(token => Task.Run(() =>
          WindowsAPICodePackWrapper.Folder(FileSystemUtils.ExecutableDirectoryName(), "Test"), token));
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
    [Timeout(2000)]
    public void APICodePackWrapperOpen()
    {
      try
      {
        UnitTestWinFormHelper.RunTaskTimeout(token => Task.Run(() =>
          WindowsAPICodePackWrapper.Open(FileSystemUtils.ExecutableDirectoryName(), "Test", "*.cs", null), token));
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
    [Timeout(2000)]
    public void APICodePackWrapperSave()
    {
      try
      {
        UnitTestWinFormHelper.RunTaskTimeout(token => Task.Run(() =>
          WindowsAPICodePackWrapper.Save(FileSystemUtils.ExecutableDirectoryName(), "Test", null), token));
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
    [Timeout(2000)]
    public void WindowsAPICodePackWrapperFolder()
    {
      try
      {
        // Used to cancel after .2 seconds
        UnitTestWinFormHelper.RunTaskTimeout(
          token => Task.Run(
            () => { WindowsAPICodePackWrapper.Folder(FileSystemUtils.ExecutableDirectoryName(), "Test"); }, token));
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
    [Timeout(2000)]
    public void WindowsAPICodePackWrapperSave()
    {
      try
      {
        // Used to cancel after .2 seconds
        UnitTestWinFormHelper.RunTaskTimeout(
          token => Task.Run(() =>
          {
            WindowsAPICodePackWrapper.Save(FileSystemUtils.ExecutableDirectoryName(), "Test",  "*.pdf", false,
              "test.pdf");
          }, token));
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
    [Timeout(5000)]
    public void MultiselectTreeView()
    {
      Extensions.RunSTAThread(() =>
      {
        using (var treeView = new MultiselectTreeView())
        {
          Assert.AreEqual(0, treeView.SelectedTreeNode.Count);

          var treeNode = new TreeNode("Test") { Tag = "test" };
          treeView.Nodes.Add(treeNode);

          var treeNode2 = new TreeNode("Test2") { Tag = "test2" };
          treeNode.Nodes.Add(treeNode2);

          var firedAfter = false;
          var firedBefore = false;
          treeView.AfterSelect += (s, args) => { firedAfter = true; };
          treeView.BeforeSelect += (s, args) => { firedBefore = true; };

          UnitTestWinFormHelper.ShowControl(treeView, .2, (control, form) =>
          {
            if (!(control is MultiselectTreeView text))
              return;
            text.PressKey(Keys.Control | Keys.A);
            text.PressKey(Keys.Control | Keys.C);
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
    [Timeout(10000)]
    public void TimedMessage()
    {
      Extensions.RunSTAThread(() =>
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
    [Timeout(5000)]
    public void SearchShow() => UnitTestWinFormHelper.ShowControl(new Search());

    [TestMethod]
    [Timeout(5000)]
    public void FillGuessSettingEditShow() => UnitTestWinFormHelper.ShowControl(new FillGuessSettingEdit());

    [TestMethod]
    [Timeout(5000)]
    public void FormHierarchyDisplay()
    {
      using (var dataTable = UnitTestStatic.GetDataTable(60))
      using (var form = new FormHierarchyDisplay(dataTable, dataTable.Select()))
      {
        UnitTestWinFormHelper.ShowFormAndClose(form, 0.1, (frm) =>
        {
          if (!(frm is FormHierarchyDisplay hd))
            return;
          hd.BuildTree("int", "ID");
        });
      }
    }

    [TestMethod]
    [Timeout(5000)]
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
            var dt = await csvDataReader.GetDataTableAsync(0, false, true, false, false, false, null,
              processDisplay.CancellationToken);

            using (var form = new FormHierarchyDisplay(dt, dataTable.Select()))
            {
              UnitTestWinFormHelper.ShowFormAndClose(form, .1, (frm) =>
              {
                if (!(frm is FormHierarchyDisplay hd))
                  return;
                hd.BuildTree("ReferenceID1", "ID");
              });
              form.Close();
            }
          }
        }
      }
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormDuplicatesDisplay()
    {
      using (var dataTable = UnitTestStatic.GetDataTable(60))
      using (var form = new FormDuplicatesDisplay(dataTable, dataTable.Select(), dataTable.Columns[0].ColumnName))
      {
        UnitTestWinFormHelper.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormUniqueDisplay()
    {
      using (var dataTable = UnitTestStatic.GetDataTable(60))
      using (var form = new FormUniqueDisplay(dataTable, dataTable.Select(), dataTable.Columns[0].ColumnName))
      {
        UnitTestWinFormHelper.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    [Timeout(3000)]
    public void FormShowMaxLength()
    {
      using (var dataTable = UnitTestStatic.GetDataTable(60))
      using (var form = new FormShowMaxLength(dataTable, dataTable.Select(), new List<string>()))
      {
        UnitTestWinFormHelper.ShowFormAndClose(form);
      }
    }



    [TestMethod]
    [Timeout(5000)]
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