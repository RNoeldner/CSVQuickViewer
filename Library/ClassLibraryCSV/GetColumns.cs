using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public static class GetColumns
  {
    /// <summary>
    /// Fills the Column Format for writer fileSettings
    /// </summary>
    /// <param name="fileSettings">The file settings.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns></returns>
    /// <exception cref="FileWriterException">No SQL Statement given or No SQL Reader set</exception>
    public static async Task FillGuessColumnFormatWriterAsync(
      this IFileSetting fileSettings,
      CancellationToken cancellationToken)
    {
      var res = await fileSettings.SqlStatement.GetColumnsSqlAsync( fileSettings.Timeout, cancellationToken);
      foreach (var item in res)
        fileSettings.ColumnCollection.Add(item);
    }

    /// <summary>
    /// Gets the Columns of a SQL statement
    /// </summary>
    /// <param name="sql">The SQL statement</param>
    /// <param name="timeout"></param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns></returns>
    /// <exception cref="FileWriterException">No SQL Statement given or No SQL Reader set</exception>
    public static async Task<IEnumerable<IColumn>> GetColumnsSqlAsync(this string sql, int timeout,
      CancellationToken cancellationToken)
    {
      if (string.IsNullOrEmpty(sql))
        throw new FileWriterException("No SQL Statement given");
      if (FunctionalDI.SqlDataReader is null)
        throw new FileWriterException("No Async SQL Reader set");

#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
      await
#endif
      using var fileReader = await FunctionalDI.SqlDataReader(sql.NoRecordSQL(), null, timeout, 1, cancellationToken).ConfigureAwait(false);

      // Put the information into the list
      var res = new List<IColumn>();
      foreach (DataRow schemaRow in fileReader.GetSchemaTable()!.Rows)
      {
        var colNo = (int) schemaRow[SchemaTableColumn.ColumnOrdinal];
        var colType = ((Type) schemaRow[SchemaTableColumn.DataType]).GetDataType();
        if (!(schemaRow[SchemaTableColumn.ColumnName] is string colName) || colName.Length == 0)
          colName = $"Column{colNo + 1}";
        res.Add(new ImmutableColumn(colName, new ImmutableValueFormat(colType), (int) schemaRow[SchemaTableColumn.ColumnOrdinal]));
      }
      return res;
    }

    /// <summary>
    ///   Gets the writer source columns, this is different to GetColumnsSqlAsync as the definition for columns are applied
    /// </summary>
    /// <param name="sqlStatement"></param>
    /// <param name="timeout"></param>
    /// <param name="valueFormatGeneral">The general format for the output</param>
    /// <param name="columnDefinitions">Definition for individual columns</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns></returns>
    public static async Task<IEnumerable<IColumn>> GetWriterColumnInformationAsync(
      this string sqlStatement,
      int timeout,
      IValueFormat valueFormatGeneral,
      IReadOnlyCollection<IColumn> columnDefinitions,
      CancellationToken cancellationToken)
    {
      if (valueFormatGeneral is null) throw new ArgumentNullException(nameof(valueFormatGeneral));
      if (columnDefinitions is null) throw new ArgumentNullException(nameof(columnDefinitions));
      if (sqlStatement == null)
        return new List<ImmutableColumn>();

      if (FunctionalDI.SqlDataReader is null)
        throw new FileWriterException("No SQL Reader set");
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
      await
#endif
      using var data = await FunctionalDI.SqlDataReader(
        sqlStatement.NoRecordSQL(),
        null,
        timeout,
        1,
        cancellationToken).ConfigureAwait(false);

      using var dt = data.GetSchemaTable();
      if (dt is null)
        throw new FileWriterException("Could not get source schema");
      return BaseFileWriter.GetColumnInformation(valueFormatGeneral, columnDefinitions, dt);
    }
  }
}
