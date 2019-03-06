using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass()]
  public class SampleRecordEntryTests
  {
    [TestMethod()]
    public void SampleRecordEntry()
    {
      var entry = new SampleRecordEntry();
      Assert.AreEqual(true, entry.ProvideEvidence);

      var entry1 = new SampleRecordEntry(100, "Error");
      Assert.AreEqual((long)100, entry1.RecordNumber);
      Assert.AreEqual("Error", entry1.Error);
      Assert.AreEqual(true, entry1.ProvideEvidence);

      var entry2 = new SampleRecordEntry(1000, false);
      Assert.AreEqual((long)1000, entry2.RecordNumber);
      Assert.AreEqual(false, entry2.ProvideEvidence);

      var entry3 = new SampleRecordEntry(2000);
      Assert.AreEqual((long)2000, entry3.RecordNumber);
    }

    [TestMethod()]
    public void CopyTo()
    {
      var entry1 = new SampleRecordEntry(100, "Error1");

      var entry2 = new SampleRecordEntry(200, "Error2");
      entry2.ProvideEvidence = false;

      entry1.CopyTo(entry2);
      Assert.AreEqual((long)100, entry2.RecordNumber);
      Assert.AreEqual("Error1", entry2.Error);
      Assert.IsTrue(entry2.ProvideEvidence);
    }

    [TestMethod()]
    public void Clone()
    {
      var entry1 = new SampleRecordEntry(100, "Error1");
      var entry2 = entry1.Clone();
      Assert.AreEqual((long)100, entry2.RecordNumber);
      Assert.AreEqual("Error1", entry2.Error);
      Assert.IsTrue(entry2.ProvideEvidence);
    }

    [TestMethod()]
    public void Equals()
    {
      var entry1 = new SampleRecordEntry(100, "Error1");
      var entry2 = entry1.Clone();
      Assert.IsTrue(entry1.Equals(entry2));
      Assert.IsTrue(entry2.Equals(entry1));
      Assert.IsFalse(entry1.Equals(null));
      entry2.RecordNumber = 10;
      Assert.IsFalse(entry1.Equals(entry2));
    }

    [TestMethod()]
    public void CompareTo()
    {
      var entry1 = new SampleRecordEntry(100, "Error1");
      var entry2 = entry1.Clone();

      Assert.AreEqual(0, entry1.CompareTo(entry2));
      entry2.RecordNumber = entry1.RecordNumber + 1;

      Assert.AreEqual(-1, entry1.CompareTo(entry2));
    }
  }
}