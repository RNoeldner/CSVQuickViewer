using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable CS8625

namespace CsvTools.Tests
{
  [TestClass]
  public class SerializationTest
  {
    //[TestMethod]
    //public void CsvSettingsProperties()
    //{

    //  var test = new CsvFileDummy{ FileName= "Dummy" };
    //  // Excluded are properties that are not serialized or that are calculated

    //  var res = test.SerializeIndentedJson();
    //  Assert.IsTrue(!string.IsNullOrEmpty(res));
    //  var test2 = res.DeserializeText<CsvFileDummy>();
    //  Assert.IsNotNull(test2);
    //  Assert.AreEqual(test.FileName, test2.FileName);

    //  UnitTestStatic.RunSerializeAllProps(test, new[] {nameof(test.FullPath), nameof(test.NoDelimitedFile), nameof(test.Passphrase), nameof(test.RootFolder) , nameof(test.DuplicateQualifierToEscape), nameof(test.QualifyOnlyIfNeeded) });
    //}

    [TestMethod]
    public void CsvSettingsJson()
    {
      var ret = UnitTestStatic.GetTestPath("Read2.setting").DeserializeFileAsync<CsvFileDummy>();
      Assert.IsNotNull(ret);
    }



    [TestMethod]
    public void ColumnMutProperties()
    {
      var input = new ColumnMut("Näme",
        new ValueFormat(DataTypeEnum.DateTime, "XXX", "-", "?", "xx", "_", "=", "Yo", "Nö", "<N>", 3, "|", false, "pat",
          "erp", "read", "Wr", "ou", false))
      { DestinationName = "->", ColumnOrdinal = 13, Convert = true };
      UnitTestStatic.RunSerializeAllProps(input,
        new[]
        {
          nameof(input.CollectionIdentifier), nameof(input.ColumnOrdinal),
          nameof(input.ValueFormat.DecimalSeparator), nameof(input.ValueFormat.NumberFormat),
          nameof(input.ValueFormat.Part), nameof(input.ValueFormat.PartSplitter),nameof(input.ValueFormat.PartToEnd),
          nameof(input.ValueFormat.False), nameof(input.ValueFormat.True)
        });

      var input2 = new ColumnMut("Näme",
        new ValueFormat(DataTypeEnum.TextPart, "XXX", "-", "?", "xx", "_", "=", "Yo", "Nö", "<N>", 3, "|", false, "pat",
          "erp", "read", "Wr", "ou", false))
      { DestinationName = "->", ColumnOrdinal = 13, Convert = true };
      UnitTestStatic.RunSerializeAllProps(input2,
        new[]
        {
          nameof(input.CollectionIdentifier), nameof(input.ColumnOrdinal),
          nameof(input.ValueFormat.DecimalSeparator), nameof(input.ValueFormat.NumberFormat),
          nameof(input.ValueFormat.DateFormat), nameof(input.ValueFormat.DateSeparator), nameof(input.ValueFormat.TimeSeparator),
          nameof(input.ValueFormat.False), nameof(input.ValueFormat.True)
        });
    }

    [TestMethod]
    public void ValueFormatMutProperties()
    {
      var input = new ValueFormatMut(DataTypeEnum.DateTime, "XXX", "-", "?", "xx", "_", "=", "Yo", "Nö", "<N>", 3, "|",
        false, "pat",
        "erp", "read", "Wr", "ou", false);
      UnitTestStatic.RunSerializeAllProps(input);
    }

    [TestMethod]
    [TestCategory("Serialization")]
    public void CsvFile()
    {
      var input = new CsvFileDummy { FileName= "MyTest.txt" };
      input.FieldQualifierChar =  '\'';

      var output = UnitTestStatic.RunSerialize(input);

      Assert.AreEqual(input.FileName, output.FileName);
      Assert.AreEqual(input.FieldQualifierChar, output.FieldQualifierChar);
    }

    [TestMethod]
    [TestCategory("Serialization")]
    public void Mapping()
    {
      var input = new Mapping("a", "Fld2", true);

      var output = UnitTestStatic.RunSerialize(input);
      Assert.AreEqual(input.FileColumn, output.FileColumn);
      Assert.AreEqual(input.TemplateField, output.TemplateField);
    }

    [TestMethod]
    [TestCategory("Serialization")]
    public void MappingCollection()
    {
      var input = new MappingCollection { new Mapping("a", "fld2", true, true), new Mapping("b", "fld1", false, true) };

      var output = UnitTestStatic.RunSerialize(input);

      Assert.AreEqual(input.Count, output.Count);
    }

    [TestMethod]
    [TestCategory("Serialization")]
    public void SampleAndErrorsInformation()
    {
      var input = new SampleAndErrorsInformation(-1,
        new[] { new SampleRecordEntry(10, true, "Error1"), new SampleRecordEntry(12, false, "Error2") },
        new[] { new SampleRecordEntry(11, true, "Sample1"), new SampleRecordEntry(15, false, "Sample2") }, 1);

      var output = UnitTestStatic.RunSerialize(input);

      Assert.AreEqual(input.Errors.Count, output.Errors.Count);
      Assert.AreEqual(input.Samples.Count, output.Samples.Count);
    }

    [TestMethod]
    [TestCategory("Serialization")]
    public void ValueFormatMutable()
    {
      var input = new ValueFormatMut(DataTypeEnum.Numeric, numberFormat: "x.00", decimalSeparator: ",");

      var output = UnitTestStatic.RunSerialize(input);
      Assert.AreEqual(input.NumberFormat, output.NumberFormat);
      Assert.AreEqual(input.DecimalSeparator, output.DecimalSeparator);
      Assert.AreEqual(input.DateFormat, output.DateFormat);
    }

    [TestMethod]
    public void ValueFormatMutableProperties()
    {
      var input = new ValueFormatMut(DataTypeEnum.DateTime, "XXX", "-", "?", "xx", "_", "=", "Yo", "Nö", "<N>", 3, "|",
        false, "pat",
        "erp", "read", "Wr", "ou", false);
      UnitTestStatic.RunSerializeAllProps(input);
    }
  }
}