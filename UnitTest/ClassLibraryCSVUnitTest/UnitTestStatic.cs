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
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace CsvTools.Tests
{
  public static class UnitTestStatic
  {
    private static readonly Random m_Random = new Random(Guid.NewGuid().GetHashCode());

    public static T ExecuteWithCulture<T>(Func<T> methodFunc, string cultureName)
    {
      var result = default(T);

      var thread = new Thread(() => { result = methodFunc(); }) {CurrentCulture = new CultureInfo(cultureName)};
      thread.Start();
      thread.Join();

      return result;
    }

    public static string GetRandomText(int length)
    {
      if (length < 1)
        return null;
      // Space is in there a few times so we get more spaces
      var chars = " abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ 1234567890 !§$%&/()=?+*#,.-;:_ "
        .ToCharArray();
      var data = new byte[length];
      using (var crypto = new RNGCryptoServiceProvider())
      {
        crypto.GetNonZeroBytes(data);
      }

      var result = new StringBuilder(length);
      foreach (var b in data)
        result.Append(chars[b % chars.Length]);
      return result.ToString();
    }

    public static DataTable GetDataTable2(long numRecords = 100)
    {
      var dataTable = new DataTable { TableName = "ArtificialTable2", Locale = new CultureInfo("en-gb") };
      dataTable.Columns.Add("string", typeof(string));
      for (long i = 1; i <= numRecords; i++)
      {
        var dr = dataTable.NewRow();
        dr[0] = i.ToString(CultureInfo.InvariantCulture);
        dataTable.Rows.Add(dr);
      }

      return dataTable;
    }

    public static DataTable GetDataTable(int numRecords = 100, bool addError = true)
    {
      var dataTable = new DataTable { TableName = "ArtificialTable", Locale = new CultureInfo("en-gb") };
      dataTable.Columns.Add("string", typeof(string));
      dataTable.Columns.Add("int", typeof(int));
      dataTable.Columns.Add("DateTime", typeof(DateTime));
      dataTable.Columns.Add("bool", typeof(bool));
      dataTable.Columns.Add("double", typeof(double));
      dataTable.Columns.Add("numeric", typeof(decimal));
      dataTable.Columns.Add("AllEmpty", typeof(string));
      dataTable.Columns.Add("PartEmpty", typeof(string));
      dataTable.Columns.Add("ID", typeof(int));
      dataTable.Columns.Add(ReaderConstants.cStartLineNumberFieldName, typeof(long));
      if (addError)
        dataTable.Columns.Add(ReaderConstants.cErrorField, typeof(string));

      dataTable.BeginLoadData();
      for (var i = 1; i <= numRecords; i++) AddRowToDataTable(dataTable, i, addError);
      dataTable.EndLoadData();
      return dataTable;
    }

    public static void AddRowToDataTable(DataTable dataTable, int recNum, bool addError)
    {
      var minDate = DateTime.Now.AddYears(-20).Ticks;
      var maxDate = DateTime.Now.AddYears(5).Ticks;
      var dr = dataTable.NewRow();
      dr[0] = GetRandomText(50);
      if (recNum % 10 == 0)
        dr[0] = dr[0] + "\r\nA Second Line";

      dr[1] = m_Random.Next(-300, +600);

      if (m_Random.NextDouble() > .2)
      {
        var dtm = Convert.ToInt64(((maxDate - minDate) * m_Random.NextDouble()) + minDate);
        dr[2] = new DateTime(dtm);
      }

      dr[3] = m_Random.Next(0, 2) == 0;

      dr[4] = m_Random.NextDouble() * 123.78;

      if (recNum % 3 == 0)
        dr[5] = m_Random.NextDouble();

      if (m_Random.NextDouble() > .4) dr[7] = GetRandomText(100);

      dr[8] = recNum; // ID
      dr[9] = recNum * 2; // #Line

      // Add Errors and Warnings to Columns and Rows
      var rand = m_Random.Next(0, 100);
      if (rand > 70)
      {
        var colNum = m_Random.Next(0, 10);
        if (rand < 85)
          dr.SetColumnError(colNum, "First Warning".AddWarningId());
        else if (rand > 85) dr.SetColumnError(colNum, @"First Error");

        // Add a possible second error in the same column
        rand = m_Random.Next(-2, 3);
        if (rand == 1)
          dr.SetColumnError(colNum, dr.GetColumnError(colNum).AddMessage("Second Warning".AddWarningId()));
        else if (rand == 2) dr.SetColumnError(colNum, dr.GetColumnError(colNum).AddMessage("Second Error"));
      }


      if (rand > 80) dr.RowError = rand > 90 ? @"Row Error" : @"Row Warning".AddWarningId();
      if (addError)
        dr[10] = dr.GetErrorInformation();

      dataTable.Rows.Add(dr);
    }
#pragma warning disable CA2211 // Non-constant fields should not be visible

    public static Column[] ColumnsDT2 =
    {
      new Column("string") //0
    };

    public static Column[] ColumnsDT =
    {
      new Column("string"), //0
      new Column("int", DataType.Integer), //1
      new Column("DateTime", DataType.DateTime), //2
      new Column("bool", DataType.Boolean), //3
      new Column("double", DataType.Double), //4
      new Column("numeric", DataType.Numeric), //5
      new Column("AllEmpty"), //6
      new Column("PartEmpty"), //7
      new Column("ID", DataType.Integer) //8
    };

#pragma warning restore CA2211 // Non-constant fields should not be visible
  }

}