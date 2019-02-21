using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass]
  public class StructuredFileTests
  {
    private readonly StructuredFile m_StructuredFile = new StructuredFile();

    [TestMethod]
    public void StructuredFileClone()
    {
      var test = m_StructuredFile.Clone();
      Assert.AreNotSame(m_StructuredFile, test);
      Assert.IsInstanceOfType(test, typeof(StructuredFile));

      m_StructuredFile.AllPropertiesEqual(test);
      // Test Properties that are not tested

      Assert.AreEqual(m_StructuredFile.Mapping.Count, test.Mapping.Count, "FieldMapping");
      Assert.AreEqual(TrimmingOption.Unquoted, test.TrimmingOption, "TrimmingOption");
      Assert.IsTrue(m_StructuredFile.Mapping.CollectionEqualWithOrder(test.Mapping), "Mapping");
      Assert.IsTrue(m_StructuredFile.Column.CollectionEqualWithOrder(test.Column), "Column");
      Assert.IsTrue(m_StructuredFile.FileFormat.Equals(test.FileFormat), "FileFormat");

      Assert.IsTrue(test.Equals(m_StructuredFile), "Equals");
    }

    [TestMethod]
    public void CsvFileCopyTo()
    {
      var test = new StructuredFile();
      m_StructuredFile.CopyTo(test);
      m_StructuredFile.AllPropertiesEqual(test);
      // Test Properties that are not tested
      Assert.AreEqual(m_StructuredFile.Mapping.Count, test.Mapping.Count, "FieldMapping");
      Assert.AreEqual(TrimmingOption.Unquoted, test.TrimmingOption, "TrimmingOption");
      Assert.IsTrue(m_StructuredFile.Mapping.CollectionEqualWithOrder(test.Mapping), "Mapping");
      Assert.IsTrue(m_StructuredFile.Column.CollectionEqualWithOrder(test.Column), "Column");
      Assert.IsTrue(m_StructuredFile.FileFormat.Equals(test.FileFormat), "FileFormat");
      Assert.IsTrue(test.Equals(m_StructuredFile), "Equals");
    }

    [TestMethod]
    public void GetFileReader()
    {
      try
      {
        var res = m_StructuredFile.GetFileReader();
        Assert.Fail("Should throw error");
      }
      catch (NotImplementedException)
      {
      }
    }

    [TestMethod]
    public void GetFileWriter()
    {
      using (var cts = new CancellationTokenSource())
      {
        var res = m_StructuredFile.GetFileWriter(cts.Token);
        Assert.IsInstanceOfType(res, typeof(IFileWriter));
      }
    }

    [TestInitialize]
    public void Init()
    {
      m_StructuredFile.FileName = "StructuredFile.txt";
      m_StructuredFile.Header = "Header";
      Assert.AreEqual("Header", m_StructuredFile.Header);
      m_StructuredFile.Header = null;
      Assert.AreEqual(string.Empty, m_StructuredFile.Header);

      m_StructuredFile.JSONEncode = false;
      Assert.IsFalse(m_StructuredFile.JSONEncode);

      m_StructuredFile.Row = "Row";
      Assert.AreEqual("Row", m_StructuredFile.Row);
      m_StructuredFile.Row = null;
      Assert.AreEqual(string.Empty, m_StructuredFile.Row);

      m_StructuredFile.Footer = "Footer";
      Assert.AreEqual("Footer", m_StructuredFile.Footer);
      m_StructuredFile.Footer = null;
      Assert.AreEqual(string.Empty, m_StructuredFile.Footer);

      m_StructuredFile.Mapping.Clear();
      m_StructuredFile.Mapping.Add(new Mapping { FileColumn = "Fld1", TemplateField = "FldA", Attention = true });
      m_StructuredFile.Mapping.Add(new Mapping { FileColumn = "Fld2", TemplateField = "FldB", Attention = true });
      Assert.AreEqual(2, m_StructuredFile.Mapping.Count, "FieldMapping");

      m_StructuredFile.Column.Clear();
      m_StructuredFile.Column.Add(new Column
      {
        ColumnOrdinal = 1,
        DataType = DataType.Integer,
        Ignore = false,
        Convert = true,
        Name = "ID"
      });
      m_StructuredFile.Column.Add(new Column { ColumnOrdinal = 2, Name = "Name" });
    }

    [TestMethod]
    public void StructuredFileTestCTOR()
    {
      var test = new StructuredFile();
      Assert.IsTrue(string.IsNullOrEmpty(test.FileName));

      var test2 = new StructuredFile("Hello");
      Assert.AreEqual("Hello", test2.FileName);
    }
  }
}