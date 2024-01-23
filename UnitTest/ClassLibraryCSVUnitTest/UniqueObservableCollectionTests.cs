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

      public TestObject(in string name)
      {
        Name= name;
      }
      public int CollectionIdentifier => Name.GetHashCode();

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