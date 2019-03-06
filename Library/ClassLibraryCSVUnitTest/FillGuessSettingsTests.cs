using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class FillGuessSettingsTests
  {
    [TestMethod]
    public void CloneTest()
    {
      var a = new FillGuessSettings
      {
        CheckedRecords = 10,
        CheckNamedDates = true,        
        DectectNumbers = true,
        DectectPercentage = true,
        DetectBoolean = true,
        DetectDateTime = true,
        DetectGUID = true,
        FalseValue = "Never",
        TrueValue = "Always",
        IgnoreIdColums = false,
        MinSamplesForIntDate = 5,
        SampleValues = 5,
        SerialDateTime = true
      };
      var b = a.Clone();
      Assert.AreNotSame(b, a);
      a.AllPropertiesEqual(b);
    }

    [TestMethod]
    public void CopyToTest()
    {
      var a = new FillGuessSettings
      {
        CheckedRecords = 10,
        CheckNamedDates = true,        
        DectectNumbers = true,
        DectectPercentage = true,
        DetectBoolean = true,
        DetectDateTime = true,
        DetectGUID = true,
        FalseValue = "Never",
        TrueValue = "Always",
        IgnoreIdColums = false,
        MinSamplesForIntDate = 5,
        SampleValues = 5,
        SerialDateTime = true
      };

      var b = new FillGuessSettings
      {
        CheckedRecords = 11,
        CheckNamedDates = !a.CheckNamedDates,        
        DectectNumbers = !a.DectectNumbers,
        DectectPercentage = !a.DectectPercentage,
        DetectBoolean = !a.DetectBoolean,
        DetectDateTime = !a.DetectDateTime,
        DetectGUID = !a.DetectGUID,
        FalseValue = "false",
        TrueValue = "true",
        IgnoreIdColums = !a.IgnoreIdColums,
        MinSamplesForIntDate = a.MinSamplesForIntDate + 1,
        SampleValues = a.SampleValues + 2,
        SerialDateTime = false
      };

      a.CopyTo(b);
      Assert.AreNotSame(b, a);
      a.AllPropertiesEqual(b);
    }

    [TestMethod]
    public void NotifyPropertyChangedTest()
    {
      var a = new FillGuessSettings
      {
        CheckedRecords = 10,
        CheckNamedDates = true,        
        DectectNumbers = true,
        DectectPercentage = true,
        DetectBoolean = true,
        DetectDateTime = true,
        DetectGUID = true,
        FalseValue = "Never",
        TrueValue = "Always",
        IgnoreIdColums = false,
        MinSamplesForIntDate = 5,
        SampleValues = 5,
        SerialDateTime = true
      };
      var fired = false;
      a.PropertyChanged += delegate { fired = true; };
      Assert.IsFalse(fired);
      a.CheckedRecords = 11;
      Assert.IsTrue(fired);
    }
  }
}