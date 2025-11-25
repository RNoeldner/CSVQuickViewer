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

namespace CsvTools.Tests;

[TestClass()]
public class ColumnExtensionsTests
{
  [TestMethod]
  public void Get()
  {
    var test = new ColumnCollection();
    var item1 = new Column("Test");
    test.Add(item1);
    Assert.AreEqual(1, test.Count);
    var item2 = new Column("Test2");
    test.Add(item2);
    Assert.IsTrue(item1.Equals(test.GetByName("Test")), "Test found");
    Assert.IsTrue(item1.Equals(test.GetByName("TEST")), "TEST found");
    Assert.IsTrue(item2.Equals(test.GetByName("tEst2")), "Test2 found");

    Assert.IsNull(test.GetByName(""));
    Assert.IsNull(test.GetByName(null));
    Assert.IsNull(test.GetByName("nonsense"));
  }
}