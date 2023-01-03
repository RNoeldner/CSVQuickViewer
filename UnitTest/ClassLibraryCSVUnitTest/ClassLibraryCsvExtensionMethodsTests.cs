using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CsvTools.Tests
{
  [TestClass()]
  public class ClassLibraryCsvExtensionMethodsTests
  {
    [TestMethod]
    public void GetDescription()
    {
      Assert.AreEqual(string.Empty, "".GetDescription());
      Assert.AreEqual("Horizontal Tab", "\t".GetDescription());
      Assert.AreEqual("Comma: ,", ",".GetDescription());
      Assert.AreEqual("Pipe: |", "|".GetDescription());
      Assert.AreEqual("Semicolon: ;", ";".GetDescription());
      Assert.AreEqual("Colon: :", ":".GetDescription());
      Assert.AreEqual("Quotation marks: \"", "\"".GetDescription());
      Assert.AreEqual("Apostrophe: '", "'".GetDescription());
      Assert.AreEqual("Space", " ".GetDescription());
      Assert.AreEqual("Backslash: \\", "\\".GetDescription());
      Assert.AreEqual("Slash: /", '/'.GetDescription());
      Assert.AreEqual("Unit Separator: Char 31", "US".WrittenPunctuation().GetDescription());
      Assert.AreEqual("Unit Separator: Char 31", "Unit Separator".WrittenPunctuation().GetDescription());
      Assert.AreEqual("Unit Separator: Char 31", "char(31)".WrittenPunctuation().GetDescription());

      Assert.AreEqual("Group Separator: Char 29", "GS".WrittenPunctuation().GetDescription());
      Assert.AreEqual("Record Separator: Char 30", "RS".WrittenPunctuation().GetDescription());
      Assert.AreEqual("File Separator: Char 28", "FS".WrittenPunctuation().GetDescription());
    }

    [TestMethod]
    public void Move()
    {
      var items = new List<int>(new[] { 0, 1, 2, 3, 4, 5 });

      items.Move(1, 2);
      Assert.AreEqual(0, items[0]);
      Assert.AreEqual(2, items[1]);
      Assert.AreEqual(1, items[2]);
      Assert.AreEqual(3, items[3]);

      items.Move(2, 1);
      Assert.AreEqual(0, items[0]);
      Assert.AreEqual(1, items[1]);
      Assert.AreEqual(2, items[2]);
      Assert.AreEqual(3, items[3]);

      items.Move(0, 1);
      Assert.AreEqual(1, items[0]);
      Assert.AreEqual(0, items[1]);

      items.Move(5, 4);
      Assert.AreEqual(4, items[5]);
      Assert.AreEqual(5, items[4]);
    }

#if XmlSerialization
    [TestMethod()]
    public void SerializeIndented()
    {
      var csv = new CsvFile(id: string.Empty, fileName: "test") { TemplateName = "Check" };
      csv.MappingCollection.Add(new Mapping("Test", "Test"));
      var ser = new XmlSerializer(typeof(CsvFile));

      var res = csv.SerializeIndentedXml(ser);
      Assert.IsTrue(res.Contains("  "));
    }
#endif

    [TestMethod()]
    public void JsonIndented()
    {
      var csv = new CsvFile(id: string.Empty, fileName: "test") { TemplateName = "Check" };
      csv.MappingCollection.Add(new Mapping("Test", "Test"));

      var res = csv.SerializeIndentedJson();
      Assert.IsTrue(res.Contains("  "));
    }

    [TestMethod()]
    public void AssumeDeflateTest()
    {
      Assert.IsFalse("test.gzip".AssumeDeflate());
      Assert.IsFalse("test.pgp".AssumeDeflate());
      Assert.IsFalse("test.gz".AssumeDeflate());
      Assert.IsTrue("test.cmp".AssumeDeflate());
      Assert.IsTrue("test.dfl".AssumeDeflate());
    }

    [TestMethod()]
    public void AssumeGZipTest()
    {
      Assert.IsTrue("test.gzip".AssumeGZip());
      Assert.IsFalse("test.pgp".AssumeGZip());
      Assert.IsTrue("test.gz".AssumeGZip());
      Assert.IsTrue("test.GZ".AssumeGZip());
      Assert.IsFalse("test.cmp".AssumeGZip());
      Assert.IsFalse("test.dfl".AssumeGZip());
    }

    [TestMethod()]
    public void AssumePgpTest()
    {
      Assert.IsTrue("test.pgp".AssumePgp());
      Assert.IsTrue("test.GPG".AssumePgp());
      Assert.IsFalse("test.gz".AssumePgp());
    }

    [TestMethod()]
    public void AssumeZipTest()
    {
      Assert.IsTrue("test.zip".AssumeZip());
      Assert.IsTrue("test.Zip".AssumeZip());
      Assert.IsFalse("test.gz".AssumeZip());
    }

    [TestMethod()]
    public void CollectionCopyTest()
    {
      var list1 = new List<string>();
      var list2 = new List<string>();
      list2.Add("Hello");
      list1.CollectionCopy(list2);
      Assert.AreEqual(0, list2.Count);

      list1.Add("Hello");
      list1.Add("World");
      list1.CollectionCopy(list2);
      Assert.AreEqual(2, list2.Count);
    }

    [TestMethod()]
    public void CollectionCopyStructTest()
    {
    }

    [TestMethod()]
    public void CountTest()
    {
      var list = new List<int> { 1, 2, 3 };
      Assert.AreEqual(2, ClassLibraryCsvExtensionMethods.Count(list.Where(x => x < 3)));
      Assert.AreEqual(3, ClassLibraryCsvExtensionMethods.Count(list));
    }

    [TestMethod()]
    public void DataTypeDisplayTest()
    {
      Assert.AreEqual("Text", DataTypeEnum.String.Description());
      Assert.AreEqual("Date Time", DataTypeEnum.DateTime.Description());
      foreach (DataTypeEnum type in Enum.GetValues(typeof(DataTypeEnum)))
        Assert.IsNotNull(type.Description());
    }

    [TestMethod()]
    public void DescriptionTest()
    {
      foreach (RecordDelimiterTypeEnum type in Enum.GetValues(typeof(RecordDelimiterTypeEnum)))
        Assert.IsNotNull(type.Description());
    }

    [TestMethod()]
    public void ExceptionMessagesTest()
    {
      var ex = new ArgumentException("name1");
      var ex1 = new ArgumentException("name2", ex);
      var res = ex1.ExceptionMessages();
      Assert.IsTrue(res.Contains("name1"));
      Assert.IsTrue(res.Contains("name2"));
    }

    [TestMethod()]
    public void GetDataTypeTest()
    {
      Assert.AreEqual(DataTypeEnum.Integer, typeof(int).GetDataType());
      Assert.AreEqual(DataTypeEnum.Integer, typeof(long).GetDataType());
      Assert.AreEqual(DataTypeEnum.String, typeof(string).GetDataType());
      Assert.AreEqual(DataTypeEnum.Guid, typeof(Guid).GetDataType());
      Assert.AreEqual(DataTypeEnum.DateTime, typeof(DateTime).GetDataType());
      Assert.AreEqual(DataTypeEnum.DateTime, typeof(TimeSpan).GetDataType());
      Assert.AreEqual(DataTypeEnum.Boolean, typeof(bool).GetDataType());
    }

    [TestMethod()]
    public void GetDescriptionTest()
    {
      Assert.AreEqual("Horizontal Tab", '\t'.GetDescription());
      Assert.AreEqual("Space", ' '.GetDescription());
      Assert.IsTrue('\\'.GetDescription().Contains("Backslash"));
      Assert.IsTrue('\''.GetDescription().Contains("\'"));
    }

    [TestMethod()]
    public void GetDescriptionTest1()
    {
      Assert.AreEqual("Horizontal Tab", "\t".GetDescription());
    }

    [TestMethod()]
    public void GetIdFromFileNameTest()
    {
    }

    [TestMethod()]
    public void GetNetTypeTest()
    {
    }

    [TestMethod()]
    public void GetRealColumnsTest()
    {
    }

    [TestMethod()]
    public void GetRealDataColumnsTest()
    {
    }

    [TestMethod()]
    public void InnerExceptionMessagesTest()
    {
    }

    [TestMethod()]
    public void NewLineStringTest()
    {
    }


    [TestMethod()]
    public void PlaceholderReplaceTest()
    {
    }

    [TestMethod()]
    public void PlaceholderReplaceFormatTest()
    {
    }

    [TestMethod()]
    public void ReplaceCaseInsensitiveTest()
    {
    }

    [TestMethod()]
    public void ReplaceCaseInsensitiveTest1()
    {
    }

    [TestMethod()]
    public void ReplaceDefaultsTest()
    {
    }

    [TestMethod()]
    public void ReplacePlaceholderWithPropertyValuesTest()
    {
    }

    [TestMethod()]
    public void ReplacePlaceholderWithTextTest()
    {
    }

    [TestMethod()]
    public void SetMaximumTest()
    {
    }

    [TestMethod()]
    public void SourceExceptionMessageTest()
    {
    }

    [TestMethod()]
    public void StringToCharTest()
    {
    }

    [TestMethod()]
    public void ToIntTest()
    {
    }

    [TestMethod()]
    public void ToIntTest1()
    {
    }

    [TestMethod()]
    public void ToIntTest2()
    {
    }

    [TestMethod()]
    public void ToIntTest3()
    {
    }

    [TestMethod()]
    public void ToInt64Test()
    {
    }

    [TestMethod()]
    public void ToInt64Test1()
    {
    }

    [TestMethod()]
    public void ToStringHandle0Test()
    {
    }

    [TestMethod()]
    public void WriteAsyncTest()
    {
    }

    [TestMethod()]
    public void WrittenPunctuationTest()
    {
    }

    [TestMethod()]
    public void WrittenPunctuationToCharTest()
    {
    }

    [TestMethod()]
    public void CollectionEqualTest()
    {
    }

    [TestMethod()]
    public void CollectionEqualWithOrderTest()
    {
    }

    [TestMethod()]
    public void CollectionHashCodeTest()
    {
    }
  }
}