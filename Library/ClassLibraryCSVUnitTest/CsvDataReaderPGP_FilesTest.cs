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
  public class CsvDataReaderPGPFilesTest
  {
    [TestMethod]
    public void ReadGZip()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = true,
      };
      setting.FileName = "BasicCSV.txt.gz";
      setting.FileFormat.AlternateQuoting = true;
      setting.ColumnCollection.AddIfNew(new Column
      {
        Name = "ExamDate",
        DataType = DataType.DateTime,
        DateFormat = @"dd/MM/yyyy"
      });
      setting.ColumnCollection.AddIfNew(new Column
      {
        Name = "ID",
        DataType = DataType.Integer
      });
      setting.ColumnCollection.AddIfNew(new Column
      {
        Name = "IsNativeLang",
        DataType = DataType.Boolean
      });
      using (var processDisplay = new DummyProcessDisplay()) using (var test = new CsvFileReader(setting, processDisplay))
      {
        test.Open();
        int row = 0;
        while (test.Read())
          row++;
        Assert.AreEqual(row, test.RecordNumber);
        Assert.AreEqual(7, row);
      }
    }

    [TestMethod]
    public void ReadPGP()

    {
      var setting = new CsvFile
      {
        HasFieldHeader = true
      };
      setting.FileFormat.AlternateQuoting = true;
      PGPKeyStorageTestHelper.SetApplicationSetting();
      setting.FileName = "BasicCSV.pgp";
      setting.ColumnCollection.AddIfNew(new Column
      {
        Name = "ExamDate",
        DataType = DataType.DateTime,
        DateFormat = @"dd/MM/yyyy"
      });
      setting.ColumnCollection.AddIfNew(new Column
      {
        Name = "ID",
        DataType = DataType.Integer
      });
      setting.ColumnCollection.AddIfNew(new Column
      {
        Name = "IsNativeLang",
        DataType = DataType.Boolean
      });
      using (var processDisplay = new DummyProcessDisplay()) using (var test = new CsvFileReader(setting, processDisplay))
      {
        test.Open();
        int row = 0;
        while (test.Read())
          row++;
        Assert.AreEqual(row, test.RecordNumber);
        Assert.AreEqual(7, row);
      }
    }

    [TestMethod]
    public void ReadGPG()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = true
      };
      setting.FileFormat.AlternateQuoting = true;
      PGPKeyStorageTestHelper.SetApplicationSetting();
      setting.FileName = "BasicCSV.pgp";
      setting.ColumnCollection.AddIfNew(new Column
      {
        Name = "ExamDate",
        DataType = DataType.DateTime,
        DateFormat = @"dd/MM/yyyy"
      });
      setting.ColumnCollection.AddIfNew(new Column
      {
        Name = "ID",
        DataType = DataType.Integer
      });
      setting.ColumnCollection.AddIfNew(new Column
      {
        Name = "IsNativeLang",
        DataType = DataType.Boolean
      });
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, processDisplay))
      {
        test.Open();
        int row = 0;
        while (test.Read())
          row++;
        Assert.AreEqual(row, test.RecordNumber);
        Assert.AreEqual(7, row);
      }
    }
  }
}