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

using System;
using System.Collections.Generic;

namespace CsvTools
{
  /// <summary>
  ///  Result of a format check, if the samples match a value type this is set, if not an example is give what did not match
  /// </summary>
  public class CheckResult
  {
    public ICollection<string> ExampleNonMatch { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///  The found value format
    /// </summary>
    public ValueFormat FoundValueFormat { get; set; }

    /// <summary>
    ///  The positive matches before an invalid value was found
    /// </summary>
    public bool PossibleMatch { get; set; }

    /// <summary>
    ///  The value format for a possible match
    /// </summary>
    public ValueFormat ValueFormatPossibleMatch { get; set; }

    /// <summary>
    ///  Combines a Sub check to an overall check
    /// </summary>
    /// <param name="subResult">The sub result.</param>
    public void KeepBestPossibleMatch(CheckResult subResult)
    {
      if (subResult == null || !subResult.PossibleMatch) return;

      if (this.PossibleMatch == false || subResult.ExampleNonMatch.Count < this.ExampleNonMatch.Count)
      {
        ExampleNonMatch.Clear();
        this.PossibleMatch = true;
        this.ValueFormatPossibleMatch = subResult.ValueFormatPossibleMatch;

        foreach (var ex in subResult.ExampleNonMatch)
        {
          if (string.IsNullOrEmpty(ex)) continue;
          this.ExampleNonMatch.Add(ex);
        }
      }
    }
  }
}