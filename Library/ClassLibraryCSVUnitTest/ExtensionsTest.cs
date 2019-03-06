using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CsvTools.Tests
{
  [TestClass]
  public class ExtensionsTest
  {
    [TestMethod()]
    public void ReplaceProjectPlaceholder()
    {
      Assert.AreEqual("Hello 1526", "Hello {TaskID}".PlaceholderReplace("TaskID", "1526"));
      Assert.AreEqual("Hello TaskName", "Hello #TaskID".PlaceholderReplace("TaskID", "TaskName"));
      Assert.AreEqual("Hello Nice World", "Hello #TaskID World".PlaceholderReplace("TaskID", "Nice"));
      Assert.AreEqual("Hello #TaskIDWorld", "Hello #TaskIDWorld".PlaceholderReplace("TaskID", "Nice"));
    }

    [TestMethod]
    public void RemoveMappingWithoutSourceTest()
    {
      var fileSetting = new CsvFile();

      var existingColumns = new[] { "Col1", "Col2", "Col3" };
      var additionalColumns = new[] { "Col4", "Col5" };
      var checkColumns = new List<string>();
      foreach (var col in existingColumns)
      {
        fileSetting.Mapping.Add(new Mapping { FileColumn = col, TemplateField = col });
        checkColumns.Add(col);
      }

      foreach (var col in additionalColumns)
        fileSetting.Mapping.Add(new Mapping { FileColumn = col, TemplateField = col });

      checkColumns.Add("Col6");
      var result = fileSetting.RemoveMappingWithoutSource(checkColumns).ToList();

      foreach (var col in additionalColumns)
        Assert.IsTrue(result.Contains(col), col);
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
      var a = new List<int>();
      a.Add(1);
      a.Add(10);

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
      Assert.AreEqual("InnerException3\nInnerException2", ex.InnerExceptionMessages(2));
    }

    [TestMethod]
    public void ReplaceCaseInsensitiveChar()
    {
      Assert.AreEqual("Text1|Text2", "Text1{0}Text2".ReplaceCaseInsensitive("{0}", '|'));
    }

    [TestMethod]
    public void ReplaceCaseInsensitiveCharEqualLen()
    {
      Assert.AreEqual("Text1,Text2", "Text1|Text2".ReplaceCaseInsensitive("|", ','));
    }

    [TestMethod]
    public void ReplaceCaseInsensitiveCharNotFound()
    {
      Assert.AreEqual("Text1{0}Text2", "Text1{0}Text2".ReplaceCaseInsensitive("{1}", '|'));
    }

    [TestMethod]
    public void ReplaceCaseInsensitiveEqualLen()
    {
      Assert.AreEqual("Text1{0}Text2", "Text1{0}Text2".ReplaceCaseInsensitive("{0}", "{0}"));
    }

    [TestMethod]
    public void ReplaceCaseInsensitiveLonger()
    {
      Assert.AreEqual("Text1Test3Text2", "Text1{0}Text2".ReplaceCaseInsensitive("{0}", "Test3"));
    }

    [TestMethod]
    public void ReplaceCaseInsensitiveNotFound()
    {
      Assert.AreEqual("Text1{0}Text2", "Text1{0}Text2".ReplaceCaseInsensitive("{1}", "|"));
    }

    [TestMethod]
    public void ReplaceCaseInsensitiveShorter()
    {
      Assert.AreEqual("Text1|Text2", "Text1{0}Text2".ReplaceCaseInsensitive("{0}", "|"));
    }

    [TestMethod]
    public void GetRealDataColumnsTest()
    {
      using (var dt = new DataTable())
      {
        dt.Columns.Add(new DataColumn
        {
          ColumnName = "ID",
          DataType = typeof(string)
        });
        dt.Columns.Add(new DataColumn
        {
          ColumnName = "ID2",
          DataType = typeof(string)
        });
        var cols = dt.GetRealColumns();
        Assert.AreEqual(2, cols.Count());
      }
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

    [TestMethod]
    public void CollectionEqualTest()
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
    public void AssumePGPTest()
    {
      Assert.IsTrue(".pgp".AssumePgp());
      Assert.IsTrue(".gpg".AssumePgp());
      Assert.IsFalse("Test.pgp.txt".AssumePgp());
      Assert.IsFalse("Hello.gpg.zip".AssumePgp());
    }
  }
}