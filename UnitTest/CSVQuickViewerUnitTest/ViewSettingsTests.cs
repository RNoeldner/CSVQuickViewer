using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ViewSettingsTests
  {
    [TestMethod]
    public void CopyConfigurationTest()
    {
      var test1 = new ViewSettings();
      var test2 = new CsvFile();
      var test3 = new ViewSettings();

      Assert.IsFalse(test1.WarnDelimiterInValue);
      test1.WarnDelimiterInValue = true;
      ViewSettings.CopyConfiguration(test1, test2, true);
      Assert.IsTrue(test2.WarnDelimiterInValue);

      test3.WarnDelimiterInValue = false;
      ViewSettings.CopyConfiguration(test3, test2, false);
      Assert.IsFalse(test2.WarnDelimiterInValue);
    }

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