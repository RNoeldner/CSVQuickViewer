using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class SqlServerConnectorTests
  {
    private static SqlServerConnector GetConnection()
    {
      var databaseConnection = new DatabaseConnection
      {
        ConnectionType = ConnectionType.SqlServer,
        IsDefault = true,
        Server = "10.12.0.7",
        User = "bart.finders",
        PasswordEncrypted = "Fvi0PNJK5aMutQo7MFdf".Encrypt()
      };

      var sql = new SqlServerConnector(databaseConnection);
      return sql;
    }

    [TestMethod()]
    public void GetConnectionAsyncTest()
    {
      var sql = GetConnection();
      Assert.IsNotNull(sql.GetConnectionAsync(UnitTestStatic.Token));
    }

    [TestMethod()]
    public async Task CheckDbHasTableAsyncTest()
    {
      var sql = GetConnection();
      Assert.IsFalse(await sql.CheckDbHasTableAsync("cxvswd", UnitTestStatic.Token));
    }

    [TestMethod()]
    public void CheckTableHasFieldAsyncTest()
    {
      Assert.Fail();
    }

    [TestMethod()]
    public void CreateEmptyDatabaseAsyncTest()
    {
      Assert.Fail();
    }

    [TestMethod()]
    public void CreateTableAsyncTest()
    {
      Assert.Fail();
    }

    [TestMethod()]
    public void DropTableAsyncTest()
    {
      Assert.Fail();
    }

  

    [TestMethod()]
    public void GetDataTypesAsyncTest()
    {
      Assert.Fail();
    }

    [TestMethod()]
    public void GetTablesAsyncTest()
    {
      Assert.Fail();
    }

    [TestMethod()]
    public void GetTimeCreatedAsyncTest()
    {
      Assert.Fail();
    }

    [TestMethod()]
    public void ListDatabasesAsyncTest()
    {
      Assert.Fail();
    }

    [TestMethod()]
    public void ProcessColumnLengthAsyncTest()
    {
      Assert.Fail();
    }

    [TestMethod()]
    public void ProcessForNumberAsyncTest()
    {
      Assert.Fail();
    }

    [TestMethod()]
    public void RenameTableAsyncTest()
    {
      Assert.Fail();
    }

    [TestMethod()]
    public void StoreDataReaderAsyncTest()
    {
      Assert.Fail();
    }

    [TestMethod()]
    public void GetMasterConnectionAsyncTest()
    {
      Assert.Fail();
    }
  }
}