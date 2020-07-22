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
  public class EncodingHelperTest
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

      Assert.AreEqual(EncodingHelper.CodePage.UTF8, EncodingHelper.GuessCodePageNoBom(null, 20));
      Assert.AreEqual(EncodingHelper.CodePage.ASCII, EncodingHelper.GuessCodePageNoBom(aSCII, aSCII.GetUpperBound(0)));
      Assert.AreEqual(EncodingHelper.CodePage.UTF8, EncodingHelper.GuessCodePageNoBom(utf8, utf8.GetUpperBound(0)));
    }

    [TestMethod]
    public void GetEncodingTest()
    {
      Assert.AreEqual(new UTF8Encoding(true), EncodingHelper.GetEncoding(65001, true));
      Assert.AreEqual(new UTF8Encoding(true), EncodingHelper.GetEncoding(-1, true));
      Assert.AreEqual(Encoding.GetEncoding(1252), EncodingHelper.GetEncoding(1252, true));
    }

    [TestMethod]
    public void GuessCodePageBOMGB18030()
    {
      byte[] test = { 132, 49, 149, 51 };
      Assert.AreEqual(EncodingHelper.CodePage.GB18030, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOMUTF16BE()
    {
      byte[] test = { 254, 255, 65, 65 };
      Assert.AreEqual(EncodingHelper.CodePage.UTF16Be, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOMUTF16LE()
    {
      byte[] test = { 255, 254, 65, 65 };
      Assert.AreEqual(EncodingHelper.CodePage.UTF16Le, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOMUTF32BE()
    {
      byte[] test = { 0, 0, 254, 255 };
      Assert.AreEqual(EncodingHelper.CodePage.UTF32Be, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOMUTF32LE()
    {
      byte[] test = { 255, 254, 0, 0 };
      Assert.AreEqual(EncodingHelper.CodePage.UTF32Le, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOMUTF7a()
    {
      byte[] test = { 43, 47, 118, 57 };
      Assert.AreEqual(EncodingHelper.CodePage.UTF7, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOMUTF7b()
    {
      byte[] test = { 43, 47, 118, 43 };
      Assert.AreEqual(EncodingHelper.CodePage.UTF7, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOMUTF7c()
    {
      byte[] test = { 43, 47, 118, 56, 45 };
      Assert.AreEqual(EncodingHelper.CodePage.UTF7, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOMUTF8()
    {
      byte[] test = { 239, 187, 191, 65 };
      Assert.AreEqual(EncodingHelper.CodePage.UTF8, EncodingHelper.GetCodePageByByteOrderMark(test));
    }
  }
}