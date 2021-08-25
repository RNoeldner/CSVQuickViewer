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
      var setting = new CsvFile { FileName = UnitTestInitializeCsv.GetTestPath("BasicCsV.txt") };
      using var res = new ImprovedStream(new SourceAccess(setting, true));
      Assert.IsNotNull(res);
    }

    [TestMethod]
    public void OpenReadTestGZipSmallRead()
    {
      using var res = new ImprovedStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("BasicCsV.txt.gz"), true));
      Assert.IsNotNull(res);
      var result1 = new byte[2048];
      using (var reader = new BinaryReader(res))
      {
        reader.Read(result1, 0, result1.Length);
      }

      // should return to teh start
      res.Seek(0, SeekOrigin.Begin);
      var result2 = new byte[2048];
      using (var reader = new BinaryReader(res))
      {
        reader.Read(result2, 0, result2.Length);
      }

      Assert.AreEqual(result1[0], result2[0]);
      Assert.AreEqual(result1[1], result2[1]);
      Assert.AreEqual(result1[2], result2[2]);
      Assert.AreEqual(result1[3], result2[3]);
      Assert.AreEqual(result1[4], result2[4]);
      Assert.AreEqual(result1[5], result2[5]);
    }

    [TestMethod]
    public void OpenReadTestZipSmallRead()
    {
      var sourceAccess = new SourceAccess(UnitTestInitializeCsv.GetTestPath("AllFormatsPipe.zip"), true);

      // opeing without IdentifierInContainer should return teh first file entry
      using (var res = new ImprovedStream(sourceAccess))
      {
        Assert.AreEqual("AllFormatsPipe.txt", sourceAccess.IdentifierInContainer);
        Assert.IsNotNull(res);
        var result1 = new byte[2048];
        using (var reader = new BinaryReader(res))
        {
          reader.Read(result1, 0, result1.Length);
        }

        // should return to teh start
        res.Seek(0, SeekOrigin.Begin);
        var result2 = new byte[2048];
        using (var reader = new BinaryReader(res))
        {
          reader.Read(result2, 0, result2.Length);
        }

        Assert.AreEqual(result1[0], result2[0]);
        Assert.AreEqual(result1[1], result2[1]);
        Assert.AreEqual(result1[2], result2[2]);
        Assert.AreEqual(result1[3], result2[3]);
        Assert.AreEqual(result1[4], result2[4]);
        Assert.AreEqual(result1[5], result2[5]);
      }

      // now sourceAccess.IdentifierInContainer is set,
      using (var res = new ImprovedStream(sourceAccess))
      {
        Assert.IsNotNull(res);
      }
    }

    [TestMethod]
    public void OpenReadTestGZipLargeRead()
    {
      using var res = new ImprovedStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("Larger.json.gz"), true));
      Assert.IsNotNull(res);

      var result1 = new byte[10000];

      // read a potion that is larger than the buffered stream
      using (var reader = new BinaryReader(res))
      {
        reader.Read(result1, 0, result1.Length);
      }

      // should return to the start
      res.Seek(0, SeekOrigin.Begin);
      var result2 = new byte[10000];
      using (var reader = new BinaryReader(res))
      {
        reader.Read(result2, 0, result2.Length);
      }

      Assert.AreEqual(result1[0], result2[0]);
      Assert.AreEqual(result1[1], result2[1]);
      Assert.AreEqual(result1[2], result2[2]);
      Assert.AreEqual(result1[3], result2[3]);
      Assert.AreEqual(result1[4], result2[4]);
      Assert.AreEqual(result1[5], result2[5]);
    }

    [TestMethod]
    public void OpenReadTestRegular()
    {
      using var res = new ImprovedStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("BasicCsV.txt"), true));
      Assert.IsNotNull(res);
    }

    private void WriteFile(string fileName, string password, string internalName, bool remove = false)
    {
      var fullname = UnitTestInitializeCsv.GetTestPath(fileName);

      var encoding = EncodingHelper.GetEncoding(65001, true);
      const string c_Line1 = "This is a test of compressed data written to a file";
      const string c_Line2 = "Yet another line to be written";
      const string c_Line3 = "A text with non ASCII characters: Raphael Nöldner";

      var sourceAccsss = new SourceAccess(fullname, false);
      if (string.IsNullOrEmpty(password))
        sourceAccsss.EncryptedPassphrase=password;
      if (!string.IsNullOrEmpty(internalName))
        sourceAccsss.IdentifierInContainer=internalName;

      using (var improvedStream = new ImprovedStream(sourceAccsss))
      {
        using (var writer = new StreamWriter(improvedStream, encoding, 8192))
        {
          writer.WriteLine(c_Line1);
          writer.WriteLine(c_Line2);
          writer.WriteLine(c_Line3);
          writer.WriteLine();
          writer.WriteLine(c_Line1);
        }

        improvedStream.Close();
      }

      Assert.IsTrue(FileSystemUtils.FileExists(fullname), "Check if File is created" + fileName);
      sourceAccsss = new SourceAccess(fullname, true);
      if (string.IsNullOrEmpty(password))
        sourceAccsss.EncryptedPassphrase=password;

      using (var improvedStream = new ImprovedStream(sourceAccsss))
      {
        using (var textReader = new StreamReader(improvedStream, encoding, true))
        {
          Assert.AreEqual(c_Line1, textReader.ReadLine(), "Line 1 : " + fileName);
          Assert.AreEqual(c_Line2, textReader.ReadLine(), "Line 2 : " + fileName);
          Assert.AreEqual(c_Line3, textReader.ReadLine(), "Line 3 : " + fileName);
          Assert.AreEqual(string.Empty, textReader.ReadLine(), "Line 4 : " + fileName);
          Assert.AreEqual(c_Line1, textReader.ReadLine(), "Line 5 : " + fileName);
        }

        improvedStream.Close();
      }

      if (remove)
        FileSystemUtils.FileDelete(fullname);
    }

    [TestMethod]
#pragma warning disable CS8625 // Ein NULL-Literal kann nicht in einen Non-Nullable-Verweistyp konvertiert werden.
    public void OpenWriteTestGZip() => WriteFile("WriteText.gz", null, null, true);

    [TestMethod]
    public void OpenWriteTestRegular() => WriteFile("WriteText.txt", null, null, true);

    [TestMethod]
    public void OpenWriteTestZip() => WriteFile("WriteText.Zip", null, null, true);

    [TestMethod]
    public void OpenWriteTestZipPassword() => WriteFile("WriteText2.Zip", "Test", null, true);

#pragma warning restore CS8625 // Ein NULL-Literal kann nicht in einen Non-Nullable-Verweistyp konvertiert werden.

    [TestMethod]
    public void OpenWriteTestZipAddUpdate()
    {
      var fn = "WriteText3.Zip";
      var path = UnitTestInitializeCsv.GetTestPath(fn);

      // Create
      WriteFile(fn, "Test", "Test.txt", false);
      WriteFile(fn, "Test", "Test2.txt", false);
      // Update
      WriteFile(fn, "Test", "Test.txt", false);

      using (var zipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(File.OpenRead(path)))
      {
        Assert.AreEqual(2, zipFile.Count);
      }
      FileSystemUtils.FileDelete(path);
    }

    [TestMethod]
    public void OpenWriteTestZipAddTwo()
    {
      var fn = "WriteText4.Zip";
      var path = UnitTestInitializeCsv.GetTestPath(fn);

      // Create two files
      WriteFile(fn, "Test", "Test.txt", false);
      WriteFile(fn, "Test", "Test2.txt", false);

      using (var zipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(File.OpenRead(path)))
      {
        Assert.AreEqual(2, zipFile.Count);
      }
      FileSystemUtils.FileDelete(path);
    }

    [TestMethod]
    public void CloseTest()
    {
      var res = new ImprovedStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("BasicCsV.txt"), true));
      res.Close();
    }

    [TestMethod]
    public void DisposeTest()
    {
      var res = new ImprovedStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("BasicCsV.txt"), true));
      res.Dispose();
    }
  }
}