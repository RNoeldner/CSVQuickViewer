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



    [TestMethod]
    public void Defaults()
    {
      var test = new IntervalAction();
      Assert.IsTrue(test.NotifyAfterSeconds > 0);
      Assert.IsTrue(test.NotifyAfterSeconds < 1);
    }

    [TestMethod]
    public void Invoke()
    {
      var test = new IntervalAction();
      var called = false;
      test.Invoke(() => called = true);
      Assert.IsTrue(called);
      called = false;
      test.Invoke(() => called = true);
      Assert.IsFalse(called);
    }

    [TestMethod]
    public void InvokeLong()
    {
      var test = new IntervalAction();
      long called = -1;
      test.Invoke(delegate (long l) { called = l; }, 666);
      Assert.AreEqual(666L, called);
      test.Invoke(delegate (long l) { called = l; }, 669);
      Assert.AreNotEqual(669L, called);
    }

    [TestMethod]
    public void InvokeStringLong()
    {
      var test = new IntervalAction();
      long called = -1;
      var calledS = string.Empty;
      var calledB = false;
      test.Invoke((s, l, arg3) =>
      {
        called = l;
        calledS = s;
        calledB = arg3;
      }, "Hello", -10, true);
      Assert.AreEqual(-10L, called);
      Assert.AreEqual("Hello", calledS);
      Assert.AreEqual(true, calledB);
      test.Invoke((s, l, arg3) =>
      {
        called = l;
        calledS = s;
        calledB = arg3;
      }, "World", -20, true);
      Assert.AreNotEqual("World", calledS);
    }
  }
}