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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace CsvTools;

/// <summary>
/// An observable collection that ensures all items are unique by <see cref="ICollectionIdentity.CollectionIdentifier"/>.
/// Supports automatic wiring of <see cref="INotifyPropertyChanged"/> events for items in the collection.
/// </summary>
/// <typeparam name="T">Type of items in the collection, must implement <see cref="ICollectionIdentity"/>.</typeparam>
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
public class UniqueObservableCollection<T> : ObservableCollection<T> where T : ICollectionIdentity, INotifyPropertyChanged
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
{

  /// <summary>
  /// Event raised when a property of any item in the collection changes (assuming the element .
  /// </summary>
  public event PropertyChangedEventHandler? CollectionItemPropertyChanged;

  /// <summary>
  /// Rewires all items in the collection to the <see cref="CollectionItemPropertyChanged"/> event.
  /// Useful after deserialization or bulk modifications.
  /// </summary>
  public virtual void WireEvents()
  {
    if (CollectionItemPropertyChanged == null)
      return;
    foreach (var item in Items)
    {
      UnwireItemEvents(item);
      WireItemEvents(item);
    }
  }

  /// <summary>
  /// Wires property changed events for an item.
  /// </summary>
  private void WireItemEvents(T item)
  {
    if (CollectionItemPropertyChanged != null)
      item.PropertyChanged += CollectionItemPropertyChanged;
  }

  /// <summary>
  /// Unwires property changed events for an item.
  /// </summary>
  private void UnwireItemEvents(T item)
  {
    if (CollectionItemPropertyChanged != null)
      item.PropertyChanged -= CollectionItemPropertyChanged;
  }

  /// <summary>
  ///   Adds the specified item to the collection and makes sure the item is not already present,
  ///   if the item does support <see cref="INotifyPropertyChanged" /><see
  ///   cref="CollectionItemPropertyChanged" /> will be registered to pass the event to the
  ///   implementing class
  /// </summary>
  /// <param name="item">The item to add</param>
  /// <remarks>
  ///   In case the item is clone able <see cref="ICloneable" /> a value copy will be made. In
  ///   this case any change to the passed in item would not be reflected in the collection
  /// </remarks>    
  public new virtual void Add(T item)
  {
    if (IndexOf(item)!=-1)
      return;
    WireItemEvents(item);
    base.Add(item);
  }

  /// <summary>
  /// Adds an item and ensures the key property is unique.
  /// </summary>
  public void AddMakeUnique(T item)
  {
    if (IndexOf(item) != -1)
      return;

    var existingKeys = Items.Select(x => x.GetUniqueKey()).Where(k => k != null).ToList();
    var uniqueKey = existingKeys.MakeUniqueInCollection(item.GetUniqueKey());

    item.SetUniqueKey(uniqueKey);
    Add(item);
  }

  /// <inheritdoc cref="ObservableCollection{T}" />
  public new void Insert(int index, T item)
  {
    // ReSharper disable once SuspiciousTypeConversion.Global
    if (item is ICloneable src)
      item = (T) src.Clone();
    if (IndexOf(item)!=-1)
      return;
    // ReSharper disable once SuspiciousTypeConversion.Global
    if (item is INotifyPropertyChanged notifyPropertyChanged)
    {
      if (CollectionItemPropertyChanged != null)
        notifyPropertyChanged.PropertyChanged += CollectionItemPropertyChanged;

    }
    base.Insert(index, item);
  }

  /// <inheritdoc cref="ICollection{T}" />
  public new void Remove(T item)
  {
    var index = IndexOf(item);
    if (index==-1)
      return;
    RemoveAt(index);
  }

  /// <inheritdoc cref="ICollection{T}"/>
  public new virtual void RemoveAt(int index)
  {
    var item = Items[index];
    UnwireItemEvents(item);
    base.RemoveAt(index);
  }

  /// <summary>
  ///   A slightly faster method to add a number of items in one go, if the item is clone able a copy is made
  /// </summary>
  /// <param name="items">Some items to add</param>
  public void AddRangeClone(IEnumerable<T> items)
  {
    using var enumerator = items.GetEnumerator();
    while (enumerator.MoveNext())
      if (enumerator.Current != null)
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (enumerator.Current is ICloneable item)
          Add((T) item.Clone());
        else
          Add(enumerator.Current);
  }

  /// <summary>
  ///   A slightly faster method to add a number of items in one go, the collection gets a reference to the passed in values
  /// </summary>
  /// <param name="items">Some items to add</param>
  public void AddRange(IEnumerable<T> items)
  {
    using var enumerator = items.GetEnumerator();
    while (enumerator.MoveNext())
      if (enumerator.Current != null)
        Add(enumerator.Current);
  }

  /// <summary>
  ///   Determines whether the specified object is equal to the current object.
  /// </summary>
  /// <param name="other">The object to compare with the current object.</param>
  /// <returns>
  ///   <see langword="true" /> if the specified object is equal to the current object; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  public override bool Equals(object? other)
  {
    if (!(other is ICollection<T> coll))
      return false;
    return Equals(coll);
  }

  /// <summary>
  ///   Determines whether the other collection is equal to the current collection.
  /// </summary>
  /// <param name="other">the collection</param>
  /// <returns>
  ///   <see langword="true" /> if the collection is equal to the current collection; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  public bool Equals(IEnumerable<T> other)
  {
    return this.CollectionEqualWithOrder(other);
  }

  /// <inheritdoc cref="IList{T}" />
  public new int IndexOf(T search)
    => GetIndexByIdentifier(search.CollectionIdentifier);

  /// <summary>
  ///   Gets the index of a collection item by the CollectionIdentifier
  /// </summary>
  /// <param name="searchId">The identifier in the collection.</param>
  /// <returns>-1 if not found, the index otherwise</returns>
  protected int GetIndexByIdentifier(int searchId)
  {
    for (var index = 0; index < Items.Count; index++)
      if (Items[index].CollectionIdentifier == searchId)
        return index;
    return -1;
  }
}