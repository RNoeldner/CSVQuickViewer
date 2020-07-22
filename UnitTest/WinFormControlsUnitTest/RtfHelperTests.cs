using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass]
  public class RtfHelperTests
  {
    [TestMethod]
    public void AddCharTest()
    {
      var rtfHelper = new RtfHelper();

      rtfHelper.AddChar(1, 'Ö');
      rtfHelper.AddRtf(1, "   ");
      rtfHelper.AddChar(2, '\\');
      rtfHelper.AddRtf(1, "   ");
      rtfHelper.AddChar(2, '[');
      rtfHelper.AddRtf(1, "   ");
      rtfHelper.AddChar(3, ']');
      rtfHelper.AddRtf(1, "   ");
      rtfHelper.AddChar(3, '{');
      rtfHelper.AddRtf(1, "   ");
      rtfHelper.AddChar(2, '{');
    }

    [TestMethod]
    public void AddParagraphTest()
    {
      var rtfHelper = new RtfHelper();
      rtfHelper.AddParagraph("Par1");
      rtfHelper.AddParagraph();
      rtfHelper.AddParagraph("Par3");
    }
  }
}