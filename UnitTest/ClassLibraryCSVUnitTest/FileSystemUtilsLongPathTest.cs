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
using System.Text;

namespace CsvTools.Tests
{
  [TestClass]
  public class FileSystemUtilsLongPathTest
  {
    private const string pre = @"\\?\";

    private const string line1 = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit.";

    // root folder 
    private static string m_Folder0;

    // directory with a total length of  259,  still valid but with a filename failing
    private static string m_Folder1;

    // directory with a total long of more than 260,  
    private static string m_Folder2;

    private static string root;

    private static string m_FileName1;
    private static string m_FileName2;

    [ClassInitialize]
    public static void MyClassInit(TestContext tc)
    {
      root = Path.GetFullPath(".");
      if (root.Length >= 236)
        root = @"C:";
      m_Folder0 = Path.Combine(root, "TestLongPath");
      Directory.CreateDirectory(m_Folder0);

      m_Folder1 = Path.Combine(m_Folder0, new string('1', 246 - m_Folder0.Length));
      Directory.CreateDirectory(m_Folder1);


      m_FileName1 = Path.Combine(m_Folder1, "TestFile.txt");
      var contens =
        line1 +
        "\r\nAenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem.\r\nNulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede";
      File.WriteAllText(pre + m_FileName1, contens);

      m_Folder2 = Path.Combine(m_Folder0, new string('2', 265 - m_Folder0.Length));
      Directory.CreateDirectory(pre + m_Folder2);
      m_FileName2 = Path.Combine(m_Folder2, "TestFile.txt");
      File.WriteAllText(pre + m_FileName2, contens);
    }

    private static void EmptyFolder(string folderName)
    {
      foreach (var file in Directory.EnumerateFiles(folderName))
        if (file.StartsWith(pre))
          File.Delete(file);
        else
          File.Delete(pre + file);
    }

    [ClassCleanup]
    public static void MyClassCleanup()
    {
      EmptyFolder(pre + m_Folder2);
      EmptyFolder(m_Folder1);
      EmptyFolder(m_Folder0);
      Directory.Delete(pre + m_Folder2);
      Directory.Delete(m_Folder1);
      Directory.Delete(m_Folder0);
    }

    [TestMethod]
    public void OpenWrite()
    {
      var fn1 = Path.Combine(m_Folder1, "Test1.txt");
      using (var fs = FileSystemUtils.OpenWrite(fn1))
      using (var sw = new StreamWriter(fs, Encoding.UTF8))
      {
        sw.WriteLine("This is a test");
      }

      Assert.IsTrue(FileSystemUtils.FileExists(fn1));

      var fn2 = Path.Combine(m_Folder2, "Test1.txt");
      using (var fs = FileSystemUtils.OpenWrite(fn2))
      using (var sw = new StreamWriter(fs, Encoding.UTF8))
      {
        sw.WriteLine("This is a test");
      }

      Assert.IsTrue(FileSystemUtils.FileExists(fn2));
    }

    [TestMethod]
    public void OpenRead()
    {
      using (var fs = FileSystemUtils.OpenRead(m_FileName1))
      using (var tr = new StreamReader(fs, Encoding.UTF8, true))
      {
        Assert.AreEqual(line1, tr.ReadLine());
      }

      using (var fs = FileSystemUtils.OpenRead(m_FileName2))
      using (var tr = new StreamReader(fs, Encoding.UTF8, true))
      {
        Assert.AreEqual(line1, tr.ReadLine());
      }
    }

    [TestMethod]
    public void GetFullPath()
    {
      Assert.AreEqual(m_FileName1, FileSystemUtils.GetFullPath(m_FileName1), "GetFullPath1");
      Assert.AreEqual(m_FileName2, FileSystemUtils.GetFullPath(m_FileName2), "GetFullPath2");

      Assert.AreEqual(m_FileName1, FileSystemUtils.GetFullPath(m_FileName1.Substring(root.Length + 1)), "GetFullPath3");
      Assert.AreEqual(m_FileName2, FileSystemUtils.GetFullPath("." + m_FileName2.Substring(root.Length)),
        "GetFullPath4");
    }

    [TestMethod]
    public void GetShortestPath()
    {
      Assert.AreEqual(m_FileName2, m_FileName2.Substring(root.Length + 1).GetAbsolutePath(root), "GetAbsolutePath1");
      Assert.AreEqual(m_FileName2, m_FileName2.GetAbsolutePath(root), "GetAbsolutePath2");

      Assert.AreEqual(m_FileName1.Substring(root.Length + 1), m_FileName1.GetShortestPath(root), "GetShortestPath");
    }
  }
}