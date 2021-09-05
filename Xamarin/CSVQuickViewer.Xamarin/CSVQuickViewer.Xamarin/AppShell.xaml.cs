using CSVQuickViewer.Xamarin.ViewModels;
using CSVQuickViewer.Xamarin.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    public async Task LoadFile(string fileName)
    {
      Settings.CurrentFile = fileName;
       
    }
  }
}
