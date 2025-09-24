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
using System;

namespace CsvTools.Tests
{
  [TestClass]
  public class DelimiterCounterTests
  {
    [TestMethod]
    public void DelimiterCounterCheckCharTest()
    {
      var i = new DetectionDelimiter.DelimiterCounter(50, Array.Empty<char>(), '"');
      Assert.IsFalse(i.CheckChar('a', char.MinValue));
      Assert.IsTrue(i.CheckChar(';', char.MinValue));
    }

    [TestMethod]
    public void DelimiterCounterDisallowTest()
    {
      var i = new DetectionDelimiter.DelimiterCounter(50, new[] { '\t', '|' }, '"');
      Assert.AreEqual(-1, i.Separators.IndexOf('\t'));
      Assert.AreEqual(-1, i.Separators.IndexOf('|'));
      Assert.AreEqual(50, i.NumRows);
      Assert.AreEqual(0, i.FilledRows);

      Assert.IsFalse(i.CheckChar('\t', char.MinValue));
      Assert.IsTrue(i.CheckChar(';', ';'));
    }

    [TestMethod]
    public void DelimiterCounterLastRowTest()
    {
      var i = new DetectionDelimiter.DelimiterCounter(2, Array.Empty<char>(), '"');
      Assert.AreEqual(0, i.LastRow);
      i.CheckChar(';', char.MinValue);
      Assert.IsTrue(i.LastRow >= 0);
    }

    [TestMethod]
    public void DelimiterCounterScore()
    {
      var i = new DetectionDelimiter.DelimiterCounter(100, Array.Empty<char>(), '"');
      Assert.AreEqual(100, i.NumRows);

      i.CheckChar(';', ';');
      i.CheckChar('\t', 'x');
      i.CheckChar(',', '"');

      // there is no score if we repeat
      Assert.AreEqual(0, i.SeparatorScore[i.Separators.IndexOf(';')]);
      // there is a score if we follow a Text
      Assert.AreEqual(1, i.SeparatorScore[i.Separators.IndexOf('\t')]);
      Assert.AreEqual(2, i.SeparatorScore[i.Separators.IndexOf(',')]);

    }

    [TestMethod]
    public void DelimiterCounterSeparatorRowsTest()
    {
      var i = new DetectionDelimiter.DelimiterCounter(5, Array.Empty<char>(), '"');
      Assert.AreEqual(14, i.SeparatorRows.Length);
      // Initially all rows should be zero
      foreach (var count in i.SeparatorRows)
        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public void DelimiterCounterSeparatorsCountTest()
    {
      var i = new DetectionDelimiter.DelimiterCounter(3, Array.Empty<char>(), '"');
      Assert.AreEqual(3, i.SeparatorsCount.GetLength(1), "");
      // Initially all counts should be zero
      for (int row = 0; row < 3; row++)
        for (int col = 0; col < i.Separators.Length; col++)
          Assert.AreEqual(0, i.SeparatorsCount[col, row]);
    }

    [TestMethod]
    public void DelimiterCounterSeparatorsPropertyTest()
    {
      var i = new DetectionDelimiter.DelimiterCounter(10, new[] { ',' }, '"');
      // Should not contain disallowed delimiter
      Assert.IsFalse(i.Separators.Contains(","));
      // Should contain default delimiters
      Assert.IsTrue(i.Separators.Contains(";"));
      Assert.IsTrue(i.Separators.Contains("\t"));
    }

    [TestMethod]
    public void DelimiterCounterTest()
    {
      var i = new DetectionDelimiter.DelimiterCounter(100, Array.Empty<char>(), '"');
      Assert.AreEqual(100, i.NumRows);
      Assert.AreEqual(0, i.FilledRows);
    }
  }
}
