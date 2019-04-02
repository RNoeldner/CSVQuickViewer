using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CsvTools
{
  public sealed class MappingCollection : Collection<Mapping>, ICloneable<MappingCollection>, IEquatable<MappingCollection>
  {
    public MappingCollection()
    {
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

    public IEnumerable<Mapping> GetByColumn(string columnName)
    {
      foreach (var mapping in Items)
      {
        if (mapping.FileColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase))
          yield return mapping;
      }
    }

    public bool AddIfNew(Mapping fieldMapping)
    {
      if (fieldMapping == null)
        return false;

      var found = false;
      foreach (var map in Items)
      {
        if (!map.FileColumn.Equals(fieldMapping.FileColumn, StringComparison.OrdinalIgnoreCase) ||
            !map.TemplateField.Equals(fieldMapping.TemplateField, StringComparison.OrdinalIgnoreCase))
        {
          continue;
        }

        found = true;
        break;
      }

      if (!found)
        Add(fieldMapping);

      return !found;
    }

    /// <summary>
    ///  Get the IFileSetting Mapping by template column
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="templateField">The template column.</param>
    /// <returns>Null if the template table field is not mapped</returns>
    public Mapping GetByField(string templateField)
    {
      foreach (var map in Items)
      {
        if (map.TemplateField.Equals(templateField, StringComparison.OrdinalIgnoreCase))
          return map;
      }
      return null;
    }

    /// <summary>
    ///  Remove a Fields mapping.
    /// </summary>
    /// <param name="columnName">The source name.</param>
    public void RemoveColumn(string columnName)
    {
      var toBeRemoved = new List<Mapping>(GetByColumn(columnName));

      foreach (var fieldMapping in toBeRemoved)
        Remove(fieldMapping);
    }
  }
}
