using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;

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