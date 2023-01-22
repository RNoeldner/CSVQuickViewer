#nullable enable
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CsvTools;

namespace Maui;

public partial class Detection : ContentPage
{
  public Detection()
  {
    InitializeComponent();
  }

  private async void Detection_OnLoaded(object? sender, EventArgs e)
  {
    try
    {
      IsBusy = true;
      await ViewModel.OpenAsync();
      IsBusy = false;

      await Shell.Current.GoToAsync("showfile?FileName=" + ViewModel.FileName,
        new Dictionary<string, object> { { "InspectionResult", ViewModel.InspectionResult } });
    }
    catch (Exception exception)
    {
      var toast = Toast.Make(exception.InnerExceptionMessages(), ToastDuration.Long, 14);
      await toast.Show();
    }
  }
}