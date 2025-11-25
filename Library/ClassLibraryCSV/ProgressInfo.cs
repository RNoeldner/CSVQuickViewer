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

namespace CsvTools
{
  /// <summary>
  /// Immutable class storing information and progress value
  /// </summary>
  public class ProgressInfo
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="T:CsvTools.ProgressInfo" /> class.
    /// </summary>
    /// <param name="text">The informational text.</param>
    public ProgressInfo(string text)
    {
      Text = text;
      Value = -1;
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="T:CsvTools.ProgressInfo" /> class.
    /// </summary>
    /// <param name="text">The informational text.</param>
    /// <param name="value">The progress value.</param>
    public ProgressInfo(string text, long value)
    {
      Text = text;
      Value = value;
    }

    /// <summary>
    /// Allows automatic conversion from string to ProgressInfo.
    /// Example:
    ///   ProgressInfo info = "Loading...";
    /// </summary>
    public static implicit operator ProgressInfo(string text)
        => new ProgressInfo(text);

    /// <summary>
    ///   Gets the text.
    /// </summary>
    /// <value>The text.</value>
    public string Text { get; }

    /// <summary>
    ///   Gets the value.
    /// </summary>
    /// <value>The value.</value>
    public long Value { get; }
  }
}
