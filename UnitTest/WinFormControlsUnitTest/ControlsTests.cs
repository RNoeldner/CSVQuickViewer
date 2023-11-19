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
    [TestMethod, Timeout(1000)]
    public void FormPasswordAndKey()
    {
      UnitTestStaticForms.ShowForm((Func<Form>) (() => new FormPasswordAndKey("Test")), 0, (f) =>
      {
        if (f is FormPasswordAndKey fpak)
        {
          fpak.Passphrase = "Test";
          fpak.FileName = @"c:\Test.asc";
        }
      });
    }

    [TestMethod, Timeout(10000)]
    public void CsvTextDisplayShowAsync()
    {
      UnitTestStaticForms.ShowFormAsync(
        () => new FormCsvTextDisplay(UnitTestStatic.GetTestPath("BasicCSV.txt"), null), 
        async frm => await frm.OpenFileAsync(false, '"', '\t', char.MinValue, 1200, 1, "##", UnitTestStatic.Token), .1);

    }

    [TestMethod, Timeout(8000)]
    public void PersistentChoice()
    {
      var pc = new PersistentChoice(DialogResult.Yes);
      MessageBox.PersistentChoice("message", "Title", pc, "Yes", "no");
    }

    [TestMethod, Timeout(3000)]
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
        UnitTestStaticForms.ShowForm(() => tm, 2);
      });
    }

    [TestMethod, Timeout(3000)]
    public void TextDisplay()
    {
      UnitTestStaticForms.ShowForm(() => new TimedMessage
      {
        Message = "Found values\rLine2\nDMS_Test_RN_Mat\tDMS_Test_RN_Mat\tDMS_Test_RN_Mat\tDMS_Test_RN_Mat\n" +
                  "DMS_Test_RN_Mat\tDMS_Test_RN_Mat\tDMS_Test_RN_Mat\tDMS_Test_RN_Mat\n\nNote: Text has been cut off after 15 characters",
        Size = new Size(600, 450)
      });
    }

    [TestMethod, Timeout(1000)]
    public void ResizeForm()
    {
      UnitTestStaticForms.ShowForm(() => new ResizeForm());
    }

    [TestMethod, Timeout(1000)]
    public void FormTextDisplay()
    {
      UnitTestStaticForms.ShowForm(() => new FormTextDisplay("This is a test text\nSpanning some\nlines ..."));
    }

    [TestMethod, Timeout(1000)]
    public void FormTextDisplayJson()
    {
      UnitTestStaticForms.ShowForm(() => new FormTextDisplay("{\r\n  \"schema\": {\r\n    \"properties\": {\r\n      \"EmployeeIdentificationData_GUID\": {\r\n        \"format\": \"uuid\",\r\n        \"trim\": \"leftRight\",\r\n        \"caseSensitive\": false,\r\n        \"title\": \"GUID\",\r\n        \"description\": \"Unique identifier for a user. This cannot be changed\",\r\n        \"type\": \"string\"\r\n      }  }\r\n}}"));
    }

    [TestMethod, Timeout(1000)]
    public void FormTextDisplayXMl()
    {
      UnitTestStaticForms.ShowForm(() => new FormTextDisplay("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<edmx:Edmx Version=\"4.0\" xmlns:edmx=\"http://docs.oasis-open.org/odata/ns/edmx\">\r\n  <edmx:DataServices>  </edmx:DataServices>\r\n</edmx:Edmx>"));
    }

    [TestMethod, Timeout(1000)]
    public void QuotingControl1()
    {
      UnitTestStaticForms.ShowControl(() =>
      {
        var ctrl = new QuotingControl();
        ctrl.CsvFile = new CsvFile(id: "CSV", fileName: "");
        return ctrl;
      });
    }

    [TestMethod, Timeout(1000)]
    public void QuotingControl2()
    {
      UnitTestStaticForms.ShowControl(() => new QuotingControl(), 0.1, control => control.CsvFile = new CsvFile(id: "CSV", fileName: ""));
    }

    [TestMethod, Timeout(2000)]
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

    [TestMethod, Timeout(2000)]
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

    [TestMethod, Timeout(2000)]
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

    [TestMethod, Timeout(2000)]
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

    [TestMethod, Timeout(2000)]
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

    [TestMethod, Timeout(3000)]
    public void TimedMessage()
    {
      Extensions.RunStaThread(() =>
      {
        using (var tm = new TimedMessage())
        {
          tm.ShowDialog("This is my message", "Title1", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1,
            .5, null, null, null);
        }

        using (var tm = new TimedMessage())
        {
          tm.ShowDialog("This is another message\n with a linefeed", "Title12", MessageBoxButtons.YesNo,
            MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button2, 0.5, null, null, null);
        }
      });
    }

    [TestMethod, Timeout(1000)]
    public void SearchShow() => UnitTestStaticForms.ShowControl(() => new Search());

    [TestMethod, Timeout(1000)]
    public void ShowSelectFont() => UnitTestStaticForms.ShowControl(() => new SelectFont());

    [TestMethod, Timeout(1000)]
    public void ShowLoggerDisplay() => UnitTestStaticForms.ShowControl(() => new LoggerDisplay());

    [TestMethod, Timeout(1000)]
    public void FillGuessSettingEditShow() => UnitTestStaticForms.ShowControl(() => new FillGuessSettingEdit());

    [TestMethod, Timeout(2000)]
    public void FormDuplicatesDisplay()
    {
      using var dataTable = UnitTestStaticData.GetDataTable(60);

      UnitTestStaticForms.ShowForm(() => new FormDuplicatesDisplay(dataTable, dataTable.Select(),
        dataTable.Columns[0].ColumnName,
        HtmlStyle.Default));
    }

    [TestMethod, Timeout(2000)]
    public void FormUniqueDisplay()
    {
      using var dataTable = UnitTestStaticData.GetDataTable(60);
      UnitTestStaticForms.ShowForm(() => new FormUniqueDisplay(dataTable, dataTable.Select(), dataTable.Columns[0].ColumnName,
        HtmlStyle.Default));
    }

    [TestMethod, Timeout(1000)]
    public void FormShowMaxLength()
    {
      using var dataTable = UnitTestStaticData.GetDataTable(60);
      UnitTestStaticForms.ShowForm((Func<Form>) (() =>
        new FormShowMaxLength(dataTable, dataTable.Select(), new List<string>(), HtmlStyle.Default)));
    }

    //[TestMethod, Timeout(1000)]
    //public void DataGridViewColumnFilterControl()
    //{
    //  using var dataTable = UnitTestStaticData.GetDataTable(60);
    //  UnitTestStaticForms.ShowControl(() => new DataGridViewColumnFilterControl(new DataGridViewTextBoxColumn
    //  {
    //    ValueType = dataTable.Columns[0].DataType,
    //    Name = dataTable.Columns[0].ColumnName,
    //    DataPropertyName = dataTable.Columns[0].ColumnName
    //  }));
    //}
  }
}