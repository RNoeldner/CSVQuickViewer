using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CsvTools
{
  public class DetailControlLoader : IDisposable
  {
    private readonly DetailControl m_DetailControl;
    private DataReaderWrapper m_DataReaderWrapper;
    private bool m_RestoreError;

    public DetailControlLoader(DetailControl detailControl) => m_DetailControl = detailControl;

    public void Dispose()
    {
      m_DataReaderWrapper?.Dispose();
      m_DataReaderWrapper = null;
    }

    public async Task StartAsync([NotNull] IFileSetting fileSetting, bool includeError, TimeSpan durationInitial,
      [NotNull] IProcessDisplay processDisplay, [CanBeNull] EventHandler<WarningEventArgs> addWarning)
    {
      using (var fileReader = FunctionalDI.GetFileReader(fileSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        m_RestoreError = includeError;
        RowErrorCollection warningList = null;
        if (addWarning != null)
        {
          warningList = new RowErrorCollection(fileReader);
          fileReader.Warning += addWarning;
          fileReader.Warning -= warningList.Add;
        }

        await fileReader.OpenAsync(processDisplay.CancellationToken).ConfigureAwait(false);

        if (addWarning != null)
        {
          warningList.HandleIgnoredColumns(fileReader);
          warningList.PassWarning += addWarning;
        }

        m_DataReaderWrapper = new DataReaderWrapper(fileReader, fileSetting.RecordLimit, includeError,
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
    }

    private async Task NextBatch(TimeSpan maxDuration, [NotNull] IProcessDisplay processDisplay)
    {
      if (m_DataReaderWrapper == null)
        return;
      if (processDisplay == null)
        throw new ArgumentNullException(nameof(processDisplay));
      try
      {
        processDisplay.SetMaximum(100);
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