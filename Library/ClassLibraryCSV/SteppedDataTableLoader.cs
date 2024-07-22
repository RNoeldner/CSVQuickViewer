#nullable enable
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <inheritdoc cref="DisposableBase" />
  public sealed class SteppedDataTableLoader : DisposableBase
  {
    private DataReaderWrapper? m_DataReaderWrapper;

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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="CsvTools.FileReaderException">Could not get reader for {fileSetting}</exception>
    public async Task<DataTable> StartAsync(
      IFileSetting fileSetting,
      TimeSpan durationInitial,
      IProgress<ProgressInfo>? progress,
      EventHandler<WarningEventArgs>? addWarning,
      CancellationToken cancellationToken)
    {
      Logger.Debug("Starting to load data");
      //m_Id = fileSetting.ID;
      var fileReader = FunctionalDI.FileReaderWriterFactory.GetFileReader(fileSetting, cancellationToken);
      if (fileReader is null)
        throw new FileReaderException($"Could not get reader for {fileSetting}");
      if (progress != null)
        fileReader.ReportProgress = progress;
      if (addWarning != null)
        fileReader.Warning += addWarning;

      Logger.Debug("Opening reader");
      await fileReader.OpenAsync(cancellationToken).ConfigureAwait(false);

      m_DataReaderWrapper = new DataReaderWrapper(fileReader, fileSetting.DisplayStartLineNo,
        fileSetting.DisplayEndLineNo, fileSetting.DisplayRecordNo, false, fileSetting.RecordLimit);

      return await GetNextBatch(progress, durationInitial, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Loads the next batch of data from a file setting into the data table from m_GetDataTable
    /// </summary>
    /// <param name="progress">Process display to pass on progress information, ideally the underlying m_FileReader.ReportProgress is used though</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="duration">For maximum duration for the read process</param>    
    public async Task<DataTable> GetNextBatch(IProgress<ProgressInfo>? progress, TimeSpan duration,
      CancellationToken cancellationToken)
    {
      if (m_DataReaderWrapper is null)
        return new DataTable();

      Logger.Debug("Getting batch");
      var dt = await m_DataReaderWrapper.GetDataTableAsync(duration, progress, cancellationToken).ConfigureAwait(false);

      if (m_DataReaderWrapper.EndOfFile)
        Dispose(true);

      return dt;
    }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
      if (m_DataReaderWrapper != null)
      {
        await m_DataReaderWrapper.DisposeAsync().ConfigureAwait(false);
        m_DataReaderWrapper = null;
      }
    }
#endif

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      m_DataReaderWrapper?.Dispose();
      m_DataReaderWrapper = null;
    }
  }
}