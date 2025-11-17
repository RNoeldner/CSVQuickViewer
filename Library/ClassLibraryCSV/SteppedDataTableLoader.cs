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
using System.Data;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <inheritdoc cref="DisposableBase" />
  public sealed class SteppedDataTableLoader : DisposableBase
  {
    private DataReaderWrapper? m_DataReaderWrapper;

    /// <summary>
    /// The number of columns supported, if more columns are found its assumed there is something wrong
    /// Without this check the DataGrid will run into issues
    /// </summary>
    private const int cMaxColumns = 2048;

    /// <summary>
    ///   Determine if the data Reader is at the end of the file
    /// </summary>
    /// <returns>True if you can read; otherwise, false.</returns>
    public bool EndOfFile => m_DataReaderWrapper?.EndOfFile ?? true;

    /// <summary>
    /// Starts the load of data from a file setting into the data table from m_GetDataTable
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="durationInitial">The duration for the initial load</param>
    /// <param name="progress">Process display to pass on progress information</param>
    /// <param name="addWarning">Add warnings.</param>    
    /// <exception cref="CsvTools.FileReaderException">Could not get reader for {fileSetting}</exception>
    public async Task<DataTable> StartAsync(
      IFileSetting fileSetting,
      TimeSpan durationInitial,
      IProgressWithCancellation progress,
      EventHandler<WarningEventArgs>? addWarning)
    {
      progress.Report("Starting to load data");
      //m_Id = fileSetting.ID;
      var fileReader = FunctionalDI.FileReaderWriterFactory.GetFileReader(fileSetting, progress.CancellationToken);
      if (fileReader is null)
        throw new FileReaderException($"Could not get reader for {fileSetting}");

      try
      {
        progress.Report("Opening reader");
        fileReader.ReportProgress = progress;
        if (addWarning != null)
          fileReader.Warning += addWarning;

        await fileReader.OpenAsync(progress.CancellationToken).ConfigureAwait(false);

        if (fileReader.FieldCount > cMaxColumns)
          throw new FileReaderException($"The amount of columns {fileReader.FieldCount:N0} is very high, assuming misconfiguration of reader {fileSetting.GetDisplay()}");

        // Stop reporting progress to the outside, we do that in the DataReaderWrapper
        fileReader.ReportProgress = ProgressCancellation.Instance;

        m_DataReaderWrapper = new DataReaderWrapper(fileReader, fileSetting.DisplayStartLineNo,
          fileSetting.DisplayEndLineNo, fileSetting.DisplayRecordNo, false, fileSetting.RecordLimit);

        return await GetNextBatch(durationInitial, progress).ConfigureAwait(false);
      }
      catch (Exception ex) when (ex is not FileReaderException)
      {
        throw new FileReaderException($"Error initializing reader for {fileSetting.GetDisplay()}", ex);
      }
    }

    /// <summary>
    /// Loads the next batch of data from a file setting into the data table from Wrapper
    /// </summary>
    /// <param name="progress">Process display to pass on progress information</param>
    /// <param name="duration">For maximum duration for the read process</param>        
    public async Task<DataTable> GetNextBatch(TimeSpan duration, IProgressWithCancellation progress)
    {
      if (m_DataReaderWrapper is null)
        return new DataTable();

      try
      {
        m_DataReaderWrapper.ReportProgress = progress;
        progress.Report("Getting batch");

        var dt = await m_DataReaderWrapper.GetDataTableAsync(duration, progress)
            .ConfigureAwait(false);

        if (m_DataReaderWrapper.EndOfFile)
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
          await DisposeAsync().ConfigureAwait(false);
#else
          Dispose();
#endif
        return dt;
      }
      catch (OperationCanceledException)
      {
        throw; // Let cancellation propagate
      }
      catch (Exception ex)
      {
        throw new FileReaderException("Error reading next batch of data", ex);
      }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (!disposing) return;

      try
      {
        m_DataReaderWrapper?.Dispose();
      }
      finally
      {
        m_DataReaderWrapper = null;
      }
    }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
      if (m_DataReaderWrapper != null)
      {
        try
        {
            await m_DataReaderWrapper.DisposeAsync().ConfigureAwait(false);
        }
        finally
        {
            m_DataReaderWrapper = null;
        }
      }
    }
#endif
  }
}
