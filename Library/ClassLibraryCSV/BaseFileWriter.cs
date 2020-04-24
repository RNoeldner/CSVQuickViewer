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
    protected readonly List<ColumnInfo> Columns = new List<ColumnInfo>();
    private readonly IFileSettingPhysicalFile m_FileSetting;
    private readonly IProcessDisplay m_ProcessDisplay;
    private readonly string m_SourceTimeZone;
    private DateTime m_LastNotification = DateTime.Now;

    /// <summary>
    ///   Initializes a new instance of the <see cref="BaseFileWriter" /> class.
    /// </summary>
    /// <param name="fileSetting">the file setting with the definition for the file</param>
    /// <param name="sourceTimeZone">Timezone of the source</param>
    /// <param name="processDisplay">The process display.</param>
    /// <exception cref="ArgumentNullException">fileSetting</exception>
    /// <exception cref="ArgumentException">No SQL Reader set</exception>
    protected BaseFileWriter(IFileSettingPhysicalFile fileSetting, string sourceTimeZone,
      IProcessDisplay processDisplay)
    {
      m_ProcessDisplay = processDisplay;
      m_SourceTimeZone = string.IsNullOrEmpty(sourceTimeZone) ? TimeZoneInfo.Local.Id : sourceTimeZone;
      m_FileSetting = fileSetting ?? throw new ArgumentNullException(nameof(fileSetting));
      Logger.Debug("Created Writer for {filesetting}", fileSetting.ToString());
    }

    private long Records { get; set; }

    /// <summary>
    ///   Gets or sets the error message.
    /// </summary>
    /// <value>The error message.</value>
    public virtual string ErrorMessage { get; protected set; }

    /// <summary>
    ///   Event handler called if a warning or error occurred
    /// </summary>
    public virtual event EventHandler<WarningEventArgs> Warning;

    /// <summary>
    ///   Event to be raised if writing is finished
    /// </summary>
    public event EventHandler WriteFinished;

    public virtual long Write()
    {
      if (string.IsNullOrEmpty(m_FileSetting.SqlStatement))
        return 0;
      if (FunctionalDI.SQLDataReader == null)
        throw new ArgumentException("No SQL Reader set");
      using (var sqlReader =
        FunctionalDI.SQLDataReader(m_FileSetting.SqlStatement, m_ProcessDisplay, m_FileSetting.Timeout))
      {
        return Write(sqlReader);
      }
    }

    /// <summary>
    ///   Writes the specified file.
    /// </summary>
    /// <returns>Number of records written</returns>
    public virtual async Task<long> WriteAsync()
    {
      if (string.IsNullOrEmpty(m_FileSetting.SqlStatement))
        return 0;
      if (FunctionalDI.SQLDataReader == null)
        throw new ArgumentException("No SQL Reader set");
      using (var sqlReader =
        FunctionalDI.SQLDataReader(m_FileSetting.SqlStatement, m_ProcessDisplay, m_FileSetting.Timeout))
      {
        return await WriteAsync(sqlReader);
      }
    }

    protected string GetRedordEnd() => m_FileSetting.FileFormat.NewLine.NewLineString();

    public async Task<long> WriteAsync(IFileReader reader)
    {
      if (reader == null)
        return -1;
      HandleWriteStart();
      if (m_ProcessDisplay != null)
        m_ProcessDisplay.Maximum = -1;
      try
      {
        using (var improvedStream = FunctionalDI.OpenWrite(m_FileSetting))
        {
          if (reader.IsClosed)
            reader.Open();

          await WriteReaderAsync(reader, improvedStream.Stream,
            m_ProcessDisplay?.CancellationToken ?? CancellationToken.None);
        }
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

      return Records;
    }

    public long Write(IFileReader reader)
    {
      if (reader == null)
        return -1;
      HandleWriteStart();
      if (m_ProcessDisplay != null)
        m_ProcessDisplay.Maximum = -1;
      try
      {
        using (var improvedStream = FunctionalDI.OpenWrite(m_FileSetting))
        {
          if (reader.IsClosed)
            reader.Open();

          WriteReader(reader, improvedStream.Stream, m_ProcessDisplay?.CancellationToken ?? CancellationToken.None);
        }
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

      return Records;
    }

    protected virtual string ReplacePlaceHolder(string input) => input.PlaceholderReplace("ID", m_FileSetting.ID)
      .PlaceholderReplace("FileName", m_FileSetting.FileName)
      .PlaceholderReplace("Records", string.Format(new CultureInfo("en-US"), "{0:n0}", Records))
      .PlaceholderReplace("Delim", m_FileSetting.FileFormat.FieldDelimiterChar.ToString(CultureInfo.CurrentCulture))
      .PlaceholderReplace("CDate", string.Format(new CultureInfo("en-US"), "{0:dd-MMM-yyyy}", DateTime.Now))
      .PlaceholderReplace("CDateLong", string.Format(new CultureInfo("en-US"), "{0:MMMM dd\\, yyyy}", DateTime.Now));

    /// <summary>
    ///   Handles the error.
    /// </summary>
    /// <param name="columnName">The column name.</param>
    /// <param name="message">The message.</param>
    protected virtual void HandleError(string columnName, string message) =>
      Warning?.Invoke(this, new WarningEventArgs(Records, 0, message, 0, 0, columnName));

    // protected void HandleProgress(string text, int progress) =>
    // m_ProcessDisplay?.SetProcess(text, progress);

    private void HandleProgress(string text) => m_ProcessDisplay?.SetProcess(text, -1, true);

    /// <summary>
    ///   Handles the time zone for a date time column
    /// </summary>
    /// <param name="dataObject">The data object.</param>
    /// <param name="columnInfo">The column information.</param>
    /// <param name="reader">The reader.</param>
    /// <returns></returns>
    protected DateTime HandleTimeZone(DateTime dataObject, ColumnInfo columnInfo, IDataRecord reader)
    {
      if (columnInfo is null)
        throw new ArgumentNullException(nameof(columnInfo));
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (columnInfo.ColumnOrdinalTimeZoneReader > -1)
      {
        var destinationTimeZoneID = reader.GetString(columnInfo.ColumnOrdinalTimeZoneReader);
        if (string.IsNullOrEmpty(destinationTimeZoneID))
          HandleWarning(columnInfo.Column.Name, "Time zone is empty, value not converted");
        else
          return FunctionalDI.AdjustTZ(dataObject, m_SourceTimeZone, destinationTimeZoneID, Columns.IndexOf(columnInfo),
            (columnNo, msg) => HandleWarning(Columns[columnNo].Column.Name, msg)).Value;
      }
      else if (!string.IsNullOrEmpty(columnInfo.ConstantTimeZone))
      {
        return FunctionalDI.AdjustTZ(dataObject, m_SourceTimeZone, columnInfo.ConstantTimeZone,
          Columns.IndexOf(columnInfo), (columnNo, msg) => HandleWarning(Columns[columnNo].Column.Name, msg)).Value;
      }

      return dataObject;
    }

    /// <summary>
    ///   Calls the event handler for warnings
    /// </summary>
    /// <param name="columnName">The column.</param>
    /// <param name="message">The message.</param>
    protected virtual void HandleWarning(string columnName, string message) => Warning?.Invoke(this,
      new WarningEventArgs(Records, 0, message.AddWarningId(), 0, 0, columnName));

    private void HandleWriteFinished()
    {
      m_FileSetting.ProcessTimeUtc = DateTime.UtcNow;
      if (!(m_FileSetting is IFileSettingPhysicalFile physicalFile) ||
          !physicalFile.SetLatestSourceTimeForWrite) return;
      FileSystemUtils.SetLastWriteTimeUtc(physicalFile.FullPath, m_FileSetting.LatestSourceTimeUtc);

      Logger.Debug("Finished writing {filesetting} Records: {records}", m_FileSetting.ToString(), Records);
      WriteFinished?.Invoke(this, null);
    }

    protected void HandleWriteStart() => Records = 0;

    protected void NextRecord()
    {
      Records++;
      if (!((DateTime.Now - m_LastNotification).TotalSeconds > .15))
        return;
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
    protected string TextEncodeField(FileFormat fileFormat, object dataObject, ColumnInfo columnInfo, bool isHeader,
      IDataReader reader, Func<string, DataType, FileFormat, string> handleQualify)
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
                  : ((int)dataObject).ToString("0", CultureInfo.InvariantCulture);
                break;

              case DataType.Boolean:
                displayAs = (bool)dataObject
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
                displayAs = StringConversion.DateTimeToString(
                  HandleTimeZone((DateTime)dataObject, columnInfo, reader), columnInfo.Column.ValueFormat);
                break;

              case DataType.Guid:
                // 382c74c3-721d-4f34-80e5-57657b6cbc27
                displayAs = ((Guid)dataObject).ToString();
                break;

              default:
                displayAs = dataObject.ToString();
                if (columnInfo.Column.ValueFormat.DataType == DataType.TextToHtml)
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
          // In case a cast did fail (eg.g trying to format as integer and providing a text, use the
          // original value
          displayAs = dataObject?.ToString() ?? string.Empty;
          if (string.IsNullOrEmpty(displayAs))
            HandleError(columnInfo.Column.Name, ex.Message);
          else
            HandleWarning(columnInfo.Column.Name, "Value stored as: " + displayAs + "\n" + ex.Message);
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
      if (!string.IsNullOrEmpty(fileFormat.FieldQualifier) && handleQualify != null)
        return handleQualify(displayAs, columnInfo.Column.ValueFormat.DataType, fileFormat);

      return displayAs;
    }

    protected abstract void WriteReader(IFileReader reader, Stream output,
      CancellationToken cancellationToken);

    protected abstract Task WriteReaderAsync(IFileReader reader, Stream output,
      CancellationToken cancellationToken);
  }
}