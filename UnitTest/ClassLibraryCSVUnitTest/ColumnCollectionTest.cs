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

namespace CsvTools.Tests
{
  [TestClass]
  public class ColumnCollectionTest
  {
    [TestMethod]
    public void Add()
    {
      var test = new ColumnCollection();
      Assert.AreEqual(0, test.Count);
      var item1 = new Column("Test");
      test.Add(item1);
      Assert.AreEqual(1, test.Count);
      var item2 = new Column("Test", ValueFormat.Empty, 0);
      test.Add(item2);
      Assert.AreEqual(1, test.Count);
      test.Add(new Column("New"));
      Assert.Throws<ArgumentException>(() => test.Add(null));
    }

    [TestMethod]
    public void TestOrder()
    {
      var test = new ColumnCollection
      {
        new Column("ColA", ValueFormat.Empty, 1),
        new Column("ColB", ValueFormat.Empty, 2),
        new Column("ColC", ValueFormat.Empty, 3)
      };
      Assert.AreEqual(3, test.Count);

      int oldColOrd = 0;
      foreach (var col in test)
      {
        Assert.IsTrue(col.ColumnOrdinal > oldColOrd);
        oldColOrd = col.ColumnOrdinal;
      }
    }

    [TestMethod]
    public void Replace()
    {
      var test = new ColumnCollection
      {
        new Column("ColA", ValueFormat.Empty, 1),
        new Column("ColB", ValueFormat.Empty, 2),
        new Column("ColC", ValueFormat.Empty, 3)
      };
      var colBnew = new Column("ColB", new ValueFormat(DataTypeEnum.Boolean), 2);
      test.Replace(colBnew);
      var colB = test.GetByName("ColB");
      Assert.AreEqual(3, test.Count);
      Assert.IsTrue(colB != null && colB.Equals(colBnew));
      int oldColOrd = 0;
      foreach (var col in test)
      {
        Assert.IsTrue(col.ColumnOrdinal > oldColOrd);
        oldColOrd = col.ColumnOrdinal;
      }
    }


    [TestMethod]
    public void CopyTo()
    {
      var test1 = new ColumnCollection { new Column("Test1"), new Column("Test2"), new Column("Test3") };
      var test2 = new ColumnCollection();
      test2.AddRangeNoClone(test1);

      Assert.IsTrue(test2.Equals(test1));
    }
  }
}
