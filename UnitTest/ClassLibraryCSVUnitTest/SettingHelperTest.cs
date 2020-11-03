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
using System.Linq;
using System.Text;

namespace CsvTools.Tests
{
  [TestClass]
  public class HelperClassesTest
  {
    [TestMethod]
    public void CommonCodePages()
    {
      Assert.IsTrue(EncodingHelper.CommonCodePages.Contains(1252), "Windows");
      Assert.IsTrue(EncodingHelper.CommonCodePages.Contains(1200), "UTF-16");
      Assert.IsTrue(EncodingHelper.CommonCodePages.Contains(65000), "UTF-8");
    }

    [TestMethod]
    public void GetEncoding()
    {
      Assert.IsInstanceOfType(EncodingHelper.GetEncoding(1200, true), typeof(UnicodeEncoding));
      Assert.IsInstanceOfType(EncodingHelper.GetEncoding(1201, true), typeof(UnicodeEncoding));
      Assert.IsInstanceOfType(EncodingHelper.GetEncoding(12000, false), typeof(UTF32Encoding));
      Assert.IsInstanceOfType(EncodingHelper.GetEncoding(12001, false), typeof(UTF32Encoding));
      Assert.IsInstanceOfType(EncodingHelper.GetEncoding(1252, true), typeof(Encoding));
    }

    [TestMethod]
    public void GetEncodingName()
    {
      Assert.IsTrue(EncodingHelper.GetEncodingName(1200, false, false).Contains("UTF-16"));
      Assert.IsTrue(EncodingHelper.GetEncodingName(1201, true, false).Contains("UTF-16"));
      Assert.IsTrue(EncodingHelper.GetEncodingName(12000, true, true).Contains("UTF-32"));
      Assert.IsTrue(EncodingHelper.GetEncodingName(437, true, true).Contains("MS-DOS"));
      Assert.IsTrue(EncodingHelper.GetEncodingName(1252, false, false).Contains("1252"));
      Assert.IsTrue(EncodingHelper.GetEncodingName(850, false, false).Contains("Western European"));
      Assert.IsTrue(EncodingHelper.GetEncodingName(852, false, false).Contains("Central European"));
    }
  }
}