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
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class ReaderExtensionMethodsTest
  {
    private readonly CsvFile m_ValidSetting = new CsvFile
    {
      FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"),
      FileFormat = {FieldDelimiter = ",", CommentLine = "#"}
    };

    [TestInitialize]
    public void Init()
    {
      m_ValidSetting.ColumnCollection.AddIfNew(new Column("Score", DataType.Integer));
      m_ValidSetting.ColumnCollection.AddIfNew(new Column("Proficiency", DataType.Numeric));
      m_ValidSetting.ColumnCollection.AddIfNew(new Column("IsNativeLang", DataType.Boolean));
      var cf = new Column("ExamDate", DataType.DateTime);
      cf.ValueFormatMutable.DateFormat = @"dd/MM/yyyy";
      m_ValidSetting.ColumnCollection.AddIfNew(cf);
    }


    [TestMethod]
    public async Task GetEmptyColumnHeaderAsyncTest()
    {
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        using (var test = new CsvFileReader(m_ValidSetting, processDisplay))
        {
          await test.OpenAsync(processDisplay.CancellationToken);
          var result = await test.GetEmptyColumnHeaderAsync(processDisplay.CancellationToken);
          Assert.AreEqual(0, result.Count);
        }
      }
    }

    [TestMethod]
    public async Task GetDataTableAsync2()
    {
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        var test2 = (CsvFile) m_ValidSetting.Clone();
        test2.RecordLimit = 4;
        using (var test = new CsvFileReader(test2, processDisplay))
        {
          await test.OpenAsync(processDisplay.CancellationToken);

          var dt = await test.GetDataTableAsync(-1, false, false, false, false, false, null,
            processDisplay.CancellationToken);
          Assert.AreEqual(test2.RecordLimit, dt.Rows.Count);
        }
      }
    }

    [TestMethod]
    public async Task GetDataTableAsync3()
    {
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        var test3 = new CsvFile(UnitTestInitializeCsv.GetTestPath("WithEoFChar.txt"))
        {
          FileFormat = {FieldDelimiter = "TAB"}
        };
        test3.ColumnCollection.Add(new Column("Memo") {Ignore = true});
        using (var test = new CsvFileReader(test3, processDisplay))
        {
          await test.OpenAsync(processDisplay.CancellationToken);

          var dt = await test.GetDataTableAsync(-1, true, true, true, true, true, null,
            processDisplay.CancellationToken);
          // 10 columns 1 ignored one added for Start line one for Error Field one for Record No one
          // for Line end
          Assert.AreEqual((10 - 1) + 4, dt.Columns.Count);
          Assert.AreEqual(19, dt.Rows.Count);
        }
      }
    }
  }
}