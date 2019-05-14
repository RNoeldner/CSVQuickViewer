using System;
using System.Collections.ObjectModel;

namespace CsvTools
{
  public sealed class ColumnCollection : ObservableCollection<Column>, ICloneable<ColumnCollection>, IEquatable<ColumnCollection>
  {
    /// <summary>
    ///  Adds the <see cref="Column" /> format to the column list if it does not exist yet
    /// </summary>
    /// <remarks>If the column name already exist it does nothing but return the already defined column</remarks>
    /// <param name="columnFormat">The column format.</param>
    public Column AddIfNew(Column columnFormat)
    {
      var found = Get(columnFormat.Name);
      if (found != null)
        return found;
      base.Add(columnFormat);
      return columnFormat;
    }

    /// <summary>
    ///   Clones this instance into a new instance of the same type
    /// </summary>
    /// <returns></returns>
    public ColumnCollection Clone()
    {
      var newColumnCollection = new ColumnCollection();
      CopyTo(newColumnCollection);
      return newColumnCollection;
    }

    /// <summary>
    ///   Copies all properties to the other instance
    /// </summary>
    /// <param name="other">The other instance</param>
    public void CopyTo(ColumnCollection other) => Items.CollectionCopy(other);

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
    /// </returns>
    public bool Equals(ColumnCollection other) => Items.CollectionEqual(other);

    /// <summary>
    ///  Gets the <see cref="CsvTools.Column" /> with the specified field name.
    /// </summary>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    /// <value>The column format found by the given name, <c>NULL</c> otherwise</value>
    public Column Get(string fieldName)
    {
      if (!string.IsNullOrEmpty(fieldName))
      {
        foreach (var column in Items)
        {
          if (column.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
            return column;
        }
      }
      return null;
    }
  }
}