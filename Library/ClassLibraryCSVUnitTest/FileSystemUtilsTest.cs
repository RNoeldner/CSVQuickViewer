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

    [TestMethod]
    public void SplitPath()
    {
      var split = FileSystemUtils.SplitPath("C:\\MyTest\\Test.dat");
      Assert.AreEqual("C:\\MyTest", split.DirectoryName);
      Assert.AreEqual("Test.dat", split.FileName);
      Assert.AreEqual("Test", split.FileNameWithoutExtension);
      Assert.AreEqual(".dat", split.Extension);
    }
    }
}