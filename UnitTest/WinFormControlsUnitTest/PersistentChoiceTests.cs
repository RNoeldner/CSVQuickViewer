using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass()]
  public class PersistentChoiceTests
  {
    [TestMethod()]
    public void PersistentChoiceTest()
    {
      var test = new PersistentChoice(DialogResult.Yes);
      Assert.AreEqual(DialogResult.Yes, test.DialogResult);
      test.Reset(10);
      Assert.AreEqual(10,test.NumRecs);
      test.ProcessedOne();
      Assert.AreEqual(9, test.NumRecs);
    }

  }
}