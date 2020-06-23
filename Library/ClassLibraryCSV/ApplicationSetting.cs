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

using JetBrains.Annotations;

namespace CsvTools
{
  /// <summary>
  ///   Static class to access application wide settings, currently HTMLStyle, FillGuessSetting and
  ///   a ColumnHeaderCache
  /// </summary>
  public static class ApplicationSetting
  {
    /// <summary>
    ///   The Application wide HTMLStyle
    /// </summary>
    [NotNull]
    public static HTMLStyle HTMLStyle { get; } = new HTMLStyle();

    /// <summary>
    ///   General Setting that determines if the menu is display in the bottom of a detail control
    /// </summary>
    public static bool MenuDown { get; set; }


    //TODO: Get rid of this, its used in get Full Path, it should be stored only once in ToolSetting
    [NotNull] 
    public static string RootFolder { get; set; } = ".";
  }
}