using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.Contracts;

namespace CsvTools.Tests
{
  [TestClass]
  public static class UnitTestInitialize
  {
    public static MimicSQLReader MimicSQLReader { get; } = new MimicSQLReader();

    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {      
      ApplicationSetting.RootFolder = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles";
      ApplicationSetting.SQLDataReader = MimicSQLReader.ReadData;

      // avoid contract violation kill the process
      Contract.ContractFailed += Contract_ContractFailed;
    }  

    private static void Contract_ContractFailed(object sender, ContractFailedEventArgs e)
    {
      e.SetHandled();
    }
  }
}
