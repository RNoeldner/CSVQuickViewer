using System;
using System.Data;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CsvTools
{
  public class DetailControlLoader : IDisposable
  {
    private readonly DetailControl m_DetailControl;
    private DataReaderWrapper m_DataReaderWrapper;
    private IFileReader m_FileReader;

    public DetailControlLoader(DetailControl detailControl) => m_DetailControl = detailControl;

    public void Dispose()
    {
      m_DataReaderWrapper?.Dispose();
      m_FileReader?.Close();
      m_FileReader?.Dispose();
      m_DataReaderWrapper = null;
      m_FileReader = null;
    }

    public async Task<DataTable> Start(IFileSetting fileSetting, long limit, TimeSpan maxDuration, IProcessDisplay processDisplay,
      EventHandler<WarningEventArgs> addWarning)
    {
      m_FileReader = FunctionalDI.GetFileReader(fileSetting, TimeZoneInfo.Local.Id, processDisplay);
      m_FileReader.Warning += addWarning;
      var warningList = new RowErrorCollection(m_FileReader);
      m_FileReader.Warning -= warningList.Add;
      await m_FileReader.OpenAsync(processDisplay.CancellationToken);
      warningList.HandleIgnoredColumns(m_FileReader);
      warningList.PassWarning += addWarning;

      m_DataReaderWrapper = new DataReaderWrapper(m_FileReader, fileSetting.RecordLimit, false,
        fileSetting.DisplayStartLineNo,
        fileSetting.DisplayEndLineNo,
        fileSetting.DisplayRecordNo);

      await m_DataReaderWrapper.OpenAsync(processDisplay.CancellationToken);

      m_DetailControl.DataTable = await m_DataReaderWrapper.GetEmptyDataTableAsync(fileSetting.DisplayStartLineNo,
        fileSetting.DisplayRecordNo, fileSetting.DisplayEndLineNo, false, processDisplay.CancellationToken);

      m_DetailControl.toolStripButtonNext.Enabled = false;
      await NextBatch(limit<1 ? long.MaxValue : limit, maxDuration, processDisplay);

      m_DetailControl.LoadNextBatchAsync = process => NextBatch(long.MaxValue, TimeSpan.MaxValue, process);
      m_DetailControl.EndOfFile = () =>
        m_DataReaderWrapper?.EndOfFile ?? true;

      m_DetailControl.toolStripButtonNext.Enabled = m_DataReaderWrapper != null;
      return m_DetailControl.DataTable;
    }

    private async Task NextBatch(long limit, TimeSpan maxDuration, [NotNull] IProcessDisplay processDisplay)
    {
      if (m_DataReaderWrapper == null)
        return;
      if (processDisplay == null)
        throw new ArgumentNullException(nameof(processDisplay));
      try
      {
        processDisplay.SetMaximum(100);
        await m_DataReaderWrapper.LoadDataTable(m_DetailControl.DataTable, limit, maxDuration, true,
          (l, i) => processDisplay.SetProcess($"Reading data...\nRecord: {l:N0}", i, false),
          processDisplay.CancellationToken);

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