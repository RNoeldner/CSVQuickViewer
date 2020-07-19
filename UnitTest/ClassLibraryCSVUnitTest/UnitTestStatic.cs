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
  internal static class UnitTestStatic
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


    private static string GetRandomText(int length)
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

    public static DataTable GetDataTable(int numRecords = 100)
    {
      var dataTable = new DataTable {TableName = "ArtificialTable", Locale = new CultureInfo("en-gb")};
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

      var minDate = DateTime.Now.AddYears(-20).Ticks;
      var maxDate = DateTime.Now.AddYears(5).Ticks;
      dataTable.BeginLoadData();

      for (var i = 1; i <= numRecords; i++)
      {
        var dr = dataTable.NewRow();
        dr[0] = GetRandomText(50);
        if (i % 10 == 0)
          dr[0] = dr[0] + "\r\nA Second Line";

        dr[1] = m_Random.Next(-300, +600);

        if (m_Random.NextDouble() > .2)
        {
          var dtm = Convert.ToInt64(((maxDate - minDate) * m_Random.NextDouble()) + minDate);
          dr[2] = new DateTime(dtm);
        }

        dr[3] = m_Random.Next(0, 2) == 0;

        dr[4] = m_Random.NextDouble() * 123.78;

        if (i % 3 == 0)
          dr[5] = m_Random.NextDouble();

        if (m_Random.NextDouble() > .4) dr[7] = GetRandomText(100);

        dr[8] = i; // ID
        dr[9] = i * 2; // #Line

        dataTable.Rows.Add(dr);
      }

      dataTable.EndLoadData();
      return dataTable;
    }
  }
}