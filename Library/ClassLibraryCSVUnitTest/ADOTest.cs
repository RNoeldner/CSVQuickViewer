namespace CsvTools.Tests
{
#if COMInterface
  [TestClass]
  public class ADOTest
  {
    private string m_ApplicationDirectory = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles";

    private void MakeTableCOM(IFileReaderCOM DataReader)
    {
      int intFieldCount = DataReader.FieldCount;

      // Parse the Types
      for (int intCol = 0; intCol < intFieldCount; intCol++)
      {
        DataReader.IgnoreRead(intCol);
        if (DataReader.GetFieldType(intCol) == typeof(string))
          DataReader.ColumnSize(intCol);
      }

      // Get The data
      while (DataReader.Read())
      {
        for (int intCol = 0; intCol < intFieldCount; intCol++)
        {
          if (!DataReader.IgnoreRead(intCol))
            DataReader.GetValueADO(intCol);
        }
      }
    }

    private void ImportCOM(IFileSetting setting)
    {
      IFileReaderCOM fileReader = null;
      try
      {
        fileReader = (IFileReaderCOM)DataReaderFactory.GetFileReader(setting);
        fileReader.Open(true);
        MakeTableCOM(fileReader);
      }
      finally
      {
        if (fileReader != null)
          fileReader.Dispose();
      }
    }

    [TestMethod]
    public void TestBasicCSVEmptyLine()
    {
      CsvFile setting = new CsvFile
      {
        HasFieldHeader = true
      };

      setting.FileName = System.IO.Path.Combine(m_ApplicationDirectory, "BasicCSVEmptyLine.txt");
      ImportCOM(setting);
    }

    [TestMethod]
    public void TestBasicCSV()
    {
      ImportCOM(Helper.ReaderGetBasicCSV());
    }

    [TestMethod]
    public void TestAllFormats()
    {
      ImportCOM(Helper.ReaderGetAllFormats());
    }
  }

#endif
}