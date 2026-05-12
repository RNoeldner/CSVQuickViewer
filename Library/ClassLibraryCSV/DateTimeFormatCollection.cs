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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CsvTools;

/// <summary>
/// Class with information on DateTime formats
/// </summary>
/// <remarks>
/// Enumerating the dictionary in MatchingForLength is still not fully thread-safe if another thread adds entries concurrently. 
/// </remarks>
public sealed class DateTimeFormatCollection : Dictionary<string, DateTimeFormatInformation>
{
  /// <summary>
  ///   A lookup for minimum and maximum length by format description
  /// </summary>
  private readonly string m_FileName;

  /// <summary>
  ///   Initializes a new instance of the <see cref="DateTimeFormatCollection" /> class.
  /// </summary>
  /// <param name="file">The file to load the default values from. It will be checked if it's a built-in resource</param>
  public DateTimeFormatCollection(string file)
  {
    m_FileName = file;
  }

  /// <summary>
  ///   Check if the length of the provided string could fit to the date format
  /// </summary>
  /// <param name="actualLength">The actual value.</param>
  /// <param name="dateFormat">The date format to check.</param>
  /// <remarks>Uses Lazy loading and is not thread safe</remarks>
  /// <returns><c>true</c> if key was found</returns>    
  public bool DateLengthMatches(int actualLength, string dateFormat)
  {
    if (actualLength<4)
      return false;
    EnsureLoaded();

    if (TryGetValue(dateFormat, out var lengthMinMax))
      return actualLength >= lengthMinMax.MinLength && actualLength <= lengthMinMax.MaxLength;

    lengthMinMax = AddInternal(dateFormat);

    return actualLength >= lengthMinMax.MinLength && actualLength <= lengthMinMax.MaxLength;
  }

  private void EnsureLoaded()
  {
    if (Count != 0)
      return;

    using var reader = FileSystemUtils.GetStreamReaderForFileOrResource(m_FileName);
    while (!reader.EndOfStream)
    {
      var entry = reader.ReadLine();
      if (string.IsNullOrEmpty(entry) || entry[0] == '#')
        continue;
      AddInternal(entry.Trim());
    }
    AddInternal(CultureInfo.CurrentCulture.DateTimeFormat.FullDateTimePattern);
    AddInternal(CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern);
    AddInternal(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
    AddInternal(CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern);
    AddInternal(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);
  }

  /// <summary>
  /// Retrieves all date format strings that accommodate the specified length range, 
  /// ordered from most specific to least specific.
  /// </summary>
  /// <param name="minLength">The minimum length of the input string to match.</param>
  /// <param name="maxLength">The maximum length of the input string to match.</param>
  /// <returns>
  /// A read-only collection of format strings, sorted by their specificity score. 
  /// Formats that precisely match the input length and avoid complex naming (like month names) 
  /// appear first.
  /// </returns>
  /// <remarks>
  /// The scoring heuristic prioritizes:
  /// <list type="number">
  /// <item><description>Exact length matches (highest priority).</description></item>
  /// <item><description>Narrower supported length ranges (greater precision).</description></item>
  /// <item><description>Numeric-heavy formats over those containing written names (e.g., MMM, ddd).</description></item>
  /// <item><description>Standard precision over sub-second precision for shorter inputs.</description></item>
  /// </list>
  /// The underlying collection is loaded lazily. This method is thread-safe for reading, 
  /// but does not support concurrent modifications to the source collection.
  /// </remarks>
  public IReadOnlyCollection<string> MatchingForLength(int minLength, int maxLength)
  {
    EnsureLoaded();
    var candidates = new List<(string Format, int Score)>();

    foreach (var kvp in this)
    {
      var format = kvp.Key;
      var range = kvp.Value;

      // Check if the input range is fully contained within the format's supported range
      if (range.MinLength <= minLength && range.MaxLength >= maxLength)
        candidates.Add((format, CalculateSpecificity(format, range)));
    }
    return candidates
        .OrderByDescending(c => c.Score)
        .Select(c => c.Format)
        .ToList()
        .AsReadOnly();

    int CalculateSpecificity(ReadOnlySpan<char> format, DateTimeFormatInformation range)
    {
      int score = 0;

      // 1. Exact Match Bonus: Highest priority if the format length 
      // exactly matches the input length.
      if (range.MinLength == minLength && range.MaxLength == maxLength)
        score += 1000;

      // 2. Precision Penalty: The wider the format's supported range compared 
      // to the input, the less specific it is.
      score -= (range.MaxLength - range.MinLength);

      // 3. Complexity Penalty: Discourage formats with written names (MMMM, dddd)
      // as they are statistically less common for raw data input.
      if (format.Contains("MMM", StringComparison.Ordinal)) score -= 100;
      if (format.Contains("ddd", StringComparison.Ordinal)) score -= 100;

      // 4. Sub-second Penalty: If our input is likely a standard date (short),
      // don't prioritize high-precision formats (like .FFFF) unless necessary.
      if (maxLength < 20 && (format.Contains(".F", StringComparison.Ordinal) || format.Contains(".f", StringComparison.Ordinal)))
        score -= 100;
      
      // 5. Penalize am/pm descriptors
      if (format.Contains("tt", StringComparison.Ordinal))
        score -= 50;

      // 6. Variable-Length Component Penalty
      // We penalize single-character tokens if a double-character equivalent exists.
      // This prioritizes "HH:mm:ss" over "H:m:s" when the input length allows for both.
      score -= CountSingleCharTokens(format) * 5;

      return score;
    }

    int CountSingleCharTokens(ReadOnlySpan<char> format)
    {
      int penalties = 0;
      // Removed 'y' and 's' per your requirement
      ReadOnlySpan<char> targets = stackalloc char[] { 'M', 'd', 'H', 'h', 'm' };
      bool inQuotes = false;

      for (int i = 0; i < format.Length; i++)
      {
        char current = format[i];

        // Handle quoted literals: characters inside '...' should not be penalized
        if (current == '\'')
        {
          inQuotes = !inQuotes;
          continue;
        }
        if (inQuotes) continue;

        // Handle escaped characters: \H should not be penalized
        if (current == '\\')
        {
          i++;
          continue;
        }

        bool isTarget = false;
        foreach (var t in targets)
        {
          if (current == t)
          {
            isTarget = true;
            break;
          }
        }

        if (isTarget)
        {
          bool hasNeighbor = (i > 0 && format[i - 1] == current) ||
                             (i < format.Length - 1 && format[i + 1] == current);

          if (!hasNeighbor)
          {
            penalties++;
          }
          else
          {
            // Skip the rest of this token block (e.g., jump past 'MM' or 'HHH')
            while (i < format.Length - 1 && format[i + 1] == current) i++;
          }
        }
      }
      return penalties;
    }
  }

  /// <summary>
  /// Adding an entry. The value stored is a DateTimeFormatInformation object that analyzes the format to determine min and max lengths.
  /// </summary>
  /// <param name="entry">New DateTime format</param>
  private DateTimeFormatInformation AddInternal(string entry)
  {
    var dtinfo = new DateTimeFormatInformation(entry);
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    TryAdd(entry, dtinfo);
#else
    if (!ContainsKey(entry))
      Add(entry, new DateTimeFormatInformation(entry));
#endif
    return dtinfo;
  }
}