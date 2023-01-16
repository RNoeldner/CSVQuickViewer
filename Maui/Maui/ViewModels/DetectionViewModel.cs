using CsvTools;

namespace Maui
{
  public class DetectionViewModel : BaseViewModel, IQueryAttributable
  {
    private string m_FileName = string.Empty;
    private DetectionResult m_DetectionResult = new DetectionResult("dummy");
    private string m_Log = string.Empty;

    public DetectionResult DetectionResult
    {
      get => m_DetectionResult;
      private set => SetProperty(ref m_DetectionResult, value);
    }

    public string FileName
    {
      get => m_FileName;
      set => SetProperty(ref m_FileName, value);
    }

    public string Log
    {
      get => m_Log;
      set => SetProperty(ref m_Log, value);
    }

    public async Task OpenAsync()
    {
      try
      {
        var preference = new PreferenceViewModel();
        Logger.LoggerInstance = new PassingLogger((s) =>
        {
          Log += s +"\n";
        });
        DetectionResult = await FileName.AnalyzeFileAsync(false, preference.GuessCodePage,
          preference.GuessEscapePrefix,
          preference.GuessDelimiter,
          preference.GuessQualifier,
          preference.GuessStartRow,
          preference.GuessHasHeader,
          false,
          preference.GuessComment,
          preference.GetFillGuessSettings(), CancellationTokenSource.Token);
      }
      finally
      {
        Logger.LoggerInstance = null;
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
    }
  }
}
