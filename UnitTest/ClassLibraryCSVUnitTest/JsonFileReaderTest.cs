﻿/*
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
using Pri.LongPath;
using System;

namespace CsvTools.Tests
{
  [TestClass]
  public class JsonFileReaderTest
  {
    [TestMethod]
    public void OpenLog()
    {
      var setting = new CsvFile(UnitTestInitialize.GetTestPath("LogFile.json"))
      {
        JsonFormat = true
      };
      using (var dpd = new DummyProcessDisplay())
      using (var jfr = new JsonFileReader(setting, TimeZoneInfo.Local.Id, dpd))
      {
        jfr.Open();
        jfr.Read();
        Assert.AreEqual("level", jfr.GetColumn(1).Name);
        Assert.AreEqual("Error", jfr.GetValue(1));

        jfr.Read();
        Assert.AreEqual("Reading EdgeAPI vw_rpt_transcript", jfr.GetValue(2));

        jfr.Read();
        Assert.AreEqual("System.Data.DataException", jfr.GetValue(4));
      }
    }

    [TestMethod]
    public void NotSupported()
    {
      var setting = new CsvFile(UnitTestInitialize.GetTestPath("Emp.json"))
      {
        JsonFormat = true
      };

      using (var dpd = new DummyProcessDisplay())
      using (var jfr = new JsonFileReader(setting, TimeZoneInfo.Local.Id, dpd))
      {
        jfr.Open();
        jfr.Read();

        try
        {
          jfr.GetByte(1);
          Assert.Fail("GetByte - No Exception");
        }
        catch (NotImplementedException)
        {
        }
        catch (Exception ex)
        {
          Assert.Fail($"GetByte - Wrong type of exception  {ex.GetType().Name}");
        }

        try
        {
          var buffer = new byte[200];
          jfr.GetBytes(1, 0, buffer, 0, 100);
          Assert.Fail("GetBytes - No Exception");
        }
        catch (NotImplementedException)
        {
        }
        catch (Exception ex)
        {
          Assert.Fail($"GetBytes - Wrong type of exception  {ex.GetType().Name}");
        }

        try
        {
          var buffer = new byte[200];
          jfr.GetBytes(1, 0, buffer, 0, 100);
          Assert.Fail("No Exception");
        }
        catch (NotImplementedException)
        {
        }
        catch (Exception ex)
        {
          Assert.Fail($"Wrong type of exception  {ex.GetType().Name}");
        }

        try
        {
          jfr.GetChar(1);
          Assert.Fail("GetChar - No Exception");
        }
        catch (NotImplementedException)
        {
        }
        catch (Exception ex)
        {
          Assert.Fail($"GetChar - Wrong type of exception  {ex.GetType().Name}");
        }

        try
        {
          var buffer = new char[200];
          jfr.GetChars(1, 0, buffer, 0, 100);
          Assert.Fail("GetChars - No Exception");
        }
        catch (NotImplementedException)
        {
        }
        catch (Exception ex)
        {
          Assert.Fail($"GetChars - Wrong type of exception  {ex.GetType().Name}");
        }

        try
        {
          var buffer = new byte[200];
          jfr.GetBytes(1, 0, buffer, 0, 100);
          Assert.Fail("No Exception");
        }
        catch (NotImplementedException)
        {
        }
        catch (Exception ex)
        {
          Assert.Fail($"Wrong type of exception  {ex.GetType().Name}");
        }

        try
        {
          jfr.GetData(2);
          Assert.Fail("GetData - No Exception");
        }
        catch (NotImplementedException)
        {
        }
        catch (Exception ex)
        {
          Assert.Fail($"GetData - Wrong type of exception  {ex.GetType().Name}");
        }
      }
    }

    [TestMethod]
    public void ReadJSonEmp()
    {
      var setting = new CsvFile(UnitTestInitialize.GetTestPath("Emp.json"))
      {
        JsonFormat = true
      };

      using (var dpd = new DummyProcessDisplay())
      using (var jfr = new JsonFileReader(setting, TimeZoneInfo.Local.Id, dpd))
      {
        jfr.Open();
        Assert.AreEqual(110, jfr.FieldCount);
        jfr.Read();
        Assert.AreEqual("String", jfr.GetDataTypeName(0));
        Assert.AreEqual("43357099", jfr.GetValue(0));
        Assert.AreEqual("T454898", jfr.GetValue(1));
        Assert.AreEqual(new DateTime(2012, 02, 06), jfr.GetValue(jfr.GetOrdinal("LastHireDate")));
        Assert.IsTrue(jfr.IsDBNull(jfr.GetOrdinal("ASSIGN_2ND_DEPT")));
        Assert.AreEqual((short)0, jfr.GetInt16(jfr.GetOrdinal("Approvals")));
        Assert.AreEqual(0, jfr.GetInt32(jfr.GetOrdinal("Approvals")));
        Assert.AreEqual(0L, jfr.GetInt64(jfr.GetOrdinal("Approvals")));
        Assert.AreEqual(0f, jfr.GetFloat(jfr.GetOrdinal("Approvals")));
        Assert.AreEqual(0, jfr.GetDecimal(jfr.GetOrdinal("Approvals")));
        Assert.AreEqual(0, jfr.GetDouble(jfr.GetOrdinal("Approvals")));
        Assert.IsTrue(jfr.GetBoolean(2));
        Assert.AreEqual(1.000, jfr.GetValue(jfr.GetOrdinal("FTE")));
        jfr.Read();
        Assert.AreEqual("43357196", jfr.GetValue(0));
        jfr.Read();
        Assert.AreEqual("43357477", jfr.GetValue(0));
        while (jfr.Read())
        {
        }
        Assert.AreEqual(2782, jfr.RecordNumber);
      }
    }

    [TestMethod]
    public void ReadJSon1()
    {
      var setting = new CsvFile(UnitTestInitialize.GetTestPath("Jason1.json"))
      {
        JsonFormat = true
      };

      using (var dpd = new DummyProcessDisplay())
      using (var jfr = new JsonFileReader(setting, TimeZoneInfo.Local.Id, dpd))
      {
        jfr.Open();
        Assert.AreEqual(20, jfr.FieldCount);
        jfr.Read();
        Assert.AreEqual("5048fde7c4aa917cbd4d8e13", jfr.GetValue(0));
        jfr.Read();
        Assert.AreEqual("5048fde7c4aa917cbd4d22333", jfr.GetValue(0));
        Assert.AreEqual(2, jfr.RecordNumber);
      }
    }

    [TestMethod]
    public void ReadJSon2()
    {
      var setting = new CsvFile(UnitTestInitialize.GetTestPath("Jason2.json"))
      {
        JsonFormat = true
      };
      using (var dpd = new DummyProcessDisplay())
      using (var jfr = new JsonFileReader(setting, TimeZoneInfo.Local.Id, dpd))
      {
        jfr.Open();
        Assert.AreEqual(7, jfr.FieldCount);
        jfr.Read();
        jfr.Read();
        Assert.AreEqual("Loading defaults C:\\Users\\rnoldner\\AppData\\Roaming\\CSVFileValidator\\Setting.xml", jfr.GetValue(6));
        while (jfr.Read())
        {
        }
        Assert.AreEqual(29, jfr.RecordNumber);
      }
    }

    [TestMethod]
    public void ReadJSon3()
    {
      var setting = new CsvFile(UnitTestInitialize.GetTestPath("Jason3.json"))
      {
        JsonFormat = true
      };

      using (var dpd = new DummyProcessDisplay())
      using (var jfr = new JsonFileReader(setting, TimeZoneInfo.Local.Id, dpd))
      {
        jfr.Open();
        Assert.AreEqual(2, jfr.FieldCount);
        jfr.Read();
        Assert.AreEqual(28L, jfr.GetValue(0));
        jfr.Read();
        Assert.AreEqual(56L, jfr.GetValue(0));
        while (jfr.Read())
        {
        }
        Assert.AreEqual(3, jfr.RecordNumber);
      }
    }
  }
}