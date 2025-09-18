using BenchmarkDotNet.Running;

namespace Benchmarking
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var summary = BenchmarkRunner.Run<DataReaderBenchmark>();
    }
  }
}
