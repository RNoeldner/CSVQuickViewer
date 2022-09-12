#nullable enable

namespace Maui
{
  public partial class AppShell : Shell
  {
    public AppShell()
    {
      InitializeComponent();
      Routing.RegisterRoute("showfile", typeof(DataGrid));
    }
  }
}