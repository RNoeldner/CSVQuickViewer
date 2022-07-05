using System;
using System.Collections.Generic;
using System.Data;


namespace CsvTools
{
  public static class ColumnExtensions
  {

    public static IColumn ToIColumn(this DataColumn dataColumn)
    {
      return new ImmutableColumn(dataColumn.ColumnName, new ImmutableValueFormat(dataColumn.DataType.GetDataType()),
        dataColumn.Ordinal);
    }

  }
}
