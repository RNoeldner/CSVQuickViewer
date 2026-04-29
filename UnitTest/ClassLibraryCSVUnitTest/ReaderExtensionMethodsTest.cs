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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CsvTools.Tests;

[TestClass]
[SuppressMessage("ReSharper", "UseAwaitUsing")]
public class ReaderExtensionMethodsTest
{

  [TestMethod]
  public async Task GetColumnsOfReaderTest()
  {
    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("BasicCSV.txt"), 65001, 0, 0, true, null,
                                       TrimmingOptionEnum.Unquoted, ',', '"', char.MinValue, 0, false, false, "#", 0,
                                       true, "", "", "", true, false, false, true, true, false, true, true, true, true,
                                       false, "NULL", true, 4, "", TimeZoneInfo.Local.Id, true, false);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(6, test.GetColumnsOfReader().Count());
  }


  [TestMethod]
  public async Task GetDataTable_BasicCSV_RecordLimit()
  {
    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("BasicCSV.txt"), recordLimit:4);
    await test.OpenAsync(UnitTestStatic.Token);

    var dt = await test.GetDataTableAsync(TimeSpan.FromSeconds(30), false,
      false, false, false, UnitTestStatic.TesterProgress);
    Assert.AreEqual(4, dt.Rows.Count);
  }

  [TestMethod]
  public async Task GetDataTable_WithEoFChar()
  {
    var columnDefinition = new[] { new Column("Memo", ValueFormat.Empty, ignore: true) };
    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("WithEoFChar.txt"),
      fieldDelimiterChar: '\t');
    await test.OpenAsync(UnitTestStatic.Token);

    using var dt = await test.GetDataTableAsync(TimeSpan.FromMinutes(1), true, true, true, true, UnitTestStatic.TesterProgress);
    // 10 columns + Start line + Error Field + Record No + Line end
    Assert.AreEqual(10 + 4, dt.Columns.Count);
    Assert.AreEqual(19, dt.Rows.Count);
  }

  [TestMethod]
  public async Task GetDataTable_WithEoFChar_Ignore()
  {
    var columnDefinition = new[] { new Column("Memo", ValueFormat.Empty, ignore: true) };
    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("WithEoFChar.txt"), 
      columnDefinition: columnDefinition, fieldDelimiterChar: '\t');
    await test.OpenAsync(UnitTestStatic.Token);

    // An ignored columns will be read in the reader

    // With one ignored columend we should not see a misalligedn column
    // But right now we do...
    // Error: Input string was not in a correct format.Couldn't store <08:42:27> in #Record Column.  Expected type is Int64.
    using var dt = await test.GetDataTableAsync(TimeSpan.FromMinutes(1), true, true, true, true, UnitTestStatic.TesterProgress);
    // 10 columns 1 ignored one added for Start line one for Error Field one for Record No one for
    // Line end
    Assert.AreEqual(10 -1 + 4, dt.Columns.Count);

    Assert.AreEqual(19, dt.Rows.Count);
  }
}