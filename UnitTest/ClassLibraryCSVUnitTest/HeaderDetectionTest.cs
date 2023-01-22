using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable UseAwaitUsing
// ReSharper disable ConvertToUsingDeclaration

namespace CsvTools.Tests
{
  [TestClass]
  public class HeaderDetectionTest
  {
    [TestMethod]
    public async Task GuessHasHeaderAsync()
    {
      using (var stream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt"))))
      using (var reader = new ImprovedTextReader(stream))
      {
        Assert.AreEqual("", await reader.InspectHasHeaderAsync("#", ',', '\0', '\0', UnitTestStatic.Token));
      }

      using (var stream =
             FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("HandlingDuplicateColumnNames.txt"))))
      using (var reader = new ImprovedTextReader(stream))
      {
        Assert.IsFalse(
          string.IsNullOrEmpty(await reader.InspectHasHeaderAsync("#", ',', '\0', '\0', UnitTestStatic.Token)));
      }

      using (var stream =
             FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("DifferentColumnDelimiter.txt"))))
      using (var reader = new ImprovedTextReader(stream))
      {
        Assert.IsFalse(
          string.IsNullOrEmpty(await reader.InspectHasHeaderAsync("", ',', '\0', '\0', UnitTestStatic.Token)));
      }

      using (var stream = new FileStream(UnitTestStatic.GetTestPath("Sessions.txt"), FileMode.Open))
      using (var reader = new ImprovedTextReader(stream))
      {
        Assert.AreEqual("",await reader.InspectHasHeaderAsync("#", '\t', '\0', '\0', UnitTestStatic.Token));
      }

      using (var stream = new FileStream(UnitTestStatic.GetTestPath("TrimmingHeaders.txt"), FileMode.Open))
      using (var reader = new ImprovedTextReader(stream))
      {
        var res = await reader.InspectHasHeaderAsync("#", ',', '\0', '\0', UnitTestStatic.Token);
        Assert.IsTrue(res.Contains("very short"), res);
      }
    }

    [TestMethod]
    public async Task GuessHeaderAllFormatsAsync()
    {
      using var improvedStream =
        FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("AllFormats.txt")));
      var result = await improvedStream.InspectHasHeaderAsync(65001, 0, "", "\t", "", "", UnitTestStatic.Token);
      Assert.IsNotNull(result);
      Assert.IsTrue(string.IsNullOrEmpty(result));
    }

    [TestMethod]
    public async Task GuessHeaderBasicCsv()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt")));
      var result = await improvedStream.InspectHasHeaderAsync(1200, 0, "", ",", "", "", UnitTestStatic.Token);
      Assert.IsNotNull(result);
      Assert.IsTrue(string.IsNullOrEmpty(result));
    }

    [TestMethod]
    public async Task GuessHeaderBasicEscapedCharactersAsync()
    {
      using var improvedStream =
        FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicEscapedCharacters.txt")));
      var result = await improvedStream.InspectHasHeaderAsync(65001, 0, "", ",", "", "", UnitTestStatic.Token);
      Assert.IsNotNull(result);
      Assert.IsFalse(string.IsNullOrEmpty(result));
      Assert.IsTrue(result.EndsWith("very short"));
      // if is a or 'a\' does not matter
      Assert.IsTrue(result.Contains("'b', 'c', 'd', 'e', 'f'"));
    }

    [TestMethod]
    public async Task GuessHeaderLongHeadersAsync()
    {
      using var improvedStream =
        FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("LongHeaders.txt")));
      var result = await improvedStream.InspectHasHeaderAsync(65001, 0, "#", ",", "\"", "\\", UnitTestStatic.Token);
      Assert.IsNotNull(result);
      Assert.IsFalse(string.IsNullOrEmpty(result));
      Assert.IsTrue(result.StartsWith("Headers", StringComparison.OrdinalIgnoreCase), result);
      Assert.IsTrue(result.EndsWith("very short", StringComparison.OrdinalIgnoreCase), result);
    }

    [TestMethod]
    public async Task GuessHeaderStrangeHeaders()
    {
      using var improvedStream =
        FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("StrangeHeaders.txt")));
      var result = await improvedStream.InspectHasHeaderAsync(1200, 0, "", ",", "", "", UnitTestStatic.Token);
      Assert.IsNotNull(result);
      Assert.IsFalse(string.IsNullOrEmpty(result));
    }

    [TestMethod]
    public void ParseLineText1()
    {
      using var stream = new MemoryStream();
      var writer = new StreamWriter(stream);
      writer.Write("1,2,3,4,5,6,7,8,9");
      writer.Flush();
      stream.Position = 0;
      using var reader = new ImprovedTextReader(stream, Encoding.UTF8.CodePage);

      var res = DetectionHeader.DelimitedRecord(reader, ',', '\0', '\0', "##").ToList();
      Assert.AreEqual(9, res.Count());
      Assert.AreEqual("1", res[0]);
      Assert.AreEqual("9", res[8]);
    }

    [TestMethod]
    public void ParseLineText2()
    {
      using var stream = new MemoryStream();
      var writer = new StreamWriter(stream);
      writer.Write("\"1\",2,3,4,5,\"6,\"\"7,8\",\"9\"");
      writer.Flush();
      stream.Position = 0;
      using var reader = new ImprovedTextReader(stream, Encoding.UTF8.CodePage);
      var res = DetectionHeader.DelimitedRecord(reader, ',', '"', '\\', "").ToList();
      Assert.AreEqual(7, res.Count());
      Assert.AreEqual("1", res[0]);
      Assert.AreEqual("6,\"7,8", res[5]);
      Assert.AreEqual("9", res[6]);
    }

    [TestMethod]
    public void ParseLineText3()
    {
      using var stream = new MemoryStream();
      var writer = new StreamWriter(stream);
      writer.Write("1\\,2,3,4,5,\"6,\\\"7,8\",9");
      writer.Flush();
      stream.Position = 0;
      using var reader = new ImprovedTextReader(stream, Encoding.UTF8.CodePage);
      var res = DetectionHeader.DelimitedRecord(reader, ',', '"', '\\', "#").ToList();
      Assert.AreEqual(6, res.Count());
      Assert.AreEqual("1,2", res[0]);
      Assert.AreEqual("6,\"7,8", res[4]);
      Assert.AreEqual("9", res[5]);
    }


    [TestMethod]
    public void ParseLineText4()
    {
      using var stream = new MemoryStream();
      var writer = new StreamWriter(stream);
      writer.Write("1\\,2,3,4,5,\"6,\"\"7,8\",\"9\r\nnl\"");
      writer.Flush();
      stream.Position = 0;
      using var reader = new ImprovedTextReader(stream, Encoding.UTF8.CodePage);
      var res = DetectionHeader.DelimitedRecord(reader, ',', '"', '\\', "#").ToList();
      Assert.AreEqual(6, res.Count());
      Assert.AreEqual("1,2", res[0]);
      Assert.AreEqual("6,\"7,8", res[4]);
      Assert.AreEqual("9\r\nnl", res[5]);
    }
  }
}
