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
using System.Data;
using System.Linq;

namespace CsvTools.Tests
{
  [TestClass]
  public class ColumnInfoTest
  {
    [TestMethod]
    public void GetSourceColumnInformation_OverwrittenType()
    {
      var cc = new ColumnCollection();
      cc.AddIfNew(new Column("Test1", DataType.Double));
      cc.AddIfNew(
        new Column("Test2", new ValueFormatMutable(DataType.DateTime) { DateFormat = "dd/MM/yyyy HH:mm" })
        {
          TimeZonePart = "\"UTC\""
        });
      cc.AddIfNew(
        new Column("Test3", new ValueFormatMutable(DataType.DateTime) { DateFormat = "dd/MM/yyyy HH:mm" })
        {
          TimePart = "Test4",
          TimePartFormat = "HH:mm"
        });

      var dt = new DataTable();
      dt.Columns.AddRange(new[]
      {
        new DataColumn("Test1", typeof(int)), new DataColumn("Test2", typeof(string)),
        new DataColumn("Test3", typeof(DateTime))
      });
      var res = ColumnInfo
        .GetWriterColumnInformation(new ValueFormatMutable(), cc.ReadonlyCopy(), dt.CreateDataReader()).ToList();
      Assert.AreEqual(4, res.Count());

      Assert.AreEqual(DataType.Double, res[0].Column.ValueFormat.DataType,
        "Usually it would be Integer bt is has to be double");
      Assert.AreEqual(DataType.DateTime, res[2].Column.ValueFormat.DataType);

      // The time column was added 
      Assert.AreEqual(DataType.DateTime, res[3].Column.ValueFormat.DataType);
      Assert.AreEqual("Test4", res[3].Column.Name);
    }

    [TestMethod]
    public void GetSourceColumnInformation_AddedTime()
    {
      var cc = new ColumnCollection();
      cc.AddIfNew(new Column("Test3", new ValueFormatMutable(DataType.DateTime) { DateFormat = "dd/MM/yyyy HH:mm" })
      {
        TimeZonePart = "\"UTC\"",
        TimePart = "Col2"
      });

      var dt = new DataTable();
      dt.Columns.AddRange(new[]
      {
        new DataColumn("Test1", typeof(int)), new DataColumn("Test2", typeof(string)),
        new DataColumn("Test3", typeof(DateTime))
      });
      var res = ColumnInfo
        .GetWriterColumnInformation(new ValueFormatMutable(), cc.ReadonlyCopy(), dt.CreateDataReader()).ToList();
      Assert.AreEqual(4, res.Count());
      Assert.AreEqual(DataType.Integer, res[0].Column.ValueFormat.DataType);
      Assert.AreEqual(DataType.DateTime, res[2].Column.ValueFormat.DataType);
      Assert.AreEqual("Col2", res[3].Column.Name, "Added column for Time is Col2");
      Assert.AreEqual(DataType.DateTime, res[3].Column.ValueFormat.DataType,
        "Added column for Time is of type dateTime");
    }
  }
}