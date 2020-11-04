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
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class DetermineColumnFormatTest
  {
    [TestMethod]
    [Timeout(2000)]
    public async Task TestJson()
    {
      var setting =
        new CsvFile(UnitTestInitializeCsv.GetTestPath("Larger.json")) { JsonFormat = true };


      var fillGuessSettings = new FillGuessSettings
      {
        DetectNumbers = true,
        DetectDateTime = true,
        DetectPercentage = true,
        DetectBoolean = true,
        DetectGUID = true,
        IgnoreIdColumns = true
      };
      Assert.AreEqual(0, setting.ColumnCollection.Count);

      await setting.FillGuessColumnFormatReaderAsync(false, true, fillGuessSettings, UnitTestInitializeCsv.Token);

      var expected =
        new Dictionary<string, DataType>
        {
          {"object_id", DataType.Guid},
          {"_last_touched_dt_utc", DataType.DateTime},
          {"classification_id", DataType.Guid},
          {"email_option_id", DataType.Integer}
        };

      foreach (var keyValue in expected)
      {
        var indexCol = setting.ColumnCollection.FirstOrDefault(x =>
          x.Name.Equals(keyValue.Key, StringComparison.InvariantCultureIgnoreCase));
        Assert.IsNotNull(indexCol, $"Column {keyValue.Key} not recognized");
        Assert.AreEqual(keyValue.Value, indexCol.ValueFormat.DataType);
      }
    }

    [TestMethod]
    public async Task GetSourceColumnInformationTestAsync()
    {
      var setting = new CsvFile
      {
        ID = "ID122",
        FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"),
        HasFieldHeader = true,
        DisplayStartLineNo = false,
        SqlStatement = "ID122",
        FileFormat = { FieldDelimiter = "," }
      };
      using (var reader = new CsvFileReader(setting, null))
      {
        UnitTestInitializeCsv.MimicSQLReader.AddSetting(setting.ID,
          await reader.GetDataTableAsync(0, false, setting.DisplayStartLineNo, setting.DisplayRecordNo,
            setting.DisplayEndLineNo, false, null, UnitTestInitializeCsv.Token));
      }

      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        var res1 = await DetermineColumnFormat.GetWriterColumnInformationAsync(setting.SqlStatement, setting.Timeout,
          setting.FileFormat.ValueFormatMutable, setting.ColumnCollection.ReadonlyCopy(),
          processDisplay.CancellationToken);
        Assert.AreEqual(6, res1.Count());
        setting.SqlStatement = null;

        var res2 = await DetermineColumnFormat.GetSqlColumnNamesAsync(setting.SqlStatement, setting.Timeout,
          processDisplay.CancellationToken);
        Assert.AreEqual(0, res2.Count());
      }
    }

    [TestMethod]
    public async Task GetSampleValuesAsync()
    {
      using (var dataTable = UnitTestStatic.GetDataTable(150, false))
      {
        using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
        {
          using (var reader = new DataTableWrapper(dataTable))
          {
            var res = await DetermineColumnFormat
              .GetSampleValuesAsync(reader, 100, new[] { 0, 1 }, 20, null, processDisplay.CancellationToken)
              .ConfigureAwait(false);
            Assert.AreEqual(20, res[0].Values.Count);
          }
        }
      }
    }

    [TestMethod]
    public void GetAllPossibleFormats()
    {
      var res = DetermineColumnFormat.GetAllPossibleFormats("17-12-2012");
      Assert.IsTrue(res.Any(x => x.DateFormat.Equals("dd/MM/yyyy") && x.DateSeparator.Equals("-")));
    }

    [TestMethod]
    public async Task GetSourceColumnInformationTest2Async()
    {
      var setting = new CsvFile { ID = "ID122", FileFormat = { FieldDelimiter = "," } };
      try
      {
        using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
          await DetermineColumnFormat.GetWriterColumnInformationAsync("setting.SqlStatement", 60,
            setting.FileFormat.ValueFormatMutable, setting.ColumnCollection.ReadonlyCopy(),
            processDisplay.CancellationToken);

        Assert.Fail("Invalid SQL should have caused error ");
      }
      catch (Exception)
      {
        // good
      }
    }


    [TestMethod]
    public async Task GetSourceColumnInformationTestAsync2()
    {
      using (var dt = UnitTestStatic.GetDataTable())
      {
        using (var reader = new DataTableWrapper(dt))
        {
          var fillGuessSettings = new FillGuessSettings
          {
            DetectNumbers = true,
            DetectDateTime = true,
            DetectPercentage = true,
            DetectBoolean = true,
            DetectGUID = true,
            IgnoreIdColumns = true
          };

          var columnCollection = new ColumnCollection();
          //await reader.OpenAsync(processDisplay.CancellationToken);

          var (information, columns) =  await reader.FillGuessColumnFormatReaderAsyncReader(fillGuessSettings,
            columnCollection, false, true, "<NULL>", UnitTestInitializeCsv.Token);

          Assert.AreEqual(7, columns.Count(), "Recognized columns");
          Assert.AreEqual(8, information.Count, "Information Lines");

          var (information2, columns2) =   await reader.FillGuessColumnFormatReaderAsyncReader(fillGuessSettings,
            columnCollection, true, true, "<NULL>", UnitTestInitializeCsv.Token);
          Assert.AreEqual(11, columns2.Count());
          // Added 4 text columns,
          Assert.AreEqual(9, information2.Count);
        }
      }
    }

    [TestMethod]
    public async Task GetSourceColumnInformationTestAsync_Parameter()
    {
      try
      {
        // ReSharper disable once AssignNullToNotNullAttribute
        await DetermineColumnFormat.GetWriterColumnInformationAsync("Nonsense SQL", 60, null, new List<IColumn>(),
          UnitTestInitializeCsv.Token);

        Assert.Fail("Expected Exception not thrown");
      }
      catch (ArgumentNullException)
      {
        // add good
      }
      catch (Exception ex)
      {
        Assert.Fail("Wrong Exception Type: " + ex.GetType());
      }
    }

    [TestMethod]
    public async Task GetSqlColumnNamesAsyncParameter()
    {
      using (var dummy = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        var backup = FunctionalDI.SQLDataReader;
        try
        {
          FunctionalDI.SQLDataReader = null;
          await DetermineColumnFormat.GetSqlColumnNamesAsync("Nonsense SQL", 60, dummy.CancellationToken);

          Assert.Fail("Expected Exception not thrown");
        }
        catch (FileWriterException)
        {
          // add good
        }
        catch (Exception ex)
        {
          Assert.Fail("Wrong Exception Type: " + ex.GetType());
        }
        finally
        {
          FunctionalDI.SQLDataReader = backup;
        }
      }
    }

    [TestMethod]
    public void GetAllPossibleFormatsTest()
    {
      var res1 = DetermineColumnFormat.GetAllPossibleFormats("1/1/2020");
      Assert.AreEqual(2, res1.Count());

      var res2 = DetermineColumnFormat.GetAllPossibleFormats("01/01/2020");
      Assert.AreEqual(4, res2.Count());

      var res3 = DetermineColumnFormat.GetAllPossibleFormats("30/1/2020");
      Assert.AreEqual(1, res3.Count());
    }

    [TestMethod]
    public async Task FillGuessColumnFormatReaderTestAsync()
    {
      var setting = new CsvFile
      {
        ID = "DetermineColumnFormatFillGuessColumnFormatWriter",
        FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"),
        HasFieldHeader = true,
        FileFormat = { FieldDelimiter = "," }
      };
      var fillGuessSettings = new FillGuessSettings
      {
        DetectNumbers = true,
        DetectDateTime = true,
        DetectPercentage = true,
        DetectBoolean = true,
        DetectGUID = true,
        IgnoreIdColumns = false
      };
      var result1 =
        await setting.FillGuessColumnFormatReaderAsync(false, true, fillGuessSettings, UnitTestInitializeCsv.Token);
      Assert.AreEqual(5, result1.Count);

      var result2 =
        await setting.FillGuessColumnFormatReaderAsync(true, false, fillGuessSettings, UnitTestInitializeCsv.Token);
      Assert.AreEqual(6, result2.Count);
    }

    [TestMethod]
    public async Task DetermineColumnFormatFillGuessColumnFormatWriterAsync()
    {
      var setting = new CsvFile
      {
        ID = "DetermineColumnFormatFillGuessColumnFormatWriter",
        FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"),
        DisplayStartLineNo = false,
        HasFieldHeader = true,
        FileFormat = { FieldDelimiter = "," }
      };
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting, null))
      {
        await reader.OpenAsync(processDisplay.CancellationToken);
        UnitTestInitializeCsv.MimicSQLReader.AddSetting(setting.ID,
          await reader.GetDataTableAsync(0, false, setting.DisplayStartLineNo, setting.DisplayRecordNo,
            setting.DisplayEndLineNo, false, null, UnitTestInitializeCsv.Token));
      }

      var writer = new CsvFile { SqlStatement = setting.ID };

      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        await writer.FillGuessColumnFormatWriterAsync(true, processDisplay);
        Assert.AreEqual(6, writer.ColumnCollection.Count);
      }
    }

    [TestMethod]
    public async Task DetermineColumnFormatGetSampleRollOverAsync()
    {
      using (var dt = new DataTable())
      {
        dt.Columns.Add(new DataColumn { ColumnName = "ID", DataType = typeof(Guid) });
        for (var i = 0; i < 30; i++)
        {
          var row = dt.NewRow();

          // add two empty rows
          if (i % 5 != 0)
            row[0] = Guid.NewGuid();
          dt.Rows.Add(row);
        }

        using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
        {
          using (var reader = new DataTableWrapper(dt))
          {
            // Move the reader to a late record
            for (var i = 0; i < dt.Rows.Count / 2; i++)
              await reader.ReadAsync(processDisplay.CancellationToken);
            string treatAsNull = string.Empty;
            var res = await DetermineColumnFormat.GetSampleValuesAsync(reader, 0, new[] { 0 }, 100,
              treatAsNull, processDisplay.CancellationToken).ConfigureAwait(false);
            Assert.AreEqual(dt.Rows.Count, res[0].RecordsRead, "RecordsRead");
            Assert.AreEqual(dt.Rows.Count - (dt.Rows.Count / 5), res[0].Values.Count());
          }
        }
      }
    }

    [TestMethod]
    public async Task DetermineColumnFormatGetSampleValues2Async()
    {
      using (var dt = new DataTable())
      {
        dt.Columns.Add(new DataColumn { ColumnName = "ID", DataType = typeof(string) });
        dt.Columns.Add(new DataColumn { ColumnName = "ID2", DataType = typeof(string) });
        for (var i = 0; i < 150; i++)
        {
          var row = dt.NewRow();
          if (i == 10 || i == 47)
            row[0] = "NULL";
          row[0] = (i / 3).ToString(CultureInfo.InvariantCulture);
          row[1] = (i * 7).ToString(CultureInfo.InvariantCulture);
          dt.Rows.Add(row);
        }

        using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
        {
          using (var reader = new DataTableWrapper(dt))
          {
            string treatAsNull = string.Empty;
            var res = await DetermineColumnFormat.GetSampleValuesAsync(reader, 0, new[] { 0, 1 }, 20,
              treatAsNull, processDisplay.CancellationToken);

            Assert.IsTrue(res[0].RecordsRead >= 20);
            Assert.IsTrue(res[1].RecordsRead >= 20);
            Assert.AreEqual(20, res[0].Values.Count());
            Assert.AreEqual(20, res[1].Values.Count());
          }
        }
      }
    }

    [TestMethod]
    public async Task DetermineColumnFormatGetSampleValuesAsync2Async()
    {
      using (var dt = new DataTable())
      {
        dt.Columns.Add(new DataColumn { ColumnName = "ID", DataType = typeof(string) });
        for (var i = 0; i < 150; i++)
        {
          var row = dt.NewRow();
          if (i == 10 || i == 47)
            row[0] = "NULL";
          row[0] = (i / 3).ToString(CultureInfo.InvariantCulture);
          dt.Rows.Add(row);
        }

        using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
        {
          using (var reader = new DataTableWrapper(dt))
          {
            string treatAsNull = string.Empty;
            var res = await DetermineColumnFormat.GetSampleValuesAsync(reader, 0, new[] { 0 }, 20,
              treatAsNull, processDisplay.CancellationToken);
            Assert.IsTrue(res[0].RecordsRead >= 20);
            Assert.AreEqual(20, res[0].Values.Count());
          }
        }
      }
    }

    [TestMethod]
    public async Task DetermineColumnFormatGetSampleValuesNoColumns()
    {
      using (var dt = new DataTable())
      {
        using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
        {
          using (var reader = new DataTableWrapper(dt))
          {
            var temp = await DetermineColumnFormat
              .GetSampleValuesAsync(reader, 0, new[] { 0 }, 20, null,
                processDisplay.CancellationToken).ConfigureAwait(false);
            Assert.AreEqual(0, temp.Count);
          }
        }
      }
    }

    [TestMethod]
    public async Task FillGuessColumnFormatAddTextColAsync()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"),
        FileFormat = { FieldDelimiter = "," },
        HasFieldHeader = true
      };

      var fillGuessSettings = new FillGuessSettings
      {
        DetectNumbers = true,
        DetectDateTime = true,
        DetectPercentage = true,
        DetectBoolean = true,
        DetectGUID = true,
        IgnoreIdColumns = false
      };
      await setting.FillGuessColumnFormatReaderAsync(true, false, fillGuessSettings, UnitTestInitializeCsv.Token);

      Assert.AreEqual(DataType.Integer, setting.ColumnCollection.Get("ID").ValueFormat.DataType);
      Assert.AreEqual(DataType.DateTime, setting.ColumnCollection.Get("ExamDate").ValueFormat.DataType);
      Assert.AreEqual(DataType.Boolean, setting.ColumnCollection.Get("IsNativeLang").ValueFormat.DataType);
    }

    [TestMethod]
    public async Task FillGuessColumnFormatDatePartsAsync()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitializeCsv.GetTestPath("Sessions.txt"),
        HasFieldHeader = true,
        ByteOrderMark = true,
        FileFormat = { FieldDelimiter = "\t" }
      };
      var fillGuessSettings = new FillGuessSettings
      {
        DetectNumbers = true,
        DetectDateTime = true,
        DetectPercentage = true,
        DetectBoolean = true,
        DetectGUID = true,
        DateParts = true,
        IgnoreIdColumns = true
      };


      await setting.FillGuessColumnFormatReaderAsync(false, true, fillGuessSettings, UnitTestInitializeCsv.Token);


      Assert.AreEqual("Start Date", setting.ColumnCollection[0].Name, "Column 1 Start date");
      Assert.AreEqual("Start Time", setting.ColumnCollection[1].Name, "Column 2 Start Time");
      Assert.AreEqual("Start Time", setting.ColumnCollection[0].TimePart, "TimePart is Start Time");
      Assert.AreEqual(DataType.DateTime, setting.ColumnCollection[0].ValueFormat.DataType);
      Assert.AreEqual("MM/dd/yyyy", setting.ColumnCollection[0].ValueFormat.DateFormat);
      Assert.AreEqual("HH:mm:ss", setting.ColumnCollection[1].ValueFormat.DateFormat);
    }

    [TestMethod]
    public async Task FillGuessColumnFormatDoNotIgnoreIDAsync()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"),
        FileFormat = { FieldDelimiter = "," },
        HasFieldHeader = true
      };

      var fillGuessSettings = new FillGuessSettings
      {
        DetectNumbers = true,
        DetectDateTime = true,
        DetectPercentage = true,
        DetectBoolean = true,
        DetectGUID = true,
        IgnoreIdColumns = false
      };

      await setting.FillGuessColumnFormatReaderAsync(false, false, fillGuessSettings, UnitTestInitializeCsv.Token);


      Assert.AreEqual(DataType.Integer, setting.ColumnCollection.Get("ID").ValueFormat.DataType);
      Assert.AreEqual(DataType.DateTime, setting.ColumnCollection.Get("ExamDate").ValueFormat.DataType);
      Assert.AreEqual(DataType.Boolean, setting.ColumnCollection.Get("IsNativeLang").ValueFormat.DataType);
    }

    [TestMethod]
    public async Task FillGuessColumnFormatGermanDateAndNumbersAsync()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitializeCsv.GetTestPath("DateAndNumber.csv"),
        HasFieldHeader = true,
        FileFormat = { FieldQualifier = "Quote", FieldDelimiter = "Tab" },
        CodePageId = 1252
      };
      var fillGuessSettings = new FillGuessSettings
      {
        DetectNumbers = true,
        DetectDateTime = true,
        DetectPercentage = true,
        DetectBoolean = true,
        DetectGUID = true,
        IgnoreIdColumns = true
      };

      // setting.TreatTextNullAsNull = true;

      await setting.FillGuessColumnFormatReaderAsync(false, false, fillGuessSettings, UnitTestInitializeCsv.Token);


      Assert.IsNotNull(setting.ColumnCollection.Get(@"Betrag Brutto (2 Nachkommastellen)"), "Data Type recognized");

      Assert.AreEqual(
        DataType.Numeric,
        setting.ColumnCollection.Get(@"Betrag Brutto (2 Nachkommastellen)").ValueFormat.DataType,
        "Is Numeric");

      Assert.AreEqual(
        ',',
        setting.ColumnCollection.Get(@"Betrag Brutto (2 Nachkommastellen)").ValueFormat.DecimalSeparatorChar,
        "Decimal Separator found");

      Assert.AreEqual(DataType.DateTime, setting.ColumnCollection.Get(@"Erstelldatum Rechnung").ValueFormat.DataType);
    }

    [TestMethod]
    public async Task FillGuessColumnFormatIgnoreID()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"),
        FileFormat = { FieldDelimiter = "," },
        HasFieldHeader = true
      };

      // setting.TreatTextNullAsNull = true;
      var fillGuessSettings = new FillGuessSettings
      {
        DetectNumbers = true,
        DetectDateTime = true,
        DetectPercentage = true,
        DetectBoolean = true,
        DetectGUID = true,
        IgnoreIdColumns = true
      };
      await setting.FillGuessColumnFormatReaderAsync(false, false, fillGuessSettings, UnitTestInitializeCsv.Token);

      Assert.IsTrue(setting.ColumnCollection.Get("ID") == null || setting.ColumnCollection.Get("ID").Convert == false);
      Assert.AreEqual(DataType.DateTime, setting.ColumnCollection.Get("ExamDate").ValueFormat.DataType);
      Assert.AreEqual(DataType.Boolean, setting.ColumnCollection.Get("IsNativeLang").ValueFormat.DataType);
    }

    [TestMethod]
    public async Task FillGuessColumnFormatTextColumnsAsync()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitializeCsv.GetTestPath("AllFormatsColon.txt"),
        HasFieldHeader = true,
        ByteOrderMark = true,
        FileFormat = { FieldDelimiter = "," }
      };
      var fillGuessSettings = new FillGuessSettings { IgnoreIdColumns = true };

      await setting.FillGuessColumnFormatReaderAsync(true, true, fillGuessSettings, UnitTestInitializeCsv.Token);

      Assert.AreEqual(10, setting.ColumnCollection.Count);
      Assert.AreEqual(DataType.DateTime, setting.ColumnCollection[0].ValueFormat.DataType);
      Assert.AreEqual(DataType.Integer, setting.ColumnCollection[1].ValueFormat.DataType);
      Assert.AreEqual(DataType.Numeric, setting.ColumnCollection[2].ValueFormat.DataType);
      Assert.AreEqual(DataType.String, setting.ColumnCollection[4].ValueFormat.DataType);
    }

    [TestMethod]
    public async Task FillGuessColumnFormatReaderAsync_Parameter()
    {
      var fillGuessSettings = new FillGuessSettings
      {
        DetectNumbers = true,
        DetectDateTime = true,
        DetectPercentage = true,
        DetectBoolean = true,
        DetectGUID = true,
        IgnoreIdColumns = true
      };
      var setting = new CsvFile
      {
        FileName = UnitTestInitializeCsv.GetTestPath("AllFormatsColon.txt"),
        HasFieldHeader = true,
        ByteOrderMark = true,
        FileFormat = { FieldDelimiter = "," },
      };

      try
      {
        // ReSharper disable once AssignNullToNotNullAttribute
        await DetermineColumnFormat.FillGuessColumnFormatReaderAsync(null, true, true, fillGuessSettings,
          UnitTestInitializeCsv.Token);
      }
      catch (ArgumentNullException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail("Wrong or exception thrown exception is : " + ex.GetType().Name);
      }

      try
      {
        // ReSharper disable once AssignNullToNotNullAttribute
        await setting.FillGuessColumnFormatReaderAsync(true, true, null, UnitTestInitializeCsv.Token);
      }
      catch (ArgumentNullException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail("Wrong or exception thrown exception is : " + ex.GetType().Name);
      }
    }

    [TestMethod]
    public async Task GetSampleValuesAsyncTest()
    {
      using (var dt = UnitTestStatic.GetDataTable(1000))
      {
        using (var dummy = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
        {
          var reader = new DataTableWrapper(dt);
          foreach (DataColumn col in dt.Columns)
          {
            var res = await DetermineColumnFormat.GetSampleValuesAsync(reader, 10,
              new[] { col.Ordinal }, 10, "null",
              dummy.CancellationToken);

            if (col.ColumnName != "AllEmpty")
              Assert.IsTrue(res[col.Ordinal].Values.Count > 0, col.ColumnName);
            else
              Assert.IsTrue(res[col.Ordinal].Values.Count == 0, col.ColumnName);
          }
        }
      }
    }

    [TestMethod]
    [Timeout(2000)]
    public async Task FillGuessColumnFormatTrailingColumnsAsync()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitializeCsv.GetTestPath("AllFormatsColon.txt"),
        HasFieldHeader = true,
        ByteOrderMark = true,
        FileFormat = { FieldDelimiter = "," }
      };
      setting.ColumnCollection.Clear();

      var fillGuessSettings = new FillGuessSettings
      {
        DetectNumbers = true,
        DetectDateTime = true,
        DetectPercentage = true,
        DetectBoolean = true,
        DetectGUID = true,
        IgnoreIdColumns = true
      };
      await setting.FillGuessColumnFormatReaderAsync(false, true, fillGuessSettings, UnitTestInitializeCsv.Token);

      // need to identify 5 typed column of the 11 existing
      Assert.AreEqual(7, setting.ColumnCollection.Count, "Number of recognized Columns");

      var v1 = setting.ColumnCollection.First(x => x.Name == "DateTime");
      var v2 = setting.ColumnCollection.First(x => x.Name == "Double");
      var v3 = setting.ColumnCollection.First(x => x.Name == "Boolean");
      Assert.AreEqual(
        DataType.DateTime,
        v1.ValueFormat.DataType,
        "DateTime (Date Time (dd/MM/yyyy))");

      // a double will always be read as decimal from Csv
      Assert.AreEqual(
        DataType.Numeric,
        v2.ValueFormat.DataType,
        "Double (Money (High Precision) (0.#####))");

      Assert.AreEqual(
        DataType.Boolean,
        v3.ValueFormat.DataType,
        "Boolean (Boolean)");
    }

    [TestMethod]
    public void GetAllPossibleFormatsD()
    {
      var res = DetermineColumnFormat.GetAllPossibleFormats("30-Oct-18 04:26:28 AM");
      var found = false;

      foreach (var item in res)
        if (item.DateFormat.Equals("dd/MMM/yy HH:mm:ss tt", StringComparison.InvariantCulture)
            && item.DateSeparator == "-")
        {
          found = true;
          break;
        }

      Assert.IsTrue(found);
    }

    [TestMethod]
    public void GetAllPossibleFormatsFrance()
    {
      var ci = new CultureInfo("fr-FR");
      var res = DetermineColumnFormat.GetAllPossibleFormats("1/2/2019 3:26:10", ci).ToList();

      var found = false;
      foreach (var item in res)
        if (item.DateFormat.Equals("M/d/yyyy H:mm:ss", StringComparison.InvariantCulture) && item.DateSeparator == "/")
        {
          found = true;
          break;
        }

      Assert.IsTrue(found);

      // we should have M/d/yyyy H:mm:ss and d/M/yyyy H:mm:ss
      Assert.AreEqual(2, res.Count);
    }

    [TestMethod]
    public void GetAllPossibleFormatsGer()
    {
      var ci = new CultureInfo("de-DE");
      var res = DetermineColumnFormat.GetAllPossibleFormats("12/13/2019 04:26", ci).ToList();

      var found = false;
      foreach (var item in res)
        if (item.DateFormat.Equals("MM/dd/yyyy HH:mm", StringComparison.InvariantCulture) && item.DateSeparator == "/")
        {
          found = true;
          break;
        }

      Assert.IsTrue(found);

      // we should have MM/dd/yyyy HH:mm, M/d/yyyy HH:mm, MM/dd/yyyy H:mm, M/d/yyyy H:mm,
      Assert.AreEqual(4, res.Count);
    }

    [TestMethod]
    public async Task GetSampleValuesByColIndexAsync()
    {
      var setting = new CsvFile { FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"), HasFieldHeader = true };
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var test = new CsvFileReader(setting, processDisplay))
      {
        await test.OpenAsync(processDisplay.CancellationToken);
        var samples = await DetermineColumnFormat.GetSampleValuesAsync(test, 1000, new[] { 0 }, 20,
          "NULL", UnitTestInitializeCsv.Token);
        Assert.AreEqual(7, samples[0].Values.Count());
        Assert.IsTrue(samples[0].RecordsRead >= 7);
        Assert.IsTrue(samples[0].Values.Contains("1"));
        Assert.IsTrue(samples[0].Values.Contains("4"));
      }
    }

    [TestMethod]
    public async Task GetSampleValuesFileEmptyAsync()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitializeCsv.GetTestPath("CSVTestEmpty.txt"),
        HasFieldHeader = true
      };
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var test = new CsvFileReader(setting, processDisplay))
      {
        await test.OpenAsync(processDisplay.CancellationToken);

        var temp = await DetermineColumnFormat
          .GetSampleValuesAsync(test, 100, new[] { 0 }, 20, "NULL", UnitTestInitializeCsv.Token)
          .ConfigureAwait(false);

        Assert.IsTrue(temp == null || temp.Count == 0);
      }
    }

    [TestMethod]
    public void GuessColumnFormatBoolean1()
    {
      string[] values = { "True", "False" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values,
        4,
        "True",
        "False",
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        null,
        UnitTestInitializeCsv.Token);
      Assert.AreEqual(DataType.Boolean, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatBoolean2()
    {
      string[] values = { "Yes", "No" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values,
        2,
        null,
        "False",
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        null,
        UnitTestInitializeCsv.Token);
      Assert.AreEqual(DataType.Boolean, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatDateNotMatching()
    {
      string[] values = { "01/02/2010", "14/02/2012", "02/14/2012" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values,
        4,
        null,
        "false",
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        null,
        UnitTestInitializeCsv.Token);
      if (res.FoundValueFormat != null)
        Assert.AreEqual(DataType.String, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatddMMyyyy()
    {
      string[] values = { "01/02/2010", "14/02/2012", "01/02/2012", "12/12/2012", "16/12/2012" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values,
        4,
        null,
        "false",
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        null,
        UnitTestInitializeCsv.Token);
      Assert.AreEqual(DataType.DateTime, res.FoundValueFormat.DataType);
      Assert.AreEqual(@"dd/MM/yyyy", res.FoundValueFormat.DateFormat);
      Assert.AreEqual("/", res.FoundValueFormat.DateSeparator);
    }

    [TestMethod]
    public void GuessColumnFormatddMMyyyy2()
    {
      string[] values = { "01.02.2010", "14.02.2012", "16.02.2012", "01.04.2014", "31.12.2010" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values,
        4,
        null,
        "false",
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        null,
        UnitTestInitializeCsv.Token);
      Assert.AreEqual(DataType.DateTime, res.FoundValueFormat.DataType);
      Assert.AreEqual(@"dd/MM/yyyy", res.FoundValueFormat.DateFormat);
      Assert.AreEqual(".", res.FoundValueFormat.DateSeparator);
    }

    [TestMethod]
    public void GuessColumnFormatGuid()
    {
      string[] values = { "{0799A029-8B85-4589-8341-C7038AFF5B48}", "99DDD263-2E2D-434F-9265-33CF893B02DF" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values,
        4,
        null,
        "false",
        false,
        true,
        false,
        false,
        false,
        false,
        false,
        null,
        UnitTestInitializeCsv.Token);
      Assert.AreEqual(DataType.Guid, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatInteger()
    {
      string[] values = { "1", "2", "3", "4", "5" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values,
        4,
        null,
        "False",
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        null,
        UnitTestInitializeCsv.Token);
      Assert.AreEqual(DataType.Integer, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatInteger2()
    {
      string[] values = { "-1", " 2", "3 ", "4", "100", "10" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values,
        4,
        null,
        "False",
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        null,
        UnitTestInitializeCsv.Token);
      Assert.AreEqual(DataType.Integer, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatMMddyyyy()
    {
      string[] values = { "01/02/2010", "02/14/2012", "02/17/2012", "02/22/2012", "03/01/2012" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values,
        4,
        null,
        "false",
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        null,
        UnitTestInitializeCsv.Token);
      Assert.AreEqual(DataType.DateTime, res.FoundValueFormat.DataType);
      Assert.AreEqual(@"MM/dd/yyyy", res.FoundValueFormat.DateFormat);
      Assert.AreEqual("/", res.FoundValueFormat.DateSeparator);
    }

    [TestMethod]
    public void GuessColumnFormatISODate()
    {
      string[] values = { "20100929", "20120214", "20120217", "20120222", "20120301" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values,
        4,
        null,
        "false",
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        null,
        UnitTestInitializeCsv.Token);
      Assert.AreEqual(DataType.DateTime, res.FoundValueFormat.DataType);
      Assert.AreEqual(@"yyyyMMdd", res.FoundValueFormat.DateFormat);
    }

    [TestMethod]
    public void GuessColumnFormatMMddyyyyNotenough()
    {
      string[] values = { "01/02/2010", "02/12/2012" };
      var res = DetermineColumnFormat.GuessValueFormat(
        values,
        4,
        null,
        "false",
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        null,
        UnitTestInitializeCsv.Token);
      Assert.IsFalse(res.PossibleMatch);
    }

    [TestMethod]
    public void GuessColumnFormatMMddyyyySuggestion()
    {
      string[] values = { "01/02/2010", "02/12/2012" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values,
        4,
        null,
        "false",
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        new ValueFormatMutable(DataType.DateTime) { DateFormat = "MM/dd/yyyy", DateSeparator = "/" },
        UnitTestInitializeCsv.Token);
      Assert.AreEqual(DataType.DateTime, res.FoundValueFormat.DataType);
      Assert.AreEqual(@"MM/dd/yyyy", res.FoundValueFormat.DateFormat);
      Assert.AreEqual("/", res.FoundValueFormat.DateSeparator);
    }

    [TestMethod]
    public void GuessColumnFormatNoSamples()
    {
      string[] values = { };
      try
      {
        DetermineColumnFormat.GuessValueFormat(
          values,
          4,
          null,
          "False",
          true,
          false,
          true,
          true,
          true,
          false,
          false,
          null,
          UnitTestInitializeCsv.Token);
        Assert.Fail("Expected Exception not thrown");
      }
      catch (ArgumentNullException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail("Wrong or exception thrown exception is : " + ex.GetType().Name);
      }
    }

    [TestMethod]
    public void GuessColumnFormatNumbersAsDate()
    {
      string[] values =
      {
        "-130.66", "-7.02", "-19.99", "-131.73", "43478.5037152778", "35634.7884722222", "35717.2918634259",
        "36858.2211226852", "43177.1338425925", "40568.3131481481", "37576.1801273148", "42573.3813078704",
        "44119.8574189815", "40060.7079976852", "43840.2724884259", "38013.3021759259", "40422.7830671296",
        "37365.5321643519", "34057.8838773148", "36490.4011805556", "40911.5474189815"
      };

      var res = DetermineColumnFormat.GuessValueFormat(
        values,
        4,
        null,
        "False",
        false,
        false,
        false,
        true,
        false,
        true,
        false,
        null,
        UnitTestInitializeCsv.Token);
      Assert.AreEqual(DataType.DateTime, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatNumeric()
    {
      string[] values = { "1", "2.5", "3", "4", "5.3" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values,
        4,
        null,
        "False",
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        null,
        UnitTestInitializeCsv.Token);
      Assert.AreEqual(DataType.Numeric, res.FoundValueFormat.DataType);
      Assert.AreEqual('.', res.FoundValueFormat.DecimalSeparatorChar);
    }

    [TestMethod]
    public void GuessColumnFormatNumeric2()
    {
      string[] values = { "1", "2,5", "1.663", "4", "5,3" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values,
        4,
        null,
        "False",
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        null,
        UnitTestInitializeCsv.Token);
      Assert.AreEqual(DataType.Numeric, res.FoundValueFormat.DataType);
      Assert.AreEqual('.', res.FoundValueFormat.GroupSeparatorChar);
      Assert.AreEqual(',', res.FoundValueFormat.DecimalSeparatorChar);
    }

    [TestMethod]
    public void GuessColumnFormatText()
    {
      string[] values = { "Hallo", "Welt" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values,
        4,
        null,
        "false",
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        null,
        UnitTestInitializeCsv.Token);
      Assert.AreEqual(DataType.String, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatVersionNumbers()
    {
      string[] values = { "1.0.1.2", "1.0.2.1", "1.0.2.2", "1.0.2.3", "1.0.2.3" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values,
        4,
        null,
        "False",
        false,
        false,
        true,
        false,
        false,
        false,
        false,
        null,
        UnitTestInitializeCsv.Token);
      Assert.IsTrue(res.FoundValueFormat == null || res.FoundValueFormat.DataType != DataType.Integer);
    }
  }
}