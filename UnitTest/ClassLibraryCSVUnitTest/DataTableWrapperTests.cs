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
using System.Data;
using System.Threading.Tasks;
#pragma warning disable CS0618

namespace CsvTools.Tests;

[TestClass]
public class DataTableWrapperTests
{
  private readonly DataTable m_DataTable = UnitTestStaticData.RandomDataTable(100);

  [TestMethod]
  public void DataTableWrapperOpenCloseTest()
  {
    using var test = new DataTableWrapper(m_DataTable);
    test.Read();
    Assert.IsFalse(test.IsClosed);
    test.Close();
    Assert.IsTrue(test.IsClosed);
  }

  [TestMethod]
  public async Task DataTableWrapperProperties()
  {
    using var test = new DataTableWrapper(m_DataTable);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.AreEqual(m_DataTable.Rows.Count, test.RecordsAffected, "RecordsAffected");
    Assert.AreEqual(1, test.Percent, "Percent");
    Assert.AreEqual(1, test.RecordNumber, "RecordNumber");
    Assert.AreEqual(1, test.StartLineNumber, "StartLineNumber");
    Assert.AreEqual(1, test.EndLineNumber, "EndLineNumber");
    Assert.AreEqual(m_DataTable.Columns.Count, test.Depth, "Depth");
    Assert.AreEqual(true, test.HasRows, "HasRows");
  }

  [TestMethod]
  public void GetNameTest()
  {
    using var test = new DataTableWrapper(m_DataTable);
    Assert.AreEqual(m_DataTable.Columns[0].ColumnName, test.GetName(0));
    Assert.AreEqual("Text", test.GetName(1));
    Assert.AreEqual("ColText1", test.GetName(2));
    Assert.AreEqual("ColText2", test.GetName(3));
    Assert.AreEqual("ColTextDT", test.GetName(4));
  }

  [TestMethod]
  public void GetDataTypeNameTestAsync()
  {
    using var test = new DataTableWrapper(m_DataTable);
    Assert.AreEqual(DataTypeEnum.Integer.GetNetType().Name, test.GetDataTypeName(0));
    Assert.AreEqual(nameof(String), test.GetDataTypeName(1));
    Assert.AreEqual(nameof(DateTime), test.GetDataTypeName(4));
    try
    {
      Assert.AreEqual(nameof(DateTime), test.GetDataTypeName(5));
      Assert.Fail("Expected exception");
    }
    catch (Exception)
    {
      //fine
    }
  }

  [TestMethod]
  public void GetFieldTypeTest()
  {
    using var test = new DataTableWrapper(m_DataTable);
    Assert.AreEqual(DataTypeEnum.Integer.GetNetType(), test.GetFieldType(0));
    Assert.AreEqual(typeof(string), test.GetFieldType(1));
  }

  [TestMethod]
  public async Task GetValueTest()
  {
    using var test = new DataTableWrapper(m_DataTable);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.IsInstanceOfType(test.GetValue(0), typeof(int));
    Assert.IsInstanceOfType(test.GetValue(1), typeof(string));
    Assert.IsInstanceOfType(test.GetValue(4), typeof(DateTime));
  }

  [TestMethod]
  public async Task GetValuesTest()
  {
    using var test = new DataTableWrapper(m_DataTable);
    await test.ReadAsync(UnitTestStatic.Token);
    var objects = new object[test.FieldCount];
    test.GetValues(objects);
    Assert.IsInstanceOfType(objects[0], typeof(int));
    Assert.IsInstanceOfType(objects[1], typeof(string));
    Assert.IsInstanceOfType(objects[4], typeof(DateTime));
  }

  [TestMethod]
  public void GetOrdinalTest()
  {
    using var test = new DataTableWrapper(m_DataTable);
    //await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(0, test.GetOrdinal(m_DataTable.Columns[0].ColumnName));
    Assert.AreEqual(4, test.GetOrdinal("ColTextDT"));
    try
    {
      Assert.AreEqual(-1, test.GetOrdinal("Nonsense"));
      Assert.Fail("Expected exception");
    }
    catch (Exception)
    {
      //fine
    }
  }

  [TestMethod]
  public async Task GetBooleanTest()
  {
    using var dt2 = m_DataTable.Copy();
    dt2.Columns.Add("myBool", typeof(bool));
    foreach (DataRow row in dt2.Rows)
      row[5] = true;

    using var test = new DataTableWrapper(dt2);
    // await test.OpenAsync(UnitTestStatic.Token);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.AreEqual(true, test.GetBoolean(5));
  }

  [TestMethod]
  public async Task GetByteTest()
  {
    using var dt2 = m_DataTable.Copy();
    dt2.Columns.Add("myByte", typeof(byte));
    foreach (DataRow row in dt2.Rows)
      row[5] = 15;

    using var test = new DataTableWrapper(dt2);
    //await test.OpenAsync(UnitTestStatic.Token);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.AreEqual(15, test.GetByte(5));
  }

  [TestMethod]
  public async Task GetGuidTest()
  {
    using var dt2 = m_DataTable.Copy();
    dt2.Columns.Add("myGuid", typeof(Guid));
    foreach (DataRow row in dt2.Rows)
      row[5] = Guid.NewGuid();

    using var test = new DataTableWrapper(dt2);
    //await test.OpenAsync(UnitTestStatic.Token);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.IsInstanceOfType(test.GetGuid(5), typeof(Guid));
  }

  [TestMethod]
  public async Task GetInt16Test()
  {
    using var dt2 = m_DataTable.Copy();
    dt2.Columns.Add("myInt", typeof(Int16));
    foreach (DataRow row in dt2.Rows)
      row[5] = 11;

    using var test = new DataTableWrapper(dt2);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.AreEqual((short) 11, test.GetInt16(5));
  }

  [TestMethod]
  public async Task GetInt32Test()
  {
    using var test = new DataTableWrapper(m_DataTable);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.IsInstanceOfType(test.GetInt32(0), typeof(Int32));
  }

  [TestMethod]
  public async Task GetInt64Test()
  {
    using var dt2 = m_DataTable.Copy();
    dt2.Columns.Add("myInt", typeof(long));
    foreach (DataRow row in dt2.Rows)
      row[5] = 1123482452;

    using var test = new DataTableWrapper(dt2);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.AreEqual(1123482452, test.GetInt64(5));
  }

  [TestMethod]
  public async Task GetFloatTest()
  {
    using var dt2 = m_DataTable.Copy();
    dt2.Columns.Add("myInt", typeof(float));
    foreach (DataRow row in dt2.Rows)
      row[5] = 11.37;

    using var test = new DataTableWrapper(dt2);
    //await test.OpenAsync(UnitTestStatic.Token);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.AreEqual((float) 11.37, test.GetFloat(5));
  }

  [TestMethod]
  public async Task GetDoubleTest()
  {
    using var dt2 = m_DataTable.Copy();
    dt2.Columns.Add("myInt", typeof(double));
    foreach (DataRow row in dt2.Rows)
      row[5] = 11.37334;

    using var test = new DataTableWrapper(dt2);
    //await test.OpenAsync(UnitTestStatic.Token);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.AreEqual(11.37334, test.GetDouble(5));
  }

  [TestMethod]
  public async Task GetStringTest()
  {
    using var test = new DataTableWrapper(m_DataTable);
    //await test.OpenAsync(UnitTestStatic.Token);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.IsInstanceOfType(test.GetString(1), typeof(string));
  }

  [TestMethod]
  public async Task GetDecimalTest()
  {
    using var dt2 = m_DataTable.Copy();
    dt2.Columns.Add("myInt", typeof(decimal));
    foreach (DataRow row in dt2.Rows)
      row[5] = 223311.37334;

    using var test = new DataTableWrapper(dt2);
    //await test.OpenAsync(UnitTestStatic.Token);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.AreEqual((decimal) 223311.37334, test.GetDecimal(5));
  }

  [TestMethod]
  public async Task GetDateTimeTest()
  {
    using var test = new DataTableWrapper(m_DataTable);
    //await test.OpenAsync(UnitTestStatic.Token);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.IsInstanceOfType(test.GetDateTime(4), typeof(DateTime));
  }

  [TestMethod]
  public void NextResultTest()
  {
    using var test = new DataTableWrapper(m_DataTable);
    Assert.IsFalse(test.NextResult());
  }

  [TestMethod]
  public void GetColumnTest()
  {
    using var test = new DataTableWrapper(m_DataTable);
    //await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(m_DataTable.Columns[0].ColumnName, test.GetColumn(0).Name);
  }
}