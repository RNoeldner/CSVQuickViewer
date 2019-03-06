using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace CsvTools.Tests
{
  [TestClass]
  public class FileSystemUtilsTest
  {
    [TestMethod]
    public void GetShortestPath()
    {
      Assert.AreEqual("C:\\CsvHelperTest.cs", FileSystemUtils.GetShortestPath("C:\\CsvHelperTest.cs", "."));
      Assert.AreEqual("..\\CsvHelperTest.cs", FileSystemUtils.GetShortestPath("..\\CsvHelperTest.cs", "."));
    }

    [TestMethod]
    public void GetRelativePath()
    {
      var root = FileSystemUtils.ExecutableDirectoryName();
      Assert.AreEqual("TestFiles\\BasicCSV.txt", FileSystemUtils.GetRelativePath(root + "\\TestFiles\\BasicCSV.txt", root));
    }

    [TestMethod]
    public void SafePath()
    {
      Assert.AreEqual("Test$Files\\Basic$CSV.txt", FileSystemUtils.SafePath("Test|Files\\Basic<CSV.txt", "$"));
    }

    [TestMethod]
    public void GetFiles()
    {
      var root = FileSystemUtils.ExecutableDirectoryName();
      var res = FileSystemUtils.GetFiles(root, "*.dll");
      Assert.IsTrue(res.Any(x => x == root + "\\ClassLibraryCSV.dll"));
      Assert.IsTrue(res.Any(x => x == root + "\\ClassLibraryCSVUnitTest.dll"));
    }

    [TestMethod]
    public void GetLatestFileOfPattern()
    {
      var root = FileSystemUtils.ExecutableDirectoryName();
      var res = FileSystemUtils.GetLatestFileOfPattern(root, "ClassLibraryCSV*.dll");
      Assert.IsTrue(res == root + "\\ClassLibraryCSVUnitTest.dll");
    }

    [TestMethod]
    public void ResolvePattern()
    {
      var root = FileSystemUtils.ExecutableDirectoryName();
      var res = FileSystemUtils.ResolvePattern(root + "\\ClassLibraryCSV*.dll");
      Assert.IsTrue(res == root + "\\ClassLibraryCSVUnitTest.dll");
    }

    [TestMethod]
    public void TestGetAbsolutePath()
    {
      var root = FileSystemUtils.ExecutableDirectoryName();
      Directory.SetCurrentDirectory(root);
      Assert.AreEqual(root + "\\TestFile.docx", "TestFile.docx".GetAbsolutePath(""));
      Assert.AreEqual(root + "\\TestFile.docx", "TestFile.docx".GetAbsolutePath("."));
      Assert.AreEqual(root + "\\TestFile.docx", ".\\TestFile.docx".GetAbsolutePath(""));
      Assert.AreEqual(root + "\\TestFile.docx", ".\\TestFile.docx".GetAbsolutePath("."));
      Assert.AreEqual("C:\\TestFile.docx", "C:\\TestFile.docx".GetAbsolutePath("."));
      Assert.AreEqual("C:\\TestFile.docx", "C:\\TestFile.docx".GetAbsolutePath(""));
    }
  }
}