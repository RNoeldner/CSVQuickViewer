using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvTools;
using System.Threading;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Security;

namespace CsvTools.Tests
{
  [TestClass]
  public class WritePGP
  {
    private void TestFile(string fileName, Encoding encoding, bool bom)
    {
      PGPKeyStorage pGPKeyStorage = ApplicationSetting.ToolSetting.PGPInformation;
      var line1 = $"1\tTest\t{fileName}";
      var line2 = $"2\tTest\t{fileName}";
      var line3 = $"3\tTest\t{fileName}";

      FileSystemUtils.FileDelete(fileName);

      var encryptionKey = pGPKeyStorage.GetEncryptionKey("test@acme.com");

      using (var encryptor = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.TripleDes, false, new SecureRandom()))
      {
        encryptor.AddMethod(encryptionKey);
        using (encryptedStream = encryptor.Open(new FileStream(fileName, FileMode.Create), new byte[16384]))
        { }

        var compressedStream = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip).Open(encryptedStream);
        using (var stream = new PgpLiteralDataGenerator().Open(compressedStream, PgpLiteralDataGenerator.Utf8, "PGPStream", DateTime.Now, new byte[4096]))
        {
          if (bom)
            stream.WriteByteOrderMark(encoding);
          using (var writer2 = new StreamWriter(stream))
          {
            writer2.WriteLine(line1);
            writer2.WriteLine(line2);
            writer2.WriteLine(line3);
            writer2.Flush();
          }
        }
      }
    }

    var baseStream = File.OpenRead(".\\TestFile.txt.pgp");
    var decryptedPassphrase = pGPKeyStorage.EncryptedPassphase.Decrypt().ToSecureString();
    var reader = pGPKeyStorage.PgpDecrypt(baseStream, decryptedPassphrase);

      using (var sr = new StreamReader(reader, encoding, bom))
      {
        Assert.AreEqual(line1, sr.ReadLine());
        Assert.AreEqual(line2, sr.ReadLine());
        Assert.AreEqual(line3, sr.ReadLine());
      }
    }

    [TestMethod]
public void WritePGPFile()
{
  PGPKeyStorageTestHelper.SetApplicationSetting();

  TestFile("TestFileBOM.txt.pgp", Encoding.UTF8, true);
  TestFile("TestFileUTF8.txt.pgp", Encoding.UTF8, false);
  TestFile("TestFileUTF32.txt.pgp", Encoding.UTF32, true);
}
  }
}