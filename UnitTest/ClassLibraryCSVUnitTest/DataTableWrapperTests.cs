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
  private readonly DataTable m_DataTable = UnitTestStaticData.GetDataTable(100);

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
    Assert.AreEqual("string", test.GetName(0));
    Assert.AreEqual("int", test.GetName(1));
    Assert.AreEqual("AllEmpty", test.GetName(7));
    Assert.AreEqual("PartEmpty", test.GetName(8));
  }
  [TestMethod]
  public void GetDataTypeNameTestAsync()
  {
    using var test = new DataTableWrapper(m_DataTable);

    Assert.AreEqual(DataTypeEnum.Integer.GetNetType().Name, test.GetDataTypeName(1));
    Assert.AreEqual(nameof(String), test.GetDataTypeName(0));
    Assert.AreEqual(nameof(DateTime), test.GetDataTypeName(2));
  }

  [TestMethod]
  public void GetFieldTypeTest()
  {
    using var test = new DataTableWrapper(m_DataTable);
    Assert.AreEqual(DataTypeEnum.Integer.GetNetType(), test.GetFieldType(1));
    Assert.AreEqual(typeof(string), test.GetFieldType(0));
  }

  [TestMethod]
  public async Task GetValueTest()
  {
    using var test = new DataTableWrapper(m_DataTable);
    object col0 = null;
    object col1 = null;
    object col2 = null;
    do
    {
      await test.ReadAsync(UnitTestStatic.Token);
      col0 = test.GetValue(0);
      col1 = test.GetValue(1);
      col2 = test.GetValue(2);
    }
    while (col0 is DBNull || col1 is DBNull || col2 is DBNull);

    await test.ReadAsync(UnitTestStatic.Token);
    Assert.IsInstanceOfType(col0, typeof(string));
    Assert.IsInstanceOfType(col1, typeof(long));
    Assert.IsInstanceOfType(col2, typeof(DateTime));
  }

  [TestMethod]
  public async Task GetValuesTest()
  {
    using var test = new DataTableWrapper(m_DataTable);

    object col0 = null;
    object col1 = null;
    object col2 = null;
    do
    {
      await test.ReadAsync(UnitTestStatic.Token);
      col0 = test.GetValue(0);
      col1 = test.GetValue(1);
      col2 = test.GetValue(2);
    }
    while (col0 is DBNull || col1 is DBNull || col2 is DBNull);
    var objects = new object[test.FieldCount];
    test.GetValues(objects);
    Assert.IsInstanceOfType(objects[0], typeof(string));
    Assert.IsInstanceOfType(objects[1], typeof(long));
    Assert.IsInstanceOfType(objects[2], typeof(DateTime));
  }

  [TestMethod]
  public void GetOrdinalTest()
  {
    using var test = new DataTableWrapper(m_DataTable);
    //await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(0, test.GetOrdinal(m_DataTable.Columns[0].ColumnName));
    Assert.AreEqual(7, test.GetOrdinal("AllEmpty"));
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
    var col = dt2.Columns.Add("myBool", typeof(bool));
    foreach (DataRow row in dt2.Rows)
      row[col] = true;

    using var test = new DataTableWrapper(dt2);
    var colnum = test.GetOrdinal("myBool");
    // await test.OpenAsync(UnitTestStatic.Token);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.AreEqual(true, test.GetBoolean(colnum));
  }

  [TestMethod]
  public async Task GetByteTest()
  {
    using var dt2 = m_DataTable.Copy();
    var col = dt2.Columns.Add("myByte", typeof(byte));
    foreach (DataRow row in dt2.Rows)
      row[col] = 15;
    
    using var test = new DataTableWrapper(dt2);
    var colnum = test.GetOrdinal("myByte");
    //await test.OpenAsync(UnitTestStatic.Token);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.AreEqual(15, test.GetByte(colnum));
  }

  [TestMethod]
  public async Task GetGuidTest()
  {
    using var dt2 = m_DataTable.Copy();
    var col = dt2.Columns.Add("myGuid", typeof(Guid));
    foreach (DataRow row in dt2.Rows)
      row[col] = Guid.NewGuid();

    using var test = new DataTableWrapper(dt2);
    var colnum = test.GetOrdinal("myGuid");
    //await test.OpenAsync(UnitTestStatic.Token);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.IsInstanceOfType(test.GetGuid(colnum), typeof(Guid));
  }

  [TestMethod]
  public async Task GetInt16Test()
  {
    using var dt2 = m_DataTable.Copy();
    var col = dt2.Columns.Add("myInt", typeof(Int16));
    foreach (DataRow row in dt2.Rows)
      row[col] = 11;

    using var test = new DataTableWrapper(dt2);
    var colnum = test.GetOrdinal("myInt");
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.AreEqual((short) 11, test.GetInt16(colnum));
  }

  [TestMethod]
  public async Task GetInt32Test()
  {
    int expected = -64747;
    using var dt2 = m_DataTable.Copy();
    var col = dt2.Columns.Add("myInt", typeof(Int32));
    foreach (DataRow row in dt2.Rows)
      row[col] = expected;

    using var test = new DataTableWrapper(dt2);
    var colnum = test.GetOrdinal("myInt");
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.AreEqual(expected, test.GetInt32(colnum));
  }

  [TestMethod]
  public async Task TestLineNumbers()
  {
    using var test = new DataTableWrapper(m_DataTable, true, true, false);
    var colStartLine = test.GetOrdinal(ReaderConstants.cStartLineNumberFieldName);
    var colEndLine = test.GetOrdinal(ReaderConstants.cEndLineNumberFieldName);

    await test.ReadAsync(UnitTestStatic.Token);
    Assert.AreEqual(1, test.GetInt32(colStartLine));
    Assert.AreEqual(1, test.GetInt32(colEndLine));

    Assert.AreEqual(1L, test.GetInt64(colStartLine));
    Assert.AreEqual(1L, test.GetInt64(colEndLine));

    Assert.AreEqual(1L, test.GetValue(colStartLine));
    Assert.AreEqual(1L, test.GetValue(colEndLine));

    await test.ReadAsync(UnitTestStatic.Token);
    Assert.AreEqual(2, test.GetInt32(colStartLine));
    Assert.AreEqual(2, test.GetInt32(colEndLine));

    Assert.AreEqual(2L, test.GetInt64(colStartLine));
    Assert.AreEqual(2L, test.GetInt64(colEndLine));

    Assert.AreEqual(2L, test.GetValue(colStartLine));
    Assert.AreEqual(2L, test.GetValue(colEndLine));
  }

  [TestMethod]
  public async Task TestRecordNumber()
  {
    using var test = new DataTableWrapper(m_DataTable, false, false, true);
    var colRecNum = test.GetOrdinal(ReaderConstants.cRecordNumberFieldName);

    await test.ReadAsync(UnitTestStatic.Token);    
    Assert.AreEqual(1, test.GetInt32(colRecNum));
    Assert.AreEqual(1L, test.GetInt64(colRecNum));
    Assert.AreEqual(1L, test.GetValue(colRecNum));
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.AreEqual(2, test.GetInt32(colRecNum));
    Assert.AreEqual(2L, test.GetInt64(colRecNum));
    Assert.AreEqual(2L, test.GetValue(colRecNum));
  }


  [TestMethod]
  public async Task GetInt64Test()
  {
    using var dt2 = m_DataTable.Copy();
    var col = dt2.Columns.Add("myInt", typeof(long));
    foreach (DataRow row in dt2.Rows)
      row[col] = 1123482452;

    using var test = new DataTableWrapper(dt2);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.AreEqual(1123482452, test.GetInt64(test.GetOrdinal("myInt")));
  }

  [TestMethod]
  public async Task GetFloatTest()
  {
    using var dt2 = m_DataTable.Copy();
    var col = dt2.Columns.Add("myFloat", typeof(float));
    foreach (DataRow row in dt2.Rows)
      row[col] = 11.37;

    using var test = new DataTableWrapper(dt2);
    //await test.OpenAsync(UnitTestStatic.Token);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.AreEqual((float) 11.37, test.GetFloat(test.GetOrdinal("myFloat")));
  }

  [TestMethod]
  public async Task GetDoubleTest()
  {
    using var test = new DataTableWrapper(m_DataTable);
    //await test.OpenAsync(UnitTestStatic.Token);
    object result = null;
    do
    {
      await test.ReadAsync(UnitTestStatic.Token);
      result = test.GetValue(4);
    }
    while (result is DBNull);
    Assert.IsInstanceOfType(result, typeof(double));
    Assert.AreEqual((double) result, test.GetDouble(4));
  }

  [TestMethod]
  public async Task GetStringTest()
  {
    using var test = new DataTableWrapper(m_DataTable);
    //await test.OpenAsync(UnitTestStatic.Token);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.IsInstanceOfType(test.GetString(0), typeof(string));
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
    object result=null;
    do
    {
      await test.ReadAsync(UnitTestStatic.Token);
      result = test.GetValue(2);
    }
    while (result is DBNull);
    Assert.IsInstanceOfType(result, typeof(DateTime));
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