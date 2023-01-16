#nullable enable
using CsvTools;
using System.Data;

namespace Maui
{
  public class DataGridViewModel : BaseViewModel, IQueryAttributable
  {
    private string m_FileName = string.Empty;
    private DetectionResult? m_DetectionResult;
    private DataTable? m_DataTable;

    //private PagedFileReader? m_FileReader;
    //public IList<DynamicDataRecord> Items => m_FileReader!;

    public DataTable DataTable
    {
      get => m_DataTable;
      private set => SetProperty(ref m_DataTable, value);
    }

    public DetectionResult Detection
    {
      get
      {
        m_DetectionResult ??= FileName.GetDetectionResultFromFile(false, true,
          true, true, true, true, true, false,
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

    private async Task<IReadOnlyCollection<Column>> GetColumns()
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

    public Task OpenAsync() => OpenAsync(CancellationTokenSource.Token);

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
      if (!string.IsNullOrEmpty(FileName))
      {
        var setting = new PreferenceViewModel();
        await using IFileReader reader = CsvHelper.GetReaderFromDetectionResult(FileName, Detection, await GetColumns());
        await reader.OpenAsync(cancellationToken);

        await using var wrapper = new DataReaderWrapper(
            reader,
            false,
            setting.DisplayStartLineNo,
            false,
            setting.DisplayRecordNo);

        m_DataTable= await wrapper.GetDataTableAsync(TimeSpan.FromMinutes(2d), true, null, cancellationToken);
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
        Detection = (DetectionResult) query["DetectionResult"];
    }
  }
}