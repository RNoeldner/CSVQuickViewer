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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class TimeToCompletionTests
  {

    [TestMethod]
    [Timeout(20000)]
    public async Task TimeToCompletionTestTimeRemainingAsync()
    {
      var test = new TimeToCompletion
      {
        TargetValue = 150
      };

      var point1 = 50;
      var speed1 = 50;
      var speed2 = 100;
      for (var counter = 0; counter < point1; counter++)
      {
        test.Value = counter;
        await Task.Delay(speed1, UnitTestStatic.Token);
        
      }
      // I now have 50 values in 2.5 + time for the processing seconds 
      // this means we should have 5+ seconds for the rest
      var estimate = test.EstimatedTimeRemaining.TotalSeconds;
      Assert.IsTrue(5.0 < estimate && estimate < 6.5, $"Should have 5 seconds for the rest {test.TargetValue - point1}\nExact Value: {test.EstimatedTimeRemaining.TotalSeconds }");
      for (var counter = point1; counter < test.TargetValue - point1; counter++)
      {
        test.Value = counter;
        await Task.Delay(speed2, UnitTestStatic.Token);        
      }
      estimate = test.EstimatedTimeRemaining.TotalSeconds;
      // now we have descreased the speed
      // I now have 100 values in 2.5+5 + time for the processing seconds,
      // this means we should have 2.5-5 seconds for the rest of 50 depedning on how the speed is picked up 
      Assert.IsTrue(4.0 < estimate && estimate < 6.0, $"Should have 5 seconds for the rest {point1}\nExact Value: {test.EstimatedTimeRemaining.TotalSeconds }");           
    }

    [TestMethod]
    [Timeout(20000)]
    public async System.Threading.Tasks.Task TimeToCompletionTestAsync()
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
        await Task.Delay(200, UnitTestStatic.Token);
      }
      var totalSec = test.EstimatedTimeRemaining.TotalSeconds;
      Assert.IsTrue(totalSec > 10.0 && totalSec < 22.0, $"10 < {totalSec} < 22");      
      Assert.AreNotEqual(string.Empty, test.EstimatedTimeRemainingDisplaySeparator);
      Assert.AreNotEqual(string.Empty, test.EstimatedTimeRemainingDisplay);
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
}