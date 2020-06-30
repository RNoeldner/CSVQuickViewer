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
using System.Linq;
using System.Threading;
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

      // load a known resource from this DLL
      var test2 = FileSystemUtils.GetStreamReaderForFileOrResource("SampleFile.txt");
      Assert.IsNotNull(test2);
    }


    [TestMethod]
    public async Task FileCopy()
    {
      var dest = UnitTestInitializeCsv.GetTestPath("xyz.txt");
      try
      {
        using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token, null))
        {
          processDisplay.Maximum = -100;

          Assert.IsFalse(FileSystemUtils.FileExists(dest));
          await FileSystemUtils.FileCopy(UnitTestInitializeCsv.GetTestPath("AllFormats.txt"), dest, processDisplay);
          Assert.IsTrue(FileSystemUtils.FileExists(dest));
          Assert.AreEqual(-100, processDisplay.Maximum);

          // Copy again, the old file should be overwritten
          await FileSystemUtils.FileCopy(UnitTestInitializeCsv.GetTestPath("AlternateTextQualifiers.txt"), dest, processDisplay);
          Assert.IsTrue(FileSystemUtils.FileExists(dest));
          Assert.AreEqual((new FileInfo(UnitTestInitializeCsv.GetTestPath("AlternateTextQualifiers.txt"))).Length, new FileInfo(dest).Length);
        }
      }
      finally
      {
        FileSystemUtils.FileDelete(dest);
      }
    }

    [TestMethod]
    public void CreateDirectory()
    {
      if (FileSystemUtils.DirectoryExists(".\\Test\\"))
        Directory.Delete(".\\Test\\");
      Assert.IsFalse(FileSystemUtils.DirectoryExists(".\\Test\\"));
      FileSystemUtils.CreateDirectory(".\\Test\\");
      Assert.IsTrue(FileSystemUtils.DirectoryExists(".\\Test\\"));
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
      Assert.AreEqual("", FileSystemUtils.ShortFileName(""));
      Assert.AreEqual("", FileSystemUtils.LongFileName(""));
      Assert.AreEqual("C:\\CsvHelperTest.cs", FileSystemUtils.ShortFileName("C:\\CsvHelperTest.cs"));
      var root = FileSystemUtils.ExecutableDirectoryName();
      var fn = root + "\\VeryLongNameForAFile.txt";
      using (var sw = File.CreateText(fn))
      {
        sw.WriteLine("Hello");
        sw.WriteLine("And");
        sw.WriteLine("Welcome");
      }

      var sfn = FileSystemUtils.ShortFileName(fn);
      Assert.IsTrue(sfn.EndsWith("VeryLo~1.txt", StringComparison.OrdinalIgnoreCase), sfn);
      Assert.AreEqual(fn, FileSystemUtils.LongFileName(sfn));
      FileSystemUtils.FileDelete(fn);
    }

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
    public void SafePath() => Assert.AreEqual("Test$Files\\Basic$CSV.txt", FileSystemUtils.SafePath("Test|Files\\Basic<CSV.txt", "$"));

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
      {
        using (var stream = FileSystemUtils.CreateText(directory + $"\\File{counter:000}.txt"))
        {
          stream.WriteLine($"Small Test {counter:000}");
        }
      }

      var fn1 = FileSystemUtils.ResolvePattern(directory + $"\\File*.txt");
      Assert.IsNotNull(fn1);

      var fn2 = FileSystemUtils.ResolvePattern(relPath + $"\\File*.txt");
      Assert.AreEqual(fn1, fn2);

      // cleanup
      for (var counter = 0; counter < 10; counter++)
      {
        FileSystemUtils.FileDelete(directory + $"\\File{counter:000}.txt");
      }
    }
  }
}