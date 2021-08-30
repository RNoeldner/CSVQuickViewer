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
      using var pd = new CustomProcessDisplay(UnitTestInitializeCsv.Token);
      using var test = new DataTableWrapper(m_DataTable);
      DataTable dt = await test.GetDataTableAsync(200, false, true, false, false, false, null,
        pd.CancellationToken);
      Assert.AreEqual(m_DataTable, dt);
    }

		[TestMethod]
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

		[TestMethod]
		public void GetDataTypeNameTest()
    {
      using var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token);
      using var test = new DataTableWrapper(m_DataTable);
      // await test.OpenAsync(processDisplay.CancellationToken);
      var typeName = test.GetDataTypeName(0);
      Assert.IsTrue(typeName.Equals("int") || typeName.Equals("Int32") || typeName.Equals("Int64") ||
                    typeName.Equals("long"));
    }

		[TestMethod]
		public void GetFieldTypeTest()
    {
      using var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token);
      using var test = new DataTableWrapper(m_DataTable);
      //await test.OpenAsync(processDisplay.CancellationToken);
      Assert.AreEqual(DataType.Integer.GetNetType(), test.GetFieldType(0));
    }

		[TestMethod]
		public void GetNameTest()
    {
      using var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token);
      using var test = new DataTableWrapper(m_DataTable);
      //await test.OpenAsync(processDisplay.CancellationToken);
      Assert.AreEqual("ID", test.GetName(0));
    }

		[TestMethod]
		public void GetOrdinalTest()
    {
      using var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token);
      using var test = new DataTableWrapper(m_DataTable);
      //await test.OpenAsync(processDisplay.CancellationToken);
      Assert.AreEqual(2, test.GetOrdinal("ColText1"));
    }

		[TestMethod]
		public async Task ReadAsyncTest()
    {
      using var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token);
      using var test = new DataTableWrapper(m_DataTable);
      //await test.OpenAsync(processDisplay.CancellationToken);
      Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
    }

		[TestMethod]
		public async Task ReadTestAsync()
    {
      using var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token);
      using var test = new DataTableWrapper(m_DataTable);
      //await test.OpenAsync(processDisplay.CancellationToken);
      Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
    }
	}
}