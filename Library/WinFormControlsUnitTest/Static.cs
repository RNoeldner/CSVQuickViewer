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

using System;
using System.Data;
using System.Globalization;

namespace CsvTools.Tests
{
  public static class UnitTestStatic
  {
#pragma warning disable CA2211 // Non-constant fields should not be visible

    public static Column[] ColumnsDT2 =
    {
      new Column {Name = "string", DataType = DataType.String} //0
    };

    public static Column[] ColumnsDT =
    {
      new Column {Name = "string", DataType = DataType.String}, //0
      new Column {Name = "int", DataType = DataType.Integer}, //1
      new Column {Name = "DateTime", DataType = DataType.DateTime}, //2
      new Column {Name = "bool", DataType = DataType.Boolean}, //3
      new Column {Name = "double", DataType = DataType.Double}, //4
      new Column {Name = "numeric", DataType = DataType.Numeric}, //5
      new Column {Name = "AllEmpty", DataType = DataType.String}, //6
      new Column {Name = "PartEmpty", DataType = DataType.String}, //7
      new Column {Name = "ID", DataType = DataType.Integer} //8
    };

#pragma warning restore CA2211 // Non-constant fields should not be visible

    private static string GetRandomText(int length)
    {
      const string cBase = "012345abcdefghijklmnopqrstuvwxyz6789ABCDEFGHIJKLMNOPQRSTUVWXYZ,.*$%&!";
      var builder = new char[length];
      for (var i = 0; i < length; i++)
        builder[i] = cBase[Convert.ToInt32(Math.Floor(cBase.Length * SecureString.Random.NextDouble()))];
      return new string(builder);
    }

    public static DataTable GetDataTable2(long numRecords = 100)
    {
      var dataTable = new DataTable
      {
        TableName = "ArtificialTable2",
        Locale = new CultureInfo("en-gb")
      };
      dataTable.Columns.Add("string", typeof(string));
      for (long i = 1; i <= numRecords; i++)
      {
        var dr = dataTable.NewRow();
        dr[0] = i.ToString(CultureInfo.InvariantCulture);
        dataTable.Rows.Add(dr);
      }

      return dataTable;
    }

    public static DataTable GetDataTable(int numRecords = 100)
    {
      var dataTable = new DataTable
      {
        TableName = "ArtificialTable",
        Locale = new CultureInfo("en-gb")
      };
      dataTable.Columns.Add("string", typeof(string));
      dataTable.Columns.Add("int", typeof(int));
      dataTable.Columns.Add("DateTime", typeof(DateTime));
      dataTable.Columns.Add("bool", typeof(bool));
      dataTable.Columns.Add("double", typeof(double));
      dataTable.Columns.Add("numeric", typeof(decimal));
      dataTable.Columns.Add("AllEmpty", typeof(string));
      dataTable.Columns.Add("PartEmpty", typeof(string));
      dataTable.Columns.Add("ID", typeof(int));
      var minDate = DateTime.Now.AddYears(-20).Ticks;
      var maxDate = DateTime.Now.AddYears(5).Ticks;

      for (var i = 1; i <= numRecords; i++)
      {
        var dr = dataTable.NewRow();
        dr[0] = GetRandomText(50);
        if (i % 10 == 0)
          dr[0] = dr[0] + "\r\nA Second Line";
        dr[1] = i;
        if (SecureString.Random.NextDouble() < .3)
        {
          dr[2] = DBNull.Value;
        }
        else
        {
          var dtm = Convert.ToInt64((maxDate - minDate) * SecureString.Random.NextDouble() + minDate);
          dr[2] = new DateTime(dtm);
        }

        dr[3] = i % 2 == 0;
        dr[4] = SecureString.Random.NextDouble() * 123.78;
        if (i % 3 == 0)
          dr[5] = SecureString.Random.NextDouble();
        dr[7] = SecureString.Random.NextDouble() < .3 ? null : GetRandomText(100);
        dr[8] = SecureString.Random.Next(1, 5000000);
        if (i % 33 < 3)
          dr.SetColumnError(i % 33, "ColumnError");
        if (i % 35 == 0)
          dr.RowError = "RowError";
        dataTable.Rows.Add(dr);
      }

      return dataTable;
    }
  }
}