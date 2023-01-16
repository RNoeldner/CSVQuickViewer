#nullable enable
namespace Maui
{
  public partial class MainPage : ContentPage
  {
    private static m_IsHandled  = false;
    public MainPage()
    {
      InitializeComponent();
    }

    private async void MainPage_OnLoaded(object? sender, EventArgs e)
    {
      if (!m_IsHandled)
      {
        var goodArgs = AppInstance.GetCurrent().GetActivatedEventArgs();

        if (goodArgs.Kind == ExtendedActivationKind.File && goodArgs.Data is IFileActivatedEventArgs fileActivatedEventArgs)
        {
          var fn = fileActivatedEventArgs.Select(file => file.Path).First();
          if (FileSystemUtils.FileExists(fn))
            // this is started but not awaited
            await ViewModel.OpenAsync(fn);
        }
        m_IsHandled = true;
      }
    }

  }
}