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
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public sealed class FormMainTests
  {
    [TestMethod]
    [Timeout(5100)]
    public void ProgramMain()
    {
      var tcs = new CancellationTokenSource(TimeSpan.FromSeconds(5));
      Task.Run(() => Extensions.RunStaThread(()=>Program.Main(Array.Empty<string>())), tcs.Token);
    }


    [TestMethod]
    //[Timeout(8000)]
    public void FormMain_LoadCsvFileAsync_CSV()
    {
      var fileToLoad = UnitTestStatic.GetTestPath("BasicCSV.txt.gz");
      Assert.IsTrue(FileSystemUtils.FileExists(fileToLoad), "Source files exists");
      UnitTestStaticForms.ShowFormAsync(
        () => new FormMain(new ViewSettings { DisplayRecordNo = true, MenuDown = true }),
        async frm =>
        {
          await frm.LoadCsvFileAsync(fileToLoad, UnitTestStatic.Token);
          Assert.IsNotNull(frm.DataTable);
          Assert.AreEqual(7, frm.DataTable.Rows.Count);
        });
    }

    [TestMethod]
    [Timeout(2000)]
    public void FormMain_LoadCsvFileAsync_AllFormatsPipe()
    {
      var fileToLoad = UnitTestStatic.GetTestPath("AllFormatsPipe.txt");
      Assert.IsTrue(FileSystemUtils.FileExists(fileToLoad), "Source files exists");
      UnitTestStaticForms.ShowFormAsync(() => new FormMain(new ViewSettings()), async frm =>
        {
          await frm.LoadCsvFileAsync(fileToLoad, UnitTestStatic.Token);
          Assert.IsNotNull(frm.DataTable);
          // 45 records, one of the lines has a linefeed
          Assert.IsTrue(frm.DataTable.Rows.Count >= 40);
        });
    }
  }
}