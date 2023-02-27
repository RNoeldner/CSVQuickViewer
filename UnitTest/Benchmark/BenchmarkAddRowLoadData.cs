using BenchmarkDotNet.Attributes;
using System;
using System.Data;
using System.Globalization;

namespace Benchmark;

[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class BenchmarkAddRowLoadData
{
  [Params(1000)]
  public int N = 1000;

  private DataTable GetDateTable()
  {
    var dataTable = new DataTable { TableName = "ArtificialTable", Locale = new CultureInfo("en-gb") };
    dataTable.Columns.Add("string", typeof(string));
    dataTable.Columns.Add("int", typeof(int));
    dataTable.Columns.Add("long", typeof(long));
    dataTable.Columns.Add("DateTime", typeof(DateTime));
    dataTable.Columns.Add("bool", typeof(bool));
    dataTable.Columns.Add("double", typeof(double));
    dataTable.Columns.Add("numeric", typeof(decimal));
    dataTable.Columns.Add("string2", typeof(string));
    dataTable.Columns.Add("string3", typeof(string));
    dataTable.Columns.Add("string4", typeof(string));
    dataTable.Columns.Add("string5", typeof(string));
    dataTable.Columns.Add("string6", typeof(string));
    dataTable.Columns.Add("double2", typeof(double));
    dataTable.BeginLoadData();
    var random = new Random(new Guid().GetHashCode());
    for (var i = 0; i < 3; i++)
    {
      var row = dataTable.NewRow();
      row["int"] = i;
      row["long"] = i * 100;
      row["string"] = $"Some Text {i}...";
      row["string2"] = $"Test{i + 1}";
      row["string3"] = $"Text {i * 2} !";
      row["string4"] = "";
      row["string5"] = DBNull.Value;
      row["string6"] = $"Longer Text {i * 2} .... {i:000000}";

      row["DateTime"] = new DateTime(random.Next(1900, 2030), random.Next(1, 12), 1).AddDays(random.Next(1, 31));
      row["double"] = random.NextDouble();
      row["double2"] = random.NextDouble();
      row["numeric"] = Convert.ToDecimal(random.NextDouble());
      dataTable.Rows.Add(row);
    }

    dataTable.EndLoadData();
    return dataTable;
  }

  [Benchmark]
  public void AddRow()
  {
    var dt = GetDateTable();
    var random = new Random(new Guid().GetHashCode());

    for (var i = 0; i < N; i++)
    {
      var dataRecord = dt.Rows[random.Next(0,2)];
      var destination = dt.NewRow();
      for (var column = 0; column < dt.Columns.Count; column++)
        destination[column] = dataRecord[column];
      dt.Rows.Add(destination);
    }
  }

  [Benchmark]
  public void LoadData()
  {
    var dt = GetDateTable();
    var random = new Random(new Guid().GetHashCode());

    for (var i = 0; i < N; i++)
    {
      var dataRecord = dt.Rows[random.Next(0,2)];
      var obj = new object[dt.Columns.Count];
      for (var column = 0; column < dt.Columns.Count; column++)
        obj[column] = dataRecord[column];
      dt.LoadDataRow(obj, true);
    }
  }

  [Benchmark]
  public void LoadDataParallel()
  {
    var dt = GetDateTable();
    var random = new Random(new Guid().GetHashCode());
    
    for (var i = 0; i < N; i++)
    {
      var obj = new object[dt.Columns.Count];
      var dataRecord = dt.Rows[random.Next(0,2)];
      Parallel.For(0, dt.Columns.Count, (column) => obj[column] = dataRecord[column]);
      dt.LoadDataRow(obj, true);
    }
  }
}