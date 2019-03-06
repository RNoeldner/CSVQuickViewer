using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CsvTools.Tests
{
  [TestClass()]
  public class ImprovedStreamTests
  {
    private readonly string m_ApplicationDirectory = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles";

    [TestMethod()]
    public void OpenReadTestSetting()
    {
      CsvFile setting = new CsvFile();
      setting.FileName = Path.Combine(m_ApplicationDirectory, "BasicCsV.txt");
      using (var res = ImprovedStream.OpenRead(setting))
      {
        Assert.IsNotNull(res);
        Assert.IsNotNull(res.Stream);
      }
    }

    [TestMethod()]
    public void OpenReadTestRegular()
    {
      using (var res = ImprovedStream.OpenRead(Path.Combine(m_ApplicationDirectory, "BasicCsV.txt")))
      {
        Assert.IsNotNull(res);
        Assert.IsNotNull(res.Stream);
      }
    }

    [TestMethod()]
    public void OpenReadTestgZip()
    {
      using (var res = ImprovedStream.OpenRead(Path.Combine(m_ApplicationDirectory, "BasicCsV.txt.gz")))
      {
        Assert.IsNotNull(res);
        Assert.IsNotNull(res.Stream);
      }
    }

    [TestMethod()]
    public void OpenReadTestPGP()
    {
      PGPKeyStorageTestHelper.SetApplicationSetting();
      using (var res = ImprovedStream.OpenRead(Path.Combine(m_ApplicationDirectory, "BasicCsV.pgp"), () => { return "UGotMe".Encrypt(); }))
      {
        Assert.IsNotNull(res);
        Assert.IsNotNull(res.Stream);
      }
    }

    [TestMethod()]
    public void OpenWriteTestRegular()
    {
    }

    [TestMethod()]
    public void OpenWriteTestgZip()
    {
    }

    [TestMethod()]
    public void OpenWriteTestPgp()
    {
    }

    [TestMethod()]
    public void CloseTest()
    {
      var res = ImprovedStream.OpenRead(Path.Combine(m_ApplicationDirectory, "BasicCsV.txt"));
      res.Close();
    }

    [TestMethod()]
    public void DisposeTest()
    {
      var res = ImprovedStream.OpenRead(Path.Combine(m_ApplicationDirectory, "BasicCsV.txt"));
      res.Dispose();
    }
  }
}