using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass()]
  public class XmlFileTests
  {
    [TestMethod()]
    public void XmlFileTest()
    {
      var x = new XmlFile();
      Assert.AreEqual(string.Empty, x.FileName);
    }

    [TestMethod()]
    public void XmlFileTest1()
    {
      var x = new XmlFile("Test.txt","xml");
      Assert.AreEqual("Test.txt", x.FileName);
    }

    [TestMethod()]
    public void CloneTest()
    {
      var x = new XmlFile("Test1.txt","xml") { ByteOrderMark = true, CodePageId = 100} ;
      var x2 = x.Clone() as IXmlFile;
      Assert.AreEqual("Test1.txt", x2!.FileName);
      Assert.AreEqual(100, x2.CodePageId);
    }

    [TestMethod()]
    public void EqualsTest()
    {
      var x1 = new XmlFile("Test1.txt","xml") { ByteOrderMark = true, CodePageId = 100} ;
      var x2 = new XmlFile("Test1.txt","xml") { ByteOrderMark = true, CodePageId = 100} ;
      Assert.IsTrue(x1.Equals(x2));
    }
  }
}