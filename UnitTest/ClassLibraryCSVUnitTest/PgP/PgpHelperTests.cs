using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class PgpHelperTests
  {

    [TestMethod]
    public void WriteStream()
    {
      var fullname = UnitTestStatic.GetTestPath("WriteMyPGP1.pgp");
      var encoding = EncodingHelper.GetEncoding(65001, true);

      using var baseStream = File.Create(fullname.LongPathPrefix());
      using var stream = PgpHelper.GetWriteStream(baseStream, PGPKeyTestHelper.PublicKey, out var stream2, out var stream3);
      using (var writer = new StreamWriter(stream, encoding, 8192))
      {
        writer.WriteLine("This is a test");
      }
      stream.Close();
      stream2.Close();
      stream3.Close();
      stream3.Dispose();
      stream2.Dispose();
    }

    [TestMethod]
    public void WriteThenRead()
    {

      var fullname = UnitTestStatic.GetTestPath("WriteMyPGP2.pgp");
      var encoding = EncodingHelper.GetEncoding(65001, true);

      using (var baseStream = File.Create(fullname.LongPathPrefix()))
      {

        using var stream1 = PgpHelper.GetWriteStream(baseStream, PGPKeyTestHelper.PublicKey, out var stream2, out var stream3);
        using (var writer = new StreamWriter(stream1, encoding, 8192))
        {
          writer.WriteLine("This is a test");
        }
        stream1.Close();
        stream2.Close();
        stream3.Close();
        stream3.Dispose();
        stream2.Dispose();
      }

      // up to this point everything seems fine but the fle is not correct...
      using var encryptedStream = File.OpenRead(fullname);

      using var closeFirst = PgpHelper.GetReadStream(encryptedStream, PGPKeyTestHelper.PrivateKey, PGPKeyTestHelper.Passphrase,
        out var closeSecond, out var closeThird);
      using var reader = new StreamReader(closeFirst, encoding, true);
      Assert.AreEqual("This is a test", reader.ReadLine());
      closeFirst.Close();
      closeSecond?.Close();
      closeThird?.Close();
      closeThird?.Dispose();
      closeSecond?.Dispose();

    }

    [TestMethod]
    public async Task PgpDecryptTestAsync()
    {
      using var input = new MemoryStream(Encoding.UTF8.GetBytes("This is a test"));
      using var encrypted = new MemoryStream(1024);

      await PgpHelper.EncryptStreamAsync(input, encrypted, PGPKeyTestHelper.PublicKey, new Progress<ProgressInfo>(), UnitTestStatic.Token);
      encrypted.Position=0;
      using var decrypted = PgpHelper.GetReadStream(encrypted, PGPKeyTestHelper.PrivateKey, PGPKeyTestHelper.Passphrase, out var closeFirst, out var closeSecond);
      using var sr = new StreamReader(decrypted, true);
      Assert.AreEqual("This is a test", await sr.ReadLineAsync());
      decrypted.Close();
      closeFirst?.Close();
      closeSecond?.Close();
      closeSecond?.Dispose();
      closeFirst?.Dispose();
    }

    private async Task PgpDecryptTestTileAsync(string filename)
    {
      using var input = FileSystemUtils.OpenRead(filename);

      using var decrypted = PgpHelper.GetReadStream(input, PGPKeyTestHelper.PrivateKey, PGPKeyTestHelper.Passphrase, out var closeFirst, out var closeSecond);
      using var sr = new StreamReader(decrypted, true);
      Assert.AreEqual("DateTime\tInteger\tDouble\tNumeric\tString\tMemo\tBoolean\tGUID\tTime", await sr.ReadLineAsync());
      decrypted.Close();
      closeFirst?.Close();
      closeSecond?.Close();
      closeSecond?.Dispose();
      closeFirst?.Dispose();
    }


    [TestMethod]
    public async Task PgpDecryptGpaFiles()
    {
      await PgpDecryptTestTileAsync(UnitTestStatic.GetTestPath("GPA_Signed.txt.gpg"));
      await PgpDecryptTestTileAsync(UnitTestStatic.GetTestPath("GPS_SignedASC.txt.asc"));

      await PgpDecryptTestTileAsync(UnitTestStatic.GetTestPath("GPA_NotSigned.txt.asc"));
      await PgpDecryptTestTileAsync(UnitTestStatic.GetTestPath("GPA_NotSigned.txt.gpg"));
    }

    [TestMethod]
    public async Task PgpEncryptTestFileAsync()
    {
      var progressTime = new ProgressTime();
      var dest = UnitTestStatic.GetTestPath("BasicCSV.txt.pgp");
      FileSystemUtils.FileDelete(dest);
      await PgpHelper.EncryptFileAsync(UnitTestStatic.GetTestPath("BasicCSV.txt"), dest,
        PGPKeyTestHelper.PublicKey, progressTime, UnitTestStatic.Token);
      Assert.IsTrue(FileSystemUtils.FileExists(dest));
    }

    [TestMethod]
    public async Task PgpDecryptTestFileAsync()
    {
      var progressTime = new ProgressTime();
      var dest = UnitTestStatic.GetTestPath("BasicCSV2.txt");
      FileSystemUtils.FileDelete(dest);
      await PgpHelper.DecryptFileAsync(UnitTestStatic.GetTestPath("BasicCSV.txt.gpg"), dest,
        PGPKeyTestHelper.PrivateKey, PGPKeyTestHelper.Passphrase, progressTime, UnitTestStatic.Token);
      Assert.IsTrue(FileSystemUtils.FileExists(dest));
    }

    [TestMethod]
    public async Task PgpEncryptTestAsync()
    {
      var pdt = new ProgressTime();
      using var input = new MemoryStream(Encoding.UTF8.GetBytes("This is a test"));
      using var output = new MemoryStream();
      await PgpHelper.EncryptStreamAsync(input, output, PGPKeyTestHelper.PublicKey, pdt, UnitTestStatic.Token);
      Assert.IsTrue(output.CanRead);
      Assert.IsTrue(output.Length > 100);
    }
  }
}
