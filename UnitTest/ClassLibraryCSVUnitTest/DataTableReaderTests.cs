using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  using System.Data;
  using System.Globalization;
  using DataTableReader = CsvTools.DataTableReader;

  [TestClass()]
  public class DataTableReaderTests
  {
    private DataTable m_DataTable = RandomDataTable(100);

    private static DataTable RandomDataTable(int recs)
    {
      var dataTable = new DataTable { TableName = "DataTable", Locale = CultureInfo.InvariantCulture })
      
      dataTable.Columns.Add("ID", typeof(int));
      dataTable.Columns.Add("Text", typeof(string));
      dataTable.Columns.Add("ColText1", typeof(string));
      dataTable.Columns.Add("ColText2", typeof(string));
      dataTable.Columns.Add("ColTextDT", typeof(DateTime));
      var random = new Random(new Guid().GetHashCode());
      for (var i = 0; i < recs; i++)
      {
        var row = dataTable.NewRow();
        row["ID"] = i;
        row["Text"] = i.ToString(CultureInfo.CurrentCulture);
        row["ColText1"] = $"Test{i + 1}";
        row["ColText2"] = $"Text {i * 2} !";
        row["ColTextDT"] = new DateTime(random.Next(1900, 2030), random.Next(1, 12), 1).AddDays(random.Next(1, 31));
        dataTable.Rows.Add(row);
      }

      return dataTable;
    }

    [TestMethod()]
    public void DataTableReaderTest()
    {
      using (var pd = new DummyProcessDisplay())
      {
        using (var test = new DataTableReader(m_DataTable, "id", pd))
        {
          Assert.IsTrue(test.IsClosed);
          test.Open();
          Assert.IsFalse(test.IsClosed);
        }
      }
    }

    [TestMethod()]
    public void GetDataTableTest()
    {
      using (var pd = new DummyProcessDisplay())
      {
        try
        {
          using (var test = new DataTableReader(null, "id", pd))
          {
          }
        }
        catch (ArgumentNullException e)
        {
        }
        catch (Exception ex)
        {
          Assert.Fail("Wrong Exception Type: " + ex.GetType());
        }
      }

    }

    [TestMethod()]
    public async Task GetDataTableAsyncTestAsync()
    {
      using (var pd = new DummyProcessDisplay())
      {
        using (var test = new DataTableReader(m_DataTable, "id", pd))
        {
          Assert.IsTrue(test.IsClosed);
          var dt = await test.GetDataTableAsync(200);
          Assert.IsFalse(test.IsClosed);
        }
      }
    }

    [TestMethod()]
    public void GetNameTest()
    {
      using (var pd = new DummyProcessDisplay())
      {
        using (var test = new DataTableReader(m_DataTable, "id", pd))
        {
          test.Open();
          Assert.AreEqual("ID", test.GetName(0));
        }
      }
    }

    [TestMethod()]
    public void GetDataTypeNameTest()
    {
      using (var pd = new DummyProcessDisplay())
      {
        using (var test = new DataTableReader(m_DataTable, "id", pd))
        {
          test.Open();
          Assert.AreEqual("int", test.GetDataTypeName(0));
        }
      }
    }

    [TestMethod()]
    public void GetFieldTypeTest()
    {
      using (var pd = new DummyProcessDisplay())
      {
        using (var test = new DataTableReader(m_DataTable, "id", pd))
        {
          test.Open();
          Assert.AreEqual(typeof(int), test.GetFieldType(0));
        }
      }
    }

  

    [TestMethod()]
    public void GetOrdinalTest()
    {
      using (var pd = new DummyProcessDisplay())
      {
        using (var test = new DataTableReader(m_DataTable, "id", pd))
        {
          test.Open();
          Assert.AreEqual(2, test.GetOrdinal("ColText1"));
        }
      }

    }

    

    [TestMethod()]
    public void ReadTest()
    {
      using (var pd = new DummyProcessDisplay())
      {
        using (var test = new DataTableReader(m_DataTable, "id", pd))
        {
          test.Open();
          Assert.IsTrue(test.Read());
        }
      }
    }

    [TestMethod()]
    public async Task ReadAsyncTest()
    {
      using (var pd = new DummyProcessDisplay())
      {
        using (var test = new DataTableReader(m_DataTable, "id", pd))
        {
          test.Open();
          Assert.IsTrue(await test.ReadAsync());
        }
      }
    }
  }
}