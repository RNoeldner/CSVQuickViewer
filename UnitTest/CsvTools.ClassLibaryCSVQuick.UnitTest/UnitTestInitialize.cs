using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace CsvTools.Tests
{
  [TestClass]
  public class UnitTestInitialize
  {

    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
      UnitTestStatic.Token = context.CancellationTokenSource.Token;
    }
  }
}