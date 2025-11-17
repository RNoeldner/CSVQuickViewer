/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
  public class XmlFileReaderTest
  {
    private static readonly TimeZoneChangeDelegate m_TimeZoneAdjust = StandardTimeZoneAdjust.ChangeTimeZone;

    [TestMethod]
    public async Task XmlFileAsync()
    {
      var setting = new CsvFileDummy();
        
      using var xml = new XmlFileReader(UnitTestStatic.GetTestPath("PlantSample.xml"), setting.ColumnCollection, setting.RecordLimit,
        setting.TrimmingOption == TrimmingOptionEnum.All,
        setting.TreatTextAsNull, setting.TreatNBSPAsSpace, m_TimeZoneAdjust, TimeZoneInfo.Local.Id, false, false);
      await xml.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual("COMMON", xml.GetName(0));
      Assert.AreEqual("BOTANICAL", xml.GetName(1));

      await xml.ReadAsync(UnitTestStatic.Token);

      Assert.AreEqual("Bloodroot", xml.GetString(0));
      Assert.AreEqual(4L, xml.GetInt64(2));
      xml.Close();
    }

    [TestMethod]
    public async Task XmlFileSyncAsync()
    {
      var setting = new CsvFileDummy();
        
      using var xml = new XmlFileReader(UnitTestStatic.GetTestPath("PlantSample.xml"), setting.ColumnCollection, setting.RecordLimit,
        setting.TrimmingOption == TrimmingOptionEnum.All,
        setting.TreatTextAsNull, setting.TreatNBSPAsSpace, m_TimeZoneAdjust, TimeZoneInfo.Local.Id, false, false);
      await xml.OpenAsync(UnitTestStatic.Token);

      Assert.AreEqual("COMMON", xml.GetName(0));
      Assert.AreEqual("BOTANICAL", xml.GetName(1));

      xml.Read(UnitTestStatic.Token);

      Assert.AreEqual("Bloodroot", xml.GetString(0));
      Assert.AreEqual(4L, xml.GetInt64(2));
      xml.Close();
    }

    [TestMethod]
    [Timeout(2000)]
    public async Task OpenLogAsStream()
    {
      using var stream = FileSystemUtils.OpenRead(UnitTestStatic.GetTestPath("PlantSample.xml"));
      using var xmlReader = new XmlFileReader(stream, Array.Empty<Column>(), 5000, false, "<nil>", true,
        m_TimeZoneAdjust, TimeZoneInfo.Local.Id, false, true);
      await xmlReader.OpenAsync(UnitTestStatic.Token);
      await xmlReader.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("Bloodroot", xmlReader.GetString(0));
      Assert.AreEqual(4L, xmlReader.GetInt64(2));
    }


    [TestMethod]
    //[Timeout(2000)]
    public async Task ReadTypes()
    {
      using var xmlReader = new XmlFileReader(UnitTestStatic.GetTestPath("PlantSample.xml"), Array.Empty<Column>(),
        5000, false, "<nil>", true,
        m_TimeZoneAdjust, TimeZoneInfo.Local.Id, false, true);
      await xmlReader.OpenAsync(UnitTestStatic.Token);
      await xmlReader.ReadAsync(UnitTestStatic.Token);
      try
      {
        _ = xmlReader.GetFloat(0);
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
        _ = xmlReader.GetGuid(0);
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
        _ = xmlReader.GetInt64(0);
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
        _ = xmlReader.GetInt32(0);
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
        _ = xmlReader.GetInt16(0);
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
        _ = xmlReader.GetDateTime(0);
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
        _ = xmlReader.GetBoolean(0);
        Assert.Fail("No Exception thrown");
      }
      catch (FormatException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail($"Wrong type of exception  {ex.GetType().Name}");
      }
      Assert.AreEqual(4L, xmlReader.GetInt64(2));
      Assert.AreEqual((int) 4, xmlReader.GetInt32(2));
      Assert.AreEqual("$2.44", xmlReader.GetString(4));
      Assert.AreEqual(2.44d, xmlReader.GetDouble(4));
        
      await xmlReader.ReadAsync(UnitTestStatic.Token);
      var target = new object[xmlReader.FieldCount];
      xmlReader.GetValues(target);
    }
  }
}
