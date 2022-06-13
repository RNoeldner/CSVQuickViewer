using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CsvTools;
using Maui.Annotations;

namespace Maui
{
  public class DataGridViewModel : BaseViewModel, IQueryAttributable
  {
    private string m_FileName = string.Empty;
    [CanBeNull] private DelimitedFileDetectionResult detectionResult = null;
    public string FileName
    {
      get => m_FileName;
      set => SetProperty(ref m_FileName, value);
    }

    public async Task OpenAsync()
    {
      if (this.detectionResult is null)
      {
        var cpv = new CustomProcessDisplay();
        cpv.Progress += (o, p) =>
        {
          var toast = Toast.Make(p.Text, ToastDuration.Short, 14);
          toast.Show(CancellationTokenSource.Token);
        };
        detectionResult = await FileName.GetDetectionResultFromFile(cpv, false, true, true, true, true, true, false,
          true, CancellationTokenSource.Token);
      }

      var m_Reader = CsvHelper.GetReaderFromDetectionResult(FileName, detectionResult, null);
      await m_Reader.OpenAsync(CancellationTokenSource.Token);
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
        detectionResult = query["DetectionResult"] as DelimitedFileDetectionResult;
    }
  }
}
