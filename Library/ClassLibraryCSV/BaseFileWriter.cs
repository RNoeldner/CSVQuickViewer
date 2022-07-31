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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   Base class with methods used by all <see cref="IFileWriter" />
  /// </summary>
  public abstract class BaseFileWriter
  {
    protected readonly IReadOnlyCollection<ImmutableColumn> ColumnDefinition;

    protected readonly List<WriterColumn> Columns = new List<WriterColumn>();
    protected readonly string FileSettingDisplay;
    private readonly string m_Footer;
    internal readonly string FullPath;
    private readonly long m_PgpKeyId;
    private readonly string m_IdentifierInContainer;
    private readonly bool m_KeepUnencrypted;
    private readonly Action<string>? m_ReportProgress;
    private readonly Action<long>? m_SetMaxProcess;
    private readonly IValueFormat m_ValueFormatGeneral;
    protected readonly TimeZoneChangeDelegate TimeZoneAdjust;
    protected string Header;
    protected readonly string SourceTimeZone;
    private DateTime m_LastNotification = DateTime.Now;

    protected BaseFileWriter(
      in string id,
      in string fullPath,
      in IValueFormat? valueFormatGeneral,
      long pgpKeyId,
      bool unencrypted,
      in string? identifierInContainer,
      in string? footer,
      in string? header,
      in IEnumerable<IColumn>? columnDefinition,
      in string fileSettingDisplay,
      in TimeZoneChangeDelegate timeZoneAdjust,
      in string sourceTimeZone,
      IProcessDisplay? processDisplay)
    {
      if (string.IsNullOrEmpty(fullPath))
        throw new ArgumentException($"{nameof(fullPath)} can not be empty");
      SourceTimeZone = sourceTimeZone;
      FullPath = FileSystemUtils.ResolvePattern(fullPath) ??
                 throw new ArgumentException($"Make sure path is correct {fullPath}");
      var fileName = FileSystemUtils.GetFileName(FullPath);
      TimeZoneAdjust = timeZoneAdjust;
      m_PgpKeyId = pgpKeyId;
      if (header != null && header.Length > 0)
        Header = ReplacePlaceHolder(
          header,
          fileName,
          id);
      else
        Header = string.Empty;

      if (footer != null && footer.Length > 0)
        m_Footer = ReplacePlaceHolder(
          footer,
          fileName,
          id);
      else
        m_Footer = string.Empty;

      m_ValueFormatGeneral = valueFormatGeneral != null
        ? new ImmutableValueFormat(valueFormatGeneral)
        : new ImmutableValueFormat();
      ColumnDefinition =
        columnDefinition
          ?.Select(col => col as ImmutableColumn ?? new ImmutableColumn(col)).ToList()
        ?? new List<ImmutableColumn>();

      FileSettingDisplay = fileSettingDisplay;
      m_KeepUnencrypted = unencrypted;
      m_IdentifierInContainer = identifierInContainer ?? string.Empty;

      Logger.Debug("Created Writer for {filesetting}", FileSettingDisplay);
      if (processDisplay is null) return;
      m_ReportProgress = t => processDisplay.SetProcess(t, 0);

      if (!(processDisplay is IProcessDisplayTime processDisplayTime)) return;
      processDisplayTime.Maximum = 0;
      m_SetMaxProcess = l => processDisplayTime.Maximum = l;
    }

    public long Records { get; protected set; }

    /// <summary>
    ///   Event handler called if a warning or error occurred
    /// </summary>
    public event EventHandler<WarningEventArgs>? Warning;

    /// <summary>
    ///   Event to be raised if writing is finished
    /// </summary>
    public event EventHandler? WriteFinished;

    /// <summary>
    ///   Gets the column information based on the SQL Source, but overwritten with the definitions
    /// </summary>
    /// <param name="generalFormat">
    ///   general value format for not explicitly specified columns format
    /// </param>
    /// <param name="columnDefinitions"></param>
    /// <param name="schemaTable"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">reader</exception>
    public static IReadOnlyCollection<WriterColumn> GetColumnInformation(
      IValueFormat? generalFormat,
      IReadOnlyCollection<IColumn> columnDefinitions,
      DataTable schemaTable)
    {
      if (schemaTable is null)
        throw new ArgumentNullException(nameof(schemaTable));
      var result = new List<WriterColumn>();

      var colNames = new BiDirectionalDictionary<int, string>();

      // Make names unique and fill the dictionary
      foreach (DataRow schemaRow in schemaTable.Rows)
      {
        var colNo = (int) schemaRow[SchemaTableColumn.ColumnOrdinal];
        if (!(schemaRow[SchemaTableColumn.ColumnName] is string colName) || colName.Length == 0)
          colName = $"Column{colNo + 1}";
        var newName = StringUtils.MakeUniqueInCollection(colNames.Values, colName);
        colNames.Add(colNo, newName);
      }

      // Get default if we do not have the information
      generalFormat ??= new ImmutableValueFormat();

      foreach (DataRow schemaRow in schemaTable.Rows)
      {
        var colNo = (int) schemaRow[SchemaTableColumn.ColumnOrdinal];
        var column =
          columnDefinitions.FirstOrDefault(x => x.Name.Equals(colNames[colNo], StringComparison.OrdinalIgnoreCase));
        if (column is { Ignore: true })
          continue;

        var valueFormat = column?.ValueFormat is null
          ? new ImmutableValueFormat(
            ((Type) schemaRow[SchemaTableColumn.DataType]).GetDataType(),
            generalFormat.DateFormat,
            generalFormat.DateSeparator,
            generalFormat.TimeSeparator,
            generalFormat.NumberFormat,
            generalFormat.GroupSeparator,
            generalFormat.DecimalSeparator,
            generalFormat.True,
            generalFormat.False,
            generalFormat.DisplayNullAs,
            readFolder: generalFormat.ReadFolder,
            writeFolder: generalFormat.WriteFolder,
            fileOutPutPlaceholder: generalFormat.FileOutPutPlaceholder,
            overwrite: generalFormat.Overwrite)
          : new ImmutableValueFormat(
            column.ValueFormat.DataType,
            column.ValueFormat.DateFormat,
            column.ValueFormat.DateSeparator,
            column.ValueFormat.TimeSeparator,
            column.ValueFormat.NumberFormat,
            column.ValueFormat.GroupSeparator,
            column.ValueFormat.DecimalSeparator,
            column.ValueFormat.True,
            column.ValueFormat.False,
            column.ValueFormat.DisplayNullAs,
            readFolder: string.IsNullOrEmpty(column.ValueFormat.ReadFolder)
              ? generalFormat.ReadFolder
              : column.ValueFormat.ReadFolder,
            writeFolder: string.IsNullOrEmpty(column.ValueFormat.WriteFolder)
              ? generalFormat.WriteFolder
              : column.ValueFormat.WriteFolder,
            fileOutPutPlaceholder: string.IsNullOrEmpty(column.ValueFormat.FileOutPutPlaceholder)
              ? generalFormat.FileOutPutPlaceholder
              : column.ValueFormat.FileOutPutPlaceholder,
            overwrite: column.ValueFormat.Overwrite);

        var fieldLength = Math.Max((int) schemaRow[SchemaTableColumn.ColumnSize], 0);
        switch (valueFormat)
        {
          case { DataType: DataTypeEnum.Integer }:
            fieldLength = 10;
            break;

          case { DataType: DataTypeEnum.Boolean }:
          {
            var lenTrue = valueFormat.True.Length;
            var lenFalse = valueFormat.False.Length;
            fieldLength = lenTrue > lenFalse ? lenTrue : lenFalse;
            break;
          }
          case { DataType: DataTypeEnum.Double }:
          case { DataType: DataTypeEnum.Numeric }:
            fieldLength = 28;
            break;

          case { DataType: DataTypeEnum.DateTime }:
            fieldLength = valueFormat.DateFormat.Length;
            break;

          case { DataType: DataTypeEnum.Guid }:
            fieldLength = 36;
            break;
        }

        var constantTimeZone = string.Empty;
        var columnOrdinalTimeZoneReader = -1;

        // the timezone information
        if (column != null)
        {
          var tz = column.TimeZonePart;
          if (!string.IsNullOrEmpty(tz) && !tz.TryGetConstant(out constantTimeZone) &&
              colNames.TryGetByValue(tz, out var ordinal))
            columnOrdinalTimeZoneReader = ordinal;
        }

        // this is problematic, we need to apply timezone mapping here and on date
        var ci = new WriterColumn(
          colNames[colNo],
          colNo,
          valueFormat,
          fieldLength,
          constantTimeZone,
          columnOrdinalTimeZoneReader);

        result.Add(ci);

        // add an extra column for the time, reading columns they get combined, writing them they
        // get separated again

        if (column is null || string.IsNullOrEmpty(column.TimePart) || colNames.ContainsValue(column.TimePart))
          continue;

        if (ci.ValueFormat.DateFormat.IndexOfAny(new[] { 'h', 'H', 'm', 's' }) != -1)
          Logger.Warning(
            $"'{ci.Name}' will create a separate time column '{column.TimePart}' but seems to write time itself '{ci.ValueFormat.DateFormat}'");

        // In case we have a split column, add the second column (unless the column is also present
        result.Add(
          new WriterColumn(
            column.TimePart,
            colNo,
            new ImmutableValueFormat(
              DataTypeEnum.DateTime,
              column.TimePartFormat,
              timeSeparator: column.ValueFormat?.TimeSeparator ?? ":"),
            column.TimePartFormat.Length,
            constantTimeZone,
            columnOrdinalTimeZoneReader));
      }

      return result;
    }

    /// <inheritdoc cref="IFileWriter" />
    public virtual async Task<long> WriteAsync(IFileReader? reader, CancellationToken token)
    {
      if (reader is null)
        return -1;
      HandleWriteStart();

      m_SetMaxProcess?.Invoke(-1);

      try
      {
        var sourceAccess = new SourceAccess(
          FullPath,
          false,
          keyID: m_PgpKeyId,
          keepEncrypted: m_KeepUnencrypted);
        if (!string.IsNullOrEmpty(m_IdentifierInContainer))
          sourceAccess.IdentifierInContainer = m_IdentifierInContainer;
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
        await
#endif
        using var stream = FunctionalDI.OpenStream(sourceAccess);
        await WriteReaderAsync(reader, stream, token).ConfigureAwait(false);
      }
      catch (Exception exc)
      {
        Logger.Error(exc, "Could not write file {filename}", FileSystemUtils.GetShortDisplayFileName(FullPath));
        throw new FileWriterException(
          $"Could not write file '{FileSystemUtils.GetShortDisplayFileName(FullPath)}'\n{exc.SourceExceptionMessage()}",
          exc);
      }
      finally
      {
        Logger.Debug("Finished writing {filesetting} Records: {records}", FileSettingDisplay, Records);
        HandleWriteEnd();
      }

      return Records;
    }

    protected string Footer() =>
      m_Footer.PlaceholderReplace("Records", string.Format(new CultureInfo("en-US"), "{0:n0}", Records));

    /// <summary>
    ///   Handles the error.
    /// </summary>
    /// <param name="columnName">The column name.</param>
    /// <param name="message">The message.</param>
    protected void HandleError(string columnName, string message) =>
      Warning?.Invoke(this, new WarningEventArgs(Records, 0, message, 0, 0, columnName));

    protected void HandleProgress(string text)
    {
      m_ReportProgress?.Invoke(text);
      Logger.Information(text);
    } 

    protected virtual void HandleWriteStart() => Records = 0;

    protected virtual void HandleWriteEnd() => WriteFinished?.Invoke(this, EventArgs.Empty);

    protected void NextRecord()
    {
      Records++;
      if ((DateTime.Now - m_LastNotification).TotalSeconds <= .15) return;
      m_LastNotification = DateTime.Now;
      HandleProgress($"Record {Records:N0}");
    }

    /// <summary>
    ///   Sets the columns by looking at the reader
    /// </summary>
    /// <param name="reader">The reader.</param>
    protected void SetColumns(IFileReader reader)
    {
      Columns.Clear();
      using var dt = reader.GetSchemaTable();
      Columns.AddRange(
        GetColumnInformation(
          m_ValueFormatGeneral,
          ColumnDefinition,
          dt ?? throw new ArgumentException("GetSchemaTable did not return information for reader")));
    }

    public abstract Task WriteReaderAsync(IFileReader reader, Stream output, CancellationToken cancellationToken);

    protected static string ReplacePlaceHolder(string input, string fileName, string id) =>
      input.PlaceholderReplace("ID", id)
        .PlaceholderReplace("FileName", fileName)
        .PlaceholderReplace("CDate", string.Format(new CultureInfo("en-US"), "{0:dd-MMM-yyyy}", DateTime.Now))
        .PlaceholderReplace("CDateLong", string.Format(new CultureInfo("en-US"), "{0:MMMM dd\\, yyyy}", DateTime.Now));

    /// <summary>
    ///   Calls the event handler for warnings
    /// </summary>
    /// <param name="columnName">The column.</param>
    /// <param name="message">The message.</param>
    protected void HandleWarning(string columnName, string message) =>
      Warning?.Invoke(this, new WarningEventArgs(Records, 0, message.AddWarningId(), 0, 0, columnName));

    /// <summary>
    ///   Value conversion of a FileWriter
    /// </summary>
    /// <param name="reader">
    ///   Data Reader / Data Records in case additional columns are needed e.G. for TimeZone
    ///   adjustment based off ColumnOrdinalTimeZone or GetFileName
    /// </param>
    /// <param name="dataObject">The actual data of the column</param>
    /// <param name="columnInfo">Information on ValueConversion</param>
    /// <param name="timeZoneAdjust">Class that does provide means to convert between timezones</param>
    /// <param name="sourceTimeZone">The assumed source timezone of date time columns</param>
    /// <param name="handleWarning">Method to pass on warnings</param>
    /// <returns>Value of the .Net Data type matching the ValueFormat.DataType</returns>
    public static object ValueConversion(in IDataRecord? reader, in object? dataObject, WriterColumn columnInfo,
      in TimeZoneChangeDelegate timeZoneAdjust, string sourceTimeZone, Action<string, string>? handleWarning = null)
    {
      if (dataObject is null || dataObject is DBNull)
        return DBNull.Value;

      // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
      switch (columnInfo.ValueFormat.DataType)
      {
        case DataTypeEnum.Integer:
          return Convert.ToInt64(dataObject);

        case DataTypeEnum.Boolean:
          return Convert.ToBoolean(dataObject);

        case DataTypeEnum.Double:
          return Convert.ToDouble(dataObject);

        case DataTypeEnum.Numeric:
          return Convert.ToDecimal(dataObject);

        case DataTypeEnum.DateTime:
          var dtm = Convert.ToDateTime(dataObject);
          if (!string.IsNullOrEmpty(columnInfo.ConstantTimeZone))
            return timeZoneAdjust(dtm, sourceTimeZone, columnInfo.ConstantTimeZone,
              (msg) => handleWarning?.Invoke(columnInfo.Name, msg));

          if (reader is null || columnInfo.ColumnOrdinalTimeZone <= -1)
            return dtm;

          var destinationTimeZoneId = reader.GetString(columnInfo.ColumnOrdinalTimeZone);
          if (string.IsNullOrEmpty(destinationTimeZoneId))
          {
            handleWarning?.Invoke(columnInfo.Name, "Time zone is empty, value not converted");
            return dtm;
          }

          return timeZoneAdjust(dtm, sourceTimeZone,
            reader.GetString(columnInfo.ColumnOrdinalTimeZone), (msg) => handleWarning?.Invoke(columnInfo.Name, msg));

        case DataTypeEnum.Guid:
          return dataObject is Guid guid ? guid : new Guid(dataObject.ToString());

        default:

          if (columnInfo.ColumnFormatter != null)
            return columnInfo.ColumnFormatter.Write(dataObject, reader,
              (msg) => handleWarning?.Invoke(columnInfo.Name, msg));

          return Convert.ToString(dataObject);
      }
    }

    protected string TextEncodeField(
      object? dataObject,
      WriterColumn columnInfo,
      IDataReader? reader)
    {
      if (columnInfo is null)
        throw new ArgumentNullException(nameof(columnInfo));

      string displayAs;
      try
      {
        var convertedValue = ValueConversion(reader, dataObject, columnInfo, TimeZoneAdjust, SourceTimeZone, HandleWarning);
        if (convertedValue == DBNull.Value)
          displayAs = columnInfo.ValueFormat.DisplayNullAs;
        else
          displayAs = convertedValue switch
          {
            long aLong => aLong.ToString(columnInfo.ValueFormat.NumberFormat, CultureInfo.InvariantCulture).Replace(
              CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator,
              columnInfo.ValueFormat.GroupSeparator),
            bool aBol => aBol ? columnInfo.ValueFormat.True : columnInfo.ValueFormat.False,
            double aDbl => StringConversion.DoubleToString(aDbl, columnInfo.ValueFormat),
            decimal aDec => StringConversion.DecimalToString(aDec, columnInfo.ValueFormat),
            DateTime aDTm => StringConversion.DateTimeToString(aDTm, columnInfo.ValueFormat),
            _ => convertedValue.ToString()
          };
      }
      catch (Exception ex)
      {
        // In case a cast did fail (eg.g trying to format as integer and providing a text, use the
        // original value
        displayAs = Convert.ToString(dataObject);
        if (string.IsNullOrEmpty(displayAs))
          HandleError(columnInfo.Name, ex.Message);
        else
          HandleWarning(
            columnInfo.Name,
            "Value stored as: " + displayAs
                                + $"\nExpected {columnInfo.ValueFormat.DataType} but was {dataObject?.GetType()}"
                                + ex.Message);
      }

      return displayAs;
    }
  }
}