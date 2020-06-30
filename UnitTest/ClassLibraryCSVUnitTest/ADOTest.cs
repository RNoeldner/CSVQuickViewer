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

      setting.FileName = System.IO.UnitTestInitialize.GetTestPath("BasicCSVEmptyLine.txt");
      ImportCOM(setting);
    }

    [TestMethod]
    public void TestBasicCSV()
    {
      ImportCOM(UnitTestHelper.ReaderGetBasicCSV());
    }

    [TestMethod]
    public void TestAllFormats()
    {
      ImportCOM(UnitTestHelper.ReaderGetAllFormats());
    }
  }

#endif
}