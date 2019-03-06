using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class FieldMappingTest
  {
    private readonly Mapping m_FieldMapping = new Mapping();

    [TestMethod]
    public void Equals()
    {
      var notEqual = new Mapping();
      notEqual.FileColumn = m_FieldMapping.FileColumn + "a";
      notEqual.TemplateField = m_FieldMapping.TemplateField;

      var equal = new Mapping();
      equal.FileColumn = m_FieldMapping.FileColumn;
      equal.TemplateField = m_FieldMapping.TemplateField;

      Assert.IsTrue(m_FieldMapping.Equals(equal));
      Assert.IsFalse(m_FieldMapping.Equals(notEqual));
    }

    [TestMethod]
    public void EqualsNull()
    {
      Assert.IsFalse(m_FieldMapping.Equals(null));
    }

    [TestInitialize]
    public void Init()
    {
      m_FieldMapping.FileColumn = "A";
      m_FieldMapping.TemplateField = "B";
    }
  }
}