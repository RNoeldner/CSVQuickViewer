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
    [Timeout(10000)]
    public void FormMain_BasicCSV()
    {
      var vs = new ViewSettings
      {
        DisplayRecordNo =true,
        MenuDown = true
      };
      using var frm = new FormMain(vs);
      UnitTestStatic.ShowFormAndClose(frm, 0, frm => frm.LoadCsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt.gz"), UnitTestStatic.Token), 8, UnitTestStatic.Token);
      Assert.IsNotNull(frm.DataTable);
      Assert.AreEqual(7, frm.DataTable.Rows.Count);
    }

    [TestMethod]
    [Timeout(10000)]
    public void FormMain_AllFormatsPipe()
    {
      var vs = new ViewSettings();
      using var frm = new FormMain(new ViewSettings());
      UnitTestStatic.ShowFormAndClose(frm, 0, frm => frm.LoadCsvFile(UnitTestStatic.GetTestPath("AllFormatsPipe.txt"), UnitTestStatic.Token), 8, UnitTestStatic.Token);
      Assert.IsNotNull(frm.DataTable);
      // 45 records, one of the lines has a linefeed
      Assert.IsTrue(frm.DataTable.Rows.Count >= 46);
    }
  }
}