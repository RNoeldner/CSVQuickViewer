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
using System.Globalization;
using System.Threading;

namespace CsvTools.Tests;

[TestClass]
public class TimeToCompletionTests
{

  [TestMethod]
  [Timeout(20000)]
  public void TimeToCompletionTestTimeRemainingAsync()
  {
    var test = new TimeToCompletion
    {
      TargetValue = 120,
    };

    var point1 = 50;
    var speed1 = 10;
    var speed2 = 20;
    for (var counter = 0; counter < point1; counter++)
    {
      test.Value = counter;
      Thread.Sleep(speed1);
    }
    // I now have 50 values in 2.5 + time for the processing seconds 
    // this means we should have 5+ seconds for the rest
    var estimate = test.EstimatedTimeRemaining.TotalSeconds;
    Assert.IsTrue(1.0 < estimate && estimate < 2, 
      $"Should have 1 seconds for the remaining {test.TargetValue - point1} items\nExact Value: {estimate:N1}");
    for (var counter = point1; counter < test.TargetValue - point1; counter++)
    {
      test.Value = counter;
      Thread.Sleep(speed2);
    }
    estimate = test.EstimatedTimeRemaining.TotalSeconds;
    // now we have decreased the speed
    // I now have 100 values in 2.5+5 + time for the processing seconds,
    // this means we should have 2.5-5 seconds for the rest of 50 depending on how the speed is picked up 
    Assert.IsTrue(estimate  > 0.8 && estimate  < 1.2, $"Should have 1 seconds for the rest {point1}\nExact Value: {estimate:N1}");
  }

  [TestMethod]
  [Timeout(4000)]
  public void TimeToCompletionTestAsync()
  {
    var test = new TimeToCompletion();
    Assert.IsNotNull(test);
    test.TargetValue = 100;
    Assert.AreEqual(100, test.TargetValue);
    Assert.AreEqual(string.Empty, test.EstimatedTimeRemainingDisplaySeparator);
    Assert.AreEqual(string.Empty, test.EstimatedTimeRemainingDisplay);
      
    // fast changes
    for (var counter = 1; counter < 10; counter++)
    {
      test.Value = counter;
      Thread.Sleep(20);
    }
    var totalSec1 = test.EstimatedTimeRemaining.TotalSeconds;
    Assert.IsTrue(totalSec1  > 1.0 && totalSec1 < 4, $"Fast: {1} < {totalSec1} < {4}"); 
      
    // Slower changes
    for (var counter = 10; counter < 60; counter++)
    {
      test.Value = counter;
      Thread.Sleep(50);
    }
    var totalSec2 = test.EstimatedTimeRemaining.TotalSeconds;
    Assert.IsTrue(totalSec1  > 1.0 && totalSec2 < 5, $"Slow: {1} < {totalSec2} < {5}"); 
      
    Assert.AreNotEqual(string.Empty, test.EstimatedTimeRemainingDisplaySeparator);
    //      Assert.AreNotEqual(string.Empty, test.EstimatedTimeRemainingDisplay);
  }

  [TestMethod]
  [Timeout(20000)]
  public void DisplayTimespan()
  {
    Assert.AreEqual("1.5 days",
      TimeToCompletion.DisplayTimespan(TimeSpan.FromDays(1.5), false).ReplaceDefaults(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".", "", ""));

    Assert.AreEqual("0.10 sec",
      TimeToCompletion.DisplayTimespan(TimeSpan.FromSeconds(0.1), false).ReplaceDefaults(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".", "", ""));

    Assert.AreEqual("20 sec",
      TimeToCompletion.DisplayTimespan(TimeSpan.FromSeconds(20), false).ReplaceDefaults(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".", "", ""));

    Assert.AreEqual("20:30 min",
      TimeToCompletion.DisplayTimespan(TimeSpan.FromMinutes(20.5), false).ReplaceDefaults(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".", "", ""));

    Assert.AreEqual("2:30 min",
      TimeToCompletion.DisplayTimespan(TimeSpan.FromMinutes(2.5), false).ReplaceDefaults(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".", "", ""));

    Assert.AreEqual("2:30 hrs",
      TimeToCompletion.DisplayTimespan(TimeSpan.FromHours(2.5), false).ReplaceDefaults(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".", "", ""));

    Assert.AreEqual("11:30 hrs",
      TimeToCompletion.DisplayTimespan(TimeSpan.FromHours(11.5), false).ReplaceDefaults(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".", "", ""));

    Assert.AreEqual("", TimeToCompletion.DisplayTimespan(TimeSpan.FromSeconds(0.1)));
  }
}