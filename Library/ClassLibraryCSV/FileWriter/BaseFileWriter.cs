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
#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
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
    protected readonly IReadOnlyCollection<Column> ColumnDefinition;
    protected readonly string FileSettingDisplay;
    private readonly string m_Footer;
    internal readonly string FullPath;
    private readonly string m_IdentifierInContainer;
#if SupportPGP
    private readonly bool m_KeepUnencrypted;
    private readonly string m_PublicKey;
#endif

    protected readonly ValueFormat ValueFormatGeneral;
    protected readonly TimeZoneChangeDelegate TimeZoneAdjust;
    protected string Header;
    protected readonly string SourceTimeZone;
    private DateTime m_LastNotification = DateTime.Now;

    public IProgress<ProgressInfo>? ReportProgress
    {
      protected get;
      set;
    }

    /// <summary>
    /// Abstract implementation of all FileWriters
    /// </summary>
    /// <param name="id">Information for  Placeholder of ID</param>
    /// <param name="fullPath">Fully qualified path of teh file to write</param>
    /// <param name="valueFormatGeneral">Fallback value format for typed values that do not have a column setup</param>
    /// <param name="identifierInContainer">In case the file is written into an archive that does support multiple files, name of teh file in the archive.</param>
    /// <param name="footer">Footer to be written after all rows are written</param>
    /// <param name="header">Header to be written before data and/or Header is written</param>
    /// <param name="columnDefinition">Individual column definitions for formatting</param>
    /// <param name="fileSettingDisplay">Info text for logging and process report</param>
    /// <param name="timeZoneAdjust">Delegate for TimeZone Conversions</param>
    /// <param name="sourceTimeZone">Identified for the timezone teh values are currently stored as</param>
    /// <param name="publicKey">Key used for encryption of the written data (not implemented in all Libraries)</param>
    /// <param name="unencrypted">If <c>true</c> teh not pgp encrypted file is kept for reference</param>
    /// <exception cref="ArgumentException"></exception>
    protected BaseFileWriter(
      in string id,
      in string fullPath,
      in ValueFormat? valueFormatGeneral,
      in string? identifierInContainer,
      in string? footer,
      in string? header,
      in IEnumerable<Column>? columnDefinition,
      in string fileSettingDisplay,
      in TimeZoneChangeDelegate timeZoneAdjust,
      in string sourceTimeZone,
      in string publicKey,
      bool unencrypted
      )
    {
      SourceTimeZone = sourceTimeZone;
      TimeZoneAdjust = timeZoneAdjust;
#if SupportPGP
      m_PublicKey = publicKey;
      m_KeepUnencrypted = unencrypted;
#endif
      FullPath = fullPath;
      var fileName = FileSystemUtils.GetFileName(FullPath);
      Header = ReplacePlaceHolder(header, fileName, id);
      m_Footer = ReplacePlaceHolder(footer, fileName, id);

      ValueFormatGeneral = valueFormatGeneral ?? ValueFormat.Empty;
      ColumnDefinition =  columnDefinition == null ? new List<Column>() : new List<Column>(columnDefinition);
      FileSettingDisplay = fileSettingDisplay;

      m_IdentifierInContainer = identifierInContainer ?? string.Empty;
    }


    protected void HandleShowProgressPeriodic(string text, long value)
      => ReportProgress?.Report(new ProgressInfo(text, value));

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
    /// <param name="reader"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">reader</exception>
    public static IReadOnlyCollection<WriterColumn> GetColumnInformation(in ValueFormat generalFormat,
      in IReadOnlyCollection<Column> columnDefinitions,
      in IFileReader reader)
    {
      var result = new List<WriterColumn>();

      // Make names unique 
      var colNames = new BiDirectionalDictionary<int, string>();
      var columns = reader.GetColumnsOfReader().ToList();

      foreach (var col in columns)
      {
        var colName = col.Name;
        if (string.IsNullOrEmpty(colName.Trim()))
          colName = $"Column{col.ColumnOrdinal + 1}";
        colNames.Add(col.ColumnOrdinal, colNames.Values.MakeUniqueInCollection(colName));
      }

      foreach (var col in columns)
      {
        var colNo = col.ColumnOrdinal;
        var column = columnDefinitions.FirstOrDefault(x => x.Name.Equals(colNames[colNo], StringComparison.OrdinalIgnoreCase));
        var writeFolder = FileSystemUtils.GetAbsolutePath(
                  string.IsNullOrEmpty(column?.ValueFormat.WriteFolder) ? generalFormat.WriteFolder : column?.ValueFormat.WriteFolder,
                  string.Empty);

        var valueFormat = column?.ValueFormat is null
          ? new ValueFormat(
            col.ValueFormat.DataType,
            generalFormat.DateFormat,
            generalFormat.DateSeparator.ToStringHandle0(),
            generalFormat.TimeSeparator.ToStringHandle0(),
            generalFormat.NumberFormat,
            generalFormat.GroupSeparator.ToStringHandle0(),
            generalFormat.DecimalSeparator.ToStringHandle0(),
            generalFormat.True,
            generalFormat.False,
            generalFormat.DisplayNullAs,
            readFolder: generalFormat.ReadFolder,
            writeFolder: writeFolder,
            fileOutPutPlaceholder: generalFormat.FileOutPutPlaceholder,
            overwrite: generalFormat.Overwrite)
          : new ValueFormat(
            column.ValueFormat.DataType,
            column.ValueFormat.DateFormat,
            column.ValueFormat.DateSeparator.ToStringHandle0(),
            column.ValueFormat.TimeSeparator.ToStringHandle0(),
            column.ValueFormat.NumberFormat,
            column.ValueFormat.GroupSeparator.ToStringHandle0(),
            column.ValueFormat.DecimalSeparator.ToStringHandle0(),
            column.ValueFormat.True,
            column.ValueFormat.False,
            column.ValueFormat.DisplayNullAs,
            readFolder: string.Empty, // No need for a read folder
            writeFolder: writeFolder,
            fileOutPutPlaceholder: string.IsNullOrEmpty(column.ValueFormat.FileOutPutPlaceholder)
              ? generalFormat.FileOutPutPlaceholder
              : column.ValueFormat.FileOutPutPlaceholder,
            overwrite: column.ValueFormat.Overwrite);

        var fieldLength = 0;
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
        var ci = new WriterColumn(colNames[colNo], valueFormat, colNo, fieldLength, constantTimeZone, columnOrdinalTimeZoneReader);

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
            new ValueFormat(
              DataTypeEnum.DateTime,
              column.TimePartFormat,
              timeSeparator: column.ValueFormat?.TimeSeparator.ToStringHandle0()),
            colNo,
            column.TimePartFormat.Length,
            constantTimeZone, columnOrdinalTimeZoneReader));
      }

      return result;
    }

    /// <inheritdoc cref="IFileWriter" />
    public virtual async Task<long> WriteAsync(IFileReader? reader, CancellationToken token)
    {
      if (reader is null)
        return -1;
      HandleWriteStart();

      try
      {

#if SupportPGP
        var sourceAccess = new SourceAccess(FullPath, false, keepEncrypted: m_KeepUnencrypted, publicKey: m_PublicKey);

#else
        var sourceAccess = new SourceAccess(FullPath, false);
#endif
        if (!string.IsNullOrEmpty(m_IdentifierInContainer))
          sourceAccess.IdentifierInContainer = m_IdentifierInContainer;
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        await
#endif
        using var stream = new ImprovedStream(sourceAccess);
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
      m_Footer.PlaceholderReplace("Records", string.Format(new CultureInfo("en-US"), "{0:N0}", Records));

    /// <summary>
    ///   Handles the error.
    /// </summary>
    /// <param name="columnName">The column name.</param>
    /// <param name="message">The message.</param>
    protected void HandleError(string columnName, string message) =>
      Warning?.Invoke(this, new WarningEventArgs(Records, 0, message, 0, 0, columnName));

    protected void HandleProgress(string text) => Logger.Information(text);

    protected virtual void HandleWriteStart() => Records = 0;

    protected virtual void HandleWriteEnd() => WriteFinished?.Invoke(this, EventArgs.Empty);

    protected void NextRecord()
    {
      Records++;
      if ((DateTime.Now - m_LastNotification).TotalSeconds <= .15) return;
      m_LastNotification = DateTime.Now;
      HandleProgress($"Record {Records:N0}");
    }

    /// <inheritdoc cref="IFileWriter"/>
    public abstract Task WriteReaderAsync(IFileReader reader, Stream output, CancellationToken cancellationToken);

    private static string ReplacePlaceHolder(string? input, string fileName, string id) =>
      input?.PlaceholderReplace("ID", id)
        .PlaceholderReplace("FileName", fileName)
        .PlaceholderReplace("CDate", string.Format(new CultureInfo("en-US"), "{0:dd-MMM-yyyy}", DateTime.Now))
        .PlaceholderReplace("CDateLong", string.Format(new CultureInfo("en-US"), "{0:MMMM dd\\, yyyy}", DateTime.Now)) ?? string.Empty;

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
    /// <param name="dataRecord">
    ///   Data Reader / Data Records in case additional columns are needed e.G. for TimeZone
    ///   adjustment based off ColumnOrdinalTimeZone or GetFileName
    /// </param>
    /// <param name="dataObject">The actual data of the column</param>
    /// <param name="columnInfo">Information on ValueConversion</param>
    /// <param name="timeZoneAdjust">Class that does provide means to convert between timezones</param>
    /// <param name="sourceTimeZone">The assumed source timezone of date time columns</param>
    /// <param name="handleWarning">Method to pass on warnings</param>
    /// <returns>Value of the .Net Data type matching the ValueFormat.DataType</returns>
    public static object ValueConversion(in IDataRecord? dataRecord, in object? dataObject, WriterColumn columnInfo,
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
              msg => handleWarning?.Invoke(columnInfo.Name, msg));

          if (dataRecord is null || columnInfo.ColumnOrdinalTimeZone <= -1)
            return dtm;

          var destinationTimeZoneId = dataRecord.GetString(columnInfo.ColumnOrdinalTimeZone);
          if (string.IsNullOrEmpty(destinationTimeZoneId))
          {
            handleWarning?.Invoke(columnInfo.Name, "Time zone is empty, value not converted");
            return dtm;
          }

          return timeZoneAdjust(dtm, sourceTimeZone,
            dataRecord.GetString(columnInfo.ColumnOrdinalTimeZone), msg => handleWarning?.Invoke(columnInfo.Name, msg));

        case DataTypeEnum.Guid:
          return dataObject is Guid guid ? guid : new Guid(dataObject.ToString() ?? string.Empty);

        default:

          if (columnInfo.ColumnFormatter != null)
            return columnInfo.ColumnFormatter.Write(dataObject, dataRecord,
              msg => handleWarning?.Invoke(columnInfo.Name, msg));

          // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
          return Convert.ToString(dataObject) ?? string.Empty;
      }
    }

    protected string TextEncodeField(object? dataObject, in WriterColumn columnInfo, in IDataRecord? dataRecord)
    {
      if (columnInfo is null)
        throw new ArgumentNullException(nameof(columnInfo));

      string displayAs;
      try
      {
        var convertedValue = ValueConversion(dataRecord, dataObject, columnInfo, TimeZoneAdjust, SourceTimeZone, HandleWarning);
        if (convertedValue == DBNull.Value)
          displayAs = columnInfo.ValueFormat.DisplayNullAs;
        else
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
          displayAs = convertedValue switch
          {
            long aLong => aLong.ToString(columnInfo.ValueFormat.NumberFormat, CultureInfo.InvariantCulture).Replace(
              ',',
              columnInfo.ValueFormat.GroupSeparator),
            bool aBol => aBol ? columnInfo.ValueFormat.True : columnInfo.ValueFormat.False,
            double aDbl => StringConversion.DoubleToString(aDbl, columnInfo.ValueFormat),
            decimal aDec => StringConversion.DecimalToString(aDec, columnInfo.ValueFormat),
            DateTime aDTm => aDTm.DateTimeToString(columnInfo.ValueFormat),
            _ => convertedValue.ToString()
          };
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
      }
      catch (Exception ex)
      {
        // In case a cast did fail (eg.g trying to format as integer and providing a text, use the
        // original value
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
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

      // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
      return displayAs ?? string.Empty;
    }
  }
}