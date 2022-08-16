using Microsoft.VisualStudio.TestTools.UnitTesting;
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
      
    }

    [TestMethod()]
    public void CreateEmptyDatabaseAsyncTest()
    {
      
    }

    [TestMethod()]
    public void CreateTableAsyncTest()
    {
      
    }

    [TestMethod()]
    public void DropTableAsyncTest()
    {
      
    }

  

    [TestMethod()]
    public void GetDataTypesAsyncTest()
    {
      
    }

    [TestMethod()]
    public void GetTablesAsyncTest()
    {
      
    }

    [TestMethod()]
    public void GetTimeCreatedAsyncTest()
    {
      
    }

    [TestMethod()]
    public void ListDatabasesAsyncTest()
    {
      
    }

    [TestMethod()]
    public void ProcessColumnLengthAsyncTest()
    {
      
    }

    [TestMethod()]
    public void ProcessForNumberAsyncTest()
    {
      
    }

    [TestMethod()]
    public void RenameTableAsyncTest()
    {
      
    }

    [TestMethod()]
    public void StoreDataReaderAsyncTest()
    {
      
    }

    [TestMethod()]
    public void GetMasterConnectionAsyncTest()
    {
      
    }
  }
}