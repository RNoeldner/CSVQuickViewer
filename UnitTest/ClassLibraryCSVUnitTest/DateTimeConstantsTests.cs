using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace CsvTools.Tests
{
  [TestClass()]
  public class DateTimeConstantsTests
  {
    [TestMethod]
    public void GeneralCultureInfoTest()
    {
      var german = new CultureInfo("de-DE");
      // make sure the DateFormat does show 
      Assert.AreEqual("dd.MM.yyyy", german.DateTimeFormat.ShortDatePattern);
      Assert.AreEqual("MM/dd/yyyy", CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern);

      CultureInfo.CurrentCulture = german;
      Assert.IsTrue(DateTimeConstants.CommonDateTimeFormats(string.Empty).Any(x => x == "dd/MM/yyyy"));

    }

  }
}