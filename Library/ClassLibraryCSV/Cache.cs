/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser public virtual License for more details.
 *
 * You should have received a copy of the GNU Lesser public virtual License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Timers;

namespace CsvTools
{
  /// <summary>
  ///  Temporarily stores object that are expensive to create in memory in order to
  ///  improve application performance.
  /// </summary>
  public class Cache<TKey, TValue> : ICache<TKey, TValue>
   where TKey : IComparable
   where TValue : class
  {
    /// <summary>
    ///  Minimum interval (in milliseconds) between two cleanup runs.
    /// </summary>
    private const int c_CleanupInterval = 10000;

    private readonly int m_DefaultLifetime;

    /// <summary>
    ///  Dictionary that stores the cache items
    /// </summary>
    private readonly ConcurrentDictionary<TKey, CacheItem<TValue>> m_Dictionary;

    /// <summary>
    ///  Timer that calls Cleanup periodically.
    /// </summary>
    private Timer m_CleanupTimer;

    /// <summary>
    ///  Initializes a new instance of the <see cref="Cache" /> class.
    /// </summary>
    /// <remarks>
    ///  The default time for an item is set to 5 minutes
    /// </remarks>
    public Cache()
     : this(300)
    {
    }

    /// <summary>
    ///  Initializes a new instance of the <see cref="Cache" /> class.
    /// </summary>
    /// <param name="defaultLifeTime">The default life time for a cache item in seconds.</param>
    public Cache(int defaultLifeTime)
    {
      Contract.Requires(defaultLifeTime > 0);
      m_Dictionary = new ConcurrentDictionary<TKey, CacheItem<TValue>>();
      m_DefaultLifetime = defaultLifeTime;
    }

    /// <summary>
    ///  Event fired for Element that have expired
    /// </summary>
    public virtual event EventHandler<TKey> ElementExpired;

    /// <summary>
    ///  Determines whether the specified key contains key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns><c>true</c> if the key is in the cache, <c>false</c> otherwise</returns>
    public virtual bool ContainsKey(TKey key) => m_Dictionary.ContainsKey(key);

    /// <summary>
    ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    ///  Removes all items from the cache.
    /// </summary>
    public virtual void Flush()
    {
      DisableCleanupTimer();

      foreach (var key in m_Dictionary.Keys)
        RemoveKey(key);
    }

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
    public virtual TValue Get(TKey key)
    {
      if (key == null || !m_Dictionary.TryGetValue(key, out var cacheItem)) return null;

      // If an item was found, check that its age does not exceed the expiration time...
      var expireTime = cacheItem.TimeEntered.AddSeconds(cacheItem.Lifetime);
      var isExpired = expireTime < DateTime.UtcNow;

      // If all criteria for a valid cache item are met, set the return value.
      return isExpired ? null : cacheItem.Item;

      // Set return value;
    }

    public virtual ICollection<TKey> Keys
    {
      get
      {
        CleanupCache(this, null);
        return m_Dictionary.Keys;
      }
    }

    /// <summary>
    ///  Removes an item from the cache.
    /// </summary>
    /// <param name="key">Specifies an item in the cache.</param>
    public virtual void Remove(TKey key)
    {
      if (RemoveKey(key))
        // Check if we need to start or stop cleanups.
        EnableCleanupTimer();
    }

    /// <summary>
    ///  Adds an item to the cache with a default lifetime of 5 minutes (300 seconds)
    /// </summary>
    /// <param name="key">Identifies an item in the cache. The key is not case-sensitive.</param>
    /// <param name="item">Object to store in the cache.</param>
    /// <remarks>
    ///  If an item with the same key exists in the cache, it is overwritten.
    ///  The default lifetime is applied. If the item is <c>null</c> the item will be removed from cache.
    /// </remarks>
    public virtual void Set(TKey key, TValue item) => Set(key, item, m_DefaultLifetime);

    /// <summary>
    ///  Adds an item to the cache.
    /// </summary>
    /// <param name="key">Identifies an item in the cache. The key is not case-sensitive.</param>
    /// <param name="item">Object to store in the cache.</param>
    /// <param name="lifetime">Life time of the item in seconds.</param>
    /// <remarks>
    ///  If an item with the same key exists in the cache, it is overwritten.
    ///  The default lifetime is applied.
    /// </remarks>
    public virtual void Set(TKey key, TValue item, int lifetime)
    {
      if (lifetime < 1)
        lifetime = 300;
      if (key == null) return;
      // Create a CacheItem to wrap the item.
      var cacheItem = new CacheItem<TValue>(item, lifetime);

      // Add the CacheItem to the Dictionary.
      // In order to prevent threading-related synchronization issues,
      // lock the hash table.

      if (!m_Dictionary.ContainsKey(key))
        m_Dictionary.TryAdd(key, cacheItem);
      else
        m_Dictionary[key] = cacheItem;

      // Check if we need to start or stop cleanups.
      EnableCleanupTimer();
    }

    /// <summary>
    ///  Check if an item is valid in the cache
    /// </summary>
    /// <param name="key">Specifies items inside the cache.</param>
    /// <param name="item">The item to be set</param>
    /// <returns>
    ///  <c>true</c> if the item is there, <c>false</c> if the item is not present or expired
    /// </returns>
    public virtual bool TryGet(TKey key, out TValue item)
    {
      item = default(TValue);
      if (key == null) return false;
      if (!m_Dictionary.TryGetValue(key, out var cacheItem)) return false;
      // If an item was found, check that its age does not exceed the expiration time...
      var expireTime = cacheItem.TimeEntered.AddSeconds(cacheItem.Lifetime);
      var isExpired = expireTime < DateTime.UtcNow;

      // If all criteria for a valid cache item are met, set the return value.
      if (isExpired) return false;
      // ... and if this is the case, set the return value.
      item = cacheItem.Item;
      return true;
    }

    /// <summary>
    ///  Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing">
    ///  <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
    ///  unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
        DisableCleanupTimer();
    }

    /// <summary>
    ///  Removes all items with an expired lifetime from the cache.
    /// </summary>
    private void CleanupCache(object sender, ElapsedEventArgs args)
    {
      if (m_Dictionary == null || m_Dictionary.Count == 0)
        return;
      var expiredItemKeys = new List<TKey>();

      foreach (var key in m_Dictionary.Keys)
      {
        if (key == null)
          continue;
        var cacheItem = m_Dictionary[key];
        // ... check if its lifetime has expired ...
        var expireTime = cacheItem.TimeEntered.AddSeconds(cacheItem.Lifetime);
        if (expireTime < DateTime.UtcNow) expiredItemKeys.Add(key);
      }

      // Disable the Cleanup timer early in case we are going to remove all...
      if (m_Dictionary.Count - expiredItemKeys.Count < 1)
        DisableCleanupTimer();

      // Now go and remove the expired items... since an event it fired this might take a while
      foreach (var key in expiredItemKeys)
        RemoveKey(key);
    }

    /// <summary>
    ///  Disables the cleanup timer.
    /// </summary>
    private void DisableCleanupTimer()
    {
      if (m_CleanupTimer == null) return;
      m_CleanupTimer.Stop();
      m_CleanupTimer.Dispose();
      m_CleanupTimer = null;
    }

    /// <summary>
    ///  Enables or disables the cleanup timer, depending on whether the
    /// </summary>
    private void EnableCleanupTimer()
    {
      // If the dictionary is empty, completely remove the cleanup timer -

      if (m_Dictionary.Count == 0)
      {
        DisableCleanupTimer();
        return;
      }

      if (m_CleanupTimer != null) return;
      m_CleanupTimer = new Timer(c_CleanupInterval);
      m_CleanupTimer.Elapsed += CleanupCache;
      m_CleanupTimer.Start();
    }

    private bool RemoveKey(TKey key)
    {
      if (key == null) return false;
      ElementExpired?.Invoke(this, key);

      m_Dictionary.TryRemove(key, out var item);
      item?.FreeItem();
      return true;
    }

    /// <summary>
    ///  Wraps an item in the cache Dictionary,
    ///  storing an object, the time it has been entered
    ///  to the cache, and its lifetime in seconds.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private class CacheItem<T>
    {
      /// <summary>
      ///  Initializes a new instance of the CacheItem class.
      /// </summary>
      /// <param name="item">The object added to the cache.</param>
      /// <param name="lifetime">The lifetime (in seconds). After this time, the item is removed.</param>
      internal CacheItem(T item, int lifetime)
      {
        Item = item;
        Lifetime = lifetime;
        TimeEntered = DateTime.UtcNow;
      }

      /// <summary>
      ///  The item stored in the cache.
      /// </summary>
      internal T Item { get; private set; }

      /// <summary>
      ///  Time (in seconds) the object is to remain in the cache.
      /// </summary>
      internal int Lifetime { get; }

      /// <summary>
      ///  Time the object has been entered into the cache.
      /// </summary>
      internal DateTime TimeEntered { get; }

      /// <summary>
      ///  Frees the item.
      /// </summary>
      internal void FreeItem()
      {
        if (Item is IDisposable disposableItem)
          disposableItem.Dispose();

        Item = default(T);
      }
    }
  }
}