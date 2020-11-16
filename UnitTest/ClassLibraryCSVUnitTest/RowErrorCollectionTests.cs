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

using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class RowErrorCollectionTests
  {
    [TestMethod]
    public void RowErrorCollection() => Assert.IsNotNull(new RowErrorCollection(100));

    [TestMethod]
    public async Task HandleIgnoredColumns()
    {
      var coll = new RowErrorCollection(5);

      using (var reader = new CsvFileReader(UnitTestInitializeCsv.GetTestPath("AllFormats.txt"), 650001, 0, true,
        new IColumn[]
        {
          new ImmutableColumn("DateTime", new ImmutableValueFormat(DataType.DateTime), 0, true, "", true),
          new ImmutableColumn("Integer", new ImmutableValueFormat(DataType.Integer), 0, true, "", true),
        }))

      {
        await reader.OpenAsync(CancellationToken.None);
        coll.HandleIgnoredColumns(reader);

        // An error i an ignored column is not stored
        coll.Add(this, new WarningEventArgs(1, 0, "Message1", 100, 100, "ColName"));
        Assert.AreEqual(0, coll.CountRows);
      }
    }

    [TestMethod]
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

      // This should be cut off
      coll.Add(this, new WarningEventArgs(6, 1, "Message1", 105, 105, "ColName"));
      Assert.AreEqual(5, coll.CountRows);
    }

    [TestMethod]
    public void Clear()
    {
      var coll = new RowErrorCollection(5);
      coll.Add(this, new WarningEventArgs(1, 1, "Message1", 100, 100, "ColName"));
      Assert.AreEqual(1, coll.CountRows);
      coll.Clear();
      Assert.AreEqual(0, coll.CountRows);
    }

    [TestMethod]
    public void TryGetValue()
    {
      var coll = new RowErrorCollection(5);
      coll.Add(this, new WarningEventArgs(1, 1, "Message1", 100, 100, "ColName"));
      Assert.IsTrue(coll.TryGetValue(1, out _));
    }

    [TestMethod]
    public void DisplayByRecordNumber()
    {
      var coll = new RowErrorCollection(5);
      coll.Add(this, new WarningEventArgs(425, 1, "Message1", 100, 100, "ColName"));
      Assert.IsTrue(coll.DisplayByRecordNumber.Contains("Row 425"));
    }
  }
}