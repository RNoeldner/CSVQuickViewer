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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  [SuppressMessage("ReSharper", "UseAwaitUsing")]
  public class RowErrorCollectionTests
  {
    [TestMethod]
    public void RowErrorCollection() => Assert.IsNotNull(new RowErrorCollection());


    [TestMethod]
    public void Add()
    {
      // Arrange
      var testRowErrorCol = new RowErrorCollection();

      // Act & Assert
      foreach (var data in new[]       {
        new { Row = 1, Column = 1, Message = "Message1" },
        new { Row = 2, Column = 1, Message = "Message1" },
        new { Row = 3, Column = 1, Message = "Message1" },
        new { Row = 4, Column = 1, Message = "Message1" },
        new { Row = 5, Column = 1, Message = "Message1" } })
      {
        testRowErrorCol.Add(this, new WarningEventArgs(data.Row, data.Column, data.Message, 100 + data.Row, 100 + data.Row, "ColName"));
        Assert.AreEqual(data.Row, testRowErrorCol.CountRows, $"After adding row {data.Row}, CountRows should be {data.Row}");
      }

      // Add a second message to an existing row/column
      testRowErrorCol.Add(this, new WarningEventArgs(5, 1, "Message2", 104, 104, "ColName"));

      // Assert row count did not increase
      Assert.AreEqual(5, testRowErrorCol.CountRows, "CountRows should not increase when adding a new message to an existing row/column");

      // Assert the message was appended correctly
      testRowErrorCol.TryGetValue(5, out var messagesforRow);
      StringAssert.Contains(messagesforRow[1], "Message1");
      StringAssert.Contains(messagesforRow[1], "Message2");
    }

    [TestMethod]
    public void Clear()
    {
      var coll = new RowErrorCollection();
      coll.Add(this, new WarningEventArgs(1, 1, "Message1", 100, 100, "ColName"));
      Assert.AreEqual(1, coll.CountRows);
      coll.Clear();
      Assert.AreEqual(0, coll.CountRows);
    }

    [TestMethod]
    public void TryGetValue()
    {
      var coll = new RowErrorCollection();
      coll.Add(this, new WarningEventArgs(1, 1, "Message1", 100, 100, "ColName"));
      Assert.IsTrue(coll.TryGetValue(1, out _));
    }

    [TestMethod]
    public void DisplayByRecordNumber()
    {
      var coll = new RowErrorCollection();
      coll.Add(this, new WarningEventArgs(425, 1, "Message1", 100, 100, "ColName"));
      Assert.IsTrue(coll.DisplayByRecordNumber.Contains("Row 425"));
    }
  }
}
