#nullable enable
using System;
using System.Collections.Generic;

namespace CsvTools;

/// <summary>
///   Represents a strongly controlled collection of <see cref="Column"/> objects.
///   All modifications that add or remove columns trigger the <see cref="ObservableList{T}.CollectionChanged"/> event.
///   Internally backed by a <see cref="List{T}"/>.
/// </summary>
public sealed class ColumnCollection : ObservableList<Column>
{
  /// <summary>
  ///   Gets or sets the column at the specified index.
  ///   Assigning a column via the indexer does not trigger <see cref="ObservableList{T}.CollectionChanged"/>.
  /// </summary>
  /// <param name="index">The zero-based index of the column to get or set.</param>
  /// <returns>The column at the specified index.</returns>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is out of range.</exception>
  /// <exception cref="ArgumentException">Thrown if the assigned <see cref="Column"/> is invalid.</exception>
  public new Column this[int index]
  {
    get => base[index];
    set
    {
      Validate(value);
      base[index] = value;
    }
  }

  /// <summary>
  ///   Replaces an existing column with the same name, or adds it if no match is found.
  /// </summary>
  /// <param name="column">The column to add or replace.</param>
  /// <exception cref="ArgumentException">Thrown if <paramref name="column"/> is null or has an empty name.</exception>

  public override void Add(Column column)
  {
    Validate(column);
    InternalAdd(column);
  }

  /// <summary>
  ///   Replaces an existing column with the same name, or adds it if no match is found.
  /// </summary>
  private void InternalAdd(Column column)
  {
    var existingIndex = FindIndex(column.Name);
    if (existingIndex >= 0)
    {
      base[existingIndex] = column; // replace in place
      OnCollectionChanged();
    }
    else
      base.Add(column);
  }

  /// <inheritdoc/>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="columns"/> is null.</exception>
  /// <exception cref="ArgumentException">Thrown if any column is null or has an empty name.</exception>
  public override void AddRange(IEnumerable<Column> columns)
  {
    if (columns is null)
      throw new ArgumentNullException(nameof(columns));

    // We have multiple enumerations
    // Materialize only if not a collection already 
    var list = columns as ICollection<Column> ?? new List<Column>(columns);
    // Validate first (fail fast, no partial mutation)
    foreach (var column in list)
      Validate(column);
    var listAdd = new List<Column>();
    foreach (var column in list)
    {
      var index = -1;
      for (int i = 0; i < Count; i++)
      {
        if (string.Equals(base[i].Name, column.Name, StringComparison.OrdinalIgnoreCase))
        {
          index = i;
          break;
        }
      }

      if (index >= 0)
        // Replace in place
        base[index] = column;
      else
        listAdd.Add(column);
    }

    // Append new column
    base.AddRange(listAdd);
  }

  private int FindIndex(string? columnName) => FindIndex(c => string.Equals(c.Name, columnName, StringComparison.OrdinalIgnoreCase));

  /// <summary>
  ///   Retrieves a column from the collection by its name, using a case-insensitive comparison.
  /// </summary>
  /// <param name="columnName">The name of the column to retrieve. Can be <c>null</c> or empty.</param>
  /// <returns>The <see cref="Column"/> with the specified name, or <c>null</c> if not found.</returns>

  public Column? GetByName(string? columnName)
  {
    if (string.IsNullOrEmpty(columnName))
      return null;

    var index = FindIndex(columnName);
    if (index!=-1)
      return base[index];
    return null;
  }

  /// <inheritdoc/>
  /// <exception cref="ArgumentException">Thrown if <paramref name="column"/> is null, has an empty name, or its name already exists at a different index.</exception>

  public override void Insert(int index, Column column)
  {
    Validate(column);
    // Try to replace existing column with same name
    var existingIndex = FindIndex(column.Name);
    if (existingIndex == index)
    {
      base[existingIndex] = column; // replace in place
      OnCollectionChanged();
    }
    else if (existingIndex==-1)
    {
      base.Insert(index, column); // insert at requested index
    }
    else
    {
      throw new ArgumentException(
            $"A column with the name '{column.Name}' already exists at index {existingIndex}.",
            nameof(column));
    }
  }

  /// <inheritdoc/>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="columns"/> is null.</exception>
  /// <exception cref="ArgumentException">
  ///   Thrown if any column is null, has an empty name, or its name already exists in the collection.
  /// </exception>

  public override void InsertRange(int index, IEnumerable<Column> columns)
  {
    if (columns is null)
      throw new ArgumentNullException(nameof(columns));

    // We have multiple enumerations
    // Materialize only if not a collection already 
    var list = columns as ICollection<Column> ?? new List<Column>(columns);
    if (list.Count > 0)
    {
      foreach (var column in list)
      {
        Validate(column);
        var existingIndex = FindIndex(column.Name);
        if (existingIndex != -1)
          throw new ArgumentException(
              $"A column with the name '{column.Name}' already exists at index {existingIndex}.",
              nameof(columns));
      }
      base.InsertRange(index, list);
    }
  }

  /// <summary>
  ///   Replaces an existing column with the same name, or adds it if no match is found.
  /// </summary>
  /// <param name="column">The column to replace or add.</param>
  /// <remarks>
  ///   Replacement is based on the column's <see cref="Column.Name"/>, independent of reference equality.
  ///   Triggers <see cref="ObservableList{T}.CollectionChanged"/> after modification.
  /// </remarks>
  public void Replace(Column column) => Add(column);

  /// <summary>
  ///   Validates that a column is not null and has a non-empty <see cref="Column.Name"/>.
  /// </summary>
  /// <param name="column">The column to validate.</param>
  /// <exception cref="ArgumentException">
  ///   Thrown if <paramref name="column"/> is null or its <see cref="Column.Name"/> is null or empty.
  /// </exception>
  private static void Validate(Column column)
  {
    if (column is null || string.IsNullOrEmpty(column.Name))
      throw new ArgumentException("The name of a column can not be empty in the collection", nameof(column));
  }
}
