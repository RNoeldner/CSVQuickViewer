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
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass]
  public class IntervalActionTests
  {
    [TestMethod]
    public void InvokeTest()
    {
      var intervalAction = new IntervalAction();
      var called = 0;
      // First Call ok
      intervalAction.Invoke(() => called++);
      Assert.AreEqual(1, called);
      // First Call This time its not called because time was not sufficient
      intervalAction.Invoke(() => called++);
      Assert.AreEqual(1, called);
    }

    [TestMethod]
    public void InvokeTest2()
    {
      var intervalAction = new IntervalAction(.01);
      var called = 0;
      // First Call ok
      intervalAction.Invoke(() => called++);
      Assert.AreEqual(1, called);
      Thread.Sleep(110);
      // First Call This time its not called because time was not sufficient
      intervalAction.Invoke(() => called++);
      Assert.AreEqual(2, called);
    }
  }
}