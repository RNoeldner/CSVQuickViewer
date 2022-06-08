namespace Maui
{
  public partial class MainPage : ContentPage
  {

    public MainPage()
    {
      InitializeComponent();
    }

    private async void OpenFile_ClickedAsync(object sender, EventArgs e)
    {
      var options = new PickOptions()
      {
        PickerTitle="Deleimted Text Files",
        FileTypes=  new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
          {
            { DevicePlatform.iOS, new[] { "public.my.comic.extension" } },
            { DevicePlatform.Android, new[] { "text/csv", "text/plain" } },
            { DevicePlatform.WinUI, new[] { "csv", "tab" , "txt"} },
            { DevicePlatform.Tizen, new[] { "*/*" } },
            { DevicePlatform.macOS, new[] { "csv", "tab" , "txt"} } })
      };
      var result = await FilePicker.Default.PickAsync(options);
      if (result != null)
      {
        OpenFile.Text = result.FileName;
      }
    }
  }
}