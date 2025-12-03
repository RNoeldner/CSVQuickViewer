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
using System.Collections.Generic;
using System.Globalization;

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
  /// Retrieves all date format strings whose defined length range fully
  /// contains the specified <paramref name="minLength"/> and <paramref name="maxLength"/> values.
  /// </summary>
  /// <param name="minLength">
  /// The minimum length of the input to compare against the format's supported range.
  /// </param>
  /// <param name="maxLength">
  /// The maximum length of the input to compare against the format's supported range.
  /// </param>
  /// <returns>
  /// A read-only collection of format strings whose length constraints include
  /// the given range.
  /// </returns>
  /// <remarks>
  /// The underlying collection is loaded lazily upon first access.
  /// Enumeration is safe as long as no other thread modifies the collection
  /// concurrently; concurrent additions or removals are not supported.
  /// </remarks>
  public IReadOnlyCollection<string> MatchingForLength(int minLength, int maxLength)
  {
    EnsureLoaded();

    // Make sure adding to collection while enumerating does not cause issues.
    List<string> matches = new();
    foreach (var kvp in this)
    {
      if (minLength >= kvp.Value.MinLength && maxLength <= kvp.Value.MaxLength)
        matches.Add(kvp.Key);
    }
    return matches;
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