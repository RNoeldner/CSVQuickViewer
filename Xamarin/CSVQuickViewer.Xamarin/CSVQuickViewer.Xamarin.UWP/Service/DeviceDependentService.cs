using CSVQuickViewer.Xamarin.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CSVQuickViewer.Xamarin.UWP.Service
{
  public class DeviceDependentService : IDeviceDependentService
  {
    public async Task<Stream> GetStreamFromFileNameAsync(string fileName)
    {
      var file = await StorageFile.GetFileFromPathAsync(fileName);
      if (file == null)
        return null;

      Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
      return await file.OpenStreamForReadAsync();
    }
  }
}
