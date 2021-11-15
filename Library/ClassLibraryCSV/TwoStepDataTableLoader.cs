using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public class TwoStepDataTableLoader : DisposableBase
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
                                        , IAsyncDisposable
#endif
  {
    private readonly Action? m_ActionBegin;
    private readonly Action<DataReaderWrapper>? m_ActionFinished;
    private readonly Func<DataTable> m_GetDataTable;
    private readonly Func<FilterType, CancellationToken, Task>? m_RefreshDisplayAsync;
    private readonly Action<DataTable> m_SetDataTable;
    private readonly Action<Func<IProcessDisplay?, CancellationToken, Task>>? m_SetLoadNextBatchAsync;
    private DataReaderWrapper? m_DataReaderWrapper;
    private IFileReader? m_FileReader;

    public TwoStepDataTableLoader(
      in Action<DataTable> actionSetDataTable,
      in Func<DataTable> getDataTable,
      in Func<FilterType, CancellationToken, Task>? setRefreshDisplayAsync,
      in Action<Func<IProcessDisplay?, CancellationToken, Task>>? loadNextBatchAsync,
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

#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
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

    public async Task StartAsync(
      IFileSetting fileSetting,
      bool includeError,
      TimeSpan durationInitial,
      IProcessDisplay? processDisplay,
      EventHandler<WarningEventArgs>? addWarning, CancellationToken cancellationToken)
    {
      m_FileReader = FunctionalDI.GetFileReader(fileSetting, TimeZoneInfo.Local.Id, processDisplay, cancellationToken);
      if (m_FileReader is null)
        throw new FileReaderException($"Could not get reader for {fileSetting}");

      RowErrorCollection? warningList = null;
      if (addWarning != null)
      {
        warningList = new RowErrorCollection(m_FileReader);
        m_FileReader.Warning += addWarning;
        m_FileReader.Warning -= warningList.Add;
      }

      Logger.Information("Reading data for display");
      await m_FileReader.OpenAsync(cancellationToken).ConfigureAwait(false);

      if (addWarning != null)
      {
        warningList!.HandleIgnoredColumns(m_FileReader);
        warningList.PassWarning += addWarning;
      }

      m_DataReaderWrapper = new DataReaderWrapper(
        m_FileReader,
        fileSetting.RecordLimit,
        includeError,
        fileSetting.DisplayStartLineNo,
        fileSetting.DisplayRecordNo,
        fileSetting.DisplayEndLineNo);

      m_ActionBegin?.Invoke();
      await GetBatchByTimeSpan(durationInitial, includeError, processDisplay, m_SetDataTable, cancellationToken).ConfigureAwait(false);

      m_SetLoadNextBatchAsync?.Invoke((process, cancellationToken) => GetBatchByTimeSpan(TimeSpan.MaxValue, includeError, process, dt => m_GetDataTable().Merge(dt), cancellationToken));

      m_ActionFinished?.Invoke(m_DataReaderWrapper);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        m_FileReader?.Dispose();
        m_FileReader = null;
      }
    }

    private async Task GetBatchByTimeSpan(
      TimeSpan maxDuration,
      bool restoreError,
      IProcessDisplay? processDisplay,
      Action<DataTable> action, CancellationToken cancellationToken)
    {
      if (m_DataReaderWrapper is null)
        return;      
      try
      {
        processDisplay?.SetMaximum(100);

        var dt = await m_DataReaderWrapper.LoadDataTable(
                   maxDuration,
                   restoreError,
                   processDisplay,
                   cancellationToken).ConfigureAwait(false);

        action.Invoke(dt);

        if (m_RefreshDisplayAsync != null)
          await m_RefreshDisplayAsync(FilterType.All, cancellationToken).ConfigureAwait(false);

        if (m_DataReaderWrapper.EndOfFile)
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
          await DisposeAsync();
#else
          Dispose();
#endif
      }
      catch (Exception e)
      {
        Logger.Error(e, $"{nameof(TwoStepDataTableLoader)} .{nameof(GetBatchByTimeSpan)} {e.Message}");
      }
    }
  }
}