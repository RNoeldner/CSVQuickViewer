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
using System.Collections;
using System.ComponentModel;

namespace CsvTools
{
  /// <summary>
  ///   A Comparer for a group or hierarchy structure
  /// </summary>
  internal class TwoLevelComparer : IComparer
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="TwoLevelComparer" /> class.
    /// </summary>
    /// <param name="firstLevel">The first level.</param>
    /// <param name="secondLevel">The second level.</param>
    public TwoLevelComparer(PropertyDescriptor firstLevel, PropertyDescriptor secondLevel)
    {
      FirstLevel = firstLevel ?? throw new ArgumentNullException("firstLevel");
      SecondLevel = secondLevel ?? throw new ArgumentNullException("secondLevel");
    }

    public PropertyDescriptor FirstLevel { get; }

    public PropertyDescriptor SecondLevel { get; }

    /// <summary>
    ///   Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>
    ///   A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in
    ///   the following table.Value Meaning Less than zero <paramref name="x" /> is less than <paramref name="y" />. Zero
    ///   <paramref name="x" /> equals <paramref name="y" />. Greater than zero <paramref name="x" /> is greater than
    ///   <paramref name="y" />.
    /// </returns>
    public int Compare(object x, object y)
    {
      if (y == null)
        throw new ArgumentNullException(nameof(y));
      if (x == null)
        throw new ArgumentNullException(nameof(x));
      var res = StringComparer.CurrentCultureIgnoreCase.Compare(Convert.ToString(FirstLevel.GetValue(x)),
        Convert.ToString(FirstLevel.GetValue(y)));
      if (res != 0)
        return res;
      return StringComparer.CurrentCultureIgnoreCase.Compare(Convert.ToString(SecondLevel.GetValue(x)),
        Convert.ToString(SecondLevel.GetValue(y)));
    }
  }
}