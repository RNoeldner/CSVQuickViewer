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
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass]
  public class ControlsTests
  {
    [TestMethod]
    [Timeout(6000)]
    public void CsvTextDisplayShow()
    {
      using var csvTextDisplay = new FormCsvTextDisplay(UnitTestStatic.GetTestPath("BasicCSV.txt"), true);
      UnitTestStatic.ShowFormAndClose(csvTextDisplay, .2, (frm) => frm.OpenFile(false, "\"", "\t", "", 1200, 1, "##"),
        UnitTestStatic.Token);
    }

    [TestMethod]
    [Timeout(8000)]
    public void PersistentChoice()
    {
      var pc = new PersistentChoice(DialogResult.Yes);
      MessageBox.PersistentChoice("message", "Title", pc, "Yes", "no");
    }

    [TestMethod]
    [Timeout(10000)]
    public void HtmlDisplay()
    {
      Extensions.RunStaThread(() =>
      {
        using var tm = new TimedMessage();

        var stringBuilder = HtmlStyle.Default.StartHtmlDoc(
          $"{SystemColors.Control.R:X2}{SystemColors.Control.G:X2}{SystemColors.Control.B:X2}");
        stringBuilder.Append(string.Format(HtmlStyle.H2, HtmlStyle.TextToHtmlEncode("Sample")));
        stringBuilder.Append(string.Format(HtmlStyle.H2, HtmlStyle.TextToHtmlEncode("Sample2")));

        stringBuilder.AppendLine(HtmlStyle.TableOpen);
        stringBuilder.AppendLine(HtmlStyle.TrOpen);
        for (var index = 1; index <= 10; index++)
        {
          stringBuilder.AppendLine(string.Format(HtmlStyle.Td,
            HtmlStyle.TextToHtmlEncode("Test " + index.ToString())));
          if (index % 4 == 0)
          {
            stringBuilder.AppendLine(HtmlStyle.TrClose);
          }
        }

        stringBuilder.AppendLine(HtmlStyle.TrClose);
        stringBuilder.AppendLine(HtmlStyle.TableClose);
        stringBuilder.AppendLine(HtmlStyle.TableClose);
        stringBuilder.AppendLine(HtmlStyle.TrClose);
        stringBuilder.AppendLine(HtmlStyle.TableClose);
        tm.Html = stringBuilder.ToString();

        tm.Size = new Size(600, 450);
        UnitTestStatic.ShowFormAndClose(tm, 2);
      });
    }

    [TestMethod]
    [Timeout(10000)]
    public void TextDisplay()
    {
      Extensions.RunStaThread(() =>
      {
        using var tm = new TimedMessage();
        tm.Message = "Found values\rLine2\nDMS_Test_RN_Mat\tDMS_Test_RN_Mat\tDMS_Test_RN_Mat\tDMS_Test_RN_Mat\n" +
                     "DMS_Test_RN_Mat\tDMS_Test_RN_Mat\tDMS_Test_RN_Mat\tDMS_Test_RN_Mat\n\nNote: Text has been cut off after 15 characters";

        tm.Size = new Size(600, 450);
        UnitTestStatic.ShowFormAndClose(tm, 2);
      });
    }

    [TestMethod]
    [Timeout(5000)]
    public void ResizeForm()
    {
      using var resizeForm = new ResizeForm();
      UnitTestStatic.ShowFormAndClose(resizeForm, .2);
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormTextDisplay()
    {
      using var resizeForm = new FormTextDisplay("This is a test text\nSpanning some\nlines ...");
      UnitTestStatic.ShowFormAndClose(resizeForm, .2);
    }

    [TestMethod]
    public void FormTextDisplayJson()
    {
      using var resizeForm = new FormTextDisplay(
        "{\r\n  \"schema\": {\r\n    \"properties\": {\r\n      \"EmployeeIdentificationData_GUID\": {\r\n        \"format\": \"uuid\",\r\n        \"trim\": \"leftRight\",\r\n        \"caseSensitive\": false,\r\n        \"title\": \"GUID\",\r\n        \"description\": \"Unique identifier for a user. This cannot be changed\",\r\n        \"type\": \"string\"\r\n      }  }\r\n}}");
      UnitTestStatic.ShowFormAndClose(resizeForm, .2);
    }

    [TestMethod]
    public void FormTextDisplayXMl()
    {
      using var resizeForm = new FormTextDisplay(
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<edmx:Edmx Version=\"4.0\" xmlns:edmx=\"http://docs.oasis-open.org/odata/ns/edmx\">\r\n  <edmx:DataServices>  </edmx:DataServices>\r\n</edmx:Edmx>");
      UnitTestStatic.ShowFormAndClose(resizeForm, .2);
    }

    [TestMethod]
    [Timeout(5000)]
    public void QuotingControl1()
    {
      var ctrl = new QuotingControl();
      ctrl.CsvFile = new CsvFile(id: "CSV", fileName: "");
      UnitTestStatic.ShowControl(ctrl);
    }

    [TestMethod]
    [Timeout(5000)]
    public void QuotingControl2()
    {
      var ctrl = new QuotingControl();
      UnitTestStatic.ShowControl(ctrl, 0.1, control => control.CsvFile = new CsvFile(id: "CSV", fileName: ""));
    }

    [TestMethod]
    public void APICodePackWrapperFolder()
    {
      try
      {
        UnitTestStatic.RunTaskTimeout(token => Task.Run(() =>
            WindowsAPICodePackWrapper.Folder(FileSystemUtils.ExecutableDirectoryName(), "Test"), token), 1,
          UnitTestStatic.Token);
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
        UnitTestStatic.RunTaskTimeout(token => Task.Run(() =>
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
        UnitTestStatic.RunTaskTimeout(token => Task.Run(() =>
            WindowsAPICodePackWrapper.Save(FileSystemUtils.ExecutableDirectoryName(), "Test", string.Empty,
              string.Empty),
          token));
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
        UnitTestStatic.RunTaskTimeout(
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
        UnitTestStatic.RunTaskTimeout(
          token => Task.Run(() =>
          {
            WindowsAPICodePackWrapper.Save(FileSystemUtils.ExecutableDirectoryName(), "Test", "*.pdf", ".pdf", false,
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
    [Timeout(10000)]
    public void TimedMessage()
    {
      Extensions.RunStaThread(() =>
      {
        using (var tm = new TimedMessage())
        {
          tm.ShowDialog("This is my message", "Title1", MessageBoxButtons.OK, MessageBoxIcon.Asterisk,
            MessageBoxDefaultButton.Button1,
            2, null, null, null);
        }

        using (var tm = new TimedMessage())
        {
          tm.ShowDialog("This is another message\n with a linefeed", "Title12", MessageBoxButtons.YesNo,
            MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button2, 2, null, null, null);
        }
      });
    }

    [TestMethod]
    [Timeout(5000)]
    public void SearchShow() => UnitTestStatic.ShowControl(new Search());

    [TestMethod]
    [Timeout(4000)]
    public void ShowSelectFont() => UnitTestStatic.ShowControl(new SelectFont());

    [TestMethod]
    [Timeout(4000)]
    public void ShowLoggerDisplay() => UnitTestStatic.ShowControl(new LoggerDisplay());

    [TestMethod]
    [Timeout(5000)]
    public void FillGuessSettingEditShow() => UnitTestStatic.ShowControl(new FillGuessSettingEdit());

    [TestMethod]
    [Timeout(5000)]
    public void FormDuplicatesDisplay()
    {
      using var dataTable = UnitTestStatic.GetDataTable(60);
      using var form = new FormDuplicatesDisplay(dataTable, dataTable.Select(), dataTable.Columns[0].ColumnName,
        HtmlStyle.Default);
      UnitTestStatic.ShowFormAndClose(form);
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormUniqueDisplay()
    {
      using var dataTable = UnitTestStatic.GetDataTable(60);
      using var form = new FormUniqueDisplay(dataTable, dataTable.Select(), dataTable.Columns[0].ColumnName,
        HtmlStyle.Default);
      UnitTestStatic.ShowFormAndClose(form);
    }

    [TestMethod]
    [Timeout(3000)]
    public void FormShowMaxLength()
    {
      using var dataTable = UnitTestStatic.GetDataTable(60);
      using var form =
        new FormShowMaxLength(dataTable, dataTable.Select(), new List<string>(), HtmlStyle.Default);
      UnitTestStatic.ShowFormAndClose(form);
    }

    [TestMethod]
    [Timeout(5000)]
    public void DataGridViewColumnFilterControl()
    {
      using var dataTable = UnitTestStatic.GetDataTable(60);
      var col = new DataGridViewTextBoxColumn
      {
        ValueType = dataTable.Columns[0].DataType,
        Name = dataTable.Columns[0].ColumnName,
        DataPropertyName = dataTable.Columns[0].ColumnName
      };

      UnitTestStatic.ShowControl(new DataGridViewColumnFilterControl(col));
    }
  }
}