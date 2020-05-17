using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class EnumDescriptionConverterTests
  {
    [TestMethod()]
    public void EnumDescriptionConverterTest()
    {
      var test = new EnumDescriptionConverter(typeof(RecordDelimiterType));
      Assert.IsNotNull(test);
    }

    [TestMethod()]
    public void CanConvertFromTest()
    {
      var test = new EnumDescriptionConverter(typeof(RecordDelimiterType));
      Assert.IsTrue(test.CanConvertFrom(typeof(string)));
    }

    [TestMethod()]
    public void CanConvertToTest()
    {
      var test = new EnumDescriptionConverter(typeof(RecordDelimiterType));
      Assert.IsTrue(test.CanConvertTo(typeof(string)));
    }

    [TestMethod()]
    public void ConvertFromTest()
    {
      var test = new EnumDescriptionConverter(typeof(RecordDelimiterType));
      Assert.AreEqual(RecordDelimiterType.LF, test.ConvertFrom("Line feed"));
    }

    [TestMethod()]
    public void ConvertToTest()
    {
      var test = new EnumDescriptionConverter(typeof(RecordDelimiterType));
      Assert.AreEqual("Line feed", test.ConvertTo(RecordDelimiterType.LF, typeof(string)));
      try
      {
        test.ConvertTo(null, typeof(string));
      }
      catch (ArgumentNullException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail($"Wrong Exception Type {ex.GetType()}, Invalid Filename");
      }
    }
  }
}