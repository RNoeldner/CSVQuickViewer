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
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class BinaryFormatterTests
  {
    [TestMethod]
    public void GetFileNameTest()
    {
      using var dt = UnitTestStatic.GetDataTable();
      using var reader = new DataTableReader(dt);
      reader.Read();
      var res1 = BinaryFormatter.GetFileName("{int}{double}.jpg", reader);
      reader.Read();
      var res2 = BinaryFormatter.GetFileName("{ID}{DateTime}_{PartEmpty}.pgp", reader);
    }

    [TestMethod]
    public async Task WriteFileTestAsync()
    {
      byte[] fileBytes = File.ReadAllBytes(UnitTestStatic.GetTestPath("BasicCSV.txt.gz"));

      await BinaryFormatter.WriteFileAsync(fileBytes, UnitTestStatic.ApplicationDirectory, "NewFile.gz", true, null, UnitTestStatic.Token);

      FileSystemUtils.FileDelete(UnitTestStatic.GetTestPath("NewFile.gz"));
    }
  }
}