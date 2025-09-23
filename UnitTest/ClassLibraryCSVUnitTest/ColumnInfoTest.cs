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
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace CsvTools.Tests
{
  [TestClass]
  public class ColumnInfoTest
  {

    [TestMethod]
    [Ignore("Used to provide information")]
    public void GetCultureInfo()
    {
      var dateSeparator = new HashSet<string>();
      var timeSeparator = new HashSet<string>();
      var decimalSeparator =new HashSet<string>();
      var groupSeperator =new HashSet<string>();
      var currency =new HashSet<string>();
      var listSep = new HashSet<string>();
      foreach (var ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
      {
        dateSeparator.Add(ci.DateTimeFormat.DateSeparator);
        timeSeparator.Add(ci.DateTimeFormat.TimeSeparator);
        //decimalSeparator.Add(ci.NumberFormat.CurrencyDecimalSeparator);
        decimalSeparator.Add(ci.NumberFormat.NumberDecimalSeparator);
        //groupSeperator.Add(ci.NumberFormat.CurrencyGroupSeparator);
        groupSeperator.Add(ci.NumberFormat.NumberGroupSeparator);
        if (ci.NumberFormat.CurrencySymbol.Length == 1)
        {
          if ((ci.NumberFormat.CurrencySymbol[0]<'a' || ci.NumberFormat.CurrencySymbol[0]>'z') &&
              (ci.NumberFormat.CurrencySymbol[0]<'A' || ci.NumberFormat.CurrencySymbol[0]>'Z'))
          currency.Add(ci.NumberFormat.CurrencySymbol);
        }

        listSep.Add(ci.TextInfo.ListSeparator);
      }
      Debug.WriteLine(listSep.Select(x=>"'" + x.SqlQuote() + "'").Join(","));
      Debug.WriteLine(dateSeparator.Select(x=>"'" + x.SqlQuote() + "'").Join(","));
      Debug.WriteLine(timeSeparator.Select(x=>"'" + x.SqlQuote() + "'").Join(","));
      Debug.WriteLine(decimalSeparator.Select(x=>"'" + x.SqlQuote() + "'").Join(","));
      Debug.WriteLine(groupSeperator.Select(x=>"'" + x.SqlQuote() + "'").Join(","));
      
      Debug.WriteLine(currency.Select(x=>"'" + x.SqlQuote() + "'").Join(","));
    }
    [TestMethod]
    public void GetSourceColumnInformation_OverwrittenType()
    {
      var cc = new ColumnCollection
      {
        new Column("Test1", new ValueFormat(DataTypeEnum.Double)),
        new Column("Test2", new ValueFormat(dataType: DataTypeEnum.DateTime, dateFormat: "dd/MM/yyyy HH:mm"),
          timeZonePart: "\"UTC\""),
        new Column("Test3", new ValueFormat(DataTypeEnum.DateTime, dateFormat: "dd/MM/yyyy HH:mm"), timePart: "Test4",
          timePartFormat: "HH:mm")
      };

      var dt = new DataTable();
      dt.Columns.AddRange(new[]
      {
        new DataColumn("Test1", typeof(long)), new DataColumn("Test2", typeof(string)),
        new DataColumn("Test3", typeof(DateTime))
      });
      using var reader = new DataTableWrapper(dt);
      var res = BaseFileWriter.GetColumnInformation(ValueFormat.Empty, cc, reader).ToList();
      Assert.AreEqual(4, res.Count());

      Assert.AreEqual(DataTypeEnum.Double, res[0].ValueFormat.DataType,
        "Usually it would be Integer bt is has to be double");
      Assert.AreEqual(DataTypeEnum.DateTime, res[2].ValueFormat.DataType);

      // The time column was added
      Assert.AreEqual(DataTypeEnum.DateTime, res[3].ValueFormat.DataType);
      Assert.AreEqual("Test4", res[3].Name);
    }

    [TestMethod]
    public void GetSourceColumnInformation_AddedTime()
    {
      var cc = new ColumnCollection
      {
        new Column("Test3",
          new ValueFormat(dataType: DataTypeEnum.DateTime, dateFormat: "dd/MM/yyyy HH:mm"),
          timeZonePart: "\"UTC\"", timePart: "Col2")
      };

      var dt = new DataTable();
      dt.Columns.AddRange(new[]
      {
        new DataColumn("Test1", typeof(long)), new DataColumn("Test2", typeof(string)),
        new DataColumn("Test3", typeof(DateTime))
      });
      using var reader = new DataTableWrapper(dt);
      var res = BaseFileWriter.GetColumnInformation(ValueFormat.Empty, cc, reader).ToList();
      Assert.AreEqual(4, res.Count());
      Assert.AreEqual(DataTypeEnum.Integer, res[0].ValueFormat.DataType);
      Assert.AreEqual(DataTypeEnum.DateTime, res[2].ValueFormat.DataType);
      Assert.AreEqual("Col2", res[3].Name, "Added column for Time is Col2");
      Assert.AreEqual(DataTypeEnum.DateTime, res[3].ValueFormat.DataType,
        "Added column for Time is of type dateTime");
    }
  }
}
