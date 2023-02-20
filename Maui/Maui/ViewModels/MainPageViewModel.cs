#nullable enable

using CsvTools;
using System.Windows.Input;

namespace Maui
{
  public class MainPageViewModel : BaseViewModel
  {

    #region Constructor
    public MainPageViewModel()
    {
      SelectFileCommand = new Command(execute: async () => await SelectFileAsync(), () => SelectEnabled);
    }

    #endregion
    private bool m_SelectEnabled = true;
    public bool SelectEnabled { get => m_SelectEnabled; set => base.SetProperty(ref m_SelectEnabled, value); }

    #region Properties
    string m_SelectedFile = "Select File";
    public string SelectedFile
    {
      get { return m_SelectedFile; }
      set { SetProperty(ref m_SelectedFile, value); }
    }

    #endregion

    #region Commands

    public ICommand SelectFileCommand { get; set; }

    async Task OpenFile(string fullPath)
    {
      try
      {
        await Shell.Current.GoToAsync("detect?FileName=" + fullPath);
      }
      catch (Exception exc)
      {
        await Application.Current?.MainPage?.DisplayAlert("Error", exc.Message, "OK")!;
      }
    }

    async Task SelectFileAsync()
    {
      try
      {
        SelectEnabled  = false;

        var options = new PickOptions
        {
          PickerTitle = "Delimited Text Files",
          FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
          {
            { DevicePlatform.iOS, new[] { "public.text", "UTType.Text" } },
            { DevicePlatform.Android, new[] { "text/csv", "text/plain", "application/x-gzip" } },
            { DevicePlatform.WinUI, new[] { "csv", "tab", "txt", "gz" } },
            { DevicePlatform.Tizen, new[] { "*/*" } },
            { DevicePlatform.macOS, new[] { "csv", "tab", "txt", "gz" } }
          })
        };
        var result = await FilePicker.Default.PickAsync(options);

        if (result != null && FileSystemUtils.FileExists(result.FullPath))
          await OpenFile(result.FullPath);
      }
      catch (Exception exc)
      {
        await Application.Current?.MainPage?.DisplayAlert("Error", exc.Message, "OK")!;
      }
      finally
      {
        SelectEnabled  = true;
      }

    }
    #endregion
  }
}
