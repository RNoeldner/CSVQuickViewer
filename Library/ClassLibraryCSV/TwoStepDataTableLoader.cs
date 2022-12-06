#nullable enable
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public class TwoStepDataTableLoader : DisposableBase
#if NETSTANDARD2_1_OR_GREATER
                                        , IAsyncDisposable
#endif
  {
    private readonly Action? m_ActionBegin;
    private readonly Action<DataReaderWrapper>? m_ActionFinished;
    private readonly Func<DataTable> m_GetDataTable;
    private readonly Func<FilterTypeEnum, CancellationToken, Task>? m_RefreshDisplayAsync;
    private readonly Action<DataTable> m_SetDataTable;
    private readonly Action<Func<IProgress<ProgressInfo>?, CancellationToken, Task>>? m_SetLoadNextBatchAsync;
    private DataReaderWrapper? m_DataReaderWrapper;
    private IFileReader? m_FileReader;
    private string m_ID = string.Empty;

    public TwoStepDataTableLoader(
      in Action<DataTable> actionSetDataTable,
      in Func<DataTable> getDataTable,
      in Func<FilterTypeEnum, CancellationToken, Task>? setRefreshDisplayAsync,
      in Action<Func<IProgress<ProgressInfo>?, CancellationToken, Task>>? loadNextBatchAsync,
      in Action? actionBegin,
      in Action<DataReaderWrapper>? actionFinished)
    {
      m_SetDataTable = actionSetDataTable ?? throw new ArgumentNullException(nameof(actionSetDataTable));
      m_GetDataTable = getDataTable ?? throw new ArgumentNullException(nameof(getDataTable));
      m_SetLoadNextBatchAsync = loadNextBatchAsync;
      m_RefreshDisplayAsync = setRefreshDisplayAsync;
      m_ActionFinished = actionFinished;
      m_ActionBegin = actionBegin;
    }

#if NETSTANDARD2_1_OR_GREATER
    public async ValueTask DisposeAsync()
    {
      if (m_FileReader != null)
      {
        await m_FileReader.DisposeAsync().ConfigureAwait(false);
        m_FileReader = null;
      }

      // Suppress finalization.
      GC.SuppressFinalize(this);
    }
#endif
    /// <summary>
    /// Starts the load of data from a file setting into the data table from m_GetDataTable
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="addErrorField">if set to <c>true</c> include error column.</param>
    /// <param name="restoreError">Restore column and row errors from error columns</param>
    /// <param name="durationInitial">The duration for the initial initial.</param>
    /// <param name="progress">Process display to pass on progress information</param>
    /// <param name="addWarning">Add warnings.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="CsvTools.FileReaderException">Could not get reader for {fileSetting}</exception>
    public async Task StartAsync(
      IFileSetting fileSetting,
      bool addErrorField,
      bool restoreError,
      TimeSpan durationInitial,
      IProgress<ProgressInfo>? progress,
      EventHandler<WarningEventArgs>? addWarning, CancellationToken cancellationToken)
    {
      m_ID = fileSetting.ID;
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

      m_ActionBegin?.Invoke();

      // the initial progress is set on the source reader
      await GetBatchByTimeSpan(durationInitial, restoreError, null, m_SetDataTable,
          cancellationToken)
        .ConfigureAwait(false);

      m_SetLoadNextBatchAsync?.Invoke((process, token) =>
        GetBatchByTimeSpan(durationInitial, restoreError, process,
          dt => m_GetDataTable().Merge(dt), token));

      m_ActionFinished?.Invoke(m_DataReaderWrapper);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      m_DataReaderWrapper?.Dispose();
      m_FileReader?.Dispose();
      m_FileReader = null;
    }

    private async Task GetBatchByTimeSpan(
      TimeSpan maxDuration,
      bool restoreError,
      IProgress<ProgressInfo>? progress,
      Action<DataTable> action, CancellationToken cancellationToken)
    {
      if (m_DataReaderWrapper is null)
        return;

      var dt = await m_DataReaderWrapper.GetDataTableAsync(
        maxDuration,
        restoreError,
        progress,
        cancellationToken).ConfigureAwait(false);

      // for Debugging its nice to know where it all came form
      if (!string.IsNullOrEmpty(m_ID))
        dt.TableName = m_ID;
      try
      {
        action.Invoke(dt);
      }
      catch (InvalidOperationException)
      {
        // ignore
      }

      if (m_RefreshDisplayAsync != null)
        try
        {
          await m_RefreshDisplayAsync(FilterTypeEnum.All, cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
          // ignore
        }

      if (m_DataReaderWrapper.EndOfFile)
#if NETSTANDARD2_1_OR_GREATER
        await DisposeAsync().ConfigureAwait(false);
#else
        Dispose();
#endif
    }
  }
}