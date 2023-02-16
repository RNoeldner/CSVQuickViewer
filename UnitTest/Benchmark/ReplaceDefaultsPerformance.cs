using BenchmarkDotNet.Attributes;
using CsvTools;

namespace Benchmark
{
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
