#nullable enable
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <inheritdoc cref="DisposableBase" />
  public sealed class SteppedDataTableLoader : DisposableBase
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    , IAsyncDisposable
#endif
  {
    private DataReaderWrapper? m_DataReaderWrapper;
    private IFileReader? m_FileReader;    
    
    /// <summary>
    ///   Determine if the data Reader is at the end of the file
    /// </summary>
    /// <returns>True if you can read; otherwise, false.</returns>
    public bool EndOfFile => m_DataReaderWrapper?.EndOfFile ?? true;

    /// <summary>
    /// Starts the load of data from a file setting into the data table from m_GetDataTable
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="actionSetDataTable">Action to pass on the data table</param>
    /// <param name="setRefreshDisplayAsync">>Action to display and filter the data table</param>
    /// <param name="durationInitial">The duration for the initial load</param>
    /// <param name="progress">Process display to pass on progress information</param>
    /// <param name="addWarning">Add warnings.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="CsvTools.FileReaderException">Could not get reader for {fileSetting}</exception>
    public async Task StartAsync(
      IFileSetting fileSetting,
      Action<DataTable> actionSetDataTable,
      Action<CancellationToken> setRefreshDisplayAsync,
      TimeSpan durationInitial,
      IProgress<ProgressInfo>? progress,
      EventHandler<WarningEventArgs>? addWarning,
      CancellationToken cancellationToken)
    {
      Logger.Debug("Starting to load data");
      //m_Id = fileSetting.ID;
      m_FileReader = FunctionalDI.FileReaderWriterFactory.GetFileReader(fileSetting, cancellationToken);
      if (m_FileReader is null)
        throw new FileReaderException($"Could not get reader for {fileSetting}");
      if (progress != null)
        m_FileReader.ReportProgress = progress;
#if !CsvQuickViewer
      RowErrorCollection? warningList = null;
      if (addWarning != null)
      {
        warningList = new RowErrorCollection(m_FileReader);
        m_FileReader.Warning += addWarning;
        m_FileReader.Warning -= warningList.Add;
      }
#endif
      Logger.Debug("Opening reader");
      await m_FileReader.OpenAsync(cancellationToken).ConfigureAwait(false);
#if !CsvQuickViewer
      if (addWarning != null)
      {
        warningList!.HandleIgnoredColumns(m_FileReader);
        warningList.PassWarning += addWarning;
      }
#endif
      m_DataReaderWrapper = new DataReaderWrapper(m_FileReader, fileSetting.DisplayStartLineNo, fileSetting.DisplayEndLineNo, fileSetting.DisplayRecordNo, false, fileSetting.RecordLimit);

      // the initial progress is set on the source reader, no need to pass it in, when calling GetNextBatch this needs to be set though
      await GetNextBatch(null, durationInitial, actionSetDataTable, setRefreshDisplayAsync, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Loads the next batch of data from a file setting into the data table from m_GetDataTable
    /// </summary>
    /// <param name="actionSendNewDataTable">Action to pass on the data table, if called a second time make sure data is merged</param>
    /// <param name="setRefreshDisplayAsync">>Action to display and filter the data table</param>
    /// <param name="progress">Process display to pass on progress information, ideally the underlying m_FileReader.ReportProgress is used though</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="duration">For maximum duration for the read process</param>    
    public async Task GetNextBatch(IProgress<ProgressInfo>? progress, TimeSpan duration, Action<DataTable> actionSendNewDataTable,
      Action<CancellationToken> setRefreshDisplayAsync, CancellationToken cancellationToken)
    {
      if (m_DataReaderWrapper is null)
        return;
      Logger.Debug("Getting batch");
      var dt = await m_DataReaderWrapper.GetDataTableAsync(duration, progress, cancellationToken).ConfigureAwait(false);
     
      try
      {
        progress?.Report(new ProgressInfo("Setting DataTable"));
        actionSendNewDataTable.Invoke(dt);
      }
      catch (InvalidOperationException ex)
      {
        // ignore
        Logger.Warning(ex, "SetDataTable");
      }

      try
      {
        progress?.Report(new ProgressInfo("Refresh Display"));
        setRefreshDisplayAsync(cancellationToken);
      }
      catch (InvalidOperationException ex)
      {
        // ignore
        Logger.Warning(ex, "RefreshDisplayAsync");
      }

      if (m_DataReaderWrapper.EndOfFile)
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        await DisposeAsync().ConfigureAwait(false);
#else
        Dispose();
#endif      
    }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
      if (m_DataReaderWrapper != null)
        await m_DataReaderWrapper.DisposeAsync().ConfigureAwait(false);

      if (m_FileReader != null)
        await m_FileReader.DisposeAsync().ConfigureAwait(false);
    }
#endif

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        m_DataReaderWrapper?.Dispose();
        m_DataReaderWrapper = null;
        m_FileReader?.Dispose();
        m_FileReader = null;
      }
    }
  }
}