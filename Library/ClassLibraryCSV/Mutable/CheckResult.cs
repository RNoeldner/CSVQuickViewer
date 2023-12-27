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
#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace CsvTools
{
  /// <summary>
  ///   Result of a format check, if the samples match a value type this is set, if not an example is give what did not match
  /// </summary>
  public sealed class CheckResult
  {
    private readonly ICollection<string> m_ExampleNonMatch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyCollection<ReadOnlyMemory<char>> ExampleNonMatch => m_ExampleNonMatch.Select(x=> x.AsMemory()).ToArray();

    /// <summary>
    ///   The found value format
    /// </summary>
    public ValueFormat? FoundValueFormat;

    /// <summary>
    ///   The positive matches before an invalid value was found
    /// </summary>
    public bool PossibleMatch;

    /// <summary>
    ///   The value format for a possible match
    /// </summary>
    public ValueFormat? ValueFormatPossibleMatch;

    /// <summary>
    /// Add value to the list of non matches
    /// </summary>
    /// <param name="value"></param>
    public void AddNonMatch(in string value)
    {
      if (!string.IsNullOrEmpty(value))
        m_ExampleNonMatch.Add(value);
    }

    /// <summary>
    ///   Combines a Sub check to an overall check
    /// </summary>
    /// <param name="subResult">The sub result.</param>
    public void KeepBestPossibleMatch(in CheckResult subResult)
    {
      if (!subResult.PossibleMatch || subResult.ExampleNonMatch.Count >= ExampleNonMatch.Count)
        return;
      m_ExampleNonMatch.Clear();
      PossibleMatch = true;
      ValueFormatPossibleMatch = subResult.ValueFormatPossibleMatch;

      foreach (var ex in subResult.ExampleNonMatch)
        if (!ex.IsEmpty)
          m_ExampleNonMatch.Add(ex.Span.ToString());
    }
  }
}