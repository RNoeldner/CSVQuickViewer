#nullable enable
using CsvTools;
using System.Data;

namespace Maui
{
  public class DataGridViewModel : BaseViewModel, IQueryAttributable
  {
    private string m_FileName = string.Empty;
    private DelimitedFileDetectionResult? m_DetectionResult;
    private DelimitedFileDetectionResultWithColumns? m_DetectionResultWithColumns;
    private DataTable? m_DataTable;

    //private PagedFileReader? m_FileReader;
    //public IList<DynamicDataRecord> Items => m_FileReader!;

    public DataTable DataTable
    {
      get => m_DataTable;
      private set => SetProperty(ref m_DataTable, value);
    }

    public DelimitedFileDetectionResult Detection
    {
      get
      {
        m_DetectionResult ??= FileName.GetDetectionResultFromFile(false, true, true, true, true, true, false,
          true, CancellationTokenSource.Token).GetAwaiter().GetResult();
        return m_DetectionResult;
      }
      private set => SetProperty(ref m_DetectionResult, value);
    }

    public string FileName
    {
      get => m_FileName;
      set => SetProperty(ref m_FileName, value);
    }

    private async Task<IReadOnlyCollection<IColumn>> GetColumns()
    {
      await using IFileReader reader = CsvHelper.GetReaderFromDetectionResult(FileName, Detection);

      await reader.OpenAsync(CancellationTokenSource.Token);
      var (_, b) = await reader.FillGuessColumnFormatReaderAsyncReader(
        new FillGuessSettings(),
        null,
        false,
        true,
        "NULL",
        CancellationTokenSource.Token);
      return b;
    }

    public async Task OpenAsync()
    {
      if (!string.IsNullOrEmpty(FileName))
      {
        var setting = new PreferenceViewModel();
        await using IFileReader reader = CsvHelper.GetReaderFromDetectionResult(FileName, Detection, await GetColumns());
        await reader.OpenAsync(CancellationTokenSource.Token);
        DataTable = reader.GetDataTable(TimeSpan.FromMinutes(2d), false, setting.DisplayStartLineNo, setting.DisplayRecordNo, false, false,
          CancellationTokenSource.Token);
      }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
      if (!query.ContainsKey("FileName"))
        return;

      var fn = query["FileName"] as string;
      if (string.IsNullOrEmpty(fn))
        return;

      FileName = fn;
      if (query.ContainsKey("DetectionResult"))
        Detection = (DelimitedFileDetectionResult) query["DetectionResult"];
    }
  }
}