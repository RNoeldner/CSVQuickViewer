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
  public class ImprovedStreamPGPTests
  {
    // ReSharper disable UseAwaitUsing

    [TestMethod]
    public async System.Threading.Tasks.Task OpenReadTestRegularAsync()
    {
      using var res = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCsV.txt")));
      Assert.IsTrue(res.CanRead);
      Assert.IsTrue(res.CanSeek);
      Assert.IsFalse(res.CanWrite);
      Assert.AreEqual(0d, res.Percentage);
      var buffer = new byte[10];
      Assert.AreEqual(10, await res.ReadAsync(buffer, 0, 10));
      res.Close();
    }

    [TestMethod]
    public async System.Threading.Tasks.Task OpenReadTestPGPAsync()
    {
      UnitTestInitialize.SetApplicationPGPSetting();
      using var res = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCsV.pgp")));
      Assert.IsTrue(res.CanRead);
      Assert.IsTrue(res.CanSeek);
      Assert.IsFalse(res.CanWrite);
      var buffer = new byte[10];
      Assert.AreEqual(10, await res.ReadAsync(buffer, 0, 10));
      res.Close();
    }

    private void WriteFile(string fileName, bool keepEncrypted)
    {
      var fullname = UnitTestStatic.GetTestPath(fileName);

      var encoding = EncodingHelper.GetEncoding(65001, true);
      const string line1 = "This is a test of compressed data written to a file";
      const string line2 = "Yet another line to be written";
      const string line3 = "A text with non ASCII character: Raphael Nöldner";

      using (var improvedStream =
        new ImprovedStream(new SourceAccess(fullname, false,null,  keepEncrypted: keepEncrypted)))
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

      using (var improvedStream = new ImprovedStream(new SourceAccess(fullname)))
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
    }

    [TestMethod]
    public void OpenWriteTestgZip() => WriteFile("WriteText.gz", false);

    [TestMethod]
    public void OpenWriteTestgZipKeep() => WriteFile("WriteText.txt.gz", true);

    [TestMethod]
    public void OpenWriteTestRegular() => WriteFile("WriteText.txt",false);
  }
}