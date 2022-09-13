#nullable enable
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CsvTools;

namespace Maui;

public partial class DataGrid : ContentPage
{
  public DataGrid()
  {
    InitializeComponent();
  }

  private async void DataGrid_OnLoaded(object? sender, EventArgs e)
  {
    try
    {
      IsBusy = true;
      await ViewModel.OpenAsync();
      IsBusy = false;
      dataGrid.ItemsSource = ViewModel.DataTable;
    }
    catch (Exception exception)
    {
      var toast = Toast.Make(exception.InnerExceptionMessages(), ToastDuration.Long, 14);
      await toast.Show();
    }
  }
}