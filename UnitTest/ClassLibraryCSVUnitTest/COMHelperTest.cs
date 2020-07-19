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

#if COMInterface
namespace CsvTools.Tests
{
  [TestClass]
  public class COMHelperTest
  {
    private string m_ApplicationDirectory = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles";
    private COMHelper m_ComHelper = new COMHelper();

    [TestMethod]
    public void GetCommonCodePages()
    {
      var result = m_ComHelper.GetCommonCodePages();
      Assert.IsTrue(result.Contains(1252), "1252");
      Assert.IsTrue(result.Contains(65001), "65001");
    }

    [TestMethod]
    public void GetDataReaderCsv()
    {
      var setting = m_ComHelper.GetCsvFileSetting();
      Assert.IsTrue(setting is ICsvFile);
      setting.FileName = System.IO.UnitTestInitialize.GetTestPath("BasicCSV.txt");
      setting.ShowProgress = false;
      var result = m_ComHelper.GetDataReader(setting);
      Assert.IsTrue(result is IFileReaderCOM);
    }

    [TestMethod]
    public void GetDataReaderExcel()
    {
      var setting = m_ComHelper.GetExcelFileSetting();
      Assert.IsTrue(setting is IExcelFile);
      setting.FileName = System.IO.UnitTestInitialize.GetTestPath("Small.xlsx");
      setting.SheetName = "Sheet1";
      setting.ShowProgress = false;
      var result = m_ComHelper.GetDataReader(setting);
      Assert.IsTrue(result is IFileReaderCOM);
    }

    [TestMethod]
    [Ignore]
    public void ShowUI()
    {
      var setting = m_ComHelper.GetExcelFileSetting();
      Assert.IsTrue(setting is IExcelFile);
      setting.FileName = System.IO.UnitTestInitialize.GetTestPath("Small.xlsx");
      setting.SheetName = "Sheet1";
      m_ComHelper.ShowUI(setting, true, true, false);
    }

    [TestMethod]
    public void WriteFile()
    {
      IToolSetting parent = new MockToolSetting();
      CsvFile readFile = new CsvFile();
      CsvFile writeFile = new CsvFile();
      writeFile.ShowProgress = false;
      readFile.ShowProgress = false;
      readFile.ID = "Read";
      readFile.FileName = System.IO.UnitTestInitialize.GetTestPath("BasicCSV.txt");
      readFile.ColumnFormatAdd(new ColumnFormat
      {
        Name = "ExamDate",
        DataType = DataType.DateTime,
        Convert = true
      });
      readFile.GetColumnFormat("ExamDate").ValueFormatMutable.DateFormat = "dd/MM/yyyy";
      readFile.FileFormat.CommentLine = "#";
      readFile.ColumnFormatAdd(new ColumnFormat
      {
        Name = "Score",
        DataType = DataType.Integer,
        Convert = true
      });

      readFile.ColumnFormatAdd(new ColumnFormat
      {
        Name = "Proficiency",
        DataType = DataType.Numeric,
        Convert = true
      });

      readFile.ColumnFormatAdd(new ColumnFormat
      {
        Name = "IsNativeLang",
        DataType = DataType.Boolean,
        IgnoreRead = true,
        Convert = true
      });

      parent.Input.Add(readFile);

      writeFile = new CsvFile();
      writeFile.ID = "Write";
      writeFile.ConnectionString = "Read";
      writeFile.Parent = parent;
      writeFile.ColumnFormatAdd(new ColumnFormat
      {
        Name = "ExamDate",
        DataType = DataType.DateTime,
        TimePart = "ExamTime"
      });
      writeFile.GetColumnFormat("ExamDate").ValueFormatMutable.DateFormat = "MM/dd/yyyy";
      writeFile.FileName = System.IO.UnitTestInitialize.GetTestPath("BasicCSVOut.txt");

      m_ComHelper.WriteFile(writeFile, false);
    }

    [TestMethod]
    public void GetEncodingName()
    {
      Assert.AreEqual(m_ComHelper[1252], EncodingHelper.GetEncodingName(1252, false, false));
    }
  }
}

#endif