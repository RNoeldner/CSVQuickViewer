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
  public class FileSystemUtilsTests
  {
    [TestMethod()]
    public void GetRelativePathUserProfileTest()
    {
      var testFile1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "TestFile.txt").GetAbsolutePath();
      Assert.AreEqual(Path.Combine("%UserProfile%", "TestFile.txt"), FileSystemUtils.GetRelativePath(testFile1, "."));
      
      var testFile2 = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
      Assert.AreEqual("%UserProfile%", FileSystemUtils.GetRelativePath(testFile2, "."));     
    }


    [TestMethod()]
    public void GetRelativePathBaseTest()
    {
      var testFile1 = "Test".GetAbsolutePath(".");
      Assert.AreEqual("Test", FileSystemUtils.GetRelativePath(testFile1, "."));

      var testFile2 = "Test";
      Assert.AreEqual("Test", FileSystemUtils.GetRelativePath(testFile2, "."));
    }
  }
}