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
using System.IO;

namespace CsvTools.Tests
{  
  [TestClass]
  public class StringUtilsTests
  {    
    [TestMethod]
    public void SqlNameSpan()
    {
      Assert.IsTrue(@"".AsSpan().SequenceEqual(@"".AsSpan().SqlName()));
      Assert.IsTrue(@"ValidationTask".AsSpan().SequenceEqual(@"ValidationTask".AsSpan().SqlName()));
      Assert.IsTrue(@"Validation]]Task".AsSpan().SequenceEqual(@"Validation]Task".AsSpan().SqlName()));
    }
    
    [TestMethod]
    public void SqlQuoteSpan()
    {
      Assert.IsTrue(@"".AsSpan().SequenceEqual(@"".AsSpan().SqlQuote()));
      Assert.IsTrue(@"ValidationTask".AsSpan().SequenceEqual(@"ValidationTask".AsSpan().SqlQuote()));
      Assert.IsTrue(@"Validation''Task".AsSpan().SequenceEqual(@"Validation'Task".AsSpan().SqlQuote()));
    }
  }
}
