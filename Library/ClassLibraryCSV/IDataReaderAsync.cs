using System.Data;
using System.Threading.Tasks;

namespace CsvTools
{
  public interface IDataReaderAsync : IDataReader
  {
    /// <summary>
    ///   Gets the next result set for readers that have multiple results
    /// </summary>
    /// <returns>Awaitable bool, if true the result set was changed</returns>
    Task<bool> NextResultAsync();

    /// <summary>
    ///   Reads the next record of the current result set
    /// </summary>
    /// <returns>Awaitable bool, if true a record was read</returns>
    Task<bool> ReadAsync();
  }
}