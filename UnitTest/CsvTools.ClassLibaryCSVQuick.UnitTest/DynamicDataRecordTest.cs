using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class DynamicDataRecordTest
  {
    [TestMethod()]
    public async Task GetDynamicMemberNames()
    {
      TimeZoneChangeDelegate m_TimeZoneAdjust = StandardTimeZoneAdjust.ChangeTimeZone;
      using (var reader = new CsvFileReader(UnitTestStatic.GetTestPath("AllFormats.txt"),
               65001, 0, true, null, TrimmingOptionEnum.Unquoted, "TAB",
               "\"", "", 0, false,
               false, "", 0,
               true, "", "",
               "", true, false,
               true, false, false,
               false, false, false, false,
               false, false, "",
               true, 1, "ID", m_TimeZoneAdjust, System.TimeZoneInfo.Local.Id, true, false))
      {
        await reader.OpenAsync(UnitTestStatic.Token);
        await reader.ReadAsync();

        dynamic test = new DynamicDataRecord(reader);
        //Assert.AreEqual(-22477, test.Integer);

        await reader.ReadAsync();
        test = new DynamicDataRecord(reader);
        //Assert.AreEqual("zehzcv", test.String);
        //test.String = "2222";
        //Assert.AreEqual("2222", test.String);
      }
    }
  }
}