using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Data;

namespace CsvTools.Tests
{
  [TestClass]
  public class DetailControlTests
  {
    [TestMethod]
    public void DetailControlTest()
    {
      using (var dt = new DataTable())
      {
        dt.Columns.Add(new DataColumn
        {
          ColumnName = "ID",
          DataType = typeof(int)
        });
        dt.Columns.Add(new DataColumn
        {
          ColumnName = "Text",
          DataType = typeof(string)
        });
        dt.Columns.Add(new DataColumn
        {
          ColumnName = "Date",
          DataType = typeof(DateTime)
        });
        dt.Columns.Add(new DataColumn
        {
          ColumnName = "Bool",
          DataType = typeof(bool)
        });
        for (var line = 1; line < 5000; line++)
        {
          var row = dt.NewRow();
          row[0] = line;
          row[1] = $"This is text {line / 2}";
          row[2] = new DateTime(2001, 6, 6).AddHours(line * 3);
          row[3] = line % 3 == 0;
          if (SecureString.Random.Next(1, 10) == 5)
            row.SetColumnError(SecureString.Random.Next(0, 3), "Error");
          if (SecureString.Random.Next(1, 50) == 5)
            row.RowError = "Row Error";
          dt.Rows.Add(row);
        }

        using (var dc = new DetailControl())
        {
          dc.Show();
          dc.DataTable = dt;
          dc.OnlyShowErrors();
          dc.MoveMenu();
        }
      }
    }

    [TestMethod]
    public void SortTest()
    {
      using (var dt = new DataTable())
      {
        dt.Columns.Add(new DataColumn
        {
          ColumnName = "ID",
          DataType = typeof(int)
        });
        dt.Columns.Add(new DataColumn
        {
          ColumnName = "Text",
          DataType = typeof(string)
        });
        dt.Columns.Add(new DataColumn
        {
          ColumnName = "Date",
          DataType = typeof(DateTime)
        });
        dt.Columns.Add(new DataColumn
        {
          ColumnName = "Bool",
          DataType = typeof(bool)
        });
        for (var line = 1; line < 5000; line++)
        {
          var row = dt.NewRow();
          row[0] = SecureString.Random.Next(1, 5000);
          row[1] = $"This is text {line / 2}";
          row[2] = new DateTime(2001, 6, 6).AddHours(line * 3);
          row[3] = line % 3 == 0;
          dt.Rows.Add(row);
        }

        using (var dc = new DetailControl())
        {
          dc.Show();
          dc.DataTable = dt;
          dc.Sort("ID", ListSortDirection.Ascending);
        }
      }
    }
  }
}