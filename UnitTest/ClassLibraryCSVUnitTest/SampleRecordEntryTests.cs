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

namespace CsvTools.Tests
{
  [TestClass]
  public class SampleRecordEntryTests
  {
    [TestMethod]
    public void SampleRecordEntry()
    {
      var entry = new SampleRecordEntry();
      Assert.AreEqual(true, entry.ProvideEvidence);

      var entry1 = new SampleRecordEntry(100, "Error");
      Assert.AreEqual(100, entry1.RecordNumber);
      Assert.AreEqual("Error", entry1.Error);
      Assert.AreEqual(true, entry1.ProvideEvidence);

      var entry2 = new SampleRecordEntry(1000, false);
      Assert.AreEqual(1000, entry2.RecordNumber);
      Assert.AreEqual(false, entry2.ProvideEvidence);

      var entry3 = new SampleRecordEntry(2000);
      Assert.AreEqual(2000, entry3.RecordNumber);
    }

    [TestMethod]
    public void Clone()
    {
      var entry1 = new SampleRecordEntry(100, "Error1");
      var entry2 = (SampleRecordEntry) entry1.Clone();
      Assert.AreEqual(100, entry2.RecordNumber);
      Assert.AreEqual("Error1", entry2.Error);
      Assert.IsTrue(entry2.ProvideEvidence);
    }

    [TestMethod]
    public void Equals()
    {
      var entry1 = new SampleRecordEntry(100, "Error1");
      var entry2 = entry1.Clone();
      Assert.IsTrue(entry1.Equals(entry2));
      Assert.IsTrue(entry2.Equals(entry1));
      Assert.IsFalse(entry1.Equals(null));

      entry2 = new SampleRecordEntry(10, "Error1");
      Assert.IsFalse(entry1!.Equals(entry2));
    }

    [TestMethod]
    public void GetHashCodeTest()
    {
      var entry1 = new SampleRecordEntry(100, "Error1");
      var entry2 = entry1.Clone();
      Assert.AreEqual(entry1.GetHashCode(), entry2.GetHashCode());

      entry2 = new SampleRecordEntry(10, "Error1");
      Assert.AreNotEqual(entry1.GetHashCode(), entry2.GetHashCode());
    }
  }
}