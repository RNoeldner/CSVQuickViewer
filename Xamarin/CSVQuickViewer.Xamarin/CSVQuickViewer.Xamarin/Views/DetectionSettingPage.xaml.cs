using CSVQuickViewer.Xamarin.ViewModels;
using CsvTools;
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
	public partial class DetectionSettingPage : ContentPage
	{
		public DetectionSettingPage()
		{
			InitializeComponent();
			this.BindingContext = new DetectionSettingsViewModel();
		}
	}
}