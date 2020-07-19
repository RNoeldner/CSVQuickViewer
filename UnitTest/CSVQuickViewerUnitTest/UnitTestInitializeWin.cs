using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class UnitTestInitializeWin
  {
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
      UnitTestInitializeCsv.AssemblyInitialize(context);
      FunctionalDI.SignalBackground = Application.DoEvents;
    }
  }
}