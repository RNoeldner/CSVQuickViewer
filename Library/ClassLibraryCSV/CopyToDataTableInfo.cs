using System.Collections.Generic;
using System.Data;

namespace CsvTools
{
  public struct CopyToDataTableInfo
  {
    public BiDirectionalDictionary<int, int> Mapping;
    public DataColumn RecordNumber;
    public DataColumn StartLine;
    public DataColumn EndLine;
    public DataColumn Error;
    public IList<string> ReaderColumns;
  }
}