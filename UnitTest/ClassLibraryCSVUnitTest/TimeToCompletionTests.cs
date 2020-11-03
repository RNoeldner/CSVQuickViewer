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
  public class TimeToCompletionTests
  {
    [TestMethod]
    [Timeout(20000)]
    public void TimeToCompletionTest()
    {
      var test = new TimeToCompletion();
      Assert.IsNotNull(test);
      test.TargetValue = 150;
      Assert.AreEqual(150, test.TargetValue);
      Assert.AreEqual(string.Empty, test.EstimatedTimeRemainingDisplaySeparator);
      Assert.AreEqual(string.Empty, test.EstimatedTimeRemainingDisplay);

      test.Value = 1;
      Assert.AreEqual(1, test.Value);
      Thread.Sleep(100);
      test.Value = 2;
      Assert.AreEqual(2, test.Value);
      Thread.Sleep(100);
      test.Value = 3;
      Assert.AreEqual(2, test.Percent);
      Assert.AreEqual(0.02.ToString("0.0%"), test.PercentDisplay);
      Thread.Sleep(100);
      test.Value = 4;
      Thread.Sleep(100);
      test.Value = 5;
      Assert.AreEqual(5, test.Value);
      for (var counter = 6; counter < 60; counter++)
      {
        test.Value = counter;
        Thread.Sleep(200);
      }

      Assert.IsTrue(test.EstimatedTimeRemaining.TotalSeconds > 0.0);
      Assert.IsTrue(test.EstimatedTimeRemaining.TotalSeconds < 20.0);
      Assert.AreNotEqual(string.Empty, test.EstimatedTimeRemainingDisplaySeparator);
      Assert.AreNotEqual(string.Empty, test.EstimatedTimeRemainingDisplay);
    }
  }
}