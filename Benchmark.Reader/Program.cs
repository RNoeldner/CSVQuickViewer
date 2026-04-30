using BenchmarkDotNet.Running;

namespace Benchmark.Reader
{
  internal class Program
  {
    static void Main(string[] args)
    {
      var _ = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
  }
}
