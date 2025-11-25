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
#nullable enable
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace CsvTools;

[Serializable]
public sealed class WindowState
{
  public static readonly WindowState Default =
    new WindowState(10, 10, 600, 600, FormWindowState.Normal, int.MinValue, string.Empty);

  [JsonConstructor]
  public WindowState(int? left, int? top, int? width, int? height, FormWindowState? state = null,
    int? customInt = null, string? customText = null)
  {
    Left = left ?? 10;
    Top = top ?? 10;
    Width = width ?? 600;
    Height = height ?? 600;
    State = state ?? FormWindowState.Normal;
    CustomInt = customInt ?? int.MinValue;
    CustomText = customText ?? string.Empty;
  }

  public readonly int Left;
  public readonly int Top;
  public readonly int Width;
  public readonly int Height;

  [DefaultValue(FormWindowState.Normal)] public readonly FormWindowState State;

  /// <summary>
  ///   Store form specific values like selected Tab or Splitter distance, can store any value but -1
  /// </summary>
  [DefaultValue(-2147483648)] public readonly int CustomInt;

  /// <summary>
  ///   Store form specific values like a filterText
  /// </summary>
  [DefaultValue("")] public readonly string CustomText;
}