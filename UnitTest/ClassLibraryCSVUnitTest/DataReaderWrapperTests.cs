/*

* Copyright (C) 2014 Raphael N�ldner : http://csvquickviewer.com
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
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  [SuppressMessage("ReSharper", "UseAwaitUsing")]
  public class DataReaderWrapperTests
  {
    private static readonly ICsvFile m_Setting;
    private static readonly TimeZoneChangeDelegate m_TimeZoneAdjust = StandardTimeZoneAdjust.ChangeTimeZone;

    static DataReaderWrapperTests()
    {
      m_Setting = new CsvFileDummy()
      {
        FileName = Path.Combine(UnitTestStatic.GetTestPath("AllFormats.txt")),
        HasFieldHeader = true,
        FieldDelimiterChar = '\t',
      };
      // columns from the file
      m_Setting.ColumnCollection.AddRangeNoClone(
        new Column[]
        {
          new Column("DateTime", new ValueFormat(dataType: DataTypeEnum.DateTime, dateFormat: @"dd/MM/yyyy"), timePart: "Time", timePartFormat: "HH:mm:ss"),
          new Column("Integer", new ValueFormat(DataTypeEnum.Integer)),
          new Column("Numeric", new ValueFormat(DataTypeEnum.Numeric, decimalSeparator: ".")),
          new Column("Double", new ValueFormat(dataType: DataTypeEnum.Double, decimalSeparator: ".")),
          new Column("Boolean", new ValueFormat(DataTypeEnum.Boolean)),
          new Column("GUID", new ValueFormat(DataTypeEnum.Guid)),
          new Column("Time", new ValueFormat(dataType: DataTypeEnum.DateTime, dateFormat: "HH:mm:ss"), ignore: true)
        });
    }

    [TestMethod()]
    public async Task GetColumnIndexFromErrorColumnTest()
    {
      using var reader = GetReader(m_Setting);
      await reader.OpenAsync(UnitTestStatic.Token);
      _ = new DataReaderWrapper(reader);
    }

    [TestMethod()]
    public async Task DepthTest()
    {
      using var reader = GetReader(m_Setting);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      Assert.AreEqual(9, wrapper.Depth);
    }


    [TestMethod()]
    public void DataTableWrapperErrorPassthoughTest()
    {
      using var dataTable = new DataTable { TableName = "DataTable", Locale = CultureInfo.InvariantCulture };
      dataTable.Columns.Add("ID", typeof(int));
      dataTable.Columns.Add("Text", typeof(string));
      dataTable.Columns.Add("#Error", typeof(string));
      for (var i = 0; i < 100; i++)
      {
        var row = dataTable.NewRow();
        row["ID"] = i;
        row["Text"] = i.ToString(CultureInfo.CurrentCulture);
        row["#Error"] = "Error" + i.ToString(CultureInfo.CurrentCulture);
        dataTable.Rows.Add(row);
      }
      using var reader = new DataTableWrapper(dataTable);
      reader.Read();
      Assert.AreEqual("Error0", reader.GetValue(2));
    }

    [TestMethod()]
    public async Task PassthroughErrorTest()
    {
      using var dataTable = new DataTable { TableName = "DataTable", Locale = CultureInfo.InvariantCulture };
      dataTable.Columns.Add("ID", typeof(int));
      dataTable.Columns.Add("Text", typeof(string));
      dataTable.Columns.Add("#Error", typeof(string));
      for (var i = 0; i < 100; i++)
      {
        var row = dataTable.NewRow();
        row["ID"] = i;
        row["Text"] = i.ToString(CultureInfo.CurrentCulture);
        row["#Error"] = "Error" + i.ToString(CultureInfo.CurrentCulture);
        dataTable.Rows.Add(row);
      }
      using var reader = dataTable.CreateDataReader();
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("Error0", wrapper.GetString(wrapper.FieldCount-1));
      await wrapper.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("Error1", wrapper.GetString(wrapper.FieldCount-1));
    }


    [TestMethod()]
    public async Task GetIntegerTest()
    {
      using var reader = GetReader(m_Setting);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual((short) -22477, wrapper.GetInt16(1));
      Assert.AreEqual(-22477, wrapper.GetInt32(1));
      Assert.AreEqual(-22477L, wrapper.GetInt64(1));
    }

    [TestMethod()]
    public async Task GetNumericTest()
    {
      using var reader = GetReader(m_Setting);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);

      Assert.AreEqual((float) -12086.66, wrapper.GetFloat(2), "float");
      Assert.AreEqual(-12086.66, wrapper.GetDouble(2), "double");
      Assert.AreEqual((decimal) -12086.66, wrapper.GetDecimal(2), "decimal");
      Assert.AreEqual((-12086.66).ToString(CultureInfo.CurrentCulture), wrapper.GetString(2), "string");
    }

    [TestMethod()]
    public async Task GetGuidTest()
    {
      using var reader = GetReader(m_Setting);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);

      Assert.AreEqual(new Guid("1BD10E34-7D66-481B-A7E3-AE817B5BEE02"), wrapper.GetGuid(7));
      Assert.AreEqual("1BD10E34-7D66-481B-A7E3-AE817B5BEE02", wrapper.GetString(7).ToUpper());
    }

    private static CsvFileReader GetReader(ICsvFile setting)
    {
      return new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows,
        setting.HasFieldHeader, setting.ColumnCollection, setting.TrimmingOption, setting.FieldDelimiterChar,
        setting.FieldQualifierChar, setting.EscapePrefixChar, setting.RecordLimit, setting.AllowRowCombining,
        setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings,
        setting.DuplicateQualifierToEscape, setting.NewLinePlaceholder, setting.DelimiterPlaceholder,
        setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
        setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue,
        setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter,
        setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull,
        setting.SkipEmptyLines, setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    }

    [TestMethod()]
    public async Task GetDateTimeTest()
    {

      using var reader = GetReader(m_Setting);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      Assert.IsTrue((new DateTime(2014, 05, 23) - wrapper.GetDateTime(0)).TotalSeconds < .5);
    }

    [TestMethod()]
    public async Task GetNameTest()
    {
      using var reader = GetReader(m_Setting);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      // DateTime Integer Double Numeric String
      Assert.AreEqual("DateTime", wrapper.GetName(0));
      Assert.AreEqual("String", wrapper.GetName(4));
      Assert.AreEqual("TZ", wrapper.GetName(8));
    }

    [TestMethod()]
    public async Task GetOrdinalTest()
    {
      using var reader = GetReader(m_Setting);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      // DateTime Integer Double Numeric String
      Assert.AreEqual(0, wrapper.GetOrdinal("DateTime"));
      Assert.AreEqual(4, wrapper.GetOrdinal("String"));
      Assert.AreEqual(8, wrapper.GetOrdinal("TZ"));
    }

    [TestMethod()]
    public async Task GetBooleanTest()
    {
      using var reader = GetReader(m_Setting);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);

      Assert.IsTrue(wrapper.GetBoolean(6), $"{m_Setting.FullPath} Row 1 - Column 6");
      await wrapper.ReadAsync(UnitTestStatic.Token);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      Assert.IsFalse(wrapper.GetBoolean(6), $"{m_Setting.FullPath} Row 4 - Column 6");
    }

    [TestMethod()]
    public async Task GetFieldTypeTestAsync()
    {
      using var reader = GetReader(m_Setting);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      // depending on type this might be int32 or int64
      Assert.IsTrue(nameof(Int64) == wrapper.GetDataTypeName(1) || nameof(Int32) == wrapper.GetDataTypeName(1));
    }

    [TestMethod()]
    public async Task MiscTest()
    {
      using var reader = GetReader(m_Setting);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      Assert.IsTrue(wrapper.HasRows);
      try
      {
      }
      catch (NotImplementedException)
      {
        // ignore
      }

      try
      {
        wrapper.GetValues(new object[10]);
      }
      catch (NotImplementedException)
      {
        // ignore
      }
    }

    [TestMethod()]
    public async Task GetValueTest()
    {
      using var reader = GetReader(m_Setting);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);

      Assert.AreEqual("-22477", wrapper.GetValue(1).ToString());
      Assert.AreEqual(-12086.66, wrapper.GetValue(2));
    }

    [TestMethod()]
    public async Task IsDBNullTest()
    {
      using var reader = GetReader(m_Setting);
      await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);
      await wrapper.ReadAsync(UnitTestStatic.Token);

      // Date is empty but time column has a value
      Assert.IsFalse(wrapper.IsDBNull(0));
      await wrapper.ReadAsync(UnitTestStatic.Token);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      await wrapper.ReadAsync(UnitTestStatic.Token);
      // this row does not have date nor time
      Assert.IsTrue(wrapper.IsDBNull(0));
    }

    [TestMethod()]
    public async Task GetSchemaTableTest()
    {
      using var reader = GetReader(m_Setting); await reader.OpenAsync(UnitTestStatic.Token);
      var wrapper = new DataReaderWrapper(reader);

      Assert.AreEqual(10 - 1, wrapper.GetSchemaTable().Rows.Count);
    }
  }
}