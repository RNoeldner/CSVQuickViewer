using System;
using System.Data;
using System.Threading.Tasks;

namespace CsvTools
{
  public interface IDataReaderAsync : IDataReader
  {
    /// <summary>
    ///   Get the schema table for the data raeder
    /// </summary>
    /// <returns>Awaitable DataTbale with Schema information</returns>
    Task<DataTable> GetSchemaTableAsync();

    [Obsolete("Use GetSchemaTableAsync")]
    new DataTable GetSchemaTable();

    /// <summary>
    ///   Gets the next result set for cousres that have multiple results
    /// </summary>
    /// <returns>Awaitable bool, if true the result set was changed</returns>
    Task<bool> NextResultAsync();

    [Obsolete("Use NextResultAsync")]
    new bool NextResult();

    /// <summary>
    ///   Reads the next record of the current result set
    /// </summary>
    /// <returns>Awaitable bool, if true a record was read</returns>
    Task<bool> ReadAsync();

    [Obsolete("Use ReadAsync")]
    new bool Read();
  }
}