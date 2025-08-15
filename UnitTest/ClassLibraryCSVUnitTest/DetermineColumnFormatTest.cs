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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS8625

namespace CsvTools.Tests
{
  [TestClass]
  [SuppressMessage("ReSharper", "UseAwaitUsing")]
  public class DetermineColumnFormatTest
  {
    [TestMethod()]
    public async Task FillGuessColumnFormatReaderAsyncTest()
    {
      var cvsSetting = new CsvFileDummy();
      cvsSetting.FileName = UnitTestStatic.GetTestPath("AllFormats.txt");
      cvsSetting.FieldDelimiterChar = '\t';

      var res = 
        await cvsSetting.FillGuessColumnFormatReaderAsync(true, true, new FillGuessSettings(), UnitTestStatic.Token);
      Assert.AreEqual(10, res.Item2.Count);
    }

    [TestMethod()]
    public void CommonDateFormatTest()
    {
      var list = new List<Column>();

      var test1 = DetermineColumnFormat.CommonDateFormat(list, "dd.MM.yyyy");
      Assert.AreEqual('.', test1.DateSeparator);
      Assert.AreEqual("dd/MM/yyyy", test1.DateFormat);

      list.Add(new Column("Text1", ValueFormat.Empty, 3));
      list.Add(new Column("Date1", new ValueFormat(DataTypeEnum.DateTime, dateFormat: "dd/MM/yyyy"), 1));
      Assert.AreEqual("dd/MM/yyyy", DetermineColumnFormat.CommonDateFormat(list, null).DateFormat);

      list.Add(new Column("Date2", new ValueFormat(DataTypeEnum.DateTime, dateFormat: "MM/dd/yyyy"), 2, true));
      list.Add(new Column("Date3", new ValueFormat(DataTypeEnum.DateTime, dateFormat: "MM/dd/yyyy"), 3, true));
      Assert.AreEqual("dd/MM/yyyy", DetermineColumnFormat.CommonDateFormat(list, null).DateFormat);

      list.Add(new Column("Date4", new ValueFormat(DataTypeEnum.DateTime, dateFormat: "MM/dd/yyyy"), 3, false));
      list.Add(new Column("Date5", new ValueFormat(DataTypeEnum.DateTime, dateFormat: "MM/dd/yyyy"), 3, false));
      Assert.AreEqual("MM/dd/yyyy", DetermineColumnFormat.CommonDateFormat(list, null).DateFormat);

      list.Add(new Column("Date6", new ValueFormat(DataTypeEnum.DateTime, dateFormat: "dd/MM/yyyy"), 4));
      list.Add(new Column("Date7", new ValueFormat(DataTypeEnum.DateTime, dateFormat: "dd/MM/yyyy"), 4));
      Assert.AreEqual("dd/MM/yyyy", DetermineColumnFormat.CommonDateFormat(list, null).DateFormat);
    }


    [TestMethod()]
    public void GuessDateTimeTest()
    {
      var list = new List<ReadOnlyMemory<char>>();
      try
      {
        Assert.IsNull(DetermineColumnFormat.GuessDateTime(list, UnitTestStatic.Token).FoundValueFormat);
      }
      catch (ArgumentException)
      {
        // fine passing an empty list can raise an error
      }

      list.Add("09/09/2020".AsMemory());
      list.Add("10/10/2020".AsMemory());
      list.Add("11/11/2020".AsMemory());
      var df = DetermineColumnFormat.GuessDateTime(list, UnitTestStatic.Token).FoundValueFormat!.DateFormat;
      Assert.IsTrue(df == "MM/dd/yyyy" | df == "dd/MM/yyyy");
      list.Add("24/12/2020".AsMemory());
      Assert.IsTrue(new ValueFormat(DataTypeEnum.DateTime, "dd/MM/yyyy").Equals(DetermineColumnFormat
        .GuessDateTime(list, UnitTestStatic.Token).FoundValueFormat!));
    }

    [TestMethod()]
    public void GuessNumericTest()
    {
      var list = new List<ReadOnlyMemory<char>>();
      try
      {
        Assert.IsNull(DetermineColumnFormat.GuessNumeric(list, true, true, true, UnitTestStatic.Token).FoundValueFormat);
      }
      catch (ArgumentException)
      {
        // fine passing an empty list can raise an error
      }
    }

    [TestMethod()]
    public void GuessValueFormatTest()
    {
      var list = new List<ReadOnlyMemory<char>>();
      try
      {
        Assert.IsNull(DetermineColumnFormat.GuessValueFormat(list, 2, "true".AsSpan(), "false".AsSpan(),
            true, true, true, true, true, true, true, new ValueFormat(DataTypeEnum.DateTime), UnitTestStatic.Token)
          .FoundValueFormat);
      }
      catch (ArgumentException)
      {
        // fine passing an empty list can raise an error
      }
    }

 

    [TestMethod]
    public async Task GetSampleValuesAsync()
    {
      using var dataTable = UnitTestStaticData.GetDataTable(150, false);

      using var reader = new DataTableWrapper(dataTable);
      var res = await DetermineColumnFormat
        .GetSampleValuesAsync(reader, 100, new[] { 0, 1 }, string.Empty, 80, UnitTestStatic.Token)
        .ConfigureAwait(false);
      Assert.AreEqual(20, res[0].Values.Count);
    }

    [TestMethod]
    public async Task FillGuessColumnFormatReaderAsyncReaderCsvFile()
    {
      var fillGuessSettings = new FillGuessSettings(true, detectNumbers: true, detectDateTime: true,
       detectPercentage: true, detectBoolean: true, detectGuid: true,
       ignoreIdColumns: true);
      using (var reader = new CsvTools.CsvFileReader(UnitTestStatic.GetTestPath("AllFormats.txt"), Encoding.UTF8.CodePage, 0, true,
        Array.Empty<Column>(), CsvTools.TrimmingOptionEnum.All,
        '\t', '"', char.MinValue, 0, false, false, "", 0, true, "", "",
        "", true, false, false, true, true, false, true, true, true, true, false,
        treatTextAsNull: "NULL", skipEmptyLines: true, consecutiveEmptyRowsMax: 4,
        identifierInContainer: string.Empty, timeZoneAdjust: CsvTools.StandardTimeZoneAdjust.ChangeTimeZone, returnedTimeZone: TimeZoneInfo.Local.Id, true, true))
      {
        await reader.OpenAsync(CancellationToken.None);
        var (information, columns) = await CsvTools.DetermineColumnFormat.FillGuessColumnFormatReaderAsyncReader(reader, fillGuessSettings,
        new CsvTools.ColumnCollection(), false, true, "<NULL>", CancellationToken.None);
      }
    }


    [TestMethod]
    public async Task GetSourceColumnInformationTestAsync2()
    {
      // Make sure we do not have errors in table as this would invalidate the rows for detection
      using var dt = UnitTestStaticData.GetDataTable(100, false);
      using var reader = new DataTableWrapper(dt);
      var fillGuessSettings = new FillGuessSettings(true, detectNumbers: true, detectDateTime: true,
        detectPercentage: true, detectBoolean: true, detectGuid: true,
        ignoreIdColumns: true);

      var (information, columns) = await reader.FillGuessColumnFormatReaderAsyncReader(fillGuessSettings,
        new ColumnCollection(), false, true, "<NULL>", UnitTestStatic.Token);
      /*
        "int (Integer)"
        "DateTime (Date Time (MM/dd/yyyy))"
        "bool (Boolean)"
        "guid"
        "double (Money (High Precision) (0,#####))"
        "numeric (Money (High Precision) (0,#####))"
        "ID (Integer)"
        "#Line (Integer)"
       */
      Assert.AreEqual(8, columns.Count(), "Recognized columns");
      Assert.AreEqual(9, information.Count, "Information Lines");

      // with Text columns
      var (information2, columns2) = await reader.FillGuessColumnFormatReaderAsyncReader(fillGuessSettings,
        new ColumnCollection(), true, true, "<NULL>", UnitTestStatic.Token);
      Assert.AreEqual(11, columns2.Count(), "Recognized columns 2");      
      Assert.AreEqual(11, information2.Count, "Information Lines 2");
    }



  


    [TestMethod, Timeout(2000)]
    public async Task DetermineColumnFormatGetSampleRollOverAsync()
    {
      using var dt = new DataTable();
      dt.Columns.Add(new DataColumn { ColumnName = "ID", DataType = typeof(Guid) });
      for (var i = 0; i < 30; i++)
      {
        var row = dt.NewRow();

        // add two empty rows
        if (i % 5 != 0)
          row[0] = Guid.NewGuid();
        dt.Rows.Add(row);
      }

      using var reader = new DataTableWrapper(dt, false, false, false, false);
      // Move the reader to a late record
      for (var i = 0; i < dt.Rows.Count / 2; i++)
        await reader.ReadAsync(UnitTestStatic.Token);
      var treatAsNull = string.Empty;
      var res = await DetermineColumnFormat.GetSampleValuesAsync(reader, 0, new[] { 0 }, treatAsNull,
        40, UnitTestStatic.Token).ConfigureAwait(false);
      Assert.AreEqual(dt.Rows.Count, res[0].RecordsRead, "RecordsRead");
      Assert.AreEqual(dt.Rows.Count - (dt.Rows.Count / 5), res[0].Values.Count());
    }

    [TestMethod]
    public async Task DetermineColumnFormatGetSampleValues2Async()
    {
      using var dt = new DataTable();
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

      using var reader = new DataTableWrapper(dt);
      var treatAsNull = string.Empty;
      var res = await DetermineColumnFormat.GetSampleValuesAsync(reader, 0, new[] { 0, 1 }, treatAsNull,
        40, UnitTestStatic.Token);

      Assert.IsTrue(res[0].RecordsRead >= 20);
      Assert.IsTrue(res[1].RecordsRead >= 20);
      Assert.AreEqual(20, res[0].Values.Count());
      Assert.AreEqual(20, res[1].Values.Count());
    }

    [TestMethod]
    public async Task DetermineColumnFormatGetSampleValuesAsync2Async()
    {
      using var dt = new DataTable();
      dt.Columns.Add(new DataColumn { ColumnName = "ID", DataType = typeof(string) });
      for (var i = 0; i < 150; i++)
      {
        var row = dt.NewRow();
        if (i == 10 || i == 47)
          row[0] = "NULL";
        row[0] = (i / 3).ToString(CultureInfo.InvariantCulture);
        dt.Rows.Add(row);
      }

      using var reader = new DataTableWrapper(dt);
      var treatAsNull = string.Empty;
      var res = await DetermineColumnFormat.GetSampleValuesAsync(reader, 0, new[] { 0 }, treatAsNull,
        40, UnitTestStatic.Token);
      Assert.IsTrue(res[0].RecordsRead >= 20);
      Assert.AreEqual(20, res[0].Values.Count());
    }

    [TestMethod]
    public async Task DetermineColumnFormatGetSampleValuesNoColumns()
    {
      using var dt = new DataTable();

      using var reader = new DataTableWrapper(dt);
      var temp = await DetermineColumnFormat
        .GetSampleValuesAsync(reader, 0, new[] { 0 }, string.Empty, 40, UnitTestStatic.Token).ConfigureAwait(false);
      Assert.AreEqual(0, temp.Count);
    }


    [TestMethod]
    [Timeout(2000)]
    public async Task GetSampleValuesAsyncTest2()
    {
      // make sure we do not have errors as these could void the records in GetSampleValuesAsync
      using var dt = UnitTestStaticData.GetDataTable(1000, false);

      var reader = new DataTableWrapper(dt);
      foreach (DataColumn col in dt.Columns)
      {
        var res = await DetermineColumnFormat.GetSampleValuesAsync(reader, 10,
          new[] { col.Ordinal }, "null", 40, UnitTestStatic.Token);

        if (col.ColumnName != "AllEmpty")
          Assert.AreNotEqual(0, res[col.Ordinal].Values.Count, $"Column {col.ColumnName} has {res[col.Ordinal].Values.Count} entries");
        else
          Assert.AreEqual(0, res[col.Ordinal].Values.Count, $"Column {col.ColumnName} has {res[col.Ordinal].Values.Count} entries");
      }
    }


    [TestMethod]
    public void GuessColumnFormatBoolean1()
    {
      string[] values = { "True", "False" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values.Select(x => x.AsMemory()).ToArray(),
        4,
        "True".AsSpan(),
        "False".AsSpan(),
        true,
        false,
        true,
        true,
        true,
        false, false,
        null,
        UnitTestStatic.Token);
      Assert.AreEqual(DataTypeEnum.Boolean, res.FoundValueFormat?.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatBoolean2()
    {
      string[] values = { "Yes", "No" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values.Select(x => x.AsMemory()).ToArray(),
        2,
        null,
        "False".AsSpan(),
        true,
        false,
        true,
        true,
        true,
        false, true,
        null,
        UnitTestStatic.Token);
      Assert.AreEqual(DataTypeEnum.Boolean, res.FoundValueFormat?.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatDateNotMatching()
    {
      string[] values = { "01/02/2010", "14/02/2012", "02/14/2012" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values.Select(x => x.AsMemory()).ToArray(),
        4,
        null,
        "false".AsSpan(),
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        new ValueFormat(),
        UnitTestStatic.Token);
      if (res.FoundValueFormat != null)
        Assert.AreEqual(DataTypeEnum.String, res.FoundValueFormat?.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatddMMyyyy()
    {
      string[] values = { "01/02/2010", "14/02/2012", "01/02/2012", "12/12/2012", "16/12/2012" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values.Select(x => x.AsMemory()).ToArray(),
        4,
        null,
        "false".AsSpan(),
        true,
        false,
        true,
        true,
        true,
        false, true,
        new ValueFormat(),
        UnitTestStatic.Token);
      Assert.AreEqual(DataTypeEnum.DateTime, res.FoundValueFormat?.DataType);
      Assert.AreEqual(@"dd/MM/yyyy", res.FoundValueFormat?.DateFormat);
      Assert.AreEqual('/', res.FoundValueFormat?.DateSeparator);
    }

    [TestMethod]
    public void GuessColumnFormatddMMyyyy2()
    {
      string[] values = { "01.02.2010", "14.02.2012", "16.02.2012", "01.04.2014", "31.12.2010" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values.Select(x => x.AsMemory()).ToArray(),
        4,
        null,
        "false".AsSpan(),
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        new ValueFormat(),
        UnitTestStatic.Token);
      Assert.AreEqual(DataTypeEnum.DateTime, res.FoundValueFormat?.DataType);
      Assert.AreEqual(@"dd/MM/yyyy", res.FoundValueFormat?.DateFormat);
      Assert.AreEqual('.', res.FoundValueFormat?.DateSeparator);
    }

    [TestMethod]
    public void GuessColumnFormatGuid()
    {
      string[] values = { "{0799A029-8B85-4589-8341-C7038AFF5B48}", "99DDD263-2E2D-434F-9265-33CF893B02DF" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values.Select(x => x.AsMemory()).ToArray(),
        4,
        null,
       null,
        false,
        true,
        false,
        false,
        false,
        false,
        false,
        new ValueFormat(),
        UnitTestStatic.Token);
      Assert.AreEqual(DataTypeEnum.Guid, res.FoundValueFormat?.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatInteger()
    {
      string[] values = { "1", "2", "3", "4", "5" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values.Select(x => x.AsMemory()).ToArray(),
        4,
        null,
        null,
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        new ValueFormat(),
        UnitTestStatic.Token);
      Assert.AreEqual(DataTypeEnum.Integer, res.FoundValueFormat?.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatInteger2()
    {
      string[] values = { "-1", " 2", "3 ", "4", "100", "10" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values.Select(x => x.AsMemory()).ToArray(),
        4,
        null,
        null,
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        new ValueFormat(),
        UnitTestStatic.Token);
      Assert.AreEqual(DataTypeEnum.Integer, res.FoundValueFormat?.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatMMddyyyy()
    {
      string[] values = { "01/02/2010", "02/14/2012", "02/17/2012", "02/22/2012", "03/01/2012" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values.Select(x => x.AsMemory()).ToArray(),
        4,
        null,
         null,
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        new ValueFormat(),
        UnitTestStatic.Token);
      Assert.AreEqual(DataTypeEnum.DateTime, res.FoundValueFormat?.DataType);
      Assert.AreEqual(@"MM/dd/yyyy", res.FoundValueFormat?.DateFormat);
      Assert.AreEqual('/', res.FoundValueFormat?.DateSeparator);
    }

    [TestMethod]
    public void GuessColumnFormatIsoDate()
    {
      string[] values = { "20100929", "20120214", "20120217", "20120222", "20120301" };

      var res = DetermineColumnFormat.GuessValueFormat(
        values.Select(x => x.AsMemory()).ToArray(),
        4,
        null,
         null,
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        new ValueFormat(),
        UnitTestStatic.Token);
      Assert.AreEqual(DataTypeEnum.DateTime, res.FoundValueFormat?.DataType);
      Assert.AreEqual(@"yyyyMMdd", res.FoundValueFormat?.DateFormat);
    }

    [TestMethod]
    public void GuessColumnFormatMMddyyyyNotenough()
    {
      string[] values = { "01/02/2010", "02/12/2012" };
      var res = DetermineColumnFormat.GuessValueFormat(
        values.Select(x => x.AsMemory()).ToArray(),
        4,
        null,
         null,
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        new ValueFormat(),
        UnitTestStatic.Token);
      Assert.IsFalse(res.PossibleMatch);
    }

    [TestMethod]
    public void GuessColumnFormatMMddyyyySuggestion()
    {
      string[] values = { "01/02/2010", "02/12/2012" };

      var res = DetermineColumnFormat.GuessValueFormat(
          values.Select(x => x.AsMemory()).ToArray(),
        4,
        null,
        null,
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        new ValueFormat(dataType: DataTypeEnum.DateTime, dateFormat: "MM/dd/yyyy", dateSeparator: "/"),
        UnitTestStatic.Token);
      Assert.AreEqual(DataTypeEnum.DateTime, res.FoundValueFormat?.DataType);
      Assert.AreEqual(@"MM/dd/yyyy", res.FoundValueFormat?.DateFormat);
      Assert.AreEqual('/', res.FoundValueFormat?.DateSeparator);
    }

    [TestMethod]
    public void GuessColumnFormatNoSamples()
    {
      string[] values = { };
      try
      {
        DetermineColumnFormat.GuessValueFormat(
          values.Select(x => x.AsMemory()).ToArray(),
          4,
          null,
          "False".AsSpan(),
          true,
          false,
          true,
          true,
          true,
          false,
          false,
          null,
          UnitTestStatic.Token);
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
          values.Select(x => x.AsMemory()).ToArray(),
        4,
        null,
        null,
        false,
        false,
        false,
        true,
        false,
        true,
        false,
        new ValueFormat(),
        UnitTestStatic.Token);
      Assert.AreEqual(DataTypeEnum.DateTime, res.FoundValueFormat?.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatNumeric()
    {
      string[] values = { "1", "2.5", "3", "4", "5.3" };

      var res = DetermineColumnFormat.GuessValueFormat(
         values.Select(x => x.AsMemory()).ToArray(),
        4,
        null,
        "False".AsSpan(),
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        new ValueFormat(),
        UnitTestStatic.Token);
      Assert.AreEqual(DataTypeEnum.Numeric, res.FoundValueFormat?.DataType);
      Assert.AreEqual('.', res.FoundValueFormat?.DecimalSeparator);
    }

    [TestMethod]
    public void GuessColumnFormatNumeric2()
    {
      string[] values = { "1", "2,5", "1.663", "4", "5,3" };

      var res = DetermineColumnFormat.GuessValueFormat(
         values.Select(x => x.AsMemory()).ToArray(),
        4,
         "True".AsSpan(),
       "False".AsSpan(),
        true,
        false,
        true,
        true,
        true,
        false,
        true,
        new ValueFormat(),
        UnitTestStatic.Token);
      Assert.AreEqual(DataTypeEnum.Numeric, res.FoundValueFormat?.DataType);
      Assert.AreEqual('.', res.FoundValueFormat?.GroupSeparator);
      Assert.AreEqual(',', res.FoundValueFormat?.DecimalSeparator);
    }



    [TestMethod]
    public void GuessColumnFormatVersionNumbers()
    {
      string[] values = { "1.0.1.2", "1.0.2.1", "1.0.2.2", "1.0.2.3", "1.0.2.3" };

      var res = DetermineColumnFormat.GuessValueFormat(
       values.Select(x => x.AsMemory()).ToArray(),
        4,
        null,
         "False".AsSpan(),
        false,
        false,
        true,
        false,
        false,
        false,
        true,
        null,
        UnitTestStatic.Token);
      Assert.IsTrue(res.FoundValueFormat is null || res.FoundValueFormat?.DataType != DataTypeEnum.Integer);
    }
  }
}