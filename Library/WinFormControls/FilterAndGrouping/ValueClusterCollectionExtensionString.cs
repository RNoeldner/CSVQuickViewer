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
  /// <summary>
  /// Provides extension methods to build groups of numeric, long, date and string values
  /// </summary>
#pragma warning disable MA0048 // File name must match type name
  public static partial class ValueClustersExtension
#pragma warning restore MA0048 // File name must match type name
  {
    private static readonly (string Display, char From, char To, char[] NonLatin)[] CharacterBlocks =
    {
        ("A to E", 'A', 'E', new [] { 'À','Á','Â','Ä','Ã','Å','Æ','Ç','È','É','Ê','Ë', 'Α','Β','Δ','Ε', 'А','Б','Д','Е','Э','Є'}),
        ("F to K", 'F', 'K', new [] { 'Φ','Κ', 'Ф','К','Х'}),
        ("L to R", 'L', 'R', new [] { 'Ł','Ñ','Ń','Ŕ','Ř','Ö', 'Λ','Ρ', 'Л','Р'}),
        ("S to Z", 'S', 'Z', new[] { 'Š', 'Ś', 'ẞ', 'Ť', 'Ü', 'Û', 'Ú', 'Ÿ', 'Ž', 'Ż', 'С', 'З', 'Ж'}),
        ("Numbers", '0', '9', Array.Empty<char>()),
      };

    public static (int nullCount, List<ValueCluster> clusters) BuildValueClustersString(
    this object[] objects,
    string escapedName,
    int max,
    double maxSeconds,
    IProgressWithCancellation progress)
    {
      var stopwatch = Stopwatch.StartNew();
      (int nullCount, var values) = MakeTypedValues(objects, Convert.ToString, progress);

      var valueClusters = new List<ValueCluster>();
      if (values.Count == 0)
        return (nullCount, valueClusters);
      values.Sort(StringComparer.OrdinalIgnoreCase);

      progress.Report(new ProgressInfo("Preparing value overview…", (long) (cTypedProgress * cMaxProgress)));
      int[] clusterLengths = { 1, 2, 3, 4, 5, 8, 12 };

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
        if (string.IsNullOrEmpty(text))
          continue;
        CheckTimeout(stopwatch, maxSeconds, progress.CancellationToken);
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
            var span = text.AsSpan();
            var prefix = string.Concat(
                char.ToUpperInvariant(span[0]),
                span.Slice(1, length - 1).ToString().ToLowerInvariant()
            );
            clusters[length].Add(prefix);
          }

        }
      }

      if (clusterIndividual.Count == 0)
        return (nullCount, valueClusters);

      var percent = cTypedProgress;

      // ------------------------------------------------------
      // CASE 1: Only a few distinct values → exact clustering
      // ------------------------------------------------------
      if (clusterIndividual.Count < max)
      {
        progress.Report("Creating groups from individual values…");

        foreach (var dv in clusterIndividual.OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase))
        {
          if (stopwatch.Elapsed.TotalSeconds > maxSeconds)
            throw new TimeoutException("Building groups took too long.");
          progress.CancellationToken.ThrowIfCancellationRequested();

          int count = CountBlockFullText(values!, dv);
          if (count > 0)
          {
            valueClusters.Add(
              new ValueCluster(dv, $"({escapedName} = '{dv.SqlQuote()}')", count));
          }
        }
        if (valueClusters.Count>0)
          return (nullCount, valueClusters);
      }

      // ------------------------------------------------------
      // CASE 2: prefix-based clusters (if initial char cluster small)
      // ------------------------------------------------------
      if (clusters[1].Count < max)
      {
        progress.Report("Building groups based on common beginnings…");

        // Get Suiting Groups 
        var allClusters = allowedLengths
            .Where(x => clusters[x].Count >= 1 && clusters[x].Count < max)
            .OrderByDescending(l => l);

        var counterEntries = 0;
        foreach (var clusterLength in allClusters)
          counterEntries += clusters[clusterLength].Count;

        var step = (1.0 - cTypedProgress) / counterEntries;
        var filtered = 0;
        foreach (var prefixLength in allClusters)
        {
          foreach (var prefix in clusters[prefixLength])
          {
            Report($"Building common beginning '{prefix}…'");

            int count = CountPrefixBlock(values!, prefix.AsSpan(), prefixLength);
            filtered+=count;
            percent += step;
            // Switch fromIncluding Lke Statement toIncluding SUBSTRING for better performance
            // and issues with special characters like % and _
            valueClusters.Add(
              new ValueCluster($"{prefix}…", $"(SUBSTRING({escapedName},1,{prefix.Length}) = '{prefix}')", count));
          }
          // If we have 90% do not add more clusters
          if (filtered>values.Count*.9)
            break;
        }

        FallbackToLengthClusters();
        return (nullCount, valueClusters);
      }

      // ------------------------------------------------------
      // CASE 3: fallback to Including character ranges
      // ------------------------------------------------------
      var step2 = (1.0 - cTypedProgress) / (CharacterBlocks.Length + 1);
      for (var i = 0; i<CharacterBlocks.Length; i++)
      {
        var (Display, From, To, NonLatin)= CharacterBlocks[i];
        percent += step2;
        Report($"Grouping values for {Display}");
        (int count, var nonLatin) = CountCharRange(values!, From, To, NonLatin);
        if (count > 0)
        {
          var sb = new System.Text.StringBuilder("(", 60);
          sb.Append($"SUBSTRING({escapedName},1,1) >= '{From}' AND SUBSTRING({escapedName},1,1) <= '{To}' OR ");
          foreach (var c in nonLatin)
            sb.Append($"SUBSTRING({escapedName},1,1) = '{c}' OR ");
          sb.Length-= 4; // remove last OR
          sb.Append(')');
          valueClusters.Add(
              new ValueCluster(Display, sb.ToString(), count));
        }
      }

      percent += step2;
      Report("Grouping values for punctuation and symbols");
      var extraSymbols = new HashSet<char> { '§', '€', '$', '£', '¥', '₩', '₽' };
      var foundChar = new HashSet<char>();
      var countP = 0;
      foreach (var x in values)
      {
        var c = x![0];
        if (char.IsPunctuation(c) || char.IsSymbol(c) || extraSymbols.Contains(c))
        {
          foundChar.Add(c);
          countP++;
        }
      }
      if (countP> 0)
      {
        var stringBuilder = new System.Text.StringBuilder("(");
        foreach (var x in foundChar)
        {
          stringBuilder.Append($"SUBSTRING({escapedName},1,1) = '");
          stringBuilder.Append(x == '\'' ? "''" : x.ToString());
          stringBuilder.Append("' OR ");
        }
        stringBuilder.Length -= 4; // remove last OR
        stringBuilder.Append(')');

        valueClusters.Add(
            new ValueCluster("Punctuation & Marks & Braces", stringBuilder.ToString(), countP));
      }
      FallbackToLengthClusters();
      return (nullCount, valueClusters);


      // ------------------------------------------------------
      // Helper functions (sorting aware)
      // ------------------------------------------------------
      void Report(string msg) => progress.Report(new ProgressInfo(msg, (long) (percent * cMaxProgress)));

      void FallbackToLengthClusters()
      {
        if (valueClusters.Count < 2 && values.Count > 2)
        {
          var groups = values.GroupBy(s => s!.Length).OrderBy(g => g.Key).ToList();
          // In case we wo not get a suitable number of groups do nothing
          if (groups.Count==1 || groups.Count > 10)
            return;
          Report("Grouping did not create meaningful distinctions; adding length-based groups…");
          // Merge very small groups into their neighbors
          const int minGroupSize = 3; // tune as needed
          var merged = new List<(int Key, List<string> Items)>();

          foreach (var g in groups)
          {
            var items = g.Cast<string>().ToList();
            if (items.Count < minGroupSize  && merged.Count > 0)
            {
              // try merge with previous
              if (merged.Count > 0 && merged[merged.Count - 1].Key < g.Key)
              {
                // merge into previous
                var prev = merged[merged.Count - 1];
                var combined = prev.Items.Concat(items).ToList();

                // Keep the key of the larger contributor (simple heuristic)
                int newLen = prev.Items.Count >= items.Count ? prev.Key : g.Key;

                // Replace the previous entry
                merged[merged.Count - 1] = (newLen, combined);
              }
              else
              {
                // no previous group to merge into, just add it
                merged.Add((g.Key, items));
              }
            }
            else
            {
              merged.Add((g.Key, items));
            }
          }

          // Build ValueCluster list using safe SQL
          foreach (var entry in merged)
          {
            int len = entry.Key;
            int count = entry.Items.Count;

            // NOTE: adjust LEN -> LENGTH if your SQL dialect requires it
            var sql = $"(LEN({escapedName}) = {len})";

            valueClusters.Add(new ValueCluster($"Length {len}", sql, count)
            );
          }
        }
      }

      static int CountBlockFullText(List<string> sorted, string text)
      {
        // BinarySearch does not guarantee returning the first matching element, in case there are many its one of them
        int idx = sorted.BinarySearch(text, StringComparer.OrdinalIgnoreCase);
        if (idx < 0) return 0;

        int count = 1;
        // Look backwards
        int i = idx - 1;
        while (i >= 0 && StringComparer.OrdinalIgnoreCase.Equals(sorted[i], text))
        {
          count++; i--;
        }
        // Look forwards
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

      static (int count, HashSet<char> foundNonLatin) CountCharRange(List<string> sorted, char fromIncluding, char toIncluding, char[] nonLatin)
      {
        int count = 0;
        var minChar = fromIncluding;
        var maxChar = toIncluding;
        for (int i = 0; i < nonLatin.Length; i++)
        {
          if (nonLatin[i]<minChar)
            minChar = nonLatin[i];
          if (nonLatin[i]>maxChar)
            maxChar = nonLatin[i];
        }
        var foundNonLatin = new HashSet<char>();
        foreach (var v in sorted)
        {
          char c = char.ToUpperInvariant(v[0]);
          if (c < minChar)
            continue;          // too early in alphabet → skip

          if (c > maxChar)
            break;             // sorted → no further matches possible

          if (c >= fromIncluding && c <= toIncluding)
          {
            count++;
            continue;
          }

          if (nonLatin.Contains(c))
          {
            count++;
            foundNonLatin.Add(c);
          }
        }
        return (count, foundNonLatin);
      }

    }
  }
}
