/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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