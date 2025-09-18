using BenchmarkDotNet.Attributes;
using CsvTools;
using CsvTools.Tests;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
namespace Benchmarking
{
  public class DataReaderBenchmark
  {
    private DataReaderWrapper _wrapper = null!;
    private DataTable _dataTable = null;
    private CancellationToken _cancellationToken;

    [Params(500, 1000000)]
    public int RowCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
      _dataTable = UnitTestStaticData.GetDataTable(RowCount, true, false, false, true, out long errorCount, out long warningCount);
      _wrapper = new DataReaderWrapper(_dataTable.CreateDataReader(), true, false, false, true);
      _cancellationToken = CancellationToken.None;
    }

    [Benchmark(Baseline = true)]
    public async Task<DataTable> OldImplementation()
    {
      return await _wrapper.GetDataTableAsync(
          TimeSpan.MaxValue, null, _cancellationToken);
    }

    [Benchmark]
    public async Task<DataTable> NewBatchImplementation()
    {
      return await _wrapper.GetDataTableAsyncOptimized(
          TimeSpan.MaxValue, null, _cancellationToken);
    }
  }
}