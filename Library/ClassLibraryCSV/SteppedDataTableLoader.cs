#nullable enable
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public class SteppedDataTableLoader : DisposableBase
#if NETSTANDARD2_1_OR_GREATER
    , IAsyncDisposable
#endif
  {
    private readonly Func<FilterTypeEnum, CancellationToken, Task> m_RefreshDisplayAsync;
    private readonly Action<DataTable> m_SetDataTable;

    private DataReaderWrapper? m_DataReaderWrapper;
    private IFileReader? m_FileReader;
    private string m_ID = string.Empty;
    private bool m_RestoreError;
    private TimeSpan m_Duration;
    public bool EndOfFile => m_DataReaderWrapper?.EndOfFile ?? true;

    /// <summary>
    /// A DataTable loaded that does load the data to a data table but does though in Batches
    /// </summary>
    /// <param name="actionSetDataTable">Action to pass on the data table</param>
    /// <param name="setRefreshDisplayAsync">>Action to display nad filter the data table</param>
    public SteppedDataTableLoader(
      in Action<DataTable> actionSetDataTable,
      in Func<FilterTypeEnum, CancellationToken, Task> setRefreshDisplayAsync)
    {
      m_SetDataTable = actionSetDataTable;
      m_RefreshDisplayAsync = setRefreshDisplayAsync;
    }

    /// <summary>
    /// Starts the load of data from a file setting into the data table from m_GetDataTable
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="addErrorField">if set to <c>true</c> include error column.</param>
    /// <param name="restoreError">Restore column and row errors from error columns</param>
    /// <param name="durationInitial">The duration for the initial initial.</param>
    /// <param name="filterType"></param>
    /// <param name="progress">Process display to pass on progress information</param>
    /// <param name="addWarning">Add warnings.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="CsvTools.FileReaderException">Could not get reader for {fileSetting}</exception>
    public async Task StartAsync(
      IFileSetting fileSetting,
      bool addErrorField,
      bool restoreError,
      TimeSpan durationInitial,
      FilterTypeEnum filterType,
      IProgress<ProgressInfo>? progress,
      EventHandler<WarningEventArgs>? addWarning, CancellationToken cancellationToken)
    {
      m_ID = fileSetting.ID;
      m_FileReader = FunctionalDI.GetFileReader(fileSetting, cancellationToken);
      m_RestoreError = restoreError;
      m_Duration = durationInitial;
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

      await m_FileReader.OpenAsync(cancellationToken).ConfigureAwait(false);

      if (addWarning != null)
      {
        warningList!.HandleIgnoredColumns(m_FileReader);
        warningList.PassWarning += addWarning;
      }

      m_DataReaderWrapper = new DataReaderWrapper(
        m_FileReader,
        addErrorField,
        fileSetting.DisplayStartLineNo,
        fileSetting.DisplayEndLineNo,
        fileSetting.DisplayRecordNo
      );

      // the initial progress is set on the source reader
      await GetNextBatch(filterType, null, cancellationToken)
        .ConfigureAwait(false);
    }


    public async Task GetNextBatch(
      FilterTypeEnum filterType,
      IProgress<ProgressInfo>? progress,
      CancellationToken cancellationToken)
    {
      if (m_DataReaderWrapper is null)
        return;

      var dt = await m_DataReaderWrapper.GetDataTableAsync(
        m_Duration,
        m_RestoreError,
        progress,
        cancellationToken).ConfigureAwait(false);

      // for Debugging its nice to know where it all came form
      if (!string.IsNullOrEmpty(m_ID))
        dt.TableName = m_ID;
      try
      {
        m_SetDataTable.Invoke(dt);
      }
      catch (InvalidOperationException ex)
      {
        // ignore
        Logger.Warning(ex, "RefreshDisplayAsync");
      }

      try
      {
        await m_RefreshDisplayAsync(filterType, cancellationToken).ConfigureAwait(false);
      }
      catch (InvalidOperationException ex)
      {
        // ignore
        Logger.Warning(ex, "RefreshDisplayAsync");
      }

      if (m_DataReaderWrapper.EndOfFile)
#if NETSTANDARD2_1_OR_GREATER
        await DisposeAsync().ConfigureAwait(false);
#else
        Dispose();
#endif
    }

#if NETSTANDARD2_1_OR_GREATER
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