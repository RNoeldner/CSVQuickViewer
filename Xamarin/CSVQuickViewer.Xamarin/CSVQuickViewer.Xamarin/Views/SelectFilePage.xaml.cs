using CSVQuickViewer.Xamarin.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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