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
/// Defines a generalized method that a value type or class implements to create a type-specific deep copy of an instance.
/// </summary>
/// <typeparam name="T">The type of the object to be cloned.</typeparam>
public interface IWithClone<out T> where T : class
{
  /// <summary>
  /// Creates a new instance that is a deep copy of the current instance.
  /// </summary>
  /// <returns>A new instance of type <typeparamref name="T"/> with all properties and nested objects copied.</returns>
  T Clone();
}
