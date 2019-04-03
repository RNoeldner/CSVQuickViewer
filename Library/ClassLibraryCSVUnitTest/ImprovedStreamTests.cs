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
  [TestClass()]
  public class ImprovedStreamTests
  {
    private readonly string m_ApplicationDirectory = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles";

    [TestMethod()]
    public void OpenReadTestSetting()
    {
      CsvFile setting = new CsvFile();
      setting.FileName = Path.Combine(m_ApplicationDirectory, "BasicCsV.txt");
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

    [TestMethod()]
    public void OpenWriteTestRegular()
    {
    }

    [TestMethod()]
    public void OpenWriteTestgZip()
    {
    }

    [TestMethod()]
    public void OpenWriteTestPgp()
    {
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