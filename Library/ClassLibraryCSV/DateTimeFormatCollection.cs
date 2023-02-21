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
using System.Globalization;

namespace CsvTools
{
  public sealed class DateTimeFormatCollection : Dictionary<string, DateTimeFormatInformation>
  {
    /// <summary>
    ///   A lookup for minimum and maximum length by format description
    /// </summary>
    private readonly string m_FileName;

    /// <summary>
    ///   Initializes a new instance of the <see cref="DateTimeFormatCollection" /> class.
    /// </summary>
    /// <param name="file">The file.</param>
    public DateTimeFormatCollection(string file)
    {
      m_FileName = file;
    }

    private void Load()
    {
      using var reader = FileSystemUtils.GetStreamReaderForFileOrResource(m_FileName);
      while (!reader.EndOfStream)
      {
        var entry = reader.ReadLine();
        if (string.IsNullOrEmpty(entry) || entry[0] == '#')
          continue;
        Add(entry.Trim());
      }
      Add(CultureInfo.CurrentCulture.DateTimeFormat.FullDateTimePattern);
      Add(CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern);
      Add(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
      Add(CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern);
      Add(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);
    }

    private void Add(in string entry)
    {
      if (string.IsNullOrWhiteSpace(entry))
        return;
      if (!ContainsKey(entry))
        Add(entry, new DateTimeFormatInformation(entry));
    }

    /// <summary>
    ///   Returns Date time formats that would fit the length of the input
    /// </summary>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public IEnumerable<string> MatchingForLength(int length)
    {
      if (Count==0)
        Load();
      var retList = new List<string>();
      using var curEnum = GetEnumerator();
      while (curEnum.MoveNext())
      {
        var item = curEnum.Current;
        if (length >= item.Value.MinLength && length <= item.Value.MaxLength)
          retList.Add(item.Key);
      }
      return retList;
    }

    /// <summary>
    ///   Check if the length of the provided string could fit to the date format
    /// </summary>
    /// <param name="actualLength">The actual value.</param>
    /// <param name="dateFormat">The date format to check.</param>
    /// <returns><c>true</c> if key was found</returns>    
    public bool DateLengthMatches(int actualLength, in string dateFormat)
    {
      if (actualLength<4)
        return false;

      if (Count==0)
        Load();

      if (TryGetValue(dateFormat, out var lengthMinMax))
        return actualLength >= lengthMinMax.MinLength && actualLength <= lengthMinMax.MaxLength;

      lengthMinMax = new DateTimeFormatInformation(dateFormat);
      base.Add(dateFormat, lengthMinMax);

      return actualLength >= lengthMinMax.MinLength && actualLength <= lengthMinMax.MaxLength;
    }
  }
}