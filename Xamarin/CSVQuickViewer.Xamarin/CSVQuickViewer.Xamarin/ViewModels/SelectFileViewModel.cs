using CSVQuickViewer.Xamarin.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using CsvTools;

namespace CSVQuickViewer.Xamarin.ViewModels
{
	public class SelectFileViewModel : BaseViewModel
	{
		public Command LoginCommand { get; }
		public Command SelectFileCommand { get; }
		private string fileName = string.Empty; 

		public string FileName
		{
			get { return fileName; }
			set { SetProperty(ref fileName, value); }
		}
		public SelectFileViewModel()
		{
			LoginCommand = new Command(OnLoginClicked);
			SelectFileCommand = new Command(PickAndShow);
		}

		private async void PickAndShow(object obj)
		{

			var result = await FilePicker.PickAsync(PickOptions.Default);
			if (result != null)
			{
				FileName= result.FullPath;
			}		
		}

		private async void OnLoginClicked(object obj)
		{
			if (FileSystemUtils.FileExists(fileName))
			  // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
				await Shell.Current.GoToAsync($"//{nameof(ItemsPage)}");
		}
	}
}
