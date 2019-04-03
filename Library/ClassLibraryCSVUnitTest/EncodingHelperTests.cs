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
using System.Text;

namespace CsvTools.Tests
{
  [TestClass]
  public class EncodingHelperTests
  {
    [TestMethod]
    public void GetEncodingNameTest()
    {
      Assert.IsNotNull(EncodingHelper.GetEncodingName(-1, false, false));
      for (var i = 0; i < EncodingHelper.CommonCodePages.Length; i++)
        Assert.IsNotNull(EncodingHelper.GetEncodingName(EncodingHelper.CommonCodePages[i], i % 3 == 0, i % 2 == 0));
    }

    [TestMethod]
    public void GuessCodePageNoBOMTest()
    {
      var utf8 = Encoding.UTF8.GetBytes("Raphael Nöldner Chinese 中文 & Thai ภาษาไทย Tailandia idioma ภาษาไทย");
      var aSCII = Encoding.ASCII.GetBytes("This is a Test");

      Assert.AreEqual(65001, EncodingHelper.GuessCodePageNoBom(null, 20));
      Assert.AreEqual(20127, EncodingHelper.GuessCodePageNoBom(aSCII, aSCII.GetUpperBound(0)));
      Assert.AreEqual(65001, EncodingHelper.GuessCodePageNoBom(utf8, utf8.GetUpperBound(0)));
    }

    [TestMethod]
    public void GetEncodingTest()
    {
      Assert.AreEqual(new UTF8Encoding(true), EncodingHelper.GetEncoding(65001, true));
      Assert.AreEqual(new UTF8Encoding(true), EncodingHelper.GetEncoding(-1, true));
      Assert.AreEqual(Encoding.GetEncoding(1252), EncodingHelper.GetEncoding(1252, true));
    }
  }
}