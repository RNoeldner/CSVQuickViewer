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
using System.ComponentModel;


namespace CsvTools;

/// <summary>
/// Interface defining a font configuration with a name and size.
/// This allows forms to listen for changes via PropertyChanged.
/// </summary>
public interface IFontConfig : INotifyPropertyChanged
{
  /// <summary>
  /// Gets the font family name (e.g., "Segoe UI").
  /// </summary>
  string Font { get; set; }

  /// <summary>
  /// Gets the font size in points (typical value 8.25F).
  /// </summary>
  float FontSize { get; set; }
}

/// <summary>
/// Concrete implementation of IFontConfig with mutable font properties.
/// Raising PropertyChanged ensures that forms can update dynamically.
/// </summary>
public class FontConfig : ObservableObject, IFontConfig
{
  // Backing fields for font name and size
  private string m_Font;
  private float m_FontSize;

  /// <summary>
  /// Default Font
  /// </summary>
  public static FontConfig Default = new();

  /// <summary>
  /// Initializes a new instance of FontConfig.
  /// </summary>
  /// <param name="font">Font family name (default: "Segoe UI").</param>
  /// <param name="fontSize">Font size in points (default: 8.25F).</param>
  public FontConfig(string font = "Segoe UI", float fontSize = 8.25F)
  {
    m_Font= font;
    m_FontSize= fontSize;
  }

  /// <summary>
  /// Gets or sets the font family.
  /// When set, raises PropertyChanged for data binding or form updates.
  /// </summary>
  public string Font
  {
    get => m_Font;
    set
    {
      // Ensure non-null and trimmed value
      if (!string.IsNullOrWhiteSpace(value))
      {
        SetProperty(ref m_Font, value.Trim());
      }
    }
  }

  /// <summary>
  /// Gets or sets the font size in points.
  /// When set, raises PropertyChanged to notify forms of updates.
  /// </summary>
  public float FontSize
  {
    get => m_FontSize;
    set
    {
      // Clamp font size to reasonable range (1 to 72 points)
      float newSize = Math.Max(1f, Math.Min(72f, value));
      SetProperty(ref m_FontSize, newSize);
    }
  }
}