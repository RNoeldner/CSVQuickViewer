using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.Contracts;

namespace CsvTools.Tests
{
  internal class ContractFailedHandlerIgnore
  {
    [TestClass]
    public static class AssemblyContextTest
    {
      [AssemblyInitialize]
      public static void Initialize(TestContext ctx)
      {
        // avoid contract violation kill the process
        Contract.ContractFailed += Contract_ContractFailed;
      }

      private static void Contract_ContractFailed(object sender, ContractFailedEventArgs e)
      {
        e.SetHandled();
        // Assert.Fail("{0}: {1} {2}", e.FailureKind, e.Message, e.Condition);
      }
    }
  }
}