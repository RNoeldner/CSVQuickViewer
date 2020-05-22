using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  using System.Data;
  using System.Globalization;
  using DataTableReader = CsvTools.DataTableReader;

  [TestClass()]
  public class DataTableReaderTests
  {
    private readonly DataTable m_DataTable = RandomDataTable(100);

    [TestMethod()]
    public async Task DataTableReaderTestAsync()
    {
      using (var pd = new DummyProcessDisplay())
      {
        using (var test = new DataTableReader(m_DataTable, "id", pd))
        {
          Assert.IsTrue(test.IsClosed);
          await test.OpenAsync();
          Assert.IsFalse(test.IsClosed);
        }
      }
    }

    [TestMethod()]
    public async Task GetDataTableAsyncTest1Async()
    {
      using (var pd = new DummyProcessDisplay())
      {
        using (var test = new DataTableReader(m_DataTable, "id", pd))
        {
          Assert.IsTrue(test.IsClosed);
          var dt = await test.GetDataTableAsync(200, false, false, true, pd.CancellationToken);
          Assert.AreEqual(m_DataTable, dt);
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
        catch (ArgumentNullException)
        {
        }
        catch (Exception ex)
        {
          Assert.Fail("Wrong Exception Type: " + ex.GetType());
        }
      }
    }

    [TestMethod()]
    public async Task GetDataTypeNameTestAsync()
    {
      using (var pd = new DummyProcessDisplay())
      {
        using (var test = new DataTableReader(m_DataTable, "id", pd))
        {
          await test.OpenAsync();
          var typeName = test.GetDataTypeName(0);
          Assert.IsTrue(typeName.Equals("int") || typeName.Equals("Int32") || typeName.Equals("Int64") || typeName.Equals("long"));
        }
      }
    }

    [TestMethod()]
    public async Task GetFieldTypeTestAsync()
    {
      using (var pd = new DummyProcessDisplay())
      {
        using (var test = new DataTableReader(m_DataTable, "id", pd))
        {
          await test.OpenAsync();
          Assert.AreEqual(typeof(int), test.GetFieldType(0));
        }
      }
    }

    [TestMethod()]
    public async Task GetNameTestAsync()
    {
      using (var pd = new DummyProcessDisplay())
      {
        using (var test = new DataTableReader(m_DataTable, "id", pd))
        {
          await test.OpenAsync();
          Assert.AreEqual("ID", test.GetName(0));
        }
      }
    }

    [TestMethod()]
    public async Task GetOrdinalTestAsync()
    {
      using (var pd = new DummyProcessDisplay())
      {
        using (var test = new DataTableReader(m_DataTable, "id", pd))
        {
          await test.OpenAsync();
          Assert.AreEqual(2, test.GetOrdinal("ColText1"));
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
          await test.OpenAsync();
          Assert.IsTrue(await test.ReadAsync());
        }
      }
    }

    [TestMethod()]
    public async Task ReadTestAsync()
    {
      using (var pd = new DummyProcessDisplay())
      {
        using (var test = new DataTableReader(m_DataTable, "id", pd))
        {
          await test.OpenAsync();
          Assert.IsTrue(await test.ReadAsync());
        }
      }
    }

    private static DataTable RandomDataTable(int recs)
    {
      var dataTable = new DataTable { TableName = "DataTable", Locale = CultureInfo.InvariantCulture };

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
  }
}