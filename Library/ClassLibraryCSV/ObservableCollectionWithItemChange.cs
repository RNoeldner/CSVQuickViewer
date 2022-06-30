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
using System.Linq;

namespace CsvTools
{
  /// <summary>
  ///   Observavle collection with unique items
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class ObservableCollectionWithItemChange<T> : ObservableCollection<T>
  {
    public ObservableCollectionWithItemChange()
    {
      CollectionChanged +=  RemovePropertyChanged;
    }

    /// <summary>
    ///   Event to be raised on Collection Level if properties of a item in the collection changes
    /// </summary>
    public event PropertyChangedEventHandler? CollectionItemPropertyChanged;

    /// <summary>
    ///   Additional EventHandlers for an implementaion needing information ona a changed item
    /// </summary>
    public PropertyChangedEventHandler? ItemPropertyChanged;

    /// <summary>
    ///   Additional EventHandlers for an implementaion needing information ona a changed item
    /// </summary>
    public EventHandler<PropertyChangedEventArgs<string>>? ItemPropertyChangedString;

    /// <summary>
    ///   By default simply look at the items
    /// </summary>
    /// <param name="search">the item to check if it present</param>
    /// <returns></returns>
    protected virtual bool Present(T search) => Items.Contains(search);

    /// <summary>
    ///   As Items are added or removed Property Chanage is registered When the item is chnaged
    ///   later on CollectionItemPropertyChanged is triggered
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">The event args with old and new items</param>
    private void RemovePropertyChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
        foreach (var oldItem in e.OldItems.OfType<INotifyPropertyChanged>())
        {
          if (CollectionItemPropertyChanged is null)
            oldItem.PropertyChanged -= CollectionItemPropertyChanged;
          if (ItemPropertyChanged!=null)
            oldItem.PropertyChanged -= ItemPropertyChanged;
          if (ItemPropertyChangedString != null && oldItem is INotifyPropertyChangedString changedBase)
            changedBase.PropertyChangedString -= ItemPropertyChangedString;
        }
    }

    public virtual new bool Add(T item)
    {
      if (item is ICloneable src)
        item = (T) src.Clone();
      if (!Present(item))
      {
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
      return false;
    }

    /// <summary>
    ///   A slightly faster method to add a number of items in one go, tthe passed in items will be cloned
    /// </summary>
    /// <param name="items"></param>
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
    ///   Determines whether the other colection is equal to the current collection.
    /// </summary>
    /// <param name="other">the collection</param>
    /// <returns>
    ///   <see langword="true" /> if the collection is equal to the current collection; otherwise,
    ///   <see langword="false" />.
    /// </returns>
    public virtual bool Equals(ICollection<T>? other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;

      return this.CollectionEqualWithOrder(other);
    }

    public override int GetHashCode() => EqualityComparer<IList<T>>.Default.GetHashCode(Items);
  }
}