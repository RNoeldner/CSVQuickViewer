#nullable enable
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public class SteppedDataTableLoader : DisposableBase
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    , IAsyncDisposable
#endif
  {
    private DataReaderWrapper? m_DataReaderWrapper;
    private IFileReader? m_FileReader;
    private string m_Id = string.Empty;
    public bool EndOfFile => m_DataReaderWrapper?.EndOfFile ?? true;

    /// <summary>
    /// Starts the load of data from a file setting into the data table from m_GetDataTable
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="actionSetDataTable">Action to pass on the data table</param>
    /// <param name="setRefreshDisplayAsync">>Action to display nad filter the data table</param>
    /// <param name="addErrorField">if set to <c>true</c> include error column.</param>
    /// <param name="restoreError">Restore column and row errors from error columns</param>
    /// <param name="durationInitial">The duration for the initial initial.</param>
    /// <param name="progress">Process display to pass on progress information</param>
    /// <param name="addWarning">Add warnings.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="CsvTools.FileReaderException">Could not get reader for {fileSetting}</exception>
    public async Task StartAsync(
      IFileSetting fileSetting,
      Action<DataTable> actionSetDataTable,
      Action<CancellationToken> setRefreshDisplayAsync,
      bool addErrorField,
      bool restoreError,
      TimeSpan durationInitial,
      IProgress<ProgressInfo>? progress,
      EventHandler<WarningEventArgs>? addWarning, CancellationToken cancellationToken)
    {
      Logger.Debug("Starting to load data");
      m_Id = fileSetting.ID;
      m_FileReader = FunctionalDI.GetFileReader(fileSetting, cancellationToken);
      if (m_FileReader is null)
        throw new FileReaderException($"Could not get reader for {fileSetting}");
      if (progress != null)
        m_FileReader.ReportProgress = progress;

      RowErrorCollection? warningList = null;
      if (addWarning != null)
      {
        warningList = new RowErrorCollection(m_FileReader);
        m_FileReader.Warning += addWarning;
        m_FileReader.Warning -= warningList.Add;
      }
      Logger.Debug("Opening reader");
      await m_FileReader.OpenAsync(cancellationToken).ConfigureAwait(false);

      if (addWarning != null)
      {
        warningList!.HandleIgnoredColumns(m_FileReader);
        warningList.PassWarning += addWarning;
      }

      m_DataReaderWrapper = new DataReaderWrapper(m_FileReader, addErrorField, fileSetting.DisplayStartLineNo, fileSetting.DisplayEndLineNo, fileSetting.DisplayRecordNo);

      // the initial progress is set on the source reader
      await GetNextBatch(progress, durationInitial, restoreError, actionSetDataTable, setRefreshDisplayAsync, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Loads teh next batch of data from a file setting into the data table from m_GetDataTable
    /// </summary>
    /// <param name="actionSetDataTable">Action to pass on the data table</param>
    /// <param name="setRefreshDisplayAsync">>Action to display nad filter the data table</param>
    /// <param name="progress">Process display to pass on progress information</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="duration">For maximum duration for the read process</param>
    /// <param name="restoreError">Restore column and row errors from error columns</param>
    public async Task GetNextBatch(
      IProgress<ProgressInfo>? progress, TimeSpan duration, bool restoreError,
      Action<DataTable> actionSetDataTable, Action<CancellationToken> setRefreshDisplayAsync,
      CancellationToken cancellationToken)
    {
      if (m_DataReaderWrapper is null)
        return;
      Logger.Debug("Getting batch");
      var dt = await m_DataReaderWrapper.GetDataTableAsync(duration, restoreError, progress, cancellationToken).ConfigureAwait(false);

      // for Debugging its nice to know where it all came form
      if (!string.IsNullOrEmpty(m_Id))
        dt.TableName = m_Id;
      try
      {
        Logger.Debug("Setting DataTable");
        actionSetDataTable.Invoke(dt);
      }
      catch (InvalidOperationException ex)
      {
        // ignore
        Logger.Warning(ex, "SetDataTable");
      }

      try
      {
        Logger.Debug("Refresh Display");
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
    public async ValueTask DisposeAsync()
    {
      if (m_DataReaderWrapper != null)
        await m_DataReaderWrapper.DisposeAsync().ConfigureAwait(false);

      if (m_FileReader != null)
        await m_FileReader.DisposeAsync().ConfigureAwait(false);

      // Suppress finalization.
      GC.SuppressFinalize(this);
    }
#endif

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      m_DataReaderWrapper?.Dispose();
      m_FileReader?.Dispose();
      m_FileReader = null;
    }
  }
}