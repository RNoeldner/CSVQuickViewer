using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass]
  public class UnitTestInitialize
  {
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
      UnitTestStatic.AssemblyInitialize(context.CancellationTokenSource.Token, s => context.WriteLine(s));
      FunctionalDI.SignalBackground = Application.DoEvents;
    }
  }
}