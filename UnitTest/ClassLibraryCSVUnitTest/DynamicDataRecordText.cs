using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class DynamicDataRecordText
  {
    [TestMethod()]
    public async Task GetDynamicMemberNames()
    {
      using (var process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(UnitTestHelper.ReaderGetAllFormats(), process))
      {
        await reader.OpenAsync(UnitTestInitializeCsv.Token);
        await reader.ReadAsync();
                
        dynamic test = new DynamicDataRecord(reader);        
        Assert.AreEqual(-22477, test.Integer);

        await reader.ReadAsync();
        test = new DynamicDataRecord(reader);
        Assert.AreEqual("zehzcv", test.String);
      }
    }
  }
}
