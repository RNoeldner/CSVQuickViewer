using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass]
  public class ApplicationSettingTest
  {
 
    [TestMethod]
    public void ApplicationSettingStatics()
    {
      Assert.IsTrue(ApplicationSetting.FillGuessSettings is FillGuessSettings);
      Assert.IsNotNull(ApplicationSetting.FillGuessSettings);
      Assert.IsTrue(ApplicationSetting.HTMLStyle is HTMLStyle);
      Assert.IsNotNull(ApplicationSetting.HTMLStyle);
    }


    [TestMethod]
    public void ApplicationSettingMenuDown()
    {
      ApplicationSetting.MenuDown = true;
      Assert.IsTrue(ApplicationSetting.MenuDown);
      ApplicationSetting.MenuDown = false;
      Assert.IsFalse(ApplicationSetting.MenuDown);
    }

    [TestMethod]
    public void SQLDataReaderText()
    {
      try
      {
        ApplicationSetting.SQLDataReader = null;
        var ignore = ApplicationSetting.SQLDataReader;
      }
      catch (ArgumentNullException)
      {
        // all good
      }
      ApplicationSetting.SQLDataReader = UnitTestInitialize.MimicSQLReader.ReadData;
      var reader = ApplicationSetting.SQLDataReader;
      Assert.IsNotNull(reader);
    }

  
    [TestMethod()]
    public void GetMappingByFieldTest()
    {
      var csv = new CsvFile();
      Assert.IsNull(csv.MappingCollection.GetByField(""));
      Assert.IsNull(csv.MappingCollection.GetByField("Hello"));

      var map = new Mapping()
      {
        FileColumn = "Column",
        TemplateField = "Field"
      };
      csv.MappingCollection.Add(map);
      Assert.AreEqual(map, csv.MappingCollection.GetByField("Field"));
    }
  }
}