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

namespace CsvTools
{
  /// <summary>
  ///   Display Items are used for combo boxes
  /// </summary>
  public class DisplayItem<T>
  {
    /// <summary>
    ///   Initializes a new instance of the DisplayItem class.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="display">The display.</param>
    public DisplayItem(T id, string display)
    {
      ID = id;
      Display = display;
    }

    /// <summary>
    ///   Gets or sets the display.
    /// </summary>
    /// <value>The display.</value>
    public string Display { get; }

    /// <summary>
    ///   Gets or sets the ID.
    /// </summary>
    /// <value>The ID.</value>
    public T ID { get; }
  }
}