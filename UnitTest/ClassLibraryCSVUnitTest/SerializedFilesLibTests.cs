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

namespace CsvTools.Tests
{
  [TestClass]
  public class SerializedFilesLibTests
  {
    private const string fileName = @".\SerializedFilesLibCSV.xml";

    [TestMethod]
    public void DeleteWithBackupTest()
    {
      var file = GetCsvFile();
      FileSystemUtils.DeleteWithBackup(fileName, false);
      Assert.IsFalse(FileSystemUtils.FileExists(fileName));
      Assert.IsFalse(FileSystemUtils.FileExists(fileName + ".bak"));

      SerializedFilesLib.SaveCsvFile(fileName, file, () => { return false; });
      Assert.IsTrue(FileSystemUtils.FileExists(fileName));
      FileSystemUtils.DeleteWithBackup(fileName, false);
      Assert.IsFalse(FileSystemUtils.FileExists(fileName));
      Assert.IsTrue(FileSystemUtils.FileExists(fileName + ".bak"));
    }

    [TestInitialize]
    public void Init()
    {
      FileSystemUtils.FileDelete(fileName);
      FileSystemUtils.FileDelete(fileName + ".bak");
    }

    [TestMethod]
    public void SaveAndLoadCsvFileTest()
    {
      var file = GetCsvFile();
      Assert.IsFalse(FileSystemUtils.FileExists(fileName));
      SerializedFilesLib.SaveCsvFile(fileName, file, () => { return true; });
      Assert.IsTrue(FileSystemUtils.FileExists(fileName));
      var test = SerializedFilesLib.LoadCsvFile(fileName);

      Assert.AreNotSame(file, test);
      Assert.IsInstanceOfType(test, typeof(CsvFile));

      file.AllPropertiesEqual(test);
      // Test Properties that are not tested

      Assert.AreEqual(file.MappingCollection.Count, test.MappingCollection.Count, "FieldMapping");
      Assert.AreEqual(TrimmingOption.Unquoted, test.TrimmingOption, "TrimmingOption");
      Assert.IsTrue(file.MappingCollection.CollectionEqual(test.MappingCollection), "Mapping");
      Assert.IsTrue(file.FileFormat.Equals(test.FileFormat), "FileFormat");
    }

    [TestMethod]
    public void SaveCsvFileTest()
    {
      var file = GetCsvFile();
      Assert.IsFalse(FileSystemUtils.FileExists(fileName));
      var asked = false;
      SerializedFilesLib.SaveCsvFile(fileName, file, () => true);
      file.ID = "Test1000";
      Assert.IsTrue(FileSystemUtils.FileExists(fileName));
      SerializedFilesLib.SaveCsvFile(fileName, file, () =>
      {
        asked = true;
        return true;
      });
      Assert.IsTrue(asked);
    }

    private CsvFile GetCsvFile()
    {
      var file = new CsvFile
      {
        ID = "TestFile",
        FileName = "Test.csv"
      };

      file.MappingCollection.Add(new Mapping("Fld1", "FldA"));
      file.MappingCollection.Add(new Mapping("Fld2", "FldB"));
      file.ColumnCollection.AddIfNew(new Column("ID", DataType.Integer) {ColumnOrdinal = 1, Ignore = false});
      file.ColumnCollection.AddIfNew(new Column("Name") {ColumnOrdinal = 2, Part = 2});
      return file;
    }
  }
}