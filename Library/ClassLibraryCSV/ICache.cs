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

using System;
using System.Collections.Generic;

namespace CsvTools
{
  /// <summary>
  ///  A Cache that does invalidate entries after a give set of time
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  public interface ICache<TKey, TValue> : IDisposable
   where TKey : IComparable
   where TValue : class
  {
    /// <summary>
    ///  Determines whether the specified key contains key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns><c>true</c> if the key is in the cache, <c>false</c> otherwise</returns>
    bool ContainsKey(TKey key);

    /// <summary>Collection of all stored Keys</summary>
    ICollection<TKey> Keys { get; }

    /// <summary>
    ///  Flushes all cached items in the cache
    /// </summary>
    void Flush();

    /// <summary>
    ///  Retrieves an item of a defined type from the cache.
    ///  If an item is found, but its type does not the specified type,
    ///  then null is returned.
    ///  This ensures that assignment of cached items can be performed
    ///  without error handlers for invalid casts.
    /// </summary>
    /// <param name="key">Specifies items inside the cache.</param>
    /// <returns>
    ///  The object found in the cache. If no object could be found in the cache,
    ///  or the found object has exceeded its lifetime, null is returned. No exception
    ///  is thrown.
    /// </returns>
    TValue Get(TKey key);

    /// <summary>
    ///  Removes an item from the cache.
    /// </summary>
    /// <param name="key">Specifies an item in the cache.</param>
    void Remove(TKey key);

    /// <summary>
    ///  Adds an item to the cache with a default lifetime of 5 minutes (300 seconds)
    /// </summary>
    /// <param name="key">Identifies an item in the cache. The key is not case-sensitive.</param>
    /// <param name="item">Object to store in the cache.</param>
    /// <remarks>
    ///  If an item with the same key exists in the cache, it is overwritten.
    ///  The default lifetime is applied. If the item is <c>null</c> the item will be removed from cache.
    /// </remarks>
    void Set(TKey key, TValue item);

    /// <summary>
    ///  Adds an item to the cache.
    /// </summary>
    /// <param name="key">Identifies an item in the cache. The key is not case-sensitive.</param>
    /// <param name="item">Object to store in the cache.</param>
    /// <param name="lifetime">Life time of the cache in seconds.</param>
    /// <remarks>
    ///  If an item with the same key exists in the cache, it is overwritten.
    ///  The default lifetime is applied. If the item is <c>null</c> the item will be removed from cache.
    /// </remarks>
    void Set(TKey key, TValue item, int lifetime);

    /// <summary>
    ///  Check if an item is valid in the cache
    /// </summary>
    /// <param name="key">Specifies items inside the cache.</param>
    /// <param name="item">The item to be set</param>
    /// <returns>
    ///  <c>true</c> if the item is there, <c>false</c> if the item is not present or expired
    /// </returns>
    bool TryGet(TKey key, out TValue item);
  }
}