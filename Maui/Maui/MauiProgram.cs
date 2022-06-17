using CommunityToolkit.Maui;

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
      return builder.Build();
    }
  }
}