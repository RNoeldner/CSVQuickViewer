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
    protected class WriterColumn : ImmutableColumn
    {
      public WriterColumn(string name, int colNum, IValueFormat valueFormat, int fieldLength = 0, string constantTimeZone = "", int columnOrdinalTimeZone = -1) : base(name, valueFormat, colNum)
      {
        FieldLength = fieldLength;
        ConstantTimeZone = constantTimeZone;
        ColumnOrdinalTimeZone = columnOrdinalTimeZone;
      }

      /// <summary>
      ///   Gets the column ordinal of the time zone column
      /// </summary>
      /// <value>The column ordinal time zone.</value>
      public int ColumnOrdinalTimeZone { get; }

      /// <summary>
      ///   Gets the constant time zone
      /// </summary>
      /// <value>The constant time zone.</value>

      public string ConstantTimeZone { get; }

      /// <summary>
      ///   Gets or sets the length of the field.
      /// </summary>
      /// <value>The length of the field. 0 means unrestricted length</value>
      public int FieldLength { get; }
    }

    protected readonly IReadOnlyCollection<ImmutableColumn> ColumnDefinition;
    
    protected readonly List<WriterColumn> Columns = new List<WriterColumn>();
    protected readonly IFileFormat FileFormat;
    protected readonly string Header;
    private readonly string m_FileSettingDisplay;
    private readonly string m_Footer;
    private readonly string m_FullPath;
    private readonly string m_Recipient;
    private readonly bool m_KeepUnencrypted;
    private readonly string m_IdentifierInContainer;
    private readonly Action<string>? m_ReportProgress;
    private readonly Action<long>? m_SetMaxProcess;
    protected readonly string NewLine;
    protected readonly IValueFormat ValueFormatGeneral;
    private DateTime m_LastNotification = DateTime.Now;

    protected BaseFileWriter(string id, string fullPath, IValueFormat? valueFormatGeneral = null,
      IFileFormat? fileFormat = null, string? recipient = null,
      bool unencrypted = false, string? identifierInContainer = null, string? footer = null, string? header = null,
      IEnumerable<IColumn>? columnDefinition = null, string fileSettingDisplay = "",
      IProcessDisplay? processDisplay = null)
    {
      m_FullPath = fullPath;
      var fileName = FileSystemUtils.GetFileName(fullPath);
    
      if (valueFormatGeneral!=null)
        ValueFormatGeneral = new ImmutableValueFormat(valueFormatGeneral.DataType, valueFormatGeneral.DateFormat, valueFormatGeneral.DateSeparator,
            valueFormatGeneral.TimeSeparator, valueFormatGeneral.NumberFormat, valueFormatGeneral.GroupSeparator, valueFormatGeneral.DecimalSeparator, valueFormatGeneral.True,
            valueFormatGeneral.False, valueFormatGeneral.DisplayNullAs);
      else
        ValueFormatGeneral =new ImmutableValueFormat();

      if (fileFormat != null)
        FileFormat = new ImmutableFileFormat(fileFormat.IsFixedLength, fileFormat.QualifyAlways,
          fileFormat.QualifyOnlyIfNeeded, fileFormat.EscapeChar, fileFormat.FieldDelimiterChar, fileFormat.DelimiterPlaceholder,
          fileFormat.FieldQualifierChar, fileFormat.QuotePlaceholder, fileFormat.NewLine, fileFormat.NewLinePlaceholder);
      else
        FileFormat = new ImmutableFileFormat();

      ColumnDefinition =  columnDefinition?.Select(col => col is ImmutableColumn immutableColumn ? immutableColumn : new ImmutableColumn(col)).ToList() ??
                           new List<ImmutableColumn>();
      NewLine = fileFormat?.NewLine.NewLineString() ?? "\r\n";
      if (!string.IsNullOrEmpty(header))
        Header = ReplacePlaceHolder(header!.HandleCRLFCombinations(NewLine), fileFormat?.FieldDelimiterChar.GetDescription() ?? string.Empty,
        fileName, id);
      else
        Header = string.Empty;

      if (!string.IsNullOrEmpty(footer))
        m_Footer = ReplacePlaceHolder(footer!.HandleCRLFCombinations(NewLine), fileFormat?.FieldDelimiterChar.GetDescription() ?? string.Empty,
          fileName, id);
      else
        m_Footer = string.Empty;

      m_FileSettingDisplay = fileSettingDisplay;
      m_Recipient = recipient?? string.Empty;
      m_KeepUnencrypted = unencrypted;
      m_IdentifierInContainer = identifierInContainer?? string.Empty;

      Logger.Debug("Created Writer for {filesetting}", m_FileSettingDisplay);
      if (processDisplay == null) return;
      m_ReportProgress = t => processDisplay.SetProcess(t, 0, true);
      if (!(processDisplay is IProcessDisplayTime processDisplayTime)) return;
      processDisplayTime.Maximum = 0;
      m_SetMaxProcess = l => processDisplayTime.Maximum = l;
    }

    /// <summary>
    ///   Sets the columns by looking at the reader
    /// </summary>
    /// <param name="reader">The reader.</param>
    protected void SetColumns(IFileReader reader)
    {
      Columns.Clear();
      using var dt = reader.GetSchemaTable();
      Columns.AddRange(GetColumnInformation(ValueFormatGeneral, ColumnDefinition, dt ?? throw new ArgumentException("GetSchemaTable did not return information for reader")).Cast<WriterColumn>());
    }

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
    public static IEnumerable<IColumn> GetColumnInformation(IValueFormat generalFormat,
      IReadOnlyCollection<IColumn> columnDefinitions, DataTable schemaTable)
    {
      if (schemaTable == null)
        throw new ArgumentNullException(nameof(schemaTable));
      var result = new List<WriterColumn>();

      var colNames = new BiDirectionalDictionary<int, string>();

      // Make names unique and fill the dictionary
      foreach (DataRow schemaRow in schemaTable.Rows)
      {
        var colNo = (int) schemaRow[SchemaTableColumn.ColumnOrdinal];
        var colName = schemaRow[SchemaTableColumn.ColumnName] as string;
        if (string.IsNullOrEmpty(colName))
          continue;
        var newName = StringUtils.MakeUniqueInCollection(colNames.Values, colName!);
        colNames.Add(colNo, newName);
      }

      foreach (DataRow schemaRow in schemaTable.Rows)
      {
        var colNo = (int) schemaRow[SchemaTableColumn.ColumnOrdinal];
        var column = columnDefinitions.FirstOrDefault(x => x.Name.Equals(colNames[colNo], StringComparison.OrdinalIgnoreCase));

        if (column is { Ignore: true })
          continue;

        // Based on the data Type in the reader defined and the general format create the value format
        var valueFormat = column?.ValueFormat ?? new ImmutableValueFormat(
          ((Type) schemaRow[SchemaTableColumn.DataType]).GetDataType(), generalFormat.DateFormat,
          generalFormat.DateSeparator,
          generalFormat.TimeSeparator, generalFormat.NumberFormat, generalFormat.GroupSeparator,
          generalFormat.DecimalSeparator, generalFormat.True,
          generalFormat.False, generalFormat.DisplayNullAs);

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
          case DataType.TextToHtml:
          case DataType.TextToHtmlFull:
          case DataType.TextPart:
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
          {
            if (!tz.TryGetConstant(out constantTimeZone))
            {
              if (colNames.TryGetByValue(tz, out var ordinal))
                columnOrdinalTimeZoneReader = ordinal;
            }
          }
        }
        var ci = new WriterColumn(colNames[colNo], colNo, valueFormat, fieldLength, constantTimeZone, columnOrdinalTimeZoneReader);
        result.Add(ci);

        // add an extra column for the time, reading columns they get combined, writing them they
        // get separated again

        if (column == null || string.IsNullOrEmpty(column.TimePart) || colNames.ContainsValue(column.TimePart))
          continue;

        if (ci.ValueFormat.DateFormat.IndexOfAny(new[] { 'h', 'H', 'm', 's' }) != -1)
          Logger.Warning(
            $"'{ci.Name}' will create a separate time column '{column.TimePart}' but seems to write time itself '{ci.ValueFormat.DateFormat}'");

        // In case we have a split column, add the second column (unless the column is also present
        result.Add(new WriterColumn(column.TimePart, colNo, new ImmutableValueFormat(DataType.DateTime, column.TimePartFormat, timeSeparator: column.ValueFormat?.TimeSeparator ?? ":"), column.TimePartFormat.Length, constantTimeZone, columnOrdinalTimeZoneReader));
      }

      return result;
    }
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

    protected string Footer() =>
      m_Footer.PlaceholderReplace("Records", string.Format(new CultureInfo("en-US"), "{0:n0}", Records));

    /// <summary>
    ///   Event handler called if a warning or error occurred
    /// </summary>
    public event EventHandler<WarningEventArgs>? Warning;

    /// <summary>
    ///   Event to be raised if writing is finished
    /// </summary>
    public event EventHandler? WriteFinished;

    public async Task<long> WriteAsync(IFileReader? reader, CancellationToken token)
    {
      if (reader == null)
        return -1;
      HandleWriteStart();
      m_SetMaxProcess?.Invoke(-1);

      try
      {
        var sourceAccess = new SourceAccess(m_FullPath, false, recipient: m_Recipient, keepEncrypted: m_KeepUnencrypted);
        if (!string.IsNullOrEmpty(m_IdentifierInContainer))
          sourceAccess.IdentifierInContainer = m_IdentifierInContainer;

        using var improvedStream = (Stream) FunctionalDI.OpenStream(sourceAccess);
        await WriteReaderAsync(reader, improvedStream, token).ConfigureAwait(false);
      }
      catch (Exception exc)
      {
        Logger.Error(exc, "Could not write file {filename}", FileSystemUtils.GetShortDisplayFileName(m_FullPath));
        throw new FileWriterException($"Could not write file '{FileSystemUtils.GetShortDisplayFileName(m_FullPath)}'\n{exc.SourceExceptionMessage()}",
          exc);
      }
      finally
      {
        Logger.Debug("Finished writing {filesetting} Records: {records}", m_FileSettingDisplay, Records);
        WriteFinished?.Invoke(this, EventArgs.Empty);
      }

      return Records;
    }

    private static string ReplacePlaceHolder(string input, string fieldDelimiter, string fileName,
      string replacement) => input.PlaceholderReplace("ID", replacement)
      .PlaceholderReplace("FileName", fileName)
      .PlaceholderReplace("Delim", fieldDelimiter)
      .PlaceholderReplace("CDate", string.Format(new CultureInfo("en-US"), "{0:dd-MMM-yyyy}", DateTime.Now))
      .PlaceholderReplace("CDateLong", string.Format(new CultureInfo("en-US"), "{0:MMMM dd\\, yyyy}", DateTime.Now));

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
          return FunctionalDI.AdjustTZExport(dataObject, destinationTimeZoneId, Columns.IndexOf(columnInfo), (columnNo, msg) => HandleWarning(Columns[columnNo].Name, msg));
      }
      else if (!string.IsNullOrEmpty(columnInfo.ConstantTimeZone))
      {
        // ReSharper disable once PossibleInvalidOperationException
        return FunctionalDI.AdjustTZExport(dataObject, columnInfo.ConstantTimeZone, Columns.IndexOf(columnInfo), (columnNo, msg) => HandleWarning(Columns[columnNo].Name, msg));
      }

      return dataObject;
    }

    /// <summary>
    ///   Calls the event handler for warnings
    /// </summary>
    /// <param name="columnName">The column.</param>
    /// <param name="message">The message.</param>
    private void HandleWarning(string columnName, string message) => Warning?.Invoke(this,
      new WarningEventArgs(Records, 0, message.AddWarningId(), 0, 0, columnName));

    protected void HandleWriteStart() => Records = 0;

    protected void NextRecord()
    {
      Records++;
      if (!((DateTime.Now - m_LastNotification).TotalSeconds > .15)) return;
      m_LastNotification = DateTime.Now;
      HandleProgress($"Record {Records:N0}");
    }

    /// <summary>
    ///   Encodes the field.
    /// </summary>
    /// <param name="fileFormat">The settings.</param>
    /// <param name="dataObject">The data object.</param>
    /// <param name="columnInfo">Column Information</param>
    /// <param name="isHeader">if set to <c>true</c> the current line is the header.</param>
    /// <param name="reader">The reader.</param>
    /// <param name="handleQualify">The handle qualify.</param>
    /// <returns>proper formatted CSV / Fix Length field</returns>
    /// <exception cref="ArgumentNullException">columnInfo or dataObject</exception>
    /// <exception cref="FileWriterException">
    ///   For fix length output the length of the columns needs to be specified.
    /// </exception>
    protected string TextEncodeField(IFileFormat fileFormat, object? dataObject,
      WriterColumn columnInfo, bool isHeader,
      IDataReader? reader, Func<string, DataType, IFileFormat, string>? handleQualify)
    {
      if (columnInfo is null)
        throw new ArgumentNullException(nameof(columnInfo));

      if (fileFormat.IsFixedLength && columnInfo.FieldLength == 0)
        throw new FileWriterException("For fix length output the length of the columns needs to be specified.");

      string displayAs;
      if (isHeader)
      {
        if (dataObject is null)
          throw new ArgumentNullException(nameof(dataObject));
        displayAs = Convert.ToString(dataObject) ?? string.Empty;
      }
      else
      {
        try
        {
          if (dataObject == null || dataObject is DBNull)
            displayAs = columnInfo.ValueFormat.DisplayNullAs;
          else
            switch (columnInfo.ValueFormat.DataType)
            {
              case DataType.Integer:
                displayAs = Convert.ToInt64(dataObject).ToString(columnInfo.ValueFormat.NumberFormat, CultureInfo.InvariantCulture).Replace(
                    CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator, columnInfo.ValueFormat.GroupSeparator);
                break;

              case DataType.Boolean:
                displayAs = (bool) dataObject
                  ? columnInfo.ValueFormat.True
                  : columnInfo.ValueFormat.False;
                break;

              case DataType.Double:
                displayAs = StringConversion.DoubleToString(
                  dataObject is double d ? d : Convert.ToDouble(Convert.ToString(dataObject), CultureInfo.InvariantCulture),
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
                displayAs = reader == null
                  ? StringConversion.DateTimeToString((DateTime) dataObject, columnInfo.ValueFormat)
                  : StringConversion.DateTimeToString(HandleTimeZone((DateTime) dataObject, columnInfo, reader),
                    columnInfo.ValueFormat);
                break;

              case DataType.Guid:
                // 382c74c3-721d-4f34-80e5-57657b6cbc27
                displayAs = ((Guid) dataObject).ToString();
                break;

              case DataType.String:
              case DataType.TextToHtml:
              case DataType.TextToHtmlFull:
              case DataType.TextPart:
                displayAs = Convert.ToString(dataObject) ?? string.Empty;
                if (columnInfo.ValueFormat.DataType == DataType.TextToHtml)
                  displayAs = HTMLStyle.TextToHtmlEncode(displayAs);

                // a new line of any kind will be replaced with the placeholder if set
                if (fileFormat.NewLinePlaceholder.Length > 0)
                  displayAs = displayAs.HandleCRLFCombinations(fileFormat.NewLinePlaceholder);

                if (fileFormat.DelimiterPlaceholder.Length > 0 &&  fileFormat.FieldDelimiterChar != '\0')
                  displayAs = displayAs.Replace(fileFormat.FieldDelimiterChar.ToStringHandle0(),
                    fileFormat.DelimiterPlaceholder);

                if (fileFormat.QuotePlaceholder.Length > 0 && fileFormat.FieldQualifierChar != '\0')
                  displayAs = displayAs.Replace(fileFormat.FieldQualifierChar.ToStringHandle0(), fileFormat.QuotePlaceholder);
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
            HandleWarning(columnInfo.Name,
              "Value stored as: " + displayAs +
              $"\nExpected {columnInfo.ValueFormat.DataType} but was {dataObject?.GetType()}" + ex.Message);
        }
      }

      // Adjust the output in case its is fixed length
      if (fileFormat.IsFixedLength)
      {
        if (displayAs.Length <= columnInfo.FieldLength || columnInfo.FieldLength <= 0)
          return displayAs.PadRight(columnInfo.FieldLength, ' ');
        HandleWarning(columnInfo.Name,
          $"Text with length of {displayAs.Length} has been cut off after {columnInfo.FieldLength} character");
        return displayAs.Substring(0, columnInfo.FieldLength);
      }

      // Qualify text if required
      if (fileFormat.FieldQualifierChar != '\0' && handleQualify != null)
        return handleQualify(displayAs, columnInfo.ValueFormat.DataType, fileFormat);

      return displayAs;
    }

    protected abstract Task WriteReaderAsync(IFileReader reader, Stream output,
      CancellationToken cancellationToken);
  }
}