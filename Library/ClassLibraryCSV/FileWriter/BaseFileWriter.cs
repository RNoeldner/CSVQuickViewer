/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using System.Threading.Tasks;

namespace CsvTools;

/// <summary>
/// Provides a base implementation for file writers, handling shared logic for 
/// column formatting, data type conversion, and time zone adjustments.
/// </summary>
public abstract class BaseFileWriter : IFileWriter
{
  /// <summary>
  /// The collection of column metadata defining the schema, data types, 
  /// and formatting rules for the output file.
  /// </summary>
  protected readonly IReadOnlyCollection<Column> ColumnDefinition;

  /// <summary>
  /// Display text for the writer used in logs and progress reporting.
  /// </summary>
  protected readonly string FileSettingDisplay;

  private readonly string m_Footer;

  /// <summary>
  /// Fully qualified file path to write.
  /// </summary>
  private readonly string FullPath;
  private readonly string m_IdentifierInContainer;
  private readonly bool m_KeepUnencrypted;
  private readonly string m_PublicKey;

  /// <summary>The general value format in case no special value format is defined for a column</summary>
  protected readonly ValueFormat ValueFormatGeneral;


  /// <summary>
  /// Delegate for converting DateTime values between time zones.
  /// </summary>
  protected readonly TimeZoneChangeDelegate TimeZoneAdjust;

  /// <summary>
  /// Delegate used to create or provide the target <see cref="Stream"/>
  /// for writing output.
  /// </summary>
  protected readonly StreamProviderDelegate m_StreamProvider;

  /// <summary>The header written before the records are stored</summary>
  protected string Header;

  /// <summary>
  /// The source time zone of the data. Used in conjunction with 
  /// <see cref="TimeZoneAdjust"/> to normalize date/time values.
  /// </summary>
  protected readonly string SourceTimeZone;

  /// <summary>
  /// Initializes a new instance of the <see cref="BaseFileWriter"/> class.
  /// </summary>
  /// <param name="fullPath">The fully qualified path of the file to write.</param>
  /// <param name="valueFormatGeneral">The fallback value format for columns without specific formatting.</param>
  /// <param name="identifierInContainer">The name of the file within an archive (e.g., ZIP).</param>
  /// <param name="footer">The template for the footer written after data records.</param>
  /// <param name="header">The template for the header written before data records.</param>
  /// <param name="columnDefinition">The collection of individual column definitions for formatting.</param>
  /// <param name="fileSettingDisplay">The descriptive text used for logging and progress reporting.</param>
  /// <param name="sourceTimeZone">The ID of the time zone in which the source values are stored.</param>
  /// <param name="publicKey">The key used for encrypting the output data.</param>
  /// <param name="unencrypted">If <c>true</c>, keeps the unencrypted file as a reference when encryption is used.</param>
  protected BaseFileWriter(
    string fullPath,
    in ValueFormat? valueFormatGeneral,
    string? identifierInContainer,
    string? footer,
    string? header,
    in IEnumerable<Column>? columnDefinition,
    string fileSettingDisplay,
    string sourceTimeZone,
    string publicKey,
    bool unencrypted
  )
  {
    SourceTimeZone = sourceTimeZone;
    TimeZoneAdjust = FunctionalDI.GetTimeZoneAdjust;
    m_StreamProvider = FunctionalDI.GetStream;
    m_PublicKey = publicKey;
    m_KeepUnencrypted = unencrypted;
    FullPath = fullPath;
    var fileName = FileSystemUtils.GetFileName(FullPath);
    Header = ReplacePlaceHolder(header, fileName);
    m_Footer = ReplacePlaceHolder(footer, fileName);

    ValueFormatGeneral = valueFormatGeneral ?? ValueFormat.Empty;
    ColumnDefinition = columnDefinition is null ? new List<Column>() : new List<Column>(columnDefinition);
    FileSettingDisplay = fileSettingDisplay;

    m_IdentifierInContainer = identifierInContainer ?? string.Empty;
  }

  /// <summary>
  /// Gets the number of records successfully written to the file.
  /// </summary>
  public long Records { get; protected set; }

  /// <summary>
  /// Occurs when a non-critical error or warning is encountered during the write process.
  /// </summary>
  public event EventHandler<WarningEventArgs>? Warning;

  /// <summary>
  /// Occurs when the file writing operation has successfully concluded.
  /// </summary>
  public event EventHandler? WriteFinished;

  private static readonly char[] timeIdentifiers = new[] { 'h', 'H', 'm', 's' };

  /// <summary>
  /// Retrieves column metadata by merging source reader information with explicit column definitions.
  /// </summary>
  /// <param name="generalFormat">The default format for columns not explicitly defined.</param>
  /// <param name="columnDefinitions">The set of predefined column configurations.</param>
  /// <param name="reader">The data reader providing the source schema.</param>
  /// <returns>A collection of <see cref="WriterColumn"/> objects defining the output structure.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="reader"/> is null.</exception>
  public static IReadOnlyCollection<WriterColumn> GetColumnInformation(in ValueFormat generalFormat,
    in IReadOnlyCollection<Column> columnDefinitions,
    in IDataReader reader)
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
      var column =
        columnDefinitions.FirstOrDefault(x => x.Name.Equals(colNames[colNo], StringComparison.OrdinalIgnoreCase));
      var writeFolder =
        (string.IsNullOrEmpty(column?.ValueFormat.WriteFolder)
          ? generalFormat.WriteFolder
          : column?.ValueFormat.WriteFolder).GetAbsolutePath(string.Empty);

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
      var ci = new WriterColumn(colNames[colNo], valueFormat, colNo, fieldLength, constantTimeZone,
        columnOrdinalTimeZoneReader);

      result.Add(ci);

      // add an extra column for the time, reading columns they get combined, writing them they
      // get separated again

      if (column is null || string.IsNullOrEmpty(column.TimePart) || colNames.ContainsValue(column.TimePart))
        continue;

      if (ci.ValueFormat.DateFormat.IndexOfAny(timeIdentifiers) != -1)
        Logger.Warning($"'{ci.Name}' will create a separate time column '{column.TimePart}' but seems to write time itself '{ci.ValueFormat.DateFormat}'");

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
  public virtual async Task<long> WriteAsync(IFileReader? source, IProgressWithCancellation progress)
  {
    if (source is null)
      return -1;
    HandleWriteStart();

    try
    {
      var sourceAccess = new SourceAccess(FullPath, false, keepEncrypted: m_KeepUnencrypted, pgpKey: m_PublicKey);
      if (!string.IsNullOrEmpty(m_IdentifierInContainer))
        sourceAccess.IdentifierInContainer = m_IdentifierInContainer;

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      var stream = FunctionalDI.GetStream(sourceAccess);
      await using (stream.ConfigureAwait(false))
#else
      using var stream = FunctionalDI.GetStream(sourceAccess);
#endif
      await WriteReaderAsync(source, stream, progress).ConfigureAwait(false);
    }
    catch (Exception exc)
    {
      Logger.Error(exc, "Could not write file {filename}", FullPath.GetShortDisplayFileName());

      throw new FileWriterException(
        $"Could not write file '{FullPath.GetShortDisplayFileName()}'\n{exc.SourceExceptionMessage()}",
        exc);
    }
    finally
    {
      Logger.Debug("Finished writing {filesetting} Records: {records}", FileSettingDisplay, Records);


      HandleWriteEnd();
    }

    return Records;
  }
  /// <summary>
  /// Generates the footer string, replacing placeholders such as {Records}, {FileName}, and {CDate}.
  /// </summary>
  /// <returns>The formatted footer string.</returns>
  protected string Footer() =>
    m_Footer.PlaceholderReplace("Records", string.Format(new CultureInfo("en-US"), "{0:N0}", Records));

  /// <summary>
  /// Raises a warning event for a specific column.
  /// </summary>
  /// <param name="columnName">The name of the column where the error occurred.</param>
  /// <param name="message">The error message.</param>
  protected void HandleError(string columnName, string message) =>
    Warning?.SafeInvoke(this, new WarningEventArgs(Records, 0, message, 0, 0, columnName));

  /// <summary>
  /// Resets record counters and prepares the writer for a new operation.
  /// </summary>
  protected void HandleWriteStart() => Records = 0;

  /// <summary>
  /// Finalizes the writing process and invokes the <see cref="WriteFinished"/> event.
  /// </summary>
  protected void HandleWriteEnd() => WriteFinished?.SafeInvoke(this);

  /// <inheritdoc cref="IFileWriter"/>
  public abstract Task WriteReaderAsync(IFileReader reader, Stream output, IProgressWithCancellation progress);

  /// <summary>
  /// Replaces standardized placeholders in a string with current file and date information.
  /// </summary>
  private static string ReplacePlaceHolder(string? input, string fileName) =>
    input?.PlaceholderReplace("FileName", fileName)
      .PlaceholderReplace("CDate", string.Format(new CultureInfo("en-US"), "{0:dd-MMM-yyyy}", DateTime.Now))
      .PlaceholderReplace("CDateLong",
        string.Format(new CultureInfo("en-US"), "{0:MMMM dd\\, yyyy}", DateTime.Now)) ?? string.Empty;

  /// <summary>
  /// Invokes the warning event handler with formatted metadata.
  /// </summary>
  /// <param name="columnName">The name of the column associated with the warning.</param>
  /// <param name="message">The warning message.</param>
  protected void HandleWarning(string columnName, string message) =>
    Warning?.SafeInvoke(this, new WarningEventArgs(Records, 0, message.AddWarningId(), 0, 0, columnName));

  /// <summary>
  /// Converts a raw database value into the target .NET type defined by the column configuration.
  /// </summary>
  /// <param name="dataObject">The raw value retrieved from the data source.</param>
  /// <param name="columnInfo">Column metadata describing the target data type and formatting behavior.</param>
  /// <param name="dataRecord">Optional record used for dynamic column access (e.g., time zone lookups).</param>
  /// <param name="timeZoneAdjust">Delegate used to convert DateTime values between time zones.</param>
  /// <param name="sourceTimeZone">The assumed source time zone for DateTime values.</param>
  /// <param name="handleWarning">Optional callback used to report conversion warnings.</param>
  /// <returns>
  /// A value compatible with the configured <see cref="DataTypeEnum"/>.  
  /// Possible return types include <see cref="DBNull"/>, <see cref="long"/>, 
  /// <see cref="bool"/>, <see cref="double"/>, <see cref="decimal"/>, 
  /// <see cref="DateTime"/>, <see cref="Guid"/>, or <see cref="string"/>.
  /// </returns>
  public static object ValueConversion(in object? dataObject, WriterColumn columnInfo, in IDataRecord? dataRecord,
    TimeZoneChangeDelegate timeZoneAdjust, string sourceTimeZone, Action<string, string>? handleWarning = null)
  {
    if (dataObject is null or DBNull)
      return DBNull.Value;
    var culture = CultureInfo.InvariantCulture;
    switch (columnInfo.ValueFormat.DataType)
    {
      case DataTypeEnum.Integer:
        return Convert.ToInt64(dataObject, culture);

      case DataTypeEnum.Boolean:
        return Convert.ToBoolean(dataObject, culture);

      case DataTypeEnum.Double:
        return Convert.ToDouble(dataObject, culture);

      case DataTypeEnum.Numeric:
        return Convert.ToDecimal(dataObject, culture);

      case DataTypeEnum.DateTime:
        var dtm = Convert.ToDateTime(dataObject, culture);
        if (columnInfo.OutputTimeZone.Length > 0)
        {
          return timeZoneAdjust(dtm, sourceTimeZone, columnInfo.OutputTimeZone,
            msg => handleWarning?.Invoke(columnInfo.Name, msg));
        }

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
        return columnInfo.ColumnFormatter.Write(dataObject, dataRecord,
          msg => handleWarning?.Invoke(columnInfo.Name, msg));
    }
  }

  /// <summary>
  /// Converts a typed object into its string representation based on column formatting rules.
  /// </summary>
  /// <param name="dataObject">The object to encode.</param>
  /// <param name="columnInfo">The formatting and type metadata for the column.</param>
  /// <param name="dataRecord">The source data record for contextual formatting.</param>
  /// <returns>A formatted string ready for file output.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="columnInfo"/> is null.</exception>
  protected string TextEncodeField(object? dataObject, in WriterColumn columnInfo, in IDataRecord? dataRecord)
  {
    if (columnInfo is null)
      throw new ArgumentNullException(nameof(columnInfo));

    string displayAs;
    try
    {
      var convertedValue = ValueConversion(dataObject, columnInfo, dataRecord, TimeZoneAdjust, SourceTimeZone,
        HandleWarning);
      if (convertedValue == DBNull.Value)
      {
        displayAs = columnInfo.ValueFormat.DisplayNullAs;
      }
      else
      {
        displayAs = convertedValue switch
        {
          long aLong => StringConversion.LongToString(aLong, columnInfo.ValueFormat),
          bool aBol => aBol ? columnInfo.ValueFormat.True : columnInfo.ValueFormat.False,
          double aDbl => StringConversion.DoubleToString(aDbl, columnInfo.ValueFormat),
          decimal aDec => StringConversion.DecimalToString(aDec, columnInfo.ValueFormat),
          DateTime aDTm => aDTm.DateTimeToString(columnInfo.ValueFormat),
          _ => convertedValue.ToString() ?? string.Empty
        };
      }
    }
    catch (Exception ex)
    {
      // In case a cast did fail (e.g. trying to format as integer and providing a text, use the
      // original value
      // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
      displayAs = Convert.ToString(dataObject) ?? string.Empty;
      if (string.IsNullOrEmpty(displayAs))
        HandleError(columnInfo.Name, ex.Message);
      else
        HandleWarning(columnInfo.Name, $"Value stored as: {displayAs}\nExpected {columnInfo.ValueFormat.DataType} but was {dataObject?.GetType()} {ex.Message}");
    }

    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
    return displayAs ?? string.Empty;
  }
}