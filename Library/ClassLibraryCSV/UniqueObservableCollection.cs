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
#nullable enable 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace CsvTools
{
  /// <summary>
  ///   Observable collection with unique items
  /// </summary>
  /// <typeparam name="T"></typeparam>
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  public class UniqueObservableCollection<T> : ObservableCollection<T> where T : ICollectionIdentity
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  {    
    /// <summary>
    ///   Event to be raised on Collection Level if properties of an item in the collection changes
    /// </summary>
    public event PropertyChangedEventHandler? CollectionItemPropertyChanged;

    /// <summary>
    ///   Adds the specified item to the collection and makes sure the item is not already present,
    ///   if the item does support <see cref="INotifyPropertyChanged" /><see
    ///   cref="CollectionItemPropertyChanged" /> will be registered to pass the event to the
    ///   implementing class
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <remarks>
    ///   In case the item is cloneable <see cref="ICloneable" /> a value copy will be made. In
    ///   this case any change to the passed in item would not be reflected in the collection
    /// </remarks>
    /// <returns>
    ///   <see langword="true" /> if it was added, otherwise the item was not added to the collection
    /// </returns>
    public new virtual void Add(T item)
    {
      if (IndexOf(item)!=-1)
        return;

      // Set Property changed Event Handlers if possible
      // ReSharper disable once SuspiciousTypeConversion.Global
      if (item is INotifyPropertyChanged notifyPropertyChanged)
      {
        if (CollectionItemPropertyChanged != null)
          notifyPropertyChanged.PropertyChanged += CollectionItemPropertyChanged;        
      }
      
      base.Add(item);
    }
  
    /// <summary>
    ///   Adds the specified item to the collection and makes sure the item is not already present
    ///   by changing the name in a way it is unique, if the item does support <see
    ///   cref="INotifyPropertyChanged" /> will be
    ///   registered to pass the event to the implementing class
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <param name="propertyName">
    ///   Name of the property that needs to be adjusted to make the item unique
    /// </param>
    /// <remarks>
    ///   In case the item is cloneable <see cref="ICloneable" /> a value copy will be made. In
    ///   this case any change to the passed in item would not be reflected in the collection
    /// </remarks>
    /// <returns>
    ///   <see langword="true" /> if it was added, otherwise the item was not added to the collection
    /// </returns>
    public void AddMakeUnique(T item, string propertyName)
    {
      if (IndexOf(item)!=-1)
      {
        var property = typeof(T).GetProperty(propertyName);
        if (property is null)
          throw new ArgumentException($"The property {propertyName} not found");
        if (property.PropertyType != typeof(string))
          throw new ArgumentException($"The property {propertyName} must be a string value");

        // now make the name unique
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
#pragma warning disable CS8604 // Possible null reference argument.
        property.SetValue(item, Items.Select(prev => property.GetValue(prev) as string).ToList().MakeUniqueInCollection(property.GetValue(item) as string));
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
      }
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
    public new void RemoveAt(int index)
    {
      var item = Items[index];
       // ReSharper disable once SuspiciousTypeConversion.Global
      if (item is INotifyPropertyChanged notifyPropertyChanged)
      {
        if (CollectionItemPropertyChanged != null)
          notifyPropertyChanged.PropertyChanged -= CollectionItemPropertyChanged;        
      }
      base.RemoveAt(index);
    }

    /// <summary>
    ///   A slightly faster method to add a number of items in one go, if the item is cloneable a copy is made
    /// </summary>
    /// <param name="items">Some items to add</param>
    public void AddRange(IEnumerable<T> items)
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
    public void AddRangeNoClone(IEnumerable<T> items)
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
#pragma warning disable CS0659
    public override bool Equals(object? obj) => Equals(obj as ICollection<T>);
#pragma warning restore CS0659

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
      return other is { } && this.CollectionEqualWithOrder(other);
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
}