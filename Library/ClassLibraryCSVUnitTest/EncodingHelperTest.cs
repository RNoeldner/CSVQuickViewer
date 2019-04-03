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

namespace CsvTools.Tests
{
  [TestClass]
  public class EncodingHelperTest
  {
    [TestMethod]
    public void GuessCodePageBOMGB18030()
    {
      byte[] test = { 132, 49, 149, 51 };
      Assert.AreEqual(54936, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOMUTF16BE()
    {
      byte[] test = { 254, 255, 65, 65 };
      Assert.AreEqual(1201, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOMUTF16LE()
    {
      byte[] test = { 255, 254, 65, 65 };
      Assert.AreEqual(1200, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOMUTF32BE()
    {
      byte[] test = { 0, 0, 254, 255 };
      Assert.AreEqual(12001, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOMUTF32LE()
    {
      byte[] test = { 255, 254, 0, 0 };
      Assert.AreEqual(12000, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOMUTF7a()
    {
      byte[] test = { 43, 47, 118, 57 };
      Assert.AreEqual(65000, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOMUTF7b()
    {
      byte[] test = { 43, 47, 118, 43 };
      Assert.AreEqual(65000, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOMUTF7c()
    {
      byte[] test = { 43, 47, 118, 56, 45 };
      Assert.AreEqual(65000, EncodingHelper.GetCodePageByByteOrderMark(test));
    }

    [TestMethod]
    public void GuessCodePageBOMUTF8()
    {
      byte[] test = { 239, 187, 191, 65 };
      Assert.AreEqual(65001, EncodingHelper.GetCodePageByByteOrderMark(test));
    }
  }
}