using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass()]
  public class UniqueObservableCollectionTests
  {

    [TestMethod()]
    public void AddMakeUniqueTest()
    {
      var collection = new UniqueObservableCollection<Mapping>();
      collection.AddMakeUnique(new Mapping("FC", "TF", true, true), nameof(Mapping.FileColumn));
      Assert.AreEqual(1, collection.Count);
      collection.AddMakeUnique(new Mapping("FC", "TF", true, true), nameof(Mapping.FileColumn));
      Assert.AreEqual(2, collection.Count);
    }

    [TestMethod()]
    public void InsertTest()
    {
      var collection = new UniqueObservableCollection<SampleRecordEntry>();
      collection.Insert(0, new SampleRecordEntry(10));
      Assert.AreEqual(1, collection.Count);
      collection.Insert(0, new SampleRecordEntry(11));
      Assert.AreEqual(2, collection.Count);
      collection.Insert(1, new SampleRecordEntry(12));
      Assert.AreEqual(3, collection.Count);

      Assert.AreEqual(11, collection[0].RecordNumber);
      Assert.AreEqual(10, collection[2].RecordNumber);
    }


    [TestMethod()]
    public void AddRangeTest()
    {
      var collection = new UniqueObservableCollection<SampleRecordEntry>();
      collection.AddRange(new []{new SampleRecordEntry(10),new SampleRecordEntry(11),new SampleRecordEntry(12)});
    }

    [TestMethod()]
    public void AddRangeNoCloneTest()
    {
      var collection = new UniqueObservableCollection<SampleRecordEntry>();
      collection.AddRangeNoClone(new []{new SampleRecordEntry(10),new SampleRecordEntry(11),new SampleRecordEntry(12)});
    }


  }
}