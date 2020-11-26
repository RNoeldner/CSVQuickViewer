using JetBrains.Annotations;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public class TwoStepDataTableLoader : IDisposable
  {
    [CanBeNull] private readonly Action m_ActionBegin;
    [CanBeNull] private readonly Action<DataReaderWrapper> m_ActionFinished;
    [NotNull] private readonly Func<DataTable> m_GetDataTable;
    [CanBeNull] private readonly Func<FilterType, CancellationToken, Task> m_RefreshDisplayAsync;
    [NotNull] private readonly Action<DataTable> m_SetDataTable;
    [CanBeNull] private readonly Action<Func<IProcessDisplay, Task>> m_SetLoadNextBatchAsync;
    [CanBeNull] private DataReaderWrapper m_DataReaderWrapper;
    [CanBeNull] private IFileReader m_FileReader;

    public TwoStepDataTableLoader([NotNull] Action<DataTable> actionSetDataTable,
      [NotNull] Func<DataTable> getDataTable,
      [CanBeNull] Func<FilterType, CancellationToken, Task> setRefreshDisplayAsync,
      [CanBeNull] Action<Func<IProcessDisplay, Task>> loadNextBatchAsync, [CanBeNull] Action actionBegin,
      [CanBeNull] Action<DataReaderWrapper> actionFinished)
    {
      m_SetDataTable = actionSetDataTable ?? throw new ArgumentNullException(nameof(actionSetDataTable));
      m_GetDataTable = getDataTable ?? throw new ArgumentNullException(nameof(getDataTable));
      m_SetLoadNextBatchAsync = loadNextBatchAsync;
      m_RefreshDisplayAsync = setRefreshDisplayAsync;
      m_ActionFinished = actionFinished;
      m_ActionBegin = actionBegin;
    }

    public void Dispose()
    {
      m_DataReaderWrapper?.Dispose();
      m_DataReaderWrapper = null;
      m_FileReader?.Dispose();
      m_FileReader = null;
    }

    public async Task StartAsync([NotNull] IFileSetting fileSetting, bool includeError, TimeSpan durationInitial,
      [NotNull] IProcessDisplay processDisplay, [CanBeNull] EventHandler<WarningEventArgs> addWarning)
    {
      m_FileReader = FunctionalDI.GetFileReader(fileSetting, TimeZoneInfo.Local.Id, processDisplay);
      if (m_FileReader == null)
        throw new FileReaderException($"Could not get reader for {fileSetting}");

      RowErrorCollection warningList = null;
      if (addWarning != null)
      {
        warningList = new RowErrorCollection(m_FileReader);
        m_FileReader.Warning += addWarning;
        m_FileReader.Warning -= warningList.Add;
      }

      Logger.Information("Reading data for display");
      await m_FileReader.OpenAsync(processDisplay.CancellationToken).ConfigureAwait(false);

      if (addWarning != null)
      {
        warningList.HandleIgnoredColumns(m_FileReader);
        warningList.PassWarning += addWarning;
      }

      m_DataReaderWrapper = new DataReaderWrapper(m_FileReader, fileSetting.RecordLimit, includeError,
        fileSetting.DisplayStartLineNo, fileSetting.DisplayRecordNo, fileSetting.DisplayEndLineNo);

      m_SetDataTable(m_DataReaderWrapper.GetEmptyDataTable());
      m_ActionBegin?.Invoke();

      await GetBatchByTimeSpan(durationInitial, includeError, processDisplay).ConfigureAwait(false);

      m_SetLoadNextBatchAsync?.Invoke(process => GetBatchByTimeSpan(TimeSpan.MaxValue, includeError, process));

      m_ActionFinished?.Invoke(m_DataReaderWrapper);
    }

    private async Task GetBatchByTimeSpan(TimeSpan maxDuration, bool restoreError,
      [NotNull] IProcessDisplay processDisplay)
    {
      if (m_DataReaderWrapper == null)
        return;
      if (processDisplay == null)
        throw new ArgumentNullException(nameof(processDisplay));
      try
      {
        processDisplay.SetMaximum(100);
        await m_DataReaderWrapper.LoadDataTable(m_GetDataTable(), maxDuration, restoreError,
          (l, i) => processDisplay.SetProcess($"Reading data...\nRecord: {l:N0}", i, false),
          processDisplay.CancellationToken).ConfigureAwait(false);

        await m_RefreshDisplayAsync(FilterType.All, processDisplay.CancellationToken)
          .ConfigureAwait(false);

        if (m_DataReaderWrapper.EndOfFile)
          Dispose();
      }
      catch (Exception e)
      {
        Logger.Error(e);
      }
    }
  }
}