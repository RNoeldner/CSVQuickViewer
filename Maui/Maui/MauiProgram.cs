#nullable enable
using CommunityToolkit.Maui;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.DataGrid.Hosting;

namespace Maui
{
  public static class MauiProgram
  {
    public static MauiApp CreateMauiApp()
    {
      var builder = MauiApp.CreateBuilder();
      builder
        .UseMauiApp<App>()
        
        .ConfigureFonts(fonts =>
        {
          fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
          fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        });
      // Initialise the toolkit
      builder.UseMauiApp<App>().UseMauiCommunityToolkit();

      builder.ConfigureSyncfusionCore();
      builder.ConfigureSyncfusionDataGrid();

      return builder.Build();
    }
  }
}