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
  ///   A Class to write CSV Files
  /// </summary>
  public abstract class BaseFileWriter
  {
    protected readonly IReadOnlyCollection<ImmutableColumn> ColumnDefinition;

    protected readonly List<WriterColumn> Columns = new List<WriterColumn>();
    protected string m_Footer;
    protected string Header;
    protected readonly IValueFormat ValueFormatGeneral;
    private readonly string m_FileSettingDisplay;
    private readonly string m_FullPath;
    private readonly string m_IdentifierInContainer;
    private readonly bool m_KeepUnencrypted;
    private readonly string m_Recipient;
    private readonly Action<string>? m_ReportProgress;
    private readonly Action<long>? m_SetMaxProcess;
    private DateTime m_LastNotification = DateTime.Now;

    protected BaseFileWriter(
      in string id,
      in string fullPath,
      in IValueFormat? valueFormatGeneral,
      in string? recipient,
      bool unencrypted,
      in string? identifierInContainer,
      in string? footer,
      in string? header,
      in IEnumerable<IColumn>? columnDefinition,
      in string fileSettingDisplay,
      IProcessDisplay? processDisplay)
    {
      if (string.IsNullOrEmpty(fullPath))
        throw new ArgumentException($"{nameof(fullPath)} can not be empty");
      var fileName = FileSystemUtils.GetFileName(fullPath);
      m_FullPath = fullPath;
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
          id );
      else
        m_Footer = string.Empty;
      
      if (valueFormatGeneral != null)
        ValueFormatGeneral = new ImmutableValueFormat(
          valueFormatGeneral.DataType,
          valueFormatGeneral.DateFormat,
          valueFormatGeneral.DateSeparator,
          valueFormatGeneral.TimeSeparator,
          valueFormatGeneral.NumberFormat,
          valueFormatGeneral.GroupSeparator,
          valueFormatGeneral.DecimalSeparator,
          valueFormatGeneral.True,
          valueFormatGeneral.False,
          valueFormatGeneral.DisplayNullAs);
      else
        ValueFormatGeneral = new ImmutableValueFormat();
      ColumnDefinition =
        columnDefinition
          ?.Select(col => col is ImmutableColumn immutableColumn ? immutableColumn : new ImmutableColumn(col)).ToList()
        ?? new List<ImmutableColumn>();
      

      m_FileSettingDisplay = fileSettingDisplay;
      m_Recipient = recipient ?? string.Empty;
      m_KeepUnencrypted = unencrypted;
      m_IdentifierInContainer = identifierInContainer ?? string.Empty;

      Logger.Debug("Created Writer for {filesetting}", m_FileSettingDisplay);
      if (processDisplay is null) return;
      m_ReportProgress = t => processDisplay.SetProcess(t, 0, true);
      if (!(processDisplay is IProcessDisplayTime processDisplayTime)) return;
      processDisplayTime.Maximum = 0;
      m_SetMaxProcess = l => processDisplayTime.Maximum = l;
    }

    /// <summary>
    ///   Event handler called if a warning or error occurred
    /// </summary>
    public event EventHandler<WarningEventArgs>? Warning;

    /// <summary>
    ///   Event to be raised if writing is finished
    /// </summary>
    public event EventHandler? WriteFinished;

    /*
        /// <summary>
        ///   Initializes a new instance of the <see cref="BaseFileWriter" /> class.
        /// </summary>
        /// <param name="fileSetting">the file setting with the definition for the file</param>
        /// <param name="processDisplay">The process display.</param>
        /// <exception cref="ArgumentNullException">fileSetting</exception>
        /// <exception cref="ArgumentException">No SQL Reader set</exception>
        protected BaseFileWriter(IFileSettingPhysicalFile fileSetting, IProcessDisplay? processDisplay)
          : this(fileSetting.ID, fileSetting.FullPath, fileSetting.FileFormat.ValueFormatMutable, fileSetting.FileFormat, recipient: fileSetting.Recipient,
            unencrypted: fileSetting.KeepUnencrypted, identifierInContainer: fileSetting.IdentifierInContainer, footer: fileSetting.Footer, header: fileSetting.Header,
            columnDefinition: fileSetting.ColumnCollection, fileSettingDisplay: Convert.ToString(fileSetting), processDisplay: processDisplay)
        {
        }
    */
    private long Records { get; set; }

    /// <summary>
    ///   Gets the column information based on the SQL Source, but overwritten with the definitions
    /// </summary>
    /// <param name="generalFormat">
    ///   general value format for not explicitly specified columns format
    /// </param>
    /// <param name="columnDefinitions"></param>
    /// <param name="schemaTable"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">reader</exception>
    public static IEnumerable<IColumn> GetColumnInformation(
      IValueFormat generalFormat,
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
          continue;
        var newName = StringUtils.MakeUniqueInCollection(colNames.Values, colName);
        colNames.Add(colNo, newName);
      }

      foreach (DataRow schemaRow in schemaTable.Rows)
      {
        var colNo = (int) schemaRow[SchemaTableColumn.ColumnOrdinal];
        var column =
          columnDefinitions.FirstOrDefault(x => x.Name.Equals(colNames[colNo], StringComparison.OrdinalIgnoreCase));

        if (column is { Ignore: true })
          continue;

        // Based on the data Type in the reader defined and the general format create the value format
        var valueFormat = column?.ValueFormat ?? new ImmutableValueFormat(
                            ((Type) schemaRow[SchemaTableColumn.DataType]).GetDataType(),
                            generalFormat.DateFormat,
                            generalFormat.DateSeparator,
                            generalFormat.TimeSeparator,
                            generalFormat.NumberFormat,
                            generalFormat.GroupSeparator,
                            generalFormat.DecimalSeparator,
                            generalFormat.True,
                            generalFormat.False,
                            generalFormat.DisplayNullAs);

        var fieldLength = Math.Max((int) schemaRow[SchemaTableColumn.ColumnSize], 0);
        switch (valueFormat.DataType)
        {
          case DataType.Integer:
            fieldLength = 10;
            break;

          case DataType.Boolean:
          {
            var lenTrue = valueFormat.True.Length;
            var lenFalse = valueFormat.False.Length;
            fieldLength = lenTrue > lenFalse ? lenTrue : lenFalse;
            break;
          }
          case DataType.Double:
          case DataType.Numeric:
            fieldLength = 28;
            break;

          case DataType.DateTime:
            fieldLength = valueFormat.DateFormat.Length;
            break;

          case DataType.Guid:
            fieldLength = 36;
            break;

          case DataType.String:
            break;

          default:
            throw new ArgumentOutOfRangeException();
        }

        var constantTimeZone = string.Empty;
        var columnOrdinalTimeZoneReader = -1;

        // the timezone information
        if (column != null)
        {
          var tz = column.TimeZonePart;
          if (!string.IsNullOrEmpty(tz))
            if (!tz.TryGetConstant(out constantTimeZone))
              if (colNames.TryGetByValue(tz, out var ordinal))
                columnOrdinalTimeZoneReader = ordinal;
        }

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
              DataType.DateTime,
              column.TimePartFormat,
              timeSeparator: column.ValueFormat?.TimeSeparator ?? ":"),
            column.TimePartFormat.Length,
            constantTimeZone,
            columnOrdinalTimeZoneReader));
      }

      return result;
    }

    public async Task<long> WriteAsync(IFileReader? reader, CancellationToken token)
    {
      if (reader is null)
        return -1;
      HandleWriteStart();
      m_SetMaxProcess?.Invoke(-1);

      try
      {
        var sourceAccess = new SourceAccess(
          m_FullPath,
          false,
          recipient: m_Recipient,
          keepEncrypted: m_KeepUnencrypted);
        if (!string.IsNullOrEmpty(m_IdentifierInContainer))
          sourceAccess.IdentifierInContainer = m_IdentifierInContainer;
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
        await
#endif
          using var improvedStream = (Stream) FunctionalDI.OpenStream(sourceAccess);

        await WriteReaderAsync(reader, improvedStream, token).ConfigureAwait(false);
      }
      catch (Exception exc)
      {
        Logger.Error(exc, "Could not write file {filename}", FileSystemUtils.GetShortDisplayFileName(m_FullPath));
        throw new FileWriterException(
          $"Could not write file '{FileSystemUtils.GetShortDisplayFileName(m_FullPath)}'\n{exc.SourceExceptionMessage()}",
          exc);
      }
      finally
      {
        Logger.Debug("Finished writing {filesetting} Records: {records}", m_FileSettingDisplay, Records);
        WriteFinished?.Invoke(this, EventArgs.Empty);
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

    protected void HandleProgress(string text) => m_ReportProgress?.Invoke(text);

    /// <summary>
    ///   Handles the time zone for a date time column
    /// </summary>
    /// <param name="dataObject">The data object.</param>
    /// <param name="columnInfo">The column information.</param>
    /// <param name="reader">The reader.</param>
    /// <returns></returns>
    protected DateTime HandleTimeZone(DateTime dataObject, WriterColumn columnInfo, IDataRecord reader)
    {
      if (columnInfo is null)
        throw new ArgumentNullException(nameof(columnInfo));
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (columnInfo.ColumnOrdinalTimeZone > -1)
      {
        var destinationTimeZoneId = reader.GetString(columnInfo.ColumnOrdinalTimeZone);
        if (string.IsNullOrEmpty(destinationTimeZoneId))
          HandleWarning(columnInfo.Name, "Time zone is empty, value not converted");
        else
          // ReSharper disable once PossibleInvalidOperationException
          return FunctionalDI.AdjustTZExport(
            dataObject,
            destinationTimeZoneId,
            Columns.IndexOf(columnInfo),
            (columnNo, msg) => HandleWarning(Columns[columnNo].Name, msg));
      }
      else if (!string.IsNullOrEmpty(columnInfo.ConstantTimeZone))
      {
        // ReSharper disable once PossibleInvalidOperationException
        return FunctionalDI.AdjustTZExport(
          dataObject,
          columnInfo.ConstantTimeZone,
          Columns.IndexOf(columnInfo),
          (columnNo, msg) => HandleWarning(Columns[columnNo].Name, msg));
      }

      return dataObject;
    }

    protected void HandleWriteStart() => Records = 0;

    protected void NextRecord()
    {
      Records++;
      if (!((DateTime.Now - m_LastNotification).TotalSeconds > .15)) return;
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
            ValueFormatGeneral,
            ColumnDefinition,
            dt ?? throw new ArgumentException("GetSchemaTable did not return information for reader"))
          .Cast<WriterColumn>());
    }


    protected abstract Task WriteReaderAsync(IFileReader reader, Stream output, CancellationToken cancellationToken);

    protected static string
      ReplacePlaceHolder(string input, string fileName, string id) =>
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
        if (dataObject is null || dataObject is DBNull)
          displayAs = columnInfo.ValueFormat.DisplayNullAs;
        else
          switch (columnInfo.ValueFormat.DataType)
          {
            case DataType.Integer:
              displayAs = Convert.ToInt64(dataObject)
                                 .ToString(columnInfo.ValueFormat.NumberFormat, CultureInfo.InvariantCulture).Replace(
                                   CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator,
                                   columnInfo.ValueFormat.GroupSeparator);
              break;

            case DataType.Boolean:
              displayAs = (bool) dataObject ? columnInfo.ValueFormat.True : columnInfo.ValueFormat.False;
              break;

            case DataType.Double:
              displayAs = StringConversion.DoubleToString(
                dataObject is double d
                  ? d
                  : Convert.ToDouble(Convert.ToString(dataObject), CultureInfo.InvariantCulture),
                columnInfo.ValueFormat);
              break;

            case DataType.Numeric:
              displayAs = StringConversion.DecimalToString(
                dataObject is decimal @decimal
                  ? @decimal
                  : Convert.ToDecimal(Convert.ToString(dataObject), CultureInfo.InvariantCulture),
                columnInfo.ValueFormat);
              break;

            case DataType.DateTime:
              displayAs = reader is null
                            ? StringConversion.DateTimeToString((DateTime) dataObject, columnInfo.ValueFormat)
                            : StringConversion.DateTimeToString(HandleTimeZone((DateTime) dataObject, columnInfo, reader),columnInfo.ValueFormat);
              break;

            case DataType.Guid:              
              displayAs = ((Guid) dataObject).ToString();
              break;

            case DataType.String:
              displayAs = Convert.ToString(dataObject) ?? string.Empty;
              break;

            default:
              displayAs = string.Empty;
              break;
          }
      }
      catch (Exception ex)
      {
        // In case a cast did fail (eg.g trying to format as integer and providing a text, use the
        // original value
        displayAs = Convert.ToString(dataObject) ?? string.Empty;
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