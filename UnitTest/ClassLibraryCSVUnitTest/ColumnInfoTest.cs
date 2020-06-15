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

using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ColumnInfoTest
  {
    [TestMethod]
    public void ColumnInfoPropertySetGet()
    {
      var vf = new ValueFormat();
      var col = new Column("Test", vf);
      var ci = new ColumnInfo(col, 100, false);

      Assert.AreEqual(col, ci.Column);
      Assert.AreEqual(100, ci.FieldLength);

      ci.IsTimePart = true;
      Assert.AreEqual(true, ci.IsTimePart);

      Assert.AreEqual(vf.DataType, ci.Column.ValueFormat.DataType);
    }

    [TestMethod]
    public void GetSourceColumnInformation_OverwrittenType()
    {
      var writerFileSetting = new CsvFile();
      writerFileSetting.ColumnCollection.AddIfNew(new Column("Test1", DataType.Double));

      writerFileSetting.ColumnCollection.AddIfNew(new Column("Test2", new ValueFormat(DataType.DateTime) { DateFormat = "dd/MM/yyyy HH:mm" })
      { TimeZonePart = "\"UTC\"" });

      var dt = new DataTable();
      dt.Columns.AddRange(new[] { new DataColumn("Test1", typeof(int)), new DataColumn("Test2", typeof(string)), new DataColumn("Test3", typeof(System.DateTime)) });
      var res = ColumnInfo.GetSourceColumnInformation(writerFileSetting, dt.CreateDataReader()).ToList();
      Assert.AreEqual(3, res.Count());
      // 
      Assert.AreEqual(DataType.Double, res[0].Column.ValueFormat.DataType, "Usually it would be Integer bt is has to be double");
      Assert.AreEqual(DataType.DateTime, res[2].Column.ValueFormat.DataType);
    }

    [TestMethod]
    public void GetSourceColumnInformation_AddedTime()
    {
      var writerFileSetting = new CsvFile();
      writerFileSetting.ColumnCollection.AddIfNew(new Column("Test3", new ValueFormat(DataType.DateTime) { DateFormat = "dd/MM/yyyy HH:mm" })
      {
        TimeZonePart = "\"UTC\"",
        TimePart = "Col2"
      });

      var dt = new DataTable();
      dt.Columns.AddRange(new[] { new DataColumn("Test1", typeof(int)), new DataColumn("Test2", typeof(string)), new DataColumn("Test3", typeof(System.DateTime)) });
      var res = ColumnInfo.GetSourceColumnInformation(writerFileSetting, dt.CreateDataReader()).ToList();
      Assert.AreEqual(4, res.Count());
      Assert.AreEqual(DataType.Integer, res[0].Column.ValueFormat.DataType);
      Assert.AreEqual(DataType.DateTime, res[2].Column.ValueFormat.DataType);
      Assert.AreEqual("Col2", res[3].Column.Name, "Added column for Time is Col2");
      Assert.AreEqual(DataType.DateTime, res[3].Column.ValueFormat.DataType, "Added column for Time is of type dateTime");
    }

  }
}