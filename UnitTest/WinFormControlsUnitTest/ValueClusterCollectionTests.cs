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
#nullable enable

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace CsvTools.Tests;

[TestClass]
public class ValueClusterCollectionTests
{
  [TestMethod, Timeout(500)]
  public void BuildValueClustersDate()
  {
    var data = new object[200];
    for (int i = 0; i < data.Length-2; i++)
      data[i]= (i % 10 == 0) ? DBNull.Value : new DateTime(UnitTestStatic.Random.Next(2000, 2023), UnitTestStatic.Random.Next(1, 12), 1).AddDays(UnitTestStatic.Random.Next(1, 31));

    // AddOrUpdate some dates multiple times to ensure they are clustered together, these dates are outside the random range
    data[1] = "Nonsense";
    data[2] = new DateTime(2025, 11, 25);
    data[data.Length - 2] = new DateTime(2025, 11, 25);
    data[data.Length - 1] = new DateTime(2025, 11, 25);

    (var countNull, var test) = data.BuildValueClustersDate("dummy", 50, false, 200.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(data.Length, test.Select(x => x.Count).Sum() + countNull);
    Assert.AreEqual(3, test.Last().Count); // The last cluster must have 3 entries for 2025-11-25
  }

  [TestMethod, Timeout(150)]
  public void BuildValueClustersDate_Combine()
  {
    var data = new List<object>();
    data.Add(new DateTime(2025, 11, 25));
    data.Add(new DateTime(2025, 11, 25));

    // make a well defined array
    for (int m = 7; m > 0; m--)
    {
      data.Add(DBNull.Value);
      for (int d = 1; d < 23; d++)
        data.Add(new DateTime(2000, m, d));
    }

    // AddOrUpdate some dates multiple times to ensure they are clustered together, these dates are outside the random range

    data.Add(new DateTime(2025, 11, 25));
    data.Add(new DateTime(2025, 11, 25));

    (var countNull, var test) = data.ToArray().BuildValueClustersDate("dummy", 150, true, 200.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(7, countNull);
    Assert.AreEqual(data.Count, test.Select(x => x.Count).Sum() + countNull);
    Assert.AreEqual(4, test.Last().Count); // The last cluster must have 4 entries for 2025-11-25
  }


  [TestMethod, Timeout(100)]
  public void BuildValueClustersString_Start()
  {
    var data = new List<object>();
    for (long i = 1; i < 200; i++)
      data.Add(i);

    (var countNull, var clusters) = data.ToArray().BuildValueClustersString("Dummy", 10, 200.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(9, clusters.Count, "We should have one entry for every starting number (1-9)"); 
    // The number of matches might be lower since special chars are not counted
    var found = countNull + clusters.Select(x => x.Count).Sum();
    Assert.AreEqual(data.Count, found, "Number of entries does not match");
  }

  [TestMethod, Timeout(100)]
  public void BuildValueClustersString_StartThreeChars()
  {
    int countNullSrc = 0;
    var data = new List<object>();
    for (long i = 1; i < 200; i++)
    {
      if (i % 10 == 1)
        data.Add($"Start{i}");
      else if (i % 10 == 2)
        data.Add($"End{i}");
      else if (i % 10 == 3)
      {
        data.Add(null);
        countNullSrc++; 
      }
      else
        data.Add($"Mid{i}");
    }

    (var countNull, var clusters) = data.ToArray().BuildValueClustersString("Dummy", 10, 200.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(3, clusters.Count, "We should have one entry for Sta one for End and one for Mid");
    Assert.AreEqual(4, clusters.First().Display.Length, $"Expected Display to have 4 chars but it is {clusters.First().Display}");

    Assert.AreEqual(countNullSrc, countNull, "Null Count");
    // The number of matches might be lower since special chars are not counted
    var found = countNull + clusters.Select(x => x.Count).Sum();
    Assert.AreEqual(data.Count, found, "Number of entries does not match");
  }

  [TestMethod, Timeout(100)]
  public void BuildValueClustersString_StartFiveChars()
  {
    var data = new List<object>();
    for (long i = 1; i < 200; i++)
    {
      if (i % 10 == 1)
        data.Add($"Start{i}");
      else if (i % 10 == 2)
        data.Add($"MyEnd{i}");
      else
        data.Add($"MyMid{i}");
    }

    (var countNull, var clusters) = data.ToArray().BuildValueClustersString("Dummy", 10, 200.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(3, clusters.Count, "We should have one entry for Sta one for End and one for Mid");
    Assert.AreEqual(6, clusters.First().Display.Length, $"Expected Display to have 4 chars but it is {clusters.First().Display}");

    Assert.AreEqual(0, countNull, "No Null Values");
    // The number of matches might be lower since special chars are not counted
    var found = countNull + clusters.Select(x => x.Count).Sum();
    Assert.AreEqual(data.Count, found, "Number of entries does not match");
  }

  [TestMethod, Timeout(100)]
  public void BuildValueClustersString_LargeGroups()
  {
    var data = new List<object>();
    for (long i = 1; i < 2000; i++)
        data.Add(UnitTestStatic.GetRandomText(20,true));

    (var countNull, var clusters) = data.ToArray().BuildValueClustersString("Dummy", 10, 200.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(6, clusters.Count, $"We should have groups like A-F, Numbers  {clusters.First().Display} - {clusters.Last().Display}");
    
    // The number of matches might be lower since special chars are not counted
    var found = countNull + clusters.Select(x => x.Count).Sum();

    Assert.IsGreaterThan(data.Count*0.95, found, "Number of entries is too low");
    //if (found < data.Count)
    //  Assert.Inconclusive($"Filters only match {found} of {data.Count} rows, goup filters not cover each record");
  }

  [TestMethod, Timeout(150)]
  public void BuildValueClustersLong()
  {
    var data = new List<object>();
    for (long i = -200; i < 200; i+=5)
      data.Add(i);

    (var countNull, var test1) = data.ToArray().BuildValueClustersLong("dummy", 100, false, 200.0, UnitTestStatic.TesterProgress);
    var number1 = test1.Select(x => x.Count).Sum();
    Assert.AreEqual(data.Count, countNull + number1, "Overall Number is correct");
    Assert.IsTrue(test1.First().Display.Contains($"{data.First():N0}"));
    // Assert.IsTrue(clusters.Last().Display.Contains($"{data.Last():N0}"));

    (var countNull2, var test2) = data.ToArray().BuildValueClustersLong("dummy", 20, true, 200.0, UnitTestStatic.TesterProgress);
    var number2 = test2.Select(x => x.Count).Sum();
    Assert.AreEqual(number1, number2, "Number of entries does not match Combine / Normal");

    (var countNull3, var test3) = data.ToArray().BuildValueClustersLongEven("dummy", 20, 200.0, UnitTestStatic.TesterProgress);
    var number3 = test3.Select(x => x.Count).Sum();
    Assert.AreEqual(number1, number3, "Number of entries does not match Even / Normal");

    (var countNull4, var test4) = data.ToArray().BuildValueClustersLong("dummy", 20, false, 200.0, UnitTestStatic.TesterProgress);
    var number4 = test4.Select(x => x.Count).Sum();
    Assert.AreEqual(number1, number4, "Number of entries does not change on number of groups");
  }


  [TestMethod, Timeout(150)]
  public void BuildValueClustersDouble()
  {
    var data = new List<object>();
    for (long i = -200; i < 200; i+=5)
      data.Add(i/10d);

    (var countNull, var test1) = data.ToArray().BuildValueClustersDouble("dummy", 100, false, 200.0, UnitTestStatic.TesterProgress);
    var number1 = test1.Select(x => x.Count).Sum();
    Assert.AreEqual(data.Count, countNull + number1, "Overall Number is correct");
    Assert.IsTrue(test1.First().Display.Contains($"{data.First():N0}"));
    // Assert.IsTrue(clusters.Last().Display.Contains($"{data.Last():N0}"));

    (var countNull2, var test2) = data.ToArray().BuildValueClustersDouble("dummy", 20, true, 200.0, UnitTestStatic.TesterProgress);
    var number2 = test2.Select(x => x.Count).Sum();
    Assert.AreEqual(number1, number2, "Number of entries does not match Combine / Normal");

    (var countNull3, var test3) = data.ToArray().BuildValueClustersDoubleEven("dummy", 20, 200.0, UnitTestStatic.TesterProgress);
    var number3 = test3.Select(x => x.Count).Sum();
    Assert.AreEqual(number1, number3, "Number of entries does not match Even / Normal");

    (var countNull4, var test4) = data.ToArray().BuildValueClustersDouble("dummy", 20, false, 200.0, UnitTestStatic.TesterProgress);
    var number4 = test4.Select(x => x.Count).Sum();
    Assert.AreEqual(number1, number4, "Number of entries does not change on number of groups");
  }
}