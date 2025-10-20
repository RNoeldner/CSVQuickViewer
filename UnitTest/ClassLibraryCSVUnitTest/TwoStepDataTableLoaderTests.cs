/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using System.Threading.Tasks;


namespace CsvTools.Tests
{
  [TestClass()]
  public class TwoStepDataTableLoaderTests
  {

    [TestMethod()]
    public async Task StartAsyncTestAsyncNoWarning()
    {
      bool warningCalled = false;

      // ReSharper disable once UseAwaitUsing
      using var tsde = new SteppedDataTableLoader();
      var csv = new CsvFileDummy
      {
        FileName = UnitTestStatic.GetTestPath("BasicCSV.txt"),
        FieldDelimiterChar = ',',
        CommentLine = "#"
      };

      var proc = new Progress<ProgressInfo>();
      var myDataTable = await tsde.StartAsync(csv, TimeSpan.FromMilliseconds(20), proc,
        (o, a) => { warningCalled = true; }, UnitTestStatic.Token);
      Assert.IsFalse(warningCalled);
      Assert.AreEqual(7, myDataTable.Columns.Count());
    }

    [TestMethod()]
    public async Task StartAsyncTestAsyncWarning()
    {
      bool warningCalled = false;

      // ReSharper disable once UseAwaitUsing
      using var tsde = new SteppedDataTableLoader();
      var csv = new CsvFileDummy
      {
        FileName = UnitTestStatic.GetTestPath("TextQualifiers.txt"),
        WarnQuotesInQuotes = true
      };

      var proc = new Progress<ProgressInfo>();
      var myDataTable = await tsde.StartAsync(csv, TimeSpan.FromMilliseconds(20), proc,
        (o, a) => { warningCalled = true; }, UnitTestStatic.Token);
      Assert.IsTrue(warningCalled);
      Assert.AreEqual(7, myDataTable.Columns.Count());
    }
  }
}
