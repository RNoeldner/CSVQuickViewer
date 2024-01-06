using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

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
     [TestMethod]
    public void Create()
    {
      var fn = UnitTestStatic.GetTestPath("Test2.dat");
      if (File.Exists(fn))
        File.Delete(fn);

      using (var stream = FileSystemUtils.Create(fn, 32000, FileOptions.Asynchronous))
      {
        Assert.AreEqual(true, stream.CanWrite);
        stream.WriteByte(10);
        stream.Close();
      }

      Assert.IsTrue(File.Exists(fn));
      File.Delete(fn);
    }

    [TestMethod]
    public void CreateDirectory()
    {
      if (FileSystemUtils.DirectoryExists($".{Path.DirectorySeparatorChar}Test{Path.DirectorySeparatorChar}"))
        Directory.Delete($".{Path.DirectorySeparatorChar}Test{Path.DirectorySeparatorChar}");
      Assert.IsFalse(FileSystemUtils.DirectoryExists($".{Path.DirectorySeparatorChar}Test{Path.DirectorySeparatorChar}"));
      FileSystemUtils.CreateDirectory($".{Path.DirectorySeparatorChar}Test{Path.DirectorySeparatorChar}");
      Assert.IsTrue(FileSystemUtils.DirectoryExists($".{Path.DirectorySeparatorChar}Test{Path.DirectorySeparatorChar}"));

      // nothing bad happens
      FileSystemUtils.CreateDirectory("");
      FileSystemUtils.CreateDirectory(null);
    }

    [TestMethod]
    public void DirectoryName()
    {
      if (!FileSystemUtils.DirectoryExists($"Test{Path.DirectorySeparatorChar}"))
        FileSystemUtils.CreateDirectory($"Test{Path.DirectorySeparatorChar}");
      Assert.AreEqual(Path.GetFullPath($".{Path.DirectorySeparatorChar}Test"), $".{Path.DirectorySeparatorChar}Test".GetDirectoryName());
    }

    [TestMethod]
    public async Task FileCopy()
    {
      var dest = UnitTestStatic.GetTestPath("xyz.txt");
      try
      {
        var progress = new Progress<ProgressInfo>( );

        Assert.IsFalse(FileSystemUtils.FileExists(dest));
        await FileSystemUtils.FileCopy(UnitTestStatic.GetTestPath("AllFormats.txt"), dest, false,
          progress, UnitTestStatic.Token);
        Assert.IsTrue(FileSystemUtils.FileExists(dest));
        

        // Copy again, the old file should be overwritten
        await FileSystemUtils.FileCopy(UnitTestStatic.GetTestPath("AlternateTextQualifiers.txt"), dest, true,
          progress, UnitTestStatic.Token);
        Assert.IsTrue(FileSystemUtils.FileExists(dest));
        Assert.AreEqual(new FileInfo(UnitTestStatic.GetTestPath("AlternateTextQualifiers.txt")).Length,
          new FileInfo(dest).Length);
      }
      finally
      {
        FileSystemUtils.FileDelete(dest);
      }
    }

    [TestMethod]
    [Ignore("PathTooLongException when compiled in .net standard")]
    public void FileInfo()
    {
      var testFile = GetLongFileName("InfoTest.txt", false);

      var test = new FileSystemUtils.FileInfo(testFile);
      Assert.AreEqual(testFile, test.Name);

      var testFile2 = GetLongFileName("InfoTest2.txt", true);
      var test2 = new FileSystemUtils.FileInfo(testFile2);
      Assert.IsTrue(test2.Exists);
      FileSystemUtils.FileDelete(testFile2);

      var date = new DateTime(2020, 10, 17, 17, 23, 44);
      var test3 = new FileSystemUtils.FileInfo(testFile, 643788L, date);
      Assert.AreEqual(643788L, test3.Length);
      Assert.AreEqual(date, test3.LastWriteTimeUtc);
    }

    [TestMethod]
    public void GetLatestFileOfPattern()
    {
      var root = FileSystemUtils.ExecutableDirectoryName();
      var res = FileSystemUtils.GetLatestFileOfPattern(root, "CsvTools.ClassLibraryCSV.*.dll");
      Assert.AreEqual(root + Path.DirectorySeparatorChar + "CsvTools.ClassLibraryCSV.UnitTest.dll", res);
    }

    [TestMethod]
    public void GetRelativeFolder()
    {
      var root = FileSystemUtils.ExecutableDirectoryName();
      Assert.AreEqual($"TestFiles{Path.DirectorySeparatorChar}SubFolder{Path.DirectorySeparatorChar}", (root + Path.DirectorySeparatorChar + $"TestFiles{Path.DirectorySeparatorChar}SubFolder").GetRelativeFolder(root));
      Assert.AreEqual($"TestFiles{Path.DirectorySeparatorChar}SubFolder{Path.DirectorySeparatorChar}", (root + Path.DirectorySeparatorChar + $"TestFiles{Path.DirectorySeparatorChar}SubFolder{Path.DirectorySeparatorChar}").GetRelativeFolder(root));
      // Assert.AreEqual("Debug\\TestFiles\\SubFolder\\", (root +
      // "\\TestFiles\\SubFolder").GetRelativeFolder(root +"\\.."));
      Assert.AreEqual($"..{Path.DirectorySeparatorChar}Debug{Path.DirectorySeparatorChar}TestFiles{Path.DirectorySeparatorChar}SubFolder{Path.DirectorySeparatorChar}",
        (root + Path.DirectorySeparatorChar + $"..{Path.DirectorySeparatorChar}Debug{Path.DirectorySeparatorChar}TestFiles{Path.DirectorySeparatorChar}SubFolder").GetRelativeFolder(root));
    }

    [TestMethod]
    public void GetRelativePath()
    {
      var root = FileSystemUtils.ExecutableDirectoryName();
      Assert.AreEqual($"TestFiles{Path.DirectorySeparatorChar}BasicCSV.txt", (root + Path.DirectorySeparatorChar + $"TestFiles{Path.DirectorySeparatorChar}BasicCSV.txt").GetRelativePath(root));
    }

    [TestMethod]
    public void GetShortestPathRel()
    {
      var path = $"..{Path.DirectorySeparatorChar}CsvHelperTest.cs";
      Assert.AreEqual(path, path.GetShortestPath("."), "Can not be shorter");
    }

    [TestMethod]
    public void GetStreamReaderForFileOrResource()
    {
      // load a known resource from the DLL
      var test1 = FileSystemUtils.GetStreamReaderForFileOrResource("DateTimeFormats.txt");
      Assert.IsNotNull(test1);

      try
      {
        // load a unknown resource from this DLL
        _ = FileSystemUtils.GetStreamReaderForFileOrResource("SampleFile2.txt");
      }
      catch (ArgumentException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail("Wrong Exception Type: " + ex.GetType());
      }
    }

    [TestMethod]
    public void GroupFromFileNameInMain()
    {
      var setting2 =
        new CsvFile(id: "csv", fileName: $"..{Path.DirectorySeparatorChar}TestFile.csv")
        {
          RootFolder = UnitTestStatic.ApplicationDirectory
        };
      var dn = FileSystemUtils.SplitPath(setting2.FullPath).DirectoryName;

      Assert.AreEqual($"..{Path.DirectorySeparatorChar}", dn.GetRelativeFolder(UnitTestStatic.ApplicationDirectory));
    }

    [TestMethod]
    public void LongFileName()
    {
      Assert.AreEqual("", "".LongFileName());
      Assert.AreEqual("Test.txt", "Test.txt".LongFileName());
      var fn = UnitTestStatic.GetTestPath("This is a very lOng filename.txt");
      Assert.IsTrue(fn.ShortFileName().LongFileName().EndsWith("This is a very lOng filename.txt"));
    }

    [TestMethod]
    public void RemovePrefix()
    {
      Assert.AreEqual("Test", "Test".RemovePrefix());
      Assert.AreEqual("", "".RemovePrefix());

      var fn = GetLongFileName("CsvDataReaderUnitTestReadFiles.txt", false);
      Assert.AreEqual(fn, fn.LongPathPrefix().RemovePrefix());
    }

    [TestMethod]
    public void ResolvePattern()
    {
      var fileName1 = UnitTestStatic.GetTestPath("ResolvePattern_a.txt");
      var fileName2 = UnitTestStatic.GetTestPath("ResolvePattern_b.txt");
      try
      {
        FileSystemUtils.FileDelete(fileName1);
        FileSystemUtils.FileDelete(fileName2);

        FileSystemUtils.WriteAllText(fileName1, "Hello World\n");
        Thread.Sleep(500);
        FileSystemUtils.WriteAllText(fileName2, "Another File\n");

        var res = FileSystemUtils.ResolvePattern(Path.Combine(fileName1.GetDirectoryName(), "ResolvePattern_*.txt"));
        Assert.AreEqual(fileName2, res);
      }
      finally
      {
        FileSystemUtils.FileDelete(fileName1);
        FileSystemUtils.FileDelete(fileName2);
      }
    }

    [TestMethod]
    public void ShortFileName()
    {
      Assert.AreEqual("", "".ShortFileName());
      var fn = UnitTestStatic.GetTestPath(".");
      Assert.IsTrue(fn.ShortFileName().Contains("CSVQUI~"), fn.ShortFileName());
    }

    [TestMethod]
    public void SplitPath()
    {
      var split = FileSystemUtils.SplitPath(Path.Combine("C:", "MyTest", "Test.dat"));
      Assert.IsTrue(split.DirectoryName.Contains("MyTest"));
      Assert.AreEqual("Test.dat", split.FileName);
      Assert.AreEqual("Test", split.FileNameWithoutExtension);
      Assert.AreEqual(".dat", split.Extension);
    }

    [TestMethod]
    public void SplitPath_NoDirectory()
    {
      var dn = FileSystemUtils.SplitPath("FileName.Ext");
      Assert.AreEqual("FileName.Ext", dn.FileName);
      Assert.IsTrue(string.IsNullOrEmpty(dn.DirectoryName));
    }

    [TestMethod]
    public void TestGetAbsolutePath()
    {
      var root = FileSystemUtils.ExecutableDirectoryName();
      Directory.SetCurrentDirectory(root);
      Assert.AreEqual(root + Path.DirectorySeparatorChar + "TestFile.docx", "TestFile.docx".GetAbsolutePath(""));
      Assert.AreEqual(root + Path.DirectorySeparatorChar + "TestFile.docx", "TestFile.docx".GetAbsolutePath("."));
      Assert.AreEqual(root + Path.DirectorySeparatorChar + "TestFile.docx", ("."+  Path.DirectorySeparatorChar + "TestFile.docx").GetAbsolutePath(""));
      Assert.AreEqual(root + Path.DirectorySeparatorChar + "TestFile.docx", ("." +  Path.DirectorySeparatorChar + "TestFile.docx").GetAbsolutePath("."));
#if Windows
      Assert.AreEqual("C:\\TestFile.docx", "C:\\TestFile.docx".GetAbsolutePath("."));
      Assert.AreEqual("C:\\TestFile.docx", "C:\\TestFile.docx".GetAbsolutePath(""));
#endif
    }

    [TestMethod]
    [Ignore("PathTooLongException when compiled in .net standard")]
    public void TestMethodsOnLongPath()
    {
      Directory.SetCurrentDirectory(UnitTestStatic.ApplicationDirectory);
      var relPath = ".";
      var directory = UnitTestStatic.ApplicationDirectory;
      while (directory.Length < 260)
      {
        relPath += Path.DirectorySeparatorChar + "This is a subfolder";
        directory += Path.DirectorySeparatorChar + "This is a subfolder";
        FileSystemUtils.CreateDirectory(directory);
      }

      for (var counter = 0; counter < 10; counter++)
      {
        using var stream = FileSystemUtils.CreateText(directory + Path.DirectorySeparatorChar + $"File{counter:000}.txt");
        stream.WriteLine($"Small Test {counter:000}");
      }

      var fn1 = FileSystemUtils.ResolvePattern(directory + Path.DirectorySeparatorChar + "File*.txt");
      Assert.IsNotNull(fn1);

      var fn2 = FileSystemUtils.ResolvePattern(relPath + Path.DirectorySeparatorChar + "File*.txt");
      Assert.AreEqual(fn1, fn2);

      // cleanup
      for (var counter = 0; counter < 10; counter++) FileSystemUtils.FileDelete(directory + Path.DirectorySeparatorChar + $"File{counter:000}.txt");
    }

    [TestMethod]
    public void UtilsCreate()
    {
      var fn = UnitTestStatic.GetTestPath("out1.txt");
      using (var result = FileSystemUtils.Create(fn, 512, FileOptions.Asynchronous))
      {
        Assert.IsNotNull(result);
        result.Close();
      }
      FileSystemUtils.FileDelete(fn);
    }

    [TestMethod]
    public void UtilsCreateText()
    {
      var fn = UnitTestStatic.GetTestPath("out2.txt");
      using (var result = FileSystemUtils.CreateText(fn))
      {
        Assert.IsNotNull(result);
        result.Close();
      }
      FileSystemUtils.FileDelete(fn);
    }
#if Windows

    [TestMethod]
    public void ShortFileName_LongFileName()
    {
      Assert.AreEqual("", "".ShortFileName());
      Assert.AreEqual("", "".LongFileName());

      Assert.AreEqual($"C:{Path.DirectorySeparatorChar}CsvHelperTest.cs", $"C:{Path.DirectorySeparatorChar}CsvHelperTest.cs".ShortFileName());

      var root = FileSystemUtils.ExecutableDirectoryName();
      var fn = root + Path.DirectorySeparatorChar + "VeryLongNameForAFile.txt";
      using (var sw = File.CreateText(fn))
      {
        sw.WriteLine("Hello");
        sw.WriteLine("And");
        sw.WriteLine("Welcome");
      }
      var sfn = fn.ShortFileName();
      Assert.IsTrue(sfn.EndsWith("VeryLo~1.txt", StringComparison.OrdinalIgnoreCase), $"EndsWith VeryLo~1.txt {sfn}");
      Assert.AreEqual(fn, sfn.LongFileName());

      FileSystemUtils.FileDelete(fn);
    }

    [TestMethod]
    public void GetShortestPathABS()
    {
      var path = $"C:{Path.DirectorySeparatorChar}CsvHelperTest.cs";
      Assert.AreEqual(path, path.GetShortestPath("."), "Can not be shorter");
    }

#endif
#if Windows

    [TestMethod]
    public void SafePath() => Assert.AreEqual($"Test$Files{Path.DirectorySeparatorChar}Basic$CSV.txt", $"Test|Files{Path.DirectorySeparatorChar}Basic<CSV.txt".SafePath("$"));

#endif
    [TestMethod]
    public void WriteAllText()
    {
      var fn = UnitTestStatic.GetTestPath("Test3.txt");
      if (File.Exists(fn))
        File.Delete(fn);
      FileSystemUtils.WriteAllText(fn, "Hello World\n");
      Assert.IsTrue(File.Exists(fn));
      File.Delete(fn);
    }
    [TestMethod]
    public void WriteAllText2()
    {
      var text= "Contens\nSecondLine";
      var fileName1 = UnitTestStatic.GetTestPath("TestFile4.txt");
      FileSystemUtils.FileDelete(fileName1);
      FileSystemUtils.WriteAllText(fileName1, text, Encoding.UTF8);
      Assert.IsTrue(File.Exists(fileName1));
      Assert.AreEqual(text,FileSystemUtils.ReadAllText(fileName1));
      FileSystemUtils.FileDelete(fileName1);
    } 

    private string GetLongFileName(string fn, bool create)
    {
      Directory.SetCurrentDirectory(UnitTestStatic.ApplicationDirectory);
      var directory = UnitTestStatic.ApplicationDirectory;
      while (directory.Length < 260)
      {
        directory += "\\This is a subfolder";
        if (create && !FileSystemUtils.DirectoryExists(directory))
          FileSystemUtils.CreateDirectory(directory);
      }

      if (directory[directory.Length - 1] != Path.DirectorySeparatorChar)
        directory += Path.DirectorySeparatorChar;

      if (create)
      {
        using var stream = FileSystemUtils.CreateText(directory + fn);
        stream.WriteLine($"Small Test {fn}");
      }

      return directory + fn;
    }
  }
}