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
using System.Threading.Tasks;

// ReSharper disable UseAwaitUsing

namespace CsvTools.Tests
{
  [TestClass]
  public class JsonFileReaderTest
  {
    private static readonly TimeZoneChangeDelegate m_TimeZoneAdjust = StandardTimeZoneAdjust.ChangeTimeZone;

    [TestMethod]
    public async Task OpenJsonArrayLevel2()
    {
      using var jfr = new JsonFileReader(UnitTestStatic.GetTestPath("Array.json"), null, 0, false,"null", false, m_TimeZoneAdjust, TimeZoneInfo.Local.Id, false, false);
      await jfr.OpenAsync(UnitTestStatic.Token);
      
      await jfr.ReadAsync(UnitTestStatic.Token);
      jfr.Close();
    }

    [TestMethod]
    public async Task OpenJsonArray()
    {
      
      
      using var jfr = new JsonFileReader(UnitTestStatic.GetTestPath("Larger.json.gz"));
      await jfr.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual("object_id", jfr.GetName(0));
      Assert.AreEqual("_last_touched_dt_utc", jfr.GetName(1));

      await jfr.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("ef21069c-3d93-4e07-878d-00e820727f65", jfr.GetString(0));
      Assert.IsTrue((new DateTime(2020, 04, 03, 20, 45, 29, DateTimeKind.Local) - (DateTime) jfr.GetValue(1))
        .TotalSeconds < 1f);
      jfr.Close();
    }

    [TestMethod]
    [Timeout(2000)]
    public async Task OpenLogAsStream()
    {
      using var stream = FileSystemUtils.OpenRead(UnitTestStatic.GetTestPath("LogFile.json"));
      using var jfr = new JsonFileReader(stream, Array.Empty<Column>(), 5000, false, "<nil>", true,
        m_TimeZoneAdjust, TimeZoneInfo.Local.Id, false, false);
      await jfr.OpenAsync(UnitTestStatic.Token);
      await jfr.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("level", jfr.GetColumn(1).Name);
      Assert.AreEqual("Error", jfr.GetValue(1));

      await jfr.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("Reading EdgeAPI vw_rpt_transcript", jfr.GetValue(2));

      await jfr.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("System.Data.DataException", jfr.GetValue(4));
    }

    [TestMethod]
    [Timeout(2000)]
    public async Task ReadJSonEmp_VariousTypedData()
    {
      var setting = new CsvFileDummy();
      
      using var jfr = new JsonFileReader(UnitTestStatic.GetTestPath("Emp.json"), setting.ColumnCollection, setting.RecordLimit,
        setting.TrimmingOption == TrimmingOptionEnum.All,
        setting.TreatTextAsNull, setting.TreatNBSPAsSpace, m_TimeZoneAdjust, TimeZoneInfo.Local.Id, false, false);
      await jfr.OpenAsync(UnitTestStatic.Token);
      await jfr.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual(0, jfr.GetByte(21));
      Assert.AreEqual('T', jfr.GetChar(1));
      var buffer = new char[200];
      jfr.GetChars(3, 0, buffer, 0, 100);
      Assert.AreEqual('M', buffer[0]);
      Assert.AreEqual('A', buffer[1]);
    }

    [TestMethod]
    [Timeout(2000)]
    public async Task GetBytes()
    {
      using var jfr = new JsonFileReader(UnitTestStatic.GetTestPath("Emp.json"));
      await jfr.OpenAsync(UnitTestStatic.Token);
      await jfr.ReadAsync(UnitTestStatic.Token);

      var buffer = new byte[200];
      Assert.AreEqual(7, jfr.GetBytes(1, 0, buffer, 0, 100));
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

    [TestMethod]
    [Timeout(2000)]
    public async Task ReadJSonTypes()
    {
      using var jfr = new JsonFileReader(UnitTestStatic.GetTestPath("Emp.json"), null, 0, false, string.Empty, false,
        StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, false, false);
      await jfr.OpenAsync(UnitTestStatic.Token);
      try
      {
        _ = jfr.GetFloat(0);
        Assert.Fail("No Exception thrown");
      }
      catch (FormatException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail($"Wrong type of exception  {ex.GetType().Name}");
      }

      try
      {
        _ = jfr.GetGuid(0);
        Assert.Fail("No Exception thrown");
      }
      catch (FormatException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail($"Wrong type of exception  {ex.GetType().Name}");
      }

      try
      {
        _ = jfr.GetInt64(0);
        Assert.Fail("No Exception thrown");
      }
      catch (FormatException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail($"Wrong type of exception  {ex.GetType().Name}");
      }

      try
      {
        _ = jfr.GetInt32(0);
        Assert.Fail("No Exception thrown");
      }
      catch (FormatException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail($"Wrong type of exception  {ex.GetType().Name}");
      }

      try
      {
        _ = jfr.GetInt16(0);
        Assert.Fail("No Exception thrown");
      }
      catch (FormatException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail($"Wrong type of exception  {ex.GetType().Name}");
      }

      try
      {
        _ = jfr.GetDateTime(0);
        Assert.Fail("No Exception thrown");
      }
      catch (FormatException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail($"Wrong type of exception  {ex.GetType().Name}");
      }

      try
      {
        _ = jfr.GetBoolean(0);
        Assert.Fail("No Exception thrown");
      }
      catch (FormatException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail($"Wrong type of exception  {ex.GetType().Name}");
      }

      await jfr.ReadAsync(UnitTestStatic.Token);
      var target = new object[jfr.FieldCount];
      jfr.GetValues(target);
    }

    [TestMethod]
    [Timeout(3000)]
    public async Task ReadJSonEmpAsync()
    {
      var dpd = new Progress<ProgressInfo>();
      using var jfr = new JsonFileReader(UnitTestStatic.GetTestPath("Emp.json"), null, 0, false, string.Empty, false,
        StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, false, false);
      jfr.ReportProgress = dpd;

      await jfr.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual(110, jfr.FieldCount);
      await jfr.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("String", jfr.GetDataTypeName(0));
      Assert.AreEqual("43357099", jfr.GetValue(0));
      Assert.AreEqual("T454898", jfr.GetValue(1));
      Assert.AreEqual(new DateTime(2012, 02, 06), jfr.GetValue(jfr.GetOrdinal("LastHireDate")));
      Assert.IsTrue(jfr.IsDBNull(jfr.GetOrdinal("ASSIGN_2ND_DEPT")));
      Assert.AreEqual((short) 0, jfr.GetInt16(jfr.GetOrdinal("Approvals")));
      Assert.AreEqual(0, jfr.GetInt32(jfr.GetOrdinal("Approvals")));
      Assert.AreEqual(0L, jfr.GetInt64(jfr.GetOrdinal("Approvals")));
      Assert.AreEqual(0f, jfr.GetFloat(jfr.GetOrdinal("Approvals")));
      Assert.AreEqual(0, jfr.GetDecimal(jfr.GetOrdinal("Approvals")));
      Assert.AreEqual(0, jfr.GetDouble(jfr.GetOrdinal("Approvals")));
      Assert.IsTrue(jfr.GetBoolean(2));
      Assert.AreEqual(1.000, jfr.GetValue(jfr.GetOrdinal("FTE")));
      _ = await jfr.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("43357196", jfr.GetValue(0));
      _ = await jfr.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("43357477", jfr.GetValue(0));
      // read each column in each row
      while (await jfr.ReadAsync(UnitTestStatic.Token))
        for (var i = 0; i < jfr.FieldCount; i++)
          _ = jfr.GetValue(i);
      Assert.AreEqual(2782, jfr.RecordNumber);
    }

    [TestMethod]
    [Timeout(2000)]
    public async Task ReadJSon1Async()
    {
      using var jfr = new JsonFileReader(UnitTestStatic.GetTestPath("Jason1.json"));
      await jfr.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual(20, jfr.FieldCount);
      await jfr.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("5048fde7c4aa917cbd4d8e13", jfr.GetValue(0));
      await jfr.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("5048fde7c4aa917cbd4d22333", jfr.GetValue(0));
      Assert.AreEqual(2, jfr.RecordNumber);
    }

    [TestMethod]
    [Timeout(2000)]
    public async Task ReadJSon1TypedAsync()
    {
      using var jfr = new JsonFileReader(UnitTestStatic.GetTestPath("Larger.json"));
      await jfr.OpenAsync(UnitTestStatic.Token);
      await jfr.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual(new Guid("ef21069c-3d93-4e07-878d-00e820727f65"), jfr.GetGuid(0));
      Assert.IsTrue((new DateTime(2020, 04, 03, 18, 45, 29, 573, DateTimeKind.Utc) -
                     jfr.GetDateTime(1).ToUniversalTime()).TotalSeconds < 2);
      Assert.AreEqual((short) 0, jfr.GetInt16(5));
      Assert.AreEqual(0, jfr.GetInt32(5));
      Assert.AreEqual(0L, jfr.GetInt64(5));
      var val = new object[jfr.FieldCount];
      jfr.GetValues(val);
      Assert.IsNull(val[2]);
    }

    [TestMethod]
    [Timeout(2000)]
    public async Task ReadJSon2Async()
    {
      
      using var jfr = new JsonFileReader(UnitTestStatic.GetTestPath("Jason2.json"));
      await jfr.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual(7, jfr.FieldCount);
      await jfr.ReadAsync(UnitTestStatic.Token);
      await jfr.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("Loading defaults C:\\Users\\rnoldner\\AppData\\Roaming\\CSVFileValidator\\Setting.xml",
        jfr.GetValue(6));
      while (await jfr.ReadAsync(UnitTestStatic.Token))
      {
      }

      Assert.AreEqual(29, jfr.RecordNumber);
    }

    [TestMethod]
    [Timeout(2000)]
    public async Task ReadJSon3Async()
    {
      using var jfr = new JsonFileReader(UnitTestStatic.GetTestPath("Jason3.json"));
      await jfr.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual(2, jfr.FieldCount);
      await jfr.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual(28L, jfr.GetValue(0));
      await jfr.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual(56L, jfr.GetValue(0));
      while (await jfr.ReadAsync(UnitTestStatic.Token))
      {
      }

      Assert.AreEqual(5, jfr.RecordNumber);
    }

    [TestMethod]
    [Timeout(2000)]
    public async Task ReadJSon4Async()
    {
      using var jfr = new JsonFileReader(UnitTestStatic.GetTestPath("Jason4.json"));
      await jfr.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual(3, jfr.FieldCount);
    }
  }
}