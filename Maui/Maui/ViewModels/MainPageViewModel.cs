
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CsvTools;
using Maui.Annotations;
using System.Windows.Input;

namespace Maui
{
  public class MainPageViewModel : BaseViewModel
  {

    #region Constructor
    public MainPageViewModel()
    {
      ConnectCommand = new Command(async () => await SelectFileAsync(), () => !InProgress);
    }
    #endregion
    private bool InProgress { get; set; }

    #region Properties
    string m_SelectedFile = "Select File";
    public string SelectedFile
    {
      get { return m_SelectedFile; }
      set { SetProperty(ref m_SelectedFile, value); }
    }
    [CanBeNull] private DelimitedFileDetectionResult m_DetectionResult;

    public DelimitedFileDetectionResult DetectionResult
    {
      get { return m_DetectionResult; }
      set { SetProperty(ref m_DetectionResult, value); }
    }

    #endregion

    #region Commands

    public ICommand ConnectCommand { get; set; }

    async Task SelectFileAsync()
    {
      try
      {
        InProgress = true;


        var options = new PickOptions()
        {
          PickerTitle="Delimited Text Files",
          FileTypes=  new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
          {
            { DevicePlatform.iOS, new[] { "public.text",  "UTType.Text"  } },
            { DevicePlatform.Android, new[] { "text/csv", "text/plain", "application/x-gzip" } },
            { DevicePlatform.WinUI, new[] { "csv", "tab" , "txt", "gz"} },
            { DevicePlatform.Tizen, new[] { "*/*" } },
            { DevicePlatform.macOS, new[] { "csv", "tab" , "txt", "gz"} } })
        };
        var result = await FilePicker.Default.PickAsync(options);

        if (result != null)
        {
          IsBusy = true;


          if (FileSystemUtils.FileExists(result.FullPath))
          {
            SelectedFile = result.FileName;
            var cpv = new CustomProcessDisplay();
            cpv.Progress += (o, p) =>
            {
              ProgressInfo = p.Text;
              /*
                var toast = Toast.Make(p.Text, ToastDuration.Short, 14);
                toast.Show(CancellationTokenSource.Token);
              */
            };
            DetectionResult = await result.FullPath.GetDetectionResultFromFile(false, true, true, true, true, true, false, true, CancellationTokenSource.Token);

            await Shell.Current.GoToAsync("showfile?FileName=" + result.FullPath, new Dictionary<string, object> { { "DetectionResult", DetectionResult } });
          }
        }

      }
      catch (Exception exc)
      {
        await Application.Current?.MainPage?.DisplayAlert("Error", exc.Message, "OK")!;
      }
      finally
      {
        InProgress = false;
        IsBusy = false;
      }

    }
    #endregion
  }
}
