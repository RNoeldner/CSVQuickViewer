using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CsvTools
{
  using System.Linq;

  public sealed class MappingCollection : ObservableCollection<Mapping>, ICloneable<MappingCollection>, IEquatable<MappingCollection>
  {
    public bool AddIfNew(Mapping fieldMapping)
    {
      if (fieldMapping == null)
        return false;

      if (Items.Any(
        map => map.FileColumn.Equals(fieldMapping.FileColumn, StringComparison.OrdinalIgnoreCase)
               && map.TemplateField.Equals(fieldMapping.TemplateField, StringComparison.OrdinalIgnoreCase)))
        return false;

      Add(fieldMapping);
      return true;
    }

    /// <summary>
    ///   Clones this instance into a new instance of the same type
    /// </summary>
    /// <returns></returns>
    public MappingCollection Clone()
    {
      var newMappingCollection = new MappingCollection();
      CopyTo(newMappingCollection);
      return newMappingCollection;
    }

    /// <summary>
    ///   Copies all properties to the other instance
    /// </summary>
    /// <param name="other">The other instance</param>
    public void CopyTo(MappingCollection other) => Items.CollectionCopy(other);

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
    /// </returns>
    public bool Equals(MappingCollection other) => Items.CollectionEqual(other);

    public IEnumerable<Mapping> GetByColumn(string columnName) =>
      Items.Where(mapping => mapping.FileColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Get the IFileSetting Mapping by template column
    /// </summary>
    /// <param name="templateFieldName">The template column.</param>
    /// <returns>
    /// Null if the template table field is not mapped
    /// </returns>
    public Mapping GetByField(string templateFieldName)
    {
      if (string.IsNullOrEmpty(templateFieldName)) return null;
      return Items.FirstOrDefault(
        map => map.TemplateField.Equals(templateFieldName, StringComparison.OrdinalIgnoreCase));
    }

    public string GetColumnName(string templateFieldName) => GetByField(templateFieldName)?.FileColumn;

    /// <summary>
    ///  Remove a Fields mapping.
    /// </summary>
    /// <param name="columnName">The source name.</param>
    public void RemoveColumn(string columnName)
    {
      var toBeRemoved = new List<Mapping>(GetByColumn(columnName));

      if (toBeRemoved.Count == 0)
        return;

      foreach (var fieldMapping in toBeRemoved)
        Remove(fieldMapping);
    }
  }
}