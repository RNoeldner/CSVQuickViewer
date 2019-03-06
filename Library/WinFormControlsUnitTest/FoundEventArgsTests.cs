using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass()]
  public class FoundEventArgsTests
  {
    [TestMethod()]
    public void FoundEventArgs()
    {
      var cell = new DataGridViewTextBoxCell();
      var test = new FoundEventArgs(123, cell);
      Assert.AreEqual(123, test.Index);
      Assert.AreEqual(cell, test.Cell);
    }

    [TestMethod()]
    public void SearchEventArgs()
    {
      var test = new SearchEventArgs("test", 2);
      Assert.AreEqual(2, test.Result);
      Assert.AreEqual("test", test.SearchText);

      var test2 = new SearchEventArgs("test2");
      Assert.AreEqual(1, test2.Result);
      Assert.AreEqual("test2", test2.SearchText);
    }
  }
}