#nullable enable
using CsvTools;
using System.Data;

namespace Maui
{
  public class DataGridViewModel : BaseViewModel, IQueryAttributable
  {
    private string m_FileName = string.Empty;
    private DataTable? m_DataTable;
    private InspectionResult m_DetectionResult = new InspectionResult();

    //private PagedFileReader? m_FileReader;
    //public IList<DynamicDataRecord> Items => m_FileReader!;

    public DataTable DataTable
    {
      get => m_DataTable!;
      private set => SetProperty(ref m_DataTable, value);
    }

    public InspectionResult InspectionResult
    {
      get => m_DetectionResult;
      private set => SetProperty(ref m_DetectionResult, value);
    }

    public string FileName
    {
      get => m_FileName;
      set => SetProperty(ref m_FileName, value);
    }

    public Task OpenAsync() => OpenAsync(CancellationTokenSource.Token);

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
      if (!string.IsNullOrEmpty(FileName))
      {
        var setting = new PreferenceViewModel();
        await using IFileReader reader = InspectionResult.GetFileReader(null);
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
      if (query.ContainsKey("InspectionResult"))
        InspectionResult = (InspectionResult) query["InspectionResult"];
      else
        InspectionResult = fn.GetDetectionResult(CancellationTokenSource.Token).GetAwaiter().GetResult();
    }
  }
}