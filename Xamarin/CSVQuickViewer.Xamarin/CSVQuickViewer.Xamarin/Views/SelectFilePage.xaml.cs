using CSVQuickViewer.Xamarin.Services;
using CSVQuickViewer.Xamarin.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSVQuickViewer.Xamarin.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SelectFilePage : ContentPage
	{
		public SelectFilePage()
		{
			InitializeComponent();
      this.BindingContext = new SelectFileViewModel();
		}
	}
}