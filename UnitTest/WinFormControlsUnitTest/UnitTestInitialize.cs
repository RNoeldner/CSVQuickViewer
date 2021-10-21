using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public sealed class UnitTestInitialize
  {
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
      UnitTestStatic.AssemblyInitialize(context.CancellationTokenSource.Token, s => context.WriteLine(s));
    }
  }
}