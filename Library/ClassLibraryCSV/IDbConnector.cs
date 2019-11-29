/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///  Interface for a database connector
  /// </summary>
  public interface IDbConnector
  {
    /// <summary>
    ///  Checks if the database has table.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="cancellationToken">a Cancellation token</param>
    /// <returns></returns>
    bool CheckDbHasTable(string tableName, CancellationToken cancellationToken);

    Task<bool> CheckDbHasTableAsync(string tableName, CancellationToken cancellationToken);

    /// <summary>
    ///  Checks if the embedded database does have the field in the table.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="cancellationToken">a Cancellation token</param>
    /// <returns><c>true</c> if field is present, otherwise <c>false</c></returns>
    bool CheckTableHasField(string tableName, string fieldName, CancellationToken cancellationToken);

    Task<bool> CheckTableHasFieldAsync(string tableName, string fieldName, CancellationToken cancellationToken);

    /// <summary>
    ///  Gets a create table statement based on the passed in columns, adds generic fields based on flags
    /// </summary>
    /// <param name="columns">An enumeration of <see cref="Column" />, each of these will be created</param>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="includeErrorField">if set to <c>true</c> add an error field.</param>
    /// <param name="includeRecordNo">if set to <c>true</c> add a record no field</param>
    /// <param name="includeEndLineNo">if set to <c>true</c> add an end-line no field.</param>
    void CreateTable(IEnumerable<Column> columns, string tableName, bool includeErrorField, bool includeRecordNo,
      bool includeEndLineNo, CancellationToken cancellationToken);

    Task CreateTableAsync(IEnumerable<Column> columns, string tableName, bool includeErrorField, bool includeRecordNo,
      bool includeEndLineNo, CancellationToken cancellationToken);

    /// <summary>
    ///  Drops the given table.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="cancellationToken">a Cancellation token</param>
    /// <returns></returns>
    void DropTable(string tableName, CancellationToken cancellationToken);

    Task DropTableAsync(string tableName, CancellationToken cancellationToken);

    /// <summary>
    /// Remove the database itself
    /// </summary>
    /// <param name="cancellationToken"></param>
    void DropDatabase(CancellationToken cancellationToken);

    Task DropDatabaseAsync(CancellationToken cancellationToken);

    /// <summary>
    ///  Removes all existing tables
    /// </summary>
    /// <param name="cancellationToken">a Cancellation token</param>
    void EmptyDatabase(CancellationToken cancellationToken);

    Task EmptyDatabaseAsync(CancellationToken cancellationToken);

    /// <summary>
    ///  Makes sure the database does exist and is accessible
    /// </summary>
    /// <param name="cancellationToken">a Cancellation token</param>
    /// <returns>True if the database was created</returns>
    bool EnsureDBExists(CancellationToken cancellationToken);

    Task<bool> EnsureDBExistsAsync(CancellationToken cancellationToken);

    /// <summary>
    ///  Executes a commands that can be separated by GO without returning a result.
    /// </summary>
    /// <param name="sqlStatement">The sql statement.</param>
    /// <param name="cancellationToken">a Cancellation token</param>
    /// <returns>Number of affected records</returns>
    int ExecuteNonQueries(string sqlStatement, CancellationToken cancellationToken);

    Task<int> ExecuteNonQueriesAsync(string sqlStatement, CancellationToken cancellationToken);

    /// <summary>
    ///  Executes a commands that can be separated by GO without returning a result.
    /// </summary>
    /// <param name="parts">The parts.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///  Number of affected records
    /// </returns>
    int ExecuteNonQueries(IEnumerable<string> parts, CancellationToken cancellationToken);

    Task<int> ExecuteNonQueriesAsync(IEnumerable<string> parts, CancellationToken cancellationToken);

    /// <summary>
    ///  Executes a command without returning a result.
    /// </summary>
    /// <param name="commandText">The command text.</param>
    /// <param name="cancellationToken">a Cancellation token</param>
    /// <returns>Number of affected records</returns>
    int ExecuteNonQuery(string commandText, CancellationToken cancellationToken);

    Task<int> ExecuteNonQueryAsync(string commandText, CancellationToken cancellationToken);

    /// <summary>
    ///  Executes a sql command, and returns a data reader.
    /// </summary>
    /// <param name="sqlStatement">The sql statement.</param>
    /// <param name="processDisplay">A process display</param>
    /// <returns>An open data reader.</returns>
    /// <remarks>
    ///  Please use this with caution, the command and connection can not be disposed
    /// </remarks>
    DbDataReader ExecuteReader(string sqlStatement, IProcessDisplay processDisplay, int commandTimeout);

    Task<DbDataReader> ExecuteReaderAsync(string sqlStatement, IProcessDisplay processDisplay, int commandTimeout);

    /// <summary>
    ///  Executes the a scalar query
    /// </summary>
    /// <param name="sqlStatement">The command text.</param>
    /// <param name="commandTimeout">The command timeout.</param>
    /// <param name="cancellationToken">a Cancellation token</param>
    /// <returns>
    ///  The first column of the first row returned by the command
    /// </returns>
    object ExecuteScalar(string sqlStatement, int commandTimeout, CancellationToken cancellationToken);

    Task<object> ExecuteScalarAsync(string sqlStatement, int commandTimeout, CancellationToken cancellationToken);

    /// <summary>
    ///  Flushes all caches of this instance.
    /// </summary>
    void Flush();

    /// <summary>
    ///  Gets the connection.
    /// </summary>
    /// <returns>The database connection object</returns>
    DbConnection GetConnection(CancellationToken cancellationToken);

    Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken);

    /// <summary>
    ///  Gets the connection.
    /// </summary>
    /// <param name="infoMessages">Handing messages from the connection</param>
    /// <param name="cancellationToken">A cancellation Token</param>
    /// <returns>The database connection object</returns>
    DbConnection GetConnection(EventHandler<string> infoMessages, CancellationToken cancellationToken);

    Task<DbConnection> GetConnectionAsync(EventHandler<string> infoMessages, CancellationToken cancellationToken);

    /// <summary>
    ///  Gets the <see cref="DataType" /> of the table.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="cancellationToken">A cancellation Token</param>
    /// <returns>
    ///  A Dictionary with name of the column and <see cref="DataType" />
    /// </returns>
    IDictionary<string, DataType> GetDataTypes(string tableName, CancellationToken cancellationToken);

    Task<IDictionary<string, DataType>> GetDataTypesAsync(string tableName, CancellationToken cancellationToken);

    /// <summary>
    ///  Gets the number of rows in a table
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="where">A where condition</param>
    /// <param name="cancellationToken">A cancellation Token</param>
    /// <returns>
    ///  The number of rows in the table
    /// </returns>
    long GetRowCount(string tableName, string where, CancellationToken cancellationToken);

    Task<long> GetRowCountAsync(string tableName, string where, CancellationToken cancellationToken);

    /// <summary>
    /// Get the UTC date/time when an table was created in the database
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="cancellationToken">A cancellation Token</param>
    /// <returns>The time stamp of the table in UTC</returns>
    DateTime GetTimeCreated(string tableName, CancellationToken cancellationToken);

    /// <summary>
    ///  Gets a dynamic SQL command.
    /// </summary>
    /// <param name="sqlStatement">The SQL.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="commandTimeout">The command timeout.</param>
    /// <returns>
    ///  A <see cref="DbCommand" /> with the SQL statement provided
    /// </returns>
    DbCommand GetSqlCommand(string sqlStatement, DbConnection connection, int commandTimeout);

    /// <summary>
    ///  Gets the tables in the database
    /// </summary>
    /// <param name="cancellationToken">A cancellation Token</param>
    /// <returns>An array of table names</returns>
    ICollection<string> GetTables(CancellationToken cancellationToken);

    Task<ICollection<string>> GetTablesAsync(CancellationToken cancellationToken);

    /// <summary>
    ///  Gets the values of a column in a table
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    void GetValues(string columnName, string tableName, ICollection<string> values, CancellationToken cancellationToken);

    Task GetValuesAsync(string columnName, string tableName, ICollection<string> values, CancellationToken cancellationToken);

    /// <summary>
    ///  Removes all existing tables
    /// </summary>
    void LeaveDatabase();

    /// <summary>
    /// Change length of the columns that are defined in nvarchar(max) to a limited list so they can be indexed
    /// </summary>
    /// <param name="tableName">The name of the database table</param>
    /// <param name="processDisplay">a process display</param>
    /// <param name="fields">collection of fields to check</param>
    /// <returns>A list of fields that can now be indexed</returns>
    ICollection<string> ProcessColumnLength(string tableName, IProcessDisplay processDisplay, ICollection<string> fields);

    Task<ICollection<string>> ProcessColumnLengthAsync(string tableName, IProcessDisplay processDisplay, ICollection<string> fields);

    /// <summary>
    ///  Stores a list of number in a table and executes a SQL statement that can reference the lines the integer are in table
    ///  (#)
    /// </summary>
    /// <param name="numbers">The line numbers.</param>
    /// <param name="commandText">
    ///  The command text make sure the command has a placeholder (#) for the name of the temporary
    ///  table.
    /// </param>
    /// <param name="processDisplay">a process Display to show progress</param>
    /// <param name="description">The lead description in the progress</param>
    void ProcessForNumber(ICollection<long> numbers, string commandText, IProcessDisplay processDisplay, string description);

    Task ProcessForNumberAsync(ICollection<long> numbers, string commandText, IProcessDisplay processDisplay, string description);

    /// <summary>
    ///  Loop though a returned SQL statement and invoke passed in method for each record
    /// </summary>
    /// <param name="sqlStatement">The sql statement.</param>
    /// <param name="beforeLoop">The before loop.</param>
    /// <param name="eachRecord">delegate (DbDataReader dataReader)/</param>
    /// <param name="infoMessages">Information types messages from the server can be processed here e.g. PRINT</param>
    /// <param name="cancellationToken">A cancellation Token</param>
    void ProcessReader(string sqlStatement, Action<DbDataReader> beforeLoop, Action<DbDataReader> eachRecord, EventHandler<string> infoMessages, CancellationToken cancellationToken);

    Task ProcessReaderAsync(string sqlStatement, Action<DbDataReader> beforeLoop, Action<DbDataReader> eachRecord, EventHandler<string> infoMessages, CancellationToken cancellationToken);

    /// <summary>
    ///  Checks if the database has table.
    /// </summary>
    /// <param name="oldTableName">Name of the table.</param>
    /// <param name="newTableName">New Name of the table.</param>
    /// <param name="cancellationToken">A cancellation Token</param>
    /// <returns></returns>
    bool RenameTable(string oldTableName, string newTableName, CancellationToken cancellationToken);

    Task<bool> RenameTableAsync(string oldTableName, string newTableName, CancellationToken cancellationToken);

    /// <summary>
    /// Does this Database connector support StoreDataTable?
    /// </summary>
    /// <<remarks>If StoreDataTable is using single INSERT statements to process the DataTable its best to do this directly</remarks>
    bool SupportStoreDataTable { get; }

    /// <summary>
    ///  Stores the data table appending all rows to the table and empties out the data table after its persistent
    /// </summary>
    /// <param name="dataTable">The data table, in case there is no record nothing will happen</param>
    /// <param name="destinationTableName">Name of the destination table.</param>
    /// <param name="eventHandler">Event called after some records have been processed</param>
    /// <param name="cancellationToken">A cancellation Token</param>
    void StoreDataTable(DataTable dataTable, string destinationTableName, EventHandler<long> eventHandler, CancellationToken cancellationToken);

    Task StoreDataTableAsync(DataTable dataTable, string destinationTableName, EventHandler<long> eventHandler, int bulkCopyTimeout, CancellationToken cancellationToken);
  }
}