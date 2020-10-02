using JetBrains.Annotations;
using System;
using System.Data;
using System.Threading.Tasks;

namespace CsvTools
{
  public class DetailControlLoader : IDisposable
  {
    private readonly DetailControl m_DetailControl;
    private DataReaderWrapper m_DataReaderWrapper;
    private IFileReader m_FileReader;
    private bool m_RestoreError;

    public DetailControlLoader(DetailControl detailControl) => m_DetailControl = detailControl;

    public void Dispose()
    {
      m_DataReaderWrapper?.Dispose();
      m_DataReaderWrapper = null;
      m_FileReader?.Dispose();
      m_FileReader=null;
    }

    public async Task StartAsync([NotNull] IFileSetting fileSetting, bool includeError, TimeSpan durationInitial,
      [NotNull] IProcessDisplay processDisplay, [CanBeNull] EventHandler<WarningEventArgs> addWarning)
    {
      m_FileReader = FunctionalDI.GetFileReader(fileSetting, TimeZoneInfo.Local.Id, processDisplay);

      m_RestoreError = includeError;
      RowErrorCollection warningList = null;
      if (addWarning != null)
      {
        warningList = new RowErrorCollection(m_FileReader);
        m_FileReader.Warning += addWarning;
        m_FileReader.Warning -= warningList.Add;
      }

      await m_FileReader.OpenAsync(processDisplay.CancellationToken).ConfigureAwait(false);

      if (addWarning != null)
      {
        warningList.HandleIgnoredColumns(m_FileReader);
        warningList.PassWarning += addWarning;
      }

      m_DataReaderWrapper = new DataReaderWrapper(m_FileReader, fileSetting.RecordLimit, includeError,
        fileSetting.DisplayStartLineNo, fileSetting.DisplayRecordNo, fileSetting.DisplayEndLineNo);

      m_DetailControl.DataTable = m_DataReaderWrapper.GetEmptyDataTable();

      m_DetailControl.toolStripButtonNext.Enabled = false;
      await NextBatch(durationInitial, processDisplay).ConfigureAwait(false);

      m_DetailControl.LoadNextBatchAsync = process => NextBatch(TimeSpan.MaxValue, process);
      m_DetailControl.EndOfFile = () =>
        m_DataReaderWrapper?.EndOfFile ?? true;

      m_DetailControl.toolStripButtonNext.Visible = m_DataReaderWrapper != null;
      m_DetailControl.toolStripButtonNext.Enabled = m_DataReaderWrapper != null;

    }

    public async Task StartAsync([NotNull] IDataReader dataReader, TimeSpan durationInitial,
          [NotNull] IProcessDisplay processDisplay)
    {
      m_RestoreError=true;
      m_DataReaderWrapper = new DataReaderWrapper(dataReader, dataReader.RecordsAffected, false, false, false, false);

      m_DetailControl.DataTable = m_DataReaderWrapper.GetEmptyDataTable();

      m_DetailControl.toolStripButtonNext.Enabled = false;
      await NextBatch(durationInitial, processDisplay).ConfigureAwait(false);

      m_DetailControl.LoadNextBatchAsync = process => NextBatch(TimeSpan.MaxValue, process);
      m_DetailControl.EndOfFile = () =>
        m_DataReaderWrapper?.EndOfFile ?? true;

      m_DetailControl.toolStripButtonNext.Visible = m_DataReaderWrapper != null;
      m_DetailControl.toolStripButtonNext.Enabled = m_DataReaderWrapper != null;
    }

    private async Task NextBatch(TimeSpan maxDuration, [NotNull] IProcessDisplay processDisplay)
    {
      if (m_DataReaderWrapper == null)
        return;
      if (processDisplay == null)
        throw new ArgumentNullException(nameof(processDisplay));
      try
      {
        processDisplay.SetMaximum(-1);
        await m_DataReaderWrapper.LoadDataTable(m_DetailControl.DataTable, maxDuration, m_RestoreError,
          (l, i) => processDisplay.SetProcess($"Reading data...\nRecord: {l:N0}", i, false),
          processDisplay.CancellationToken).ConfigureAwait(false);

        await m_DetailControl.RefreshDisplayAsync(FilterType.All, processDisplay.CancellationToken)
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