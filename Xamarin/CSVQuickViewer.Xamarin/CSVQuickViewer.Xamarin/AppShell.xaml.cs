using CSVQuickViewer.Xamarin.ViewModels;
using CSVQuickViewer.Xamarin.Views;
using Foundation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;

namespace CSVQuickViewer.Xamarin
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();
		}

		private async void OnMenuItemClicked(object sender, EventArgs e)
		{
			await Shell.Current.GoToAsync("//SelectFilePage");
		}


  /*  public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
    {
      Settings.CurrentFile= url.Path;
      Shell.Current.GoToAsync("//SelectFilePage");
      return true;
    }
    */
  }
}
