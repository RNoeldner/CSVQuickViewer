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

namespace CsvTools
{
  /// <summary>
  /// Attributes for enums to have a shorter description
  /// </summary>
  [AttributeUsage(AttributeTargets.All)]
  public class ShortDescriptionAttribute : Attribute
  {
    
    /// <summary>
    /// Short description
    /// </summary>
    public string ShortDescription { get;  }
    
    /// <inheritdoc />
    public ShortDescriptionAttribute(string value) => ShortDescription = value;
  }
}
