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
  public class ObservableCollectionWithItemChange<T> : ObservableCollection<T> where T : ICloneable
  {
    /// <summary>
    ///   Event to be raised on Collection Level if properties of a item in the collection changes
    /// </summary>
    public event PropertyChangedEventHandler? CollectionItemPropertyChanged;


    private void ObservableCollectionWithItemChange_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (CollectionItemPropertyChanged is null)
        return;

      if (e.OldItems != null)
        foreach (var oldItem in e.OldItems.OfType<INotifyPropertyChanged>())
          oldItem.PropertyChanged -= CollectionItemPropertyChanged;

      if (e.NewItems != null)
        foreach (var newItem in e.NewItems.OfType<INotifyPropertyChanged>())
          newItem.PropertyChanged += CollectionItemPropertyChanged;
    }

    public void AddRangeClone(IEnumerable<T>? items)
    {
      if (items is null)
        return;

      // Do set PropertyChanged one by one but do this in one go
      CollectionChanged -= ObservableCollectionWithItemChange_CollectionChanged;
      try
      {
        using var enumerator = items.GetEnumerator();
        while (enumerator.MoveNext())
          if (enumerator.Current != null)
          {
            var item = (T) enumerator.Current.Clone();
            if (item is INotifyPropertyChanged notifyPropertyChanged)
              notifyPropertyChanged.PropertyChanged += CollectionItemPropertyChanged;
            Add(item);
          }
      }
      finally
      {
        // From now on default behaviour
        CollectionChanged += ObservableCollectionWithItemChange_CollectionChanged;
      }

    }

    /// <summary>
    ///   Copies all properties to the other instance
    /// </summary>
    /// <param name="other">The other instance</param>
    public void CopyTo(ICollection<T> other)
    {
      other.Clear();
      foreach (var item in Items)
        other.Add((T) item.Clone());
    }
  }
}