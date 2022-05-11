using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class ImprovedTextReaderTests
  {

    private class NonSeekableStream : Stream
    {
      Stream m_stream;
      public NonSeekableStream(Stream baseStream)
      {
        m_stream = baseStream;
      }
      public override bool CanRead
      {
        get { return m_stream.CanRead; }
      }

      public override bool CanSeek
      {
        get { return false; }
      }

      public override bool CanWrite
      {
        get { return m_stream.CanWrite; }
      }

      public override void Flush()
      {
        m_stream.Flush();
      }

      public override long Length
      {
        get { throw new NotSupportedException(); }
      }

      public override long Position
      {
        get
        {
          return m_stream.Position;
        }
        set
        {
          throw new NotSupportedException();
        }
      }

      public override int Read(byte[] buffer, int offset, int count)
      {
        return m_stream.Read(buffer, offset, count);
      }

      public override long Seek(long offset, SeekOrigin origin)
      {
        throw new NotImplementedException();
      }

      public override void SetLength(long value)
      {
        throw new NotSupportedException();
      }

      public override void Write(byte[] buffer, int offset, int count)
      {
        m_stream.Write(buffer, offset, count);
      }
    }

    [TestMethod]
    public async Task ImprovedTextReaderTestBomAsync()
    {
      using var impStream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCsV.txt")));
      using var test = new ImprovedTextReader(impStream);
      Assert.AreEqual(1, test.LineNumber);
      Assert.AreEqual("ID,LangCodeID,ExamDate,Score,Proficiency,IsNativeLang", await test.ReadLineAsync());
      Assert.AreEqual("1,German,20/01/2010,276,0.94,Y", await test.ReadLineAsync());
    }

    [TestMethod]
    public async Task ImprovedTextReaderTestNonSeekUTF8_ReadLineAsync()
    {
      /// Some streams like the response stream form a web request may not be seekable      
      var nonSeekableStream = new NonSeekableStream(FileSystemUtils.OpenRead(UnitTestStatic.GetTestPath("UnicodeUTF8.txt")));
      using var impStream = new ImprovedStream(new SourceAccess(nonSeekableStream));
      using var test = new ImprovedTextReader(impStream);

      using var fs = FileSystemUtils.OpenRead(UnitTestStatic.GetTestPath("UnicodeUTF8.txt"));
      using var sr = new StreamReader(fs, Encoding.UTF8, true, 4096, true);
      Assert.AreEqual(sr.ReadLine(), await test.ReadLineAsync());

    }

    [TestMethod]
    public async Task ImprovedTextReaderTestNonSeekUTF8NoBOM_ReadLineAsync()
    {
      var nonSeekableStream = new NonSeekableStream(FileSystemUtils.OpenRead(UnitTestStatic.GetTestPath("UnicodeUTF8NoBOM.txt")));
      using var impStream = new ImprovedStream(new SourceAccess(nonSeekableStream));
      using var test = new ImprovedTextReader(impStream);

      using var fs = FileSystemUtils.OpenRead(UnitTestStatic.GetTestPath("UnicodeUTF8NoBOM.txt"));
      using var sr = new StreamReader(fs, Encoding.UTF8, true, 4096, true);
      Assert.AreEqual(sr.ReadLine(), await test.ReadLineAsync());
    }

    [TestMethod]
    public void ImprovedTextReaderTestNonSeekUTF8_Read()
    {
      /// Some streams like the response stream form a web request may not be seekable
      var nonSeekableStream = new NonSeekableStream(FileSystemUtils.OpenRead(UnitTestStatic.GetTestPath("UnicodeUTF8.txt")));
      using var impStream = new ImprovedStream(new SourceAccess(nonSeekableStream));
      using var test = new ImprovedTextReader(impStream);

      using var fs = FileSystemUtils.OpenRead(UnitTestStatic.GetTestPath("UnicodeUTF8.txt"));
      using var sr = new StreamReader(fs, Encoding.UTF8, true, 4096, true);

      Assert.AreEqual(sr.ReadLine()[0], (char) test.Read());
    }

    [TestMethod]
    public void ImprovedTextReaderTestNonSeekUTF8NoBOM_Read()
    {
      /// Some streams like the response stream form a web request may not be seekable
      var nonSeekableStream = new NonSeekableStream(FileSystemUtils.OpenRead(UnitTestStatic.GetTestPath("UnicodeUTF8NoBOM.txt")));
      using var impStream = new ImprovedStream(new SourceAccess(nonSeekableStream));
      using var test = new ImprovedTextReader(impStream);

      using var fs = FileSystemUtils.OpenRead(UnitTestStatic.GetTestPath("UnicodeUTF8NoBOM.txt"));
      using var sr = new StreamReader(fs, Encoding.UTF8, true, 4096, true);

      Assert.AreEqual(sr.ReadLine()[0], (char) test.Read());
    }
    [TestMethod]
    public async Task ImprovedTextReaderTestCodePageAsync()
    {
      using var impStream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCsV.txt")));
      using var test = new ImprovedTextReader(impStream, 12000);
      Assert.AreEqual(1, test.LineNumber);
      Assert.AreEqual("ID,LangCodeID,ExamDate,Score,Proficiency,IsNativeLang", await test.ReadLineAsync());
      Assert.AreEqual("1,German,20/01/2010,276,0.94,Y", await test.ReadLineAsync());
    }

    [TestMethod]
    public async Task ImprovedTextReaderTestGzAsync()
    {
      using var impStream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCsV.txt.gz")));
      using var test = new ImprovedTextReader(impStream, 12000);
      Assert.AreEqual(1, test.LineNumber);
      Assert.AreEqual("ID,LangCodeID,ExamDate,Score,Proficiency,IsNativeLang", await test.ReadLineAsync());
      Assert.AreEqual("1,German,20/01/2010,276,0.94,Y", await test.ReadLineAsync());
    }

    [TestMethod]
    public async Task BomTest()
    {
      // create files
      var fn = new[]
      {
        new Tuple<string, int, byte[]>("GB18030", 54936,
          new[] { (byte) 0x84, (byte) 0x31, (byte) 0x95, (byte) 0x33 }),
        new Tuple<string, int, byte[]>("UTF-7_2", 65000,
          new[] { (byte) 0x2B, (byte) 0x2F, (byte) 0x76, (byte) 0x39 }),
        new Tuple<string, int, byte[]>("UTF-7_3", 65000,
          new[] { (byte) 0x2B, (byte) 0x2F, (byte) 0x76, (byte) 0x2B }),
        new Tuple<string, int, byte[]>("UTF-7_4", 65000,
          new[] { (byte) 0x2B, (byte) 0x2F, (byte) 0x76, (byte) 0x2F }),
        new Tuple<string, int, byte[]>("UTF-16 (BE)", 1201,
          new[] { (byte) 0xFE, (byte) 0xFF }),
        new Tuple<string, int, byte[]>("UTF8", 65001,
          new[] { (byte) 0xEF, (byte) 0xBB, (byte) 0xBF }),
        new Tuple<string, int, byte[]>("UTF-16 (LE)", 1200,
          new[] { (byte) 0xFF, (byte) 0xFE }),
        new Tuple<string, int, byte[]>("UTF-32 (BE)", 12001,
          new[] { (byte) 0, (byte) 0, (byte) 0xFE, (byte) 255 }),
        new Tuple<string, int, byte[]>("UTF-32 (LE)", 12000,
          new[] { (byte) 0xFF, (byte) 0xFE, (byte) 0, (byte) 0 }),
        new Tuple<string, int, byte[]>("UTF-7_1", 65000,
          new[] { (byte) 0x2B, (byte) 0x2F, (byte) 0x76, (byte) 0x38 })
      };

      //var Text = Encoding.ASCII.GetBytes("This is a test\\r\nLine2");
      var line1 = "This is a test - First Line!";
      var line2 = "Another line...";

      foreach (var type in fn)
      {        
        var fileName = UnitTestStatic.GetTestPath("Test_" + type.Item1 + ".txt");
        FileSystemUtils.FileDelete(fileName);
        using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
        {
          // write the BOM
          fs.Write(type.Item3, 0, type.Item3.Length);

          using var fs2 = new StreamWriter(fs, Encoding.GetEncoding(type.Item2));
          await fs2.WriteLineAsync(line1);
          await fs2.WriteAsync(line2);
        }

        using (var impStream = new ImprovedStream(new SourceAccess(fileName)))
        {
          using var test = new ImprovedTextReader(impStream, type.Item2);

          Assert.AreEqual(1, test.LineNumber);
          Assert.AreEqual(line1, await test.ReadLineAsync(), $"Issue reading Line1 {type.Item1}");
          Assert.AreEqual(2, test.LineNumber);
          Assert.AreEqual(line2, await test.ReadLineAsync(), $"Issue reading Line2 {type.Item1}");

          test.ToBeginning();

          Assert.AreEqual(1, test.LineNumber);
          Assert.AreEqual(line1, await test.ReadLineAsync(), $"Issue reading after reset {type.Item1}");
        }
        FileSystemUtils.FileDelete(fileName);
      }
    }

    [TestMethod]
    public async Task ToBeginningTestAsync()
    {
      // use a file with a BOM
      using var impStream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("txTranscripts.txt")));
      using var test = new ImprovedTextReader(impStream);

      Assert.AreEqual(1, test.LineNumber);
      Assert.AreEqual(
        "#UserID	CurriculumID	TranscriptStatus	RequestDateTime	RegistrationDateTime	CompletionDateTime",
        await test.ReadLineAsync());
      for (var i = 0; i < 200; i++)
        _ = await test.ReadLineAsync();

      test.ToBeginning();

      Assert.AreEqual(1, test.LineNumber);
      Assert.AreEqual(
        "#UserID	CurriculumID	TranscriptStatus	RequestDateTime	RegistrationDateTime	CompletionDateTime",
        await test.ReadLineAsync());
      for (var i = 0; i < 300; i++)
        await test.ReadLineAsync();

      test.ToBeginning();

      Assert.AreEqual(1, test.LineNumber);
      Assert.AreEqual(
        "#UserID	CurriculumID	TranscriptStatus	RequestDateTime	RegistrationDateTime	CompletionDateTime",
        await test.ReadLineAsync());
    }
  }
}