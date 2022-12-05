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

      var test1FillGuessSettings = new FillGuessSettings(checkNamedDates: true);
      test1.FillGuessSettings = test1FillGuessSettings;

      Assert.IsTrue(test1FillGuessSettings.Equals(test1.FillGuessSettings));
    }

    [TestMethod]
    public void SerializeCheckPropsFillGuessSettingsTest()
    {
      var test1 = new FillGuessSettings(true);
      UnitTestStatic.RunSerializeAllProps(test1);
    }

    [TestMethod]
    public void SerializeViewSettingsTest()
    {
      var test1 = new ViewSettings { AllowJson = false, HtmlStyle = new HtmlStyle("Dummy") };
      var output = UnitTestStatic.RunSerialize(test1, true, true);
      Assert.AreEqual(test1.AllowJson, output.AllowJson);
      Assert.AreEqual(test1.HtmlStyle.Style, output.HtmlStyle.Style);
    }

    [TestMethod]
    public void SerializeCheckViewSettingsTest()
    {
      var test1 = new ViewSettings { AllowJson = false, HtmlStyle = new HtmlStyle("Dummy") };
      UnitTestStatic.RunSerializeAllProps(test1);
    }

  }
}