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
    private readonly HTMLStyle m_HTMLStyle = new HTMLStyle();

    [TestMethod]
    [Timeout(8000)]
    public void PersistentChoice()
    {
      var pc = new PersistentChoice(DialogResult.Yes);
      _MessageBox.PersistentChoice("message", "Title", pc, "Yes", "no");
    }

    [TestMethod]
    [Timeout(10000)]
    public void HTMLDisplay()
    {
      Extensions.RunSTAThread(() =>
      {
        using (var tm = new TimedMessage())
        {
          var stringBuilder = HTMLStyle.StartHTMLDoc($"{SystemColors.Control.R:X2}{SystemColors.Control.G:X2}{SystemColors.Control.B:X2}", "");
          stringBuilder.Append(string.Format(UnitTestInitializeWin.HTMLStyle.H2, HTMLStyle.TextToHtmlEncode("Sample")));
          stringBuilder.Append(string.Format(UnitTestInitializeWin.HTMLStyle.H2, HTMLStyle.TextToHtmlEncode("Sample2")));

          stringBuilder.AppendLine(UnitTestInitializeWin.HTMLStyle.TableOpen);
          stringBuilder.AppendLine(UnitTestInitializeWin.HTMLStyle.TROpen);
          for (var index = 1; index <= 10; index++)
          {
            stringBuilder.AppendLine(string.Format(UnitTestInitializeWin.HTMLStyle.TD,
              HTMLStyle.TextToHtmlEncode("Test " + index.ToString())));
            if (index % 4 == 0)
            {
              stringBuilder.AppendLine(UnitTestInitializeWin.HTMLStyle.TRClose);
            }
          }

          stringBuilder.AppendLine(UnitTestInitializeWin.HTMLStyle.TRClose);
          stringBuilder.AppendLine(UnitTestInitializeWin.HTMLStyle.TableClose);
          stringBuilder.AppendLine(UnitTestInitializeWin.HTMLStyle.TableClose);
          stringBuilder.AppendLine(UnitTestInitializeWin.HTMLStyle.TRClose);
          stringBuilder.AppendLine(UnitTestInitializeWin.HTMLStyle.TableClose);
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
#pragma warning disable CS8625 // Ein NULL-Literal kann nicht in einen Non-Nullable-Verweistyp konvertiert werden.
					WindowsAPICodePackWrapper.Save(FileSystemUtils.ExecutableDirectoryName(), "Test", null, null), token));
#pragma warning restore CS8625 // Ein NULL-Literal kann nicht in einen Non-Nullable-Verweistyp konvertiert werden.
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
			Extensions.RunSTAThread(() =>
			{
				using (var tm = new TimedMessage())
				{
					tm.ShowDialog("This is my message", "Title1", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1,
						2, null, null, null);
				}

				using (var tm = new TimedMessage())
				{
					tm.ShowDialog("This is another message\n with a linefeed", "Title12", MessageBoxButtons.YesNo, MessageBoxIcon.Error,
						MessageBoxDefaultButton.Button2, 2, null, null, null);
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
    public void FormDuplicatesDisplay()
    {
      using (var dataTable = UnitTestStatic.GetDataTable(60))
      using (var form = new FormDuplicatesDisplay(dataTable, dataTable.Select(), dataTable.Columns[0].ColumnName, UnitTestInitializeWin.HTMLStyle))
      {
        UnitTestWinFormHelper.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormUniqueDisplay()
    {
      using (var dataTable = UnitTestStatic.GetDataTable(60))
      using (var form = new FormUniqueDisplay(dataTable, dataTable.Select(), dataTable.Columns[0].ColumnName, UnitTestInitializeWin.HTMLStyle))
      {
        UnitTestWinFormHelper.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    [Timeout(3000)]
    public void FormShowMaxLength()
    {
      using (var dataTable = UnitTestStatic.GetDataTable(60))
      using (var form = new FormShowMaxLength(dataTable, dataTable.Select(), new List<string>(), UnitTestInitializeWin.HTMLStyle))
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