using System.IO;
using System.Threading.Tasks;

namespace CSVQuickViewer.Xamarin.Services
{
  public interface IDeviceDependentService
  {
    Task<Stream> GetStreamFromFileNameAsync(string fileName);
  }
}