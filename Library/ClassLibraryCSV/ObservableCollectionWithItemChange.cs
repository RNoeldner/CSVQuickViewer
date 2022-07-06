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
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CsvTools
{
  /// <summary>
  ///   Observable collection with unique items
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class ObservableCollectionWithItemChange<T> : ObservableCollection<T> where T : ICollectionIdentity
  {
    /// <summary>
    ///   Additional EventHandlers for an implementation needing information ona a changed item
    /// </summary>
    public PropertyChangedEventHandler? ItemPropertyChanged;

    /// <summary>
    ///   Additional EventHandlers for an implementation needing information ona a changed item
    /// </summary>
    public EventHandler<PropertyChangedEventArgs<string>>? ItemPropertyChangedString;

    /// <summary>
    ///   Event to be raised on Collection Level if properties of a item in the collection changes
    /// </summary>
    public event PropertyChangedEventHandler? CollectionItemPropertyChanged;

    /// <summary>
    ///   Adds the specified item to the collection and makes sure the item is not already present,
    ///   if the item does support <see cref="INotifyPropertyChanged" /><see
    ///   cref="CollectionItemPropertyChanged" />, <see cref="ItemPropertyChanged" /> or <see
    ///   cref="ItemPropertyChangedString" /> will be registered to pass the event to the
    ///   implementing class
    /// </summary>
    /// <param name="item">
    ///   The item to add, the calling routine should make sure the item does have eth properties
    ///   set to determine presence
    /// </param>
    /// <remarks>
    ///   In case the the item is cloneable <see cref="ICloneable" /> a value copy will be made. In
    ///   this case any change to the passed in item would not be reflected in the collection
    /// </remarks>
    /// <returns>
    ///   <see langword="true" /> if it was added, otherwise the item was not added to the collection
    /// </returns>
    public new bool Add(T item)
    {
      if (item is ICloneable src)
        item = (T) src.Clone();
      if (IndexOf(item)!=-1)
        return false;

      // Set Property changed Event Handlers if possible
      if (item is INotifyPropertyChanged notifyPropertyChanged)
      {
        if (CollectionItemPropertyChanged != null)
          notifyPropertyChanged.PropertyChanged += CollectionItemPropertyChanged;
        if (ItemPropertyChanged!=null)
          notifyPropertyChanged.PropertyChanged += ItemPropertyChanged;
      }
      if (ItemPropertyChangedString != null && item is INotifyPropertyChangedString notifyPropertyChangedString)
        notifyPropertyChangedString.PropertyChangedString += ItemPropertyChangedString;
      base.Add(item);
      return true;
    }

    /// <inheritdoc />
    public new void Insert(int index, T item)
    {
      if (item is ICloneable src)
        item = (T) src.Clone();
      if (IndexOf(item)!=-1)
        return;
      if (item is INotifyPropertyChanged notifyPropertyChanged)
      {
        if (CollectionItemPropertyChanged != null)
          notifyPropertyChanged.PropertyChanged += CollectionItemPropertyChanged;
        if (ItemPropertyChanged!=null)
          notifyPropertyChanged.PropertyChanged += ItemPropertyChanged;
      }
      if (ItemPropertyChangedString != null && item is INotifyPropertyChangedString notifyPropertyChangedString)
        notifyPropertyChangedString.PropertyChangedString += ItemPropertyChangedString;
      base.Insert(index, item);
    }

    /// <inheritdoc />
    public new void Remove(T item)
    {
      var index = IndexOf(item);
      if (index==-1)
        return;
      RemoveAt(index);
    }

    /// <inheritdoc />
    public new void RemoveAt(int index)
    {
      var item = Items[index];
      base.RemoveAt(index);
      if (item is INotifyPropertyChanged notifyPropertyChanged)
      {
        if (CollectionItemPropertyChanged != null)

          notifyPropertyChanged.PropertyChanged -= CollectionItemPropertyChanged;
        if (ItemPropertyChanged!=null)
          notifyPropertyChanged.PropertyChanged -= ItemPropertyChanged;
      }

      if (ItemPropertyChangedString != null && item is INotifyPropertyChangedString notifyPropertyChangedString)
        notifyPropertyChangedString.PropertyChangedString -= ItemPropertyChangedString;
    }

    /// <summary>
    ///   A slightly faster method to add a number of items in one go
    /// </summary>
    /// <param name="items">Some items to add</param>
    public virtual void AddRange(IEnumerable<T> items)
    {
      using var enumerator = items.GetEnumerator();
      while (enumerator.MoveNext())
        if (enumerator.Current != null)
          Add(enumerator.Current);
    }

    /// <summary>
    ///   Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>
    ///   <see langword="true" /> if the specified object is equal to the current object; otherwise,
    ///   <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj) => Equals(obj as ICollection<T>);

    /// <summary>
    ///   Determines whether the other collection is equal to the current collection.
    /// </summary>
    /// <param name="other">the collection</param>
    /// <returns>
    ///   <see langword="true" /> if the collection is equal to the current collection; otherwise,
    ///   <see langword="false" />.
    /// </returns>
    public bool Equals(ICollection<T>? other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;

      return this.CollectionEqualWithOrder(other);
    }

    /// <summary>
    ///   Returns a hash code for this instance. Build from all items in the collection
    /// </summary>
    /// <returns>
    ///   A hash code for this instance, suitable for use in hashing algorithms and data structures
    ///   like a hash table.
    /// </returns>
    public override int GetHashCode() => EqualityComparer<IList<T>>.Default.GetHashCode(Items);

    /// <inheritdoc cref="IList{T}" />
    public new int IndexOf(T search)
    => GetIndexByIdentifier(search.CollectionIdentifier);

    /// <summary>
    ///   Gets the index of a collection item by the CollectionIdentifier
    /// </summary>
    /// <param name="searchID">The identifier in teh collection.</param>
    /// <returns>-1 if not found, the index otherwise</returns>
    protected int GetIndexByIdentifier(int searchID)
    {
      for (var index = 0; index < Items.Count; index++)
        if (Items[index].CollectionIdentifier == searchID)
          return index;
      return -1;
    }
  }
}