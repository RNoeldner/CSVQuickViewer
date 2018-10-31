using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvTools;

namespace CsvTools.Tests
{
  [TestClass]
  public class sFTPTests
  {
    [ClassInitialize]
    public static void SetAccount(TestContext tc)
    {
      ApplicationSetting.ToolSetting.RemoteAccess.Protocol = AccessProtocol.Ftp;
      ApplicationSetting.ToolSetting.RemoteAccess.HostName = "demo.wftpserver.com";
      ApplicationSetting.ToolSetting.RemoteAccess.User = "demo-user";
      ApplicationSetting.ToolSetting.RemoteAccess.Password = "demo-user".Encrypt();
    }

    [TestMethod()]
    public void TestConnectionTest()
    {
      sFTP.TestConnection();
    }

    [TestMethod()]
    public void CloseSessionTest()
    {
      sFTP.CloseSession();
    }

    [TestMethod()]
    public void GetRemotePathTest()
    {
      Assert.AreEqual("/upload/wftpserver-mac-i386.tar.gz", sFTP.GetRemotePath("/upload", "wftpserver-mac-i386.tar.gz"));
      Assert.AreEqual("/upload/wftpserver-mac-i386.tar.gz", sFTP.GetRemotePath("\\upload", "wftpserver-mac-i386.tar.gz"));
      Assert.AreEqual("/upload/wftpserver-mac-i386.tar.gz", sFTP.GetRemotePath("upload", "wftpserver-mac-i386.tar.gz"));
      Assert.AreEqual("/upload/wftpserver-mac-i386.tar.gz", sFTP.GetRemotePath("\\upload", "/upload/wftpserver-mac-i386.tar.gz"));
      Assert.AreEqual("/upload/wftpserver-mac-i386.tar.gz", sFTP.GetRemotePath("", "/upload/wftpserver-mac-i386.tar.gz"));
    }

    [TestMethod()]
    public void RemoteFileExistsTest()
    {
      Assert.IsTrue(sFTP.RemoteFileExists("/download", "wftpserver-mac-i386.tar.gz"));
      Assert.IsFalse(sFTP.RemoteFileExists("/upload", "wftpserver.gz"));
    }

    [TestMethod()]
    public void ProcessRemoteFileReadTest()
    {
      FileSystemUtils.FileDelete("down.tar.gz");
      sFTP.ProcessRemoteFileRead("/download/wftpserver-mac-i386.tar.gz", "down.tar.gz", null, true);
      Assert.IsTrue(FileSystemUtils.FileExists("down.tar.gz"));
    }

    [TestMethod()]
    public void ProcessRemoteFileWriteTest()
    {
      sFTP.ProcessRemoteFileWrite("TestFiles\\BasicCSV.pgp", "/upload/BasicCSV.pgp", null);
      Assert.IsTrue(sFTP.FileExists("/upload/BasicCSV.pgp"));
    }

    [TestMethod()]
    public void EnumerateRemoteFilesTest()
    {
      var res = sFTP.EnumerateRemoteFiles("/upload/", "*.txt");
      Assert.IsFalse(res.IsEmpty());
    }

    [TestMethod()]
    public void FileExistsTest()
    {
      Assert.IsTrue(sFTP.FileExists("/download/wftpserver-mac-i386.tar.gz"));
      Assert.IsFalse(sFTP.FileExists("/upload/wftpserver-mac-i386.tar.gz"));
    }

    [TestMethod()]
    public void CreateDirectoryTest()
    {
      sFTP.CreateDirectory("/upload/test");
    }

    [TestMethod()]
    public void DirectoryExistsTest()
    {
      Assert.IsTrue(sFTP.DirectoryExists("/download"));
      Assert.IsFalse(sFTP.DirectoryExists("/download/wftpserver-mac-i386.tar.gz"));
    }

    [TestMethod()]
    public void FileDeleteTest()
    {
    }

    [TestMethod()]
    public void GetFileInfoTest()
    {
      Assert.IsFalse(sFTP.GetFileInfo("/download/wftpserver-mac-i386.tar.gz").IsDirectory);
    }

    [TestMethod()]
    public void ListDirectoryTest()
    {
      Assert.IsTrue(sFTP.ListDirectory("/download").Files.Count > 0);
    }
  }
}