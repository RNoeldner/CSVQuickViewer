#nullable enable
using CommunityToolkit.Maui;
using Syncfusion.Maui.Core.Hosting;

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
      Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NzE1MjcyQDMyMzAyZTMyMmUzMGFGaTZnK0FmbDhhMjRZSlpGUVV0UmRoNGFwT3diRUxGZWZWM1JXcmFYRE09");
      builder.ConfigureSyncfusionCore();

      return builder.Build();
    }
  }
}