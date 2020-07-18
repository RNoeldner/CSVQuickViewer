using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  using System.Data;
  using System.Globalization;

  [TestClass()]
  public class DataTableReaderTests
  {
    private readonly DataTable m_DataTable = RandomDataTable(100);

    [TestMethod()]
    public async Task DataTableReaderTestAsync()
    {
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        using (var test = new DataTableWrapper(m_DataTable))
        {
          Assert.IsTrue(test.IsClosed);
          await test.OpenAsync(processDisplay.CancellationToken);
          Assert.IsFalse(test.IsClosed);
        }
      }
    }

    [TestMethod()]
    public async Task GetDataTableAsyncTest1Async()
    {
      using (var pd = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        using (var test = new DataTableWrapper(m_DataTable))
        {
          Assert.IsTrue(test.IsClosed);
          var dt = await test.GetDataTableAsync(200, false, true, false, false, false, null, null, pd.CancellationToken);
          Assert.AreEqual(m_DataTable, dt);
        }
      }
    }

    [TestMethod()]
    public void GetDataTableTest()
    {

      try
      {
        // ReSharper disable once AssignNullToNotNullAttribute
        using (new DataTableWrapper(null))
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

    [TestMethod()]
    public async Task GetDataTypeNameTestAsync()
    {
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        using (var test = new DataTableWrapper(m_DataTable))
        {
          await test.OpenAsync(processDisplay.CancellationToken);
          var typeName = test.GetDataTypeName(0);
          Assert.IsTrue(typeName.Equals("int") || typeName.Equals("Int32") || typeName.Equals("Int64") || typeName.Equals("long"));
        }
      }
    }

    [TestMethod()]
    public async Task GetFieldTypeTestAsync()
    {
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        using (var test = new DataTableWrapper(m_DataTable))
        {
          await test.OpenAsync(processDisplay.CancellationToken);
          Assert.AreEqual(typeof(int), test.GetFieldType(0));
        }
      }
    }

    [TestMethod()]
    public async Task GetNameTestAsync()
    {
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        using (var test = new DataTableWrapper(m_DataTable))
        {
          await test.OpenAsync(processDisplay.CancellationToken);
          Assert.AreEqual("ID", test.GetName(0));
        }
      }
    }

    [TestMethod()]
    public async Task GetOrdinalTestAsync()
    {
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        using (var test = new DataTableWrapper(m_DataTable))
        {
          await test.OpenAsync(processDisplay.CancellationToken);
          Assert.AreEqual(2, test.GetOrdinal("ColText1"));
        }
      }
    }

    [TestMethod()]
    public async Task ReadAsyncTest()
    {
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        using (var test = new DataTableWrapper(m_DataTable))
        {
          await test.OpenAsync(processDisplay.CancellationToken);
          Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
        }
      }
    }

    [TestMethod()]
    public async Task ReadTestAsync()
    {
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        using (var test = new DataTableWrapper(m_DataTable))
        {
          await test.OpenAsync(processDisplay.CancellationToken);
          Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
        }
      }
    }

    private static DataTable RandomDataTable(int records)
    {
      var dataTable = new DataTable { TableName = "DataTable", Locale = CultureInfo.InvariantCulture };

      dataTable.Columns.Add("ID", typeof(int));
      dataTable.Columns.Add("Text", typeof(string));
      dataTable.Columns.Add("ColText1", typeof(string));
      dataTable.Columns.Add("ColText2", typeof(string));
      dataTable.Columns.Add("ColTextDT", typeof(DateTime));
      var random = new Random(new Guid().GetHashCode());
      for (var i = 0; i < records; i++)
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