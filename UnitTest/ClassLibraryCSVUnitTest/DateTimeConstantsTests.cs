/*
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
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace CsvTools.Tests
{
  [TestClass()]
  public class DateTimeConstantsTests
  {
    [TestMethod]
    public void GeneralCultureInfoTest()
    {
      var german = new CultureInfo("de-DE");
      // make sure the DateFormat does show 
      Assert.AreEqual("dd.MM.yyyy", german.DateTimeFormat.ShortDatePattern);
      Assert.AreEqual("MM/dd/yyyy", CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern);

      CultureInfo.CurrentCulture = german;
      Assert.IsTrue(DateTimeConstants.CommonDateTimeFormats(string.Empty).Any(x => x == "dd/MM/yyyy"));

    }

  }
}
