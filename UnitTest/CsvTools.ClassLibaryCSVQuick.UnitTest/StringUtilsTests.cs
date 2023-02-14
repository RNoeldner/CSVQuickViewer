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

namespace CsvTools.Tests
{
  [TestClass]
  public class StringUtilsTests
  {
    [TestMethod]
    [Timeout(200)]
    public void SqlNameSpan()
    {
      Assert.IsTrue(@"".AsSpan().SequenceEqual(@"".AsSpan().SqlName()));
      Assert.IsTrue(@"ValidationTask".AsSpan().SequenceEqual(@"ValidationTask".AsSpan().SqlName()));
      Assert.IsTrue(@"Validation]]Task".AsSpan().SequenceEqual(@"Validation]Task".AsSpan().SqlName()));
    }

    [TestMethod]
    [Timeout(200)]
    public void SqlQuoteSpan()
    {
      Assert.IsTrue(@"".AsSpan().SequenceEqual(@"".AsSpan().SqlQuote()));
      Assert.IsTrue(@"ValidationTask".AsSpan().SequenceEqual(@"ValidationTask".AsSpan().SqlQuote()));
      Assert.IsTrue(@"Validation''Task".AsSpan().SequenceEqual(@"Validation'Task".AsSpan().SqlQuote()));
    }


    [TestMethod]
    [Timeout(200)]
    public void PassesFilter()
    {
      Assert.AreEqual(true, "This is a test".AsSpan().PassesFilter("The+test", StringComparison.OrdinalIgnoreCase));
      Assert.AreEqual(false, "This is a test".AsSpan().PassesFilter("+The+test", StringComparison.OrdinalIgnoreCase));
      Assert.AreEqual(true, "This is a test".AsSpan().PassesFilter("This +test", StringComparison.OrdinalIgnoreCase));
      Assert.AreEqual(true, ReadOnlySpan<char>.Empty.PassesFilter(ReadOnlySpan<char>.Empty, StringComparison.OrdinalIgnoreCase));
      Assert.AreEqual(true, "This is a test".AsSpan().PassesFilter("test", StringComparison.OrdinalIgnoreCase));
      Assert.AreEqual(true, "This is a test".AsSpan().PassesFilter("This", StringComparison.OrdinalIgnoreCase));
      Assert.AreEqual(true, "This is a test".AsSpan().PassesFilter("+", StringComparison.OrdinalIgnoreCase));
    }
  }
}
