/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CsvTools
{
  public static class ValueClusterCollectionExtensionString
  {
    delegate bool SpanPredicate(ReadOnlySpan<char> span);

    public static bool BuildValueClustersString(
    this ValueClusterCollection valueClusters,
    List<string> values,
    string escapedName,
    int max,
    double maxSeconds,
    IProgressWithCancellation progress)
    {
      if (values.Count == 0)
        return false;

      // values are guaranteed to be sorted
      var stopwatch = Stopwatch.StartNew();
      progress.Report(new ProgressInfo("Preparing value overview…",
          (long) (ValueClusterCollection.cPercentTyped * ValueClusterCollection.cProgressMax)));

      values.Sort(StringComparer.OrdinalIgnoreCase);

      int[] clusterLengths = { 1, 2, 4, 8, 12 };

      // Prepare cluster-prefix collections
      var clusters = clusterLengths.ToDictionary(
          length => length,
          length => new HashSet<string>(StringComparer.OrdinalIgnoreCase));

      var clusterIndividual = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      // Pre-compute once: minimal/maximal usable prefix length
      int maxLength = 0;
      int maxPossible = clusterLengths.Max();
      for (int i = 0; i < values.Count; i++)
      {
        var len = values[i]?.Length ?? 0;
        if (len > maxLength)
        {
          maxLength = len;
          if (maxLength >= maxPossible)
          {
            maxLength = maxPossible;
            break;
          }
        }
      }
      var allowedLengths = clusterLengths.Where(l => l <= maxLength).ToList();

      // -----------------------------
      // FIRST PASS (linear): collect prefix sets + individual values
      // -----------------------------
      foreach (var text in values)
      {
        if (text == null)
          continue;

        if (stopwatch.Elapsed.TotalSeconds > maxSeconds)
          throw new TimeoutException("Preparing overview took too long.");
        progress.CancellationToken.ThrowIfCancellationRequested();

        if (clusters[1].Count > max)
          break;

        if (clusterIndividual.Count < max)
          clusterIndividual.Add(text);

        foreach (var length in allowedLengths)
        {
          if (text.Length < length)
            continue;

          if (clusters[length].Count < max)
          {
            if (length == 1)
              clusters[length].Add(char.ToUpperInvariant(text[0]).ToString());
            else
              clusters[length].Add(text.Substring(0, 1).ToUpperInvariant() +
                  text.Substring(1, length - 1).ToLowerInvariant());
          }

        }
      }

      if (clusterIndividual.Count == 0)
        return false;

      var percent = ValueClusterCollection.cPercentTyped;

      // ------------------------------------------------------
      // CASE 1: Only a few distinct values → exact clustering
      // ------------------------------------------------------
      if (clusterIndividual.Count < max)
      {
        progress.Report("Creating groups from individual values…");

        var distinctValues = clusterIndividual.OrderBy(x => x).ToArray();
        foreach (var dv in distinctValues)
        {
          if (stopwatch.Elapsed.TotalSeconds > maxSeconds)
            throw new TimeoutException("Building groups took too long.");
          progress.CancellationToken.ThrowIfCancellationRequested();

          int count = CountBlock(values, dv);
          if (count > 0)
          {
            valueClusters.AddUnique(
                new ValueCluster(dv,
                    $"({escapedName} = '{dv.SqlQuote()}')",
                    count, dv));
          }
        }

        return true;
      }

      // ------------------------------------------------------
      // CASE 2: prefix-based clusters (if initial char cluster small)
      // ------------------------------------------------------
      if (clusters[1].Count < max)
      {
        progress.Report("Building groups based on common beginnings…");

        var bestCluster = allowedLengths
            .OrderByDescending(l => l)
            .Where(x => clusters[x].Count >= 1 && clusters[x].Count < max)
            .Select(x => clusters[x])
            .First();

        int prefixLength = bestCluster.First().Length;

        var step = (1.0 - ValueClusterCollection.cPercentTyped) / clusters[1].Count;

        foreach (var prefix in bestCluster)
        {
          progress.Report(new ProgressInfo(
              $"Building common beginning '{prefix}…'",
              (long) Math.Round(percent * ValueClusterCollection.cProgressMax)));

          int count = CountPrefixBlock(values, prefix.AsSpan(), prefixLength);
          percent += step;
          // Switch from Lke Statement to SUBSTRING for better performance
          // and issues with special characters like % and _
          valueClusters.AddUnique(
            new ValueCluster($"{prefix}…",
                $"(SUBSTRING({escapedName},1,{prefix.Length}) = '{prefix}')",
                count, prefix));
        }

        return true;
      }

      // ------------------------------------------------------
      // CASE 3: fallback to character ranges
      // ------------------------------------------------------
      var charRanges = new (string Display, char From, char To)[]
      {
        ("A to E", 'A', 'E'),
        ("F to K", 'F', 'K'),
        ("L to R", 'L', 'R'),
        ("S to Z", 'S', 'Z'),
        ("numbers", '0', '9'),
      };

      var step2 = (1.0 - ValueClusterCollection.cPercentTyped) / (charRanges.Length + 3);

      int countChar = 0;
      foreach (var block in charRanges)
      {
        percent += step2;
        progress.Report(new ProgressInfo($"Grouping values for {block.Display}",
            (long) Math.Round(percent * ValueClusterCollection.cProgressMax)));
        int count = CountCharRange(values, block.From, block.To);
        if (count > 0)
        {
          valueClusters.AddUnique(
              new ValueCluster(block.Display,
                  SqlSubstringRange(escapedName, block.From, block.To),
                  count, block.From, block.To));
        }

        countChar += count;
      }

      // Special characters (< 32)
      var countS = CountPredicate(values, x => x[0] < 32);
      percent += step2;
      progress.Report(new ProgressInfo("Grouping values for special characters",
          (long) Math.Round(percent * ValueClusterCollection.cProgressMax)));

      if (countS > 0)
        valueClusters.AddUnique(
            new ValueCluster("Special",
                $"(SUBSTRING({escapedName},1,1) < ' ')", countS, null));

      // Punctuation and symbols
      var countP = CountPredicate(values, x =>
             (x[0] >= ' ' && x[0] <= '/')
          || (x[0] >= ':' && x[0] <= '@')
          || (x[0] >= '[' && x[0] <= '`')
          || (x[0] >= '{' && x[0] <= '~'));

      percent += step2;
      progress.Report(new ProgressInfo("Grouping values for punctuation and symbols",
          (long) Math.Round(percent * ValueClusterCollection.cProgressMax)));

      if (countP > 0)
      {
        valueClusters.AddUnique(
            new ValueCluster("Punctuation & Marks & Braces",
                $"({SqlSubstringRange(escapedName, ' ', '/')} " +
                $"OR {SqlSubstringRange(escapedName, ':', '@')} " +
                $"OR {SqlSubstringRange(escapedName, '[', '`')} " +
                $"OR {SqlSubstringRange(escapedName, '{', '~')})",
                countP, ' '));
      }

      // Remaining values
      var countR = values.Count - countS - countP - countChar;
      progress.Report(new ProgressInfo("Grouping remaining values…",
          ValueClusterCollection.cProgressMax));

      if (countR > 0)
        valueClusters.AddUnique(
            new ValueCluster("Other",
                $"(SUBSTRING({escapedName},1,1) > '~')", countR, '~'));

      return true;


      // ------------------------------------------------------
      // Helper functions (sorting aware)
      // ------------------------------------------------------

      static int CountBlock(List<string> sorted, string text)
      {
        int idx = sorted.BinarySearch(text, StringComparer.OrdinalIgnoreCase);
        if (idx < 0) return 0;

        int count = 1;
        int i = idx - 1;
        while (i >= 0 && StringComparer.OrdinalIgnoreCase.Equals(sorted[i], text))
        {
          count++; i--;
        }
        i = idx + 1;
        while (i < sorted.Count && StringComparer.OrdinalIgnoreCase.Equals(sorted[i], text))
        {
          count++; i++;
        }
        return count;
      }

      static int CountPrefixBlock(List<string> sorted, ReadOnlySpan<char> prefix, int length)
      {
        int count = 0;
        for (int i = 0; i < sorted.Count; i++)
        {
          var span = sorted[i].AsSpan();
          if (span.Length < length)
            continue;

          if (span.Slice(0, length).Equals(prefix, StringComparison.OrdinalIgnoreCase))
          {
            count++;

            // because sorted → next values with same prefix follow
            int j = i + 1;
            while (j < sorted.Count)
            {
              var s2 = sorted[j].AsSpan();
              if (s2.Length >= length &&
                  s2.Slice(0, length).Equals(prefix, StringComparison.OrdinalIgnoreCase))
              {
                count++; j++;
              }
              else break;
            }
            break;
          }
        }
        return count;
      }

      static int CountCharRange(List<string> sorted, char from, char to)
      {
        int count = 0;
        foreach (var v in sorted)
        {
          char c = char.ToUpperInvariant(v[0]);
          if (c < from)
            continue;          // too early in alphabet → skip

          if (c > to)
            break;             // sorted → no further matches possible

          count++;
        }
        return count;
      }

      static int CountPredicate(List<string> sorted, Func<string, bool> pred)
      {
        int c = 0;
        foreach (var v in sorted)
          if (pred(v))
            c++;
        return c;
      }

      static string SqlSubstringRange(string escapedName, char from, char to)
          => $"(SUBSTRING({escapedName},1,1) >= '{from}' AND SUBSTRING({escapedName},1,1) <= '{to}')";
    }

  }
}
