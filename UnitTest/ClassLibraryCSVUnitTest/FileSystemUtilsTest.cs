/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class FileSystemUtilsTest
  {
    [TestMethod]
    public void GetStreamReaderForFileOrResource()
    {
      // load a known resource from the DLL
      var test1 = FileSystemUtils.GetStreamReaderForFileOrResource("DateTimeFormats.txt");
      Assert.IsNotNull(test1);

      // load a unknown resource from this DLL
      var test2 = FileSystemUtils.GetStreamReaderForFileOrResource("SampleFile2.txt");
      Assert.IsNull(test2);
    }

    [TestMethod]
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
    public async Task FileCopy()
    {
      var dest = UnitTestInitializeCsv.GetTestPath("xyz.txt");
      try
      {
        using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
        {
          processDisplay.Maximum = -100;

          Assert.IsFalse(FileSystemUtils.FileExists(dest));
          await FileSystemUtils.FileCopy(UnitTestInitializeCsv.GetTestPath("AllFormats.txt"), dest, false,
            processDisplay);
          Assert.IsTrue(FileSystemUtils.FileExists(dest));
          Assert.AreEqual(-100, processDisplay.Maximum);

          // Copy again, the old file should be overwritten
          await FileSystemUtils.FileCopy(UnitTestInitializeCsv.GetTestPath("AlternateTextQualifiers.txt"), dest, true,
            processDisplay);
          Assert.IsTrue(FileSystemUtils.FileExists(dest));
          Assert.AreEqual(new FileInfo(UnitTestInitializeCsv.GetTestPath("AlternateTextQualifiers.txt")).Length,
            new FileInfo(dest).Length);
        }
      }
      finally
      {
        FileSystemUtils.FileDelete(dest);
      }
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
    public void CreateDirectory()
    {
      if (FileSystemUtils.DirectoryExists(".\\Test\\"))
        Directory.Delete(".\\Test\\");
      Assert.IsFalse(FileSystemUtils.DirectoryExists(".\\Test\\"));
      FileSystemUtils.CreateDirectory(".\\Test\\");
      Assert.IsTrue(FileSystemUtils.DirectoryExists(".\\Test\\"));

      // nothing bad happens
      FileSystemUtils.CreateDirectory("");
      FileSystemUtils.CreateDirectory(null);
    }

    [TestMethod]
    public void DirectoryName()
    {
      if (!FileSystemUtils.DirectoryExists(".\\Test\\"))
        FileSystemUtils.CreateDirectory(".\\Test\\");
      Assert.AreEqual(Path.GetFullPath(".\\Test"), ".\\Test".GetDirectoryName());
    }

    [TestMethod]
    public void ShortFileName_LongFileName()
    {
      Assert.AreEqual("", "".ShortFileName());
      Assert.AreEqual("", "".LongFileName());
      Assert.AreEqual("C:\\CsvHelperTest.cs", "C:\\CsvHelperTest.cs".ShortFileName());
      var root = FileSystemUtils.ExecutableDirectoryName();
      var fn = root + "\\VeryLongNameForAFile.txt";
      using (var sw = File.CreateText(fn))
      {
        sw.WriteLine("Hello");
        sw.WriteLine("And");
        sw.WriteLine("Welcome");
      }

      var sfn = fn.ShortFileName();
      Assert.IsTrue(sfn.EndsWith("VeryLo~1.txt", StringComparison.OrdinalIgnoreCase), sfn);
      Assert.AreEqual(fn, sfn.LongFileName());
      FileSystemUtils.FileDelete(fn);
    }

    [TestMethod]
    public void GetShortestPath()
    {
      Assert.AreEqual("C:\\CsvHelperTest.cs", "C:\\CsvHelperTest.cs".GetShortestPath("."));
      Assert.AreEqual("..\\CsvHelperTest.cs", "..\\CsvHelperTest.cs".GetShortestPath("."));
    }

    [TestMethod]
    public void GetRelativePath()
    {
      var root = FileSystemUtils.ExecutableDirectoryName();
      Assert.AreEqual("TestFiles\\BasicCSV.txt", (root + "\\TestFiles\\BasicCSV.txt").GetRelativePath(root));
    }

    [TestMethod]
    public void GroupFromFileNameInMain()
    {
      var setting2 = new CsvFile("..\\TestFile.csv") { RootFolder = UnitTestInitializeCsv.ApplicationDirectory };
      var dn = FileSystemUtils.SplitPath(setting2.FullPath).DirectoryName;

      Assert.AreEqual("..\\", dn.GetRelativeFolder(UnitTestInitializeCsv.ApplicationDirectory));
    }

    [TestMethod]
    public void GetRelativeFolder()
    {
      var root = FileSystemUtils.ExecutableDirectoryName();
      Assert.AreEqual("TestFiles\\SubFolder\\", (root + "\\TestFiles\\SubFolder").GetRelativeFolder(root));
      Assert.AreEqual("TestFiles\\SubFolder\\", (root + "\\TestFiles\\SubFolder\\").GetRelativeFolder(root));
      // Assert.AreEqual("Debug\\TestFiles\\SubFolder\\", (root +
      // "\\TestFiles\\SubFolder").GetRelativeFolder(root +"\\.."));
      Assert.AreEqual("..\\Debug\\TestFiles\\SubFolder\\",
        (root + "\\..\\Debug\\TestFiles\\SubFolder").GetRelativeFolder(root));
    }

    [TestMethod]
    public void SafePath() => Assert.AreEqual("Test$Files\\Basic$CSV.txt", "Test|Files\\Basic<CSV.txt".SafePath("$"));

    [TestMethod]
    public void GetLatestFileOfPattern()
    {
      var root = FileSystemUtils.ExecutableDirectoryName();
      var res = FileSystemUtils.GetLatestFileOfPattern(root, "ClassLibraryCSVU*.dll");
      Assert.IsTrue(res == root + "\\ClassLibraryCSVUnitTest.dll");
    }

    [TestMethod]
    public void ResolvePattern()
    {
      var root = FileSystemUtils.ExecutableDirectoryName();
      var res = FileSystemUtils.ResolvePattern(root + "\\ClassLibraryCSV*.dll");
      Assert.IsTrue(res == root + "\\ClassLibraryCSVUnitTest.dll" ||
                    res == root + "\\ClassLibraryCSV.dll");
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

    [TestMethod]
    public void SplitPath()
    {
      var split = FileSystemUtils.SplitPath("C:\\MyTest\\Test.dat");
      Assert.AreEqual("C:\\MyTest", split.DirectoryName);
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
    public void Create()
    {
      var fn = UnitTestInitializeCsv.GetTestPath("Test2.dat");
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
    public void WriteAllText()
    {
      var fn = UnitTestInitializeCsv.GetTestPath("Test3.txt");
      if (File.Exists(fn))
        File.Delete(fn);
      FileSystemUtils.WriteAllText(fn, "Hello World\n");
      Assert.IsTrue(File.Exists(fn));
      File.Delete(fn);
    }

    private string GetLongFileName(string fn, bool create)
    {
      Directory.SetCurrentDirectory(UnitTestInitializeCsv.ApplicationDirectory);
      var directory = UnitTestInitializeCsv.ApplicationDirectory;
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
        using (var stream = FileSystemUtils.CreateText(directory + fn))
          stream.WriteLine($"Small Test {fn}");
      }

      return directory + fn;
    }

    [TestMethod]
    public void TestMethodsOnLongPath()
    {
      Directory.SetCurrentDirectory(UnitTestInitializeCsv.ApplicationDirectory);
      var relPath = ".";
      var directory = UnitTestInitializeCsv.ApplicationDirectory;
      while (directory.Length < 260)
      {
        relPath += "\\This is a subfolder";
        directory += "\\This is a subfolder";
        FileSystemUtils.CreateDirectory(directory);
      }

      for (var counter = 0; counter < 10; counter++)
        using (var stream = FileSystemUtils.CreateText(directory + $"\\File{counter:000}.txt"))
        {
          stream.WriteLine($"Small Test {counter:000}");
        }

      var fn1 = FileSystemUtils.ResolvePattern(directory + "\\File*.txt");
      Assert.IsNotNull(fn1);

      var fn2 = FileSystemUtils.ResolvePattern(relPath + "\\File*.txt");
      Assert.AreEqual(fn1, fn2);

      // cleanup
      for (var counter = 0; counter < 10; counter++) FileSystemUtils.FileDelete(directory + $"\\File{counter:000}.txt");
    }
  }
}