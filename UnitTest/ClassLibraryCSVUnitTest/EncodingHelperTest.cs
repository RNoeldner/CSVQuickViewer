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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable IdentifierTypo

// ReSharper disable StringLiteralTypo

namespace CsvTools.Tests
{
  [TestClass]
  public class EncodingHelperTest
  {
    [TestMethod]
    public void GetEncodingNameTest()
    {
      Assert.IsNotNull(EncodingHelper.GetEncodingName(-1, false));
      for (var i = 0; i < EncodingHelper.CommonCodePages.Length; i++)
        Assert.IsNotNull(EncodingHelper.GetEncodingName(EncodingHelper.CommonCodePages[i], i % 2 == 0));

      Assert.AreEqual("Unicode (UTF-16) / ISO 10646 / UCS-2 Little-Endian with BOM",
        EncodingHelper.GetEncodingName(Encoding.Unicode, true));
    }

    [TestMethod]
    public void GuessCodePageNoBomTest()
    {
      var utf8 = Encoding.UTF8.GetBytes("Raphael Nöldner Chinese 中文 & Thai ภาษาไทย Tailandia idioma ภาษาไทย");
      var ascii = Encoding.ASCII.GetBytes("This is a Test, that does not contain any characters that exist outside of the English Language");
      var win1252 = Encoding.GetEncoding(1252)
        .GetBytes("This is a Test for generic encoding adding some non english text like Nöldner Jürgen Ñ Æstrid or γλώσα.\r\nSymbols like: ½ pound x² and 10‰\r\nCurrency symbols like £,$ and ¥");

      Assert.AreEqual(Encoding.UTF8, EncodingHelper.DetectEncodingNoBom(null));
      Assert.AreEqual(Encoding.ASCII, EncodingHelper.DetectEncodingNoBom(ascii));
      Assert.AreEqual(Encoding.UTF8, EncodingHelper.DetectEncodingNoBom(utf8));
      Assert.AreEqual(Encoding.GetEncoding(1252), EncodingHelper.DetectEncodingNoBom(win1252));
    }

    [TestMethod]
    public void GuessCodePageGeneric()
    {
      var notRecognized = new List<int>();
      foreach (var cp in new[]
               {
                 437, 850, 852, 855, 866, 932, 1200, 1201, 1250, 1251, 1252, 1253, 1254, 10002, 10007, 12000, 12001,
                 20127, 20866, 28592, 28595, 28597, 28598, 51932, 51949, 54936, 65000, 65001
               })
      {
        try
        {
          var enc = Encoding.GetEncoding(cp);

          var array = enc.GetBytes(
            "This is a Test for generic encoding adding some non english text like Jürgen Ñ Æstrid or γλώσσα or гимназии.\r\nSymbols like: ♂ ♀ ♫ ½ ░ pound x² and 10‰\r\nCurrency symbols like £,$ and ¥\r\nAsian: これは、アプリケーション ウィザードで生成される");
          if (cp != EncodingHelper.DetectEncodingNoBom(array).CodePage)
            notRecognized.Add(cp);
        }
        catch (Exception)
        {
          // in case the code page was not found we ignore
        }
      }

      var expected = 12;
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        expected = 20;

      Assert.AreEqual(expected, notRecognized.Count);
    }

    [TestMethod]
    public void BomLength()
    {
      Assert.AreEqual(2, EncodingHelper.BOMLength(1200));
      Assert.AreEqual(3, EncodingHelper.BOMLength(65001));
      Assert.AreEqual(4, EncodingHelper.BOMLength(12001));
      Assert.AreEqual(0, EncodingHelper.BOMLength(1252));
    }

    [TestMethod]
    public void GetEncodingTest()
    {
      Assert.AreEqual(new UTF8Encoding(true), EncodingHelper.GetEncoding(65001, true));
      Assert.AreEqual(new UTF8Encoding(true), EncodingHelper.GetEncoding(-1, true));
      Assert.AreEqual(Encoding.GetEncoding(1252), EncodingHelper.GetEncoding(1252, true));
    }

    [TestMethod]
    public void GuessCodePageBomgb18030()
    {
      byte[] test = { 132, 49, 149, 51 };
      Assert.AreEqual(54936, EncodingHelper.GetEncodingByByteOrderMark(test, 4)!.CodePage);
    }

    [TestMethod]
    public void GuessCodePageBomutf16Be()
    {
      byte[] test = { 254, 255, 65, 65 };
      Assert.AreEqual(1201, EncodingHelper.GetEncodingByByteOrderMark(test, 4)!.CodePage);
    }

    [TestMethod]
    public void GuessCodePageBomutf16Le()
    {
      byte[] test = { 255, 254, 65, 65 };
      Assert.AreEqual(1200, EncodingHelper.GetEncodingByByteOrderMark(test, 4)!.CodePage);
    }

    [TestMethod]
    public void GuessCodePageBomutf32Be()
    {
      byte[] test = { 0, 0, 254, 255 };
      Assert.AreEqual(12001, EncodingHelper.GetEncodingByByteOrderMark(test, 4)!.CodePage);
    }

    [TestMethod]
    public void GuessCodePageBomutf32Le()
    {
      byte[] test = { 255, 254, 0, 0 };
      Assert.AreEqual(12000, EncodingHelper.GetEncodingByByteOrderMark(test, 4)!.CodePage);
    }

    [TestMethod]
    public void GuessCodePageBomutf7A()
    {
      byte[] test = { 43, 47, 118, 57 };
      Assert.AreEqual(65000, EncodingHelper.GetEncodingByByteOrderMark(test, 4)!.CodePage);
    }

    [TestMethod]
    public void GuessCodePageBomutf7B()
    {
      byte[] test = { 43, 47, 118, 43 };
      Assert.AreEqual(65000, EncodingHelper.GetEncodingByByteOrderMark(test, 4)!.CodePage);
    }

    [TestMethod]
    public void GuessCodePageBomutf7C()
    {
      byte[] test = { 43, 47, 118, 56, 45 };
      Assert.AreEqual(65000, EncodingHelper.GetEncodingByByteOrderMark(test, 4)!.CodePage);
    }

    [TestMethod]
    public void GuessCodePageBomutf8()
    {
      byte[] test = { 239, 187, 191, 65 };
      Assert.AreEqual(65001, EncodingHelper.GetEncodingByByteOrderMark(test, 4)!.CodePage);
    }
  }
}