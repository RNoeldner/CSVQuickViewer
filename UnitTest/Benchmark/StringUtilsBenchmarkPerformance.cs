using BenchmarkDotNet.Attributes;
using CsvTools;

namespace Benchmark
{
  [MemoryDiagnoser]
  [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
  [RankColumn]
  public class StringUtilsBenchmarkPerformance
  {
    [Params(1000)]
    public int N;

    [Benchmark]
    public void Span()
    {
      var src1 = @"ValidationTask".AsSpan();
      var src2 = @"Validation]Task".AsSpan();
      for (int i = 0; i < N; i++)
      {
        var test1 = src1.SqlName();
        var test2 = src2.SqlName();
      }
    }

    [Benchmark]
    public void String()
    {
      var src1 = @"ValidationTask";
      var src2 = @"Validation]Task";
      for (int i = 0; i < N; i++)
      {
        var test1 = src1.SqlName();
        var test2 = src2.SqlName();
      }
    }
  }

  [MemoryDiagnoser]
  [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
  [RankColumn]
  public class ReplaceDefaultsPerformance
  {
    [Params(1000)]
    public int N;

    [Benchmark]
    public void Char1()
    {
      for (int i = 0; i < N; i++)
      {
        "12345678901234567890".ReplaceDefaults('1', '-', '2', '.');
        "12345678901234567890".ReplaceDefaults('1', '-', '2', char.MinValue);
        "12345678901234567890".ReplaceDefaults('1', '-', 'x', '.');
        "12345678901234567890".ReplaceDefaults('1', char.MinValue, 'x', char.MinValue);
      }
    }
    
    [Benchmark]
    public void String()
    {
      for (int i = 0; i < N; i++)
      {
        "12345678901234567890".ReplaceDefaults("1", "-", "2", ".");
        "12345678901234567890".ReplaceDefaults("1", "-", "2", string.Empty);
        "12345678901234567890".ReplaceDefaults("1", "-", "x", ".");
        "12345678901234567890".ReplaceDefaults("1", string.Empty, "x", string.Empty);
      }
    }
  }
}
