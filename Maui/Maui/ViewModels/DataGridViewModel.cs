using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CsvTools;
using Maui.Annotations;

namespace Maui
{
  public class DataGridViewModel : BaseViewModel, IQueryAttributable
  {
    private string m_FileName = string.Empty;
    [CanBeNull] private DelimitedFileDetectionResult m_DetectionResult = null;
    [CanBeNull] private PagedFileReader m_FileReader;

    public IList<DynamicDataRecord> Items => m_FileReader!;
    public DelimitedFileDetectionResult Detection => m_DetectionResult;

    public string FileName
    {
      get => m_FileName;
      set => SetProperty(ref m_FileName, value);
    }

    public async Task OpenAsync()
    {
      if (m_DetectionResult is null)
      {
        var cpv = new CustomProcessDisplay();
        cpv.Progress += (o, p) =>
        {
          var toast = Toast.Make(p.Text, ToastDuration.Short, 14);
          toast.Show(CancellationTokenSource.Token);
        };
        m_DetectionResult = await FileName.GetDetectionResultFromFile(false, true, true, true, true, true, false,
          true, CancellationTokenSource.Token);
      }
      m_FileReader = new PagedFileReader(CsvHelper.GetReaderFromDetectionResult(FileName, m_DetectionResult), 20);
      await m_FileReader.OpenAsync(false, false, false, false, CancellationTokenSource.Token);

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
        m_DetectionResult = (DelimitedFileDetectionResult) query["DetectionResult"];
    }
  }
}
