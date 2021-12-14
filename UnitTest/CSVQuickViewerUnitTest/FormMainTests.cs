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

namespace CsvTools.Tests
{
  [TestClass]
  public sealed class FormMainTests
  {
    [TestMethod]
    [Timeout(25000)]
    public async System.Threading.Tasks.Task FormMain_BasicCSVAsync()
    {
      using var frm = new FormMain(new ViewSettings());
      await UnitTestStatic.ShowFormAndCloseAsync(frm, .2, frm.LoadCsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt.gz"), UnitTestStatic.Token),
        UnitTestStatic.Token);

      Assert.IsNotNull(frm.DataTable);
      Assert.AreEqual(7, frm.DataTable.Rows.Count);
    }

    [TestMethod]
    [Timeout(25000)]
    public async System.Threading.Tasks.Task FormMain_AllFormatsPipeAsync()
    {
      using var frm = new FormMain(new ViewSettings());
      await UnitTestStatic.ShowFormAndCloseAsync(frm, .1, frm.LoadCsvFile(UnitTestStatic.GetTestPath("AllFormatsPipe.txt"), UnitTestStatic.Token),
        UnitTestStatic.Token);
      Assert.IsNotNull(frm.DataTable);
      // 45 records, one of the lines has a linefeed
      Assert.IsTrue(frm.DataTable.Rows.Count >= 46);
    }
  }
}