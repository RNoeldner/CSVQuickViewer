using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class EnumDescriptionConverterTests
  {
    [TestMethod]
    public void EnumDescriptionConverterTest()
    {
      Assert.IsNotNull(new EnumDescriptionConverter(typeof(TestEnum)));
    }

    [TestMethod]
    public void CanConvertFromTest()
    {
      var conv = new EnumDescriptionConverter(typeof(TestEnum));
      Assert.IsTrue(conv.CanConvertFrom(typeof(string)));
    }

    [TestMethod]
    public void CanConvertToTest()
    {
      var conv = new EnumDescriptionConverter(typeof(TestEnum));
      Assert.IsTrue(conv.CanConvertTo(typeof(string)));
    }

    [TestMethod]
    public void ConvertFromTest()
    {
      var conv = new EnumDescriptionConverter(typeof(TestEnum));
      Assert.AreEqual(TestEnum.Value1, conv.ConvertFromString("Value1"));
    }

    [TestMethod]
    public void ConvertToTest()
    {
      var conv = new EnumDescriptionConverter(typeof(TestEnum));
      Assert.AreEqual("Value1", conv.ConvertToString(TestEnum.Value1));
    }

    /// <summary>
    ///   Error Placement
    /// </summary>
    private enum TestEnum
    {
      Value1 = 0,
      Value2 = 1,
      AnotherValue = 2,
      YetAnotherValue = 3
    }
  }
}