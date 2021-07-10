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
using System.Text;

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

			Assert.AreEqual("CP 1200 - Unicode (UTF-16) / ISO 10646 / UCS-2 Little-Endian with BOM", EncodingHelper.GetEncodingName(Encoding.Unicode, true));
		}

		[TestMethod]
		public void GuessCodePageNoBOMTest()
		{
			var utf8 = Encoding.UTF8.GetBytes("Raphael Nöldner Chinese 中文 & Thai ภาษาไทย Tailandia idioma ภาษาไทย");
			var aSCII = Encoding.ASCII.GetBytes("This is a Test, that does not contain any characters that exist outside of teh English Language");
			var win1252 = Encoding.GetEncoding(1252)
														.GetBytes(
															"This is a Test for Windows 1252 the encoding support mainly european countries with Æ ã è ü but has symbols as well ½ ² etc.");

			Assert.AreEqual(Encoding.UTF8, EncodingHelper.GuessEncodingNoBom(null));
			Assert.AreEqual(Encoding.ASCII, EncodingHelper.GuessEncodingNoBom(aSCII));
			Assert.AreEqual(Encoding.UTF8, EncodingHelper.GuessEncodingNoBom(utf8));
			Assert.AreEqual(Encoding.GetEncoding(1252), EncodingHelper.GuessEncodingNoBom(win1252));
		}

		[TestMethod]
		public void GuessCodePageGeneric()
		{
			var notRecognized = new List<int>();
			foreach (var cp in new[]
			{
				437, 850, 852, 855, 866, 932, 1200, 1201, 1250, 1251, 1252, 1253, 1254, 10002, 10007, 12000, 12001, 20127, 20866, 28592, 28595, 28597, 28598, 51932,
				51949, 54936, 65000, 65001
			})
			{
				try
				{
					var enc = Encoding.GetEncoding(cp);

					var array = enc.GetBytes(
						"This is a Test for generic encoding adding some non english text like Jürgen Ñ Æstrid or γλώσσα or гимназии.\r\nSymbols like: ♂ ♀ ♫ ½ ░ pound x² and 10‰\r\nCurrency symbols like £,$ and ¥\r\nAsian: これは、アプリケーション ウィザードで生成される");
					if (cp != EncodingHelper.GuessEncodingNoBom(array).CodePage)
						notRecognized.Add(cp);
				}
				catch (Exception)
				{
					// in case teh code page was not found we ignore
				}
			}

			Assert.AreEqual(17, notRecognized.Count);
		}

		[TestMethod]
		public void BOMLength()
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
		public void GuessCodePageBOMGB18030()
		{
			byte[] test = { 132, 49, 149, 51 };
			Assert.AreEqual(54936, EncodingHelper.GetEncodingByByteOrderMark(test).CodePage);
		}

		[TestMethod]
		public void GuessCodePageBOMUTF16BE()
		{
			byte[] test = { 254, 255, 65, 65 };
			Assert.AreEqual(1201, EncodingHelper.GetEncodingByByteOrderMark(test).CodePage);
		}

		[TestMethod]
		public void GuessCodePageBOMUTF16LE()
		{
			byte[] test = { 255, 254, 65, 65 };
			Assert.AreEqual(1200, EncodingHelper.GetEncodingByByteOrderMark(test).CodePage);
		}

		[TestMethod]
		public void GuessCodePageBOMUTF32BE()
		{
			byte[] test = { 0, 0, 254, 255 };
			Assert.AreEqual(12001, EncodingHelper.GetEncodingByByteOrderMark(test).CodePage);
		}

		[TestMethod]
		public void GuessCodePageBOMUTF32LE()
		{
			byte[] test = { 255, 254, 0, 0 };
			Assert.AreEqual(12000, EncodingHelper.GetEncodingByByteOrderMark(test).CodePage);
		}

		[TestMethod]
		public void GuessCodePageBOMUTF7a()
		{
			byte[] test = { 43, 47, 118, 57 };
			Assert.AreEqual(65000, EncodingHelper.GetEncodingByByteOrderMark(test).CodePage);
		}

		[TestMethod]
		public void GuessCodePageBOMUTF7b()
		{
			byte[] test = { 43, 47, 118, 43 };
			Assert.AreEqual(65000, EncodingHelper.GetEncodingByByteOrderMark(test).CodePage);
		}

		[TestMethod]
		public void GuessCodePageBOMUTF7c()
		{
			byte[] test = { 43, 47, 118, 56, 45 };
			Assert.AreEqual(65000, EncodingHelper.GetEncodingByByteOrderMark(test).CodePage);
		}

		[TestMethod]
		public void GuessCodePageBOMUTF8()
		{
			byte[] test = { 239, 187, 191, 65 };
			Assert.AreEqual(65001, EncodingHelper.GetEncodingByByteOrderMark(test).CodePage);
		}
	}
}