using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;

namespace CsvTools.Tests
{
  [TestClass]
  public class ValidationResultTests
  {
    private readonly ValidationResult m_ValidationResult = new ValidationResult();

    [TestMethod]
    public void ValidationResultFileSizeTest()
    {
      m_ValidationResult.FileSize = 0;
      Assert.IsTrue(string.IsNullOrEmpty(m_ValidationResult.FileSizeDisplay));

      m_ValidationResult.FileSize = 12345;
      Assert.AreEqual(12345, m_ValidationResult.FileSize);
      Assert.IsNotNull(m_ValidationResult.FileSizeDisplay);
    }

    [TestMethod]
    public void ValidationResultTableNameTest()
    {
      var setting = new CsvFile { ID = "Hello" };

      setting.SetValidationResult(500, 1, 5);

      Assert.AreEqual(5, setting.ValidationResult.WarningCount);
    }

    [TestMethod]
    public void ValidationResultSetValidationResultTest()
    {
      m_ValidationResult.TableName = "Hallo";
      Assert.AreEqual("Hallo", m_ValidationResult.TableName);
    }

    [TestMethod]
    public void ValidationResultRatiosTest()
    {
      Assert.AreEqual(-1, new ValidationResult().ErrorCount);
      Assert.AreEqual(0, new ValidationResult().NumberRecords);
      m_ValidationResult.NumberRecords = 0;
      Assert.AreEqual(0, m_ValidationResult.ErrorRatio);
      Assert.AreEqual(0, m_ValidationResult.WarningRatio);

      m_ValidationResult.ErrorCount = 10;
      Assert.AreEqual(10, m_ValidationResult.ErrorCount);
      m_ValidationResult.WarningCount = 40;
      Assert.AreEqual(40, m_ValidationResult.WarningCount);

      m_ValidationResult.NumberRecords = 1000;
      Assert.AreEqual(1000, m_ValidationResult.NumberRecords);
      Assert.AreEqual(.01, m_ValidationResult.ErrorRatio);
      Assert.AreEqual(.04, m_ValidationResult.WarningRatio);
    }

    [TestMethod]
    public void ValidationResultNotifyPropertyChangedTest()
    {
      var property = string.Empty;
      m_ValidationResult.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
      {
        property += "-" + e.PropertyName + "-";
      };
      m_ValidationResult.NumberRecords = 1;
      m_ValidationResult.NumberRecords = 2;
      Assert.IsTrue(property.Contains("-NumberRecords-"));
    }
  }
}