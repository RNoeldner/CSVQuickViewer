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

using System.ComponentModel;


namespace CsvTools
{
  /// <summary>
  /// IFontConfig interface defines font configuration properties.
  /// </summary>
  public interface IFontConfig : INotifyPropertyChanged
  {
    /// <summary>
    /// Gets the font name could be Segoe UI
    /// </summary>
    string Font { get; }

    /// <summary>
    /// Gets the font size, usual values are 8.25F
    /// </summary>
    float FontSize { get; }
  }

  /// <summary>
  /// FontConfig class implements IFontConfig interface to provide concrete font configuration.
  /// </summary>
  public class FontConfig : ObservableObject, IFontConfig
  {
    private readonly string m_Font;
    private readonly float m_FontSize;
    /// <summary>
    /// Initializes a new instance of the <see cref="FontConfig"/> class.
    /// </summary>
    /// <param name="font">The font name.</param>
    /// <param name="fontSize">Size of the font.</param>
    public FontConfig(string? font = null, float? fontSize = null)
    {
      m_Font=font ?? "Segoe UI";
      m_FontSize=fontSize ?? 8.25F;
    }

    /// <inheritdoc/>
    public string Font => m_Font;

    /// <inheritdoc/>
    public float FontSize => m_FontSize;
  }
}

