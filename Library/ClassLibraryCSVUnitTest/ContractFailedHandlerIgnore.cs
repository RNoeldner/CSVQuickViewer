using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading;

namespace CsvTools.Tests
{
  internal static class UnitTestStatic
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

    public static T ExecuteWithCulture<T>(Func<T> methodFunc, string cultureName)
    {
      T result = default(T);

      var thread = new Thread(() => { result = methodFunc(); })
      {
        CurrentCulture = new CultureInfo(cultureName)
      };
      thread.Start();
      thread.Join();

      return result;
    }
  }
}