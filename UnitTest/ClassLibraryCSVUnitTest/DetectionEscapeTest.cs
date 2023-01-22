using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable ConvertToUsingDeclaration

namespace CsvTools.Tests
{
  [TestClass]
  public class DetectionEscapeTest
  {

    [TestMethod]
    public async Task GuessHasHeaderAsync_NotEscaped()
    {
      using (var stream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt"))))
      using (var reader = new ImprovedTextReader(stream))
      {
        Assert.AreEqual("", await reader.InspectEscapePrefixAsync(",", "\"", UnitTestStatic.Token));
      }

      using (var stream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt"))))
        Assert.AreEqual("",
          await stream.InspectEscapePrefixAsync(Encoding.UTF8.CodePage, 0, ",", "\"", UnitTestStatic.Token));
    }

    [TestMethod]
    public async Task GuessHasHeaderAsync_EscapedCharacterAtEndOfRowDelimiter()
    {
      using (var stream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("EscapedCharacterAtEndOfRowDelimiter.txt"))))
      using (var reader = new ImprovedTextReader(stream))
      {
        Assert.AreEqual("", await reader.InspectEscapePrefixAsync(",", "\"", UnitTestStatic.Token));
      }
    }


    [TestMethod]
    public async Task GuessHasHeaderAsync_Escaped()
    {
      using (var stream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicEscapedCharacters.txt"))))
      using (var reader = new ImprovedTextReader(stream))
      {
        Assert.AreEqual("\\", await reader.InspectEscapePrefixAsync(",", "\"", UnitTestStatic.Token));
      }

      using (var stream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicEscapedCharacters.txt"))))
        Assert.AreEqual("\\",
          await stream.InspectEscapePrefixAsync(Encoding.UTF8.CodePage, 0, ",", "\"", UnitTestStatic.Token));
    }
  }
}