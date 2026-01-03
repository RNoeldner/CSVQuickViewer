using System;
using System.Collections.Generic;

namespace CsvTools
{
  /// <summary>
  ///   Represents a list of objects of type <typeparamref name="T"/> that notifies
  ///   subscribers whenever its contents change.
  ///   Inherits from <see cref="List{T}"/> and extends it with change notifications.
  /// </summary>
  /// <typeparam name="T">The type of elements in the collection.</typeparam>
  /// <remarks>Unlike ObservableCollection, this class provides range methods that raise CollectionChanged only after actual changes.</remarks>
  public class ObservableList<T> : List<T>
  {
    /// <summary>
    ///   Occurs whenever the collection content changes, such as when an item is added, inserted, removed, or cleared.
    /// </summary>
    [field: NonSerialized]
    public event EventHandler? CollectionChanged;

    /// <summary>
    ///   Adds an item to the collection and raises <see cref="CollectionChanged"/>.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public new virtual void Add(T item)
    {
      base.Add(item);
      OnCollectionChanged();
    }

    /// <summary>
    ///   Adds multiple items to the collection and raises <see cref="CollectionChanged"/>.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public new virtual void AddRange(IEnumerable<T> items)
    {
      base.AddRange(items);
      OnCollectionChanged();
    }

    /// <summary>
    ///   Replaces all items in the collection with the specified sequence
    ///   and raises <see cref="CollectionChanged"/>.
    /// </summary>
    /// <param name="items">The new items to replace the current collection. Must not be <c>null</c>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="items"/> is <c>null</c>.</exception>
    public virtual void Overwrite(IEnumerable<T> items)
    {
      base.Clear();
      base.AddRange(items);
      OnCollectionChanged();
    }

    /// <summary>
    ///   Removes all items from the collection and raises <see cref="CollectionChanged"/> if the collection was not empty.
    /// </summary>
    public new void Clear()
    {
      if (Count == 0)
        return;

      base.Clear();
      OnCollectionChanged();
    }

    /// <summary>
    ///   Inserts an item at the specified index and raises <see cref="CollectionChanged"/>.
    /// </summary>
    /// <param name="index">The zero-based index at which to insert the item.</param>
    /// <param name="item">The item to insert.</param>
    public new virtual void Insert(int index, T item)
    {
      base.Insert(index, item);
      OnCollectionChanged();
    }

    /// <summary>
    ///   Inserts multiple items at the specified index and raises <see cref="CollectionChanged"/>.
    /// </summary>
    /// <param name="index">The zero-based index at which to insert the items.</param>
    /// <param name="items">The items to insert.</param>
    public new virtual void InsertRange(int index, IEnumerable<T> items)
    {
      base.InsertRange(index, items);
      OnCollectionChanged();
    }

    /// <summary>
    ///   Removes the specified item from the collection and raises <see cref="CollectionChanged"/> if the item was successfully removed.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns><c>true</c> if the item was removed; otherwise, <c>false</c>.</returns>
    public virtual new bool Remove(T item)
    {
      var removed = base.Remove(item);
      if (removed)
        OnCollectionChanged();
      return removed;
    }

    /// <summary>
    ///   Removes the item at the specified index and raises <see cref="CollectionChanged"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    public new virtual void RemoveAt(int index)
    {
      base.RemoveAt(index);
      OnCollectionChanged();
    }

    /// <summary>
    ///   Removes the specified items from the collection. 
    ///   Raises <see cref="CollectionChanged"/> if at least one item was successfully removed.
    /// </summary>
    /// <param name="items">The items to remove. Must not be <c>null</c>.</param>
    /// <returns>
    ///   <c>true</c> if at least one item was removed; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="items"/> is <c>null</c>.</exception>
    public bool RemoveRange(IEnumerable<T> items)
    {
      bool removed = false;
      foreach (var item in items)
        removed |= base.Remove(item);

      if (removed)
        OnCollectionChanged();
      return removed;
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

    /// <summary>
    ///   Raises the <see cref="CollectionChanged"/> event to notify subscribers that the collection has changed.
    /// </summary>
    protected void OnCollectionChanged() => CollectionChanged?.Invoke(this, EventArgs.Empty);

    /// <inheritdoc/>
    public override int GetHashCode() => this.CollectionHashCode();
  }
}
