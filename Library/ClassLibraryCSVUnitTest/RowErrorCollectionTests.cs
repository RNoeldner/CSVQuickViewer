using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass()]
  public class RowErrorCollectionTests
  {
    [TestMethod()]
    public void RowErrorCollection()
    {
      Assert.IsNotNull(new RowErrorCollection());
      Assert.IsNotNull(new RowErrorCollection(100));
    }

    [TestMethod()]
    public void Add()
    {
      var coll = new RowErrorCollection(5);
      coll.Add(this, new WarningEventArgs(1, 1, "Message1", 100, 100, "ColName"));
      Assert.AreEqual(1, coll.CountRows);
      coll.Add(this, new WarningEventArgs(2, 1, "Message1", 101, 101, "ColName"));
      Assert.AreEqual(2, coll.CountRows);
      coll.Add(this, new WarningEventArgs(3, 1, "Message1", 102, 102, "ColName"));
      Assert.AreEqual(3, coll.CountRows);
      coll.Add(this, new WarningEventArgs(4, 1, "Message1", 103, 103, "ColName"));
      Assert.AreEqual(4, coll.CountRows);
      coll.Add(this, new WarningEventArgs(5, 1, "Message1", 104, 104, "ColName"));
      Assert.AreEqual(5, coll.CountRows);

      /// This should be cut off
      coll.Add(this, new WarningEventArgs(6, 1, "Message1", 105, 105, "ColName"));
      Assert.AreEqual(5, coll.CountRows);
    }

    [TestMethod()]
    public void Clear()
    {
      var coll = new RowErrorCollection(5);
      coll.Add(this, new WarningEventArgs(1, 1, "Message1", 100, 100, "ColName"));
      Assert.AreEqual(1, coll.CountRows);
      coll.Clear();
      Assert.AreEqual(0, coll.CountRows);
    }

    [TestMethod()]
    public void TryGetValue()
    {
      var coll = new RowErrorCollection(5);
      coll.Add(this, new WarningEventArgs(1, 1, "Message1", 100, 100, "ColName"));
      Assert.IsTrue(coll.TryGetValue(1, out var val));
    }

    [TestMethod()]
    public void DisplayByRecordNumber()
    {
      var coll = new RowErrorCollection(5);
      coll.Add(this, new WarningEventArgs(425, 1, "Message1", 100, 100, "ColName"));
      Assert.IsTrue(coll.DisplayByRecordNumber.Contains("Row 425"));
    }
  }
}