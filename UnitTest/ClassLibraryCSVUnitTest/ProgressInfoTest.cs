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

namespace CsvTools.Tests
{
  [TestClass]
  public class ProgressInfoTest
  {

    [TestMethod]
    public void CtorLong()
    {
      var test = new ProgressInfo("TestText", 10L);
      Assert.IsNotNull(test);
      Assert.AreEqual("TestText", test.Text);
      Assert.AreEqual(10L, test.Value);
    }

    [TestMethod]
    public void CtorFloat()
    {
      var test = new ProgressInfo("TestText", 3L);
      Assert.IsNotNull(test);
      Assert.AreEqual("TestText", test.Text);
      Assert.AreEqual(3L, test.Value);
    }

    [TestMethod]
    public void CtorText()
    {
      var test = new ProgressInfo("TestText1");
      Assert.IsNotNull(test);
      Assert.AreEqual("TestText1", test.Text);
      Assert.AreEqual(-1L, test.Value);
    }
  }
}
