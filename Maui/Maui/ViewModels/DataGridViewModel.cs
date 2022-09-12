#nullable enable
using CsvTools;
using System.Data;

namespace Maui
{
  public class DataGridViewModel : BaseViewModel, IQueryAttributable
  {
    private string m_FileName = string.Empty;
    private DelimitedFileDetectionResult? m_DetectionResult;

    private PagedFileReader? m_FileReader;
    public IList<DynamicDataRecord> Items => m_FileReader!;

    public DataTable DataTable
    {
      get;
      private set;
    }

    public DelimitedFileDetectionResult Detection => m_DetectionResult!;

    public string FileName
    {
      get => m_FileName;
      set => SetProperty(ref m_FileName, value);
    }

    public async Task OpenAsync()
    {
      if (m_DetectionResult is null)
      {
        //var cpv = new ProgressTime();
        //cpv.ProgressChanged += (_, p) =>
        //{
        //  var toast = Toast.Make(p.Text, ToastDuration.Short, 14);
        //  toast.Show(CancellationTokenSource.Token);
        //};
        m_DetectionResult = await FileName.GetDetectionResultFromFile(false, true, true, true, true, true, false,
          true, CancellationTokenSource.Token);
      }

      using (IFileReader reader = CsvHelper.GetReaderFromDetectionResult(FileName, m_DetectionResult))
      {
        DataTable = reader.GetDataTable(TimeSpan.FromMinutes(2d), false, false, false, false, false,
          CancellationTokenSource.Token);
      }

      {
        var fr = new PagedFileReader(CsvHelper.GetReaderFromDetectionResult(FileName, m_DetectionResult), 20);
        await fr.OpenAsync(false, false, false, false, CancellationTokenSource.Token);
        SetProperty(ref m_FileReader, fr, nameof(Items));
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
        SetProperty(ref m_DetectionResult, (DelimitedFileDetectionResult) query["DetectionResult"], nameof(Detection));
    }
  }
}