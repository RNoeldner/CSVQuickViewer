using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ExtensionTests
  {
    [TestMethod]
    public void GetIdFromFileName()
    {
      Assert.AreEqual("lo_data.csv", "lo_data_07102019_2303.csv".GetIdFromFileName());
      Assert.AreEqual("lo_data_100.csv", "lo_data_100.csv".GetIdFromFileName());

      Assert.AreEqual("lo_data.csv", "lo_data_201910072303.csv".GetIdFromFileName());
      Assert.AreEqual("lo_data.csv", "lo_data_20191007_11:03 pm.csv".GetIdFromFileName());
    }

    [TestMethod]
    public void ReplacePlaceholder()
    {
      var csv = new CsvFile("fileName")
      {
        ID = "12234"
      };

      Assert.AreEqual("This is a test 12234", "This is a test {Id}".ReplacePlaceholderWithPropertyValues(csv));
      Assert.AreEqual("This is fileName a test 12234", "This is {FileName} a test {Id}".ReplacePlaceholderWithPropertyValues(csv));
      Assert.AreEqual("This is {nonsense} a test", "This is {nonsense} a test".ReplacePlaceholderWithPropertyValues(csv));
    }
  }
}