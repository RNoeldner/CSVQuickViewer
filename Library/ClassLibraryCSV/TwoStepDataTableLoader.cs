using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public class TwoStepDataTableLoader : IDisposable
  {
    private readonly Action? m_ActionBegin;

    private readonly Action<DataReaderWrapper>? m_ActionFinished;

    private readonly Func<DataTable> m_GetDataTable;

    private readonly Func<FilterType, CancellationToken, Task>? m_RefreshDisplayAsync;

    private readonly Action<DataTable> m_SetDataTable;

    private readonly Action<Func<IProcessDisplay, Task>>? m_SetLoadNextBatchAsync;

    private DataReaderWrapper? m_DataReaderWrapper;

    private IFileReader? m_FileReader;

    public TwoStepDataTableLoader(
      in Action<DataTable> actionSetDataTable,
      in Func<DataTable> getDataTable,
      in Func<FilterType, CancellationToken, Task>? setRefreshDisplayAsync,
      in Action<Func<IProcessDisplay, Task>>? loadNextBatchAsync,
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

    public void Dispose()
    {
      m_DataReaderWrapper?.Dispose();
      m_DataReaderWrapper = null;
      m_FileReader?.Dispose();
      m_FileReader = null;
    }

    public async Task StartAsync(
      IFileSetting fileSetting,
      bool includeError,
      TimeSpan durationInitial,
      IProcessDisplay processDisplay,
      EventHandler<WarningEventArgs>? addWarning)
    {
      m_FileReader = FunctionalDI.GetFileReader(fileSetting, TimeZoneInfo.Local.Id, processDisplay);
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
      await m_FileReader.OpenAsync(processDisplay.CancellationToken).ConfigureAwait(false);

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
      await GetBatchByTimeSpan(durationInitial, includeError, processDisplay, m_SetDataTable).ConfigureAwait(false);

      m_SetLoadNextBatchAsync?.Invoke(
        process => GetBatchByTimeSpan(TimeSpan.MaxValue, includeError, process, dt => m_GetDataTable().Merge(dt)));

      m_ActionFinished?.Invoke(m_DataReaderWrapper);
    }

    private async Task GetBatchByTimeSpan(
      TimeSpan maxDuration,
      bool restoreError,
      IProcessDisplay processDisplay,
      Action<DataTable> action)
    {
      if (m_DataReaderWrapper is null)
        return;
      if (processDisplay is null)
        throw new ArgumentNullException(nameof(processDisplay));
      try
      {
        processDisplay.SetMaximum(100);
        var dt = await m_DataReaderWrapper.LoadDataTable(
                   maxDuration,
                   restoreError,
                   (l, i) => processDisplay.SetProcess($"Reading data...\nRecord: {l:N0}", i, false),
                   processDisplay.CancellationToken).ConfigureAwait(false);
        action.Invoke(dt);

        if (m_RefreshDisplayAsync != null)
          await m_RefreshDisplayAsync(FilterType.All, processDisplay.CancellationToken).ConfigureAwait(false);

        if (m_DataReaderWrapper.EndOfFile)
          Dispose();
      }
      catch (Exception e)
      {
        Logger.Error(e, nameof(TwoStepDataTableLoader) + "." + nameof(GetBatchByTimeSpan));
      }
    }
  }
}