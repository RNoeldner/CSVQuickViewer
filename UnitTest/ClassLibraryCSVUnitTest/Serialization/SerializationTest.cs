using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

#pragma warning disable CS8625

namespace CsvTools.Tests
{
  [TestClass]
  public class SerializationTest
  {

    [TestMethod]
    public void ColumnMut()
    {
      var input = new ColumnMut("Näme", new ValueFormat(DataTypeEnum.DateTime, "XXX", "-", "?", "xx", "_", "=", "Yo", "Nö", "<N>", 3, "|", false, "pat", "erp", "read", "Wr", "ou", false))
      {
        DestinationName = "->",
        ColumnOrdinal = 13,
        Convert = true
      };
      var output = RunSerlializer(input, true, false);
      Assert.AreEqual(input.Name, output.Name);
      Assert.AreEqual(input.DestinationName, output.DestinationName);
    }

    [TestMethod]
    [TestCategory("Serialization")]
    public async Task CsvFile()
    {
      var input = new CsvFile("MyTest.txt");
      input.FieldQualifier = "'";

      var output = RunSerlializer(input);

      Assert.AreEqual(input.FileName, output.FileName);
      Assert.AreEqual(input.FieldQualifier, output.FieldQualifier);
    }

    [TestMethod]
    [TestCategory("Serialization")]
    public async Task Mapping()
    {
      var input = new Mapping()
      {
        FileColumn = "a",
        TemplateField ="Fld2",
        Attention = true
      };

      var output = RunSerlializer(input);
      Assert.AreEqual(input.FileColumn, output.FileColumn);
      Assert.AreEqual(input.TemplateField, output.TemplateField);
    }

    [TestMethod]
    [TestCategory("Serialization")]
    public async Task MappingCollection()
    {
      var input = new MappingCollection();

      input.Add(new Mapping("a", "fld2", true, true));
      input.Add(new Mapping("b", "fld1", false, true));

      var output = RunSerlializer(input);

      Assert.AreEqual(input.Count, output.Count);
    }

    [TestMethod]
    [TestCategory("Serialization")]
    public async Task SampleAndErrorsInformation()
    {
      var input = new SampleAndErrorsInformation();

      input.Errors.Add(new SampleRecordEntry(10, true, "Error1"));
      input.Errors.Add(new SampleRecordEntry(15, false, "Error2"));

      input.Samples.Add(new SampleRecordEntry(11, true, "Sample1"));
      input.Samples.Add(new SampleRecordEntry(15, false, "Sample2"));

      var output = RunSerlializer(input);

      Assert.AreEqual(input.Errors.Count, output.Errors.Count);
      Assert.AreEqual(input.Samples.Count, output.Samples.Count);
    }

    [TestMethod]
    [TestCategory("Serialization")]
    public async Task ValueFormatMutable()
    {
      var input = new ValueFormatMut(DataTypeEnum.Numeric, numberFormat: "x.00", decimalSeparator: ",");

      var output = RunSerlializer(input, false);
      Assert.AreEqual(input.NumberFormat, output.NumberFormat);
      Assert.AreEqual(input.DecimalSeparator, output.DecimalSeparator);
      Assert.AreEqual(input.DateFormat, output.DateFormat);
    }

    private static T RunSerlializer<T>(T obj, bool includeXml = true, bool includeJson = true) where T : class
    {
      if (obj == null)
        throw new ArgumentNullException("obj");

      T ret = obj;

      if (includeXml)
      {
        var serializer = new XmlSerializer(typeof(T));
        var testXml = obj.SerializeIndentedXml(serializer);
        Assert.IsFalse(string.IsNullOrEmpty(testXml));
        using TextReader reader = new StringReader(testXml);
        ret = serializer.Deserialize(reader) as T;
        Assert.IsNotNull(ret);
      }
      if (includeJson)
      {
        var testJson = obj.SerializeIndentedJson();
        Assert.IsFalse(string.IsNullOrEmpty(testJson));
        var pos = testJson.IndexOf("Specified\":", StringComparison.OrdinalIgnoreCase);
        Assert.IsFalse(pos != -1, $"Conteins Specifed as position {pos} in \n{testJson}");
        ret = JsonConvert.DeserializeObject<T>(testJson, ClassLibraryCsvExtensionMethods.JsonSerializerSettings.Value);
        Assert.IsNotNull(ret);
      }

      return ret;
    }
  }
}