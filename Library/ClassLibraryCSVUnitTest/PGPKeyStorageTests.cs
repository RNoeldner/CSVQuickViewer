using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CsvTools.Tests
{
  public static class PGPKeyStorageTestHelper
  {
    #region const

    //
    // Generated these keys on https://www.igolder.com/pgp/generate-key/
    //
    public const string PUBLIC = @"-----BEGIN PGP PUBLIC KEY BLOCK-----
Version: BCPG C# v1.6.1.0

mQENBFoLFdgBCACLNKAwnyLa2wyW2MEFtgJpxlXoXBhzhVfOl3zf6f6JL2ok4lbn
Rcl8SPAjjh3hwp8H1IoI8tP4y+Qq+x/QEYZzVnp/XQiLexL6lfyMiU8v2lswie0S
LXFYJ5wNcvTOL75sPAYtKDMy9RtNBjD0C+wAjeD8I01+AH/VntSgPcQNrKCkXsEV
2bKiOLYL1jO8VKIFVYvOZ0ywBh0udoyj8TWvzt9r/jXcYMLfQE6stE4/ERg5rVii
o93KEOgPh6whfCYW8CRhsEby2gqa4QBKWMOXktgd7azmxkOsguAWlHwAV3OkDVn4
D0O0N45+cotiZnyDvDVTAWtK1C97DBzOBiopABEBAAG0DXRlc3RAYWNtZS5jb22J
ARwEEAECAAYFAloLFdgACgkQ1rug2FQoEIlO9wf/S9SeORm5uhLBqh4Yu9NEfGj0
R0YuJv13LINZQ7Yqdfvanz13hpFeWpTy3q5bmI4Tbzw6yHmCXrwkmshENlJTWnIQ
Yt8F9lkJZ5uYPV2/H2UT/nBPhKqfZNEkLxVSePHScG9gpwyptuA6l4qFqVPtJgVs
Y05HdjllSD4qR09UnOz7u1/VxwYp8uuLLrWG00b3PiQPftX1UcLMlSrj0uFcKGja
XaLKMIAfWiFzht3a7HN0oeUacZNpsOMp0zWEEI1Yak0S5uVgTqon0zS+qVv81npZ
rIl4ZI0lO9GACgiKjJa0AC1AQfZ+s1bH6UcOUoayqpu1EUExwpLky12H+o7hiA==
=FLjK
-----END PGP PUBLIC KEY BLOCK-----";

    public const string PRIVATE = @"-----BEGIN PGP PRIVATE KEY BLOCK-----
Version: BCPG C# v1.6.1.0

lQOsBFoLFdgBCACLNKAwnyLa2wyW2MEFtgJpxlXoXBhzhVfOl3zf6f6JL2ok4lbn
Rcl8SPAjjh3hwp8H1IoI8tP4y+Qq+x/QEYZzVnp/XQiLexL6lfyMiU8v2lswie0S
LXFYJ5wNcvTOL75sPAYtKDMy9RtNBjD0C+wAjeD8I01+AH/VntSgPcQNrKCkXsEV
2bKiOLYL1jO8VKIFVYvOZ0ywBh0udoyj8TWvzt9r/jXcYMLfQE6stE4/ERg5rVii
o93KEOgPh6whfCYW8CRhsEby2gqa4QBKWMOXktgd7azmxkOsguAWlHwAV3OkDVn4
D0O0N45+cotiZnyDvDVTAWtK1C97DBzOBiopABEBAAH/AwMCEbyFWRQd9oBgixKj
xzs218z+dQAEA9eU4sHkeXgA/4XMUKjtl4vxdf6LVNpZ3BGPpsF+kD7DFkQCNEnA
b/MSffC4tAgRL0WkiZ6ONoTWlLuWf5pEHh4t9on0A3AFx3gEI2RoYLS3pQjThhyH
PZXzFez8anuNKvoT7aRQuG12ROPn+P5CPiRC/jdeJldRDwr2qfIgute3P7KQRyW0
DGhSOZsHgis8UepYyYUUsUR0YjsULWJ9+ysI9vQ0QIrQitVcytNGwDA1kZZigCI3
VGekkVq3OUID5yZ7Q7MmQbFJuKJ+9B546x+NY65rkVDD4UHqf7FxqnHk3FOlw4Mx
93S2JncKi7YBcHoZjLWke8NzGH+i4qPMG8+YNOLI5wl5j9HtL4s+lIjj+w3yD22C
QyNbxtibNkClj1laYfgxwWxwTxkIr8DJSPzYLUYbXInaa5DDRXAl0bm/MrajUqqF
3XE/fCJMZZllv8d3+tPhR9Sao4ezTGbFZlOPJWl9XOG0arLDXrPX2PhU5Vbs7NKR
HJdmxBYFpSahQVaVizFcMIgiMVtrNhx9uJ18i7VGkTtC5vCzFoQN77S2e/CF171F
y7UFq8B9mFO7hoVexgCZqs3ei2ReSXQMTEkekr66eJApVI3lgBTrxpQCJVHNF0HR
blz11Dxx54ew2tyOKET8LDYAhVzByL24qg6BYfmEYd5lGnjcTC90ashMs5Hg+AWX
y+VGEF6aN14s8j4grNqsC41FuXLz6uU/yhjUNOY2Y8Udn+bOOIjnN2lYoCys+xPQ
msUfUOQoBzco8SSwepC0IDqGDrDwamqXIgDI538kM2r6hyi0u+bmlgRZ9H73a03F
W24zZsPLw7frImkQQ85LCvLID3JzrYs7FQey3aTTqLQNdGVzdEBhY21lLmNvbYkB
HAQQAQIABgUCWgsV2AAKCRDWu6DYVCgQiU73B/9L1J45Gbm6EsGqHhi700R8aPRH
Ri4m/Xcsg1lDtip1+9qfPXeGkV5alPLerluYjhNvPDrIeYJevCSayEQ2UlNachBi
3wX2WQlnm5g9Xb8fZRP+cE+Eqp9k0SQvFVJ48dJwb2CnDKm24DqXioWpU+0mBWxj
Tkd2OWVIPipHT1Sc7Pu7X9XHBiny64sutYbTRvc+JA9+1fVRwsyVKuPS4VwoaNpd
osowgB9aIXOG3drsc3Sh5Rpxk2mw4ynTNYQQjVhqTRLm5WBOqifTNL6pW/zWelms
iXhkjSU70YAKCIqMlrQALUBB9n6zVsfpRw5ShrKqm7URQTHCkuTLXYf6juGI
=nF4x
-----END PGP PRIVATE KEY BLOCK-----";

    #endregion const

    public static PGPKeyStorage PGPKeyStorage
    {
      get
      {
        PGPKeyStorage pGPKeyStorage = new PGPKeyStorage
        {
          EncryptedPassphase = "UGotMe".Encrypt()
        };
        pGPKeyStorage.AddPrivateKey(PRIVATE);
        pGPKeyStorage.AddPublicKey(PUBLIC);
        return pGPKeyStorage;
      }
    }

    public static void SetApplicationSetting()
    {
      PGPKeyStorage pGPKeyStorage = ApplicationSetting.ToolSetting.PGPInformation;
      pGPKeyStorage.EncryptedPassphase = "UGotMe".Encrypt();
      pGPKeyStorage.AddPrivateKey(PRIVATE);
      pGPKeyStorage.AddPublicKey(PUBLIC);
    }
  }

  [TestClass]
  public class PGPKeyStorageTests
  {
    private readonly PGPKeyStorage m_PGPKeyStorage = PGPKeyStorageTestHelper.PGPKeyStorage;

    [TestMethod]
    public void IsValidKeyRingBundleTest()
    {
      Assert.IsFalse(PGPKeyStorage.IsValidKeyRingBundle("Hello Test", true, out _));
      Assert.IsFalse(PGPKeyStorage.IsValidKeyRingBundle("", false, out _));

      Assert.IsTrue(PGPKeyStorage.IsValidKeyRingBundle(@"-----BEGIN PGP PRIVATE KEY BLOCK-----
Version: GnuPG v2

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
=Bz2c
-----END PGP PRIVATE KEY BLOCK-----", true, out _));

      Assert.IsTrue(PGPKeyStorage.IsValidKeyRingBundle(@"-----BEGIN PGP PUBLIC KEY BLOCK-----
Version: GnuPG v2

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
0l3kIPkMvSVx+8lp3UqCnS5etOA=
=g7cd
-----END PGP PUBLIC KEY BLOCK-----", false, out _));
    }

    [TestMethod]
    public void AddPrivateKeyTest()
    {
      m_PGPKeyStorage.AddPrivateKey(PGPKeyStorageTestHelper.PRIVATE);
      Assert.AreEqual(1, m_PGPKeyStorage.PrivateKeys.Count());
    }

    [TestMethod]
    public void AddPublicKeyTest()
    {
      m_PGPKeyStorage.AddPublicKey(PGPKeyStorageTestHelper.PUBLIC);
      Assert.AreEqual(1, m_PGPKeyStorage.PublicKeys.Count());
    }

    [TestMethod]
    public void GetPrivateKeyRingBundleListTest()
    {
      var res = m_PGPKeyStorage.GetPrivateKeyRingBundleList();
      Assert.AreEqual(1, res.Count());
    }

    [TestMethod]
    public void GetPublicKeyRingBundleListTest()
    {
      var res = m_PGPKeyStorage.GetPublicKeyRingBundleList();
      Assert.AreEqual(1, res.Count());
    }

    [TestMethod]
    public void GetPublicKeyRingBundlesTest()
    {
      var res = m_PGPKeyStorage.GetPublicKeyRingBundles();
      Assert.AreEqual(1, res.Count());
    }

    [TestMethod]
    public void GetRecipientListTest()
    {
      var res = m_PGPKeyStorage.GetRecipientList();
      Assert.AreEqual(1, res.Count());
    }

    [TestMethod]
    public void GetRecipientsTest()
    {
      var res = m_PGPKeyStorage.GetRecipients();
      Assert.AreEqual(1, res.Count());
    }

    [TestMethod]
    public void GetSecretKeyRingBundlesTest()
    {
      var res = m_PGPKeyStorage.GetSecretKeyRingBundles();
      Assert.AreEqual(1, res.Count());
    }

    [TestMethod]
    public void NotifyPropertyChangedTest()
    {
      var raised = false;
      m_PGPKeyStorage.PropertyChanged += delegate { raised = true; };
      m_PGPKeyStorage.EncryptedPassphase = "Nonsense";
      Assert.IsTrue(raised);
    }

    [TestMethod]
    public void PgpDecryptTest()
    {
      using (var pdt = new ProcessDisplayTime(CancellationToken.None))
      {
        using (var input = new MemoryStream(Encoding.UTF8.GetBytes("This is a test")))
        {
          using (var output = new MemoryStream())
          {
            m_PGPKeyStorage.PgpEncrypt(input, output, m_PGPKeyStorage.GetRecipientList().First(), pdt);
            output.Position = 0;
            using (var decrypted =
              m_PGPKeyStorage.PgpDecrypt(output, m_PGPKeyStorage.EncryptedPassphase.Decrypt().ToSecureString()))
            {
              var buffer = new byte[1024];
              var count = decrypted.Read(buffer, 0, buffer.Length);
              Assert.AreEqual("This is a test", Encoding.UTF8.GetString(buffer, 0, count));
            }
          }
        }
      }
    }
    
    //[TestMethod]
    //public void PgpEncryptTestFile()
    //{
    //  using (var pdt = new ProcessDisplayTime(CancellationToken.None))
    //  {
    //    using (var input = File.OpenRead(m_ApplicationDirectory + "\\BasicCSV.txt"))
    //    {
    //      using (var output = File.OpenWrite(m_ApplicationDirectory + "\\BasicCSV.pgp"))
    //      {
    //        m_PGPKeyStorage.PgpEncrypt(input, output, m_PGPKeyStorage.GetRecipientList().First(), pdt);
    //      }
    //    }
    //  }
    //}

    [TestMethod]
    public void PgpEncryptTest()
    {
      using (var pdt = new ProcessDisplayTime(CancellationToken.None))
      {
        using (var input = new MemoryStream(Encoding.UTF8.GetBytes("This is a test")))
        {
          using (var output = new MemoryStream())
          {
            m_PGPKeyStorage.PgpEncrypt(input, output, m_PGPKeyStorage.GetRecipientList().First(), pdt);
          }
        }
      }

      using (var pdt = new DummyProcessDisplay(CancellationToken.None))
      {
        using (var input = new MemoryStream(Encoding.UTF8.GetBytes("This is a test")))
        {
          using (var output = new MemoryStream())
          {
            m_PGPKeyStorage.PgpEncrypt(input, output, m_PGPKeyStorage.GetRecipientList().First(), pdt);
          }
        }
      }
    }

    [TestMethod]
    public void RemovePrivateKeyTest()
    {
      Assert.AreEqual(1, m_PGPKeyStorage.PrivateKeys.Length);
      m_PGPKeyStorage.RemovePrivateKey(0);
      Assert.AreEqual(0, m_PGPKeyStorage.PrivateKeys.Length);
    }

    [TestMethod]
    public void RemovePublicKeyTest()
    {
      Assert.AreEqual(1, m_PGPKeyStorage.PublicKeys.Length);
      m_PGPKeyStorage.RemovePublicKey(0);
      Assert.AreEqual(0, m_PGPKeyStorage.PublicKeys.Length);
    }

    [TestMethod]
    public void EqualsTest()
    {
      var key2 = new PGPKeyStorage();
      Assert.IsTrue(m_PGPKeyStorage.Equals(m_PGPKeyStorage));
      Assert.IsFalse(m_PGPKeyStorage.Equals(null));
      key2.EncryptedPassphase = m_PGPKeyStorage.EncryptedPassphase;
      Assert.IsFalse(key2.Equals(m_PGPKeyStorage));
      Assert.IsFalse(m_PGPKeyStorage.Equals("Test"));
    }

    [TestMethod]
    public void CloneTest()
    {
      var key2 = m_PGPKeyStorage.Clone();
      Assert.IsTrue(key2.Equals(m_PGPKeyStorage));
    }

    [TestMethod]
    public void CopyToTest()
    {
      var key2 = new PGPKeyStorage();
      m_PGPKeyStorage.CopyTo(key2);
      Assert.IsTrue(key2.Equals(m_PGPKeyStorage));
    }
  }
}