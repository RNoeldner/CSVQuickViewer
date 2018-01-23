using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace CsvTools.Tests
{
  [TestClass()]
  public class TimeZoneMappingTests
  {
    [TestMethod()]
    public void GetAlternateNames()
    {
      var result1 = TimeZoneMapping.GetAlternateNames(TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
      Assert.IsTrue(result1.Contains("IST"));
      var result2 = TimeZoneMapping.GetAlternateNames(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
      Assert.IsTrue(result2.Contains("CET"));
    }

    [TestMethod()]
    public void WithSameRule()
    {
      var result = TimeZoneMapping.WithSameRule(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"), 2017);
      bool found = false;
      foreach (var item in result)
        if (item.Id.Equals("Central Europe Standard Time"))
          found = true;

      Assert.IsTrue(found);
    }

    [TestMethod()]
    public void GetTimeZone()
    {
      Assert.AreEqual("W. Europe Standard Time", TimeZoneMapping.GetTimeZone("W. Europe Standard Time").Id);
    }
  }
}