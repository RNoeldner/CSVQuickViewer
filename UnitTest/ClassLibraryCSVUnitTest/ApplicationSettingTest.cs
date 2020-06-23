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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ApplicationSettingTest
  {
    [TestMethod]
    public void ApplicationSettingStatics()
    {
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
        FunctionalDI.SQLDataReader = null;
      }
      catch (ArgumentNullException)
      {
        // all good
      }

      FunctionalDI.SQLDataReader = UnitTestInitialize.MimicSQLReader.ReadDataAsync;
      var readerAsync = FunctionalDI.SQLDataReader;
      Assert.IsNotNull(readerAsync);
    }

    [TestMethod]
    public void GetMappingByFieldTest()
    {
      var csv = new CsvFile();
      Assert.IsNull(csv.MappingCollection.GetByField(""));
      Assert.IsNull(csv.MappingCollection.GetByField("Hello"));

      var map = new Mapping("Column","Field");
      csv.MappingCollection.Add(map);
      Assert.AreEqual(map, csv.MappingCollection.GetByField("Field"));
    }
  }
}