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
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class ExtensionsTest
  {
    [TestMethod]
    public void GetIdFromFileName()
    {
      Assert.AreEqual("lo_data", "lo_data_07102019_2303.csv".GetIdFromFileName());
      Assert.AreEqual("lo_data_100", "lo_data_100.csv".GetIdFromFileName());

      Assert.AreEqual("lo_data", "lo_data_201910072303.csv".GetIdFromFileName());
      Assert.AreEqual("lo_data", "lo_data_20191007_11:03 pm.csv".GetIdFromFileName());
    }

    [TestMethod()]
    public void WriteAsyncTest()
    {
    }

    [TestMethod]
    public void ReplacePlaceholder()
    {
      var csv = new CsvFileDummy()
      { FileName= "fileName" };

      Assert.AreEqual("This is fileName a test", "This is {FileName} a test".ReplacePlaceholderWithPropertyValues(csv));
      Assert.AreEqual("This is {nonsense} a test", "This is {nonsense} a test".ReplacePlaceholderWithPropertyValues(csv));
    }

    [TestMethod]
    public void ReplaceProjectPlaceholder()
    {
      Assert.AreEqual("Hello 1526", "Hello {TaskID}".PlaceholderReplace("TaskID", "1526"));
      Assert.AreEqual("Hello Nice World", "Hello #TaskID World".PlaceholderReplace("TaskID", "Nice"));
      Assert.AreEqual("Hello #TaskIDWorld", "Hello #TaskIDWorld".PlaceholderReplace("TaskID", "Nice"));
      Assert.AreEqual("Hello TaskName", "Hello #TaskID".PlaceholderReplace("TaskID", "TaskName"));

    }

    [TestMethod]
    public void CollectionIsEqual()
    {
      var a = new List<int>();
      var b = new List<int>();

      a.Add(1);
      a.Add(10);

      b.Add(10);
      b.Add(1);

      Assert.IsTrue(a.CollectionEqual(b));
    }

    [TestMethod]
    public void CollectionIsNotEqual()
    {
      var a = new List<int>();
      var b = new List<int>();

      a.Add(1);
      a.Add(10);

      b.Add(2);
      b.Add(10);

      Assert.IsFalse(a.CollectionEqual(b));
    }

    [TestMethod]
    public void CollectionIsNotEqualNull()
    {
      var a = new List<int> { 1, 10 };

      Assert.IsFalse(a.CollectionEqual(null));
    }

    [TestMethod]
    public void ExceptionMessages()
    {
      var inner1 = new Exception("InnerException");
      var ex = new Exception("Exception", inner1);
      Assert.AreEqual("Exception\nInnerException", ex.ExceptionMessages());
    }

    [TestMethod]
    public void ExceptionMessages2()
    {
      var ex = new Exception("Exception");
      Assert.AreEqual("Exception", ex.ExceptionMessages());
    }

    [TestMethod]
    public void SourceExceptionMessage()
    {
      var ex = new Exception("Exception L1",
        new Exception("Exception L2", new Exception("Exception L3", new Exception("Exception L4"))));
      Assert.AreEqual("Exception L4", ex.SourceExceptionMessage());
    }

    [TestMethod]
    [Timeout(2000)]
    public void ExceptionMessagesAggregate()
    {
      try
      {
        var task1 = new Task(() =>
        {
          Thread.Sleep(50);
          var task2 = new Task(() =>
          {
            Thread.Sleep(50);
            throw new Exception("<Exception2>");
          });
          var task3 = new Task(() =>
          {
            Thread.Sleep(50);
            throw new Exception("<Exception3>");
          });
          task2.Start();
          task3.Start();
          Task.WaitAll(task2, task3);
          throw new Exception("<Exception1>");
        });

        var task4 = new Task(() =>
        {
          Thread.Sleep(100);
          throw new Exception("<Exception4>");
        });
        var task5 = new Task(() =>
        {
          Thread.Sleep(100);
          throw new Exception("<Exception5>");
        });
        task1.Start();
        task4.Start();
        task5.Start();
        Task.WaitAll(task1, task4, task5);
      }
      catch (Exception ex)
      {
        var message = ex.ExceptionMessages();
        // "<Exception1>" is not reached
        Assert.IsFalse(message.Contains("<Exception1>"));

        Assert.IsTrue(message.Contains("<Exception4>"));
        Assert.IsTrue(message.Contains("<Exception5>"));
        Assert.IsTrue(message.Contains("<Exception2>"));
        Assert.IsTrue(message.Contains("<Exception3>"));
      }
    }

    [TestMethod]
    public void InnerExceptionMessages1()
    {
      var inner1 = new Exception("InnerException");
      var ex = new Exception("Exception", inner1);
      Assert.AreEqual("InnerException", ex.InnerExceptionMessages());
    }

    [TestMethod]
    public void InnerExceptionMessages2()
    {
      var inner1 = new Exception("InnerException1");
      var inner2 = new Exception("InnerException2", inner1);
      var ex = new Exception("Exception", inner2);
      Assert.AreEqual("InnerException2\nInnerException1", ex.InnerExceptionMessages());
    }

    [TestMethod]
    public void InnerExceptionMessages3()
    {
      var inner1 = new Exception("InnerException1");
      var inner2 = new Exception("InnerException2", inner1);
      var inner3 = new Exception("InnerException3", inner2);
      var ex = new Exception("Exception", inner3);
      Assert.AreEqual("InnerException3\nInnerException2", ex.InnerExceptionMessages());
    }

    [TestMethod]
    public void ReplaceCaseInsensitiveChar() =>
      Assert.AreEqual("Text1|Text2", "Text1{0}Text2".ReplaceCaseInsensitive("{0}", '|'));

    [TestMethod]
    public void ReplaceCaseInsensitiveCharEqualLen() =>
      Assert.AreEqual("Text1,Text2", "Text1|Text2".ReplaceCaseInsensitive("|", ','));

    [TestMethod]
    public void ReplaceCaseInsensitiveCharNotFound() =>
      Assert.AreEqual("Text1{0}Text2", "Text1{0}Text2".ReplaceCaseInsensitive("{1}", '|'));

    [TestMethod]
    public void ReplaceCaseInsensitiveEqualLen() =>
      Assert.AreEqual("Text1{0}Text2", "Text1{0}Text2".ReplaceCaseInsensitive("{0}", "{0}"));

    [TestMethod]
    public void ReplaceCaseInsensitiveLonger() =>
      Assert.AreEqual("Text1Test3Text2", "Text1{0}Text2".ReplaceCaseInsensitive("{0}", "Test3"));

    [TestMethod]
    public void ReplaceCaseInsensitiveNotFound() =>
      Assert.AreEqual("Text1{0}Text2", "Text1{0}Text2".ReplaceCaseInsensitive("{1}", "|"));

    [TestMethod]
    public void ReplaceCaseInsensitiveShorter() =>
      Assert.AreEqual("Text1|Text2", "Text1{0}Text2".ReplaceCaseInsensitive("{0}", "|"));


    [TestMethod]
    public void GetRealDataColumnsTest()
    {
      using var dt = new DataTable();
      dt.Columns.Add(new DataColumn { ColumnName = "ID", DataType = typeof(string) });
      dt.Columns.Add(new DataColumn { ColumnName = "ID2", DataType = typeof(string) });
      var cols = dt.GetRealColumns();
      Assert.AreEqual(2, cols.Count());
    }

    [TestMethod]
    public void AssumeExtension()
    {
      Assert.IsTrue("Test.pgp".AssumePgp());
      Assert.IsTrue("Test.GpG".AssumePgp());
      Assert.IsTrue("Test.ZIP".AssumeZip());
      Assert.IsTrue("Test.gz".AssumeGZip());
      Assert.IsFalse("Test.gz".AssumeZip());
      Assert.IsFalse("Test.gz".AssumePgp());
    }

    [TestMethod()]
    public void Description_ShortDescription_Test()
    {
      var recType = RecordDelimiterTypeEnum.Cr;
      Assert.IsTrue(recType.Description().StartsWith("Carriage Return"));
      Assert.AreEqual("CR", recType.ShortDescription());
    }

    [TestMethod()]
    public void NewLineStringTest()
    {
      var recType = RecordDelimiterTypeEnum.Cr;
      Assert.AreEqual("\r", recType.NewLineString());
    }

    [TestMethod()]
    public void AssumeGZipTest()
    {
      Assert.IsTrue("MyFile.gz".AssumeGZip());
      Assert.IsFalse("MyFile.pgp".AssumeGZip());
      Assert.IsFalse("MyFile.gpg".AssumeGZip());
      Assert.IsFalse("MyFile.txt".AssumeGZip());
    }

    [TestMethod()]
    public void AssumePgpTest()
    {
      Assert.IsFalse("MyFile.gz".AssumePgp());
      Assert.IsTrue("MyFile.pgp".AssumePgp());
      Assert.IsTrue("MyFile.gpg".AssumePgp());
      Assert.IsFalse("MyFile.txt".AssumePgp());
    }

    [TestMethod()]
    public void AssumeZipTest()
    {
      Assert.IsFalse("MyFile.gz".AssumeZip());
      Assert.IsTrue("MyFile.zip".AssumeZip());
      Assert.IsFalse("MyFile.txt".AssumeZip());
    }


    [TestMethod]
    public void CollectionCopyStructTest()
    {
      var l1 = new[] { 1, 2, 13, 5, 17 }.ToList();
      var l2 = new List<int>();
      l1.CollectionCopyStruct(l2);
      foreach (var i in l1)
        Assert.AreEqual(i, l2[l1.IndexOf(i)]);
    }

    [TestMethod()]
    public void CountTest()
    {
      var l1 = new[] { 1, 2, 13, 5, 17 };
      Assert.AreEqual(5, l1.Count());
    }

    [TestMethod()]
    public void DataTypeDisplayTest()
    {
      Assert.IsNotNull(DataTypeEnum.Double.Description());
      Assert.IsNotNull(DataTypeEnum.Integer.Description());
      Assert.AreEqual("Boolean", DataTypeEnum.Boolean.Description());
      Assert.AreEqual("GUID / UUID", DataTypeEnum.Guid.Description());
    }

    [TestMethod()]
    public void ExceptionMessagesTest()
    {
      var ex1 = new ApplicationException("AppEx1");
      var ex2 = new ApplicationException("AppEx2", ex1);
      Assert.AreEqual("AppEx1", ex1.ExceptionMessages());
      Assert.AreEqual("AppEx2\nAppEx1", ex2.ExceptionMessages());
    }

    [TestMethod]
    public void CollectionEqualTest1()
    {
      var l1 = new[] { 1, 2, 13, 5, 17 }.ToList();
      var l2 = new[] { 2, 1, 5, 13, 17 }.ToList();
      Assert.IsTrue(l1.CollectionEqual(l1));
      Assert.IsFalse(l1.CollectionEqual(null));
      Assert.IsTrue(l1.CollectionEqual(l2));

      l2.Remove(17);
      Assert.IsFalse(l1.CollectionEqual(l2));
      l2.Remove(19);
      Assert.IsFalse(l1.CollectionEqual(l2));
    }

    [TestMethod]
    public void CollectionEqualTest2()
    {
      var l1 = new[] { 1, 1, 13, 5, 17 }.ToList();
      Assert.IsTrue(l1.CollectionEqual(l1));

      var l2 = new[] { 2, 1, 1, 13, 17 }.ToList();
      Assert.IsFalse(l1.CollectionEqual(l2));

      l1.Add(2);
      Assert.IsFalse(l1.CollectionEqual(l2));
      l2.Add(5);
      Assert.IsTrue(l1.CollectionEqual(l2));
    }

    [TestMethod]
    public void CollectionEqualWithOrder()
    {
      var l1 = new[] { 1, 2, 3, 5, 17 }.ToList();
      Assert.IsTrue(l1.CollectionEqualWithOrder(l1));

      var l2 = new[] { 1, 2, 3, 5 }.ToList();
      Assert.IsFalse(l1.CollectionEqualWithOrder(l2));

      l2.Add(17);
      Assert.IsTrue(l1.CollectionEqualWithOrder(l2));
      l2.Remove(3);
      Assert.IsFalse(l1.CollectionEqualWithOrder(l2));
    }

    [TestMethod()]
    public void ToIntTest()
    {
      Assert.AreEqual(int.MaxValue, long.MaxValue.ToInt());
    }

    [TestMethod()]
    public void ToIntTest1()
    {
      Assert.AreEqual(int.MaxValue, ulong.MaxValue.ToInt());
    }

    [TestMethod]
    public void CollectionHashCode()
    {
      var li1 = new[] { "Hello", "World" };
      Assert.AreEqual(li1.CollectionHashCode(), li1.CollectionHashCode());

      var li2 = new[] { "Hello", "World" };
      Assert.AreEqual(li1.CollectionHashCode(), li2.CollectionHashCode());

      var li3 = new[] { "World", "Hello" };
      Assert.AreNotEqual(li3.CollectionHashCode(), li2.CollectionHashCode());
    }

    [TestMethod]
    public void GetRealColumns()
    {
      var dt = new DataTable();
      Assert.AreEqual(0, dt.GetRealColumns().Count());

      dt.Columns.Add(new DataColumn { ColumnName = ReaderConstants.cEndLineNumberFieldName });
      Assert.AreEqual(0, dt.GetRealColumns().Count());

      var dataColumn = new DataColumn { ColumnName = "Test" };
      dt.Columns.Add(dataColumn);
      Assert.AreEqual(1, dt.GetRealColumns().Count());
      Assert.AreEqual(dataColumn.ColumnName, dt.GetRealColumns().First().ColumnName);
    }

    [TestMethod]
    public void ReplacePlaceholderWithText()
    {
      Assert.AreEqual("Die ist ein Test", "Die ist ein Test".ReplacePlaceholderWithText("Shiny"));
      Assert.AreEqual("Die ist Shiny Test", "Die ist {ein} Test".ReplacePlaceholderWithText("Shiny"));
    }
  }
}