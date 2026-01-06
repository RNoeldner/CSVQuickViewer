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
using System.ComponentModel;
using System.Linq;

namespace CsvTools.Tests;

[TestClass()]
public class UniqueObservableCollectionTests
{
  public class TestObject : ObservableObject, ICollectionIdentity, ICloneable, IEquatable<TestObject>
  {
    private int iD;

    private string name = string.Empty;

    public int ID { get => iD; set => SetProperty(ref iD, value); }

    public string Name { get => name; set => SetProperty(ref name, value); }

    public bool Cloned { get; set; } = false;

    public TestObject(string name, int ID =0)
    {
      Name= name;
    }

    public string UniqueKeyPropertyName => nameof(Name);
    public object Clone() => new TestObject(Name) { ID = ID, Cloned = true };
    public string GetUniqueKey() => Name;
    public void SetUniqueKey(string key) => Name= key;
    public bool Equals(TestObject other) => ID==other.ID && Name.Equals(other.Name);
  }


  [TestMethod()]
  public void AddMakeUniqueTest()
  {
    var collection = new UniqueObservableCollection<TestObject>();
    collection.AddMakeUnique(new TestObject("FC"));
    Assert.AreEqual(1, collection.Count);
    collection.AddMakeUnique(new TestObject("FC"));
    Assert.AreEqual(2, collection.Count);
  }

  [TestMethod()]
  public void InsertTest()
  {
    var collection = new UniqueObservableCollection<TestObject>();
    collection.Insert(0, new TestObject("Test"));
    Assert.AreEqual(1, collection.Count);
    collection.Insert(0, new TestObject("Test2"));
    Assert.AreEqual(2, collection.Count);
    collection.Insert(1, new TestObject("Test3"));
    Assert.AreEqual(3, collection.Count);

    Assert.AreEqual("Test2", collection[0].Name);
    Assert.AreEqual("Test", collection[2].Name);
  }

  [TestMethod()]
  public void ChangeCalled()
  {
    var collection = new UniqueObservableCollection<TestObject>();
    int changeCalled = 0;
    collection.CollectionChanged += (s, e) => changeCalled++; 

    var item = new TestObject("Text1",1);
    collection.Add(item);
    collection.Add(new TestObject("Test2", 2));
    Assert.AreEqual(2, changeCalled);
    
    collection.AddRange(new[] { new TestObject("Test3", 3), new TestObject("Test4", 4), new TestObject("Test5", 5) });
    Assert.AreEqual(3, changeCalled);
  }


  [TestMethod()]
  public void AddRange()
  {
    var collection = new UniqueObservableCollection<TestObject>();
    collection.AddRange(new[] { new TestObject("10", 10), new TestObject("11", 11), new TestObject("12", 12) });
    Assert.IsFalse(collection.First().Cloned);
  }
}