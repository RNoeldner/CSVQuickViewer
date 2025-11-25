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
namespace CsvTools;
/// <summary>
/// Represents a displayable item, typically used for ComboBox or ListBox data binding.
/// </summary>
/// <typeparam name="T">Type of the underlying value or identifier.</typeparam>
#if NETFRAMEWORK
  public sealed class DisplayItem<T>
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
    /// Gets the display text for this item.
    /// </summary>
    public string Display { get; }

    /// <summary>
    /// Gets the underlying identifier or value.
    /// </summary>
    public T ID { get; }

    /// <summary>
    /// Returns the display text.
    /// </summary>
    public override string ToString() => Display ?? string.Empty;
  }
#else
/// <summary>
/// Represents a displayable item, typically used for ComboBox or ListBox data binding.
/// </summary>
/// <typeparam name="T">Type of the underlying value or identifier.</typeparam>
public sealed record class DisplayItem<T>(T ID, string Display)
{
  public override string ToString() => Display ?? string.Empty;
}
#endif