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
	public class DelimiterCounterTests
	{
		[TestMethod]
		public void DelimiterCounterTest()
		{
			var i = new DelimiterCounter(100, null);
      Assert.AreEqual(100, i.NumRows);
      Assert.AreEqual(0, i.FilledRows);
    }

    [TestMethod]
    public void DelimiterCounterDisallowTest()
    {
      var i = new DelimiterCounter(50, new [] { '\t', '|' });
      Assert.AreEqual(-1, i.Separators.IndexOf('\t'));
      Assert.AreEqual(-1, i.Separators.IndexOf('|'));
      Assert.AreEqual(50, i.NumRows);
      Assert.AreEqual(0, i.FilledRows);

      Assert.IsFalse(i.CheckChar('\t'));
      Assert.IsTrue(i.CheckChar(';')); 
    }

    [TestMethod]
    public void DelimiterCounterCheckCharTest()
    {
      var i = new DelimiterCounter(50, null);
      Assert.IsFalse(i.CheckChar('a'));
      Assert.IsTrue(i.CheckChar(';'));
    }
  }
}