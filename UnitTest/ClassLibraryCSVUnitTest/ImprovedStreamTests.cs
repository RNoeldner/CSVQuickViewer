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

namespace CsvTools.Tests
{
  [TestClass]
  public class ImprovedStreamTests
  {
    [TestMethod]
    public void OpenReadTestSetting()
    {
      var setting = new CsvFile {FileName = UnitTestInitializeCsv.GetTestPath("BasicCsV.txt")};
      using (var res = new ImprovedStream(setting.FullPath, true))
      {
        Assert.IsNotNull(res);
      }
    }

    [TestMethod]
    public void OpenReadTestgZip()
    {
      using (var res = new ImprovedStream(UnitTestInitializeCsv.GetTestPath("BasicCsV.txt.gz"), true))
      {
        Assert.IsNotNull(res);
      }
    }

    [TestMethod]
    public void OpenReadTestRegular()
    {
      using (var res = new ImprovedStream(UnitTestInitializeCsv.GetTestPath("BasicCsV.txt"), true))
      {
        Assert.IsNotNull(res);
      }
    }

    private void WriteFile(string fileName)
    {
      var fullname = UnitTestInitializeCsv.GetTestPath(fileName);

      var encoding = EncodingHelper.GetEncoding(65001, true);
      const string Line1 = "This is a test of compressed data written to a file";
      const string Line2 = "Yet another line to be written";
      const string Line3 = "A text with non ASCII characters: Raphael Nöldner";

      using (var improvedStream = new ImprovedStream(fullname, false))
      {
        using (var writer = new StreamWriter(improvedStream, encoding, 8192))
        {
          writer.WriteLine(Line1);
          writer.WriteLine(Line2);
          writer.WriteLine(Line3);
          writer.WriteLine();
          writer.WriteLine(Line1);
        }

        improvedStream.Close();
      }

      Assert.IsTrue(FileSystemUtils.FileExists(fullname), "Check if File is created" + fileName);

      using (var improvedStream = new ImprovedStream(fullname, true))
      {
        using (var textReader = new StreamReader(improvedStream, encoding, true))
        {
          Assert.AreEqual(Line1, textReader.ReadLine(), "Line 1 : " + fileName);
          Assert.AreEqual(Line2, textReader.ReadLine(), "Line 2 : " + fileName);
          Assert.AreEqual(Line3, textReader.ReadLine(), "Line 3 : " + fileName);
          Assert.AreEqual(string.Empty, textReader.ReadLine(), "Line 4 : " + fileName);
          Assert.AreEqual(Line1, textReader.ReadLine(), "Line 5 : " + fileName);
        }

        improvedStream.Close();
      }
    }

    [TestMethod]
    public void OpenWriteTestgZip() => WriteFile("WriteText.gz");

    [TestMethod]
    public void OpenWriteTestRegular() => WriteFile("WriteText.txt");

    [TestMethod]
    public void CloseTest()
    {
      var res = new ImprovedStream(UnitTestInitializeCsv.GetTestPath("BasicCsV.txt"), true);
      res.Close();
    }

    [TestMethod]
    public void DisposeTest()
    {
      var res = new ImprovedStream(UnitTestInitializeCsv.GetTestPath("BasicCsV.txt"), true);
      res.Dispose();
    }
  }
}