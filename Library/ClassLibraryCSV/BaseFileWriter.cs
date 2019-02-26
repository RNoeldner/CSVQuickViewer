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
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading;

namespace CsvTools
{
  /// <summary>
  ///  A Class to write CSV Files
  /// </summary>
  public abstract class BaseFileWriter
  {
    private readonly IFileSetting m_FileSetting;

    /// <summary>
    ///  A cancellation token, to stop long running processes
    /// </summary>
    private CancellationToken m_CancellationToken;

    private DateTime m_LastNotification = DateTime.Now;
    private IProcessDisplay m_ProcessDisplay;
    private long m_Records;

    /// <summary>
    ///  Initializes a new instance of the <see cref="BaseFileWriter" /> class.
    /// </summary>
    /// <param name="fileSetting">the file setting with the definition for the file</param>
    /// <param name="cancellationToken">A cancellation token to stop writing the file</param>
    protected BaseFileWriter(IFileSetting fileSetting, CancellationToken cancellationToken)
    {
      Contract.Requires(fileSetting != null);
      m_CancellationToken = cancellationToken;
      m_FileSetting = fileSetting;
    }

    /// <summary>
    ///  Event handler called as progress should be displayed
    /// </summary>
    public event EventHandler<ProgressEventArgs> Progress;

    /// <summary>
    ///  Event handler called if a warning or error occurred
    /// </summary>
    public virtual event EventHandler<WarningEventArgs> Warning;

    /// <summary>
    ///  Event to be raised if writing is finished
    /// </summary>
    public event EventHandler WriteFinished;

    /// <summary>
    ///  Gets or sets the error message.
    /// </summary>
    /// <value>The error message.</value>
    public virtual string ErrorMessage { get; protected internal set; }

    /// <summary>
    ///  A Process Display
    /// </summary>
    public virtual IProcessDisplay ProcessDisplay
    {
      get => m_ProcessDisplay;
      set
      {
        if (m_ProcessDisplay != null) Progress -= m_ProcessDisplay.SetProcess;
        m_ProcessDisplay = value;
        if (m_ProcessDisplay == null) return;
        m_CancellationToken = m_ProcessDisplay.CancellationToken;
        Progress += m_ProcessDisplay.SetProcess;
      }
    }

    /// <summary>
    ///  Gets the column information based on the SQL Source, but overwritten with the definitions
    /// </summary>
    /// <param name="dataTable">The schema.</param>
    /// <param name="readerFileSetting">The file format of the reader, can be null.</param>
    /// <returns></returns>
    public IEnumerable<ColumnInfo> GetColumnInformation(IDataReader reader)
    {
      if (reader == null)
        throw new ArgumentNullException(nameof(reader));

      Contract.Ensures(Contract.Result<IEnumerable<ColumnInfo>>() != null);
      var dataTable = reader.GetSchemaTable();

      if (dataTable == null)
        throw new ArgumentNullException(nameof(reader));

      var headers = new HashSet<string>();
      var fieldInfoList = new List<ColumnInfo>();

      // Used for Uniqueness in GetColumnInformationForOneColumn
      var allColums = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      if (dataTable.Columns.Contains(SchemaTableColumn.ColumnName)
        && dataTable.Columns.Contains(SchemaTableColumn.DataType)
        && dataTable.Columns.Contains(SchemaTableColumn.ColumnOrdinal)
        && dataTable.Columns.Contains(SchemaTableColumn.ColumnSize))
      {
        foreach (DataRow schemaRow in dataTable.Rows)
        {
          var columnName = schemaRow[SchemaTableColumn.ColumnName].ToString();
          allColums.Add(columnName);
        }

        // Its a schema table, loop though rows
        foreach (DataRow schemaRow in dataTable.Rows)
        {
          var timeZonePartOrdinal = -1;
          var col = m_FileSetting.GetColumn(schemaRow[SchemaTableColumn.ColumnName].ToString());
          if (!string.IsNullOrEmpty(col?.TimeZonePart))
            foreach (DataRow schemaRowTz in dataTable.Rows)
            {
              var otherColumnName = schemaRowTz[SchemaTableColumn.ColumnName].ToString();
              if (!otherColumnName.Equals(col.TimeZonePart, StringComparison.OrdinalIgnoreCase)) continue;
              timeZonePartOrdinal = (int)schemaRowTz[SchemaTableColumn.ColumnOrdinal];
              break;
            }

          fieldInfoList.AddRange(GetColumnInformationForOneColumn(m_FileSetting, headers,
           schemaRow[SchemaTableColumn.ColumnName].ToString(), (Type)schemaRow[SchemaTableColumn.DataType],
           (int)schemaRow[SchemaTableColumn.ColumnOrdinal], (int)schemaRow[SchemaTableColumn.ColumnSize], allColums,
           timeZonePartOrdinal));
        }
      }
      else
      {
        foreach (DataColumn col in dataTable.Columns) allColums.Add(col.ColumnName);
        // Its a data table retrieve information from columns
        foreach (DataColumn col in dataTable.Columns)
          fieldInfoList.AddRange(GetColumnInformationForOneColumn(m_FileSetting, headers,
           col.ColumnName, col.DataType, col.Ordinal, col.MaxLength, allColums, -1));
      }

      return fieldInfoList;
    }

    /// <summary>
    ///  Gets the data reader schema.
    /// </summary>
    /// <returns>A Data Table</returns>
    public virtual IDataReader GetSchemaReader()
    {
      if (string.IsNullOrEmpty(m_FileSetting.SqlStatement))
      {
        return null;
      }

      var parts = m_FileSetting.SqlStatement.SplitCommandTextByGo();
      // only use the last command
      string sql = parts[parts.Count - 1];
      // in case there is no filter add a filer that filters all we only need the Schema
      if (sql.Contains("SELECT", StringComparison.OrdinalIgnoreCase) &&
        !sql.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
      {
        var idxof = sql.IndexOf("ORDER BY", StringComparison.OrdinalIgnoreCase);
        if (idxof == -1)
          sql += " WHERE 1=0";
        else
          sql = sql.Substring(0, idxof) + "WHERE 1=0";
      }

      return ApplicationSetting.SQLDataReader(sql, m_CancellationToken);
    }

    /// <summary>
    ///  Gets the source data table.
    /// </summary>
    /// <param name="recordLimit">The record limit.</param>
    /// <returns>A data table with all source data</returns>
    public virtual DataTable GetSourceDataTable(uint recordLimit)
    {
      // Using the connection string
      if (string.IsNullOrEmpty(m_FileSetting.SqlStatement)) return null;
      HandleProgress("Executing SQL Statement");
      using (var dataReader = ApplicationSetting.SQLDataReader(m_FileSetting.SqlStatement, m_CancellationToken))
      {
        HandleProgress("Reading returned data");
        var dt = new DataTable();
        dt.BeginLoadData();
        dt.Load(dataReader);
        dt.EndLoadData();
        return dt;
      }
    }

    public void HandleProgress(string text, int progress)
    {
      Progress?.Invoke(this, new ProgressEventArgs(text, progress));
    }

    /// <summary>
    ///  Writes the specified file.
    /// </summary>
    /// <returns>Number of records written</returns>
    public virtual long Write()
    {
      using (IDataReader reader = ApplicationSetting.SQLDataReader(m_FileSetting.SqlStatement, m_CancellationToken))
      {
        return Write(reader);
      }
    }

    /// <summary>
    ///  Writes the specified file reading from the a data table
    /// </summary>
    /// <param name="source">The data that should be written in a <see cref="DataTable" /></param>
    /// <returns>Number of records written</returns>
    public virtual long WriteDataTable(DataTable source)
    {
      using (var reader = source.CreateDataReader())
      {
        return Write(reader);
      }
    }

    protected internal virtual string ReplacePlaceHolder(string input)
    {
      return input.PlaceholderReplace("ID", m_FileSetting.ID)
       .PlaceholderReplace("FileName", m_FileSetting.FileName)
       .PlaceholderReplace("Records", string.Format(new CultureInfo("en-US"), "{0:n0}", m_Records))
       .PlaceholderReplace("Delim", m_FileSetting.FileFormat.FieldDelimiterChar.ToString(CultureInfo.CurrentCulture))
       .PlaceholderReplace("CDate", string.Format(new CultureInfo("en-US"), "{0:dd-MMM-yyyy}", DateTime.Now))
       .PlaceholderReplace("CDateLong", string.Format(new CultureInfo("en-US"), "{0:MMMM dd\\, yyyy}", DateTime.Now));
    }

    /// <summary>
    ///  Handles the error.
    /// </summary>
    /// <param name="columnName">The column name.</param>
    /// <param name="message">The message.</param>
    protected virtual void HandleError(string columnName, string message)
    {
      Warning?.Invoke(this, new WarningEventArgs(m_Records, 0, message, 0, 0, columnName));
    }

    protected void HandleProgress(string text)
    {
      Progress?.Invoke(this, new ProgressEventArgs(text));
    }

    /// <summary>
    /// Handles the time zone for da date time column
    /// </summary>
    /// <param name="dataObject">The data object.</param>
    /// <param name="columnInfo">The column information.</param>
    /// <param name="reader">The reader.</param>
    /// <returns></returns>
    protected DateTime HandleTimeZone(DateTime dataObject, ColumnInfo columnInfo, IDataRecord reader)
    {
      if (columnInfo.ColumnOridinalTimeZoneReader > -1)
      {
        var sourcetimeZoneID = reader.GetString(columnInfo.ColumnOridinalTimeZoneReader);
        if (string.IsNullOrEmpty(sourcetimeZoneID))
        {
          HandleWarning(columnInfo.Header, "Time zone is empty, value not converted");
        }
        else
        {
          try
          {
            return TimeZoneMapping.ConvertTime(dataObject, sourcetimeZoneID, ApplicationSetting.ToolSetting.DestinationTimeZone);
          }
          catch (ApplicationException ex)
          {
            HandleWarning(columnInfo.Header, ex.Message);
          }
        }
      }
      return dataObject;
    }

    /// <summary>
    ///  Calls the event handler for warnings
    /// </summary>
    /// <param name="columnName">The column.</param>
    /// <param name="message">The message.</param>
    protected virtual void HandleWarning(string columnName, string message)
    {
      Warning?.Invoke(this, new WarningEventArgs(m_Records, 0, message.AddWarningId(), 0, 0, columnName));
    }

    protected void HandleWriteFinished()
    {
      m_FileSetting.FileLastWriteTimeUtc = DateTime.UtcNow;
      WriteFinished?.Invoke(this, null);
    }

    protected void HandleWriteStart()
    {
      m_Records = 0;
    }

    protected void NextRecord()
    {
      m_Records++;
      if (Progress == null) return;
      if (!((DateTime.Now - m_LastNotification).TotalSeconds > .15)) return;
      m_LastNotification = DateTime.Now;
      HandleProgress($"Record {m_Records:N0}");
    }

    /// <summary>
    ///  Encodes the field.
    /// </summary>
    /// <param name="fileFormat">The settings.</param>
    /// <param name="dataObject">The data object.</param>
    /// <param name="columnInfo">Column Information</param>
    /// <param name="isHeader">if set to <c>true</c> the current line is the header.</param>
    /// <param name="getTimeZone"></param>
    /// <param name="handleQualify"></param>
    /// <returns>proper formated CSV / Fix Length field</returns>
    protected string TextEncodeField(FileFormat fileFormat, object dataObject, ColumnInfo columnInfo, bool isHeader,
   IDataReader reader, Func<string, ColumnInfo, FileFormat, string> handleQualify)
    {
      Contract.Requires(fileFormat != null);
      if (fileFormat.IsFixedLength && columnInfo.FieldLength == 0)
        throw new ApplicationException("For fix length output the length of the columns needs to be specified.");

      string displayAs;
      if (isHeader)
      {
        displayAs = dataObject.ToString();
      }
      else
      {
        if (columnInfo.ValueFormat == null)
          columnInfo.ValueFormat = new ValueFormat();

        try
        {
          if (dataObject == null || dataObject is DBNull)
            displayAs = string.Empty;
          else
            switch (columnInfo.DataType)
            {
              case DataType.Integer:
                displayAs = dataObject is long l
                 ? l.ToString("0", CultureInfo.InvariantCulture)
                 : ((int)dataObject).ToString("0", CultureInfo.InvariantCulture);
                break;

              case DataType.Boolean:
                displayAs = (bool)dataObject ? columnInfo.ValueFormat.True : columnInfo.ValueFormat.False;
                break;

              case DataType.Double:
                displayAs = StringConversion.DoubleToString(
                 dataObject is double d ? d : Convert.ToDouble(dataObject.ToString(), CultureInfo.InvariantCulture),
                 columnInfo.ValueFormat);
                break;

              case DataType.Numeric:
                displayAs = StringConversion.DecimalToString(
                 dataObject is decimal @decimal
                  ? @decimal
                  : Convert.ToDecimal(dataObject.ToString(), CultureInfo.InvariantCulture), columnInfo.ValueFormat);
                break;

              case DataType.DateTime:
                displayAs = StringConversion.DateTimeToString(
                 HandleTimeZone((DateTime)dataObject, columnInfo, reader), columnInfo.ValueFormat);
                break;

              case DataType.Guid:
                // 382c74c3-721d-4f34-80e5-57657b6cbc27
                displayAs = ((Guid)dataObject).ToString();
                break;

              default:
                displayAs = dataObject.ToString();
                if (columnInfo.DataType == DataType.TextToHtml)
                  displayAs = HTMLStyle.TextToHtmlEncode(displayAs);

                // a new line of any kind will be replaced with the placeholder if set
                if (fileFormat.NewLinePlaceholder.Length > 0)
                  displayAs = StringUtils.HandleCRLFCombinations(displayAs, fileFormat.NewLinePlaceholder);

                if (fileFormat.DelimiterPlaceholder.Length > 0)
                  displayAs = displayAs.Replace(fileFormat.FieldDelimiterChar.ToString(CultureInfo.CurrentCulture),
                   fileFormat.DelimiterPlaceholder);

                if (fileFormat.QuotePlaceholder.Length > 0 && fileFormat.FieldQualifier.Length > 0)
                  displayAs = displayAs.Replace(fileFormat.FieldQualifier, fileFormat.QuotePlaceholder);

                break;
            }
        }
        catch (Exception ex)
        {
          // In case a cast did fail (eg.g trying to format as integer and providing a text, use the original value
          displayAs = dataObject?.ToString() ?? string.Empty;
          if (string.IsNullOrEmpty(displayAs))
            HandleError(columnInfo.Header, ex.Message);
          else
            HandleWarning(columnInfo.Header, "Value stored as: " + displayAs + "\n" + ex.Message);
        }
      }

      // Adjust the output in case its is fixed length
      if (fileFormat.IsFixedLength)
      {
        if (displayAs.Length <= columnInfo.FieldLength || columnInfo.FieldLength <= 0)
          return displayAs.PadRight(columnInfo.FieldLength, ' ');
        HandleWarning(columnInfo.Header,
         $"Text with length of {displayAs.Length} has been cut off after {columnInfo.FieldLength} character");
        return displayAs.Substring(0, columnInfo.FieldLength);
      }

      // Qualify text if required
      if (!string.IsNullOrEmpty(fileFormat.FieldQualifier) && handleQualify != null)
        return handleQualify(displayAs, columnInfo, fileFormat);

      return displayAs;
    }

    protected abstract void Write(IDataReader reader, System.IO.Stream output, CancellationToken cancellationToken);

    /// <summary>
    ///  Gets the column information for one column, can return up to two columns for a date time column because of TimePart
    /// </summary>
    /// <param name="writerFileSetting">The writer file setting.</param>
    /// <param name="readerFileSetting">The reader file setting.</param>
    /// <param name="headers">The column headers.</param>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="columnDataType">Type of the column data.</param>
    /// <param name="columnOrdinal">The column ordinal.</param>
    /// <param name="columnSize">defined size of the column.</param>
    /// <param name="columns">The column collection</param>
    /// <param name="columnOridinalTimeZoneReader"></param>
    /// <returns>
    ///  Can return multiple columns in case we have a TimeaPart and the other column is hidden (If the Time part is
    ///  not hidden it will be returned when looking at it)
    /// </returns>
    private static IEnumerable<ColumnInfo> GetColumnInformationForOneColumn(IFileSetting writerFileSetting,
    ICollection<string> headers, string columnName, Type columnDataType,
   int columnOrdinal, int columnSize, ICollection<string> columns, int columnOridinalTimeZoneReader)
    {
      Contract.Requires(headers != null);

      var columnFormat = writerFileSetting.GetColumn(columnName);
      if (columnFormat != null && columnFormat.Ignore)
        yield break;

      var valueFormat = columnFormat != null ? columnFormat.ValueFormat : writerFileSetting.FileFormat.ValueFormat;
      var dataType = columnDataType.GetDataType();

      yield return new ColumnInfo
      {
        Header = GetUniqueFieldName(headers, columnName),
        ColumnOridinalReader = columnOrdinal,
        ColumnOridinalTimeZoneReader = columnOridinalTimeZoneReader,
        Column = columnFormat,
        FieldLength = GetFieldLength(dataType, columnSize, valueFormat),
        ValueFormat = valueFormat,
        DataType = dataType
      };

      var addTimeFormat = false;
      // Without TimePart we are done, otherwise we need to add a field for this
      if (!string.IsNullOrEmpty(columnFormat?.TimePart))
        addTimeFormat |= !columns.Contains(columnFormat.TimePart);

      if (!addTimeFormat) yield break;
      var columnNameTime = GetUniqueFieldName(headers, columnFormat.TimePart);
      var cfTimePart = new Column
      {
        Name = columnNameTime,
        DataType = DataType.DateTime,
        TimeSeparator = columnFormat.TimeSeparator,
        DateFormat = columnFormat.TimePartFormat
      };

      // In case we have a split column, add the second column (unless the column is also present
      yield return new ColumnInfo
      {
        IsTimePart = true,
        Header = columnNameTime,
        ColumnOridinalReader = columnOrdinal,
        DataType = DataType.DateTime,
        Column = cfTimePart,
        FieldLength = columnFormat.TimePartFormat.Length,
        ValueFormat = new ValueFormat
        {
          TimeSeparator = columnFormat.TimeSeparator,
          DateFormat = columnFormat.TimePartFormat
        }
      };
    }

    /// <summary>
    ///  Get the length of a fields based on the value format, value fields do have a length for
    ///  aligned text exports
    /// </summary>
    /// <param name="dataType">Type of the data.</param>
    /// <param name="defaultLength">The default length.</param>
    /// <param name="format">The format <see cref="ValueFormat" />.</param>
    /// <returns>The length of the field in characters.</returns>
    private static int GetFieldLength(DataType dataType, int defaultLength, ValueFormat format)
    {
      switch (dataType)
      {
        case DataType.Integer:
          return 10;

        case DataType.Boolean:
          var lenTrue = format.True.Length;
          var lenFalse = format.False.Length;
          return lenTrue > lenFalse ? lenTrue : lenFalse;

        case DataType.Double:
        case DataType.Numeric:
          return 28;

        case DataType.DateTime:
          return format.DateFormat.Length;

        default:
          return Math.Max(defaultLength, 0);
      }
    }

    private static string GetUniqueFieldName(ICollection<string> headers, string defaultName)
    {
      Contract.Requires(headers != null);
      var counter = 1;
      var fieldName = defaultName;
      while (headers.Contains(fieldName))
        fieldName = defaultName + (++counter).ToString(CultureInfo.InvariantCulture);
      headers.Add(fieldName);
      return fieldName;
    }

    private long Write(IDataReader reader)
    {
      if (reader == null)
        return -1;
      m_Records = 0;
      if (m_ProcessDisplay != null) m_ProcessDisplay.Maximum = -1;
      try
      {
        using (var improvedStream = ImprovedStream.OpenWrite(m_FileSetting.FullPath, ProcessDisplay, m_FileSetting.Recipient))
        {
          Write(reader, improvedStream.Stream, m_CancellationToken);
        }

        m_FileSetting.FileLastWriteTimeUtc = DateTime.UtcNow;
      }
      catch (Exception exc)
      {
        ErrorMessage = $"Could not write file '{m_FileSetting.FileName}'.\r\n{exc.ExceptionMessages()}";
        if (m_FileSetting.InOverview)
          throw;
      }
      finally
      {
        HandleWriteFinished();
      }
      return m_Records;
    }
  }
}