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
using System.Collections.Specialized;
using System.ComponentModel;

namespace CsvTools
{
  /// <summary>
  ///   Observable collection with unique items
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class ObservableCollectionWithItemChange<T> : ObservableCollection<T>
  {
    /// <summary>
    ///   Event to be raised on Collection Level if properties of a item in the collection changes
    /// </summary>
    public event PropertyChangedEventHandler? CollectionItemPropertyChanged;

    /// <summary>
    ///   Additional EventHandlers for an implementation needing information ona a changed item
    /// </summary>
    public PropertyChangedEventHandler? ItemPropertyChanged;

    /// <summary>
    ///   Additional EventHandlers for an implementation needing information ona a changed item
    /// </summary>
    public EventHandler<PropertyChangedEventArgs<string>>? ItemPropertyChangedString;

    
    public ObservableCollectionWithItemChange(PropertyChangedEventHandler? itemPropertyChanged = null, EventHandler<PropertyChangedEventArgs<string>>? itemPropertyChangedString = null)
    {
      ItemPropertyChangedString = itemPropertyChangedString;
      ItemPropertyChanged = itemPropertyChanged;
      CollectionChanged +=  RemovePropertyChanged;
    }

    /// <summary>
    ///   Function to determine if an item is present in the collection
    /// </summary>
    /// <param name="search">the item to check if it present</param>
    /// <returns>
    ///   <see langword="true" /> if the collection does contain the item already; otherwise,
    ///   <see langword="false" />.
    /// </returns>
    protected  virtual bool Present(T search) => Items.Contains(search);

    /// <summary>
    ///   As Items are added or removed Property Change is registered When the item is changed
    ///   later on CollectionItemPropertyChanged is triggered
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">The event args with old and new items</param>
    private void RemovePropertyChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems == null) 
        return;
      foreach (var item in e.OldItems)
      {
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
    }

    /// <summary>
    /// Adds the specified item to the collection and makes sure the item is not already present, if the item does support <see cref="INotifyPropertyChanged"/> <see cref="CollectionItemPropertyChanged"/>, <see cref="ItemPropertyChanged"/> and <see cref="ItemPropertyChangedString"/> will be registered to pass the event to the implementing class
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <remarks>In case the the item is cloneable <see cref="ICloneable"/> a value copy will be made. In this case any change to teh passed in item would not be reflected in the collection</remarks>
    /// <returns><see langword="true" /> if it was added, otherwise the item was not added to the collection</returns>
    public new bool Add(T item)
    {
      if (item is ICloneable src)
        item = (T) src.Clone();
      if (Present(item)) 
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

    /// <summary>
    ///   A slightly faster method to add a number of items in one go
    /// </summary>
    /// <param name="items">Some items to add</param>
    public virtual void AddRange(IEnumerable<T> items)
    {
      // Do set PropertyChanged one by one but do this in one go
      CollectionChanged -= RemovePropertyChanged;
      try
      {
        using var enumerator = items.GetEnumerator();
        while (enumerator.MoveNext())
          if (enumerator.Current != null)
            Add(enumerator.Current);
      }
      finally
      {
        // From now on default behaviour
        CollectionChanged += RemovePropertyChanged;
      }
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
    /// Returns a hash code for this instance. Build from all items in the collection
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode() => EqualityComparer<IList<T>>.Default.GetHashCode(Items);
  }
}