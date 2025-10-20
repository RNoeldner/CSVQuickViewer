﻿/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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


namespace CsvTools.Tests
{
  [TestClass()]
  public class ViewerFileReaderWriterFactoryTests
  {

    [TestMethod]
    public void GetFileReaderTestJson()
    {
      var setting = new CsvFileDummy() { FileName= UnitTestStatic.GetTestPath("AllFormatsPipe.csv"), IsJson= true, };
      var fact = new ViewerFileReaderWriterFactory(StandardTimeZoneAdjust.ChangeTimeZone, new FillGuessSettings(true));
      using var test2 = fact.GetFileReader(setting, UnitTestStatic.Token);
      Assert.IsInstanceOfType(test2, typeof(JsonFileReader));
    }

    [TestMethod]
    public void GetFileReaderTestXml()
    {
      var setting = new CsvFileDummy() { FileName= UnitTestStatic.GetTestPath("AllFormatsPipe.csv"), IsXml= true, };
      var fact = new ViewerFileReaderWriterFactory(StandardTimeZoneAdjust.ChangeTimeZone, new FillGuessSettings(true));
      using var test2 = fact.GetFileReader(setting, UnitTestStatic.Token);
      Assert.IsInstanceOfType(test2, typeof(XmlFileReader));
    }
  }
}
