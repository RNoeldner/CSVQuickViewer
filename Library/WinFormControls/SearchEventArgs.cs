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

namespace CsvTools
{
  /// <summary>
  ///   Event Arguments for the Search
  /// </summary>
  public sealed class SearchEventArgs : EventArgs
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="SearchEventArgs" /> class.
    /// </summary>
    /// <param name="searchText">Text to search</param>
    /// <param name="result">Number of the result to focus</param>
    public SearchEventArgs(string searchText, int result = 1)
    {
      SearchText = searchText;
      Result = result;
    }

    /// <summary>
    ///   Gets or sets the result to be shown
    /// </summary>
    /// <value>The result.</value>
    public int Result { get; }

    /// <summary>
    ///   Gets or sets the search text.
    /// </summary>
    /// <value>The search text.</value>
    public string SearchText { get; }
  }
}