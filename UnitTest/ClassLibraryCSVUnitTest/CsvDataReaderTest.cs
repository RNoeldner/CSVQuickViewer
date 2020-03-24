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
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;

namespace CsvTools.Tests
{
  [TestClass]
  public class CsvDataReaderUnitTest
  {
    private readonly CsvFile m_ValidSetting = new CsvFile();

    [TestInitialize]
    public void Init()
    {
      m_ValidSetting.FileName = UnitTestInitialize.GetTestPath("BasicCSV.txt");
      var cf = m_ValidSetting.ColumnCollection.AddIfNew(new Column("ExamDate", DataType.DateTime));

      Assert.IsNotNull(cf);
      cf.ValueFormat.DateFormat = @"dd/MM/yyyy";
      m_ValidSetting.FileFormat.FieldDelimiter = ",";
      m_ValidSetting.FileFormat.CommentLine = "#";
      m_ValidSetting.ColumnCollection.AddIfNew(new Column("Score", DataType.Integer));
      m_ValidSetting.ColumnCollection.AddIfNew(new Column("Proficiency", DataType.Numeric));
      m_ValidSetting.ColumnCollection.AddIfNew(new Column("IsNativeLang", DataType.Boolean));
    }

    [TestMethod]
    public void IssueReader()
    {
      var basIssues = new CsvFile
      {
        TreatLFAsSpace = true,
        TryToSolveMoreColumns = true,
        AllowRowCombining = true,
        FileName = UnitTestInitialize.GetTestPath("BadIssues.csv"),
        FileFormat = {FieldDelimiter = "TAB", FieldQualifier = string.Empty}
      };
      basIssues.ColumnCollection.AddIfNew(new Column("effectiveDate", "yyyy/MM/dd", "-"));
      basIssues.ColumnCollection.AddIfNew(new Column("timestamp",  "yyyy/MM/ddTHH:mm:ss", "-"));
      
      basIssues.ColumnCollection.AddIfNew(new Column("version", DataType.Integer));
      basIssues.ColumnCollection.AddIfNew(new Column("retrainingRequired", DataType.Boolean));

      basIssues.ColumnCollection.AddIfNew(new Column("classroomTraining", DataType.Boolean));
      basIssues.ColumnCollection.AddIfNew(new Column("webLink", DataType.TextToHtml));

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(basIssues, TimeZoneInfo.Local.Id, processDisplay))
      {
        var warningList = new RowErrorCollection(test);
        test.Open();
        warningList.HandleIgnoredColumns(test);
        // need 22 columns
        Assert.AreEqual(22, test.GetSchemaTable().Rows.Count());

        // This should work
        test.Read();
        Assert.AreEqual(0, warningList.CountRows);

        Assert.AreEqual("Eagle_sop020517", test.GetValue(0));
        Assert.AreEqual("de-DE", test.GetValue(2));

        // There are more columns
        test.Read();
        Assert.AreEqual(1, warningList.CountRows);
        Assert.AreEqual("Eagle_SRD-0137699", test.GetValue(0));
        Assert.AreEqual("de-DE", test.GetValue(2));
        Assert.AreEqual(3, test.StartLineNumber);

        test.Read();
        Assert.AreEqual("Eagle_600.364", test.GetValue(0));
        Assert.AreEqual(4, test.StartLineNumber);

        test.Read();
        Assert.AreEqual("Eagle_spt029698", test.GetValue(0));
        Assert.AreEqual(5, test.StartLineNumber);

        test.Read();
        Assert.AreEqual("Eagle_SRD-0137698", test.GetValue(0));
        Assert.AreEqual(2, warningList.CountRows);
        Assert.AreEqual(6, test.StartLineNumber);

        test.Read();
        Assert.AreEqual("Eagle_SRD-0138074", test.GetValue(0));
        Assert.AreEqual(7, test.StartLineNumber);

        test.Read();
        Assert.AreEqual("Eagle_SRD-0125563", test.GetValue(0));
        Assert.AreEqual(8, test.StartLineNumber);

        test.Read();
        Assert.AreEqual("doc_1004040002982", test.GetValue(0));
        Assert.AreEqual(3, warningList.CountRows);
        Assert.AreEqual(9, test.StartLineNumber);

        test.Read();
        Assert.AreEqual("doc_1004040002913", test.GetValue(0));
        Assert.AreEqual(10, test.StartLineNumber);
        Assert.AreEqual(5, warningList.CountRows);

        test.Read();
        Assert.AreEqual("doc_1003001000427", test.GetValue(0));
        Assert.AreEqual(12, test.StartLineNumber);

        test.Read();
        Assert.AreEqual("doc_1008017000611", test.GetValue(0));

        test.Read();
        Assert.AreEqual("doc_1004040000268", test.GetValue(0));

        test.Read();
        Assert.AreEqual("doc_1008011000554", test.GetValue(0));
        test.Read();
        Assert.AreEqual("doc_1003001000936", test.GetValue(0));

        test.Read();
        Assert.AreEqual("doc_1200000124471", test.GetValue(0));

        test.Read();
        Assert.AreEqual("doc_1200000134529", test.GetValue(0));

        test.Read();
        Assert.AreEqual("doc_1004040003504", test.GetValue(0));

        test.Read();
        Assert.AreEqual("doc_1200000016068", test.GetValue(0));

        test.Read();
      }
    }

    [TestMethod]
    public void TestGetDataTypeName()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.AreEqual("String", test.GetDataTypeName(0));
      }
    }

 
    [TestMethod]
    public void TestWarningsRecordNoMapping()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        var dataTable = new DataTable
        {
          TableName = "DataTable",
          Locale = CultureInfo.InvariantCulture
        };

        dataTable.Columns.Add(test.GetName(0), test.GetFieldType(0));

        var recordNumberColumn = dataTable.Columns.Add(BaseFileReader.cRecordNumberFieldName, typeof(int));
        recordNumberColumn.AllowDBNull = true;

        var lineNumberColumn = dataTable.Columns.Add(BaseFileReader.cEndLineNumberFieldName, typeof(int));
        lineNumberColumn.AllowDBNull = true;

        _ = dataTable.NewRow();
        test.Read();
        _ = new Dictionary<int, string>
        {
          {-1, "Test1"},
          {0, "Test2"}
        };

        //test.AssignNumbersAndWarnings(dataRow, null, recordNumberColumn, lineNumberColumn, null, warningsList);
        //Assert.AreEqual("Test1", dataRow.RowError);
        //Assert.AreEqual("Test2", dataRow.GetColumnError(0));
      }
    }

    //[TestMethod]
    //public void GetPart()
    //{
    //  var partToEnd = new Column
    //  {
    //    DataType = DataType.TextPart,
    //    PartSplitter = '-',
    //    Part = 2,
    //    PartToEnd = true
    //  };
    //  var justPart = new Column
    //  {
    //    DataType = DataType.TextPart,
    //    PartSplitter = '-',
    //    Part = 2,
    //    PartToEnd = false
    //  };

    // using (var test = new CsvFileReader(m_ValidSetting, null)) { var inputValue =
    // "17-Hello-World"; var value = test.GetPart(inputValue, partToEnd);
    // Assert.AreEqual("Hello-World", value);

    //    var value2 = test.GetPart(inputValue, justPart);
    //    Assert.AreEqual("Hello", value2);
    //  }
    //}

    [TestMethod]
    public void GetInteger32And64()
    {
      var column = new Column();
      column.ValueFormat.DataType = DataType.Integer;
      column.ValueFormat.GroupSeparator = ",";
      column.ValueFormat.DecimalSeparator = ",";

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        var inputValue = "17";

        var value32 = test.GetInt32Null(inputValue, column);
        Assert.AreEqual(17, value32.Value);

        var value64 = test.GetInt64Null(inputValue, column);
        Assert.AreEqual(17, value64.Value);

        value32 = test.GetInt32Null(null, column);
        Assert.IsFalse(value32.HasValue);

        value64 = test.GetInt64Null(null, column);
        Assert.IsFalse(value64.HasValue);
      }
    }

    [TestMethod]
    public void TestBatchFinishedNotifcation()
    {
      var finished = false;
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.ReadFinished += delegate
        { finished = true; };
        test.Open();

        while (test.Read())
        {
        }
      }

      Assert.IsTrue(finished, "ReadFinished");
    }

    [TestMethod]
    public void TestReadFinishedNotifcation()
    {
      var finished = false;
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.ReadFinished += delegate
        { finished = true; };
        test.Open();
        while (test.Read())
        {
        }
      }

      Assert.IsTrue(finished);
    }

    [TestMethod]
    public void ColumnFormat()
    {
      var target = new CsvFile();
      m_ValidSetting.CopyTo(target);

      Assert.IsNotNull(target.ColumnCollection.Get("Score"));
      var cf = target.ColumnCollection.Get("Score");
      Assert.AreEqual(cf.Name, "Score");

      // Remove the one filed
      target.ColumnCollection.Remove(target.ColumnCollection.Get("Score"));
      Assert.IsNull(target.ColumnCollection.Get("Score"));
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void GetDateTimeTest()
    {
      var csvFile = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("TestFile.txt"),
        CodePageId = 65001,
        FileFormat = { FieldDelimiter = "tab" }
      };

      csvFile.ColumnCollection.AddIfNew(new Column("Title", DataType.DateTime));

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(csvFile, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        test.Read();
        test.GetDateTime(1);
      }
    }

    [TestMethod]
    public void CsvDataReaderImportFileEmptyNullNotExisting()
    {
      var setting = new CsvFile();

      try
      {
        setting.FileName = string.Empty;
        using (new CsvFileReader(setting, TimeZoneInfo.Local.Id, null))
        {
        }

        Assert.Fail("Exception expected");
      }
      catch (ArgumentException)
      {
      }
      catch (FileReaderException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail($"Wrong Exception Type {ex.GetType()}, Empty Filename");
      }

      try
      {
        setting.FileName = @"b;dslkfg;sldfkgjs;ldfkgj;sldfkg.sdfgsfd";
        using (new CsvFileReader(setting, TimeZoneInfo.Local.Id, null))
        {
        }

        Assert.Fail("Exception expected");
      }
      catch (ArgumentException)
      {
      }
      catch (FileNotFoundException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail($"Wrong Exception Type {ex.GetType()}, Invalid Filename");
      }
    }

    [TestMethod]
    public void CsvDataReaderRecordNumberEmptyLines()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("BasicCSVEmptyLine.txt"),
        HasFieldHeader = true
      };

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        var row = 0;
        while (test.Read())
          row++;
        Assert.AreEqual(row, test.RecordNumber);
        Assert.AreEqual(2, row);
      }
    }

    [TestMethod]
    public void CsvDataReaderRecordNumberEmptyLinesSkipEmptyLines()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("BasicCSVEmptyLine.txt"),
        HasFieldHeader = true,
        SkipEmptyLines = false,
        ConsecutiveEmptyRows = 3
      };
      /*
       * ID,LangCode,ExamDate,Score,Proficiency,IsNativeLang
1
2 00001,German,20/01/2010,276,0.94,Y
3 ,,,,,
4 ,,,,,
5 00001,English,22/01/2012,190,,N
6 ,,,,,
7 ,,,,,
8 ,,,,,
9 ,,,,,
10
*/

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        var row = 0;
        while (test.Read())
          row++;
        Assert.AreEqual(row, test.RecordNumber, "Compare with read numbers");
        Assert.AreEqual(7, row, "Read");
      }
    }

    [TestMethod]
    public void CsvDataReaderProperties()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();

        Assert.AreEqual(0, test.Depth, "Depth");
        Assert.AreEqual(6, test.FieldCount, "FieldCount");
        Assert.AreEqual(0U, test.RecordNumber, "RecordNumber");
        Assert.AreEqual(-1, test.RecordsAffected, "RecordsAffected");

        Assert.IsFalse(test.EndOfFile, "EndOfFile");
        Assert.IsFalse(test.IsClosed, "IsClosed");
      }
    }

    [TestMethod]
    public void CsvDataReaderGetName()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.AreEqual("ID", test.GetName(0));
        Assert.AreEqual("LangCodeID", test.GetName(1));
        Assert.AreEqual("ExamDate", test.GetName(2));
        Assert.AreEqual("Score", test.GetName(3));
        Assert.AreEqual("Proficiency", test.GetName(4));
        Assert.AreEqual("IsNativeLang", test.GetName(5));
      }
    }

    [TestMethod]
    public void CsvDataReaderGetOrdinal()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.AreEqual(0, test.GetOrdinal("ID"));
        Assert.AreEqual(1, test.GetOrdinal("LangCodeID"));
        Assert.AreEqual(2, test.GetOrdinal("ExamDate"));
        Assert.AreEqual(3, test.GetOrdinal("Score"));
        Assert.AreEqual(4, test.GetOrdinal("Proficiency"));
        Assert.AreEqual(5, test.GetOrdinal("IsNativeLang"));
        Assert.AreEqual(-1, test.GetOrdinal("Not Existing"));
      }
    }

    [TestMethod]
    public void CsvDataReaderUseIndexer()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.AreEqual("1", test["ID"]);
        Assert.AreEqual("German", test[1]);
        Assert.AreEqual(new DateTime(2010, 01, 20), test["ExamDate"]);
        Assert.IsTrue(test.Read());
        Assert.AreEqual(DBNull.Value, test["Proficiency"]);
      }
    }

    [TestMethod]
    public void CsvDataReaderGetValueNull()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.AreEqual(DBNull.Value, test.GetValue(4));
      }
    }

#if COMInterface
    [TestMethod]
    public void CsvDataReader_GetValueADONull()
    {
      using (CsvFileReader test = new CsvFileReader())
      {
        test.Open(false);
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.AreEqual(DBNull.Value, test.GetValueADO(4));
      }
    }

    [TestMethod]
    [ExpectedException(typeof(NullReferenceException))]
    public void CsvDataReader_GetValueADONoRead()
    {
      using (CsvFileReader test = new CsvFileReader())
      {
        test.Open(false);
        Assert.AreEqual(DBNull.Value, test.GetValueADO(0));
      }
    }

    [TestMethod]
    public void CsvDataReader_GetValueADO()
    {
      using (CsvFileReader test = new CsvFileReader())
      {
        test.Open(false);
        Assert.IsTrue(test.Read());
        Assert.AreEqual("1", test.GetValueADO(0));
        Assert.AreEqual("German", test.GetValueADO(1));
        Assert.AreEqual(new DateTime(2010, 01, 20), test.GetValueADO(2));
      }
    }

#endif

    [TestMethod]
    public void CsvDataReaderGetBoolean()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.GetBoolean(5));
        Assert.IsTrue(test.Read());
        Assert.IsFalse(test.GetBoolean(5));
      }
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void CsvDataReaderGetBooleanError()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        test.GetBoolean(1);
      }
    }

    [TestMethod]
    public void CsvDataReaderGetDateTime()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        // 20/01/2010
        Assert.AreEqual(new DateTime(2010, 01, 20), test.GetDateTime(2));
      }
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void CsvDataReaderGetDateTimeError()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        test.GetDateTime(1);
      }
    }

    [TestMethod]
    public void CsvDataReaderGetInt32()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.AreEqual(276, test.GetInt32(3));
      }
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void CsvDataReaderGetInt32Error()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        test.GetInt32(1);
      }
    }

    [TestMethod]
    public void CsvDataReaderGetDecimal()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.AreEqual(0.94m, test.GetDecimal(4));
      }
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void CsvDataReaderGetDecimalError()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        test.GetDecimal(1);
      }
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void CsvDataReaderGetInt32Null()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        test.GetInt32(4);
      }
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void CsvDataReaderGetBytes()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        test.GetBytes(0, 0, null, 0, 0);
      }
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void CsvDataReaderGetData()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        test.GetData(0);
      }
    }

    [TestMethod]
    public void CsvDataReaderGetFloat()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.AreEqual(Convert.ToSingle(0.94), test.GetFloat(4));
      }
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void CsvDataReaderGetFloatError()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        test.GetFloat(1);
      }
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void CsvDataReaderGetGuid()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        test.GetGuid(1);
      }
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void CsvDataReaderGetDateTimeNull()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        test.GetDateTime(2);
      }
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void CsvDataReaderGetDateTimeWrongType()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        test.GetDateTime(1);
      }
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void CsvDataReaderGetDecimalFormatException()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        test.GetDecimal(4);
      }
    }

    [TestMethod]
    public void CsvDataReaderGetByte()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.AreEqual(1, test.GetByte(0));
      }
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void CsvDataReaderGetByteFrormat()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.AreEqual(1, test.GetByte(1));
      }
    }

    [TestMethod]
    public void CsvDataReaderGetDouble()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.AreEqual(1, test.GetDouble(0));
      }
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void CsvDataReaderGetDoubleFrormat()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.AreEqual(1, test.GetDouble(1));
      }
    }

    [TestMethod]
    public void CsvDataReaderGetInt16()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.AreEqual(1, test.GetInt16(0));
      }
    }

    [TestMethod]
    public void CsvDataReaderInitWarnings()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("BasicCSV.txt"),
        HasFieldHeader = false,
        SkipRows = 1
      };
      setting.FileFormat.FieldQualifier = "XX";
      setting.FileFormat.FieldDelimiter = ",,";

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        var warningList = new RowErrorCollection(test);
        test.Open();
        warningList.HandleIgnoredColumns(test);

        Assert.IsTrue(warningList.Display.Contains("Only the first character of 'XX' is be used for quoting."));
        Assert.IsTrue(warningList.Display.Contains("Only the first character of ',,' is used as delimiter."));
      }
    }

    [TestMethod]
    public void CsvDataReaderInitErrorFieldDelimiterCr()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("BasicCSV.txt"),
        HasFieldHeader = false,
        SkipRows = 1,
        FileFormat = {FieldDelimiter = "\r"}
      };
      var exception = false;
      try
      {
        using (var processDisplay = new DummyProcessDisplay())
        using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
        {
          test.Open();
        }
      }
      catch (ArgumentException)
      {
        exception = true;
      }
      catch (FileReaderException)
      {
        exception = true;
      }
      catch (Exception)
      {
        Assert.Fail("Wrong Exception Type");
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public void CsvDataReaderInitErrorFieldQualifierCr()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("BasicCSV.txt"),
        HasFieldHeader = false,
        SkipRows = 1,
        FileFormat = {FieldQualifier = "Carriage return"}
      };
      var exception = false;
      try
      {
        using (var processDisplay = new DummyProcessDisplay())
        using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
        {
          test.Open();
        }
      }
      catch (ArgumentException)
      {
        exception = true;
      }
      catch (FileReaderException)
      {
        exception = true;
      }
      catch (Exception)
      {
        Assert.Fail("Wrong Exception Type");
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public void CsvDataReaderInitErrorFieldQualifierLF()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("BasicCSV.txt"),
        HasFieldHeader = false,
        SkipRows = 1,
        FileFormat = {FieldQualifier = "Line feed"}
      };
      var exception = false;
      try
      {
        using (var processDisplay = new DummyProcessDisplay())
        using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
        {
          test.Open();
        }
      }
      catch (ArgumentException)
      {
        exception = true;
      }
      catch (FileReaderException)
      {
        exception = true;
      }
      catch (Exception)
      {
        Assert.Fail("Wrong Exception Type");
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public void CsvDataReaderGuessCodePage()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("BasicCSV.txt"),
        HasFieldHeader = true,
        CodePageId = 0
      };
      setting.FileFormat.FieldDelimiter = ",";
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
      }

      Assert.AreEqual(1200, setting.CurrentEncoding.WindowsCodePage); // UTF-16 little endian
    }

    [TestMethod]
    public void CsvDataReaderInitErrorFieldDelimiterLF()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("BasicCSV.txt"),
        HasFieldHeader = false,
        SkipRows = 1,
        FileFormat = {FieldDelimiter = "\n"}
      };
      var exception = false;
      try
      {
        using (var processDisplay = new DummyProcessDisplay())
        using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
        {
          test.Open();
        }
      }
      catch (ArgumentException)
      {
        exception = true;
      }
      catch (FileReaderException)
      {
        exception = true;
      }
      catch (Exception)
      {
        Assert.Fail("Wrong Exception Type");
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public void CsvDataReaderInitErrorFieldDelimiterSpace()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("BasicCSV.txt"),
        HasFieldHeader = false,
        SkipRows = 1,
        FileFormat = {FieldDelimiter = " "}
      };
      var exception = false;
      try
      {
        using (var processDisplay = new DummyProcessDisplay())
        using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
        {
          test.Open();
        }
      }
      catch (ArgumentException)
      {
        exception = true;
      }
      catch (FileReaderException)
      {
        exception = true;
      }
      catch (Exception)
      {
        Assert.Fail("Wrong Exception Type");
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public void CsvDataReaderInitErrorFieldQualifierIsFieldDelimiter()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("BasicCSV.txt"),
        HasFieldHeader = false,
        SkipRows = 1,
        FileFormat = {FieldQualifier = "\""}
      };
      setting.FileFormat.FieldDelimiter = setting.FileFormat.FieldQualifier;
      var exception = false;
      try
      {
        using (var processDisplay = new DummyProcessDisplay())
        using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
        {
          test.Open();
        }
      }
      catch (ArgumentException)
      {
        exception = true;
      }
      catch (FileReaderException)
      {
        exception = true;
      }
      catch (Exception)
      {
        Assert.Fail("Wrong Exception Type");
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public void CsvDataReaderGetInt16Format()
    {
      var exception = false;
      try
      {
        using (var processDisplay = new DummyProcessDisplay())
        using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
        {
          test.Open();
          Assert.IsTrue(test.Read());
          Assert.AreEqual(1, test.GetInt16(1));
        }
      }
      catch (FormatException)
      {
        exception = true;
      }
      catch (Exception)
      {
        Assert.Fail("Wrong Exception Type");
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public void CsvDataReaderGetInt64()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.AreEqual(1, test.GetInt64(0));
      }
    }

    [TestMethod]
    public void CsvDataReaderGetInt64Error()
    {
      var exception = false;
      try
      {
        using (var processDisplay = new DummyProcessDisplay())
        using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
        {
          test.Open();
          Assert.IsTrue(test.Read());
          Assert.AreEqual(1, test.GetInt64(1));
        }
      }
      catch (FormatException)
      {
        exception = true;
      }
      catch (Exception)
      {
        Assert.Fail("Wrong Exception Type");
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public void CsvDataReaderGetChar()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.AreEqual('G', test.GetChar(1));
      }
    }

    [TestMethod]
    public void CsvDataReaderGetStringColumnNotExisting()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        var exception = false;
        test.Open();
        test.Read();
        try
        {
          test.GetString(666);
        }
        catch (IndexOutOfRangeException)
        {
          exception = true;
        }
        catch (InvalidOperationException)
        {
          exception = true;
        }
        catch (Exception)
        {
          Assert.Fail("Wrong Exception Type");
        }

        Assert.IsTrue(exception, "No Exception thrown");
      }
    }

    [TestMethod]
    public void CsvDataReaderGetString()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.AreEqual("German", test.GetString(1));
        Assert.AreEqual("German", test.GetValue(1));
      }
    }

    public void DataReaderResetPositionToFirstDataRow()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.ResetPositionToFirstDataRow();
      }
    }

    [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
    [TestMethod]
    public void CsvDataReaderIsDBNull()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.IsFalse(test.IsDBNull(4));
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.IsDBNull(4));
        test.Close();
      }
    }

    [TestMethod]
    public void CsvDataReaderTreatNullTextTrue()
    {
      //m_ValidSetting.TreatTextNullAsNull = true;
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());

        Assert.AreEqual(DBNull.Value, test["LangCodeID"]);
      }
    }

    [TestMethod]
    public void CsvDataReaderTreatNullTextFalse()
    {
      m_ValidSetting.TreatTextAsNull = null;
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());

        Assert.AreEqual("NULL", test["LangCodeID"]);
      }
    }

    [TestMethod]
    public void CsvDataReaderGetValues()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        var values = new object[test.FieldCount];
        Assert.AreEqual(6, test.GetValues(values));
      }
    }

    [TestMethod]
    public void CsvDataReaderGetChars()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        char[] buffer = { '0', '0', '0', '0' };
        test.GetChars(1, 0, buffer, 0, 4);
        Assert.AreEqual('G', buffer[0], "G");
        Assert.AreEqual('e', buffer[1], "E");
        Assert.AreEqual('r', buffer[2], "R");
        Assert.AreEqual('m', buffer[3], "M");
      }
    }

    [TestMethod]
    public void CsvDataReaderGetSchemaTable()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        var dt = test.GetSchemaTable();
        Assert.IsInstanceOfType(dt, typeof(DataTable));
        Assert.AreEqual(6, dt.Rows.Count);
      }
    }

    [TestMethod]
    public void CsvDataReaderReadAfterEnd()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsFalse(test.Read());
        Assert.IsFalse(test.Read());
      }
    }

    [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
    [TestMethod]
    public void CsvDataReaderReadAfterClose()
    {
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(m_ValidSetting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        test.Close();
        Assert.IsFalse(test.Read());
      }
    }

    //[TestMethod]
    //public void CsvDataReader_OpenDetails()
    //{
    //  using (CsvFileReader test = new CsvFileReader())
    //  {
    //    test.Open(true);
    //    Assert.AreEqual(1, m_ValidSetting.Column[0].Size);
    //    Assert.AreEqual(7, m_ValidSetting.Column[1].Size, "LangCodeID");
    //    Assert.AreEqual(10, m_ValidSetting.Column[2].Size, "ExamDate");
    //    Assert.AreEqual(3, m_ValidSetting.Column[3].Size, "Score");
    //    Assert.AreEqual(5, m_ValidSetting.Column[4].Size, "Proficiency");
    //    Assert.AreEqual(1, m_ValidSetting.Column[5].Size, "IsNativeLang");
    //  }
    //}

    [TestMethod]
    public void CsvDataReaderOpenDetailSkipRows()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("BasicCSV.txt"),
        HasFieldHeader = false,
        SkipRows = 1
      };
      setting.FileFormat.FieldDelimiter = ",";
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        test.Read();
        Assert.AreEqual(1, test.GetColumn(0).Size, "ID: 1");
        Assert.AreEqual(6, test.GetColumn(1).Size, "LangCodeID: German");
        Assert.AreEqual(10, test.GetColumn(2).Size, "ExamDate");
        Assert.AreEqual(3, test.GetColumn(3).Size, "Score");
        Assert.AreEqual(4, test.GetColumn(4).Size, "Proficiency: 0.94");
        Assert.AreEqual(1, test.GetColumn(5).Size, "IsNativeLang ");
      }
    }

    [TestMethod]
    public void CsvDataReaderNoHeader()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("BasicCSV.txt"),
        HasFieldHeader = false,
        SkipRows = 1
      };
      setting.FileFormat.FieldDelimiter = ",";
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.AreEqual("Column1", test.GetName(0));
        Assert.AreEqual("Column2", test.GetName(1));
        Assert.AreEqual("Column3", test.GetName(2));
        Assert.AreEqual("Column4", test.GetName(3));
        Assert.AreEqual("Column5", test.GetName(4));
        Assert.AreEqual("Column6", test.GetName(5));
      }
    }
  }
}