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
  public class BinaryFormatterTests
  {
   

    [TestMethod]
    public void WriteFileTestAsync()
    {
      var testContent = BinaryFormatter.CombineNameAndContent("BasicCSV.txt.gz",
        File.ReadAllBytes(UnitTestStatic.GetTestPath("BasicCSV.txt.gz")));

      var bin = new BinaryFormatter(-1, UnitTestStatic.ApplicationDirectory, UnitTestStatic.ApplicationDirectory, "NewFile.gz");
      bin.Write(testContent, null, null);
      Assert.IsTrue(FileSystemUtils.FileExists(UnitTestStatic.GetTestPath("NewFile.gz")));
      FileSystemUtils.FileDelete(UnitTestStatic.GetTestPath("NewFile.gz"));
    }

    [TestMethod]
    public void FormatText()
    {
      var bin = new BinaryFormatter(-1, UnitTestStatic.ApplicationDirectory, UnitTestStatic.ApplicationDirectory, "");
      var res = bin.FormatInputText("BasicCSV.txt.gz", null);
      Assert.AreEqual("BasicCSV.txt.gz", BinaryFormatter.GetNameFromNameAndContent(res));
    }
  }
}