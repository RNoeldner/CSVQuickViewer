using System;

namespace CsvTools
{
  /// <summary>
  ///   Class should support CollectionIdentifier to support uniqueness and finding in collection
  /// </summary>
  public interface ICollectionIdentity
  {
    /// <summary>
    ///   Identifier in collections, similar to a hashcode based on a properties that should be
    ///   unique in a collection
    /// </summary>
    /// <returns>HashCode of the identifying properties</returns>
    /// <remarks>In case a required property is not set, this should raise an error</remarks>
    public int CollectionIdentifier { get; }
  }
}