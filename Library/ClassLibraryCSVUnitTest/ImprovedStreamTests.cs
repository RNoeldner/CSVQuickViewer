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
  [TestClass()]
  public class ImprovedStreamTests
  {
    private readonly string m_ApplicationDirectory = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles";



    [TestMethod()]
    public void OpenReadTestSetting()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCsV.txt")
      };
      using (var res = ImprovedStream.OpenRead(setting))
      {
        Assert.IsNotNull(res);
        Assert.IsNotNull(res.Stream);
      }
    }

    [TestMethod()]
    public void OpenReadTestRegular()
    {
      using (var res = ImprovedStream.OpenRead(Path.Combine(m_ApplicationDirectory, "BasicCsV.txt")))
      {
        Assert.IsNotNull(res);
        Assert.IsNotNull(res.Stream);
      }
    }

    [TestMethod()]
    public void OpenReadTestgZip()
    {
      using (var res = ImprovedStream.OpenRead(Path.Combine(m_ApplicationDirectory, "BasicCsV.txt.gz")))
      {
        Assert.IsNotNull(res);
        Assert.IsNotNull(res.Stream);
      }
    }

    [TestMethod()]
    public void OpenReadTestPGP()
    {
      PGPKeyStorageTestHelper.SetApplicationSetting();
      using (var res = ImprovedStream.OpenRead(Path.Combine(m_ApplicationDirectory, "BasicCsV.pgp"), () => { return "UGotMe".Encrypt(); }))
      {
        Assert.IsNotNull(res);
        Assert.IsNotNull(res.Stream);
      }
    }
    private void WriteFile(string fileName)
    {
      var fullname = Path.Combine(m_ApplicationDirectory, fileName);

      var encoding = EncodingHelper.GetEncoding(65001, true);
      const string Line1 = "This is a test of compressed data written to a file";
      const string Line2 = "Yet another line to be written";
      const string Line3 = "A text with non ASCII chacarters: Raphael Nöldner";

      using (var improvedStream = ImprovedStream.OpenWrite(fullname, PGPKeyStorageTestHelper.PGPKeyStorage.GetRecipients().Keys.First()))
      {
        using (var writer = new StreamWriter(improvedStream.Stream, encoding, 8192))
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

      using (var improvedStream = ImprovedStream.OpenRead(fullname, () => { return PGPKeyStorageTestHelper.PGPKeyStorage.EncryptedPassphase; }))
      {
        using (var textReader = new StreamReader(improvedStream.Stream, encoding, true))
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

    [TestMethod()]
    public void OpenWriteTestgZip() => WriteFile("WriteText.gz");


    [TestMethod()]
    public void OpenWriteTestRegular() => WriteFile("WriteText.txt");


    [TestMethod()]
    public void OpenWriteTestPgp()
    {
      PGPKeyStorageTestHelper.SetApplicationSetting();
      WriteFile("WriteText.pgp");
    }

    [TestMethod()]
    public void CloseTest()
    {
      var res = ImprovedStream.OpenRead(Path.Combine(m_ApplicationDirectory, "BasicCsV.txt"));
      res.Close();
    }

    [TestMethod()]
    public void DisposeTest()
    {
      var res = ImprovedStream.OpenRead(Path.Combine(m_ApplicationDirectory, "BasicCsV.txt"));
      res.Dispose();
    }
  }
}