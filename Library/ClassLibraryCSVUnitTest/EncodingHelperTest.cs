using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class EncodingHelperTest
  {
    [TestMethod]
    public void GuessCodePageBOM_GB18030()
    {
      byte[] test = { 132, 49, 149, 51 };
      Assert.AreEqual(54936, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOM_UTF16_BE()
    {
      byte[] test = { 254, 255, 65, 65 };
      Assert.AreEqual(1201, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOM_UTF16_LE()
    {
      byte[] test = { 255, 254, 65, 65 };
      Assert.AreEqual(1200, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOM_UTF32_BE()
    {
      byte[] test = { 0, 0, 254, 255 };
      Assert.AreEqual(12001, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOM_UTF32_LE()
    {
      byte[] test = { 255, 254, 0, 0 };
      Assert.AreEqual(12000, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOM_UTF7a()
    {
      byte[] test = { 43, 47, 118, 57 };
      Assert.AreEqual(65000, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOM_UTF7b()
    {
      byte[] test = { 43, 47, 118, 43 };
      Assert.AreEqual(65000, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOM_UTF7c()
    {
      byte[] test = { 43, 47, 118, 56, 45 };
      Assert.AreEqual(65000, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOM_UTF8()
    {
      byte[] test = { 239, 187, 191, 65 };
      Assert.AreEqual(65001, EncodingHelper.GetCodePageByByteOrderMark(test));
    }
  }
}