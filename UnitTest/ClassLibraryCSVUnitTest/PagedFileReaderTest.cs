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
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace CsvTools.Tests
{
  [TestClass()]
  public class PagedFileReaderTest
  {
    private static readonly TimeZoneChangeDelegate m_TimeZoneAdjust = StandardTimeZoneAdjust.ChangeTimeZone;

    [TestMethod()]
    public async Task MoveToLastPageFirstPageAsync()
    {
      const int pageSize = 20;
      const int numRec = 1065;
      // 1065 / 17
      using (var reader = new CsvFileReader(UnitTestStatic.GetTestPath("AllFormats.txt"),
               65001, 0, true, null, TrimmingOptionEnum.Unquoted, '\t',
               '"', char.MinValue, 0, false,
               false, "", 0,
               true, "", "",
               "", true, false,
               true, false, false,
               false, false, false, false,
               false, false, "",
               true, 1, "ID", m_TimeZoneAdjust, System.TimeZoneInfo.Local.Id, true, false))
      {
        var test = new PagedFileReader(reader, pageSize);
        await test.OpenAsync(true, true, true, true, UnitTestStatic.Token);

        await test.MoveToLastPageAsync(UnitTestStatic.Token);
        Assert.AreEqual((numRec / pageSize) + 1, test.PageIndex);
        Assert.AreEqual(numRec % pageSize, test.Count);

        await test.MoveToFirstPageAsync(UnitTestStatic.Token);
        Assert.AreEqual(pageSize, test.Count);
      }
    }

    [TestMethod()]
    public async Task MoveToNextPageAsync()
    {
      const int pageSize = 20;
      // 1065 / 17
      using (var reader = new CsvFileReader(UnitTestStatic.GetTestPath("AllFormats.txt"),
               65001, 0, true, null, TrimmingOptionEnum.Unquoted, '\t',
               '"', char.MinValue, 0, false,
               false, "", 0,
               true, "", "",
               "", true, false,
               true, false, false,
               false, false, false, false,
               false, false, "",
               true, 1, "ID", m_TimeZoneAdjust, System.TimeZoneInfo.Local.Id, true, false))
      {
        var test = new PagedFileReader(reader, pageSize);
        await test.OpenAsync(true, true, true, true, UnitTestStatic.Token);
        Assert.AreEqual(pageSize, test.Count);

        var collectionChangedCalled = false;
        test.CollectionChanged += (e_, s_) => { collectionChangedCalled = true; };
        await test.MoveToNextPageAsync(UnitTestStatic.Token);
        Assert.IsTrue(collectionChangedCalled);
        Assert.AreEqual(pageSize, test.Count);
      }
    }

    [TestMethod()]
    public async Task MoveToPreviousPageAsync()
    {
      const int pageSize = 33;
      // 1065 / 17
      using (var reader = new CsvFileReader(UnitTestStatic.GetTestPath("AllFormats.txt"),
               65001, 0, true, null, TrimmingOptionEnum.Unquoted, '\t',
              '"', char.MinValue, 0, false,
               false, "", 0,
               true, "", "",
               "", true, false,
               true, false, false,
               false, false, false, false,
               false, false, "",
               true, 1, "ID", m_TimeZoneAdjust, System.TimeZoneInfo.Local.Id, true, false))
      {
        var test = new PagedFileReader(reader, pageSize);
        await test.OpenAsync(true, true, true, true, UnitTestStatic.Token);
        Assert.AreEqual(pageSize, test.Count);

        await test.MoveToNextPageAsync(UnitTestStatic.Token);
        Assert.AreEqual(pageSize, test.Count);
        await test.MoveToPreviousPageAsync(UnitTestStatic.Token);
        Assert.AreEqual(pageSize, test.Count);
      }
    }

    [TestMethod()]
    public async Task PagedFileReaderOpen()
    {
      using (var reader = new CsvFileReader(UnitTestStatic.GetTestPath("AllFormats.txt"),
               65001, 0, true, null, TrimmingOptionEnum.Unquoted, '\t',
               '"', char.MinValue, 0, false,
               false, "", 0,
               true, "", "",
               "", true, false,
               true, false, false,
               false, false, false, false,
               false, false, "",
               true, 1, "ID", m_TimeZoneAdjust, System.TimeZoneInfo.Local.Id, true, true))
      {
        var test = new PagedFileReader(reader, 20);
        await test.OpenAsync(true, true, true, true, UnitTestStatic.Token);
        Assert.AreEqual(20, test.Count);

        // get the value in row 1 for the property TZ
        // Assert.AreEqual("CET", testValue.TZ);

        test.Close();
      }
    }
  }
}
