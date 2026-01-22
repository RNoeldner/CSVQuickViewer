#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace CsvTools;

/// <summary>
/// An <see cref="ObservableCollection{T}"/> that enforces uniqueness of its items
/// based on a case-insensitive string key provided by <see cref="ICollectionIdentity"/>.
/// 
/// Internally, a dictionary is maintained for O(1) lookups by key, while the
/// observable collection preserves insertion order and serializes as a JSON array.
/// 
/// This collection is not thread-safe. All mutations must occur on the owning thread.
/// </summary>
/// <typeparam name="T">
/// The item type. Must expose a unique string key and notify about property changes.
/// </typeparam>
public class UniqueObservableCollection<T> : ObservableCollection<T>
  where T : class, ICollectionIdentity, INotifyPropertyChanged, IEquatable<T>
{
  private bool m_SuppressOnCollectionChanged = false;

  /// <summary>
  /// Internal lookup table mapping unique keys to items.
  /// Uses case-insensitive comparison to enforce key uniqueness.
  /// This dictionary is an implementation detail and must stay in sync with the list.
  /// </summary>
  private readonly Dictionary<string, T> m_InternalDictionary =
    new(StringComparer.OrdinalIgnoreCase);

  /// <inheritdoc/>
  protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
  {
    if (m_SuppressOnCollectionChanged)
      return;
    base.OnCollectionChanged(e);
  }

  /// <summary>
  /// Raised whenever any property of any item in the collection changes.
  /// This allows consumers to listen at the collection level instead of
  /// subscribing to each individual item.
  /// </summary>
  public event PropertyChangedEventHandler? CollectionItemPropertyChanged;

  /// <summary>
  /// Adds an item to the collection, automatically modifying its key
  /// if necessary so that it becomes unique within the collection.
  /// </summary>
  public void AddMakeUnique(T item)
  {
    MakeUnique(item);
    Add(item);
  }

  /// <summary>
  /// Adds multiple items to the collection, ensuring uniqueness of all keys.
  /// Raises a single CollectionChanged event.
  /// </summary>
  public void AddRange(IEnumerable<T> items)
  {
    if (items == null) return;
    bool addedAny = false;
    try
    {
      m_SuppressOnCollectionChanged = true;
      foreach (var item in items)
      {
        AddMakeUnique(item);
        addedAny = true;
      }
    }
    finally
    {
      m_SuppressOnCollectionChanged = false;
      if (addedAny)
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items));
    }

  }

  /// <summary> Rewires all items in the collection to the <see cref="CollectionItemPropertyChanged"/> event. 
  /// Useful after deserialization or bulk modifications. 
  /// </summary>
  public virtual void WireEvents()
  {
    foreach (var item in Items)
    {
      Unregister(item);
      Register(item);
    }
  }


  /// <summary>
  /// Clears the collection and fully resets internal bookkeeping,
  /// including dictionary entries and event subscriptions.
  /// </summary>
  protected override void ClearItems()
  {
    foreach (var item in Items)
      Unregister(item);
    m_InternalDictionary.Clear();
    base.ClearItems();
  }

  /// <summary>
  /// Determines whether an item with the specified unique key exists in the collection.
  /// Comparison is case-insensitive.
  /// </summary>
  public bool ContainsKey(string key)
    => m_InternalDictionary.ContainsKey(key);

  /// <summary>
  /// Retrieves an item by its unique key, or <c>null</c> if no such item exists.
  /// </summary>
  public T? GetByKey(string key)
    => m_InternalDictionary.TryGetValue(key, out var value) ? value : null;

  /// <summary>
  /// Returns the zero-based index of the item with the specified unique key,
  /// or -1 if no such item exists.
  /// Comparison is case-insensitive.
  /// </summary>
  public int IndexOf(string key) => m_InternalDictionary.TryGetValue(key, out var item) ? Items.IndexOf(item) : -1;

  /// <summary>
  /// Inserts an item at the specified index after verifying that its unique key
  /// does not collide with an existing item. This is called when an item is added by de-serialisation
  /// Also wires property change notifications for the item. This is called by the underlying ObservableCollection 
  /// </summary>
  protected override void InsertItem(int index, T item)
  {
    m_InternalDictionary[MakeUnique(item)] = item;
    base.InsertItem(index, item);
    Register(item);
  }

  /// <summary>
  /// Registers an item with the collection.
  /// Derived classes may override to wire additional events,
  /// but must call the base implementation.
  /// </summary>
  protected virtual void Register(T item)
    => item.PropertyChanged += OnItemPropertyChanged;

  /// <summary>
  /// Removes the item at the specified index and unwires all associated event handlers.
  /// </summary>
  protected override void RemoveItem(int index)
  {
    var item = Items[index];
    Unregister(item);
    m_InternalDictionary.Remove(item.GetUniqueKey());
    base.RemoveItem(index);
  }

  /// <summary>
  /// Replaces an item at the given index, ensuring that the new item
  /// satisfies the uniqueness constraint and is correctly registered.
  /// </summary>
  protected override void SetItem(int index, T item)
  {
    var old = Items[index];
    Unregister(old);
    m_InternalDictionary.Remove(old.GetUniqueKey());

    var key = EnsureUniqueKey(item);
    base.SetItem(index, item);
    m_InternalDictionary[key] = item;
    Register(item);
  }

  /// <summary>
  /// Unregisters an item from the collection.
  /// Derived classes may override to unwire additional events,
  /// but must call the base implementation.
  /// </summary>
  protected virtual void Unregister(T item)
      => item.PropertyChanged -= OnItemPropertyChanged;

  /// <summary>
  /// Ensures that the item's unique key is valid (non-null, non-empty)
  /// and does not already exist in the collection.
  /// </summary>
  /// <exception cref="InvalidOperationException">
  /// Thrown if the key is invalid or already present.
  /// </exception>
  private string EnsureUniqueKey(T item)
  {
    var key = item.GetUniqueKey();

    if (string.IsNullOrWhiteSpace(key))
      throw new InvalidOperationException($"{item.UniqueKeyPropertyName} must not be null or empty.");

    if (m_InternalDictionary.ContainsKey(key))
      throw new InvalidOperationException(
        $"Duplicate {item.UniqueKeyPropertyName} '{key}' (case-insensitive).");
    return key;
  }

  /// <summary>
  /// Modifies the item's key so that it becomes unique within the collection,
  /// based on the keys currently present.
  ///
  /// IMPORTANT:
  /// This method is ONLY used during insertion (JSON load, Add, AddRange).
  /// Key changes on existing items must NEVER be auto-adjusted.
  /// </summary>
  private string MakeUnique(T item)
  {
    var existingKeys = m_InternalDictionary.Keys.ToList();
    var key = item.GetUniqueKey();
    var unique = existingKeys.MakeUniqueInCollection(key);
    // Key was unique already
    if (unique == key)
      return key;

    // Item added twice
    if (m_InternalDictionary.Any(p => ReferenceEquals(p.Value, item)))
      throw new InvalidOperationException("The same item instance cannot be added to the collection more than once.");

    item.SetUniqueKey(unique);
    return unique;
  }

  /// <summary>
  /// Handles property change notifications from items in the collection.
  ///
  /// If the property that defines the unique key changes, the change is
  /// validated and either accepted or rejected.
  /// </summary>
  private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (sender is not T item)
      return;

    // React to key changes
    if (item.UniqueKeyPropertyName.Equals(e.PropertyName))
    { 
      HandleKeyChange(item);
      return;
    }
      

    CollectionItemPropertyChanged?.Invoke(sender, e);
  }

  /// <summary>
  /// Handles changes to an item's unique key.
  ///
  /// Duplicate keys are rejected and the previous value is restored.
  /// Automatic key adjustment is explicitly NOT performed here.
  /// </summary>
  private void HandleKeyChange(T item)
  {
    // Find old key
    var oldEntry = m_InternalDictionary.FirstOrDefault(p => ReferenceEquals(p.Value, item));
    var oldKey = oldEntry.Key;
    var newKey = item.GetUniqueKey();

    if (oldKey == null || oldKey.Equals(newKey, StringComparison.OrdinalIgnoreCase))
      return;

    // Reject duplicates
    if (m_InternalDictionary.ContainsKey(newKey))
    {
      // revert change
      item.PropertyChanged -= OnItemPropertyChanged;
      item.SetUniqueKey(oldKey);
      item.PropertyChanged += OnItemPropertyChanged;

      throw new InvalidOperationException(
        $"Duplicate {item.UniqueKeyPropertyName} '{newKey}' (case-insensitive).");
    }

    // Accept change
    m_InternalDictionary.Remove(oldKey);
    m_InternalDictionary[newKey] = item;

    CollectionItemPropertyChanged?.Invoke(item,
      new PropertyChangedEventArgs(item.UniqueKeyPropertyName));
  }

  /// <inheritdoc/>
  public bool Equals(ICollection<T> other) => this.CollectionEqual(other);
}
