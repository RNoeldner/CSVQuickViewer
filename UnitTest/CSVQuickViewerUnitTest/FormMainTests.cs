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

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class FormMainTests
  {
    [TestMethod]
    public void FormMain_BasicCSV()
    {
      UnitTestWinFormHelper.RunSTAThread(() =>
      {
        using (var frm = new FormMain(Path.Combine(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt.gz"))))
        {
          UnitTestWinFormHelper.ShowFormAndClose(frm, 1, () =>
          {
            while (!frm.LoadFinished && !UnitTestInitializeCsv.Token.IsCancellationRequested)
              UnitTestWinFormHelper.WaitSomeTime(.2);
          });
          Assert.IsNotNull(frm.DataTable);
          Assert.AreEqual(7, frm.DataTable.Rows.Count);
        }
      });
    }

    [TestMethod]
    public void FormMain_AllFormatsPipe()
    {
      UnitTestWinFormHelper.RunSTAThread(() =>
      {
        using (var frm = new FormMain(UnitTestInitializeCsv.GetTestPath("AllFormatsPipe.txt")))
        {
          UnitTestWinFormHelper.ShowFormAndClose(frm, 1, () =>
          {
            while (!frm.LoadFinished && !UnitTestInitializeCsv.Token.IsCancellationRequested)
              UnitTestWinFormHelper.WaitSomeTime(.2);
          });
          Assert.IsNotNull(frm.DataTable);
          // 45 records, one of the lines has a linefeed
          Assert.AreEqual(46, frm.DataTable.Rows.Count);
        }
      });
    }
  }
}