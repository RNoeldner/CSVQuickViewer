using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ViewSettingsTests
  {
  
    [TestMethod]
    public void PropertiesTest()
    {
      var test1 = new ViewSettings();

      Assert.IsFalse(test1.StoreSettingsByFile);
      test1.StoreSettingsByFile = true;
      Assert.IsTrue(test1.StoreSettingsByFile);

      var test1FillGuessSettings = new FillGuessSettings { CheckNamedDates = true };
      test1.FillGuessSettings = test1FillGuessSettings;

      Assert.AreEqual(test1FillGuessSettings, test1.FillGuessSettings);
    }
  }
}