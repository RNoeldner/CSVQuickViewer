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
using System.Linq;

namespace CsvTools.Tests
{
  [TestClass()]
  public class UniqueObservableCollectionTests
  {
    public class TestObject : ICollectionIdentity, ICloneable
    {
      public int ID { get; set; }
      public string Name { get; set; } = string.Empty;
      public bool Cloned { get; set; } = false;

      public TestObject(int id)
      {
        ID= id;
      }

      public TestObject(string name)
      {
        Name= name;
      }
      public int CollectionIdentifier => ID.GetHashCode() + Name.GetHashCode();

      public object Clone() => new TestObject(Name) { ID = ID, Cloned = true };
    }

    [TestMethod()]
    public void AddMakeUniqueTest()
    {
      var collection = new UniqueObservableCollection<TestObject>();
      collection.AddMakeUnique(new TestObject("FC"), nameof(TestObject.Name));
      Assert.AreEqual(1, collection.Count);
      collection.AddMakeUnique(new TestObject("FC"), nameof(TestObject.Name));
      Assert.AreEqual(2, collection.Count);
    }

    [TestMethod()]
    public void InsertTest()
    {
      var collection = new UniqueObservableCollection<TestObject>();
      collection.Insert(0, new TestObject(10));
      Assert.AreEqual(1, collection.Count);
      collection.Insert(0, new TestObject(11));
      Assert.AreEqual(2, collection.Count);
      collection.Insert(1, new TestObject(12));
      Assert.AreEqual(3, collection.Count);

      Assert.AreEqual(11, collection[0].ID);
      Assert.AreEqual(10, collection[2].ID);
    }


    [TestMethod()]
    public void AddRangeTest()
    {
      var collection = new UniqueObservableCollection<TestObject>();
      collection.AddRange(new[] { new TestObject(10), new TestObject(11), new TestObject(12) });
      Assert.IsTrue(collection.First().Cloned);
    }

    [TestMethod()]
    public void AddRangeNoCloneTest()
    {
      var collection = new UniqueObservableCollection<TestObject>();
      collection.AddRangeNoClone(new[] { new TestObject(10), new TestObject(11), new TestObject(12) });
      Assert.IsFalse(collection.First().Cloned);
    }


  }
}
