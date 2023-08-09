using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass()]
  public class ColumnFormatterFactoryTests
  {
    [TestMethod()]
    public void GetColumnFormatterTest()
    {
      var fac1 = ColumnFormatterFactory.GetColumnFormatter(new ValueFormat(DataTypeEnum.Binary));
      Assert.IsInstanceOfType(fac1, typeof(BinaryFormatter) );

      var fac2 = ColumnFormatterFactory.GetColumnFormatter(new ValueFormat(DataTypeEnum.TextToHtml));
      Assert.IsInstanceOfType(fac2, typeof(TextToHtmlFormatter) );
      
      var fac3 = ColumnFormatterFactory.GetColumnFormatter(new ValueFormat(DataTypeEnum.TextReplace));
      Assert.IsInstanceOfType(fac3, typeof(TextReplaceFormatter) );

      var fac4 = ColumnFormatterFactory.GetColumnFormatter(new ValueFormat(DataTypeEnum.TextUnescape));
      Assert.IsInstanceOfType(fac4, typeof(TextUnescapeFormatter) );
      
    }
  }
}