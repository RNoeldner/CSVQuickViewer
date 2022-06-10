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
    private static readonly string m_FileName = UnitTestStatic.GetTestPath("Test.csv") + CsvFile.cCsvSettingExtension;

    [TestMethod]
    [Timeout(2000)]
    public void DeleteWithBackupTest()
    {
      var file = GetCsvFile();
      FileSystemUtils.DeleteWithBackup(m_FileName, false);
      Assert.IsFalse(FileSystemUtils.FileExists(m_FileName));
      Assert.IsFalse(FileSystemUtils.FileExists(m_FileName + ".bak"));

      SerializedFilesLib.SaveSettingFile(file, () => false);
      Assert.IsTrue(FileSystemUtils.FileExists(m_FileName));
      FileSystemUtils.DeleteWithBackup(m_FileName, false);
      Assert.IsFalse(FileSystemUtils.FileExists(m_FileName));
      Assert.IsTrue(FileSystemUtils.FileExists(m_FileName + ".bak"));
    }

    [TestInitialize]
    public void Init()
    {
      FileSystemUtils.FileDelete(m_FileName);
      FileSystemUtils.FileDelete(m_FileName + ".bak");
    }


    [TestMethod]
    public void SaveAndLoadCsvFileTest()
    {
      var file = GetCsvFile();
      Assert.IsFalse(FileSystemUtils.FileExists(m_FileName));
      SerializedFilesLib.SaveSettingFile(file, () => true);
      Assert.IsTrue(FileSystemUtils.FileExists(m_FileName));
      var test = SerializedFilesLib.LoadCsvFile(m_FileName);

      Assert.AreEqual(file.ColumnCollection.Count, test.ColumnCollection.Count, "ColumnCollection.Count");

      Assert.AreEqual(file.MappingCollection.Count, test.MappingCollection.Count, "MappingCollection.Count");
      Assert.IsTrue(file.MappingCollection.CollectionEqual(test.MappingCollection), "MappingCollection");

      Assert.AreNotSame(file, test);
      Assert.IsInstanceOfType(test, typeof(CsvFile));

      // FileName and ID are not serialized
      test.FileName = file.FileName;
      test.ID = file.ID;
      file.CheckAllPropertiesEqual(test, new []{"LastChange"});
      // Test Properties that are not tested

      Assert.AreEqual(file.MappingCollection.Count, test.MappingCollection.Count, "FieldMapping");
      Assert.AreEqual(TrimmingOptionEnum.Unquoted, test.TrimmingOption, "TrimmingOption");
    }

    [TestMethod]
    public void SaveCsvFileTest()
    {
      var file = GetCsvFile();
      file.ByteOrderMark = false;
      Assert.IsFalse(FileSystemUtils.FileExists(m_FileName));
      var asked = false;
      SerializedFilesLib.SaveSettingFile(file, () => true);
      file.ByteOrderMark = true;

      Assert.IsTrue(FileSystemUtils.FileExists(m_FileName));
      SerializedFilesLib.SaveSettingFile(file, () =>
      {
        asked = true;
        return true;
      });
      Assert.IsTrue(asked);
    }

    private CsvFile GetCsvFile()
    {
      var file = new CsvFile(UnitTestStatic.GetTestPath("Test.csv")) { ID = "TestFile", CommentLine = "##" };

      file.MappingCollection.Add(new Mapping("Fld1", "FldA"));
      file.MappingCollection.Add(new Mapping("Fld2", "FldB"));
      file.ColumnCollection.Add(new Column("ID", DataTypeEnum.Integer) { ColumnOrdinal = 1, Ignore = false });
      file.ColumnCollection.Add(new Column("Name", DataTypeEnum.TextPart) { ColumnOrdinal = 2, Part = 2 });
      return file;
    }
  }
}