using CSVQuickViewer.Xamarin.Models;
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
    public Command<Item> ItemTapped { get; }

    public ItemsViewModel()
    {
      Items = new ObservableCollection<Item>();
      LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
    }

    private async Task ExecuteLoadItemsCommand()
    {
      try
      {
        Items.Clear();
        foreach (var item in new[] {
                                      new Item { Id = Guid.NewGuid().ToString(), Text = "First item", Description="This is an item description." },
                                      new Item { Id = Guid.NewGuid().ToString(), Text = "Second item", Description="This is an item description." },
                                      new Item { Id = Guid.NewGuid().ToString(), Text = "Third item", Description="This is an item description." },
                                      new Item { Id = Guid.NewGuid().ToString(), Text = "Fourth item", Description="This is an item description." },
                                      new Item { Id = Guid.NewGuid().ToString(), Text = "Fifth item", Description="This is an item description." },
                                      new Item { Id = Guid.NewGuid().ToString(), Text = "Sixth item", Description="This is an item description." }
                                    })
        {
          Items.Add(item);
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex);
      }
    }

    public void OnAppearing()
    {
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