using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ExceptionTest
  {
    [TestMethod]
    public void ConfigurationException()
    {
      var ex1 = new ConfigurationException("MyMessage1");
      var ex2 = new ConfigurationException("MyMessage2", ex1);
      Assert.AreEqual(ex1, ex2.InnerException);
      Assert.AreEqual("MyMessage2", ex2.Message);
    }

    [TestMethod]
    public void ConversionException()
    {
      var ex1 = new ConversionException("MyMessage1");
      var ex2 = new ConversionException("MyMessage2", ex1);
      Assert.AreEqual(ex1, ex2.InnerException);
      Assert.AreEqual("MyMessage2", ex2.Message);
    }

    [TestMethod]
    public void EncryptionException()
    {
      var ex1 = new EncryptionException("MyMessage1");
      var ex2 = new EncryptionException("MyMessage2", ex1);
      Assert.AreEqual(ex1, ex2.InnerException);
      Assert.AreEqual("MyMessage2", ex2.Message);

    }

    [TestMethod]
    public void FileException()
    {
      var ex1 = new FileException("MyMessage1");
      var ex2 = new FileException("MyMessage2", ex1);
      Assert.AreEqual(ex1, ex2.InnerException);
      Assert.AreEqual("MyMessage2", ex2.Message);
    }
    

    [TestMethod]
    public void FileReaderException()
    {
      var ex1 = new FileReaderException("MyMessage1");
      var ex2 = new FileReaderException("MyMessage2", ex1);
      Assert.AreEqual(ex1, ex2.InnerException);
      Assert.AreEqual("MyMessage2", ex2.Message);
    }


    [TestMethod]
    public void FileWriterException()
    {
      var ex1 = new FileWriterException("MyMessage1");
      var ex2 = new FileWriterException("MyMessage2", ex1);
      Assert.AreEqual(ex1, ex2.InnerException);
      Assert.AreEqual("MyMessage2", ex2.Message);
    }
  }
}
