using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ImprovedTextReaderTests
  {
    [TestMethod]
    public async Task ImprovedTextReaderTestBOM()
    {
      using (var impStream = ImprovedStream.OpenRead(UnitTestInitializeCsv.GetTestPath("BasicCsV.txt")))
      {
        using (var test = new ImprovedTextReader(impStream))
        {
          Assert.AreEqual(1, test.LineNumber);
          Assert.AreEqual("ID,LangCodeID,ExamDate,Score,Proficiency,IsNativeLang", await test.ReadLineAsync());
          Assert.AreEqual("1,German,20/01/2010,276,0.94,Y", await test.ReadLineAsync());
        }
      }
    }

    [TestMethod]
    public async Task ImprovedTextReaderTestCodePage()
    {
      using (var impStream = ImprovedStream.OpenRead(UnitTestInitializeCsv.GetTestPath("BasicCsV.txt")))
      {
        using (var test = new ImprovedTextReader(impStream, 12000))
        {
          Assert.AreEqual(1, test.LineNumber);
          Assert.AreEqual("ID,LangCodeID,ExamDate,Score,Proficiency,IsNativeLang", await test.ReadLineAsync());
          Assert.AreEqual("1,German,20/01/2010,276,0.94,Y", await test.ReadLineAsync());
        }
      }
    }

    [TestMethod]
    public async Task ImprovedTextReaderTestGz()
    {
      using (var impStream = ImprovedStream.OpenRead(UnitTestInitializeCsv.GetTestPath("BasicCsV.txt.gz")))
      {
        using (var test = new ImprovedTextReader(impStream, 12000))
        {
          Assert.AreEqual(1, test.LineNumber);
          Assert.AreEqual("ID,LangCodeID,ExamDate,Score,Proficiency,IsNativeLang", await test.ReadLineAsync());
          Assert.AreEqual("1,German,20/01/2010,276,0.94,Y", await test.ReadLineAsync());
        }
      }
    }

    [TestMethod]
    public async Task BOMTest()
    {
      // create files
      var fn = new[]
      {
        new Tuple<string, int, byte[]>("GB18030", (int) EncodingHelper.CodePage.GB18030,
          new[] {(byte) 0x84, (byte) 0x31, (byte) 0x95, (byte) 0x33}),
        new Tuple<string, int, byte[]>("UTF-7_2", (int) EncodingHelper.CodePage.UTF7,
          new[] {(byte) 0x2B, (byte) 0x2F, (byte) 0x76, (byte) 0x39}),
        new Tuple<string, int, byte[]>("UTF-7_3", (int) EncodingHelper.CodePage.UTF7,
          new[] {(byte) 0x2B, (byte) 0x2F, (byte) 0x76, (byte) 0x2B}),
        new Tuple<string, int, byte[]>("UTF-7_4", (int) EncodingHelper.CodePage.UTF7,
          new[] {(byte) 0x2B, (byte) 0x2F, (byte) 0x76, (byte) 0x2F}),
        new Tuple<string, int, byte[]>("UTF-16 (BE)", (int) EncodingHelper.CodePage.UTF16Be,
          new[] {(byte) 0xFE, (byte) 0xFF}),
        new Tuple<string, int, byte[]>("UTF8", (int) EncodingHelper.CodePage.UTF8,
          new[] {(byte) 0xEF, (byte) 0xBB, (byte) 0xBF}),
        new Tuple<string, int, byte[]>("UTF-16 (LE)", (int) EncodingHelper.CodePage.UTF16Le,
          new[] {(byte) 0xFF, (byte) 0xFE}),
        new Tuple<string, int, byte[]>("UTF-32 (BE)", (int) EncodingHelper.CodePage.UTF32Be,
          new[] {(byte) 0, (byte) 0, (byte) 0xFE, (byte) 255}),
        new Tuple<string, int, byte[]>("UTF-32 (LE)", (int) EncodingHelper.CodePage.UTF32Le,
          new[] {(byte) 0xFF, (byte) 0xFE, (byte) 0, (byte) 0}),
        new Tuple<string, int, byte[]>("UTF-7_1", (int) EncodingHelper.CodePage.UTF7,
          new[] {(byte) 0x2B, (byte) 0x2F, (byte) 0x76, (byte) 0x38})
      };

      //var Text = Encoding.ASCII.GetBytes("This is a test\\r\nLine2");
      var line1 = "This is a test - First Line!";
      var line2 = "Another line...";

      foreach (var type in fn)
      {
        var fileName = UnitTestInitializeCsv.GetTestPath("Test_" + type.Item1 + ".txt");
        using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
        {
          // write the BOM
          fs.Write(type.Item3, 0, type.Item3.Length);

          using (var fs2 = new StreamWriter(fs, Encoding.GetEncoding(type.Item2)))
          {
            await fs2.WriteLineAsync(line1);
            await fs2.WriteAsync(line2);
          }
        }

        using (var impStream = ImprovedStream.OpenRead(fileName))
        {
          using (var test = new ImprovedTextReader(impStream, type.Item2))
          {
            Assert.AreEqual(1, test.LineNumber);
            Assert.AreEqual(line1, await test.ReadLineAsync(), $"Issue reading Line1 {type.Item1}");
            Assert.AreEqual(2, test.LineNumber);
            Assert.AreEqual(line2, await test.ReadLineAsync(), $"Issue reading Line2 {type.Item1}");

            await test.ToBeginningAsync();

            Assert.AreEqual(1, test.LineNumber);
            Assert.AreEqual(line1, await test.ReadLineAsync(), $"Issue reading after reset {type.Item1}");
          }
        }

        File.Delete(fileName);
      }
    }

    [TestMethod]
    public async Task ToBeginningTest()
    {
      // use a file with a BOM
      using (var impStream = ImprovedStream.OpenRead(UnitTestInitializeCsv.GetTestPath("txTranscripts.txt")))
      {
        using (var test = new ImprovedTextReader(impStream))
        {
          Assert.AreEqual(1, test.LineNumber);
          Assert.AreEqual(
            "#UserID	CurriculumID	TranscriptStatus	RequestDateTime	RegistrationDateTime	CompletionDateTime",
            await test.ReadLineAsync());
          for (var i = 0; i < 200; i++)
            _ = await test.ReadLineAsync();

          await test.ToBeginningAsync();

          Assert.AreEqual(1, test.LineNumber);
          Assert.AreEqual(
            "#UserID	CurriculumID	TranscriptStatus	RequestDateTime	RegistrationDateTime	CompletionDateTime",
            await test.ReadLineAsync());
          for (var i = 0; i < 300; i++)
            await test.ReadLineAsync();

          await test.ToBeginningAsync();

          Assert.AreEqual(1, test.LineNumber);
          Assert.AreEqual(
            "#UserID	CurriculumID	TranscriptStatus	RequestDateTime	RegistrationDateTime	CompletionDateTime",
            await test.ReadLineAsync());
        }
      }
    }
  }
}