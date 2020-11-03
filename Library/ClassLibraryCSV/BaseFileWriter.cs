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

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   A Class to write CSV Files
  /// </summary>
  public abstract class BaseFileWriter
  {
    [NotNull] protected readonly IReadOnlyCollection<IColumn> ColumnDefinition;
    protected readonly bool ColumnHeader;
    [NotNull] protected readonly List<ColumnInfo> Columns = new List<ColumnInfo>();
    [NotNull] protected readonly IFileFormat FileFormat;
    protected readonly string Header;
    private readonly string m_FileSettingDisplay;
    private readonly string m_Footer;
    [NotNull] private readonly string m_FullPath;
    private readonly string m_Recipient;
    private readonly Action<string> m_ReportProgress;
    private readonly Action<long> m_SetMaxProcess;
    protected readonly string NewLine;
    [NotNull] protected readonly IValueFormat ValueFormatGeneral;
    private DateTime m_LastNotification = DateTime.Now;

    protected BaseFileWriter([NotNull] string id,
      [NotNull] string fullPath, string recipient, bool hasFieldHeader,
      [NotNull] string footer,
      [NotNull] string header,
      [NotNull] IValueFormat valueFormat, [NotNull] IFileFormat fileFormat,
      [NotNull] IReadOnlyCollection<IColumn> columns,
      [NotNull] string fileSettingDisplay,
      [CanBeNull] IProcessDisplay processDisplay)
    {
      m_FullPath = fullPath;
      var fileName = FileSystemUtils.GetFileName(fullPath);
      ColumnHeader = hasFieldHeader;
      ValueFormatGeneral = new ImmutableValueFormat(valueFormat);
      FileFormat = new ImmutableFileFormat(fileFormat);
      ColumnDefinition = columns;
      NewLine = fileFormat.NewLine.NewLineString();
      Header = ReplacePlaceHolder(StringUtils.HandleCRLFCombinations(header, NewLine), fileFormat.FieldDelimiterChar,
        fileName, id);
      m_Footer = ReplacePlaceHolder(StringUtils.HandleCRLFCombinations(footer, NewLine), fileFormat.FieldDelimiterChar,
        fileName, id);
      m_FileSettingDisplay = fileSettingDisplay;
      m_Recipient = recipient;

      Logger.Debug("Created Writer for {filesetting}", m_FileSettingDisplay);
      if (processDisplay == null) return;
      m_ReportProgress = t => processDisplay.SetProcess(t, 0, true);
      if (!(processDisplay is IProcessDisplayTime processDisplayTime)) return;
      processDisplayTime.Maximum = 0;
      m_SetMaxProcess = l => processDisplayTime.Maximum = l;
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="BaseFileWriter" /> class.
    /// </summary>
    /// <param name="fileSetting">the file setting with the definition for the file</param>
    /// <param name="processDisplay">The process display.</param>
    /// <exception cref="ArgumentNullException">fileSetting</exception>
    /// <exception cref="ArgumentException">No SQL Reader set</exception>
    protected BaseFileWriter([NotNull] IFileSettingPhysicalFile fileSetting, [CanBeNull] IProcessDisplay processDisplay)
      : this(fileSetting.ID, fileSetting.FullPath, fileSetting.Recipient, fileSetting.HasFieldHeader,
        fileSetting.Footer, fileSetting.Header, fileSetting.FileFormat.ValueFormatMutable, fileSetting.FileFormat,
        fileSetting.ColumnCollection.ReadonlyCopy(), fileSetting.ToString(), processDisplay)
    {
    }

    private long Records { get; set; }

    protected string Footer() =>
      m_Footer.PlaceholderReplace("Records", string.Format(new CultureInfo("en-US"), "{0:n0}", Records));

    /// <summary>
    ///   Event handler called if a warning or error occurred
    /// </summary>
    public virtual event EventHandler<WarningEventArgs> Warning;

    /// <summary>
    ///   Event to be raised if writing is finished
    /// </summary>
    public event EventHandler WriteFinished;

    public async Task<long> WriteAsync([CanBeNull] IFileReader reader, CancellationToken token)
    {
      if (reader == null)
        return -1;
      HandleWriteStart();
      m_SetMaxProcess?.Invoke(-1);

      try
      {
        using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(m_FullPath, false) { Recipient = m_Recipient }))
          await WriteReaderAsync(reader, improvedStream as Stream, token).ConfigureAwait(false);
      }
      catch (Exception exc)
      {
        Logger.Error(exc, "Could not write file {filename}", FileSystemUtils.GetShortDisplayFileName(m_FullPath));
        throw new FileWriterException($"Could not write file '{FileSystemUtils.GetShortDisplayFileName(m_FullPath)}'",
          exc);
      }
      finally
      {
        Logger.Debug("Finished writing {filesetting} Records: {records}", m_FileSettingDisplay, Records);
        WriteFinished?.Invoke(this, null);
      }

      return Records;
    }

    private static string ReplacePlaceHolder([NotNull] string input, char fieldDelimiterChar, string fileName,
      string replacement) => input.PlaceholderReplace("ID", replacement)
      .PlaceholderReplace("FileName", fileName)
      .PlaceholderReplace("Delim", fieldDelimiterChar.ToString(CultureInfo.CurrentCulture))
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
    protected DateTime HandleTimeZone(DateTime dataObject, [NotNull] ColumnInfo columnInfo,
      [NotNull] IDataRecord reader)
    {
      if (columnInfo is null)
        throw new ArgumentNullException(nameof(columnInfo));
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (columnInfo.ColumnOrdinalTimeZoneReader > -1)
      {
        var destinationTimeZoneId = reader.GetString(columnInfo.ColumnOrdinalTimeZoneReader);
        if (string.IsNullOrEmpty(destinationTimeZoneId))
          HandleWarning(columnInfo.Column.Name, "Time zone is empty, value not converted");
        else
          // ReSharper disable once PossibleInvalidOperationException
          return FunctionalDI.AdjustTZExport(dataObject, destinationTimeZoneId, Columns.IndexOf(columnInfo),
            (columnNo, msg) => HandleWarning(Columns[columnNo].Column.Name, msg)).Value;
      }
      else if (!string.IsNullOrEmpty(columnInfo.ConstantTimeZone))
      {
        // ReSharper disable once PossibleInvalidOperationException
        return FunctionalDI.AdjustTZExport(dataObject, columnInfo.ConstantTimeZone,
          Columns.IndexOf(columnInfo), (columnNo, msg) => HandleWarning(Columns[columnNo].Column.Name, msg)).Value;
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
    [NotNull]
    protected string TextEncodeField([NotNull] IFileFormat fileFormat, object dataObject,
      [NotNull] ColumnInfo columnInfo, bool isHeader,
      [CanBeNull] IDataReader reader, [CanBeNull] Func<string, DataType, IFileFormat, string> handleQualify)
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
        displayAs = dataObject.ToString();
      }
      else
      {
        try
        {
          if (dataObject == null || dataObject is DBNull)
            displayAs = columnInfo.Column.ValueFormat.DisplayNullAs;
          else
            switch (columnInfo.Column.ValueFormat.DataType)
            {
              case DataType.Integer:
                displayAs = dataObject is long l
                  ? l.ToString("0", CultureInfo.InvariantCulture)
                  : ((int) dataObject).ToString("0", CultureInfo.InvariantCulture);
                break;

              case DataType.Boolean:
                displayAs = (bool) dataObject
                  ? columnInfo.Column.ValueFormat.True
                  : columnInfo.Column.ValueFormat.False;
                break;

              case DataType.Double:
                displayAs = StringConversion.DoubleToString(
                  dataObject is double d ? d : Convert.ToDouble(dataObject.ToString(), CultureInfo.InvariantCulture),
                  columnInfo.Column.ValueFormat);
                break;

              case DataType.Numeric:
                displayAs = StringConversion.DecimalToString(
                  dataObject is decimal @decimal
                    ? @decimal
                    : Convert.ToDecimal(dataObject.ToString(), CultureInfo.InvariantCulture),
                  columnInfo.Column.ValueFormat);
                break;

              case DataType.DateTime:
                displayAs = reader == null
                  ? StringConversion.DateTimeToString((DateTime) dataObject, columnInfo.Column.ValueFormat)
                  : StringConversion.DateTimeToString(HandleTimeZone((DateTime) dataObject, columnInfo, reader),
                    columnInfo.Column.ValueFormat);
                break;

              case DataType.Guid:
                // 382c74c3-721d-4f34-80e5-57657b6cbc27
                displayAs = ((Guid) dataObject).ToString();
                break;

              case DataType.String:
              case DataType.TextToHtml:
              case DataType.TextToHtmlFull:
              case DataType.TextPart:
                displayAs = dataObject.ToString();
                if (columnInfo.Column.ValueFormat.DataType == DataType.TextToHtml)
                  displayAs = HTMLStyle.TextToHtmlEncode(displayAs);

                // a new line of any kind will be replaced with the placeholder if set
                if (fileFormat.NewLinePlaceholder.Length > 0)
                  displayAs = StringUtils.HandleCRLFCombinations(displayAs, fileFormat.NewLinePlaceholder);

                if (fileFormat.DelimiterPlaceholder.Length > 0)
                  displayAs = displayAs.Replace(fileFormat.FieldDelimiterChar.ToString(CultureInfo.CurrentCulture),
                    fileFormat.DelimiterPlaceholder);

                if (fileFormat.QuotePlaceholder.Length > 0 && fileFormat.FieldQualifierChar != '\0')
                  displayAs = displayAs.Replace(fileFormat.FieldQualifierChar.ToString(), fileFormat.QuotePlaceholder);
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
          displayAs = dataObject?.ToString() ?? string.Empty;
          if (string.IsNullOrEmpty(displayAs))
            HandleError(columnInfo.Column.Name, ex.Message);
          else
            HandleWarning(columnInfo.Column.Name,
              "Value stored as: " + displayAs +
              $"\nExpected {columnInfo.Column.ValueFormat.DataType} but was {dataObject?.GetType()}" + ex.Message);
        }
      }

      // Adjust the output in case its is fixed length
      if (fileFormat.IsFixedLength)
      {
        if (displayAs.Length <= columnInfo.FieldLength || columnInfo.FieldLength <= 0)
          return displayAs.PadRight(columnInfo.FieldLength, ' ');
        HandleWarning(columnInfo.Column.Name,
          $"Text with length of {displayAs.Length} has been cut off after {columnInfo.FieldLength} character");
        return displayAs.Substring(0, columnInfo.FieldLength);
      }

      // Qualify text if required
      if (fileFormat.FieldQualifierChar != '\0' && handleQualify != null)
        return handleQualify(displayAs, columnInfo.Column.ValueFormat.DataType, fileFormat);

      return displayAs;
    }

    protected abstract Task WriteReaderAsync(IFileReader reader, Stream output,
      CancellationToken cancellationToken);
  }
}