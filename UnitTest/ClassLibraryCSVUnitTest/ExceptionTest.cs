using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public sealed class ExceptionTest
  {
    [TestMethod]
    public void ConversionException()
    {
      var ex1 = new ConversionException("MyMessage1");
      var ex2 = new ConversionException("MyMessage2", ex1);
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
    public void FileReaderExceptionOpen()
    {
      var ex1 = new FileReaderOpenException("MyMessage1");
      var ex2 = new FileReaderOpenException("MyMessage2", ex1);
      Assert.AreEqual(ex1, ex2.InnerException);
      Assert.AreNotEqual("MyMessage2", ex2.Message);
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