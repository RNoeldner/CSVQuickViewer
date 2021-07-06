/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
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


namespace CsvTools
{
  using System.Linq;

  public class DateTimeFormatCollection
  {
    /// <summary>
    /// A lookup for minimum and maximum length by format description
    /// </summary>
    private readonly Dictionary<string, DateTimeFormatInformation> m_DateLengthMinMax = new Dictionary<string, DateTimeFormatInformation>();

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeFormatCollection"/> class.
    /// </summary>
    /// <param name="file">The file.</param>
    public DateTimeFormatCollection(string file)
    {
      using var reader = FileSystemUtils.GetStreamReaderForFileOrResource(file);
      
      while (!reader.EndOfStream)
      {
        var entry = reader.ReadLine();
        if (string.IsNullOrEmpty(entry) || entry[0] == '#')
          continue;
        Add(entry);
      }
    }

    /// <summary>
    /// Gets the Date Time Formats by format description
    /// </summary>
    /// <value>
    /// The keys.
    /// </value>
    public IEnumerable<string> Keys => m_DateLengthMinMax.Keys;

    /// <summary>
    /// Returns Date time formats that would fit the length of the input
    /// </summary>
    /// <param name="length">The length.</param>
    /// <param name="checkNamedDates">if set to <c>true</c> check named dates e.g. January, February</param>
    /// <returns></returns>
    public IEnumerable<string> MatchingForLength(int length, bool checkNamedDates) =>
      from kvFormatInformation in m_DateLengthMinMax
      where (checkNamedDates || !kvFormatInformation.Value.NamedDate) && length >= kvFormatInformation.Value.MinLength
                                                                      && length <= kvFormatInformation.Value.MaxLength
      select kvFormatInformation.Key;

    /// <summary>
    /// Tries the get value.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The <see cref="DateTimeFormatInformation"/>.</param>
    /// <returns><c>true</c> if key was found </returns>
    public bool TryGetValue(string key, out DateTimeFormatInformation value) => m_DateLengthMinMax.TryGetValue(key, out value);

    /// <summary>
    /// Adds the specified entry.
    /// </summary>
    /// <param name="entry">The entry.</param>
    private void Add(string entry)
    {
      if (string.IsNullOrWhiteSpace(entry))
        return;
      if (!m_DateLengthMinMax.ContainsKey(entry))
        m_DateLengthMinMax.Add(entry, new DateTimeFormatInformation(entry));
    }
  }
}