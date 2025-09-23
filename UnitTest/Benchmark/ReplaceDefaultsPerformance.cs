/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael N�ldner
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

