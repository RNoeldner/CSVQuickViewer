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

      SerializedFilesLib.SaveCsvFile(fileName, file);
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
    public void LoadCsvFileOldTest()
    {
      Assert.Inconclusive();
    }

    [TestMethod]
    public void SaveCsvFileTest()
    {
      var file = GetCsvFile();
      Assert.IsFalse(FileSystemUtils.FileExists(fileName));
      SerializedFilesLib.SaveCsvFile(fileName, file);
      Assert.IsTrue(FileSystemUtils.FileExists(fileName));
      var test = SerializedFilesLib.LoadCsvFile(fileName);

      Assert.AreNotSame(file, test);
      Assert.IsInstanceOfType(test, typeof(CsvFile));

      file.AllPropertiesEqual(test);
      // Test Properties that are not tested

      Assert.AreEqual(file.Mapping.Count, test.Mapping.Count, "FieldMapping");
      Assert.AreEqual(TrimmingOption.Unquoted, test.TrimmingOption, "TrimmingOption");
      Assert.IsTrue(file.Mapping.CollectionEqual(test.Mapping), "Mapping");
      Assert.IsTrue(file.FileFormat.Equals(test.FileFormat), "FileFormat");
    }

    private CsvFile GetCsvFile()
    {
      var file = new CsvFile();
      file.ID = "TestFile";
      file.FileName = "Test.csv";

      file.Mapping.Add(new Mapping { FileColumn = "Fld1", TemplateField = "FldA" });
      file.Mapping.Add(new Mapping { FileColumn = "Fld2", TemplateField = "FldB" });
      file.Column.Add(new Column { ColumnOrdinal = 1, DataType = DataType.Integer, Ignore = false, Name = "ID" });
      file.Column.Add(new Column { ColumnOrdinal = 2, Name = "Name", Part = 2 });
      return file;
    }
  }
}