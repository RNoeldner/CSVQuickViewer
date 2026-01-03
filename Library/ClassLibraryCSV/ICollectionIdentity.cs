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
///   Class should support CollectionIdentifier to support uniqueness and finding in collection
/// </summary>
public interface ICollectionIdentity
{
  /// <summary>
  /// Name of the property that backs the unique key
  /// </summary>
  string UniqueKeyPropertyName { get; }

  /// <summary>
  /// Returns the string key used for uniqueness in collections.
  /// </summary>
  string GetUniqueKey();

  /// <summary>
  /// Optionally allows updating the key if needed.
  /// </summary>
  void SetUniqueKey(string key);
}