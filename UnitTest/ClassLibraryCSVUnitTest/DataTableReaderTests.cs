using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class DataTableReaderTests
  {
    private readonly DataTable m_DataTable = UnitTestStatic.RandomDataTable(100);

    [TestMethod]
    public async Task GetDataTableAsyncTest1Async()
    {
      var processDisplay = new CustomProcessDisplay();
      using var test = new DataTableWrapper(m_DataTable);
      var dt = await test.GetDataTableAsync(TimeSpan.FromSeconds(30), false,
        true, false, false, false, null, UnitTestStatic.Token);
      Assert.AreEqual(m_DataTable, dt);
    }

    [TestMethod]
    public void GetDataTableTest()
    {
      try
      {
        // ReSharper disable once AssignNullToNotNullAttribute
#pragma warning disable CS8625
        using (new DataTableWrapper(null))
#pragma warning restore CS8625
        {
        }
      }
      catch (NullReferenceException)
      { 
      }
      catch (ArgumentNullException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail("Wrong Exception Type: " + ex.GetType());
      }
    }

    [TestMethod]
    public void GetDataTypeNameTest()
    {
      using var test = new DataTableWrapper(m_DataTable);
      // await test.OpenAsync(UnitTestStatic.Token);
      var typeName = test.GetDataTypeName(0);
      Assert.IsTrue(typeName.Equals("int") || typeName.Equals("Int32") || typeName.Equals("Int64") ||
                    typeName.Equals("long"));
    }

    [TestMethod]
    public void GetFieldTypeTest()
    {
      using var test = new DataTableWrapper(m_DataTable);
      //await test.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual(DataTypeEnum.Integer.GetNetType(), test.GetFieldType(0));
    }

    [TestMethod]
    public void GetNameTest()
    {
      using var test = new DataTableWrapper(m_DataTable);
      //await test.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual("ID", test.GetName(0));
    }

    [TestMethod]
    public void GetOrdinalTest()
    {
      using var test = new DataTableWrapper(m_DataTable);
      //await test.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual(2, test.GetOrdinal("ColText1"));
    }

    [TestMethod]
    public async Task ReadAsyncTest()
    {
      using var test = new DataTableWrapper(m_DataTable);
      //await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    }

    [TestMethod]
    public async Task ReadTestAsync()
    {
      using var test = new DataTableWrapper(m_DataTable);
      //await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    }
  }
}