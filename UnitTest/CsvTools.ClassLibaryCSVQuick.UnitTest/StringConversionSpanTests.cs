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
using System.Linq;

namespace CsvTools.Tests
{
  [TestClass()]
  public class StringConversionSpanTests
  {
    [TestMethod]
    //[Timeout(500)]
    public void GetPartsEndingSepTest()
    {
      var span = "This;is;a;".AsSpan();
      var list = span.GetSlices(StringUtils.DelimiterChars);
      Assert.AreEqual(3, list.Count());
      var first = list.First();
      Assert.AreEqual("This", new string(span.Slice(first.start, first.length)));
      Assert.IsTrue("This".AsSpan()
        .Equals(span.Slice(first.start, first.length), StringComparison.OrdinalIgnoreCase));
      var last = list.Last();
      Assert.AreEqual("a", new string(span.Slice(last.start, last.length)));
    }

    [TestMethod]
    [Timeout(500)]
    public void GetPartsOpenEnding()
    {
      var span = "This;is;a".AsSpan();
      var list = span.GetSlices(StringUtils.DelimiterChars);
      Assert.AreEqual(3, list.Count());
      var first = list.First();
      Assert.AreEqual("This", new string(span.Slice(first.start, first.length)));
      var last = list.Last();
      Assert.AreEqual("a", new string(span.Slice(last.start, last.length)));
    }

    [TestMethod]
    [Timeout(500)]
    public void GetPartsNoSep()
    {
      var span = "This is".AsSpan();
      var list = span.GetSlices(StringUtils.DelimiterChars);
      Assert.AreEqual(1, list.Count());
      var first = list.First();
      Assert.AreEqual("This is", new string(span.Slice(first.start, first.length)));
    }

    [TestMethod]
    [Timeout(500)]
    public void StringToBooleanSpanTest()
    {
      Assert.IsTrue("*".AsSpan().StringToBoolean("True;*;", "?"));
      Assert.IsTrue("*".AsSpan().StringToBoolean("*", "?"));
      Assert.IsFalse("?".AsSpan().StringToBoolean("True;*;", "?"));
      Assert.IsNull("*".AsSpan().StringToBoolean(string.Empty, ReadOnlySpan<char>.Empty));
      Assert.IsTrue("True".AsSpan().StringToBoolean(ReadOnlySpan<char>.Empty, ReadOnlySpan<char>.Empty));
    }

    [TestMethod]
    [Timeout(500)]
    public void StringToTextPartTest()
    {
      Assert.AreEqual("Part2;Part3;Part4", StringConversion.StringToTextPart("Part1;Part2;Part3;Part4", ';', 2, true));
      Assert.AreEqual("Part2;Part3;Part4;", StringConversion.StringToTextPart("Part1;Part2;Part3;Part4", ';', 3, true));
      Assert.AreEqual("Part1", StringConversion.StringToTextPart("Part1;Part2;Part3;Part4", ';', 1, false));
      Assert.AreEqual("", StringConversion.StringToTextPart("Part1;;Part3;Part4", ';', 2, false));
      Assert.AreEqual("Part4", StringConversion.StringToTextPart("Part1;Part2;Part3;Part4", ';', 4, false));
    }

  }
}
