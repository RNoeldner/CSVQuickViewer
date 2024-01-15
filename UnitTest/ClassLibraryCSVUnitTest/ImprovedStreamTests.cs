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
      var setting = new CsvFileDummy { FileName= UnitTestStatic.GetTestPath("BasicCsV.txt") };
      using var res = new ImprovedStream(new SourceAccess(setting));
      Assert.IsNotNull(res);
    }


    [TestMethod]
    public async System.Threading.Tasks.Task PercentageAtEnd()
    {
      var buffer = new byte[32000];
      var setting = new CsvFileDummy { FileName= UnitTestStatic.GetTestPath("Warnings.txt") };
      // ReSharper disable once UseAwaitUsing
      using var res = new ImprovedStream(new SourceAccess(setting));
      Assert.AreEqual(0d, res.Percentage);
      // ReSharper disable once MustUseReturnValue
      await res.ReadAsync(buffer, 0, 32000).ConfigureAwait(false);
      Assert.AreEqual(1d, res.Percentage);
    }

    [TestMethod]
    public void EmptyFile()
    {
      var setting = new CsvFileDummy { FileName=  UnitTestStatic.GetTestPath("EmptyFile.txt") };
      using var res = new ImprovedStream(new SourceAccess(setting));
      Assert.AreEqual(1d, res.Percentage);
      Assert.AreEqual(-1, res.ReadByte());
    }

    [TestMethod]
    public void OpenReadTestGZipSmallRead()
    {
      using var res = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCsV.txt.gz")));
      Assert.IsNotNull(res);
      var result1 = new byte[2048];
      using (var reader = new BinaryReader(res))
      {
        // ReSharper disable once MustUseReturnValue
        reader.Read(result1, 0, result1.Length);
      }

      // should return to the start
      res.Seek(0, SeekOrigin.Begin);
      var result2 = new byte[2048];
      using (var reader = new BinaryReader(res))
      {
        // ReSharper disable once MustUseReturnValue
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
      var sourceAccess = new SourceAccess(UnitTestStatic.GetTestPath("AllFormatsPipe.zip"));

      // opening without IdentifierInContainer should return the first file entry
      using (var res = new ImprovedStream(sourceAccess))
      {
        Assert.AreEqual("AllFormatsPipe.txt", sourceAccess.IdentifierInContainer);
        Assert.IsNotNull(res);
        var result1 = new byte[2048];
        using (var reader = new BinaryReader(res))
        {
          // ReSharper disable once MustUseReturnValue
          reader.Read(result1, 0, result1.Length);
        }

        // should return to the start
        res.Seek(0, SeekOrigin.Begin);
        var result2 = new byte[2048];
        using (var reader = new BinaryReader(res))
        {
          // ReSharper disable once MustUseReturnValue
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
      using var res = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("Larger.json.gz")));
      Assert.IsNotNull(res);

      var result1 = new byte[10000];

      // read a potion that is larger than the buffered stream
      using (var reader = new BinaryReader(res))
      {
        // ReSharper disable once MustUseReturnValue
        reader.Read(result1, 0, result1.Length);
      }

      // should return to the start
      res.Seek(0, SeekOrigin.Begin);
      var result2 = new byte[10000];
      using (var reader = new BinaryReader(res))
      {
        // ReSharper disable once MustUseReturnValue
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
      using var res = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCsV.txt")));
      Assert.IsNotNull(res);
    }

    private void WriteFile(string fileName, string password, string internalName, bool remove = false)
    {
      var fullname = UnitTestStatic.GetTestPath(fileName);

      var encoding = EncodingHelper.GetEncoding(65001, true);
      const string line1 = "This is a test of compressed data written to a file";
      const string line2 = "Yet another line to be written";
      const string line3 = "A text with non ASCII characters: Raphael Nöldner";

      var sourceAccess = new SourceAccess(fullname, false);
      if (!string.IsNullOrEmpty(internalName))
        sourceAccess.IdentifierInContainer = internalName;

      using (var improvedStream = new ImprovedStream(sourceAccess))
      {
        using (var writer = new StreamWriter(improvedStream, encoding, 8192))
        {
          writer.WriteLine(line1);
          writer.WriteLine(line2);
          writer.WriteLine(line3);
          writer.WriteLine();
          writer.WriteLine(line1);
        }

        improvedStream.Close();
      }

      Assert.IsTrue(FileSystemUtils.FileExists(fullname), "Check if File is created" + fileName);
      sourceAccess = new SourceAccess(fullname);

      using (var improvedStream = new ImprovedStream(sourceAccess))
      {
        using (var textReader = new StreamReader(improvedStream, encoding, true))
        {
          Assert.AreEqual(line1, textReader.ReadLine(), "Line 1 : " + fileName);
          Assert.AreEqual(line2, textReader.ReadLine(), "Line 2 : " + fileName);
          Assert.AreEqual(line3, textReader.ReadLine(), "Line 3 : " + fileName);
          Assert.AreEqual(string.Empty, textReader.ReadLine(), "Line 4 : " + fileName);
          Assert.AreEqual(line1, textReader.ReadLine(), "Line 5 : " + fileName);
        }

        improvedStream.Close();
      }

      if (remove)
        FileSystemUtils.FileDelete(fullname);
    }

    [TestMethod]
#pragma warning disable CS8625
    public void OpenWriteTestGZip() => WriteFile("WriteText.gz", null, null, true);

    [TestMethod]
    public void OpenWriteTestRegular() => WriteFile("WriteText.txt", null, null, true);

    [TestMethod]
    public void OpenWriteTestZip() => WriteFile("WriteText.Zip", null, null, true);

    [TestMethod]
    public void OpenWriteTestZipPassword() => WriteFile("WriteText2.Zip", "Test", null, true);

#pragma warning restore CS8625

    [TestMethod]
    public void OpenWriteTestZipAddUpdate()
    {
      var fn = "WriteText3.Zip";
      var path = UnitTestStatic.GetTestPath(fn);

      // Create
      WriteFile(fn, "Test", "Test.txt");
      WriteFile(fn, "Test", "Test2.txt");
      // Update
      WriteFile(fn, "Test", "Test.txt");

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
      var path = UnitTestStatic.GetTestPath(fn);

      // Create two files
      WriteFile(fn, "Test", "Test.txt");
      WriteFile(fn, "Test", "Test2.txt");

      using (var zipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(File.OpenRead(path)))
      {
        Assert.AreEqual(2, zipFile.Count);
      }

      FileSystemUtils.FileDelete(path);
    }

    [TestMethod]
    public void CloseTest()
    {
      var res = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCsV.txt")));
      res.Close();
    }

    [TestMethod]
    public void DisposeTest()
    {
      var res = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCsV.txt")));
      res.Dispose();
    }
  }
}