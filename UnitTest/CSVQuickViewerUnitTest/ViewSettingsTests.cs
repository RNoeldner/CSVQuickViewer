using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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

      var test1FillGuessSettings = new FillGuessSettings(true);
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
      var output = UnitTestStatic.RunSerialize(test1);
      Assert.AreEqual(test1.AllowJson, output.AllowJson);
      Assert.AreEqual(test1.HtmlStyle.Style, output.HtmlStyle.Style);
    }

    [TestMethod]
    public void SerializeCheckViewSettingsTest()
    {
      var test1 = new ViewSettings { AllowJson = false, HtmlStyle = new HtmlStyle("Dummy") };
      // JsonIgnore
      var ignore = new List<string>() { 
        nameof(ViewSettings.InitialFolder), 
        nameof(ViewSettings.DefaultInspectionResult), 
        nameof(ViewSettings.WriteSetting), 
        nameof(ViewSettings.DurationTimeSpan) };

      UnitTestStatic.RunSerializeAllProps(test1, ignore);
    }

  }
}