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

  /// <inheritdoc/>
  public override void Add(Column column)
  {
    Validate(column);
    base.Add(column);
  }

  /// <inheritdoc/>
  public override void AddRange(IEnumerable<Column> columns)
  {
    if (columns is null)
      throw new ArgumentNullException(nameof(columns));

    // We have multiple enumerations
    // Materialize only if not a collection already 
    var list = columns as ICollection<Column> ?? new List<Column>(columns);

    foreach (var column in list)
      Validate(column);

    base.AddRange(list);
  }

  /// <summary>
  ///   Retrieves a column from the collection by its name, using a case-insensitive comparison.
  /// </summary>
  /// <param name="columnName">The name of the column to retrieve. Can be <c>null</c> or empty.</param>
  /// <returns>
  ///   The <see cref="Column"/> with the specified name, or <c>null</c> if no matching column is found
  ///   or if <paramref name="columnName"/> is <c>null</c> or empty.
  /// </returns>
  public Column? GetByName(string? columnName)
  {
    if (string.IsNullOrEmpty(columnName))
      return null;

    for (int i = 0; i < Count; i++)
    {
      if (string.Equals(base[i].Name, columnName, StringComparison.OrdinalIgnoreCase))
        return base[i];
    }

    return null;
  }

  /// <inheritdoc/>
  public override void Insert(int index, Column column)
  {
    Validate(column);
    base.Insert(index, column);
  }
  /// <inheritdoc/>
  public override void InsertRange(int index, IEnumerable<Column> columns)
  {
    if (columns is null)
      throw new ArgumentNullException(nameof(columns));

    // We have multiple enumerations
    // Materialize only if not a collection already 
    var list = columns as ICollection<Column> ?? new List<Column>(columns);

    foreach (var column in list)
      Validate(column);

    base.InsertRange(index, list);
  }
  /// <summary>
  ///   Replaces an existing column with the same name, or adds it if no match is found.
  /// </summary>
  /// <param name="column">The column to replace or add. Must not be <c>null</c> and must have a valid <see cref="Column.Name"/>.</param>
  /// <remarks>
  ///   The replacement is based on the column's <see cref="Column.Name"/>, 
  ///   ensuring predictable behavior independent of reference equality.
  ///   Triggers <see cref="ObservableList{T}.CollectionChanged"/> after a modification.
  /// </remarks>
  public void Replace(Column column)
  {
    Validate(column);

    for (int i = 0; i < base.Count; i++)
    {
      if (string.Equals(base[i].Name, column.Name, StringComparison.OrdinalIgnoreCase))
      {
        base[i] = column;
        OnCollectionChanged();
        return;
      }
    }
    Add(column);
  }

  /// <summary>
  ///   Validates that a column is not <c>null</c> and has a non-empty <see cref="Column.Name"/>.
  /// </summary>
  /// <param name="column">The column to validate.</param>
  /// <exception cref="ArgumentException">
  ///   Thrown if <paramref name="column"/> is <c>null</c> or its <see cref="Column.Name"/> is <c>null</c> or empty.
  /// </exception>
  private static void Validate(Column column)
  {
    if (column is null || string.IsNullOrEmpty(column.Name))
      throw new ArgumentException("The name of a column can not be empty in the collection", nameof(column));
  }
}
