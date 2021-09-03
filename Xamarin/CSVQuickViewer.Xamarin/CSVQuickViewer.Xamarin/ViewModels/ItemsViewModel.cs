using CSVQuickViewer.Xamarin.Models;
using CSVQuickViewer.Xamarin.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CSVQuickViewer.Xamarin.ViewModels
{
	public class ItemsViewModel : BaseViewModel
	{
		private Item _selectedItem;

		public ObservableCollection<Item> Items { get; }
		public Command LoadItemsCommand { get; }
		public Command AddItemCommand { get; }
		public Command<Item> ItemTapped { get; }

		public ItemsViewModel()
		{
			Title = "Browse";
			Items = new ObservableCollection<Item>();
			LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

		}

		async Task ExecuteLoadItemsCommand()
		{
			IsBusy = true;

			try
			{
				Items.Clear();
				var items = await DataStore.GetItemsAsync(true);
				foreach (var item in items)
				{
					Items.Add(item);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
			finally
			{
				IsBusy = false;
			}
		}

		public void OnAppearing()
		{
			IsBusy = true;
			SelectedItem = null;
		}

		public Item SelectedItem
		{
			get => _selectedItem;
			set
			{
				SetProperty(ref _selectedItem, value);
      }
		}

		
	}
}